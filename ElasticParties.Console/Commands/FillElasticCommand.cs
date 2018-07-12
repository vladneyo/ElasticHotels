using ElasticParties.Data.Constants;
using ElasticParties.Data.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using ElasticParties.Services;

namespace ElasticParties.CLI.Commands
{
    class FillElasticCommand : ICliCommand
    {
        public async Task Invoke()
        {
            var elastic = new ElasticService();
            await elastic.FillElastic(Console.WriteLine);
        }
    }
}
