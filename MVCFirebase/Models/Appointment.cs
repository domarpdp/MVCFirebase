using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MVCFirebase.Models
{
    [FirestoreData]
    public class Appointment
    {
        [FirestoreProperty]
        [Required(ErrorMessage = "Required.")]
        public bool bill_sms { get; set; }

        [FirestoreProperty]
        [Required(ErrorMessage = "Required.")]
        public string clinic_id { get; set; }

        [FirestoreProperty]
        [Required(ErrorMessage = "Required.")]
        public DateTime complitionDate { get; set; }

        [FirestoreProperty]
        [Required(ErrorMessage = "Required.")]
        public string date { get; set; }

        [FirestoreProperty]
        [Required(ErrorMessage = "Required.")]
        public string days { get; set; }

        [FirestoreProperty]
        [Required(ErrorMessage = "Required.")]
        public string fee { get; set; }

        [FirestoreProperty]
        [Required(ErrorMessage = "Required.")]
        public string patient { get; set; }

        [FirestoreProperty]
        [Required(ErrorMessage = "Required.")]
        public string patient_id { get; set; }

        [FirestoreProperty]
        [Required(ErrorMessage = "Required.")]
        public DateTime raisedDate { get; set; }

        [FirestoreProperty]
        [Required(ErrorMessage = "Required.")]
        public bool reminder_sms { get; set; }

        [FirestoreProperty]
        [Required(ErrorMessage = "Required.")]
        public string severity { get; set; }

        [FirestoreProperty]
        [Required(ErrorMessage = "Required.")]
        public string status { get; set; }

        [FirestoreProperty]
        [Required(ErrorMessage = "Required.")]
        public DateTime timeStamp { get; set; }

        [FirestoreProperty]
        [Required(ErrorMessage = "Required.")]
        public string token { get; set; }

        [FirestoreProperty]
        [Required(ErrorMessage = "Required.")]
        public string patient_name { get; set; }

        [FirestoreProperty]
        [Required(ErrorMessage = "Required.")]
        public string clinic_name { get; set; }
        

        

        

        


    }
}