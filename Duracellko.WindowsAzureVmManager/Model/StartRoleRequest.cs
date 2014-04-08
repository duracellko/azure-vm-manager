using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Duracellko.WindowsAzureVmManager.Client;

namespace Duracellko.WindowsAzureVmManager.Model
{
    public class StartRoleRequest : AzureManagementRequestBase<RoleIdentifier, object>
    {
        public StartRoleRequest(IAzureManagementClient client)
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
                new XElement(WindowsAzureNamespace + "StartRoleOperation",
                    new XElement(WindowsAzureNamespace + "OperationType", "StartRoleOperation")
                )
            );
        }

        protected override object DeserializeResponse(XDocument xml)
        {
            return null;
        }
    }
}
