using System.Collections.Generic;
using System.Threading.Tasks;
using ElasticParties.Data.Dtos;
using ElasticParties.Data.Models;
using ElasticParties.Lucene.Data.Constants;
using ElasticParties.Lucene.Data.Extensions;
using ElasticParties.Lucene.Search.Helpers;
using ElasticParties.Lucene.Search.Queries;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Spatial4n.Core.Context;
using LuceneNet = Lucene.Net;

namespace ElasticParties.Services
{
    public class LuceneService
    {
        public async Task<List<NearestPlace>> GetNearest(string type, double lat, double lng, int distance)
        {
            var index = await CreateIndex(await new GooglePlacesService().GetDataAsync());

            using (var reader = IndexReader.Open(index, true))
            using (var searcher = new IndexSearcher(reader))
            {
                using (var analyzer = new StandardAnalyzer(LuceneNet.Util.Version.LUCENE_30))
                {
                    var ctx = SpatialContext.GEO;
                    var origin = ctx.MakePoint(lat, lng);
                    var distanceCalculator = new CustomDistanceCalculator();

                    var typesQueryParser = new QueryParser(LuceneNet.Util.Version.LUCENE_30, Schema.Types, analyzer);
                    var typesQuery = typesQueryParser.Parse(type);

                    var distanceQuery = new DistanceCustomScoreQuery(typesQuery, ctx, origin, distance);

                    var collector = TopScoreDocCollector.Create(1000, true);
                    var posCollector = new PositiveScoresOnlyCollector(collector);

                    searcher.Search(distanceQuery, posCollector);

                    var matches = collector.TopDocs();
                    var result = new List<NearestPlace>();

                    foreach (var match in matches.ScoreDocs)
                    {
                        var id = match.Doc;
                        var doc = searcher.Doc(id);

                        var nearestPlace = DocToNearestPlace(doc);
                        nearestPlace.Distance = distanceCalculator.Calculate(ctx, doc, origin, true);
                        result.Add(nearestPlace);
                    }

                    return result;
                }
            }
        }

        public async Task<List<BestPlaceAround>> GetBestPlacesAround(int distance, double lat, double lng, bool descRates, bool openedOnly)
        {
            var index = await CreateIndex(await new GooglePlacesService().GetDataAsync());

            using (var reader = IndexReader.Open(index, true))
            using (var searcher = new IndexSearcher(reader))
            {
                using (var analyzer = new StandardAnalyzer(LuceneNet.Util.Version.LUCENE_30))
                {
                    var ctx = SpatialContext.GEO;
                    var origin = ctx.MakePoint(lat, lng);
                    var distanceCalculator = new CustomDistanceCalculator();

                    var openNowQueryParser = new QueryParser(LuceneNet.Util.Version.LUCENE_30, Schema.OpenNow, analyzer);
                    var openNowQuery = openNowQueryParser.Parse(openedOnly.ToString());

                    var distanceQuery = new DistanceCustomScoreQuery(openNowQuery, ctx, origin, distance);

                    var sort = new Sort();
                    sort.SetSort(new SortField[] 
                    {
                        SortField.FIELD_SCORE,
                        new SortField(Schema.Rating, SortField.DOUBLE, descRates)
                    });

                    var matches = searcher.Search(distanceQuery, null, 100, sort);

                    var result = new List<BestPlaceAround>();

                    foreach (var match in matches.ScoreDocs)
                    {
                        var id = match.Doc;
                        var doc = searcher.Doc(id);

                        var nearestPlace = DocToBestPlaceAround(doc);
                        nearestPlace.Distance = distanceCalculator.Calculate(ctx, doc, origin, true);
                        result.Add(nearestPlace);
                    }

                    return result;
                }
            }
        }

        public async Task<Directory> CreateIndex(List<Place> places)
        {

            var dir = new RAMDirectory();

            using (var analyzer = new StandardAnalyzer(LuceneNet.Util.Version.LUCENE_30))
            using (var writter = new IndexWriter(dir, analyzer, IndexWriter.MaxFieldLength.UNLIMITED))
            {
                foreach (var place in places)
                {
                    writter.AddDocument(CreateDocument(place));
                }

                writter.Optimize();
                writter.Flush(true, true, true);
            }

            return dir;
        }

        public Document CreateDocument(Place place)
        {
            var doc = new Document();

            var ratingField = new NumericField(Schema.Rating, 2, Field.Store.YES, true);
            ratingField.SetDoubleValue(place.Rating);

            doc.Add(new Field(Schema.Id, place.Id, Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field(Schema.Name, true, place.Name, Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS));
            doc.Add(new Field(Schema.PlaceId, place.PlaceId, Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(ratingField);
            doc.Add(new Field(Schema.Vicinity, true, place.Vicinity, Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS));
            foreach (var type in place.Types)
            {
                doc.Add(new Field(Schema.Types, type, Field.Store.YES, Field.Index.ANALYZED));
            }
            doc.Add(new Field(Schema.OpenNow, place.OpeningHours?.OpenNow.ToString() ?? string.Empty, Field.Store.YES, Field.Index.ANALYZED));
            doc.Add(new Field(Schema.Location, place.Geometry.Location.ToString(), Field.Store.YES, Field.Index.ANALYZED));

            return doc;
        }

        private NearestPlace DocToNearestPlace(Document doc)
        {
            var place = new NearestPlace();
            place.OpeningHours = new OpeningHours();

            place.Id = doc.GetField(Schema.Id).StringValue;
            place.Name = doc.GetField(Schema.Name).StringValue;
            place.Rating = double.Parse(doc.GetField(Schema.Rating).StringValue);
            place.Vicinity = doc.GetField(Schema.Vicinity).StringValue;
            place.Types = doc.GetValues(Schema.Types);

            bool open = false;
            if (bool.TryParse(doc.GetField(Schema.OpenNow).StringValue, out open))
            {
                place.OpeningHours.OpenNow = open;
            }
            else
            {
                place.OpeningHours.OpenNow = false;
            }

            return place;
        }

        private BestPlaceAround DocToBestPlaceAround(Document doc)
        {
            var place = new BestPlaceAround();
            place.OpeningHours = new OpeningHours();

            place.Id = doc.GetField(Schema.Id).StringValue;
            place.Name = doc.GetField(Schema.Name).StringValue;
            place.Rating = double.Parse(doc.GetField(Schema.Rating).StringValue);
            place.Vicinity = doc.GetField(Schema.Vicinity).StringValue;
            place.Types = doc.GetValues(Schema.Types);

            bool open = false;
            if (bool.TryParse(doc.GetField(Schema.OpenNow).StringValue, out open))
            {
                place.OpeningHours.OpenNow = open;
            }
            else
            {
                place.OpeningHours.OpenNow = false;
            }

            return place;
        }
    }
}
