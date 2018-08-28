using ElasticParties.Data.Dtos;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using System;
using System.Collections.Generic;
using System.Text;
using static Lucene.Net.Documents.Field;

namespace ElasticParties.Lucene.Data.Dtos
{
    public class TermVectorsModel
    {
        public TermVectorsModel()
        {
            Terms = new Dictionary<string, TermInfo>();
        }
        public ITermFreqVector TermVector { get; set; }
        public Dictionary<string, TermInfo> Terms { get; set; }
    }
    public class TermInfo
    {
        public int Index { get; set; }
        public int TermFrequency { get; set; }
    }

}
