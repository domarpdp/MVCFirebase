using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVCFirebase.Models
{
    public class GenericAPIResult
    {
        public string message { get; set; }

        public string statusCode { get; set; }
        public string error { get; set; }
        public List<dynamic> data { get; set; }
    }
}