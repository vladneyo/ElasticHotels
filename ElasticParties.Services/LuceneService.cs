using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ElasticParties.Data.Models;
using Lucene.Net;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;

namespace ElasticParties.Services
{
    public class LuceneService
    {
        void main()
        {
            //Spatial4n.Core.Context.SpatialContext.GEO.MakePoint()
        }

        public async Task<object> GetNearest(string type, double lat, double lng, int distance)
        {
            var index = await CreateIndex(await new GooglePlacesService().GetDataAsync());

            using (var reader = IndexReader.Open(index, true))
            using (var searcher = new IndexSearcher(reader))
            {
                using(var analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30))
                {
                    var queryParser = new QueryParser(Lucene.Net.Util.Version.LUCENE_30, "Types", analyzer);
                    var query = queryParser.Parse(type);
                    var collector = TopScoreDocCollector.Create(1000, true);

                    searcher.Search(query, collector);

                    var matches = collector.TopDocs();
                    var result = new List<Place>();

                    foreach (var match in matches.ScoreDocs)
                    {
                        var id = match.Doc;
                        var doc = searcher.Doc(id);

                        result.Add(DocToPlace(doc));
                    }

                    return result;
                }
            }
        }

        public async Task<Directory> CreateIndex(List<Place> places)
        {
            
            var dir = new RAMDirectory();

            using (var analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30))
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

            var ratingField = new NumericField("Rating", 2, Field.Store.YES, true);
            ratingField.SetDoubleValue(place.Rating);

            doc.Add(new Field("Id", place.Id, Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field("Name", true, place.Name, Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS));
            doc.Add(new Field("PlaceId", place.PlaceId, Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(ratingField);
            doc.Add(new Field("Vicinity", true, place.Vicinity, Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS));
            foreach (var type in place.Types)
            {
                doc.Add(new Field("Types", type, Field.Store.YES, Field.Index.ANALYZED));
            }
            doc.Add(new Field("OpeningHours.OpenNow", place.OpeningHours?.OpenNow.ToString() ?? string.Empty, Field.Store.YES, Field.Index.ANALYZED));
            doc.Add(new Field("Geometry.Location", place.Geometry.Location.ToString(), Field.Store.YES, Field.Index.ANALYZED));

            return doc;
        }

        private Place DocToPlace(Document doc)
        {
            var place = new Place();
            place.Geometry = new Geometry();
            place.OpeningHours = new OpeningHours();

            place.Id = doc.GetField("Id").StringValue;
            place.Name = doc.GetField("Name").StringValue;
            place.PlaceId = doc.GetField("PlaceId").StringValue;
            place.Rating = double.Parse(doc.GetField("Rating").StringValue);
            place.Vicinity = doc.GetField("Vicinity").StringValue;
            place.Types = doc.GetValues("Types");

            var loc = doc.GetField("Geometry.Location").StringValue;
            place.Geometry.Location = new Nest.GeoLocation(double.Parse(loc.Substring(0, loc.IndexOf(","))), double.Parse(loc.Substring(loc.IndexOf(",") + 1, loc.Length - loc.IndexOf(",") - 1)));

            bool open = false;
            if (bool.TryParse(doc.GetField("OpeningHours.OpenNow").StringValue, out open))
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
