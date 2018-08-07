using ElasticParties.Lucene.Data.Extensions;
using Lucene.Net.Documents;
using Spatial4n.Core.Context;
using Spatial4n.Core.Distance;
using Spatial4n.Core.Shapes;
using System;
using System.Collections.Generic;
using System.Text;

namespace ElasticParties.Lucene.Search.Helpers
{
    public class CustomDistanceCalculator
    {
        public double Calculate(SpatialContext ctx, Document document, Point origin, bool round = false)
        {
            var point = document.GetPoint(ctx);
            var calc = ctx.GetDistCalc();
            var distance = calc.Distance(origin, point);
            var distanceKm = DistanceUtils.Degrees2Dist(distance, DistanceUtils.EARTH_EQUATORIAL_RADIUS_KM);
            return round ? Math.Round(distanceKm * 1000d, 2, MidpointRounding.AwayFromZero) : distanceKm;
        }
    }
}
