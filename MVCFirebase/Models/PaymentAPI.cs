using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MVCFirebase.Models
{
    public class PaymentAPI
    {

        [Key]
        public int Id { get; set; }
        public string clinicCode { get; set; }

        public string OrderId { get; set; }

        public string Status { get; set; }
        public string OrderType { get; set; }
        public string PaymentId { get; set; }

        public DateTime? CreationDate { get; set; }

        public decimal Amount { get; set; }

        public string plan_id { get; set; }
        

    }
}