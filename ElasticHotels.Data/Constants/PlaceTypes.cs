using System;
using System.Collections.Generic;
using System.Text;

namespace ElasticParties.Data.Constants
{
    public class PlaceTypes
    {
        public const string AmusementPark = "amusement_park";
        public const string Bar = "bar";
        public const string Cafe = "cafe";
        public const string Casino = "casino";
        public const string LiquorStore = "liquor_store";
        public const string NightClub = "night_club";
        public const string Spa = "spa";

        public static readonly Dictionary<string, string> Places = new Dictionary<string, string>
        {
            { AmusementPark, "парк" },
            { Bar, "bar pub" },
            { Cafe, "кафе" },
            { Casino, "казино" },
            { LiquorStore, "" },
            { NightClub, "клуб стриптиз массаж" },
            { Spa, "спа массаж сауна" }
        };
    }
}
