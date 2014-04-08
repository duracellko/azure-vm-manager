using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Mvc;
using Autofac;
using Autofac.Integration.Mvc;
using Autofac.Integration.WebApi;
using Duracellko.WindowsAzureVmManager.Client;
using Duracellko.WindowsAzureVmManager.Manager;
using Duracellko.WindowsAzureVmManager.Web.Controllers;

namespace Duracellko.WindowsAzureVmManager.Web
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: new { action = "Get", id = RouteParameter.Optional }
            );

            BuildContainer(config);
        }

        private static void BuildContainer(HttpConfiguration config)
        {
            var builder = new ContainerBuilder();
            Autofac.Integration.Mvc.RegistrationExtensions.RegisterControllers(builder, typeof(Global).Assembly);

            builder.RegisterType<VirtualMachinesManager>().As<IWindowsAzureVmManager>().InstancePerLifetimeScope();
            builder.RegisterType<AzureManagementClient>().As<IAzureManagementClient>();
            builder.RegisterType<VmManagerConfiguration>().AsSelf().SingleInstance();

            builder.RegisterType<VmManagementController>().AsSelf();

            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
            config.DependencyResolver = new AutofacWebApiDependencyResolver(container);
        }
    }
}
