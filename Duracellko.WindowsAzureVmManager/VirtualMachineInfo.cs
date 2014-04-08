using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duracellko.WindowsAzureVmManager
{
    public class VirtualMachineInfo
    {
        public string Name { get; set; }

        public string CloudServiceName { get; set; }

        public VirtualMachineStatus Status { get; set; }
    }
}
