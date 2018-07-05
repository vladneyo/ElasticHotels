using ElasticParties.Data.Models;
using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace ElasticParties.Data.Converters
{
    public class LocationConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(GeoLocation) == objectType;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var o = JObject.Load(reader);
            return new GeoLocation(o.Value<double>("lat"), o.Value<double>("lng"));
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
