using ElasticParties.Data.Constants;
using ElasticParties.Data.Models;
using Nest;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ElasticParties.Services
{
    public class ElasticService
    {
        public async Task<object> GetNearest(string type, double lat, double lng, int distance)
        {
            var client = GetClient();
            
            var results = await client.SearchAsync<Place>(q =>
                q.Query(x => x.Term(t => t.Field("types").Value(type)))
                .Query(x =>
                    x.GeoDistance(g =>
                        g.Distance(new Distance(distance, DistanceUnit.Kilometers))
                        .Location(lat, lng)
                        .Field(f => f.Geometry.Location)))
                );

            return results;
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
            var index = settings.DefaultMappingFor<Place>(x => x.IndexName(ElasticConstants.PlacesCollectionName).Ignore(i => i.PlaceId));
            var client = new ElasticClient(index);
            return client;
        }
    }
}
