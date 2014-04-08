using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Duracellko.WindowsAzureVmManager.Web.Models
{
    public class VirtualMachine
    {
        public string Name { get; set; }

        public string CloudServiceName { get; set; }

        public string Status { get; set; }
    }
}