using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MVCFirebase.Models
{
    public class SMSPlansAPI
    {
        [Key]
        public int Id { get; set; }

        public decimal amount { get; set; }

        public decimal perSms { get; set; }

        public int smsCount { get; set; }

        public bool defaultSelect { get; set; }

        public string plan_id { get; set; }
         
    }
}