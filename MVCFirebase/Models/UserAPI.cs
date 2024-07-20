using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MVCFirebase.Models
{
    public class UserAPI
    {
        [Key]
        public int UserId { get; set; }
        public string documentId { get; set; }

        public string clinicid { get; set; }

        public string clinicCode { get; set; }

        public string email { get; set; }

        public string name { get; set; }
        public string password { get; set; }
        public string mobile_number { get; set; }
        public Dictionary<int, string> user_roles { get; set; }
        public string id { get; set; }//having clinicCode data
        public string user_qualification { get; set; }
        public string idproof { get; set; }

        public string signature { get; set; }

        public int? stats_enable { get; set; }

        public DateTime? creation_date { get; set; }

        public int? user_deactivated { get; set; }

        public string GetUserRolesAsJsonArrayString()
        {
            var roles = user_roles.Values.Select(role => $"\"{role}\"");
            return $"[{string.Join(",", roles)}]";
        }
    }

}