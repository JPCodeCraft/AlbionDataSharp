using AlbionData.Models;
using AlbionDataSharp.Config;
using AlbionDataSharp.State;
using Serilog;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace AlbionDataSharp.Network.Http
{
    public class PowManager
    {
        public async Task Upload(MarketUpload marketUpload)
        {
            var offers = marketUpload.Orders.Where(x => x.AuctionType == "offer").Count();
            var requests = marketUpload.Orders.Where(x => x.AuctionType == "request").Count();

            try
            {
                var data = JsonSerializer.SerializeToUtf8Bytes(marketUpload, new JsonSerializerOptions { IncludeFields = true });
                var powRequest = await GetPowRequest();
                var powSolution = await SolvePow(powRequest);
                if (await UploadWithPow(powRequest, powSolution, data, ConfigurationHelper.natsSettings.MarketOrdersIngestSubject))
                {
                    if (offers > 0 && requests == 0) Log.Information("Published {amount} offers to AODataProject.", offers);
                    else if (offers == 0 && requests > 0) Log.Information("Published {amount} requests to AODataProject.", requests);
                    else if (offers == 0 && requests == 0) Log.Debug("Published nothing to AODataProject.");
                    else Log.Information("Published {amount} offers and {amount} requests to AODataProject.", offers, requests);
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
                if (await UploadWithPow(powRequest, powSolution, data, ConfigurationHelper.natsSettings.MarketHistoriesIngestSubject))
                {
                    Log.Information("Published {Amount} histories for {ItemID} quality {Quality} in location {Location} timescale {Timescale} to AODataProject.",
                        marketHistoriesUpload.MarketHistories.Count, marketHistoriesUpload.AlbionId, marketHistoriesUpload.QualityLevel,
                        marketHistoriesUpload.LocationId, marketHistoriesUpload.Timescale);
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
                    fullURL = ConfigurationHelper.natsSettings.AlbionDataEastServer + "/pow";
                    break;
                case Server.West:
                    fullURL = ConfigurationHelper.natsSettings.AlbionDataWestServer + "/pow";
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
        private async Task<bool> UploadWithPow(PowRequest pow, string solution, byte[] natsmsg, string topic)
        {
            string fullURL = string.Empty;

            switch (PlayerStatus.Server)
            {
                case Server.Unknown:
                    Log.Warning("Server has not been set. Can't GetPow. Please change maps.");
                    return false;
                case Server.East:
                    fullURL = ConfigurationHelper.natsSettings.AlbionDataEastServer + "/pow/" + topic;
                    break;
                case Server.West:
                    fullURL = ConfigurationHelper.natsSettings.AlbionDataWestServer + "/pow/" + topic;
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
                return false;
            }

            Log.Debug("Successfully sent ingest request to {0}", fullURL);
            return true;
        }

        // Generates a random hex string e.g.: faa2743d9181dca5
        private string RandomHex(int n)
        {
            byte[] bytes = new byte[n];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            var result = BitConverter.ToString(bytes).Replace("-", "").ToLower();
            return result;
        }

        // Converts a hexadecimal string to a binary string e.g.: 0110011...
        public static string ToBinaryBytes(string s)
        {
            StringBuilder buffer = new StringBuilder();
            for (int i = 0; i < s.Length; i++)
            {
                buffer.AppendFormat("{0}", Convert.ToString(s[i], 2).PadLeft(8, '0'));
            }
            return buffer.ToString();
        }

        // Solves a pow looping through possible solutions
        // until a correct one is found
        // returns the solution
        private async Task<string> SolvePow(PowRequest pow)
        {
            string solution = "";
            while (true)
            {
                string randhex = RandomHex(8);
                string hash = ToBinaryBytes(await GetHash("aod^" + randhex + "^" + pow.Key));
                if (hash.StartsWith(pow.Wanted))
                {
                    solution = randhex;
                    Log.Debug("Solved PoW {key} with solution {solution}.", pow.Key, solution);
                    break;
                }
            }
            return solution;
        }
        private async Task<string> GetHash(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = await sha256.ComputeHashAsync(new MemoryStream(bytes));
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
