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

        public async Task<List<SearchPlace>> Search(string queryString, double lat, double lng, bool descRates)
        {
            var client = GetClient();
            var searchDescriptor = new SearchDescriptor<Place>();
            var dumper = new NestDescriptorDumper(client.RequestResponseSerializer);

            QueryContainerDescriptor<Place> queryContainer = new QueryContainerDescriptor<Place>();
            var query = queryContainer.Bool(b =>
                        b.Should(
                            q => q.Match(m => m.Field(f => f.Name).Query(queryString)),
                            q => q.Match(m => m.Field(f => f.Vicinity).Query(queryString))
                        )
                    );

            Func<SortDescriptor<Place>, SortDescriptor<Place>> SortByGeo =
                (SortDescriptor<Place> s) => s.GeoDistance(g =>
                                        g.Field(f => f.Geometry.Location)
                                            .DistanceType(GeoDistanceType.Arc)
                                            .Unit(DistanceUnit.Kilometers)
                                            .Order(SortOrder.Ascending)
                                            .Points(new GeoLocation(lat, lng)));

            Func<SortDescriptor<Place>, IPromise<IList<ISort>>> sort = s => SortByGeo(descRates ? s.Descending(SortSpecialField.Score).Descending(d => d.Rating) : s.Descending(SortSpecialField.Score));

            searchDescriptor
                .Index<Place>()
                .Query(x => query)
                .Sort(sort)
                .ScriptFields(x =>
                    x.ScriptField("distance", s => s.Source($"doc['geometry.location'].arcDistance({lat},{lng})")))
                .Take(10)
                .Source(true)
                ;

            var sss = dumper.Dump<SearchDescriptor<Place>>(searchDescriptor);

            var results = await client.SearchAsync<Place>(searchDescriptor);

            return ToSearchPlaces(results.Hits);
        }

        public async Task<object> Aggregation(double lat, double lng)
        {
            var client = GetClient();
            var searchDescriptor = new SearchDescriptor<Place>();
            var dumper = new NestDescriptorDumper(client.RequestResponseSerializer);

            searchDescriptor.Aggregations(aggs =>
                    aggs.Children<Place>("child", child =>
                        child.Aggregations(caggs =>
                            caggs.Max("max", max => max.Field(f => f.Rating)))
                            ));

            var results = await client.SearchAsync<Place>(searchDescriptor);

            var sss = dumper.Dump<SearchDescriptor<Place>>(searchDescriptor);

            return results.Aggregations.Children("child").Max("max");
        }

        public async Task<object> TermVectors(string queryString)
        {
            var client = GetClient();
            var searchDescriptor = new SearchDescriptor<Place>();
            var dumper = new NestDescriptorDumper(client.RequestResponseSerializer);

            QueryContainerDescriptor<Place> queryContainer = new QueryContainerDescriptor<Place>();
            var query = queryContainer.Bool(b =>
                        b.Should(
                            q => q.Match(m => m.Field(f => f.Name).Query(queryString)),
                            q => q.Match(m => m.Field(f => f.Vicinity).Query(queryString))
                        )
                    );

            searchDescriptor
                .Index<Place>()
                .Query(x => query)
                .Take(10)
                .Source(true)
                ;

            var sss = dumper.Dump<SearchDescriptor<Place>>(searchDescriptor);

            var results = await client.SearchAsync<Place>(searchDescriptor);

            return ToSearchPlaces(results.Hits);
        }

        public async Task CleanElastic(Action<string> output)
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
                    output?.Invoke("Error while deleting index");
                    return;
                }
                output?.Invoke("Index deleted");
            }
        }

        public async Task FillElastic(Action<string> output)
        {
            var node = new Uri(ElasticConstants.Endpoint);
            var settings = new ConnectionSettings(node);
            var client = new ElasticClient(settings);

            var existsResponse = await client.IndexExistsAsync(Indices.Index(ElasticConstants.PlacesCollectionName));
            if (!existsResponse.Exists)
            {
                output?.Invoke("Index does not exist");
                var index = settings.DefaultMappingFor<Place>(x => x.IndexName(ElasticConstants.PlacesCollectionName));
                client = new ElasticClient(index);

                var indexCreate = await client.CreateIndexAsync(IndexName.From<Place>(), i =>
                        i.Mappings(m =>
                            m.Map<Place>(mp =>
                            mp.AutoMap()
                            .Properties(p =>
                                p.Text(t => t.Fielddata(true)
                                    .TermVector(TermVectorOption.WithPositionsOffsetsPayloads)
                                    .Name(n => n.Name))
                                .Text(t => t.Fielddata(true)
                                    .Name(n => n.PlaceId))
                                .Text(t => t.Fielddata(true)
                                    .TermVector(TermVectorOption.WithPositionsOffsetsPayloads)
                                    .Name(n => n.Vicinity))
                                .Text(t => t.Fielddata(true)
                                    .Name(n => n.Types)
                                    .Fielddata(true))
                                )
                            )));
                if (!indexCreate.Acknowledged)
                {
                    output?.Invoke("Error while creating index");
                    return;
                }
                output?.Invoke("Index created");
            }

            var places = await new GooglePlacesService().GetDataAsync();
            var bulkResponse = client.Bulk(b =>
            {

                places.ForEach(p => b.Index<Place>(i => i.Document(p)));
                return b;
            });
            if (bulkResponse.Errors)
            {
                foreach (var e in bulkResponse.ItemsWithErrors)
                {
                    output?.Invoke($"{e.Error.Index} - { e.Error.Reason}");
                }
            }
            output?.Invoke("Filled");
        }

        public async Task ShowElastic(Action<string> output)
        {
            var client = GetClient();

            var res = await client.IndexExistsAsync(Indices.Index(ElasticConstants.PlacesCollectionName));
            if (!res.Exists)
            {
                output?.Invoke("Index does not exist");
                return;
            }

            var count = client.Count<Place>();

            var result = await client.SearchAsync<Place>(x => x
                .Index<Place>()
                .Take((int)count.Count));

            output?.Invoke($"Total: {result.Documents.Count}");
            foreach (var place in result.Documents)
            {
                output?.Invoke(place.Name);
            }
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

        private List<SearchPlace> ToSearchPlaces(IEnumerable<IHit<Place>> hits)
        {
            var list = new List<SearchPlace>();
            foreach (var hit in hits)
            {
                list.Add(new SearchPlace
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
