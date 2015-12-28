using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duracellko.WindowsAzureVmManager.Model
{
    public class Operation
    {
        public string RequestId { get; set; }

        public OperationStatus Status { get; set; }

        public string ErrorCode { get; set; }

        public string ErrorMessage { get; set; }
    }
}
