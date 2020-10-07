using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MVCFirebase.Models
{
    [FirestoreData]
    public class Patient
    {
        [FirestoreProperty]
        public string id { get; set; }

        [FirestoreProperty]
        [Required(ErrorMessage = "Required.")]
        [MaxLength(50)]
        public string patient_name { get; set; }

        [FirestoreProperty]
        [Display(Name = "Mobile Number:")]
        [Required(ErrorMessage = "Required.")]
        [RegularExpression(@"^([0-9]{10})$", ErrorMessage = "Invalid Mobile Number.")]
        public string patient_mobile_number { get; set; }

        [FirestoreProperty]
        [MaxLength(200)]
        public string clinic_name { get; set; }

        [FirestoreProperty]
        public string patient_id { get; set; }

        [FirestoreProperty]
        [Required(ErrorMessage = "Required.")]
        [MaxLength(24)]
        public string city { get; set; }

        [FirestoreProperty]
        [Required(ErrorMessage = "Required.")]
        [Range(1, 99)]
        public string age { get; set; }

        [FirestoreProperty]
        [Required(ErrorMessage = "Required.")]
        [MaxLength(50)]
        public string care_of { get; set; }

        [FirestoreProperty]
        public DateTime creation_date { get; set; }

        [FirestoreProperty]
        public DateTime appointment_date { get; set; }

        [FirestoreProperty]
        [Range(0, int.MaxValue, ErrorMessage = "Please enter valid integer Number")]
        [MaxLength(3)]
        public string tokenNumber { get; set; }

        [FirestoreProperty]
        [Required(ErrorMessage = "Required.")]
        [MaxLength(50)]
        public string disease { get; set; }

        [FirestoreProperty]
        [Required(ErrorMessage = "Required.")]
        [MaxLength(6)]
        public string gender { get; set; }

        [FirestoreProperty]
        [Required(ErrorMessage = "Required.")]
        [MaxLength(6)]
        public string severity { get; set; }

        [FirestoreProperty]
        [Required(ErrorMessage = "Required.")]
        [MaxLength(50)]
        public string refer_by { get; set; }

        [FirestoreProperty]
        [Required(ErrorMessage = "Required.")]
        [MaxLength(50)]
        public string refer_to_doctor { get; set; }

        [FirestoreProperty]
        [MaxLength(200)]
        public string search_text { get; set; }

        [FirestoreProperty]
        
        public string createAppointment { get; set; }


    }
}