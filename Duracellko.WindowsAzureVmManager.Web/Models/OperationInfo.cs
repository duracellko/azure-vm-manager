using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Duracellko.WindowsAzureVmManager.Model;

namespace Duracellko.WindowsAzureVmManager.Web.Models
{
    public class OperationInfo
    {
        public string RequestId { get; set; }

        public OperationStatus Status { get; set; }

        public string ErrorCode { get; set; }

        public string ErrorMessage { get; set; }
    }
}