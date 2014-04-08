using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Duracellko.WindowsAzureVmManager.Client;
using Duracellko.WindowsAzureVmManager.Manager;
using Duracellko.WindowsAzureVmManager.Web.Models;

namespace Duracellko.WindowsAzureVmManager.Web.Controllers
{
    [Authorize]
    public class VirtualMachinesController : Controller
    {
        // GET: /VirtualMachines/
        public ActionResult Index()
        {
            return View();
        }
	}
}