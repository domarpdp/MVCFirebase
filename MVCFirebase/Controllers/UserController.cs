using Google.Cloud.Firestore;
using MVCFirebase.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace MVCFirebase.Controllers
{[Authorize]
    public class UserController : Controller
    {
        // GET: User
        public async Task<ActionResult> Index()
        {
            string ClinicMobileNumber = GlobalSessionVariables.ClinicMobileNumber;
            string Path = AppDomain.CurrentDomain.BaseDirectory + @"greenpaperdev-firebase-adminsdk-8k2y5-fb46e63414.json";
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path);
            FirestoreDb db = FirestoreDb.Create("greenpaperdev");
            List<User> UserList = new List<User>();


            //Query Qref = db.Collection("Students").WhereEqualTo("StudentName","Suvidhi");
            Query Qref = db.Collection("clinics").WhereEqualTo("clinicmobilenumber", ClinicMobileNumber);
            QuerySnapshot snapClinics = await Qref.GetSnapshotAsync();

            foreach (DocumentSnapshot docsnapClinics in snapClinics)
            {
                Clinic clinic = docsnapClinics.ConvertTo<Clinic>();
                QuerySnapshot snapUsers = await docsnapClinics.Reference.Collection("user").OrderByDescending("name").GetSnapshotAsync();

                foreach (DocumentSnapshot docsnapUsers in snapUsers)
                {


                    User user = docsnapUsers.ConvertTo<User>();
                    user.clinicmobilenumber = clinic.clinicmobilenumber;

                    //QuerySnapshot snapPatient = await docsnapClinics.Reference.Collection("patientList").WhereEqualTo("patient_id", appointment.patient_id).Limit(1).GetSnapshotAsync();
                    //DocumentSnapshot docsnapPatient = snapPatient.Documents[0];

                    //Patient patient = docsnapPatient.ConvertTo<Patient>();
                    if (docsnapUsers.Exists)
                    {
                        UserList.Add(user);
                    }
                }

            }

            return View(UserList);
            //return View();
        }

        // GET: User/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: User/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: User/Create
        [HttpPost]
        public ActionResult Create(User user)
        {try
            {
                if (ModelState.IsValid)
                {
                    string Path = AppDomain.CurrentDomain.BaseDirectory + @"greenpaperdev-firebase-adminsdk-8k2y5-fb46e63414.json";
                    Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path);
                    FirestoreDb db = FirestoreDb.Create("greenpaperdev");


                    //CollectionReference col1 = db.Collection("clinics").Document(GlobalSessionVariables.ClinicDocumentAutoId).Collection("patientList").Document("test");
                    CollectionReference col1 = db.Collection("clinics").Document(GlobalSessionVariables.ClinicDocumentAutoId).Collection("user");
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
                    string[] roles = user.user_roles[0].Remove(user.user_roles[0].Length - 1,1).Split(',');
                    
                    Dictionary<string, object> data1 = new Dictionary<string, object>
                    {
                        {"name" ,user.name},
                        {"email" ,""},
                        {"idproof" ,""},
                        {"creation_date" ,DateTime.UtcNow},
                        {"mobile_number" ,user.mobile_number},
                        {"password" ,user.password},
                        {"signature" ,""},
                        {"status_enable",false},
                        {"user_qualification" ,user.quaification},
                        
                    };
                    data1.Add("user_roles", roles);

                    

                    col1.Document().SetAsync(data1);

                    

                    return RedirectToAction("Index");
                }
                else
                {
                    return View(user);
                }


            }
            catch (Exception ex)
            {
                return View(user);
            }
        }

        // GET: User/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: User/Edit/5
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

        // GET: User/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: User/Delete/5
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
