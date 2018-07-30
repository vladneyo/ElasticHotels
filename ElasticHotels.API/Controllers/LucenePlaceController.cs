﻿using System;
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
        public async Task<IActionResult> BestPlacesAround(int distance, double lat, double lng, bool descRates, bool openedOnly)
        {
            return Ok(await new LuceneService().GetBestPlacesAround(distance, lat, lng, descRates, openedOnly));
        }
    }
}