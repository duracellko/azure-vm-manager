using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Duracellko.WindowsAzureVmManager.Client
{
    internal class RequestHelper
    {
        private const string AzureManagementUrlPattern = "https://management.core.windows.net/{0}/{1}";
        private const string AzureManagementVersion = "2013-11-01";

        public static WebRequest CreateRequest(string subscriptionId, string path, X509Certificate2 certificate, IDictionary<string, string> headers)
        {
            var requestUrl = string.Format(AzureManagementUrlPattern, subscriptionId, path);
            var request = WebRequest.CreateHttp(requestUrl);
            request.Headers.Add("x-ms-version", AzureManagementVersion);
            request.ContentType = "application/xml";

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }

            request.ClientCertificates.Add(certificate);

            return request;
        }

        public static X509Certificate2 LoadCertificate(byte[] certificateBytes, SecureString password)
        {
            return new X509Certificate2(certificateBytes, password, X509KeyStorageFlags.MachineKeySet);
        }
    }
}
