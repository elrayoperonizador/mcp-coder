using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCP.POC.Tools
{
    public class MyResourcesFactory
    {
        public static MyResources New(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            return new MyResources(configuration, loggerFactory);
        }
    }
}
