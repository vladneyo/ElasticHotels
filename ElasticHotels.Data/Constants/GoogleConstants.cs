using System;
using System.Collections.Generic;
using System.Text;

namespace ElasticParties.Data.Constants
{
    public class GoogleConstants
    {
        public const string GooglePlacesAPIKey = "AIzaSyAi9Yeb6X-twTIlouz8_y66DQ5GBBoav_w";
        public static readonly string SearchPlacesLinkPattern = @"https://maps.googleapis.com/maps/api/place/nearbysearch/json?location={0},{1}&radius={2}&types={3}&name={4}&key={5}";
    }
}
