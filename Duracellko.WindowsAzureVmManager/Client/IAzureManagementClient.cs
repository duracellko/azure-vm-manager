using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Duracellko.WindowsAzureVmManager.Client
{
    public interface IAzureManagementClient
    {
        Task<AzureManagementResponse> GetResponseAsync(string path);

        Task<AzureManagementResponse> GetResponseAsync(string path, XDocument request, IDictionary<string, string> headers = null);

        Task<AzureManagementBinaryResponse> GetBinaryResponseAsync(string path, IDictionary<string, string> headers = null);
    }
}
