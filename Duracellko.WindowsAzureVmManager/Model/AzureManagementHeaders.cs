using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duracellko.WindowsAzureVmManager.Model
{
    public static class AzureManagementHeaders
    {
        public const string RequestId = "x-ms-request-id";

        public static string GetHeaderValue(this IDictionary<string, string> headers, string name)
        {
            if (headers == null)
            {
                throw new ArgumentNullException(nameof(headers));
            }
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            string value = null;
            if (headers.TryGetValue(name, out value))
            {
                return value;
            }
            else
            {
                return null;
            }
        }
    }
}
