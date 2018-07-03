using ElasticParties.Data.Constants;
using ElasticParties.Data.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ElasticParties.CLI.Commands
{
    class RetrieveDataCommand : ICliCommand
    {
        public async Task Invoke()
        {
            using (var httpClient = new HttpClient())
            {
                Console.WriteLine("Search Results:");

                List<Place> places = new List<Place>();
                foreach (var place in PlaceTypes.Places)
                {
                    var content = await httpClient.GetStringAsync(string.Format(GoogleConstants.SearchPlacesLinkPattern, 50.0268781, 36.2205936, 5000, place.Value, place.Value, GoogleConstants.GooglePlacesAPIKey));
                    var json = await Task.Run(() => JObject.Parse(content));

                    places.AddRange(JsonConvert.DeserializeObject<List<Place>>(json.Property("results").Value.ToString(),
                        new JsonSerializerSettings
                        {
                            ContractResolver = new DefaultContractResolver
                            {
                                NamingStrategy = new SnakeCaseNamingStrategy()
                            }
                        }));
                }

                foreach (var place in places)
                {
                    Console.WriteLine(place.Name);
                }
            }
        }
    }
}
