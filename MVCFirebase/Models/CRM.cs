using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MVCFirebase.Models
{
    [FirestoreData]
    public class CRM
    {
        
        [FirestoreProperty]
        [Display(Name = "Refered By:")]
        [Required(ErrorMessage = "Required.")]
        public string referedby { get; set; }

        [FirestoreProperty]
        [Display(Name = "Clinic Type:")]
        public string clinictype { get; set; }

        [FirestoreProperty]
        [Display(Name = "Add 1:")]
        public string address1 { get; set; }

        [FirestoreProperty]
        [Display(Name = "Add 2:")]
        public string address2 { get; set; }

        [FirestoreProperty]
        [Display(Name = "Add 3:")]
        public string address3 { get; set; }

        [FirestoreProperty]
        [Display(Name = "City:")]
        [Required(ErrorMessage = "Required.")]
        public string city { get; set; }
        [FirestoreProperty]
        [Display(Name = "State:")]
        [Required(ErrorMessage = "Required.")]
        public string state { get; set; }

        [FirestoreProperty]
        [Display(Name = "PIN Code:")]
        public string pin { get; set; }

        [FirestoreProperty]
        public string country { get; set; }

        [FirestoreProperty]
        [Display(Name = "Clinic Name:")]
        public string clinicname { get; set; }

        [FirestoreProperty]
        [Display(Name = "Doctor Name:")]
        [Required(ErrorMessage = "Required.")]
        public string doctorname { get; set; }

        [FirestoreProperty]
        [Display(Name = "Mobile:")]
        [Required(ErrorMessage = "Required.")]
        public string contact { get; set; }


        [FirestoreProperty]
        [Display(Name = "Comments:")]
        public string comments { get; set; }

        [FirestoreProperty]
        [Display(Name = "Status:")]
        public string status { get; set; }

        [FirestoreProperty]
        [Display(Name = "Appointment Date")]
        [Required(ErrorMessage = "Required.")]
        public DateTime nextmeetingdate { get; set; }

        [FirestoreProperty]
        public string Id { get; set; }
    }
}