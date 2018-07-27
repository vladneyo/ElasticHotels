using ElasticParties.Lucene.Data.Extensions;
using ElasticParties.Lucene.Search.Helpers;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Search.Function;
using Spatial4n.Core.Context;
using Spatial4n.Core.Distance;
using Spatial4n.Core.Shapes;
using System;
using System.Collections.Generic;
using System.Text;

namespace ElasticParties.Lucene.Search.Queries
{
    public class DistanceCustomScoreQuery : CustomScoreQuery
    {
        private readonly SpatialContext _ctx;
        private readonly Point _origin;
        private readonly double _distance;

        public DistanceCustomScoreQuery(Query subQuery, SpatialContext ctx, Point origin, double distance) : base(subQuery)
        {
            _ctx = ctx;
            _origin = origin;
            _distance = distance;
        }

        public override string Name() => "DistanceCustomScoreQuery";

        protected override CustomScoreProvider GetCustomScoreProvider(IndexReader reader)
        {
            return new DistranceCustomScoreProvider(reader, _ctx, _origin, _distance);
        }

        private class DistranceCustomScoreProvider : CustomScoreProvider
        {
            private readonly IndexReader _reader;
            private readonly SpatialContext _ctx;
            private readonly Point _origin;
            private readonly double _distance;
            private readonly CustomDistanceCalculator _distanceCalculator;

            public DistranceCustomScoreProvider(IndexReader reader, SpatialContext ctx, Point origin, double distance) : base(reader)
            {
                _reader = reader;
                _ctx = ctx;
                _origin = origin;
                _distance = distance;
                this._distanceCalculator = new CustomDistanceCalculator();
            }

            public override float CustomScore(int doc, float subQueryScore, float valSrcScore)
            {
                var document = _reader.Document(doc);
                var distanceKm = _distanceCalculator.Calculate(_ctx, document, _origin);
                if (distanceKm < _distance)
                {
                    // the smaller the distance - the bigger the score
                    float modifier = 1.0f - (float)distanceKm / (float)_distance;
                    if (modifier < 0.0f) modifier = 0.0f;
                    if (modifier > 1.0f) modifier = 1.0f;
                    float newScore = subQueryScore * (1.0f + modifier);
                    return newScore;
                }
                return 0.0f;
            }


        }
    }
}
