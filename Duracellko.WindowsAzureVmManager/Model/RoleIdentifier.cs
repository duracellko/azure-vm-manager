using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duracellko.WindowsAzureVmManager.Model
{
    public class RoleIdentifier
    {
        public RoleIdentifier(string cloudService, string deployment, string role)
        {
            if (string.IsNullOrEmpty(cloudService))
            {
                throw new ArgumentNullException("cloudService");
            }
            if (string.IsNullOrEmpty(deployment))
            {
                throw new ArgumentNullException("deployment");
            }
            if (string.IsNullOrEmpty(role))
            {
                throw new ArgumentNullException("role");
            }

            this.CloudService = cloudService;
            this.Deployment = deployment;
            this.Role = role;
        }

        public string CloudService { get; private set; }

        public string Deployment { get; private set; }

        public string Role { get; private set; }
    }
}
