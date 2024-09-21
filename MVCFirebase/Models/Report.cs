using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Google.Cloud.Firestore;
using System.ComponentModel.DataAnnotations;

namespace MVCFirebase.Models
{
    [FirestoreData]
    public class Report
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
        [Display(Name = "Clinic Code")]
        [MaxLength(200)]
        public string clinicCode { get; set; }

        [FirestoreProperty]
        [Display(Name = "Patient Id")]
        [MaxLength(200)]
        public string patientId { get; set; }


        [FirestoreProperty]
        public DateTime timeStamp { get; set; }


        [FirestoreProperty]
        public DateTime updatedAt { get; set; }
    }
}