using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duracellko.WindowsAzureVmManager.Model
{
    public class RoleInstance
    {
        public string RoleName { get; set; }

        public string InstanceName { get; set; }

        public string InstanceStatus { get; set; }

        public string PowerState { get; set; }
    }
}
