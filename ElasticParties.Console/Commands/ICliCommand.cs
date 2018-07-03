using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ElasticParties.CLI.Commands
{
    interface ICliCommand
    {
        Task Invoke();
    }
}
