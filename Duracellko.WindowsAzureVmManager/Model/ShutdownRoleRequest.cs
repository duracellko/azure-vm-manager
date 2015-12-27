using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Duracellko.WindowsAzureVmManager.Client;

namespace Duracellko.WindowsAzureVmManager.Model
{
    public class ShutdownRoleRequest : AzureManagementRequestBase<RoleIdentifier, object>
    {
        public ShutdownRoleRequest(IAzureManagementClient client)
            : base(client)
        {
        }

        protected override string GetPath(RoleIdentifier request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            return string.Format("services/hostedservices/{0}/deployments/{1}/roleinstances/{2}/Operations", request.CloudService, request.Deployment, request.Role);
        }

        protected override XDocument SerializeRequest(RoleIdentifier request)
        {
            return new XDocument(
                new XElement(WindowsAzureNamespace + "ShutdownRoleOperation",
                    new XElement(WindowsAzureNamespace + "OperationType", "ShutdownRoleOperation"),
                    new XElement(WindowsAzureNamespace + "PostShutdownAction", "StoppedDeallocated")
                )
            );
        }

        protected override object DeserializeResponse(AzureManagementResponse response)
        {
            return null;
        }
    }
}
