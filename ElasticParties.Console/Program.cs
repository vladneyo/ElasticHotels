using ElasticParties.Data.Constants;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ElasticParties.Data.Models;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using System.Text;
using ElasticParties.CLI.Commands;

namespace ElasticParties.CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            Task.Run(() => MainAsync()).Wait();
            Console.ReadKey();
        }

        static async void MainAsync()
        {
            int command = 0;
            do
            {
                Console.OutputEncoding = Encoding.UTF8;
                Console.WriteLine("----------------------------------");
                Console.WriteLine("1 - Retrieve and show data");
                Console.WriteLine("2 - Show data from Elastic");
                Console.WriteLine("3 - Fill Elastic");
                Console.WriteLine("0 - Exit");
                command = int.Parse(Console.ReadLine());
                Console.WriteLine("----------------------------------");

                switch (command)
                {
                    case 1:
                        await new RetrieveDataCommand().Invoke();
                        break;
                    case 2:
                        await new ShowElasticCollection().Invoke();
                        break;
                    case 3:
                        await new FillElasticCommand().Invoke();
                        break;
                }
            }
            while (command != 0);
        }
    }
}
