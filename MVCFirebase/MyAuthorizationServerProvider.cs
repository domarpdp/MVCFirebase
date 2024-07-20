using Microsoft.Owin.Security.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

namespace MVCFirebase
{

    //Class is used to verify oauthowin token 
    public class MyAuthorizationServerProvider :OAuthAuthorizationServerProvider
    {
        public override async Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            context.Validated();
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            var identity = new ClaimsIdentity(context.Options.AuthenticationType);
            if(context.UserName == "gnamaaitahb" && context.Password == "Rijul333#")
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, "admin"));
                identity.AddClaim(new Claim("username", "admin"));
                identity.AddClaim(new Claim(ClaimTypes.Name, "WEBAPIADMIN"));
                context.Validated(identity);
            }
            else if (context.UserName == "domarpdp" && context.Password == "Prabal1912#")
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, "user"));
                identity.AddClaim(new Claim("username", "user"));
                identity.AddClaim(new Claim(ClaimTypes.Name, "WEBAPIUSER"));
                context.Validated(identity);
            }
            else
            {
                context.SetError("Invalid grant", "Provided username and password is incorrect");
            }
        }

    }
}