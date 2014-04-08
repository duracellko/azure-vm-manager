using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duracellko.WindowsAzureVmManager
{
    public interface IWindowsAzureVmManager
    {
        Task<IEnumerable<VirtualMachineInfo>> GetVirtualMachines();

        Task<bool> StartVirtualMachine(string name, string cloudServiceName);

        Task<bool> StopVirtualMachine(string name, string cloudServiceName);

        Task<BinaryContent> GetRemoteDesktopConnection(string name, string cloudServiceName);
    }
}
