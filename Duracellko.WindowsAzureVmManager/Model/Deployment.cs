using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duracellko.WindowsAzureVmManager.Model
{
    public class Deployment
    {
        public Deployment()
        {
            this.RoleList = new List<Role>();
            this.RoleInstanceList = new List<RoleInstance>();
        }

        public string Name { get; set; }

        public string DeploymentSlot { get; set; }

        public string Url { get; set; }

        public string Status { get; set; }

        public string PrivateId { get; set; }

        public IList<Role> RoleList { get; set; }

        public IList<RoleInstance> RoleInstanceList { get; set; }
    }
}
