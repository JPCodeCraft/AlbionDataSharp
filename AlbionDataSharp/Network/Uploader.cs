using AlbionData.Models;
using AlbionDataSharp.Config;
using AlbionDataSharp.Network.Pow;
using AlbionDataSharp.State;
using AlbionDataSharp.UI;
using NATS.Client;
using Serilog;
using System.Net;
using System.Text;
using System.Text.Json;

namespace AlbionDataSharp.Network
{
    public class Uploader
    {
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

        protected async Task<bool> UploadData(byte[] data, ServerInfo server, string topic)
        {
            if (server.UploadType == UploadType.Nats)
            {
                return UploadToNats(data, topic, server);
            }
            else if (server.UploadType == UploadType.PoW)
            {
                PowSolver solver = new PowSolver();
                var powRequest = await solver.GetPowRequest(server);
                if (powRequest is not null)
                {
                    var solution = await solver.SolvePow(powRequest);
                    if (!string.IsNullOrEmpty(solution))
                    {
                        await UploadWithPow(powRequest, solution, data, topic, server);
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
            else
            {
                Log.Error("Unknown upload type {0}", server.UploadType);
                return false;
            }
        }

        private async Task UploadWithPow(PowRequest pow, string solution, byte[] data, string topic, ServerInfo server)
        {
            string fullURL = server.Url + "/pow/" + topic;

            var dataToSend = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("key", pow.Key),
                new KeyValuePair<string, string>("solution", solution),
                new KeyValuePair<string, string>("serverid", server.AlbionServer.ToString()),
                new KeyValuePair<string, string>("natsmsg", Encoding.UTF8.GetString(data)),
            });

            HttpResponseMessage response;
            using (HttpClient client = new())
            {
                var request = new HttpRequestMessage(HttpMethod.Post, fullURL);
                request.Headers.Add("User-Agent", "AlbionDataSharp");
                request.Content = dataToSend;

                response = await client.SendAsync(request);
            }

            if (response.StatusCode != HttpStatusCode.OK)
            {
                Log.Error("HTTP Error while proving pow. Returned: {0} ({1})", response.StatusCode, await response.Content.ReadAsStringAsync());
                return;
            }

            Log.Debug("Successfully sent ingest request to {0}", fullURL);
            return;
        }

        private bool UploadToNats(byte[] data, string topic, ServerInfo server)
        {
            Options opts = ConnectionFactory.GetDefaultOptions();
            //hacks so nats won't log it's default event to console
            opts.DisconnectedEventHandler = (sender, args) => { };
            opts.ClosedEventHandler = (sender, args) => { };
            opts.Url = server.Url;

            try
            {
                using (IConnection c = new ConnectionFactory().CreateConnection(opts))
                {
                    c.Publish(topic, data);
                    c.Flush(10000);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.ToString());
                return false;
            }
        }

        protected void LogOfferRequestUpload(int offers, int requests, ServerInfo server)
        {
            if (offers > 0 && requests == 0) Log.Information("Published {amount} offers to {server}.", offers, server.Name);
            else if (offers == 0 && requests > 0) Log.Information("Published {amount} requests to {server}.", requests, server.Name);
            else if (offers == 0 && requests == 0) Log.Debug("Published nothing to {server}.", server.Name);
            else Log.Information("Published {amount} offers and {amount} requests to {server}.", offers, requests, server.Name);
        }

        protected void LogHistoryUpload(int count, Timescale timescale, ServerInfo server)
        {
            Log.Information("Published {amount} histories in timescale {timescale} to {server}.", count, timescale.ToString(), server.Name);
        }
    }
}
