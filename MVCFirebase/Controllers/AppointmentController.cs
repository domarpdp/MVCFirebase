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
        [AccessDeniedAuthorize(Roles = "Receptionist")]
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

                snapAppointments = await docsnapClinics.Reference.Collection("appointments").WhereGreaterThanOrEqualTo("raisedDate", Timestamp.FromDateTime(SearchDate.Date)).WhereLessThan("raisedDate", Timestamp.FromDateTime(SearchDate.AddDays(1))).WhereEqualTo("status", "Waiting").GetSnapshotAsync();

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
                        appointment.patient_care_of = patient.care_of;
                        appointment.patient_gender = patient.gender;
                        appointment.patient_age = patient.age;
                        appointment.patient_mobile = patient.patient_mobile_number;
                        appointment.id = docsnapAppointments.Id;
                        appointment.tokenIteger = Convert.ToInt32(docsnapAppointments.GetValue<string>("token"));
                        AppointmentList.Add(appointment);
                    }
                }

            }
            AppointmentList = AppointmentList.OrderByDescending(a => a.tokenIteger).ToList();
            ViewBag.Message = SearchDate.Date;
            return View(AppointmentList);
        }
        [AccessDeniedAuthorize(Roles = "Chemist,Cashier")]
        public async Task<ActionResult> Waiting(string startdate)
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
            string statusCashier = "";
            string statusChemist = "";
            foreach (DocumentSnapshot docsnapClinics in snapClinics)
            {
                Clinic clinic = docsnapClinics.ConvertTo<Clinic>();

                QuerySnapshot snapSettings = await docsnapClinics.Reference.Collection("settings").Limit(1).GetSnapshotAsync();
                if (snapSettings.Count > 0)
                {
                    DocumentSnapshot docSnapSettings = snapSettings.Documents[0];

                    if (docSnapSettings.Exists)
                    {
                        WhoFirst = docSnapSettings.GetValue<string>("whofirst");
                    }
                }

                snapAppointments = await docsnapClinics.Reference.Collection("appointments").WhereGreaterThanOrEqualTo("raisedDate", Timestamp.FromDateTime(SearchDate.Date)).WhereLessThan("raisedDate", Timestamp.FromDateTime(SearchDate.AddDays(1))).WhereEqualTo("status", "Completed").GetSnapshotAsync();

                


                foreach (DocumentSnapshot docsnapAppointments in snapAppointments)
                {


                    Appointment appointment = docsnapAppointments.ConvertTo<Appointment>();

                    QuerySnapshot snapPatient = await docsnapClinics.Reference.Collection("patientList").WhereEqualTo("patient_id", appointment.patient_id).Limit(1).GetSnapshotAsync();
                    DocumentSnapshot docsnapPatient = snapPatient.Documents[0];

                    Patient patient = docsnapPatient.ConvertTo<Patient>();
                    if (docsnapAppointments.Exists)
                    {
                        if (User.IsInRole("Cashier") && User.IsInRole("Chemist"))
                        {
                            try
                            {
                                statusCashier = docsnapAppointments.GetValue<string>("statusCashier");
                            }
                            catch
                            {
                                statusCashier = null;
                            }
                            if (statusCashier == null)
                            {
                                appointment.clinic_name = clinic.clinicname;
                                appointment.patient_name = patient.patient_name;
                                appointment.patient_care_of = patient.care_of;
                                appointment.patient_gender = patient.gender;
                                appointment.patient_age = patient.age;
                                appointment.patient_mobile = patient.patient_mobile_number;
                                appointment.id = docsnapAppointments.Id;
                                appointment.tokenIteger = Convert.ToInt32(docsnapAppointments.GetValue<string>("token"));
                                AppointmentList.Add(appointment);
                            }

                        }
                        else if (User.IsInRole("Cashier"))
                        {
                            try
                            {
                                statusCashier = docsnapAppointments.GetValue<string>("statusCashier");
                            }
                            catch
                            {
                                statusCashier = null;
                            }
                            if (statusCashier == null)
                            {
                                appointment.clinic_name = clinic.clinicname;
                                appointment.patient_name = patient.patient_name;
                                appointment.patient_care_of = patient.care_of;
                                appointment.patient_gender = patient.gender;
                                appointment.patient_age = patient.age;
                                appointment.patient_mobile = patient.patient_mobile_number;
                                appointment.id = docsnapAppointments.Id;
                                appointment.tokenIteger = Convert.ToInt32(docsnapAppointments.GetValue<string>("token"));
                                AppointmentList.Add(appointment);
                            }
                        }
                        else if (User.IsInRole("Chemist"))
                        {
                            try
                            {
                                statusChemist = docsnapAppointments.GetValue<string>("statusChemist");
                            }
                            catch
                            {
                                statusChemist = null;
                            }
                            if (statusChemist == null)
                            {
                                appointment.clinic_name = clinic.clinicname;
                                appointment.patient_name = patient.patient_name;
                                appointment.patient_care_of = patient.care_of;
                                appointment.patient_gender = patient.gender;
                                appointment.patient_age = patient.age;
                                appointment.patient_mobile = patient.patient_mobile_number;
                                appointment.id = docsnapAppointments.Id;
                                appointment.tokenIteger = Convert.ToInt32(docsnapAppointments.GetValue<string>("token"));
                                AppointmentList.Add(appointment);
                            }
                        }
                    }
                }

            }
            AppointmentList = AppointmentList.OrderByDescending(a => a.tokenIteger).ToList();
            ViewBag.Message = SearchDate.Date;
            return View(AppointmentList);
        }
        [AccessDeniedAuthorize(Roles = "Chemist,Cashier")]
        public async Task<ActionResult> Completed(string startdate)
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
            string statusCashier = "";
            string statusChemist;
            foreach (DocumentSnapshot docsnapClinics in snapClinics)
            {
                Clinic clinic = docsnapClinics.ConvertTo<Clinic>();

                QuerySnapshot snapSettings = await docsnapClinics.Reference.Collection("settings").Limit(1).GetSnapshotAsync();
                if (snapSettings.Count > 0)
                {
                    DocumentSnapshot docSnapSettings = snapSettings.Documents[0];

                    if (docSnapSettings.Exists)
                    {
                        WhoFirst = docSnapSettings.GetValue<string>("whofirst");
                    }
                }

                snapAppointments = await docsnapClinics.Reference.Collection("appointments").WhereGreaterThanOrEqualTo("raisedDate", Timestamp.FromDateTime(SearchDate.Date)).WhereLessThan("raisedDate", Timestamp.FromDateTime(SearchDate.AddDays(1))).WhereEqualTo("status", "Completed").GetSnapshotAsync();




                foreach (DocumentSnapshot docsnapAppointments in snapAppointments)
                {


                    Appointment appointment = docsnapAppointments.ConvertTo<Appointment>();

                    QuerySnapshot snapPatient = await docsnapClinics.Reference.Collection("patientList").WhereEqualTo("patient_id", appointment.patient_id).Limit(1).GetSnapshotAsync();
                    DocumentSnapshot docsnapPatient = snapPatient.Documents[0];

                    Patient patient = docsnapPatient.ConvertTo<Patient>();
                    if (docsnapAppointments.Exists)
                    {
                        if (User.IsInRole("Cashier") && User.IsInRole("Chemist"))
                        {
                            try
                            {
                                statusCashier = docsnapAppointments.GetValue<string>("statusCashier");
                            }
                            catch
                            {
                                statusCashier = null;
                            }
                            if (statusCashier != null)
                            {
                                appointment.clinic_name = clinic.clinicname;
                                appointment.patient_name = patient.patient_name;
                                appointment.patient_care_of = patient.care_of;
                                appointment.patient_gender = patient.gender;
                                appointment.patient_age = patient.age;
                                appointment.patient_mobile = patient.patient_mobile_number;
                                appointment.id = docsnapAppointments.Id;
                                appointment.tokenIteger = Convert.ToInt32(docsnapAppointments.GetValue<string>("token"));
                                AppointmentList.Add(appointment);
                            }
                        }
                        else if (User.IsInRole("Cashier"))
                        {
                            try
                            {
                                statusCashier = docsnapAppointments.GetValue<string>("statusCashier");
                            }
                            catch
                            {
                                statusCashier = null;
                            }
                            if (statusCashier != null)
                            {
                                appointment.clinic_name = clinic.clinicname;
                                appointment.patient_name = patient.patient_name;
                                appointment.patient_care_of = patient.care_of;
                                appointment.patient_gender = patient.gender;
                                appointment.patient_age = patient.age;
                                appointment.patient_mobile = patient.patient_mobile_number;
                                appointment.id = docsnapAppointments.Id;
                                appointment.tokenIteger = Convert.ToInt32(docsnapAppointments.GetValue<string>("token"));
                                AppointmentList.Add(appointment);
                            }
                        }
                        else if (User.IsInRole("Chemist"))
                        {
                            try
                            {
                                statusChemist = docsnapAppointments.GetValue<string>("statusChemist");
                            }
                            catch
                            {
                                statusChemist = null;
                            }
                            if (statusChemist != null)
                            {
                                appointment.clinic_name = clinic.clinicname;
                                appointment.patient_name = patient.patient_name;
                                appointment.patient_care_of = patient.care_of;
                                appointment.patient_gender = patient.gender;
                                appointment.patient_age = patient.age;
                                appointment.patient_mobile = patient.patient_mobile_number;
                                appointment.id = docsnapAppointments.Id;
                                appointment.tokenIteger = Convert.ToInt32(docsnapAppointments.GetValue<string>("token"));
                                AppointmentList.Add(appointment);
                            }
                        }

                    }
                }

            }
            AppointmentList = AppointmentList.OrderByDescending(a => a.tokenIteger).ToList();
            ViewBag.Message = SearchDate.Date;
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
                    Dictionary<string, object> data1 = new Dictionary<string, object>
                        {
                            {"completiondateCashier" ,DateTime.UtcNow},
                            {"statusCashier" ,"Completed"}
                        
                        };

                    
                    await docRef.UpdateAsync(data1);

                }
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }


        [HttpPost]
        public async Task<ActionResult> SubmitCashier(FormCollection collection)
        {
            try
            {

                string appointmentAutoId = collection["appointmentAutoIdFee"];

                string Path = AppDomain.CurrentDomain.BaseDirectory + @"greenpaperdev-firebase-adminsdk-8k2y5-fb46e63414.json";
                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path);
                FirestoreDb db = FirestoreDb.Create("greenpaperdev");

                DocumentReference docRef = db.Collection("clinics").Document(GlobalSessionVariables.ClinicDocumentAutoId).Collection("appointments").Document(appointmentAutoId);
                DocumentSnapshot docSnap = await docRef.GetSnapshotAsync();

                if (docSnap.Exists)
                {
                    Dictionary<string, object> data1 = new Dictionary<string, object>
                        {
                            {"completiondateCashier" ,DateTime.UtcNow},
                            {"statusCashier" ,"Completed"},
                            {"modeofpayment",collection["modeofpaymentFee"]}
                        };


                    await docRef.UpdateAsync(data1);

                }
                // TODO: Add delete logic here

                return RedirectToAction("Waiting");
            }
            catch
            {
                return View();
            }
        }

        [HttpPost]
        public async Task<ActionResult> SubmitChemist(FormCollection collection)
        {
            try
            {

                string appointmentAutoId = collection["appid"]; ;

                string Path = AppDomain.CurrentDomain.BaseDirectory + @"greenpaperdev-firebase-adminsdk-8k2y5-fb46e63414.json";
                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path);
                FirestoreDb db = FirestoreDb.Create("greenpaperdev");

                DocumentReference docRef = db.Collection("clinics").Document(GlobalSessionVariables.ClinicDocumentAutoId).Collection("appointments").Document(appointmentAutoId);
                DocumentSnapshot docSnap = await docRef.GetSnapshotAsync();

                if (docSnap.Exists)
                {
                    Dictionary<string, object> data1 = new Dictionary<string, object>
                        {
                            {"completiondateChemist" ,DateTime.UtcNow},
                            {"statusChemist" ,"Completed"}
                        };


                    await docRef.UpdateAsync(data1);

                }
                // TODO: Add delete logic here

                return RedirectToAction("Waiting");
            }
            catch
            {
                return View();
            }
        }
        [HttpPost]
        public async Task<ActionResult> CashierChemistUpdate(FormCollection collection)
        {
            try
            {

                string appointmentAutoId = collection["appointmentAutoId"];

                string Path = AppDomain.CurrentDomain.BaseDirectory + @"greenpaperdev-firebase-adminsdk-8k2y5-fb46e63414.json";
                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path);
                FirestoreDb db = FirestoreDb.Create("greenpaperdev");

                DocumentReference docRef = db.Collection("clinics").Document(GlobalSessionVariables.ClinicDocumentAutoId).Collection("appointments").Document(appointmentAutoId);
                DocumentSnapshot docSnap = await docRef.GetSnapshotAsync();

                if (docSnap.Exists)
                {
                    Dictionary<string, object> data1 = new Dictionary<string, object>
                        {
                            {"completiondateCashier" ,DateTime.UtcNow},
                            {"statusCashier" ,"Completed"},
                            {"completiondateChemist" ,DateTime.UtcNow},
                            {"statusChemist" ,"Completed"},
                            {"modeofpayment",collection["modeofpayment"]}
                        };


                    await docRef.UpdateAsync(data1);

                }
                // TODO: Add delete logic here

                return RedirectToAction("Waiting");
            }
            catch
            {
                return View();
            }
        }
    }
}
