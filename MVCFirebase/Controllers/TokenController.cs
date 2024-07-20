using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using MVCFirebase.Models;

namespace MVCFirebase.Controllers
{
    public class TokenController : ApiController
    {
                

        [HttpPost]
        [Route("api/Token/PostJWTToken")]
        public string PostJWTToken([FromBody] AccountLoginModel accountloginmodel)
        {
            try
            {
                if(accountloginmodel.username == "" || accountloginmodel.username is null)
                {
                    return "username can not be blank";
                }
                if (accountloginmodel.password == "" || accountloginmodel.password is null)
                {
                    return "password can not be blank";
                }
                if(accountloginmodel.username == "domarpdp" && accountloginmodel.password == "Prabal1912#")
                {
                    var roles = new string[] { "user" };
                    var jwtSecurityToken = AuthenticateJWT.GenerateJWTToken(accountloginmodel.username, roles.ToList());
                    return jwtSecurityToken;
                }
                if (accountloginmodel.username == "gnamaaitahb" && accountloginmodel.password == "Rijul333#")
                {
                    var roles = new string[] { "admin" };
                    var jwtSecurityToken = AuthenticateJWT.GenerateJWTToken(accountloginmodel.username, roles.ToList());
                    return jwtSecurityToken;
                }
                else
                {
                    return "Invalid username or password";
                }

            }
            catch
            {
                return "Invalid username or password";
            }

        }
    }
}