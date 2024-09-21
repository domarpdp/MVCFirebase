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

    }
}