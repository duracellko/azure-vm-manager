using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duracellko.WindowsAzureVmManager.Model
{
    public class HostedService
    {
        public string Url { get; set; }

        public string ServiceName { get; set; }

        public string Status { get; set; }
    }
}
