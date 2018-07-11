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
                .Query(x => 
                    x.Bool(b => 
                        b.Must(
                            m => m.Term(t => t.Field(f => f.Types).Value(type)), 
                            m => m.GeoDistance(g =>
                                g.Distance(new Distance(distance, DistanceUnit.Kilometers))
                                .Location(lat, lng)
                                .Field(f => f.Geometry.Location))
                        )
                    ))
                .ScriptFields(x =>
                    x.ScriptField("distance", s => s.Source($"doc['geometry.location'].arcDistance({lat},{lng})")))
                .Source(true)
                .DocValueFields(d => 
                    d.Field(f => f.Name)
                    .Field(f => f.Vicinity)
                    .Field(f => f.Types))
                ;

            var sss = dumper.Dump<SearchDescriptor<Place>>(searchDescriptor);

            var results = await client.SearchAsync<Place>(searchDescriptor);

            return ToNearestPlaces(results.Hits);
        }

        public async Task<List<BestPlaceAround>> GetBestPlacesAround(int distance, double lat, double lng, bool descRates, bool openedOnly)
        {
            var client = GetClient();
            var searchDescriptor = new SearchDescriptor<Place>();
            var dumper = new NestDescriptorDumper(client.RequestResponseSerializer);

            Func<SortDescriptor<Place>, bool, SortDescriptor<Place>> SortByRates = (SortDescriptor<Place> s, bool desc) => desc ? s.Descending(d => d.Rating) : s;

            searchDescriptor
                .Index<Place>()
                .Query(x => 
                    x.Bool(b => 
                        b.Must(
                            m => m.Term(t => t.Field(f => f.OpeningHours.OpenNow).Value(openedOnly)),
                            m => m.GeoDistance(g =>
                                g.Distance(new Distance(distance, DistanceUnit.Kilometers))
                                .Location(lat, lng)
                                .Field(f => f.Geometry.Location))
                        )
                    ))
                .Sort(s => SortByRates(s.GeoDistance(g =>
                                        g.Field(f => f.Geometry.Location)
                                            .DistanceType(GeoDistanceType.Arc)
                                            .Unit(DistanceUnit.Kilometers)
                                            .Order(SortOrder.Ascending)
                                            .Points(new GeoLocation(lat, lng))), descRates))
                .ScriptFields(x =>
                    x.ScriptField("distance", s => s.Source($"doc['geometry.location'].arcDistance({lat},{lng})")))
                .Take(10)
                .Source(true)
                ;

            var sss = dumper.Dump<SearchDescriptor<Place>>(searchDescriptor);

            var results = await client.SearchAsync<Place>(searchDescriptor);

            return ToBestPlacesAround(results.Hits);
        }

        public async Task<object> Search(string queryString, double lat, double lng, bool descRates)
        {
            var client = GetClient();
            var searchDescriptor = new SearchDescriptor<Place>();
            var dumper = new NestDescriptorDumper(client.RequestResponseSerializer);

            Func<SortDescriptor<Place>, SortDescriptor<Place>> SortByGeo =
                (SortDescriptor<Place> s) => s.GeoDistance(g =>
                                        g.Field(f => f.Geometry.Location)
                                            .DistanceType(GeoDistanceType.Arc)
                                            .Unit(DistanceUnit.Kilometers)
                                            .Order(SortOrder.Ascending)
                                            .Points(new GeoLocation(lat, lng)));

            searchDescriptor
                .Index<Place>()
                .Query(x =>
                    x.Bool(b =>
                        b.Should(
                            q => q.Match(m => m.Field(f => f.Name).Query(queryString)),
                            q => q.Match(m => m.Field(f => f.Vicinity).Query(queryString))
                        )
                    ))
                .Sort(s => SortByGeo(descRates ? s.Descending(d => d.Rating) : s))
                .ScriptFields(x =>
                    x.ScriptField("distance", s => s.Source($"doc['geometry.location'].arcDistance({lat},{lng})")))
                .Take(10)
                .Source(true)
                ;

            var sss = dumper.Dump<SearchDescriptor<Place>>(searchDescriptor);

            var results = await client.SearchAsync<Place>(searchDescriptor);

            return ToBestPlacesAround(results.Hits);
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

        private List<BestPlaceAround> ToBestPlacesAround(IEnumerable<IHit<Place>> hits)
        {
            var list = new List<BestPlaceAround>();
            foreach (var hit in hits)
            {
                list.Add(new BestPlaceAround
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
