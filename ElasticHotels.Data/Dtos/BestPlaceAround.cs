using ElasticParties.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ElasticParties.Data.Dtos
{
    public class BestPlaceAround
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public OpeningHours OpeningHours { get; set; }
        public double Rating { get; set; }
        public string[] Types { get; set; }
        public string Vicinity { get; set; }
        public double Distance { get; set; }
    }
}
