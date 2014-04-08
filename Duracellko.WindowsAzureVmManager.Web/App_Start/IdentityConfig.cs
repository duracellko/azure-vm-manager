using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using Duracellko.WindowsAzureVmManager.Identity;

namespace Duracellko.WindowsAzureVmManager.Web.App_Start
{
    public class IdentityConfig
    {
        public static void ConfigureIdentity()
        {
            string metadataLocation = ConfigurationManager.AppSettings["ida:FederationMetadataLocation"];
            CacheIssuerNameRegistry.MetadataLocation = metadataLocation;
        }
    }
}