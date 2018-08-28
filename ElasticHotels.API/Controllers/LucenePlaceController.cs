using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ElasticParties.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ElasticParties.API.Controllers
{
    [Produces("application/json")]
    [Route("api/LucenePlace")]
    public class LucenePlaceController : Controller
    {
        [HttpGet]
        [Route("nearest")]
        public async Task<IActionResult> Nearest(string type, double lat, double lng, int distance)
        {
            return Ok(await new LuceneService().GetNearest(type, lat, lng, distance));
        }

        [HttpGet]
        [Route("bestaround")]
        public async Task<IActionResult> BestPlacesAround(int distance, double lat, double lng, bool openedOnly)
        {
            return Ok(await new LuceneService().GetBestPlacesAround(distance, lat, lng, openedOnly));
        }

        [HttpGet]
        [Route("search")]
        public async Task<IActionResult> Search(string queryString, double lat, double lng)
        {
            return Ok(await new LuceneService().Search(queryString, lat, lng));
        }

        [HttpGet]
        [Route("aggr")]
        public async Task<IActionResult> Aggregation(double lat, double lng)
        {
            return Ok(await new LuceneService().Aggregation(lat, lng));
        }

        [HttpGet]
        [Route("termvectors")]
        public async Task<IActionResult> TermVectors(string queryString, double lat, double lng)
        {
            return Ok(await new LuceneService().TermVectors(queryString, lat, lng));
        }
    }
}