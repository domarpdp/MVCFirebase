using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MVCFirebase.Models
{
    public class Medicine
    {
        [Display(Name = "Medicine Name")]
        public int serialno { get; set; }

        [Display(Name = "Medicine Name")]
        public string medicinename { get; set; }

        [Display(Name = "Quantity")]
        public string Quantity { get; set; }

        
    }
}