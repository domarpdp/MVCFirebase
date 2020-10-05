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
    
    public class AppointmentController : Controller
    {
        // GET: Appointment

        [CheckSessionTimeOut]
        [AccessDeniedAuthorize(Roles = "Receptionist,Chemist,Cashier")]
        public async Task<ActionResult> Index(string startdate)
        {

            DateTime SearchDate;
            if (startdate == null)
            {
                SearchDate = Convert.ToDateTime(DateTime.UtcNow);
            }
            else
            {
                SearchDate = Convert.ToDateTime(startdate);
                SearchDate = DateTime.SpecifyKind(SearchDate, DateTimeKind.Utc);
            }
            
            string ClinicMobileNumber = GlobalSessionVariables.ClinicMobileNumber;
            string Path = AppDomain.CurrentDomain.BaseDirectory + @"greenpaperdev-firebase-adminsdk-8k2y5-fb46e63414.json";
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path);
            FirestoreDb db = FirestoreDb.Create("greenpaperdev");
            

            List<Appointment> AppointmentList = new List<Appointment>();
            List<string> statusList = new List<string>();
            

            //Query Qref = db.Collection("Students").WhereEqualTo("StudentName","Suvidhi");
            Query Qref = db.Collection("clinics").WhereEqualTo("clinicmobilenumber", ClinicMobileNumber); 
            QuerySnapshot snapClinics = await Qref.GetSnapshotAsync();
            QuerySnapshot snapAppointments;
            string WhoFirst = "Cashier";
            foreach (DocumentSnapshot docsnapClinics in snapClinics)
            {
                Clinic clinic = docsnapClinics.ConvertTo<Clinic>();

                QuerySnapshot snapSettings = await docsnapClinics.Reference.Collection("settings").Limit(1).GetSnapshotAsync();
                if(snapSettings.Count > 0)
                {
                    DocumentSnapshot docSnapSettings = snapSettings.Documents[0];

                    if (docSnapSettings.Exists)
                    {
                        WhoFirst = docSnapSettings.GetValue<string>("whofirst");
                    }
                }
                if (User.IsInRole("Cashier") && User.IsInRole("Chemist"))
                {
                    if(WhoFirst == "Chemist")
                    {
                        statusList.Add("Completed");
                        statusList.Add("MedicineGiven");
                    }
                    else
                    {
                        statusList.Add("Completed");
                        statusList.Add("FeeTaken");
                    }

                    snapAppointments = await docsnapClinics.Reference.Collection("appointments").WhereGreaterThanOrEqualTo("raisedDate", Timestamp.FromDateTime(SearchDate.Date)).WhereLessThan("raisedDate", Timestamp.FromDateTime(SearchDate.AddDays(1))).WhereIn("status", statusList).GetSnapshotAsync();
                } 
                else if (User.IsInRole("Cashier"))
                {
                    if(WhoFirst == "Cashier")
                    {
                        snapAppointments = await docsnapClinics.Reference.Collection("appointments").WhereGreaterThanOrEqualTo("raisedDate", Timestamp.FromDateTime(SearchDate.Date)).WhereLessThan("raisedDate", Timestamp.FromDateTime(SearchDate.AddDays(1))).WhereEqualTo("status", "Completed").GetSnapshotAsync();
                    }
                    else
                    {
                        snapAppointments = await docsnapClinics.Reference.Collection("appointments").WhereGreaterThanOrEqualTo("raisedDate", Timestamp.FromDateTime(SearchDate.Date)).WhereLessThan("raisedDate", Timestamp.FromDateTime(SearchDate.AddDays(1))).WhereEqualTo("status", "MedicineGiven").GetSnapshotAsync();
                    }
                    
                }
                else if (User.IsInRole("Chemist"))
                {
                    if (WhoFirst == "Chemist")
                    {
                        snapAppointments = await docsnapClinics.Reference.Collection("appointments").WhereGreaterThanOrEqualTo("raisedDate", Timestamp.FromDateTime(SearchDate.Date)).WhereLessThan("raisedDate", Timestamp.FromDateTime(SearchDate.AddDays(1))).WhereEqualTo("status", "Completed").GetSnapshotAsync();
                    }
                    else
                    {
                        snapAppointments = await docsnapClinics.Reference.Collection("appointments").WhereGreaterThanOrEqualTo("raisedDate", Timestamp.FromDateTime(SearchDate.Date)).WhereLessThan("raisedDate", Timestamp.FromDateTime(SearchDate.AddDays(1))).WhereEqualTo("status", "FeeTaken").GetSnapshotAsync();
                    }
                        
                }
                else
                {
                    snapAppointments = await docsnapClinics.Reference.Collection("appointments").WhereGreaterThanOrEqualTo("raisedDate", Timestamp.FromDateTime(SearchDate.Date)).WhereLessThan("raisedDate", Timestamp.FromDateTime(SearchDate.AddDays(1))).WhereEqualTo("status", "Waiting").GetSnapshotAsync();
                }
                    

                foreach (DocumentSnapshot docsnapAppointments in snapAppointments)
                {
                    

                    Appointment appointment = docsnapAppointments.ConvertTo<Appointment>();

                    QuerySnapshot snapPatient = await docsnapClinics.Reference.Collection("patientList").WhereEqualTo("patient_id", appointment.patient_id).Limit(1).GetSnapshotAsync();
                    DocumentSnapshot docsnapPatient = snapPatient.Documents[0];

                    Patient patient = docsnapPatient.ConvertTo<Patient>();
                    if (docsnapAppointments.Exists)
                    {
                        appointment.clinic_name = clinic.clinicname;
                        appointment.patient_name = patient.patient_name;
                        appointment.id = docsnapAppointments.Id;
                        AppointmentList.Add(appointment);
                    }
                }

            }
            AppointmentList = AppointmentList.OrderByDescending(a => a.token).ToList();
            ViewBag.Message = SearchDate;
            return View(AppointmentList);
        }



        // GET: Appointment/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Appointment/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Appointment/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Appointment/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Appointment/Edit/5
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

        // GET: Appointment/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Appointment/Delete/5
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

        
        
        //public async Task<string> Prescription(string patientAutoId)
        //{

        //    string ClinicMobileNumber = GlobalSessionVariables.ClinicMobileNumber;
        //    string Path = AppDomain.CurrentDomain.BaseDirectory + @"greenpaperdev-firebase-adminsdk-8k2y5-fb46e63414.json";
        //    Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path);
        //    FirestoreDb db = FirestoreDb.Create("greenpaperdev");

        //    string prescriptionString = "";
            

        //    Query QrefPrescriptions = db.Collection("clinics").Document(GlobalSessionVariables.ClinicDocumentAutoId).Collection("patientList").Document(patientAutoId).Collection("prescriptions").OrderByDescending("timeStamp").Limit(1);
        //    QuerySnapshot snap = await QrefPrescriptions.GetSnapshotAsync();

        //    foreach (DocumentSnapshot docsnap in snap)
        //    {
        //        if (docsnap.Exists)
        //        {

        //            prescriptionString = docsnap.GetValue<string>("file");
        //        }
        //    }

        //    return prescriptionString;
        //}

        //public async Task<ActionResult> PrescriptionList(string patientAutoId)
        //{
        //    string ClinicMobileNumber = GlobalSessionVariables.ClinicMobileNumber;
        //    string Path = AppDomain.CurrentDomain.BaseDirectory + @"greenpaperdev-firebase-adminsdk-8k2y5-fb46e63414.json";
        //    Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path);
        //    FirestoreDb db = FirestoreDb.Create("greenpaperdev");


        //    List<string> prescriptionStringList = new List<string>();

        //    Query QrefPrescriptions = db.Collection("clinics").Document(GlobalSessionVariables.ClinicDocumentAutoId).Collection("patientList").Document(patientAutoId).Collection("prescriptions").OrderByDescending("timeStamp");
        //    QuerySnapshot snap = await QrefPrescriptions.GetSnapshotAsync();

        //    foreach (DocumentSnapshot docsnap in snap)
        //    {
        //        if (docsnap.Exists)
        //        {

        //            prescriptionStringList.Add(docsnap.GetValue<string>("file"));
        //        }
        //    }



        //    return View(prescriptionStringList);

        //}

        public ActionResult Fee(string id,string patient,string fee)
        {
            TempData["appointmentAutoId"] = id;
            TempData["patientAutoId"] = patient;
            TempData["fee"] = fee;
            List<SelectListItem> paymentmode = new List<SelectListItem>() {
                new SelectListItem {
                    Text = "Cash", Value = "Cash"
                },
                new SelectListItem {
                    Text = "Paytm", Value = "Paytm"
                },
                new SelectListItem {
                    Text = "Credit Card", Value = "Credit Card"
                },
                new SelectListItem {
                    Text = "Debit Card", Value = "Debit Card"
                },
            };
            ViewBag.PAYMENTMODES = paymentmode;
            return View();
        }

        // POST: Appointment/Create
        [HttpPost]
        public async Task<ActionResult> Fee()
        {
            try
            {
                string patientAutoId = TempData["patientAutoId"].ToString();
                string appointmentAutoId = TempData["appointmentAutoId"].ToString();

                string Path = AppDomain.CurrentDomain.BaseDirectory + @"greenpaperdev-firebase-adminsdk-8k2y5-fb46e63414.json";
                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path);
                FirestoreDb db = FirestoreDb.Create("greenpaperdev");

                DocumentReference docRef = db.Collection("clinics").Document(GlobalSessionVariables.ClinicDocumentAutoId).Collection("appointments").Document(appointmentAutoId);
                DocumentSnapshot docSnap = await docRef.GetSnapshotAsync();

                if (docSnap.Exists)
                {
                    await docRef.UpdateAsync("completionFee", DateTime.UtcNow);
                }
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
