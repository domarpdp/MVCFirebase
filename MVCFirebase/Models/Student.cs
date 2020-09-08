using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Google.Cloud.Firestore;

namespace MVCFirebase.Models
{
    [FirestoreData]
    public class Student
    {
        [FirestoreProperty]
        public int StudentId { get; set; }
        [FirestoreProperty]
        public string StudentName { get; set; }
        [FirestoreProperty]
        public int Age { get; set; }
    }
}