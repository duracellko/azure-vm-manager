using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Duracellko.WindowsAzureVmManager.Model;

namespace Duracellko.WindowsAzureVmManager
{
    public interface IWindowsAzureVmManager
    {
        Task<IEnumerable<VirtualMachineInfo>> GetVirtualMachines();

        Task<Operation> GetOperationStatus(string requestId);

        Task<string> StartVirtualMachine(string name, string cloudServiceName);

        Task<string> StopVirtualMachine(string name, string cloudServiceName);

        Task<BinaryContent> GetRemoteDesktopConnection(string name, string cloudServiceName);
    }
}
