using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MVCFirebase.Models
{
    public class PrescriptionAPI
    {

        [Key]
        public int Id { get; set; }
        public string documentId { get; set; }
        public string receptionist { get; set; }
        public string chemist { get; set; }

        public string cashier { get; set; }

        public string doctor { get; set; }
        public string days { get; set; }

        public string fee { get; set; }
        public string date { get; set; }

        public int? isCreated { get; set; }

        public int? isSynced { get; set; }
        public int? isDeleted { get; set; }

        public int? isPrescription { get; set; }

        public string file { get; set; }
        public string fileUrl { get; set; }

        public string clinicId { get; set; }

        public string clinicCode { get; set; }

        public DateTime? updatedAt { get; set; }

        public DateTime? timeStamp { get; set; }

        //public long? timeStampLong { get; set; }

        public string patientId { get; set; }

        
    }
}