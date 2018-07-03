using System;
using System.Collections.Generic;
using System.Text;

namespace ElasticParties.Data.Models
{
    public class Place
    {
        public string Id { get; set; }
        public Geometry Geometry { get; set; }
        public string Name { get; set; }
        public OpeningHours OpeningHours { get; set; }
        public string PlaceId { get; set; }
        public double Rating { get; set; }
        public string[] Types { get; set; }
        public string Vicinity { get; set; }
    }
}
