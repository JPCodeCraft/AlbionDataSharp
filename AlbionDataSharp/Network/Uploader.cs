using AlbionData.Models;
using AlbionDataSharp.Config;
using AlbionDataSharp.Network.Events;
using AlbionDataSharp.Network.Pow;
using AlbionDataSharp.State;
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
        private readonly HttpClient httpClient = new HttpClient();
        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
        private readonly Dictionary<Config.ServerInfo, IConnection> natsConnections = new Dictionary<Config.ServerInfo, IConnection>();
        private PlayerState playerState;
        private ConfigurationService configurationService;
        private PowSolver powSolver;

        private int maxServerNameLength;

        public event EventHandler<MarketUploadEventArgs> OnMarketUpload;
        public event EventHandler<GoldPriceUploadEventArgs> OnGoldPriceUpload;
        public event EventHandler<MarketHistoriesUploadEventArgs> OnMarketHistoryUpload;
        public Uploader(PlayerState playerState, ConfigurationService configurationService, PowSolver powSolver)
        {
            this.playerState = playerState;
            this.configurationService = configurationService;
            this.powSolver = powSolver;

            maxServerNameLength = GetMaxServerNameLength();
            AppDomain.CurrentDomain.ProcessExit += (s, e) => OnShutDown();
            foreach (var server in configurationService.NetworkSettings.UploadServers)
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
            foreach (var server in configurationService.NetworkSettings.UploadServers.Where(x => x.AlbionServer == playerState.AlbionServer))
            {
                if (await UploadData(data, server, configurationService.NetworkSettings.MarketOrdersIngestSubject))
                {
                    LogOfferRequestUpload(offers, requests, server);
                    OnMarketUpload?.Invoke(this, new MarketUploadEventArgs(marketUpload, server));
                }
            }
        }
        public async Task Upload(GoldPriceUpload goldHistoryUpload)
        {
            var amount = goldHistoryUpload.Prices.Length;
            var data = SerializeData(goldHistoryUpload);
            foreach (var server in configurationService.NetworkSettings.UploadServers.Where(x => x.AlbionServer == playerState.AlbionServer))
            {
                if (await UploadData(data, server, configurationService.NetworkSettings.GoldDataIngestSubject))
                {
                    LogGoldHistoryUpload(amount, server);
                    OnGoldPriceUpload?.Invoke(this, new GoldPriceUploadEventArgs(goldHistoryUpload, server));
                }
            }
        }


        public async Task Upload(MarketHistoriesUpload marketHistoriesUpload)
        {
            var count = marketHistoriesUpload.MarketHistories.Count;
            var timescale = marketHistoriesUpload.Timescale;
            var data = SerializeData(marketHistoriesUpload);
            foreach (var server in configurationService.NetworkSettings.UploadServers.Where(x => x.AlbionServer == playerState.AlbionServer))
            {
                if (await UploadData(data, server, configurationService.NetworkSettings.MarketHistoriesIngestSubject))
                {
                    LogHistoryUpload(count, timescale, server);
                    OnMarketHistoryUpload?.Invoke(this, new MarketHistoriesUploadEventArgs(marketHistoriesUpload, server));
                }
            }
        }

        private byte[] SerializeData(object upload)
        {
            return JsonSerializer.SerializeToUtf8Bytes(upload, new JsonSerializerOptions { IncludeFields = true });
        }
        private async Task<bool> UploadData(byte[] data, Config.ServerInfo server, string topic)
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
                        var powRequest = await powSolver.GetPowRequest(server, httpClient);
                        if (powRequest is not null)
                        {
                            var solution = await powSolver.SolvePow(powRequest);
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
        private void LogOfferRequestUpload(int offers, int requests, Config.ServerInfo server)
        {
            string paddedServerName = server.Name.PadRight(maxServerNameLength);
            string serverMarkup = $"[{server.Color}]{paddedServerName}[/]";
            string offersMarkup = $"[green]{offers}[/]";
            string requestsMarkup = $"[yellow]{requests}[/]";

            if (offers > 0 && requests == 0) Log.Information($"{serverMarkup} Published {offersMarkup} offers.");
            else if (offers == 0 && requests > 0) Log.Information($"{serverMarkup} Published {requestsMarkup} requests.");
            else if (offers == 0 && requests == 0) Log.Debug($"{serverMarkup} Published nothing.");
            else Log.Information($"{serverMarkup} Published {offersMarkup} offers and {requestsMarkup} requests.");
        }
        private void LogHistoryUpload(int count, Timescale timescale, Config.ServerInfo server)
        {
            string paddedServerName = server.Name.PadRight(maxServerNameLength);
            string serverMarkup = $"[{server.Color}]{paddedServerName}[/]";
            string countMarkup = $"[greenyellow]{count}[/]";
            string timescaleMarkup = $"[darkorange3_1]{timescale}[/]";

            Log.Information($"{serverMarkup} Published {countMarkup} histories in timescale {timescaleMarkup}.");
        }
        private void LogGoldHistoryUpload(int count, Config.ServerInfo server)
        {
            string paddedServerName = server.Name.PadRight(maxServerNameLength);
            string serverMarkup = $"[{server.Color}]{paddedServerName}[/]";
            string countMarkup = $"[gold3]{count}[/]";

            Log.Information($"{serverMarkup} Published {countMarkup} gold histories.");
        }
        private int GetMaxServerNameLength()
        {
            return configurationService.NetworkSettings.UploadServers.Max(s => s.Name.Length);
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
