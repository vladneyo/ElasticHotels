using ElasticParties.Data.Constants;
using ElasticParties.Data.Models;
using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace ElasticParties.CLI.Commands
{
    class FillElasticCommand : ICliCommand
    {
        public async Task Invoke()
        {
            var node = new Uri(ElasticConstants.Endpoint);
            var settings = new ConnectionSettings(node);
            var client = new ElasticClient(settings);

            var existsResponse = await client.IndexExistsAsync(Indices.Index(ElasticConstants.PlacesCollectionName));
            if (!existsResponse.Exists)
            {
                Console.WriteLine("Index does not exist");
                var index = settings.DefaultMappingFor<Place>(x => x.IndexName(ElasticConstants.PlacesCollectionName));
                client = new ElasticClient(index);

                var indexCreate = await client.CreateIndexAsync(IndexName.From<Place>(), i =>
                        i.Mappings(m =>
                            m.Map<Place>(mp =>
                            mp.AutoMap()
                            .Properties(p =>
                                p.Text(t => t.Fielddata(true)
                                    .Name(n => n.Name))
                                .Text(t => t.Fielddata(true)
                                    .Name(n => n.PlaceId))
                                .Text(t => t.Fielddata(true)
                                    .Name(n => n.Vicinity))
                                .Text(t => t.Fielddata(true)
                                    .Name(n => n.Types)
                                    .Fielddata(true))
                                )
                            )));
                if (!indexCreate.Acknowledged)
                {
                    Console.WriteLine("Error while creating index");
                    return;
                }
                Console.WriteLine("Index created");
            }

            var places = await RetrieveDataCommand.GetDataAsync();
            var bulkResponse = client.Bulk(b =>
                {

                    places.ForEach(p => b.Index<Place>(i => i.Document(p)));
                    return b;
                });
            if (bulkResponse.Errors)
            {
                foreach (var e in bulkResponse.ItemsWithErrors)
                {
                    Console.WriteLine($"{e.Error.Index} - { e.Error.Reason}");
                }
            }
            Console.WriteLine("Filled");
        }
    }
}
