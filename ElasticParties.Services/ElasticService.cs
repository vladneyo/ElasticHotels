using ElasticParties.Data.Constants;
using ElasticParties.Data.Dtos;
using ElasticParties.Data.Models;
using ElasticParties.Services.Helpers;
using Elasticsearch.Net;
using Nest;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ElasticParties.Services
{
    public class ElasticService
    {
        public async Task<List<NearestPlace>> GetNearest(string type, double lat, double lng, int distance)
        {
            var client = GetClient();
            var searchDescriptor = new SearchDescriptor<Place>();
            var dumper = new NestDescriptorDumper(client.RequestResponseSerializer);

            searchDescriptor
                .Index<Place>()
                .Query(x => x.Term(t => t.Field("types").Value(type)))
                .Query(x =>
                    x.GeoDistance(g =>
                        g.Distance(new Distance(distance, DistanceUnit.Kilometers))
                        .Location(lat, lng)
                        .Field(f => f.Geometry.Location)))
                .ScriptFields(x =>
                    x.ScriptField("distance", s => s.Source($"doc['geometry.location'].arcDistance({lat},{lng})")))
                .Source(true)
                .DocValueFields(d => 
                    d.Field(f => f.Name)
                    .Field(f => f.Vicinity)
                    .Field(f => f.Types))
                ;

            var results = await client.SearchAsync<Place>(searchDescriptor);

            //var sss = dumper.Dump<SearchDescriptor<Place>>(searchDescriptor);

            return ToNearestPlaces(results.Hits);
        }

        public Task<object> GetBestPlacesAround(int distance, double lat, double lng, bool descRates, bool descDistance, bool openedOnly)
        {
            return null;
        }

        public Task<object> Search(string queryString, double lat, double lng, bool descRates, bool descDistance)
        {
            return null;
        }

        private ElasticClient GetClient()
        {
            var node = new Uri(ElasticConstants.Endpoint);
            var settings = new ConnectionSettings(node);            
            var index = settings.DefaultMappingFor<Place>(x => x.IndexName(ElasticConstants.PlacesCollectionName));
            var client = new ElasticClient(index);
            return client;
        }

        private List<NearestPlace> ToNearestPlaces(IEnumerable<IHit<Place>> hits)
        {
            var list = new List<NearestPlace>();
            foreach (var hit in hits)
            {
                list.Add(new NearestPlace
                {
                    Id = hit.Source.Id,
                    Name = hit.Source.Name,
                    OpeningHours = hit.Source.OpeningHours,
                    Rating = hit.Source.Rating,
                    Types = hit.Source.Types,
                    Vicinity = hit.Source.Vicinity,
                    Distance = Math.Round(hit.Fields.Value<double>("distance"), 2, MidpointRounding.AwayFromZero)
                });
            }
            return list;
        }
    }
}
