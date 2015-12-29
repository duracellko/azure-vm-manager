using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Duracellko.WindowsAzureVmManager.Manager;

namespace Duracellko.WindowsAzureVmManager.Client
{
    public class AzureManagementClient : IAzureManagementClient
    {
        #region Fields

        private static readonly XNamespace WindowsAzureNamespace = XNamespace.Get("http://schemas.microsoft.com/windowsazure");
        private static readonly byte[] emptyBytes = new byte[0];
        private readonly VmManagerConfiguration configuration;
        private readonly Lazy<X509Certificate2> certificate;

        #endregion

        #region Constructor

        public AzureManagementClient(VmManagerConfiguration configuration)
        {
            this.configuration = configuration;
            this.SubscriptionId = configuration.SubscriptionId;
            this.certificate = new Lazy<X509Certificate2>(() => RequestHelper.LoadCertificate(this.configuration.CertificateData, this.configuration.CertificatePassword));
        }

        #endregion

        #region Properties

        public string SubscriptionId { get; private set; }

        #endregion

        #region IAzureManagementClient

        public Task<AzureManagementResponse> GetResponseAsync(string path)
        {
            return this.GetResponseAsync(path, null, null);
        }

        public async Task<AzureManagementResponse> GetResponseAsync(string path, XDocument request, IDictionary<string, string> headers = null)
        {
            var webRequest = RequestHelper.CreateRequest(this.SubscriptionId, path, this.certificate.Value, headers);
            
            if (request != null)
            {
                webRequest.Method = "POST";
                using (var requestStream = webRequest.GetRequestStream())
                {
                    request.Save(requestStream);
                }
            }

            try
            {
                using (var webResponse = await webRequest.GetResponseAsync())
                {
                    XDocument body = null;
                    var responseHeaders = ConvertHeaders(webResponse);
                    if (webResponse.ContentLength != 0)
                    {
                        using (var responseStream = webResponse.GetResponseStream())
                        {
                            body = XDocument.Load(responseStream);
                        }
                    }

                    return new AzureManagementResponse(body, responseHeaders);
                }
            }
            catch (WebException ex)
            {
                if (ex.Response == null)
                {
                    throw;
                }

                using (var responseStream = ex.Response.GetResponseStream())
                {
                    var responseXml = XDocument.Load(responseStream);
                    var code = (string)responseXml.Root.Element(WindowsAzureNamespace + "Code");
                    var message = (string)responseXml.Root.Element(WindowsAzureNamespace + "Message");
                    throw new AzureManagementException(code, message, ex);
                }
            }
        }

        public async Task<AzureManagementBinaryResponse> GetBinaryResponseAsync(string path, IDictionary<string, string> headers = null)
        {
            var webRequest = RequestHelper.CreateRequest(this.SubscriptionId, path, this.certificate.Value, headers);

            try
            {
                using (var webResponse = await webRequest.GetResponseAsync())
                {
                    AzureManagementBinaryResponse result;
                    var responseHeaders = ConvertHeaders(webResponse);
                    if (webResponse.ContentLength == 0)
                    {
                        result = new AzureManagementBinaryResponse(emptyBytes, responseHeaders);
                    }
                    else
                    {
                        using (var responseStream = webResponse.GetResponseStream())
                        {
                            using (var memoryStream = new MemoryStream())
                            {
                                await responseStream.CopyToAsync(memoryStream);
                                result = new AzureManagementBinaryResponse(memoryStream.ToArray(), responseHeaders);
                            }
                        }
                    }

                    result.ContentType = webResponse.ContentType;
                    return result;
                }
            }
            catch (WebException ex)
            {
                if (ex.Response == null)
                {
                    throw;
                }

                using (var responseStream = ex.Response.GetResponseStream())
                {
                    var responseXml = XDocument.Load(responseStream);
                    var code = (string)responseXml.Root.Element(WindowsAzureNamespace + "Code");
                    var message = (string)responseXml.Root.Element(WindowsAzureNamespace + "Message");
                    throw new AzureManagementException(code, message, ex);
                }
            }
        }

        #endregion

        #region Private methods

        private static IDictionary<string, string> ConvertHeaders(WebResponse webResponse)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (string header in webResponse.Headers)
            {
                result[header] = webResponse.Headers[header];
            }

            return result;
        }

        #endregion
    }
}
