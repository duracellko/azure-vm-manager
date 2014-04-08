using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Duracellko.WindowsAzureVmManager.Web.Models
{
    public class CurrentUserViewModel
    {
        public bool IsAuthenticated { get; set; }

        public string Name { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }
    }
}