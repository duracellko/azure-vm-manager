using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duracellko.WindowsAzureVmManager.Client
{
    public class AzureManagementBinaryResponse : BinaryContent
    {
        public AzureManagementBinaryResponse(byte[] content, IDictionary<string, string> headers) : base(content)
        {
            this.Headers = headers;
        }

        public IDictionary<string, string> Headers { get; private set; }

    }
}
