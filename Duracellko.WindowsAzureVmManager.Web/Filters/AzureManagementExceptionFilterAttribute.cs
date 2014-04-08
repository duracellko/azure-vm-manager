using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http.Filters;
using System.Xml.Linq;
using Duracellko.WindowsAzureVmManager.Client;

namespace Duracellko.WindowsAzureVmManager.Web.Filters
{
    public class AzureManagementExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            base.OnException(actionExecutedContext);

            var azureManagementException = actionExecutedContext.Exception as AzureManagementException;
            if (azureManagementException != null)
            {
                var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                response.ReasonPhrase = "AzureManagementError";
                response.Content = new ByteArrayContent(GetAzureManagementErrorXmlBytes(azureManagementException));
                response.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/xml; charset=UTF-8");
                actionExecutedContext.Response = response;
            }
        }

        private static byte[] GetAzureManagementErrorXmlBytes(AzureManagementException exception)
        {
            var xml = new XDocument(
                new XElement(
                    "Error",
                    new XElement("Code", exception.Code),
                    new XElement("Message", exception.Message)
                )
            );

            using (var stream = new MemoryStream())
            {
                xml.Save(stream);
                return stream.ToArray();
            }
        }
    }
}