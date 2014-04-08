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
        Task<XDocument> GetResponseAsync(string path);

        Task<XDocument> GetResponseAsync(string path, XDocument request);

        Task<BinaryContent> GetBinaryResponseAsync(string path);
    }
}
