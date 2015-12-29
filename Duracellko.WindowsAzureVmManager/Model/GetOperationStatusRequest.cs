using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Duracellko.WindowsAzureVmManager.Client;

namespace Duracellko.WindowsAzureVmManager.Model
{
    public class GetOperationStatusRequest : AzureManagementRequestBase<string, Operation>
    {
        public GetOperationStatusRequest(IAzureManagementClient client) : base(client)
        {
        }

        protected override string GetPath(string request)
        {
            if (string.IsNullOrEmpty(request))
            {
                throw new ArgumentNullException(nameof(request));
            }

            return "operations/" + request;
        }

        protected override Operation DeserializeResponse(AzureManagementResponse response)
        {
            var result = new Operation();
            var xml = response.Body;

            var operationElement = xml.Root;
            result.RequestId = (string)operationElement.Element(WindowsAzureNamespace + "ID");

            var status = (string)operationElement.Element(WindowsAzureNamespace + "Status");
            if (string.Equals(status, "Succeeded", StringComparison.OrdinalIgnoreCase))
            {
                result.Status = OperationStatus.Succeeded;
            }
            else if (string.Equals(status, "Failed", StringComparison.OrdinalIgnoreCase))
            {
                result.Status = OperationStatus.Failed;
            }
            else
            {
                result.Status = OperationStatus.InProgress;
            }

            var errorElement = operationElement.Element(WindowsAzureNamespace + "Error");
            if (errorElement != null)
            {
                result.ErrorCode = (string)errorElement.Element(WindowsAzureNamespace + "Code");
                result.ErrorMessage = (string)errorElement.Element(WindowsAzureNamespace + "Message");
            }

            return result;
        }
    }
}
