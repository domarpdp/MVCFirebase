using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MVCFirebase.Models
{
    public class AppointmentAPI
    {
        [Key]
        public int Id { get; set; }
        public string documentId { get; set; }
        public string days { get; set; }

        public string fee { get; set; }
        public string date { get; set; }

        public string status { get; set; }

        public string statusCashier { get; set; }

        public string statusChemist { get; set; }

        public string severity { get; set; }
        
        public string patient_id { get; set; }

        public string clinic_id { get; set; }

        public string clinicCode { get; set; }

        public string patient { get; set; }

        public int? bill_sms { get; set; }

        public int? reminder_sms { get; set; }

        public DateTime? timeStamp { get; set; }

        
        public DateTime? updatedAt { get; set; }

        public DateTime? completionDate { get; set; }

        public DateTime? completiondateCashier { get; set; }

        public DateTime? completiondateChemist { get; set; }

        public DateTime raisedDate { get; set; }

        public string token { get; set; }

        public string referTo { get; set; }

        public string medicineFee { get; set; }

        public string medicineCost { get; set; }

        public string modeofpayment { get; set; }

        public string modeofpaymentChemist { get; set; }

        public int? isCreated { get; set; }

        public int? isSynced { get; set; }

        public string createdBy { get; set; }
        public string receptionist { get; set; }

        public string chemist { get; set; }

        public string cashier { get; set; }

        public string doctor { get; set; }

        public int? request_by_patient { get; set; }

    }
}