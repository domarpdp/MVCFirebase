using Google.Cloud.Firestore;
using MVCFirebase.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace MVCFirebase.Controllers
{[Authorize]
    public class AppointmentController : Controller
    {
        // GET: Appointment
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


            //Query Qref = db.Collection("Students").WhereEqualTo("StudentName","Suvidhi");
            Query Qref = db.Collection("clinics").WhereEqualTo("clinicmobilenumber", ClinicMobileNumber); 
            QuerySnapshot snapClinics = await Qref.GetSnapshotAsync();

            foreach (DocumentSnapshot docsnapClinics in snapClinics)
            {
                Clinic clinic = docsnapClinics.ConvertTo<Clinic>();
                QuerySnapshot snapAppointments = await docsnapClinics.Reference.Collection("appointments").WhereGreaterThanOrEqualTo("raisedDate",Timestamp.FromDateTime(SearchDate)).WhereLessThan("raisedDate",Timestamp.FromDateTime(SearchDate.AddDays(1))).OrderByDescending("raisedDate").GetSnapshotAsync();

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
                        AppointmentList.Add(appointment);
                    }
                }

            }

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
    }
}
