using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duracellko.WindowsAzureVmManager
{
    public enum VirtualMachineStatus
    {
        RoleStateUnknown,
        CreatingVM,
        StartingVM,
        CreatingRole,
        StartingRole,
        ReadyRole,
        BusyRole,
        StoppingRole,
        StoppingVM,
        DeletingVM,
        StoppedVM,
        RestartingRole,
        CyclingRole,
        FailedStartingRole,
        FailedStartingVM,
        UnresponsiveRole,
        StoppedDeallocated
    }
}
