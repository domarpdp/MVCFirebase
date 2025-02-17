using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Google.Cloud.Firestore;

namespace MVCFirebase.Models
{
    [FirestoreData]
    public class Prescription
    {

        [FirestoreProperty]
        public string id { get; set; }

        [FirestoreProperty]
        [Display(Name = "File")]
        [Required(ErrorMessage = "Required.")]
        [MaxLength(50)]
        public string file { get; set; }

        [FirestoreProperty]
        
        [Required(ErrorMessage = "Required.")]
        public string fileUrl { get; set; }

        [FirestoreProperty]
        [Display(Name = "Clinic Name")]
        [MaxLength(200)]
        public string clinic_name { get; set; }

        
        [FirestoreProperty]
        public DateTime timeStamp { get; set; }


        [FirestoreProperty]
        public DateTime updatedAt { get; set; }

        public string cashier { get; set; }
        public string chemist { get; set; }

        public string clinicCode { get; set; }

        public string clinicId { get; set; }

        public string date { get; set; }

        public string days { get; set; }

        public string fee { get; set; }

        public bool isCreated { get; set; }
        public bool isDeleted { get; set; }
        public string isPrescription { get; set; }

        public bool isSynced { get; set; }

        public string patientId { get; set; }

        public string doctor { get; set; }

        public string receptionist { get; set; }
    }
}