using Elasticsearch.Net;
using Nest;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ElasticParties.Services.Helpers
{
    class NestDescriptorDumper
    {
        private readonly IElasticsearchSerializer _serializer;

        public NestDescriptorDumper(IElasticsearchSerializer serializer)
        {
            _serializer = serializer;
        }

        public string Dump<T>(T descriptor) where T : IRequest
        {
            if (descriptor == null)
                return null;

            using (var memStream = new MemoryStream())
            {
                _serializer.Serialize(descriptor, memStream);
                return Encoding.UTF8.GetString(memStream.ToArray());
            }
        }
    }
}
