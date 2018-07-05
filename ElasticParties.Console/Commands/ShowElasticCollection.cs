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
            var index = settings.DefaultMappingFor<Place>(x => x.IndexName(ElasticConstants.PlacesCollectionName).Ignore(i => i.PlaceId));
            var client = new ElasticClient(index);

            var res = await client.IndexExistsAsync(Indices.Index(ElasticConstants.PlacesCollectionName));
            if (!res.Exists)
            {
                Console.WriteLine("Index does not exist");
            }

            var count = client.Count<Place>();

            var result = await client.SearchAsync<Place>(x => x
                .Index<Place>()
                .Take((int)count.Count));

            Console.WriteLine($"Total: {result.Documents.Count}");
            foreach(var place in result.Documents)
            {
                Console.WriteLine(place.Name);
            }
        }
    }
}
