using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Duracellko.WindowsAzureVmManager.Client;
using Duracellko.WindowsAzureVmManager.Model;

namespace Duracellko.WindowsAzureVmManager.Manager
{
    public class VirtualMachinesManager : IWindowsAzureVmManager
    {
        #region Fields

        private const string VirtualMachinePattern = "^[a-zA-Z][-0-9a-zA-Z]*[0-9a-zA-Z]$";
        private const string CloudServicePattern = "^[0-9a-zA-Z][-0-9a-zA-Z]*$";

        private readonly VmManagerConfiguration configuration;
        private readonly IAzureManagementClient client;

        #endregion

        #region Constructor

        public VirtualMachinesManager(VmManagerConfiguration configuration, IAzureManagementClient client)
        {
            this.configuration = configuration;
            this.client = client;
        }

        #endregion

        #region IWindowsAzureVmManager

        public async Task<IEnumerable<VirtualMachineInfo>> GetVirtualMachines()
        {
            var getCloudServicesRequest = new GetCloudServicesRequest(this.client);
            var hostedServices = await getCloudServicesRequest.Submit(null);

            var getVirtualMachinesTasks = hostedServices.Services.Select(s => this.GetVirtualMachinesForServiceAsync(s)).ToArray();
            var virtualMachines = await Task.WhenAll(getVirtualMachinesTasks);
            return virtualMachines.Where(c => c != null).SelectMany(c => c).ToList();
        }

        public async Task<string> StartVirtualMachine(string name, string cloudServiceName)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }
            if (string.IsNullOrEmpty(cloudServiceName))
            {
                throw new ArgumentNullException("cloudServiceName");
            }
            if (!ValidateVirtualMachineName(name))
            {
                throw new ArgumentException(Properties.Resources.Error_InvalidVirtualMachineName, "name");
            }
            if (!ValidateCloudServiceName(cloudServiceName))
            {
                throw new ArgumentException(Properties.Resources.Error_InvalidCloudServiceName, "name");
            }

            var getProductionDeploymentRequest = new GetProductionDeploymentRequest(this.client);
            var deployment = await getProductionDeploymentRequest.Submit(cloudServiceName);

            var roleIdentifier = new RoleIdentifier(cloudServiceName, deployment.Name, name);
            var startRequest = new StartRoleRequest(this.client);
            return await startRequest.Submit(roleIdentifier);
        }

        public async Task<string> StopVirtualMachine(string name, string cloudServiceName)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }
            if (string.IsNullOrEmpty(cloudServiceName))
            {
                throw new ArgumentNullException("cloudServiceName");
            }
            if (!ValidateVirtualMachineName(name))
            {
                throw new ArgumentException(Properties.Resources.Error_InvalidVirtualMachineName, "name");
            }
            if (!ValidateCloudServiceName(cloudServiceName))
            {
                throw new ArgumentException(Properties.Resources.Error_InvalidCloudServiceName, "name");
            }

            var getProductionDeploymentRequest = new GetProductionDeploymentRequest(this.client);
            var deployment = await getProductionDeploymentRequest.Submit(cloudServiceName);

            var roleIdentifier = new RoleIdentifier(cloudServiceName, deployment.Name, name);
            var shutdownRequest = new ShutdownRoleRequest(this.client);
            return await shutdownRequest.Submit(roleIdentifier);
        }

        public async Task<BinaryContent> GetRemoteDesktopConnection(string name, string cloudServiceName)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }
            if (string.IsNullOrEmpty(cloudServiceName))
            {
                throw new ArgumentNullException("cloudServiceName");
            }
            if (!ValidateVirtualMachineName(name))
            {
                throw new ArgumentException(Properties.Resources.Error_InvalidVirtualMachineName, "name");
            }
            if (!ValidateCloudServiceName(cloudServiceName))
            {
                throw new ArgumentException(Properties.Resources.Error_InvalidCloudServiceName, "name");
            }

            var getProductionDeploymentRequest = new GetProductionDeploymentRequest(this.client);
            var deployment = await getProductionDeploymentRequest.Submit(cloudServiceName);

            var path = string.Format("services/hostedservices/{0}/deployments/{1}/roleinstances/{2}/ModelFile?FileType=RDP", cloudServiceName, deployment.Name, name);
            return await this.client.GetBinaryResponseAsync(path);
        }

        #endregion

        #region Private methods

        private async Task<IEnumerable<VirtualMachineInfo>> GetVirtualMachinesForServiceAsync(HostedService hostedService)
        {
            try
            {
                var getProductionDeploymentRequest = new GetProductionDeploymentRequest(this.client);
                var deployment = await getProductionDeploymentRequest.Submit(hostedService.ServiceName);
                if (deployment != null)
                {
                    return this.GetVirtualMachinesForDeployment(deployment, hostedService);
                }
            }
            catch (AzureManagementException ex)
            {
                if (!string.Equals(ex.Code, "ResourceNotFound", StringComparison.OrdinalIgnoreCase))
                {
                    throw;
                }

                // ignore if deployment was not found
            }

            return null;
        }

        private List<VirtualMachineInfo> GetVirtualMachinesForDeployment(Deployment deployment, HostedService cloudService)
        {
            var result = new List<VirtualMachineInfo>();

            foreach (var roleInstance in deployment.RoleInstanceList)
            {
                var role = deployment.RoleList.FirstOrDefault(r => string.Equals(r.RoleName, roleInstance.RoleName, StringComparison.OrdinalIgnoreCase));
                if (role != null && role.RoleType == "PersistentVMRole")
                {
                    var virtualMachineInfo = new VirtualMachineInfo()
                    {
                        Name = roleInstance.InstanceName,
                        CloudServiceName = cloudService.ServiceName,
                        Status = (VirtualMachineStatus)Enum.Parse(typeof(VirtualMachineStatus), roleInstance.InstanceStatus)
                    };
                    result.Add(virtualMachineInfo);
                }
            }

            return result;
        }

        private bool ValidateVirtualMachineName(string name)
        {
            if (name.Length < 3)
            {
                return false;
            }
            if (name.Length > 15)
            {
                return false;
            }

            return Regex.IsMatch(name, VirtualMachinePattern, RegexOptions.Singleline | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }

        private bool ValidateCloudServiceName(string name)
        {
            if (name.Length > 64)
            {
                return false;
            }

            return Regex.IsMatch(name, CloudServicePattern, RegexOptions.Singleline | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }

        #endregion
    }
}
