using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MVCFirebase.Models
{
    [FirestoreData]
    public class User
    {
        [FirestoreProperty]
        public string clinicmobilenumber { get; set; }

        [FirestoreProperty]
        [Required(ErrorMessage = "Required.")]
        public string mobile_number { get; set; }

        [FirestoreProperty]
        [Required(ErrorMessage = "Required.")]
        public string name { get; set; }


        [FirestoreProperty]
        [Required(ErrorMessage = "Required.")]
        public string password { get; set; }


        [FirestoreProperty]
        public bool RememberMe { get; set; }
        
        [FirestoreProperty]
        public string[] user_roles { get; set; }
    }
}