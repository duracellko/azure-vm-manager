using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using Duracellko.WindowsAzureVmManager.Web.Filters;
using Duracellko.WindowsAzureVmManager.Web.Models;

namespace Duracellko.WindowsAzureVmManager.Web.Controllers
{
    [Authorize]
    [AzureManagementExceptionFilterAttribute]
    public class VmManagementController : ApiController
    {
        private readonly IWindowsAzureVmManager manager;

        public VmManagementController(IWindowsAzureVmManager manager)
        {
            this.manager = manager;
        }

        [HttpGet]
        [ActionName("VirtualMachines")]
        public async Task<IEnumerable<VirtualMachine>> GetVirtualMachines()
        {
            var virtualMachines = await this.manager.GetVirtualMachines();
            return virtualMachines.Select(vm => new VirtualMachine()
                    {
                        Name = vm.Name,
                        CloudServiceName = vm.CloudServiceName,
                        Status = vm.Status.ToString()
                    }
                ).OrderBy(vm => vm.Name, StringComparer.CurrentCultureIgnoreCase).ToList();
        }

        [HttpGet]
        [ActionName("Operation")]
        public async Task<OperationInfo> GetOperationInfo(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id));
            }
            var operation = await this.manager.GetOperationStatus(id);
            var result = new OperationInfo()
            {
                RequestId = operation.RequestId,
                Status = operation.Status,
                ErrorCode = operation.ErrorCode,
                ErrorMessage = operation.ErrorMessage
            };

            return result;
        }

        [HttpGet]
        public async Task<string> StartVM(string id, string cloudService)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }
            if (string.IsNullOrEmpty(cloudService))
            {
                throw new ArgumentNullException("cloudService");
            }

            return await this.manager.StartVirtualMachine(id, cloudService);
        }

        [HttpGet]
        public async Task<string> ShutdownVM(string id, string cloudService)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }
            if (string.IsNullOrEmpty(cloudService))
            {
                throw new ArgumentNullException("cloudService");
            }

            return await this.manager.StopVirtualMachine(id, cloudService);
        }

        [HttpGet]
        public async Task<IHttpActionResult> RemoteDesktop(string id, string cloudService)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }
            if (string.IsNullOrEmpty(cloudService))
            {
                throw new ArgumentNullException("cloudService");
            }

            var content = await this.manager.GetRemoteDesktopConnection(id, cloudService);

            var result = new HttpResponseMessage();
            result.Content = new ByteArrayContent(content.Content);
            result.Content.Headers.ContentType = new MediaTypeHeaderValue(content.ContentType);
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = id + ".rdp"
            };
            return this.ResponseMessage(result);
        }
    }
}