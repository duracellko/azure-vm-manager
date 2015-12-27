using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Duracellko.WindowsAzureVmManager.Client
{
    public class AzureManagementResponse
    {
        public AzureManagementResponse(XDocument body) : this(body, null)
        {
        }

        public AzureManagementResponse(XDocument body, IDictionary<string, string> headers)
        {
            this.Body = body;
            this.Headers = headers;
        }

        public XDocument Body { get; private set; }

        public IDictionary<string, string> Headers { get; private set; }
    }
}
