using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.Owin.Extensions;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ADFSWebForms2
{
    public partial class Startup
    {
        private static string clientId = ConfigurationManager.AppSettings["ida:ClientId"];
        // private static string aadInstance = EnsureTrailingSlash(ConfigurationManager.AppSettings["ida:AADInstance"]);
        // private static string tenantId = ConfigurationManager.AppSettings["ida:TenantId"];
        private static string metadataAddress = ConfigurationManager.AppSettings["ida:ADFSDiscoveryDoc"];
        private static string redirectUri = ConfigurationManager.AppSettings["ida:RedirectUri"];
        private static string postLogoutRedirectUri = ConfigurationManager.AppSettings["ida:PostLogoutRedirectUri"];

        //string authority = aadInstance + tenantId;

        public void ConfigureAuth(IAppBuilder app)
        {
            //Needed to ignore CA root certificate errors when pulling metadata from AD FS
            ServicePointManager
            .ServerCertificateValidationCallback +=
            (sender, cert, chain, sslPolicyErrors) => true;

            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            app.UseCookieAuthentication(new CookieAuthenticationOptions());

            app.UseOpenIdConnectAuthentication(
          new OpenIdConnectAuthenticationOptions
          {
              ClientId = clientId,
              //Authority = authority,
              MetadataAddress = metadataAddress,
              PostLogoutRedirectUri = postLogoutRedirectUri,
              RedirectUri = redirectUri,
              Notifications = new OpenIdConnectAuthenticationNotifications()
              {
                  AuthenticationFailed = (context) =>
                  {
                      return System.Threading.Tasks.Task.FromResult(0);
                  },
                  SecurityTokenValidated = (context) =>
                  {
                      var claims = context.AuthenticationTicket.Identity.Claims;
                      var groups = from c in claims
                                   where c.Type == "groups"
                                   select c;

                      foreach (var group in groups)
                      {
                          context.AuthenticationTicket.Identity.AddClaim(new Claim(ClaimTypes.Role, group.Value));
                      }

                      context.AuthenticationTicket.Identity.AddClaim(new Claim("id_token", context.ProtocolMessage.IdToken));

                      return Task.FromResult(0);
                  },
                  RedirectToIdentityProvider = (context) =>
                  {
                      if (context.ProtocolMessage.RequestType == OpenIdConnectRequestType.Logout)
                      {
                          var idTokenHint = context.OwinContext.Authentication.User.FindFirst("id_token").Value;
                          context.ProtocolMessage.IdTokenHint = idTokenHint;
                      }
                      return Task.FromResult(0);
                  }
              }
          });

            // This makes any middleware defined above this line run before the Authorization rule is applied in web.config
            app.UseStageMarker(PipelineStage.Authenticate);
        }

        private static string EnsureTrailingSlash(string value)
        {
            if (value == null)
            {
                value = string.Empty;
            }

            if (!value.EndsWith("/", StringComparison.Ordinal))
            {
                return value + "/";
            }

            return value;
        }
    }
}