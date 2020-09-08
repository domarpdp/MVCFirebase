using Google.Cloud.Firestore;
using MVCFirebase.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace MVCFirebase.Controllers
{
    public class PatientController : Controller
    {
        // GET: Patient
        public async Task<ActionResult> Index()
        {
            string ClinicMobileNumber = GlobalSessionVariables.ClinicMobileNumber;
            string Path = AppDomain.CurrentDomain.BaseDirectory + @"greenpaperdev-firebase-adminsdk-8k2y5-fb46e63414.json";
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path);
            FirestoreDb db = FirestoreDb.Create("greenpaperdev");


            List<Patient> PatientList = new List<Patient>();

            
            //Query Qref = db.Collection("Students").WhereEqualTo("StudentName","Suvidhi");
            Query Qref = db.Collection("clinics").WhereEqualTo("clinicmobilenumber", ClinicMobileNumber);
            QuerySnapshot snap = await Qref.GetSnapshotAsync();

            foreach (DocumentSnapshot docsnap in snap)
            {
                Clinic clinic = docsnap.ConvertTo<Clinic>();
                QuerySnapshot snap2 = await docsnap.Reference.Collection("patientList").Limit(100).GetSnapshotAsync();

                foreach (DocumentSnapshot docsnap2 in snap2)
                {
                    Patient patient = docsnap2.ConvertTo<Patient>();
                    if (docsnap2.Exists)
                    {
                        patient.clinic_name = clinic.clinicname;
                        PatientList.Add(patient);
                    }
                }
                    
            }

            return View(PatientList);
            
        }

        // GET: Patient/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Patient/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Patient/Create
        [HttpPost]
        public ActionResult Create(Patient patient)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    string Path = AppDomain.CurrentDomain.BaseDirectory + @"greenpaperdev-firebase-adminsdk-8k2y5-fb46e63414.json";
                    Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path);
                    FirestoreDb db = FirestoreDb.Create("greenpaperdev");

                    
                    //CollectionReference col1 = db.Collection("clinics").Document(GlobalSessionVariables.ClinicDocumentAutoId).Collection("patientList").Document("test");
                    CollectionReference col1 = db.Collection("clinics").Document(GlobalSessionVariables.ClinicDocumentAutoId).Collection("patientList");
                    //Dictionary<string, object> data1 = new Dictionary<string, object>
                    //{
                    //    {"patient_name" ,collection["patient_name"]},
                    //    {"age" ,collection["age"]},
                    //    {"care_of" ,collection["care_of"]},
                    //    {"city" ,collection["city"]},
                    //    {"creation_date" ,DateTime.UtcNow},
                    //    {"disease" ,collection["disease"]},
                    //    {"gender" ,collection["gender"]},
                    //    {"patient_id" ,"Test"},
                    //    {"patient_mobile_number",collection["patient_mobile_number"]},
                    //    {"refer_by" ,collection["refer_by"]},
                    //    {"refer_to_doctor" ,collection["refer_to_doctor"]},
                    //    {"search_text" ,collection["patient_name"]+collection["patient_mobile_number"]+"Test"}
                    //};

                    Dictionary<string, object> data1 = new Dictionary<string, object>
                    {
                        {"patient_name" ,patient.patient_name},
                        {"age" ,patient.age},
                        {"care_of" ,patient.care_of},
                        {"city" ,patient.city},
                        {"creation_date" ,DateTime.UtcNow},
                        {"disease" ,patient.disease},
                        {"gender" ,patient.gender},
                        {"patient_id" ,"Test"},
                        {"patient_mobile_number",patient.patient_mobile_number},
                        {"refer_by" ,patient.refer_by},
                        {"refer_to_doctor" ,patient.refer_to_doctor},
                        {"search_text" ,patient.patient_name+patient.patient_mobile_number+"Test"}
                    };

                    //col1.AddAsync(data1);
                    
                    col1.Document().SetAsync(data1);

                    // TODO: Add insert logic here
                    //var result = await fireBaseClient.Child("Students").PostAsync(std);

                    return RedirectToAction("Index");
                }
                else
                {
                    return View(patient);
                }

                
            }
            catch (Exception ex)
            {
                return View();
            }

            //try
            //{
            //    if(!ModelState.IsValid)
            //    {
            //        return View();
            //    }
            //    else
            //    {
            //        return RedirectToAction("Index");
            //    }
            //    // TODO: Add insert logic here

                
            //}
            //catch
            //{
            //    return View();
            //}
        }

        // GET: Patient/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Patient/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Patient/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Patient/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
