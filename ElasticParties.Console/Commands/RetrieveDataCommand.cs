using ElasticParties.Data.Constants;
using ElasticParties.Data.Converters;
using ElasticParties.Data.Models;
using ElasticParties.Services;
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
            Console.WriteLine("Search Results:");
            List<Place> places = await new GooglePlacesService().GetDataAsync();
            Console.WriteLine($"Total: {places.Count}");
            foreach (var place in places)
            {
                Console.WriteLine(place.Name);
            }
        }
    }
}
