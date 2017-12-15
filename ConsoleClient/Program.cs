using System;
using System.Net.Http;
using System.Threading.Tasks;
using IdentityModel.Client;
using Newtonsoft.Json.Linq;

namespace Client
{
    class Program
    {
        public static void Main(string[] args) => MainAsync(args).GetAwaiter().GetResult();

        private static async Task MainAsync(string[] args)
        {
            // discover endpoints from metadata
            var disco = await DiscoveryClient.GetAsync("http://localhost:5000");
            if (disco.IsError)
            {
                Console.WriteLine(disco.Error);
                return;
            }

            // request token
            TokenClient tokenClient = new TokenClient(disco.TokenEndpoint, "client", "secret");
            TokenResponse tokenResponse;
            if (args?.Length >= 2)
            {
                tokenClient = new TokenClient(disco.TokenEndpoint, "ro.client", "secret");
                tokenResponse = await tokenClient.RequestResourceOwnerPasswordAsync(args[0], args[1], "api1");
            }
            else
            {
                tokenClient = new TokenClient(disco.TokenEndpoint, "client", "secret");
                tokenResponse = await tokenClient.RequestClientCredentialsAsync("api1");
            }
            if (tokenResponse.IsError)
            {
                Console.WriteLine(tokenResponse.Error);
                return;
            }

            Console.WriteLine(tokenResponse.Json);
            Console.WriteLine("\n\n");

            // call api
            var client = new HttpClient();
            client.SetBearerToken(tokenResponse.AccessToken);

            var response = await client.GetAsync("http://localhost:5001/api/identity");
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine(response.StatusCode);
            }
            else
            {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine(JArray.Parse(content));
            }
        }
    }
}
