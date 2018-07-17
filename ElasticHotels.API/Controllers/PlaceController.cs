using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ElasticParties.Data.Models;
using ElasticParties.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ElasticParties.API.Controllers
{
    [Produces("application/json")]
    [Route("api/Place")]
    public class PlaceController : Controller
    {
        [HttpGet]
        [Route("nearest")]
        public async Task<IActionResult> Nearest(string type, double lat, double lng, int distance)
        {
            return Ok(await new ElasticService().GetNearest(type, lat, lng, distance));
        }

        [HttpGet]
        [Route("bestaround")]
        public async Task<IActionResult> BestPlacesAround(int distance, double lat, double lng, bool descRates, bool openedOnly)
        {
            return Ok(await new ElasticService().GetBestPlacesAround(distance, lat, lng, descRates, openedOnly));
        }

        [HttpGet]
        [Route("search")]
        public async Task<IActionResult> Search(string queryString, double lat, double lng, bool descRates)
        {
            return Ok(await new ElasticService().Search(queryString, lat, lng, descRates));
        }

        [HttpGet]
        [Route("aggr")]
        public async Task<IActionResult> Aggregation(double lat, double lng)
        {
            return Ok(await new ElasticService().Aggregation(lat, lng));
        }

        [HttpGet]
        [Route("termvectors")]
        public async Task<IActionResult> TermVectors(double lat, double lng)
        {
            return Ok(await new ElasticService().TermVectors(lat, lng));
        }
    }
}