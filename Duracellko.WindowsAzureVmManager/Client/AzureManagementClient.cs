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

        public Task<XDocument> GetResponseAsync(string path)
        {
            return this.GetResponseAsync(path, null);
        }

        public async Task<XDocument> GetResponseAsync(string path, XDocument request)
        {
            var webRequest = RequestHelper.CreateRequest(this.SubscriptionId, path, this.certificate.Value);
            
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
                    if (webResponse.ContentLength == 0)
                    {
                        return null;
                    }

                    using (var responseStream = webResponse.GetResponseStream())
                    {
                        return XDocument.Load(responseStream);
                    }
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

        public async Task<BinaryContent> GetBinaryResponseAsync(string path)
        {
            var webRequest = RequestHelper.CreateRequest(this.SubscriptionId, path, this.certificate.Value);

            try
            {
                using (var webResponse = await webRequest.GetResponseAsync())
                {
                    BinaryContent result;
                    if (webResponse.ContentLength == 0)
                    {
                        result = new BinaryContent(emptyBytes);
                    }
                    else
                    {
                        using (var responseStream = webResponse.GetResponseStream())
                        {
                            using (var memoryStream = new MemoryStream())
                            {
                                await responseStream.CopyToAsync(memoryStream);
                                result = new BinaryContent(memoryStream.ToArray());
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
    }
}
