using System;
using Google.Cloud.Firestore;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVCFirebase.Models
{
    public class FirestoreService
    {
        private readonly FirestoreDb _db;

        public FirestoreService()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + @"greenpaperdev-firebase-adminsdk-8k2y5-fb46e63414.json";
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", path);
            _db = FirestoreDb.Create("greenpaperdev");
        }

        public FirestoreDb GetDb()
        {
            return _db;
        }
    }
}