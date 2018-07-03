using ElasticParties.Data.Constants;
using ElasticParties.Data.Models;
using Nest;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ElasticParties.CLI.Commands
{
    class ShowElasticCollection : ICliCommand
    {
        public async Task Invoke()
        {
            var node = new Uri(ElasticConstants.Endpoint);
            var settings = new ConnectionSettings(node);
            var client = new ElasticClient(settings);

            var res = await client.IndexExistsAsync(Indices.Index(ElasticConstants.PlacesCollectionName));
            if (!res.Exists)
            {
                Console.WriteLine("Index does not exist");
            }

            var result = await client.SearchAsync<Place>(x => x
                .AllIndices());

            foreach(var place in result.Documents)
            {
                Console.WriteLine(place.Name);
            }
        }
    }
}
