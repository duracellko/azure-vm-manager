using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duracellko.WindowsAzureVmManager.Model
{
    public class HostedServices
    {
        public HostedServices()
        {
            this.Services = new List<HostedService>();
        }

        public IList<HostedService> Services { get; private set; }
    }
}
