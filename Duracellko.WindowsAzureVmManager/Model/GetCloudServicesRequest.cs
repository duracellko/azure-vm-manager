using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Duracellko.WindowsAzureVmManager.Client;

namespace Duracellko.WindowsAzureVmManager.Model
{
    public class GetCloudServicesRequest : AzureManagementRequestBase<object, HostedServices>
    {
        public GetCloudServicesRequest(IAzureManagementClient client)
            : base(client)
        {
        }

        protected override string GetPath(object request)
        {
            return "services/hostedservices";
        }

        protected override HostedServices DeserializeResponse(AzureManagementResponse response)
        {
            var result = new HostedServices();
            var xml = response.Body;

            foreach (var hostedServiceElement in xml.Root.Elements(WindowsAzureNamespace + "HostedService"))
            {
                var hostedService = new HostedService()
                {
                    ServiceName = (string)hostedServiceElement.Element(WindowsAzureNamespace + "ServiceName"),
                    Url = (string)hostedServiceElement.Element(WindowsAzureNamespace + "Url"),
                    Status = (string)hostedServiceElement.Element(WindowsAzureNamespace + "HostedServiceProperties")
                        .Element(WindowsAzureNamespace + "Status")
                };
                result.Services.Add(hostedService);
            }

            return result;
        }
    }
}
