using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;

namespace MVCFirebase.Controllers
{
    public class DataController : ApiController
    {
        [AllowAnonymous]
        [HttpGet]
        [Route("api/Data/forall")]
        public IHttpActionResult Get()
        {
            return Ok("Now server time is :" + DateTime.Now.ToString());
        }

        [Authorize]
        [HttpGet]
        [Route("api/Data/authenticate")]
        public IHttpActionResult GetForAuthenticate()
        {
            var identity = (ClaimsIdentity)User.Identity;
            return Ok("Hello :" + identity.Name);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        [Route("api/Data/authorize")]
        public IHttpActionResult GetForAdmin()
        {
            var identity = (ClaimsIdentity)User.Identity;
            var roles = identity.Claims.Where(c => c.Type == ClaimTypes.Role).Select(a => a.Value);
            return Ok("Hello :" + identity.Name + " Role :" + string.Join(",",roles.ToList()));
        }

    }
}
