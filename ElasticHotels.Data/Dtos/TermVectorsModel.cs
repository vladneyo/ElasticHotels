using System;
using System.Collections.Generic;
using System.Text;
using ElasticParties.Data.Models;
using Nest;

namespace ElasticParties.Data.Dtos
{
    public class TermVectorsModel
    {
        public SearchPlace Document { get; set; }
        public IReadOnlyDictionary<Field, TermVector> TermVectors { get; set; }
    }
}
