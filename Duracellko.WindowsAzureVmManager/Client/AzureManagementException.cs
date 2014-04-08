using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Duracellko.WindowsAzureVmManager.Client
{
    [Serializable]
    public class AzureManagementException : Exception
    {
        public AzureManagementException(string code, string message)
            : base(message)
        {
            this.Code = code;
        }

        public AzureManagementException(string code, string message, Exception innerException)
            : base(message, innerException)
        {
            this.Code = code;
        }

        [SecuritySafeCritical]
        public AzureManagementException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.Code = info.GetString("Code");
        }

        public string Code { get; private set; }

        [SecurityCritical]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Code", this.Code);
        }
    }
}
