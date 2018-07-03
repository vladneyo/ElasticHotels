using ElasticParties.Data.Constants;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ElasticParties.Data.Models;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using System.Text;

namespace ElasticParties.CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine("Search Results:");
            Task.Run(() => MainAsync()).GetAwaiter();
            Console.ReadKey();
        }

        static async void MainAsync()
        {
            var httpClient = new HttpClient();
            var content = await httpClient.GetStringAsync(string.Format(GoogleConstants.SearchPlacesLinkPattern, 50.0268781, 36.2205936, 5000, "bar", "bar", GoogleConstants.GooglePlacesAPIKey));
            var json = await Task.Run(() => JObject.Parse(content));
            
            var places = JsonConvert.DeserializeObject<List<Place>>(json.Property("results").Value.ToString(), new JsonSerializerSettings { ContractResolver = new DefaultContractResolver { NamingStrategy = new SnakeCaseNamingStrategy() } });
            foreach (var place in places)
            {
                Console.WriteLine(place.Name);
            }
        }
    }
}
