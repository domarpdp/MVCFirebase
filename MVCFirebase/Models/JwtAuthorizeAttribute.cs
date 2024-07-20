using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Web;
using System.Web.Http.Controllers;

namespace MVCFirebase.Models
{
    public class JwtAuthorizeAttribute : System.Web.Http.AuthorizeAttribute
    {
        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            if (actionContext == null)
            {
                throw new ArgumentNullException(nameof(actionContext));
            }

            var principal = GetPrincipal(actionContext.Request);
            if (principal != null)
            {
                var roles = Roles.Split(',');
                if (roles.Any() && !roles.Any(role => principal.IsInRole(role)))
                {
                    return false;
                }

                return true;
            }

            return false;
        }

        private ClaimsPrincipal GetPrincipal(HttpRequestMessage request)
        {
            var token = GetJwtTokenFromHeader(request.Headers.Authorization);
            if (string.IsNullOrEmpty(token))
            {
                return null;
            }

            var secretKey = "UHJTFRTYUY787FVGHMJYAERvlkuytnbf";

            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
            };

            try
            {
                SecurityToken validatedToken;
                var principal = tokenHandler.ValidateToken(token, validationParameters, out validatedToken);
                return principal;
            }
            catch
            {
                return null;
            }
        }

        private string GetJwtTokenFromHeader(System.Net.Http.Headers.AuthenticationHeaderValue authHeader)
        {
            if (authHeader?.Scheme != "bearer")
            {
                return null;
            }

            return authHeader.Parameter;
        }
    }
}