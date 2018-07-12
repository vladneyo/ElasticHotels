using ElasticParties.Data.Constants;
using ElasticParties.Data.Models;
using ElasticParties.Services;
using Nest;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ElasticParties.CLI.Commands
{
    class ShowElasticCollection : ICliCommand
    {
        public async Task Invoke()
        {
            var elastic = new ElasticService();
            await elastic.ShowElastic(Console.WriteLine);
        }
    }
}
