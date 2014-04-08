using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duracellko.WindowsAzureVmManager
{
    public class BinaryContent
    {
        public BinaryContent(byte[] content)
        {
            this.Content = content;
        }

        public byte[] Content { get; private set; }

        public string ContentType { get; set; }
    }
}
