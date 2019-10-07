using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.Owin.Security.OpenIdConnect;
using Microsoft.Owin.Security.Cookies;

namespace ADFSWebForms2
{
    public partial class _Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Request.IsAuthenticated)
            {
                HttpContext.Current.GetOwinContext().Authentication.Challenge(
                    new AuthenticationProperties { RedirectUri = "https://localhost:44300/About" },
                    OpenIdConnectAuthenticationDefaults.AuthenticationType);
            }
        }

        protected void LogoutBtn_Click(object sender, EventArgs e)
        {
            HttpContext.Current.GetOwinContext().Authentication.SignOut(
             CookieAuthenticationDefaults.AuthenticationType,
                 OpenIdConnectAuthenticationDefaults.AuthenticationType

            );

        }
    }
}