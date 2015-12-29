using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Duracellko.WindowsAzureVmManager.Client;

namespace Duracellko.WindowsAzureVmManager.Model
{
    public class GetProductionDeploymentRequest : AzureManagementRequestBase<string, Deployment>
    {
        public GetProductionDeploymentRequest(IAzureManagementClient client)
            : base(client)
        {
        }

        protected override string GetPath(string request)
        {
            return string.Format("services/hostedservices/{0}/deploymentslots/production", request);
        }

        protected override Deployment DeserializeResponse(AzureManagementResponse response)
        {
            var xml = response.Body;
            var result = new Deployment()
            {
                Name = (string)xml.Root.Element(WindowsAzureNamespace + "Name"),
                DeploymentSlot = (string)xml.Root.Element(WindowsAzureNamespace + "DeploymentSlot"),
                Url = (string)xml.Root.Element(WindowsAzureNamespace + "Url"),
                Status = (string)xml.Root.Element(WindowsAzureNamespace + "Status"),
                PrivateId = (string)xml.Root.Element(WindowsAzureNamespace + "PrivateID")
            };

            foreach (var roleElement in xml.Root.Element(WindowsAzureNamespace + "RoleList").Elements(WindowsAzureNamespace + "Role"))
            {
                result.RoleList.Add(this.DeserializeRole(roleElement));
            }

            foreach (var roleInstanceElement in xml.Root.Element(WindowsAzureNamespace + "RoleInstanceList").Elements(WindowsAzureNamespace + "RoleInstance"))
            {
                result.RoleInstanceList.Add(this.DeserializeRoleInstance(roleInstanceElement));
            }

            return result;
        }

        private Role DeserializeRole(XElement element)
        {
            return new Role()
            {
                RoleName = (string)element.Element(WindowsAzureNamespace + "RoleName"),
                RoleType = (string)element.Element(WindowsAzureNamespace + "RoleType"),
                RoleSize = (string)element.Element(WindowsAzureNamespace + "RoleSize")
            };
        }

        private RoleInstance DeserializeRoleInstance(XElement element)
        {
            return new RoleInstance()
            {
                RoleName = (string)element.Element(WindowsAzureNamespace + "RoleName"),
                InstanceName = (string)element.Element(WindowsAzureNamespace + "InstanceName"),
                InstanceStatus = (string)element.Element(WindowsAzureNamespace + "InstanceStatus"),
                PowerState = (string)element.Element(WindowsAzureNamespace + "PowerState")
            };
        }
    }
}
