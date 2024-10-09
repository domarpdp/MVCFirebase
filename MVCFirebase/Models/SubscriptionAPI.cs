using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MVCFirebase.Models
{
    public class SubscriptionAPI
    {
        [Key]
        public int Id { get; set; }

        public string plan_id { get; set; }

        public string plan_name { get; set; }

        public decimal plan_price { get; set; }

        public decimal monthly_price { get; set; }

        public decimal annual_subscription { get; set; }

        public string plan_subTitle { get; set; }

        public string plan_title { get; set; }

        public bool showPlan { get; set; }

        public int total_users { get; set; }

        public decimal plan_discount_price { get; set; }

        public List<string> plan_benefits { get; set; }

        public List<string> plan_other_benefits { get; set; }

        public string plan_offer_id { get; set; }

        public string order { get; set; }

        public DateTime? offer_end_date { get; set; }

    }
}