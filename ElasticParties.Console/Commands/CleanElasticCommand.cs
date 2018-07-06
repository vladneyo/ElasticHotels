using ElasticParties.Data.Constants;
using ElasticParties.Data.Models;
using Nest;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ElasticParties.CLI.Commands
{
    class CleanElasticCommand : ICliCommand
    {
        public async Task Invoke()
        {
            var node = new Uri(ElasticConstants.Endpoint);
            var settings = new ConnectionSettings(node);
            var client = new ElasticClient(settings);

            var existsResponse = await client.IndexExistsAsync(Indices.Index(ElasticConstants.PlacesCollectionName));
            if (existsResponse.Exists)
            {
                var index = settings.DefaultMappingFor<Place>(x => x.IndexName(ElasticConstants.PlacesCollectionName));
                client = new ElasticClient(index);
                var indexDelete = await client.DeleteIndexAsync(IndexName.From<Place>());
                if (!indexDelete.Acknowledged)
                {
                    Console.WriteLine("Error while deleting index");
                    return;
                }
                Console.WriteLine("Index deleted");
            }
        }
    }
}
