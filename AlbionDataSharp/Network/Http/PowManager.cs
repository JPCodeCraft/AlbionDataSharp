using AlbionData.Models;
using AlbionDataSharp.Config;
using AlbionDataSharp.State;
using AlbionDataSharp.UI;
using Serilog;
using System.Diagnostics;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace AlbionDataSharp.Network.Http
{
    public class PowManager
    {
        private static readonly RandomNumberGenerator rng = RandomNumberGenerator.Create();
        public async Task Upload(MarketUpload marketUpload)
        {
            var offers = marketUpload.Orders.Where(x => x.AuctionType == "offer").Count();
            var requests = marketUpload.Orders.Where(x => x.AuctionType == "request").Count();

            try
            {
                var data = JsonSerializer.SerializeToUtf8Bytes(marketUpload, new JsonSerializerOptions { IncludeFields = true });
                var powRequest = await GetPowRequest();
                var powSolution = await SolvePow(powRequest);
                var uploadReturn = await UploadWithPow(powRequest, powSolution, data, ConfigurationHelper.networkSettings.MarketOrdersIngestSubject);
                if (uploadReturn.success)
                {
                    ConsoleManager.UpdateRequestsSent(uploadReturn.serverName, requests);
                    ConsoleManager.UpdateOffersSent(uploadReturn.serverName, offers);
                    if (offers > 0 && requests == 0) Log.Information("Published {amount} offers to {server}.", offers, uploadReturn.serverName);
                    else if (offers == 0 && requests > 0) Log.Information("Published {amount} requests to {server}.", requests, uploadReturn.serverName);
                    else if (offers == 0 && requests == 0) Log.Debug("Published nothing to {server}.", uploadReturn.serverName);
                    else Log.Information("Published {amount} offers and {amount} requests to {server}.", offers, requests, uploadReturn.serverName);
                }

            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.ToString());
            }

        }
        public async Task Upload(MarketHistoriesUpload marketHistoriesUpload)
        {
            try
            {
                var data = JsonSerializer.SerializeToUtf8Bytes(marketHistoriesUpload, new JsonSerializerOptions { IncludeFields = true });
                var powRequest = await GetPowRequest();
                var powSolution = await SolvePow(powRequest);
                var uploadReturn = await UploadWithPow(powRequest, powSolution, data, ConfigurationHelper.networkSettings.MarketHistoriesIngestSubject);
                if (uploadReturn.success)
                {
                    ConsoleManager.UpdateHistoriesSent(uploadReturn.serverName, marketHistoriesUpload.MarketHistories.Count, marketHistoriesUpload.Timescale);

                    Log.Information("Published {Amount} histories for {ItemID} quality {Quality} in location {Location} timescale {Timescale} to {server}.",
                        marketHistoriesUpload.MarketHistories.Count, marketHistoriesUpload.AlbionId, marketHistoriesUpload.QualityLevel,
                        marketHistoriesUpload.LocationId, marketHistoriesUpload.Timescale, uploadReturn.serverName);
                }

            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.ToString());
            }

        }
        private async Task<PowRequest> GetPowRequest()
        {
            Log.Debug("Getting PoW");

            string fullURL = string.Empty;

            switch (PlayerStatus.Server)
            {
                case Server.Unknown:
                    Log.Warning("Server has not been set. Can't GetPow. Please change maps.");
                    return await Task.FromResult<PowRequest>(new PowRequest());
                case Server.East:
                    fullURL = ConfigurationHelper.networkSettings.AlbionDataServers.East.Url + "/pow";
                    break;
                case Server.West:
                    fullURL = ConfigurationHelper.networkSettings.AlbionDataServers.West.Url + "/pow";
                    break;
            };

            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, fullURL);
            request.Headers.Add("User-Agent", "AlbionDataSharp");
            var response = await client.SendAsync(request);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                Log.Error("Got bad response code when getting PoW: {0}", response.StatusCode);
                return await Task.FromResult<PowRequest>(new PowRequest());
            }

            var content = await response.Content.ReadAsStringAsync();
            var pow = JsonSerializer.Deserialize<PowRequest>(content);
            return pow;
        }

        // Prooves to the server that a pow was solved by submitting
        // the pow's key, the solution and a nats msg as a POST request
        // the topic becomes part of the URL
        private async Task<(bool success, string serverName)> UploadWithPow(PowRequest pow, string solution, byte[] natsmsg, string topic)
        {
            string fullURL = string.Empty;
            string serverName = string.Empty;

            switch (PlayerStatus.Server)
            {
                case Server.Unknown:
                    Log.Warning("Server has not been set. Can't GetPow. Please change maps.");
                    return (false, string.Empty);
                case Server.East:
                    fullURL = ConfigurationHelper.networkSettings.AlbionDataServers.East.Url + "/pow/" + topic;
                    serverName = ConfigurationHelper.networkSettings.AlbionDataServers.East.Name;
                    break;
                case Server.West:
                    fullURL = ConfigurationHelper.networkSettings.AlbionDataServers.West.Url + "/pow/" + topic;
                    serverName = ConfigurationHelper.networkSettings.AlbionDataServers.West.Name;
                    break;
            };

            var data = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("key", pow.Key),
                new KeyValuePair<string, string>("solution", solution),
                new KeyValuePair<string, string>("serverid", ((int)PlayerStatus.Server).ToString()),
                new KeyValuePair<string, string>("natsmsg", Encoding.UTF8.GetString(natsmsg)),
            });

            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, fullURL);
            request.Headers.Add("User-Agent", "AlbionDataSharp");
            request.Content = data;

            var response = await client.SendAsync(request);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                Log.Error("HTTP Error while proving pow. Returned: {0} ({1})", response.StatusCode, await response.Content.ReadAsStringAsync());
                return (false, string.Empty);
            }

            Log.Debug("Successfully sent ingest request to {0}", fullURL);
            return (true, serverName);
        }

        // Generates a random hex string e.g.: faa2743d9181dca5
        private string RandomHex(int n)
        {
            byte[] bytes = new byte[n];

            rng.GetBytes(bytes);
            StringBuilder result = new StringBuilder(n * 2);
            foreach (byte b in bytes)
            {
                result.Append(b.ToString("x2"));
            }
            return result.ToString();
        }

        // Converts a hexadecimal string to a binary string e.g.: 0110011...
        public string ToBinaryBytes(string s)
        {
            StringBuilder buffer = new StringBuilder(s.Length * 8);
            foreach (char c in s)
            {
                buffer.Append(Convert.ToString(c, 2).PadLeft(8, '0'));
            }
            return buffer.ToString();
        }

        // Solves a pow looping through possible solutions
        // until a correct one is found
        // returns the solution
        private async Task<string> SolvePow(PowRequest pow)
        {
            var sw = Stopwatch.StartNew();

            string solution = "";
            int threadLimit = Math.Max(1, ((int)(Environment.ProcessorCount * ConfigurationHelper.networkSettings.ThreadLimitPercentage)));
            var tasks = new List<Task<string>>();
            var tokenSource = new CancellationTokenSource();
            CancellationToken ct = tokenSource.Token;
            for (int i = 0; i < threadLimit; i++)
            {
                tasks.Add(Task.Run(() => ProcessPow(pow, ct), ct));
            }

            solution = await Task.WhenAny(tasks).Result;
            tokenSource.Cancel();
            sw.Stop();
            Log.Debug("Solved PoW {key} with solution {solution} in {time} ms.", pow.Key, solution, sw.ElapsedMilliseconds.ToString());
            return solution;
        }

        private string ProcessPow(PowRequest pow, CancellationToken token)
        {
            while (true)
            {
                string randhex = RandomHex(8);
                string hash = ToBinaryBytes(GetHash("aod^" + randhex + "^" + pow.Key));
                if (hash.StartsWith(pow.Wanted))
                {
                    return randhex;
                }
                if (token.IsCancellationRequested)
                {
                    Log.Verbose("canceled PoW async task because of {token}.", nameof(token.IsCancellationRequested));
                    return "";
                }
            }
        }

        private string GetHash(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = sha256.ComputeHash(new MemoryStream(bytes));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
