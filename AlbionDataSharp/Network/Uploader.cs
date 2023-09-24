using AlbionData.Models;
using AlbionDataSharp.Config;
using AlbionDataSharp.Network.Pow;
using AlbionDataSharp.State;
using AlbionDataSharp.UI;
using NATS.Client;
using Serilog;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace AlbionDataSharp.Network
{
    public class Uploader
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private static readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
        private static readonly Dictionary<Config.ServerInfo, IConnection> natsConnections = new Dictionary<Config.ServerInfo, IConnection>();
        public Uploader()
        {

            AppDomain.CurrentDomain.ProcessExit += (s, e) => OnShutDown();
            foreach (var server in ConfigurationHelper.networkSettings.UploadServers)
            {
                if (server.UploadType == UploadType.Nats)
                {
                    var options = ConnectionFactory.GetDefaultOptions();
                    options.Url = server.Url;
                    //hacks so nats won't log it's default event to console
                    options.DisconnectedEventHandler = (sender, args) =>
                    {
                        Log.Information("Nats connection of {server} disconnected with error {error}", server.Name, args.Error);
                    };
                    options.ClosedEventHandler = (sender, args) =>
                    {
                        Log.Information("Nats connection of {server} closed with error {error}", server.Name, args.Error);
                    };
                    options.ReconnectedEventHandler = (sender, args) =>
                    {
                        Log.Information("Nats connection of {server} reconnected with error {error}", server.Name, args.Error);
                    };
                    natsConnections[server] = new ConnectionFactory().CreateConnection(options);
                }
            }
        }

        public async Task Upload(MarketUpload marketUpload)
        {
            var offers = marketUpload.Orders.Where(x => x.AuctionType == "offer").Count();
            var requests = marketUpload.Orders.Where(x => x.AuctionType == "request").Count();
            var data = SerializeData(marketUpload);
            foreach (var server in ConfigurationHelper.networkSettings.UploadServers.Where(x => x.AlbionServer == PlayerStatus.AlbionServer))
            {
                if (await UploadData(data, server, ConfigurationHelper.networkSettings.MarketOrdersIngestSubject))
                {
                    LogOfferRequestUpload(offers, requests, server);
                    if (offers > 0) ConsoleManager.IncrementOffersSent(server.Name, offers);
                    if (requests > 0) ConsoleManager.IncrementRequestsSent(server.Name, requests);
                }
            }
        }

        public async Task Upload(MarketHistoriesUpload marketHistoriesUpload)
        {
            var count = marketHistoriesUpload.MarketHistories.Count;
            var timescale = marketHistoriesUpload.Timescale;
            var data = SerializeData(marketHistoriesUpload);
            foreach (var server in ConfigurationHelper.networkSettings.UploadServers.Where(x => x.AlbionServer == PlayerStatus.AlbionServer))
            {
                if (await UploadData(data, server, ConfigurationHelper.networkSettings.MarketHistoriesIngestSubject))
                {
                    LogHistoryUpload(count, timescale, server);
                    if (count > 0) ConsoleManager.IncrementHistoriesSent(server.Name, count, timescale);
                }
            }
        }

        private byte[] SerializeData(object upload)
        {
            return JsonSerializer.SerializeToUtf8Bytes(upload, new JsonSerializerOptions { IncludeFields = true });
        }

        protected async Task<bool> UploadData(byte[] data, Config.ServerInfo server, string topic)
        {
            try
            {
                if (server.UploadType == UploadType.Nats)
                {
                    if (natsConnections.TryGetValue(server, out var natsConnection))
                    {
                        return UploadToNats(data, topic, server, natsConnection);
                    }
                    else
                    {
                        Log.Error("Nats connection for {0} was not found.", server.Name);
                        return false;
                    }
                }
                else if (server.UploadType == UploadType.PoW)
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        PowSolver solver = new PowSolver();
                        var powRequest = await solver.GetPowRequest(server, httpClient);
                        if (powRequest is not null)
                        {
                            var solution = await solver.SolvePow(powRequest);
                            if (!string.IsNullOrEmpty(solution))
                            {
                                await UploadWithPow(powRequest, solution, data, topic, server, httpClient);
                                return true;
                            }
                            else
                            {
                                Log.Error("PoW solution is null or empty.");
                                return false;
                            }
                        }
                        else
                        {
                            Log.Error("PoW request is null.");
                            return false;
                        }
                    }
                    finally
                    {
                        semaphore.Release();
                    }

                }
                else
                {
                    Log.Error("Unknown upload type {0}.", server.UploadType);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Exception while uploading data to {0}.", server.Name);
                return false;
            }

        }

        private async Task UploadWithPow(PowRequest pow, string solution, byte[] data, string topic, Config.ServerInfo server, HttpClient client)
        {
            string fullURL = server.Url + "/pow/" + topic;

            var dataToSend = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("key", pow.Key),
                new KeyValuePair<string, string>("solution", solution),
                new KeyValuePair<string, string>("serverid", server.AlbionServer.ToString()),
                new KeyValuePair<string, string>("natsmsg", Encoding.UTF8.GetString(data)),
            });

            var request = new HttpRequestMessage(HttpMethod.Post, fullURL);
            request.Headers.Add("User-Agent", "AlbionDataSharp");
            request.Content = dataToSend;

            HttpResponseMessage response = await client.SendAsync(request);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                Log.Error("HTTP Error while proving pow. Returned: {0} ({1})", response.StatusCode, await response.Content.ReadAsStringAsync());
                return;
            }

            Log.Debug("Successfully sent ingest request to {0}", fullURL);
            return;
        }

        private bool UploadToNats(byte[] data, string topic, Config.ServerInfo server, IConnection natsConnection)
        {
            try
            {
                natsConnection.Publish(topic, data);
                return true;
            }
            catch (SocketException ex)
            {
                Log.Error(ex, "SocketException: {Message}", ex.Message);
            }
            catch (IOException ex)
            {
                Log.Error(ex, "IOException: {Message}", ex.Message);
            }
            catch (ObjectDisposedException ex)
            {
                Log.Error(ex, "ObjectDisposedException: {Message}", ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.ToString());
            }
            return false;
        }

        protected void LogOfferRequestUpload(int offers, int requests, Config.ServerInfo server)
        {
            if (offers > 0 && requests == 0) Log.Information("Published {amount} offers to {server}.", offers, server.Name);
            else if (offers == 0 && requests > 0) Log.Information("Published {amount} requests to {server}.", requests, server.Name);
            else if (offers == 0 && requests == 0) Log.Debug("Published nothing to {server}.", server.Name);
            else Log.Information("Published {amount} offers and {amount} requests to {server}.", offers, requests, server.Name);
        }

        protected void LogHistoryUpload(int count, Timescale timescale, Config.ServerInfo server)
        {
            Log.Information("Published {amount} histories in timescale {timescale} to {server}.", count, timescale.ToString(), server.Name);
        }

        private void OnShutDown()
        {
            // Close and flush NATS connections here
            foreach (var connection in natsConnections.Values)
            {
                connection.Drain();
                connection.Close();
            }
        }
    }
}
