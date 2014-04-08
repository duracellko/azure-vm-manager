using System;
using System.Collections.Generic;
using System.IdentityModel.Services;
using System.IdentityModel.Services.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using Duracellko.WindowsAzureVmManager.Web.Models;

namespace Duracellko.WindowsAzureVmManager.Web.Controllers
{
    public class LoginController : Controller
    {
        private const string FirstNameClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname";
        private const string LastNameClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname";

        // GET: /Login/
        [Authorize]
        public ActionResult Index()
        {
            return RedirectToAction("Index", "Home");
        }

        public ActionResult CurrentUser()
        {
            var claimsPrincipal = ClaimsPrincipal.Current;
            var currentUser = new CurrentUserViewModel()
            {
                IsAuthenticated = claimsPrincipal != null && claimsPrincipal.Identity != null && claimsPrincipal.Identity.IsAuthenticated
            };
            
            if (currentUser.IsAuthenticated)
            {
                currentUser.Name = claimsPrincipal.Identity.Name;

                currentUser.FirstName = claimsPrincipal.Claims.Where(c => c.Type == FirstNameClaimType).Select(c => c.Value).FirstOrDefault();
                currentUser.LastName = claimsPrincipal.Claims.Where(c => c.Type == LastNameClaimType).Select(c => c.Value).FirstOrDefault();
            }

            return PartialView(currentUser);
        }

        public ActionResult SignOut()
        {
            WsFederationConfiguration config = FederatedAuthentication.FederationConfiguration.WsFederationConfiguration;

            // Redirect to SignOutCallback after signing out.
            string callbackUrl = Url.Action("Index", "Home", routeValues: null, protocol: Request.Url.Scheme);
            SignOutRequestMessage signoutMessage = new SignOutRequestMessage(new Uri(config.Issuer), callbackUrl);
            signoutMessage.SetParameter("wtrealm", config.Realm);
            FederatedAuthentication.SessionAuthenticationModule.SignOut();

            return new RedirectResult(signoutMessage.WriteQueryString());
        }
	}
}