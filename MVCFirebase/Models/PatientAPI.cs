using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MVCFirebase.Models
{
    public class PatientAPI
    {

            //new field added in SQL Server
            [Key]
            public int PatientId { get; set; }


            public string documentId { get; set; }
            public string id { get; set; }


            public string patient_id { get; set; }

            public string clinicId { get; set; }

            public string clinicCode { get; set; }


            public string search_text { get; set; }

            public string care_of { get; set; }



            public string patient_name { get; set; }


            public string gender { get; set; }


            public string age { get; set; }



            public string patient_mobile_number { get; set; }


            public string refer_by { get; set; }


            public string disease { get; set; }



            public string refer_to_doctor { get; set; }


            public string city { get; set; }



            

            public string severity { get; set; }


            public DateTime? creation_date { get; set; }

            public DateTime? updatedAt { get; set; }

            public bool? isCreated { get; set; }

            public bool? isSynced { get; set; }

            public string createdBy { get; set; }

            public DateTime? loginAt { get; set; }

            public bool? patientAppDownloaded { get; set; }

            public string dob { get; set; }



        
    }
}