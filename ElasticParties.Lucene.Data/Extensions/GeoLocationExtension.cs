using Lucene.Net.Documents;
using Spatial4n.Core.Context;
using Spatial4n.Core.Shapes;
using System;
using System.Collections.Generic;
using System.Text;

namespace ElasticParties.Lucene.Data.Extensions
{
    public static class GeoLocationExtension
    {
        public static Point GetPoint(this Document doc, SpatialContext ctx)
        {
            var location = doc.GetField("Geometry.Location").StringValue;
            return ctx.MakePoint(GetLat(location), GetLon(location));
        }

        public static double GetLat(this string location)
        {
            return double.Parse(location.Substring(0, location.IndexOf(",")));
        }

        public static double GetLon(this string location)
        {
            return double.Parse(location.Substring(location.IndexOf(",") + 1, location.Length - location.IndexOf(",") - 1));
        }
    }
}
