using Google.Cloud.Firestore;
using Microsoft.Ajax.Utilities;
using MVCFirebase.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace MVCFirebase.Controllers
{
    
    public class AppointmentController : Controller
    {

        private readonly FirestoreDb _db;

        public AppointmentController()
        {
            var firestoreService = (FirestoreService)System.Web.HttpContext.Current.Application["FirestoreService"];
            _db = firestoreService.GetDb();
        }


        [AccessDeniedAuthorize(Roles = "Doctor")]
        [HttpGet]
        public async Task<ActionResult> Doctor()
        {

            return View();

        }

        // GET: Appointment

        [AccessDeniedAuthorize(Roles = "Receptionist, Doctor, Chemist, Cashier")]
        [HttpGet]
        public async Task<ActionResult> Index()
        {

            List<Appointment> AppointmentList = new List<Appointment>();
            List<Patient> PatientList = new List<Patient>();
            List<AppointmentPatientViewModel> AppointmentPatientList = new List<AppointmentPatientViewModel>();
            List<SelectListItem> UserList = new List<SelectListItem>();
            string totalPatientCount = "0";
            string totalTodayAppointmentsReceptionist = "0";
            string totalWaitingAppointments = "0";
            string totalCompletedAppointments = "0";

            string RolesString = "";

            List<SelectListItem> diseases = new List<SelectListItem>() {
                        new SelectListItem {
                            Text = "General -(fever/general acute complaints)", Value = "General"
                        },
                        new SelectListItem {
                            Text = "ENT (ear nose throat)", Value = "ENT(ear nose throat)"
                        },
                        new SelectListItem {
                            Text = "CNS (central nervous system", Value = "CNS(central nervous system)"
                        },
                        new SelectListItem {
                            Text = "DERMATO (skin/hair)", Value = "DERMATO./SKIN/hair"
                        },
                        new SelectListItem {
                            Text = "GIT (gastro intestinal track)", Value = "GIT(gastro intestinal track)"
                        },
                        new SelectListItem {
                            Text = "SURGERY", Value = "SURGERY"
                        },
                        new SelectListItem {
                            Text = "TUMORS/ONCOLOGY", Value = "TUMOURS/ONCOLOGY"
                        },
                        new SelectListItem {
                            Text = "HAEMATOLOGY (blood)", Value = "HAEMATOLOGY(blood)"
                        },
                        new SelectListItem {
                            Text = "ORTHOPADICS (bones & muscles)", Value = "ORTHOPADICS(bones & muscles)"
                        },
                        new SelectListItem {
                            Text = "PAEDIATRIC (child)", Value = "paediatric"
                        },
                        new SelectListItem {
                            Text = "OPTHALMOLOGISTS (EYE)", Value = "opthalmologists(eye)"
                        },
                        new SelectListItem {
                            Text = "CVS (cardio vascular system/heart)", Value = "CVS(cardio vascular system/heart)"
                        },
                        new SelectListItem {
                            Text = "ENDOCRINOLOGY", Value = "ENDOCRINOLOGY"
                        },
                        new SelectListItem {
                            Text = "GYNAECOLOGY & OBS", Value = "GYNAECOLOGY & OBS"
                        },
                        new SelectListItem {
                            Text = "GENETICS", Value = "GENETICS"
                        },
                    };
            ViewBag.DISEASES = diseases;

            List<SelectListItem> referby = new List<SelectListItem>() {
                new SelectListItem {
                    Text = "PATIENT", Value = "PATIENT"
                },
                new SelectListItem {
                    Text = "TV ADV", Value = "TV ADV"
                },
                new SelectListItem {
                    Text = "SOCIAL MEDIA", Value = "SOCIAL MEDIA"
                },
                new SelectListItem {
                    Text = "WALL PAINTING", Value = "WALL PAINTING"
                },
                new SelectListItem {
                    Text = "POSTERS", Value = "POSTERS"
                },
                new SelectListItem {
                    Text = "INTERNET SEARCH", Value = "INTERNET SEARCH"
                },
                new SelectListItem {
                    Text = "CALLS", Value = "CALLS"
                },
                new SelectListItem {
                    Text = "DOCTOR", Value = "DOCTOR"
                },
                new SelectListItem {
                    Text = "HOSPITAL", Value = "HOSPITAL"
                },
                new SelectListItem {
                    Text = "MAIL/MSG", Value = "MAILMSG"
                },
                new SelectListItem {
                    Text = "RANDOM", Value = "RANDOM"
                },

            };
            ViewBag.REFERBYS = referby;

            DateTime SearchDate;
            SearchDate = Convert.ToDateTime(DateTime.UtcNow);

            SearchDate = SearchDate.Date;

            Timestamp SearchDateFrom = Timestamp.FromDateTime(SearchDate.Date.AddHours(-5).AddMinutes(-30));
            Timestamp SearchDateTo = Timestamp.FromDateTime(SearchDate.Date.AddHours(-5).AddMinutes(-30).AddDays(1));

            string ClinicMobileNumber = "";
            string ClinicFirebaseDocumentId = "";
            string[] Roles = null;
            string UserMobileNumber = "";
            string UserDetail = "";

            string msg = "";

            var authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            string savedString = "";

            if (authCookie != null)
            {
                var ticket = FormsAuthentication.Decrypt(authCookie.Value);
                if (ticket != null)
                {
                    savedString = ticket.Name; // Get the stored string
                    ClinicMobileNumber = savedString.Split('|')[3];
                    ClinicFirebaseDocumentId = savedString.Split('|')[4];
                    Roles = savedString.Split('|')[2].Split('_');
                    RolesString = savedString.Split('|')[2];
                    UserMobileNumber = savedString.Split('|')[0];
                    UserDetail = savedString.Split('|')[1] + "(" + string.Join(",", Roles.Select(r => r.Substring(0, 3))) + ")";
                    msg = savedString;
                }
            }
            else
            {
                savedString = "Cookie is Blank";
                msg = savedString;
            }



            if (Session["sessionid"] == null)
            {
                Session["sessionid"] = "empty";
                msg = "Session empty";
            }
            else
            {
                msg = Session["sessionid"].ToString();
            }

            List<string> statusList = new List<string>();
            try
            {
                Query Qref = _db.Collection("clinics").WhereEqualTo("clinicmobilenumber", ClinicMobileNumber);
                QuerySnapshot snapClinics = await Qref.GetSnapshotAsync();
                string WhoFirst = "Cashier";
                int i = 0;
                foreach (DocumentSnapshot docsnapClinics in snapClinics)
                {
                    Clinic clinic = docsnapClinics.ConvertTo<Clinic>();

                    #region Code to get doctors list for refer to
                    QuerySnapshot snapUsersDoctors = await docsnapClinics.Reference.Collection("user").WhereArrayContainsAny("user_roles", new string[] { "Doctor", "Chemist" }).GetSnapshotAsync();
                    foreach (DocumentSnapshot docsnapUsers in snapUsersDoctors)
                    {
                        SelectListItem user = new SelectListItem();
                        user.Text = docsnapUsers.GetValue<string>("name");
                        user.Value = docsnapUsers.Id;
                        if (docsnapUsers.Exists)
                        {
                            try
                            {
                                if (docsnapUsers.GetValue<bool>("user_deactivated") == false)
                                {
                                    UserList.Add(user);
                                }
                            }
                            catch
                            {
                                UserList.Add(user);
                            }


                        }
                    }



                    #endregion Code to Code to get doctors list for refer to

                    #region to get user documentid
                    QuerySnapshot snapUser = await docsnapClinics.Reference.Collection("user").WhereEqualTo("mobile_number", UserMobileNumber).GetSnapshotAsync();
                    DocumentSnapshot docUser = snapUser.Documents[0];

                    string userId = docUser.Id;
                    string Status = "";
                    #endregion

                    #region Get WhosFirst
                    QuerySnapshot snapSettings = await docsnapClinics.Reference.Collection("settings").Limit(1).GetSnapshotAsync();
                    if (snapSettings.Count > 0)
                    {
                        DocumentSnapshot docSnapSettings = snapSettings.Documents[0];

                        if (docSnapSettings.Exists)
                        {
                            WhoFirst = docSnapSettings.GetValue<string>("whofirst");
                        }
                    }
                    #endregion

                    //#region All Patient count
                    //QuerySnapshot snapPatientList = await docsnapClinics.Reference.Collection("patientList").GetSnapshotAsync();
                    //totalPatientCount += snapPatientList.Count;
                    //#endregion

                    //#region Waiting Count Logic
                    //if (Roles.Contains("Doctor") || Roles.Contains("Chemist") || Roles.Contains("Cashier")|| Roles.Contains("Receptionist"))
                    //{
                    //    if (Roles.Contains("Receptionist"))
                    //    {
                    //        QuerySnapshot snapWaitingAppointments = await docsnapClinics.Reference.Collection("appointments").WhereGreaterThanOrEqualTo("raisedDate", SearchDateFrom).WhereLessThan("raisedDate", SearchDateTo).WhereEqualTo("receptionist", userId).GetSnapshotAsync();
                    //        totalWaitingAppointments += snapWaitingAppointments.Count;
                    //    }
                    //    else if (Roles.Contains("Doctor"))
                    //    {
                    //        QuerySnapshot snapWaitingAppointments = await docsnapClinics.Reference.Collection("appointments").WhereGreaterThanOrEqualTo("raisedDate", SearchDateFrom).WhereLessThan("raisedDate", SearchDateTo).WhereEqualTo("status", "Waiting").WhereEqualTo("referTo", userId).GetSnapshotAsync();
                    //        totalWaitingAppointments += snapWaitingAppointments.Count;
                    //    }
                    //    else if (Roles.Contains("Chemist"))
                    //    {
                    //        QuerySnapshot snapWaitingAppointments = await docsnapClinics.Reference.Collection("appointments").WhereGreaterThanOrEqualTo("raisedDate", SearchDateFrom).WhereLessThan("raisedDate", SearchDateTo).WhereEqualTo("status", "Completed").GetSnapshotAsync();
                    //        totalWaitingAppointments += snapWaitingAppointments.Count;
                    //    }
                    //    else if (Roles.Contains("Cashier"))
                    //    {
                    //        QuerySnapshot snapWaitingAppointments = await docsnapClinics.Reference.Collection("appointments").WhereGreaterThanOrEqualTo("raisedDate", SearchDateFrom).WhereLessThan("raisedDate", SearchDateTo).WhereEqualTo("statusChemist", "Completed").GetSnapshotAsync();
                    //        totalWaitingAppointments += snapWaitingAppointments.Count;
                    //    }
                    //}
                    //#endregion

                    //#region Completed Count Logic
                    //if (Roles.Contains("Doctor") || Roles.Contains("Chemist") || Roles.Contains("Cashier"))
                    //{
                    //    if (Roles.Contains("Doctor"))
                    //    {
                    //        QuerySnapshot snapCompletedAppointments = await docsnapClinics.Reference.Collection("appointments").WhereGreaterThanOrEqualTo("raisedDate", SearchDateFrom).WhereLessThan("raisedDate", SearchDateTo).WhereEqualTo("status", "Completed").WhereEqualTo("referTo", userId).GetSnapshotAsync();
                    //        totalCompletedAppointments += snapCompletedAppointments.Count;
                    //    }
                    //    else if (Roles.Contains("Chemist"))
                    //    {
                    //        QuerySnapshot snapCompletedAppointments = await docsnapClinics.Reference.Collection("appointments").WhereGreaterThanOrEqualTo("raisedDate", SearchDateFrom).WhereLessThan("raisedDate", SearchDateTo).WhereEqualTo("statusChemist", "Completed").WhereEqualTo("referTo", userId).GetSnapshotAsync();
                    //        totalCompletedAppointments += snapCompletedAppointments.Count;
                    //    }
                    //    else if (Roles.Contains("Cashier"))
                    //    {
                    //        QuerySnapshot snapCompletedAppointments = await docsnapClinics.Reference.Collection("appointments").WhereGreaterThanOrEqualTo("raisedDate", SearchDateFrom).WhereLessThan("raisedDate", SearchDateTo).WhereEqualTo("statusCashier", "Completed").WhereEqualTo("referTo", userId).GetSnapshotAsync();
                    //        totalCompletedAppointments += snapCompletedAppointments.Count;
                    //    }
                    //}
                    //#endregion

                    #region Waiting Patient count and Appointment logic
                    
                    if (Roles.Contains("Receptionist"))
                    {
                        QuerySnapshot snapTodayAppointments = await docsnapClinics.Reference.Collection("appointments").WhereGreaterThanOrEqualTo("raisedDate", SearchDateFrom).WhereLessThan("raisedDate", SearchDateTo).WhereEqualTo("receptionist", userId).GetSnapshotAsync();
                        //totalTodayAppointmentsReceptionist += snapTodayAppointments.Count;

                        foreach (DocumentSnapshot docsnapAppointments in snapTodayAppointments)
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
                                appointment.status = docsnapAppointments.GetValue<string>("status");
                                appointment.statusCashier = docsnapAppointments.GetValue<string>("statusCashier");
                                appointment.statusChemist = docsnapAppointments.GetValue<string>("statusChemist");
                                //appointment.status = !string.IsNullOrEmpty(docsnapAppointments.GetValue<string>("status")) ? "Doct:" + docsnapAppointments.GetValue<string>("status") : "";
                                //appointment.statusCashier = !string.IsNullOrEmpty(docsnapAppointments.GetValue<string>("statusCashier")) ? "Cash:" + docsnapAppointments.GetValue<string>("statusCashier") : "";
                                //appointment.statusChemist = !string.IsNullOrEmpty(docsnapAppointments.GetValue<string>("statusChemist")) ? "Chem:" + docsnapAppointments.GetValue<string>("statusChemist") : "";
                                try { appointment.tokenIteger = Convert.ToInt32(docsnapAppointments.GetValue<string>("token")); }
                                catch { appointment.tokenIteger = i + 1; }

                                AppointmentList.Add(appointment);
                            }
                        }
                    }
                    else if (Roles.Contains("Doctor"))
                    {
                        QuerySnapshot snapTodayAppointments = await docsnapClinics.Reference.Collection("appointments").WhereGreaterThanOrEqualTo("raisedDate", SearchDateFrom).WhereLessThan("raisedDate", SearchDateTo).WhereEqualTo("status", "Waiting").WhereEqualTo("referTo", userId).GetSnapshotAsync();
                        //totalTodayAppointmentsReceptionist += snapTodayAppointments.Count;

                        foreach (DocumentSnapshot docsnapAppointments in snapTodayAppointments)
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
                                appointment.status = docsnapAppointments.GetValue<string>("status");
                                appointment.statusCashier = docsnapAppointments.GetValue<string>("statusCashier");
                                appointment.statusChemist = docsnapAppointments.GetValue<string>("statusChemist");
                                //appointment.status = !string.IsNullOrEmpty(docsnapAppointments.GetValue<string>("status")) ? "Doct:" + docsnapAppointments.GetValue<string>("status") : "";
                                //appointment.statusCashier = !string.IsNullOrEmpty(docsnapAppointments.GetValue<string>("statusCashier")) ? "Cash:" + docsnapAppointments.GetValue<string>("statusCashier") : "";
                                //appointment.statusChemist = !string.IsNullOrEmpty(docsnapAppointments.GetValue<string>("statusChemist")) ? "Chem:" + docsnapAppointments.GetValue<string>("statusChemist") : "";
                                try { appointment.tokenIteger = Convert.ToInt32(docsnapAppointments.GetValue<string>("token")); }
                                catch { appointment.tokenIteger = i + 1; }

                                AppointmentList.Add(appointment);
                            }
                        }
                    }
                    else if (Roles.Contains("Chemist"))
                    {
                        QuerySnapshot snapTodayAppointments = await docsnapClinics.Reference.Collection("appointments").WhereGreaterThanOrEqualTo("raisedDate", SearchDateFrom).WhereLessThan("raisedDate", SearchDateTo).WhereEqualTo("status", "Completed").GetSnapshotAsync();
                        //totalTodayAppointmentsReceptionist += snapTodayAppointments.Count;

                        foreach (DocumentSnapshot docsnapAppointments in snapTodayAppointments)
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
                                appointment.status = docsnapAppointments.GetValue<string>("status");
                                appointment.statusCashier = docsnapAppointments.GetValue<string>("statusCashier");
                                appointment.statusChemist = docsnapAppointments.GetValue<string>("statusChemist");
                                //appointment.status = !string.IsNullOrEmpty(docsnapAppointments.GetValue<string>("status")) ? "Doct:" + docsnapAppointments.GetValue<string>("status") : "";
                                //appointment.statusCashier = !string.IsNullOrEmpty(docsnapAppointments.GetValue<string>("statusCashier")) ? "Cash:" + docsnapAppointments.GetValue<string>("statusCashier") : "";
                                //appointment.statusChemist = !string.IsNullOrEmpty(docsnapAppointments.GetValue<string>("statusChemist")) ? "Chem:" + docsnapAppointments.GetValue<string>("statusChemist") : "";
                                try { appointment.tokenIteger = Convert.ToInt32(docsnapAppointments.GetValue<string>("token")); }
                                catch { appointment.tokenIteger = i + 1; }

                                AppointmentList.Add(appointment);
                            }
                        }
                    }

                    else if (Roles.Contains("Cashier"))
                    {
                        QuerySnapshot snapTodayAppointments = await docsnapClinics.Reference.Collection("appointments").WhereGreaterThanOrEqualTo("raisedDate", SearchDateFrom).WhereLessThan("raisedDate", SearchDateTo).WhereEqualTo("statusChemist", "Completed").GetSnapshotAsync();
                        //totalTodayAppointmentsReceptionist += snapTodayAppointments.Count;

                        foreach (DocumentSnapshot docsnapAppointments in snapTodayAppointments)
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
                                appointment.status = docsnapAppointments.GetValue<string>("status");
                                appointment.statusCashier = docsnapAppointments.GetValue<string>("statusCashier");
                                appointment.statusChemist = docsnapAppointments.GetValue<string>("statusChemist");
                                //appointment.status = !string.IsNullOrEmpty(docsnapAppointments.GetValue<string>("status")) ? "Doct:" + docsnapAppointments.GetValue<string>("status") : "";
                                //appointment.statusCashier = !string.IsNullOrEmpty(docsnapAppointments.GetValue<string>("statusCashier")) ? "Cash:" + docsnapAppointments.GetValue<string>("statusCashier") : "";
                                //appointment.statusChemist = !string.IsNullOrEmpty(docsnapAppointments.GetValue<string>("statusChemist")) ? "Chem:" + docsnapAppointments.GetValue<string>("statusChemist") : "";
                                try { appointment.tokenIteger = Convert.ToInt32(docsnapAppointments.GetValue<string>("token")); }
                                catch { appointment.tokenIteger = i + 1; }

                                AppointmentList.Add(appointment);
                            }
                        }
                    }
                    #endregion


                }

                JsonResult result = await GetCounts(SearchDate.ToString()); // Pass a date or null
                AppointmentsCount data = result.Data as AppointmentsCount;

                if (data != null)
                {
                    totalPatientCount = data.AllPatientsCount;
                    totalTodayAppointmentsReceptionist = data.TodayAppointmentsCount;
                    totalWaitingAppointments = data.WaitingAppointmentsCounts;
                    totalCompletedAppointments = data.CompletedAppointmentsCounts;

                    ViewBag.TotalPatientCount = totalPatientCount;
                    ViewBag.TotalTodayAppointmentsReceptionist = totalTodayAppointmentsReceptionist;
                    ViewBag.TotalWaitingAppointments = totalWaitingAppointments;
                    ViewBag.TotalCompletedAppointments = totalCompletedAppointments;

                    // Now you can use these values
                }
                else
                {
                    ViewBag.TotalPatientCount = totalPatientCount;
                    ViewBag.TotalTodayAppointmentsReceptionist = totalTodayAppointmentsReceptionist;
                    ViewBag.TotalWaitingAppointments = totalWaitingAppointments;
                    ViewBag.TotalCompletedAppointments = totalCompletedAppointments;
                }

                AppointmentList = AppointmentList.OrderByDescending(a => a.tokenIteger).ToList();
                ViewBag.USERS = UserList;
                ViewBag.Message = SearchDate.Date;
                ViewBag.Type = "Appointments";
                //ViewBag.TotalPatientCount = totalPatientCount;
                //ViewBag.TotalTodayAppointmentsReceptionist = totalTodayAppointmentsReceptionist;
                //ViewBag.TotalWaitingAppointments = totalWaitingAppointments;
                //ViewBag.TotalCompletedAppointments = totalCompletedAppointments;
                ViewBag.UserDetail = UserDetail;
                ViewBag.RolesString = RolesString;
                //Thread.Sleep(1000);
                //ModelState.AddModelError("", "-- " + Session["sessionid"].ToString() + " -- " + "Succesfully Found Post records.");
                var model = new AppointmentPatientViewModel
                {
                    Patients = PatientList, // Replace with actual data fetching logic
                    Appointments = AppointmentList
                };
                AppointmentPatientList.Add(model);
                return View(AppointmentPatientList);

                


            }
            catch (Exception ex)
            {
                ViewBag.USERS = UserList;
                ViewBag.ErrorMessage = ex.Message;
                ViewBag.Message = SearchDate.Date;
                ViewBag.Type = "Appointments";
                ViewBag.TotalPatientCount = totalPatientCount;
                ViewBag.TotalTodayAppointmentsReceptionist = totalTodayAppointmentsReceptionist;
                ViewBag.TotalWaitingAppointments = totalWaitingAppointments;
                ViewBag.TotalCompletedAppointments = totalCompletedAppointments;
                ViewBag.UserDetail = UserDetail;
                ViewBag.RolesString = RolesString;
                ModelState.AddModelError("", ex.Message);
                var model = new AppointmentPatientViewModel
                {
                    Patients = PatientList, // Replace with actual data fetching logic
                    Appointments = AppointmentList
                };
                AppointmentPatientList.Add(model);


                return View(AppointmentPatientList);

                
            }

        }

        //This function is called by ajax for actions TodayPatients,AllPatients
        public async Task<ActionResult> Search(string startdate, string Type, string Search)
        {
            List<Appointment> AppointmentList = new List<Appointment>();
            List<Patient> PatientList = new List<Patient>();
            List<AppointmentPatientViewModel> AppointmentPatientList = new List<AppointmentPatientViewModel>();

            List<SelectListItem> diseases = new List<SelectListItem>() {
                        new SelectListItem {
                            Text = "General -(fever/general acute complaints)", Value = "General"
                        },
                        new SelectListItem {
                            Text = "ENT (ear nose throat)", Value = "ENT(ear nose throat)"
                        },
                        new SelectListItem {
                            Text = "CNS (central nervous system", Value = "CNS(central nervous system)"
                        },
                        new SelectListItem {
                            Text = "DERMATO (skin/hair)", Value = "DERMATO./SKIN/hair"
                        },
                        new SelectListItem {
                            Text = "GIT (gastro intestinal track)", Value = "GIT(gastro intestinal track)"
                        },
                        new SelectListItem {
                            Text = "SURGERY", Value = "SURGERY"
                        },
                        new SelectListItem {
                            Text = "TUMORS/ONCOLOGY", Value = "TUMOURS/ONCOLOGY"
                        },
                        new SelectListItem {
                            Text = "HAEMATOLOGY (blood)", Value = "HAEMATOLOGY(blood)"
                        },
                        new SelectListItem {
                            Text = "ORTHOPADICS (bones & muscles)", Value = "ORTHOPADICS(bones & muscles)"
                        },
                        new SelectListItem {
                            Text = "PAEDIATRIC (child)", Value = "paediatric"
                        },
                        new SelectListItem {
                            Text = "OPTHALMOLOGISTS (EYE)", Value = "opthalmologists(eye)"
                        },
                        new SelectListItem {
                            Text = "CVS (cardio vascular system/heart)", Value = "CVS(cardio vascular system/heart)"
                        },
                        new SelectListItem {
                            Text = "ENDOCRINOLOGY", Value = "ENDOCRINOLOGY"
                        },
                        new SelectListItem {
                            Text = "GYNAECOLOGY & OBS", Value = "GYNAECOLOGY & OBS"
                        },
                        new SelectListItem {
                            Text = "GENETICS", Value = "GENETICS"
                        },
                    };
            ViewBag.DISEASES = diseases;

            List<SelectListItem> referby = new List<SelectListItem>() {
                new SelectListItem {
                    Text = "PATIENT", Value = "PATIENT"
                },
                new SelectListItem {
                    Text = "TV ADV", Value = "TV ADV"
                },
                new SelectListItem {
                    Text = "SOCIAL MEDIA", Value = "SOCIAL MEDIA"
                },
                new SelectListItem {
                    Text = "WALL PAINTING", Value = "WALL PAINTING"
                },
                new SelectListItem {
                    Text = "POSTERS", Value = "POSTERS"
                },
                new SelectListItem {
                    Text = "INTERNET SEARCH", Value = "INTERNET SEARCH"
                },
                new SelectListItem {
                    Text = "CALLS", Value = "CALLS"
                },
                new SelectListItem {
                    Text = "DOCTOR", Value = "DOCTOR"
                },
                new SelectListItem {
                    Text = "HOSPITAL", Value = "HOSPITAL"
                },
                new SelectListItem {
                    Text = "MAIL/MSG", Value = "MAILMSG"
                },
                new SelectListItem {
                    Text = "RANDOM", Value = "RANDOM"
                },

            };
            ViewBag.REFERBYS = referby;

            DateTime SearchDate;
            if (startdate == null || startdate == "")
            {
                SearchDate = Convert.ToDateTime(DateTime.UtcNow);
            }
            else
            {
                SearchDate = Convert.ToDateTime(startdate);
                SearchDate = DateTime.SpecifyKind(SearchDate, DateTimeKind.Utc);
            }

            SearchDate = SearchDate.Date;
            Timestamp SearchDateFrom = Timestamp.FromDateTime(SearchDate.Date.AddHours(-5).AddMinutes(-30));
            Timestamp SearchDateTo = Timestamp.FromDateTime(SearchDate.Date.AddHours(-5).AddMinutes(-30).AddDays(1));

            string ClinicMobileNumber = "";
            string ClinicFirebaseDocumentId = "";
            string[] Roles = null;
            string UserMobileNumber = "";

            string msg = "";

            var authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            string savedString = "";

            if (authCookie != null)
            {
                var ticket = FormsAuthentication.Decrypt(authCookie.Value);
                if (ticket != null)
                {
                    savedString = ticket.Name; // Get the stored string
                    ClinicMobileNumber = savedString.Split('|')[3];
                    ClinicFirebaseDocumentId = savedString.Split('|')[4];
                    Roles = savedString.Split('|')[2].Split('_');
                    UserMobileNumber = savedString.Split('|')[0];
                    msg = savedString;
                }
            }
            else
            {
                savedString = "Cookie is Blank";
                msg = savedString;
            }



            if (Session["sessionid"] == null)
            {
                Session["sessionid"] = "empty";
                msg = "Session empty";
            }
            else
            {
                msg = Session["sessionid"].ToString();
            }

            if (Type == "Patients")
            {

                List<SelectListItem> UserList = new List<SelectListItem>();

                try
                {

                    //string Path = AppDomain.CurrentDomain.BaseDirectory + @"greenpaperdev-firebase-adminsdk-8k2y5-fb46e63414.json";
                    //Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path);
                    //FirestoreDb db = FirestoreDb.Create("greenpaperdev");


                    //Query Qref = db.Collection("Students").WhereEqualTo("StudentName","Suvidhi");
                    Query Qref = _db.Collection("clinics").WhereEqualTo("clinicmobilenumber", ClinicMobileNumber);
                    QuerySnapshot snap = await Qref.GetSnapshotAsync();


                    foreach (DocumentSnapshot docsnap in snap)
                    {
                        #region to get user documentid
                        QuerySnapshot snapUser = await docsnap.Reference.Collection("user").WhereEqualTo("mobile_number", UserMobileNumber).GetSnapshotAsync();
                        DocumentSnapshot docUser = snapUser.Documents[0];

                        string userId = docUser.Id;
                        #endregion



                        Clinic clinic = docsnap.ConvertTo<Clinic>();

                        #region Code to get doctors list for refer to
                        QuerySnapshot snapUsersDoctors = await docsnap.Reference.Collection("user").WhereArrayContainsAny("user_roles", new string[] { "Doctor", "Chemist" }).GetSnapshotAsync();
                        foreach (DocumentSnapshot docsnapUsers in snapUsersDoctors)
                        {
                            SelectListItem user = new SelectListItem();
                            user.Text = docsnapUsers.GetValue<string>("name");
                            user.Value = docsnapUsers.Id;
                            if (docsnapUsers.Exists)
                            {
                                try
                                {
                                    if (docsnapUsers.GetValue<bool>("user_deactivated") == false)
                                    {
                                        UserList.Add(user);
                                    }
                                }
                                catch
                                {
                                    UserList.Add(user);
                                }


                            }
                        }



                        #endregion Code to Code to get doctors list for refer to

                        if (Search != null && Search != "")
                        {
                            string searchInputToLower = Search.ToLower();
                            string searchInputToUpper = Search.ToUpper();
                            QuerySnapshot snapPatientId = await docsnap.Reference.Collection("patientList").OrderBy("patient_id").StartAt(searchInputToUpper).EndAt(searchInputToUpper + "\uf8ff").GetSnapshotAsync();

                            if (snapPatientId.Count == 0)
                            {
                                QuerySnapshot snapPatientMobileNumber = await docsnap.Reference.Collection("patientList").OrderBy("patient_mobile_number").StartAt(searchInputToLower).EndAt(searchInputToLower + "\uf8ff").GetSnapshotAsync();
                                if (snapPatientMobileNumber.Count == 0)
                                {
                                    QuerySnapshot snapPatientName = await docsnap.Reference.Collection("patientList").OrderBy("patient_name").StartAt(searchInputToLower).EndAt(searchInputToLower + "\uf8ff").GetSnapshotAsync();
                                    if (snapPatientName.Count > 0)
                                    {
                                        foreach (DocumentSnapshot docsnap2 in snapPatientName)
                                        {
                                            Patient patient = docsnap2.ConvertTo<Patient>();
                                            patient.id = docsnap2.Id;

                                            if (docsnap2.Exists)
                                            {
                                                patient.clinic_name = clinic.clinicname;
                                                PatientList.Add(patient);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    foreach (DocumentSnapshot docsnap2 in snapPatientMobileNumber)
                                    {
                                        Patient patient = docsnap2.ConvertTo<Patient>();
                                        patient.id = docsnap2.Id;

                                        if (docsnap2.Exists)
                                        {
                                            patient.clinic_name = clinic.clinicname;
                                            PatientList.Add(patient);
                                        }
                                    }
                                }

                            }
                            else
                            {
                                foreach (DocumentSnapshot docsnap2 in snapPatientId)
                                {
                                    Patient patient = docsnap2.ConvertTo<Patient>();
                                    patient.id = docsnap2.Id;

                                    if (docsnap2.Exists)
                                    {
                                        patient.clinic_name = clinic.clinicname;
                                        PatientList.Add(patient);
                                    }
                                }
                            }
                        }
                        else
                        {

                            QuerySnapshot snap2 = await docsnap.Reference.Collection("patientList").OrderByDescending("patient_id").Limit(100).GetSnapshotAsync();
                            if (snap2.Count > 0)
                            {
                                foreach (DocumentSnapshot docsnap2 in snap2)
                                {
                                    Patient patient = docsnap2.ConvertTo<Patient>();
                                    patient.id = docsnap2.Id;

                                    if (docsnap2.Exists)
                                    {
                                        patient.clinic_name = clinic.clinicname;
                                        PatientList.Add(patient);
                                    }
                                }
                            }
                        }
                    }
                    if (UserList == null || !UserList.Any())
                    {
                        UserList.Add(new SelectListItem
                        {
                            Text = "No users available",
                            Value = "0" // You can assign a specific value for your custom item
                        });
                    }
                    ViewBag.USERS = UserList;
                    ViewBag.ErrorMessage = msg;
                    ViewBag.Message = SearchDate.Date;
                    ViewBag.Type = "Patients";

                    //ModelState.AddModelError("", "-- " + Session["sessionid"].ToString() + " --" + "Succesfully Found Get records.");
                    var model = new AppointmentPatientViewModel
                    {
                        Patients = PatientList, // Replace with actual data fetching logic
                        Appointments = AppointmentList
                    };
                    AppointmentPatientList.Add(model);
                    //return View(AppointmentPatientList);
                    return PartialView("_PatientListPartial", AppointmentPatientList);
                }
                catch (Exception ex)
                {

                    UserList.Add(new SelectListItem
                    {
                        Text = "in catch error",
                        Value = "0" // You can assign a specific value for your custom item
                    });


                    ViewBag.USERS = UserList;
                    ViewBag.ErrorMessage = ex.Message;
                    ViewBag.Message = SearchDate.Date;
                    ViewBag.Type = "Patients";

                    ModelState.AddModelError("", ex.Message + " in catch");
                    var model = new AppointmentPatientViewModel
                    {
                        Patients = PatientList, // Replace with actual data fetching logic
                        Appointments = AppointmentList

                    };
                    AppointmentPatientList.Add(model);
                    //return View(AppointmentPatientList);
                    return PartialView("_PatientListPartial", AppointmentPatientList);

                }

            }
            else if (Type == "Appointments")
            {
                List<string> statusList = new List<string>();
                try
                {
                    Query Qref = _db.Collection("clinics").WhereEqualTo("clinicmobilenumber", ClinicMobileNumber);
                    QuerySnapshot snapClinics = await Qref.GetSnapshotAsync();
                    string WhoFirst = "Cashier";
                    int i = 0;
                    foreach (DocumentSnapshot docsnapClinics in snapClinics)
                    {
                        Clinic clinic = docsnapClinics.ConvertTo<Clinic>();

                        #region to get user documentid
                        QuerySnapshot snapUser = await docsnapClinics.Reference.Collection("user").WhereEqualTo("mobile_number", UserMobileNumber).GetSnapshotAsync();
                        DocumentSnapshot docUser = snapUser.Documents[0];

                        string userId = docUser.Id;
                        #endregion

                        #region Get WhosFirst
                        QuerySnapshot snapSettings = await docsnapClinics.Reference.Collection("settings").Limit(1).GetSnapshotAsync();
                        if (snapSettings.Count > 0)
                        {
                            DocumentSnapshot docSnapSettings = snapSettings.Documents[0];

                            if (docSnapSettings.Exists)
                            {
                                WhoFirst = docSnapSettings.GetValue<string>("whofirst");
                            }
                        }
                        #endregion

                        #region Today Patient count and Appointment logic
                        if(Roles.Contains("Receptionist"))
                        {
                            QuerySnapshot snapTodayAppointments = await docsnapClinics.Reference.Collection("appointments").WhereGreaterThanOrEqualTo("raisedDate", SearchDateFrom).WhereLessThan("raisedDate", SearchDateTo).WhereEqualTo("receptionist", userId).GetSnapshotAsync();

                            foreach (DocumentSnapshot docsnapAppointments in snapTodayAppointments)
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
                                    appointment.status = docsnapAppointments.GetValue<string>("status");
                                    appointment.statusCashier = docsnapAppointments.GetValue<string>("statusCashier");
                                    appointment.statusChemist = docsnapAppointments.GetValue<string>("statusChemist");
                                    //appointment.status = !string.IsNullOrEmpty(docsnapAppointments.GetValue<string>("status")) ? "Doct:" + docsnapAppointments.GetValue<string>("status") : "";
                                    //appointment.statusCashier = !string.IsNullOrEmpty(docsnapAppointments.GetValue<string>("statusCashier")) ? "Cash:" + docsnapAppointments.GetValue<string>("statusCashier") : "";
                                    //appointment.statusChemist = !string.IsNullOrEmpty(docsnapAppointments.GetValue<string>("statusChemist")) ? "Chem:" + docsnapAppointments.GetValue<string>("statusChemist") : "";
                                    try { appointment.tokenIteger = Convert.ToInt32(docsnapAppointments.GetValue<string>("token")); }
                                    catch { appointment.tokenIteger = i + 1; }

                                    AppointmentList.Add(appointment);
                                }
                            }

                        }

                        else if (Roles.Contains("Doctor"))
                        {
                            QuerySnapshot snapTodayAppointments = await docsnapClinics.Reference.Collection("appointments").WhereGreaterThanOrEqualTo("raisedDate", SearchDateFrom).WhereLessThan("raisedDate", SearchDateTo).WhereEqualTo("referTo", userId).GetSnapshotAsync();

                            foreach (DocumentSnapshot docsnapAppointments in snapTodayAppointments)
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
                                    appointment.status = docsnapAppointments.GetValue<string>("status");
                                    appointment.statusCashier = docsnapAppointments.GetValue<string>("statusCashier");
                                    appointment.statusChemist = docsnapAppointments.GetValue<string>("statusChemist");
                                    //appointment.status = !string.IsNullOrEmpty(docsnapAppointments.GetValue<string>("status")) ? "Doct:" + docsnapAppointments.GetValue<string>("status") : "";
                                    //appointment.statusCashier = !string.IsNullOrEmpty(docsnapAppointments.GetValue<string>("statusCashier")) ? "Cash:" + docsnapAppointments.GetValue<string>("statusCashier") : "";
                                    //appointment.statusChemist = !string.IsNullOrEmpty(docsnapAppointments.GetValue<string>("statusChemist")) ? "Chem:" + docsnapAppointments.GetValue<string>("statusChemist") : "";
                                    try { appointment.tokenIteger = Convert.ToInt32(docsnapAppointments.GetValue<string>("token")); }
                                    catch { appointment.tokenIteger = i + 1; }

                                    AppointmentList.Add(appointment);
                                }
                            }
                        }

                        else if (Roles.Contains("Chemist"))
                        {
                            QuerySnapshot snapTodayAppointments = await docsnapClinics.Reference.Collection("appointments").WhereGreaterThanOrEqualTo("raisedDate", SearchDateFrom).WhereLessThan("raisedDate", SearchDateTo).WhereEqualTo("status", "Completed").GetSnapshotAsync();

                            foreach (DocumentSnapshot docsnapAppointments in snapTodayAppointments)
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
                                    appointment.status = docsnapAppointments.GetValue<string>("status");
                                    appointment.statusCashier = docsnapAppointments.GetValue<string>("statusCashier");
                                    appointment.statusChemist = docsnapAppointments.GetValue<string>("statusChemist");
                                    //appointment.status = !string.IsNullOrEmpty(docsnapAppointments.GetValue<string>("status")) ? "Doct:" + docsnapAppointments.GetValue<string>("status") : "";
                                    //appointment.statusCashier = !string.IsNullOrEmpty(docsnapAppointments.GetValue<string>("statusCashier")) ? "Cash:" + docsnapAppointments.GetValue<string>("statusCashier") : "";
                                    //appointment.statusChemist = !string.IsNullOrEmpty(docsnapAppointments.GetValue<string>("statusChemist")) ? "Chem:" + docsnapAppointments.GetValue<string>("statusChemist") : "";
                                    try { appointment.tokenIteger = Convert.ToInt32(docsnapAppointments.GetValue<string>("token")); }
                                    catch { appointment.tokenIteger = i + 1; }

                                    AppointmentList.Add(appointment);
                                }
                            }
                        }

                        else if (Roles.Contains("Cashier"))
                        {
                            QuerySnapshot snapTodayAppointments = await docsnapClinics.Reference.Collection("appointments").WhereGreaterThanOrEqualTo("raisedDate", SearchDateFrom).WhereLessThan("raisedDate", SearchDateTo).WhereEqualTo("statusChemist", "Completed").GetSnapshotAsync();

                            foreach (DocumentSnapshot docsnapAppointments in snapTodayAppointments)
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
                                    appointment.status = docsnapAppointments.GetValue<string>("status");
                                    appointment.statusCashier = docsnapAppointments.GetValue<string>("statusCashier");
                                    appointment.statusChemist = docsnapAppointments.GetValue<string>("statusChemist");
                                    //appointment.status = !string.IsNullOrEmpty(docsnapAppointments.GetValue<string>("status")) ? "Doct:" + docsnapAppointments.GetValue<string>("status") : "";
                                    //appointment.statusCashier = !string.IsNullOrEmpty(docsnapAppointments.GetValue<string>("statusCashier")) ? "Cash:" + docsnapAppointments.GetValue<string>("statusCashier") : "";
                                    //appointment.statusChemist = !string.IsNullOrEmpty(docsnapAppointments.GetValue<string>("statusChemist")) ? "Chem:" + docsnapAppointments.GetValue<string>("statusChemist") : "";
                                    try { appointment.tokenIteger = Convert.ToInt32(docsnapAppointments.GetValue<string>("token")); }
                                    catch { appointment.tokenIteger = i + 1; }

                                    AppointmentList.Add(appointment);
                                }
                            }
                        }
                        #endregion


                    }
                    AppointmentList = AppointmentList.OrderByDescending(a => a.tokenIteger).ToList();
                    ViewBag.Message = SearchDate.Date;
                    ViewBag.Type = "Appointments";
                    //Thread.Sleep(1000);
                    //ModelState.AddModelError("", "-- " + Session["sessionid"].ToString() + " -- " + "Succesfully Found Post records.");
                    var model = new AppointmentPatientViewModel
                    {
                        Patients = PatientList, // Replace with actual data fetching logic
                        Appointments = AppointmentList
                    };
                    AppointmentPatientList.Add(model);
                    //return View(AppointmentPatientList);
                    return PartialView("_AppointmentListPartial", AppointmentPatientList);


                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                    var model = new AppointmentPatientViewModel
                    {
                        Patients = PatientList, // Replace with actual data fetching logic
                        Appointments = AppointmentList
                    };
                    AppointmentPatientList.Add(model);
                    //return View(AppointmentPatientList);

                    return PartialView("_AppointmentListPartial", AppointmentPatientList);
                }


            }
            else if (Type == "WaitingAppointments")
            {
                List<string> statusList = new List<string>();
                try
                {
                    Query Qref = _db.Collection("clinics").WhereEqualTo("clinicmobilenumber", ClinicMobileNumber);
                    QuerySnapshot snapClinics = await Qref.GetSnapshotAsync();
                    string WhoFirst = "Cashier";
                    int i = 0;
                    foreach (DocumentSnapshot docsnapClinics in snapClinics)
                    {
                        Clinic clinic = docsnapClinics.ConvertTo<Clinic>();

                        #region to get user documentid
                        QuerySnapshot snapUser = await docsnapClinics.Reference.Collection("user").WhereEqualTo("mobile_number", UserMobileNumber).GetSnapshotAsync();
                        DocumentSnapshot docUser = snapUser.Documents[0];

                        string userId = docUser.Id;
                        #endregion

                        #region Get WhosFirst
                        QuerySnapshot snapSettings = await docsnapClinics.Reference.Collection("settings").Limit(1).GetSnapshotAsync();
                        if (snapSettings.Count > 0)
                        {
                            DocumentSnapshot docSnapSettings = snapSettings.Documents[0];

                            if (docSnapSettings.Exists)
                            {
                                WhoFirst = docSnapSettings.GetValue<string>("whofirst");
                            }
                        }
                        #endregion

                        #region All Patient count
                        QuerySnapshot snapPatientList = await docsnapClinics.Reference.Collection("patientList").GetSnapshotAsync();

                        #endregion

                        #region Completed Count Logic
                        if (Roles.Contains("Doctor") || Roles.Contains("Chemist") || Roles.Contains("Cashier"))
                        {
                            if (Roles.Contains("Doctor"))
                            {
                                QuerySnapshot snapCompletedAppointments = await docsnapClinics.Reference.Collection("appointments").WhereGreaterThanOrEqualTo("raisedDate", SearchDateFrom).WhereLessThan("raisedDate", SearchDateTo).WhereEqualTo("status", "Completed").WhereEqualTo("referTo", userId).GetSnapshotAsync();

                            }
                            else if (Roles.Contains("Chemist"))
                            {
                                QuerySnapshot snapCompletedAppointments = await docsnapClinics.Reference.Collection("appointments").WhereGreaterThanOrEqualTo("raisedDate", SearchDateFrom).WhereLessThan("raisedDate", SearchDateTo).WhereEqualTo("statusChemist", "Completed").WhereEqualTo("referTo", userId).GetSnapshotAsync();

                            } 
                            else if (Roles.Contains("Cashier"))
                            {
                                QuerySnapshot snapCompletedAppointments = await docsnapClinics.Reference.Collection("appointments").WhereGreaterThanOrEqualTo("raisedDate", SearchDateFrom).WhereLessThan("raisedDate", SearchDateTo).WhereEqualTo("statusCashier", "Completed").WhereEqualTo("referTo", userId).GetSnapshotAsync();

                            }
                        }
                        #endregion

                        #region Today Patient count logic
                        if (Roles.Contains("Doctor"))
                        {
                            QuerySnapshot snapAppointments = await docsnapClinics.Reference.Collection("appointments").WhereGreaterThanOrEqualTo("raisedDate", SearchDateFrom).WhereLessThan("raisedDate", SearchDateTo).WhereEqualTo("referTo", userId).GetSnapshotAsync();

                        }
                        else if (Roles.Contains("Receptionist"))
                        {
                            QuerySnapshot snapAppointments = await docsnapClinics.Reference.Collection("appointments").WhereGreaterThanOrEqualTo("raisedDate", SearchDateFrom).WhereLessThan("raisedDate", SearchDateTo).WhereEqualTo("receptionist", userId).GetSnapshotAsync();

                        }
                        #endregion

                        #region Waiting Count and appointment Logic
                        if (Roles.Contains("Doctor") || Roles.Contains("Chemist") || Roles.Contains("Cashier")|| Roles.Contains("Receptionist"))
                        {
                            if (Roles.Contains("Receptionist"))
                            {
                                QuerySnapshot snapWaitingAppointments = await docsnapClinics.Reference.Collection("appointments").WhereGreaterThanOrEqualTo("raisedDate", SearchDateFrom).WhereLessThan("raisedDate", SearchDateTo).WhereEqualTo("receptionist", userId).GetSnapshotAsync();


                                foreach (DocumentSnapshot docsnapAppointments in snapWaitingAppointments)
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
                                        appointment.status = docsnapAppointments.GetValue<string>("status");
                                        appointment.statusCashier = docsnapAppointments.GetValue<string>("statusCashier");
                                        appointment.statusChemist = docsnapAppointments.GetValue<string>("statusChemist");
                                        //appointment.status = !string.IsNullOrEmpty(docsnapAppointments.GetValue<string>("status")) ? "Doct:" + docsnapAppointments.GetValue<string>("status") : "";
                                        //appointment.statusCashier = !string.IsNullOrEmpty(docsnapAppointments.GetValue<string>("statusCashier")) ? "Cash:" + docsnapAppointments.GetValue<string>("statusCashier") : "";
                                        //appointment.statusChemist = !string.IsNullOrEmpty(docsnapAppointments.GetValue<string>("statusChemist")) ? "Chem:" + docsnapAppointments.GetValue<string>("statusChemist") : "";
                                        try { appointment.tokenIteger = Convert.ToInt32(docsnapAppointments.GetValue<string>("token")); }
                                        catch { appointment.tokenIteger = i + 1; }

                                        AppointmentList.Add(appointment);
                                    }
                                }
                            } 
                            else if (Roles.Contains("Doctor"))
                            {
                                QuerySnapshot snapWaitingAppointments = await docsnapClinics.Reference.Collection("appointments").WhereGreaterThanOrEqualTo("raisedDate", SearchDateFrom).WhereLessThan("raisedDate", SearchDateTo).WhereEqualTo("status", "Waiting").WhereEqualTo("referTo", userId).GetSnapshotAsync();


                                foreach (DocumentSnapshot docsnapAppointments in snapWaitingAppointments)
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
                                        appointment.status = docsnapAppointments.GetValue<string>("status");
                                        appointment.statusCashier = docsnapAppointments.GetValue<string>("statusCashier");
                                        appointment.statusChemist = docsnapAppointments.GetValue<string>("statusChemist");
                                        //appointment.status = !string.IsNullOrEmpty(docsnapAppointments.GetValue<string>("status")) ? "Doct:" + docsnapAppointments.GetValue<string>("status") : "";
                                        //appointment.statusCashier = !string.IsNullOrEmpty(docsnapAppointments.GetValue<string>("statusCashier")) ? "Cash:" + docsnapAppointments.GetValue<string>("statusCashier") : "";
                                        //appointment.statusChemist = !string.IsNullOrEmpty(docsnapAppointments.GetValue<string>("statusChemist")) ? "Chem:" + docsnapAppointments.GetValue<string>("statusChemist") : "";
                                        try { appointment.tokenIteger = Convert.ToInt32(docsnapAppointments.GetValue<string>("token")); }
                                        catch { appointment.tokenIteger = i + 1; }

                                        AppointmentList.Add(appointment);
                                    }
                                }
                            }
                            else if (Roles.Contains("Chemist"))
                            {
                                QuerySnapshot snapWaitingAppointments = await docsnapClinics.Reference.Collection("appointments").WhereGreaterThanOrEqualTo("raisedDate", SearchDateFrom).WhereLessThan("raisedDate", SearchDateTo).WhereEqualTo("status", "Completed").GetSnapshotAsync();


                                foreach (DocumentSnapshot docsnapAppointments in snapWaitingAppointments)
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
                                        appointment.status = docsnapAppointments.GetValue<string>("status");
                                        appointment.statusCashier = docsnapAppointments.GetValue<string>("statusCashier");
                                        appointment.statusChemist = docsnapAppointments.GetValue<string>("statusChemist");
                                        //appointment.status = !string.IsNullOrEmpty(docsnapAppointments.GetValue<string>("status")) ? "Doct:" + docsnapAppointments.GetValue<string>("status") : "";
                                        //appointment.statusCashier = !string.IsNullOrEmpty(docsnapAppointments.GetValue<string>("statusCashier")) ? "Cash:" + docsnapAppointments.GetValue<string>("statusCashier") : "";
                                        //appointment.statusChemist = !string.IsNullOrEmpty(docsnapAppointments.GetValue<string>("statusChemist")) ? "Chem:" + docsnapAppointments.GetValue<string>("statusChemist") : "";
                                        try { appointment.tokenIteger = Convert.ToInt32(docsnapAppointments.GetValue<string>("token")); }
                                        catch { appointment.tokenIteger = i + 1; }

                                        AppointmentList.Add(appointment);
                                    }
                                }
                            }
                            else if (Roles.Contains("Cashier"))
                            {
                                QuerySnapshot snapWaitingAppointments = await docsnapClinics.Reference.Collection("appointments").WhereGreaterThanOrEqualTo("raisedDate", SearchDateFrom).WhereLessThan("raisedDate", SearchDateTo).WhereEqualTo("statusChemist", "Completed").GetSnapshotAsync();

                                foreach (DocumentSnapshot docsnapAppointments in snapWaitingAppointments)
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
                                        appointment.status = docsnapAppointments.GetValue<string>("status");
                                        appointment.statusCashier = docsnapAppointments.GetValue<string>("statusCashier");
                                        appointment.statusChemist = docsnapAppointments.GetValue<string>("statusChemist");
                                        //appointment.status = !string.IsNullOrEmpty(docsnapAppointments.GetValue<string>("status")) ? "Doct:" + docsnapAppointments.GetValue<string>("status") : "";
                                        //appointment.statusCashier = !string.IsNullOrEmpty(docsnapAppointments.GetValue<string>("statusCashier")) ? "Cash:" + docsnapAppointments.GetValue<string>("statusCashier") : "";
                                        //appointment.statusChemist = !string.IsNullOrEmpty(docsnapAppointments.GetValue<string>("statusChemist")) ? "Chem:" + docsnapAppointments.GetValue<string>("statusChemist") : "";
                                        try { appointment.tokenIteger = Convert.ToInt32(docsnapAppointments.GetValue<string>("token")); }
                                        catch { appointment.tokenIteger = i + 1; }

                                        AppointmentList.Add(appointment);
                                    }
                                }
                            }

                        }
                        #endregion


                    }
                    AppointmentList = AppointmentList.OrderByDescending(a => a.tokenIteger).ToList();
                    ViewBag.Message = SearchDate.Date;
                    ViewBag.Type = "Appointments";

                    //Thread.Sleep(1000);
                    //ModelState.AddModelError("", "-- " + Session["sessionid"].ToString() + " -- " + "Succesfully Found Post records.");
                    var model = new AppointmentPatientViewModel
                    {
                        Patients = PatientList, // Replace with actual data fetching logic
                        Appointments = AppointmentList
                    };
                    AppointmentPatientList.Add(model);
                    //return View(AppointmentPatientList);
                    return PartialView("_AppointmentListPartial", AppointmentPatientList);


                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                    var model = new AppointmentPatientViewModel
                    {
                        Patients = PatientList, // Replace with actual data fetching logic
                        Appointments = AppointmentList
                    };
                    AppointmentPatientList.Add(model);
                    //return View(AppointmentPatientList);

                    return PartialView("_AppointmentListPartial", AppointmentPatientList);
                }


            }
            else if (Type == "CompletedAppointments")
            {
                List<string> statusList = new List<string>();
                try
                {
                    Query Qref = _db.Collection("clinics").WhereEqualTo("clinicmobilenumber", ClinicMobileNumber);
                    QuerySnapshot snapClinics = await Qref.GetSnapshotAsync();
                    string WhoFirst = "Cashier";
                    int i = 0;
                    foreach (DocumentSnapshot docsnapClinics in snapClinics)
                    {
                        Clinic clinic = docsnapClinics.ConvertTo<Clinic>();

                        #region to get user documentid
                        QuerySnapshot snapUser = await docsnapClinics.Reference.Collection("user").WhereEqualTo("mobile_number", UserMobileNumber).GetSnapshotAsync();
                        DocumentSnapshot docUser = snapUser.Documents[0];

                        string userId = docUser.Id;
                        #endregion

                        #region Get WhosFirst
                        QuerySnapshot snapSettings = await docsnapClinics.Reference.Collection("settings").Limit(1).GetSnapshotAsync();
                        if (snapSettings.Count > 0)
                        {
                            DocumentSnapshot docSnapSettings = snapSettings.Documents[0];

                            if (docSnapSettings.Exists)
                            {
                                WhoFirst = docSnapSettings.GetValue<string>("whofirst");
                            }
                        }
                        #endregion



                        #region Completed Count Logic
                        if (Roles.Contains("Doctor") || Roles.Contains("Chemist") || Roles.Contains("Cashier"))
                        {
                            if (Roles.Contains("Doctor"))
                            {
                                QuerySnapshot snapCompletedAppointments = await docsnapClinics.Reference.Collection("appointments").WhereGreaterThanOrEqualTo("raisedDate", SearchDateFrom).WhereLessThan("raisedDate", SearchDateTo).WhereEqualTo("status", "Completed").WhereEqualTo("referTo", userId).GetSnapshotAsync();


                                foreach (DocumentSnapshot docsnapAppointments in snapCompletedAppointments)
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
                                        appointment.status = docsnapAppointments.GetValue<string>("status");
                                        appointment.statusCashier = docsnapAppointments.GetValue<string>("statusCashier");
                                        appointment.statusChemist = docsnapAppointments.GetValue<string>("statusChemist");
                                        //appointment.status = !string.IsNullOrEmpty(docsnapAppointments.GetValue<string>("status")) ? "Doct:" + docsnapAppointments.GetValue<string>("status") : "";
                                        //appointment.statusCashier = !string.IsNullOrEmpty(docsnapAppointments.GetValue<string>("statusCashier")) ? "Cash:" + docsnapAppointments.GetValue<string>("statusCashier") : "";
                                        //appointment.statusChemist = !string.IsNullOrEmpty(docsnapAppointments.GetValue<string>("statusChemist")) ? "Chem:" + docsnapAppointments.GetValue<string>("statusChemist") : "";
                                        try { appointment.tokenIteger = Convert.ToInt32(docsnapAppointments.GetValue<string>("token")); }
                                        catch { appointment.tokenIteger = i + 1; }

                                        AppointmentList.Add(appointment);
                                    }
                                }
                            }
                            else if (Roles.Contains("Chemist"))
                            {
                                QuerySnapshot snapCompletedAppointments = await docsnapClinics.Reference.Collection("appointments").WhereGreaterThanOrEqualTo("raisedDate", SearchDateFrom).WhereLessThan("raisedDate", SearchDateTo).WhereEqualTo("statusChemist", "Completed").GetSnapshotAsync();


                                foreach (DocumentSnapshot docsnapAppointments in snapCompletedAppointments)
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
                                        appointment.status = docsnapAppointments.GetValue<string>("status");
                                        appointment.statusCashier = docsnapAppointments.GetValue<string>("statusCashier");
                                        appointment.statusChemist = docsnapAppointments.GetValue<string>("statusChemist");
                                        //appointment.status = !string.IsNullOrEmpty(docsnapAppointments.GetValue<string>("status")) ? "Doct:" + docsnapAppointments.GetValue<string>("status") : "";
                                        //appointment.statusCashier = !string.IsNullOrEmpty(docsnapAppointments.GetValue<string>("statusCashier")) ? "Cash:" + docsnapAppointments.GetValue<string>("statusCashier") : "";
                                        //appointment.statusChemist = !string.IsNullOrEmpty(docsnapAppointments.GetValue<string>("statusChemist")) ? "Chem:" + docsnapAppointments.GetValue<string>("statusChemist") : "";
                                        try { appointment.tokenIteger = Convert.ToInt32(docsnapAppointments.GetValue<string>("token")); }
                                        catch { appointment.tokenIteger = i + 1; }

                                        AppointmentList.Add(appointment);
                                    }
                                }
                            }
                            else if (Roles.Contains("Cashier"))
                            {
                                QuerySnapshot snapCompletedAppointments = await docsnapClinics.Reference.Collection("appointments").WhereGreaterThanOrEqualTo("raisedDate", SearchDateFrom).WhereLessThan("raisedDate", SearchDateTo).WhereEqualTo("statusCashier", "Completed").GetSnapshotAsync();


                                foreach (DocumentSnapshot docsnapAppointments in snapCompletedAppointments)
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
                                        appointment.status = docsnapAppointments.GetValue<string>("status");
                                        appointment.statusCashier = docsnapAppointments.GetValue<string>("statusCashier");
                                        appointment.statusChemist = docsnapAppointments.GetValue<string>("statusChemist");
                                        //appointment.status = !string.IsNullOrEmpty(docsnapAppointments.GetValue<string>("status")) ? "Doct:" + docsnapAppointments.GetValue<string>("status") : "";
                                        //appointment.statusCashier = !string.IsNullOrEmpty(docsnapAppointments.GetValue<string>("statusCashier")) ? "Cash:" + docsnapAppointments.GetValue<string>("statusCashier") : "";
                                        //appointment.statusChemist = !string.IsNullOrEmpty(docsnapAppointments.GetValue<string>("statusChemist")) ? "Chem:" + docsnapAppointments.GetValue<string>("statusChemist") : "";
                                        try { appointment.tokenIteger = Convert.ToInt32(docsnapAppointments.GetValue<string>("token")); }
                                        catch { appointment.tokenIteger = i + 1; }

                                        AppointmentList.Add(appointment);
                                    }
                                }
                            }
                        }
                        #endregion
                    }
                    AppointmentList = AppointmentList.OrderByDescending(a => a.tokenIteger).ToList();
                    ViewBag.Message = SearchDate.Date;
                    ViewBag.Type = "Appointments";

                    //Thread.Sleep(1000);
                    //ModelState.AddModelError("", "-- " + Session["sessionid"].ToString() + " -- " + "Succesfully Found Post records.");
                    var model = new AppointmentPatientViewModel
                    {
                        Patients = PatientList, // Replace with actual data fetching logic
                        Appointments = AppointmentList
                    };
                    AppointmentPatientList.Add(model);
                    //return View(AppointmentPatientList);
                    return PartialView("_AppointmentListPartial", AppointmentPatientList);


                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                    var model = new AppointmentPatientViewModel
                    {
                        Patients = PatientList, // Replace with actual data fetching logic
                        Appointments = AppointmentList
                    };
                    AppointmentPatientList.Add(model);
                    //return View(AppointmentPatientList);

                    return PartialView("_AppointmentListPartial", AppointmentPatientList);
                }


            }
            else
            {
                ViewBag.Message = SearchDate.Date;
                ViewBag.Type = "Appointments";

                //Thread.Sleep(1000);
                //ModelState.AddModelError("", "-- " + Session["sessionid"].ToString() + " -- " + "Succesfully Found Post records.");
                var model = new AppointmentPatientViewModel
                {
                    Patients = PatientList, // Replace with actual data fetching logic
                    Appointments = AppointmentList
                };
                AppointmentPatientList.Add(model);
                //return View(AppointmentPatientList);
                return PartialView("_AppointmentListPartial", AppointmentPatientList);
            }
        }

        //This function is called by ajax for getting TodayAppointmentsCount,AllPatientsCount,WaitingAppointmentsCounts,CompletedAppointmentsCounts
        public async Task<JsonResult> GetCounts(string startdate)
        {
            int totalPatientCount = 0;
            int totalTodayAppointmentsReceptionist = 0;
            int totalWaitingAppointments = 0;
            int totalCompletedAppointments = 0;

            DateTime SearchDate;
            if (startdate == null || startdate == "")
            {
                SearchDate = Convert.ToDateTime(DateTime.UtcNow);
            }
            else
            {
                SearchDate = Convert.ToDateTime(startdate);
                SearchDate = DateTime.SpecifyKind(SearchDate, DateTimeKind.Utc);
            }

            SearchDate = SearchDate.Date;
            Timestamp SearchDateFrom = Timestamp.FromDateTime(SearchDate.Date.AddHours(-5).AddMinutes(-30));
            Timestamp SearchDateTo = Timestamp.FromDateTime(SearchDate.Date.AddHours(-5).AddMinutes(-30).AddDays(1));

            string ClinicMobileNumber = "";
            string ClinicFirebaseDocumentId = "";
            string[] Roles = null;
            string UserMobileNumber = "";

            string msg = "";

            var authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            string savedString = "";

            if (authCookie != null)
            {
                var ticket = FormsAuthentication.Decrypt(authCookie.Value);
                if (ticket != null)
                {
                    savedString = ticket.Name; // Get the stored string
                    ClinicMobileNumber = savedString.Split('|')[3];
                    ClinicFirebaseDocumentId = savedString.Split('|')[4];
                    Roles = savedString.Split('|')[2].Split('_');
                    UserMobileNumber = savedString.Split('|')[0];
                    msg = savedString;
                }
            }
            else
            {
                savedString = "Cookie is Blank";
                msg = savedString;
            }

          
            try
            {
                Query Qref = _db.Collection("clinics").WhereEqualTo("clinicmobilenumber", ClinicMobileNumber);
                QuerySnapshot snapClinics = await Qref.GetSnapshotAsync();

                foreach (DocumentSnapshot docsnapClinics in snapClinics)
                {
                    Clinic clinic = docsnapClinics.ConvertTo<Clinic>();

                    #region to get user documentid
                    QuerySnapshot snapUser = await docsnapClinics.Reference.Collection("user").WhereEqualTo("mobile_number", UserMobileNumber).GetSnapshotAsync();
                    DocumentSnapshot docUser = snapUser.Documents[0];

                    string userId = docUser.Id;
                    #endregion

                    
                    #region All Patient count
                    QuerySnapshot snapPatientList = await docsnapClinics.Reference.Collection("patientList").GetSnapshotAsync();
                    totalPatientCount += snapPatientList.Count;
                    #endregion

                    #region Waiting Count Logic
                    if (Roles.Contains("Receptionist") || Roles.Contains("Doctor") || Roles.Contains("Chemist") || Roles.Contains("Cashier"))
                    {
                        if (Roles.Contains("Receptionist"))
                        {
                            QuerySnapshot snapWaitingAppointments = await docsnapClinics.Reference.Collection("appointments").WhereGreaterThanOrEqualTo("raisedDate", SearchDateFrom).WhereLessThan("raisedDate", SearchDateTo).WhereEqualTo("receptionist", userId).GetSnapshotAsync();
                            totalWaitingAppointments += snapWaitingAppointments.Count;
                        }
                        else if (Roles.Contains("Doctor"))
                        {
                            QuerySnapshot snapWaitingAppointments = await docsnapClinics.Reference.Collection("appointments").WhereGreaterThanOrEqualTo("raisedDate", SearchDateFrom).WhereLessThan("raisedDate", SearchDateTo).WhereEqualTo("status", "Waiting").WhereEqualTo("referTo", userId).GetSnapshotAsync();
                            totalWaitingAppointments += snapWaitingAppointments.Count;
                        }
                        else if (Roles.Contains("Chemist"))
                        {
                            QuerySnapshot snapWaitingAppointments = await docsnapClinics.Reference.Collection("appointments").WhereGreaterThanOrEqualTo("raisedDate", SearchDateFrom).WhereLessThan("raisedDate", SearchDateTo).WhereEqualTo("status", "Completed").GetSnapshotAsync();
                            totalWaitingAppointments += snapWaitingAppointments.Count;
                        }
                        else if (Roles.Contains("Cashier"))
                        {
                            QuerySnapshot snapWaitingAppointments = await docsnapClinics.Reference.Collection("appointments").WhereGreaterThanOrEqualTo("raisedDate", SearchDateFrom).WhereLessThan("raisedDate", SearchDateTo).WhereEqualTo("statusChemist", "Completed").GetSnapshotAsync();
                            totalWaitingAppointments += snapWaitingAppointments.Count;
                        }
                    }
                    #endregion

                    #region Completed Count Logic
                    if (Roles.Contains("Doctor") || Roles.Contains("Chemist") || Roles.Contains("Cashier"))
                    {
                        if (Roles.Contains("Doctor"))
                        {
                            QuerySnapshot snapCompletedAppointments = await docsnapClinics.Reference.Collection("appointments").WhereGreaterThanOrEqualTo("raisedDate", SearchDateFrom).WhereLessThan("raisedDate", SearchDateTo).WhereEqualTo("status", "Completed").WhereEqualTo("referTo", userId).GetSnapshotAsync();
                            totalCompletedAppointments += snapCompletedAppointments.Count;
                        }
                        else if (Roles.Contains("Chemist"))
                        {
                            QuerySnapshot snapCompletedAppointments = await docsnapClinics.Reference.Collection("appointments").WhereGreaterThanOrEqualTo("raisedDate", SearchDateFrom).WhereLessThan("raisedDate", SearchDateTo).WhereEqualTo("statusChemist", "Completed").GetSnapshotAsync();
                            totalCompletedAppointments += snapCompletedAppointments.Count;
                        }
                        else if (Roles.Contains("Cashier"))
                        {
                            QuerySnapshot snapCompletedAppointments = await docsnapClinics.Reference.Collection("appointments").WhereGreaterThanOrEqualTo("raisedDate", SearchDateFrom).WhereLessThan("raisedDate", SearchDateTo).WhereEqualTo("statusCashier", "Completed").GetSnapshotAsync();
                            totalCompletedAppointments += snapCompletedAppointments.Count;
                        }
                    }
                    #endregion

                    #region Today Patient count and Appointment logic

                    if (Roles.Contains("Receptionist"))
                    {
                        QuerySnapshot snapTodayAppointments = await docsnapClinics.Reference.Collection("appointments").WhereGreaterThanOrEqualTo("raisedDate", SearchDateFrom).WhereLessThan("raisedDate", SearchDateTo).WhereEqualTo("receptionist", userId).GetSnapshotAsync();
                        totalTodayAppointmentsReceptionist += snapTodayAppointments.Count;
                    }
                    else if (Roles.Contains("Doctor"))
                    {
                        QuerySnapshot snapTodayAppointments = await docsnapClinics.Reference.Collection("appointments").WhereGreaterThanOrEqualTo("raisedDate", SearchDateFrom).WhereLessThan("raisedDate", SearchDateTo).WhereEqualTo("referTo", userId).GetSnapshotAsync();
                        totalTodayAppointmentsReceptionist += snapTodayAppointments.Count;
                    }
                    else if (Roles.Contains("Chemist"))
                    {
                        QuerySnapshot snapTodayAppointments = await docsnapClinics.Reference.Collection("appointments").WhereGreaterThanOrEqualTo("raisedDate", SearchDateFrom).WhereLessThan("raisedDate", SearchDateTo).WhereEqualTo("status", "Completed").GetSnapshotAsync();
                        totalTodayAppointmentsReceptionist += snapTodayAppointments.Count;
                    }
                    else if (Roles.Contains("Cashier"))
                    {
                        QuerySnapshot snapTodayAppointments = await docsnapClinics.Reference.Collection("appointments").WhereGreaterThanOrEqualTo("raisedDate", SearchDateFrom).WhereLessThan("raisedDate", SearchDateTo).WhereEqualTo("statusChemist", "Completed").GetSnapshotAsync();
                        totalTodayAppointmentsReceptionist += snapTodayAppointments.Count;
                    }

                    #endregion


                }
             


            }
            catch (Exception ex)
            {
                
            }

            AppointmentsCount ac = new AppointmentsCount();

            ac.AllPatientsCount = totalPatientCount.ToString();
            ac.TodayAppointmentsCount = totalTodayAppointmentsReceptionist.ToString();
            ac.WaitingAppointmentsCounts = totalWaitingAppointments.ToString();
            ac.CompletedAppointmentsCounts = totalCompletedAppointments.ToString();

            return Json(ac, JsonRequestBehavior.AllowGet);
        }


        //This function is called by ajax for Create Future Appointments
        public async Task<JsonResult> ValidationsCreateAppointment(string startdate, string Type, string Search, string datepicker, string patientAutoId, string token, string referto)
        {

            DateTime SearchDate;
            if (startdate == null || startdate == "")
            {
                SearchDate = Convert.ToDateTime(DateTime.UtcNow);
            }
            else
            {
                SearchDate = Convert.ToDateTime(startdate);
                SearchDate = DateTime.SpecifyKind(SearchDate, DateTimeKind.Utc);
            }

            SearchDate = SearchDate.Date;
            Timestamp SearchDateFrom = Timestamp.FromDateTime(SearchDate.Date.AddHours(-5).AddMinutes(-30));
            Timestamp SearchDateTo = Timestamp.FromDateTime(SearchDate.Date.AddHours(-5).AddMinutes(-30).AddDays(1));

            string ClinicMobileNumber = "";
            string ClinicFirebaseDocumentId = "";

            string msg = "";
            string ErrorMessage = "";

            var authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            string savedString = "";

            if (authCookie != null)
            {
                var ticket = FormsAuthentication.Decrypt(authCookie.Value);
                if (ticket != null)
                {
                    savedString = ticket.Name; // Get the stored string
                    ClinicMobileNumber = savedString.Split('|')[3];
                    ClinicFirebaseDocumentId = savedString.Split('|')[4];


                }
            }
            else
            {
                return Json(new { success = false, message = "User is not authenticated" });
            }

            try
            {
                #region Create Appointment
                string fee = "";
                string days = "";
                DateTime CompletionDate;//field used when refer to has Chemist role
                string appointmentDate = datepicker;
                DateTime ConvertedAppDate = DateTime.SpecifyKind(Convert.ToDateTime(appointmentDate).AddHours(-5).AddMinutes(-30), DateTimeKind.Utc);
                Timestamp AppointmentDateFrom = Timestamp.FromDateTime(ConvertedAppDate.Date.AddHours(-5).AddMinutes(-30));
                Timestamp AppointmentDateTo = Timestamp.FromDateTime(ConvertedAppDate.Date.AddHours(-5).AddMinutes(-30).AddDays(1));

                DocumentReference docRefPatientUID = _db.Collection("clinics").Document(ClinicFirebaseDocumentId).Collection("patientList").Document(patientAutoId);
                DocumentSnapshot docsnapPatientUID = await docRefPatientUID.GetSnapshotAsync();

                string PatientUID = docsnapPatientUID.GetValue<string>("patient_id");
                string severity = "";
                try { severity = docsnapPatientUID.GetValue<string>("severity"); }
                catch
                {
                    severity = "Low";
                }

                string PatientName = docsnapPatientUID.GetValue<string>("patient_name");

                Query Qref1 = _db.Collection("clinics").Document(ClinicFirebaseDocumentId).Collection("appointments").WhereEqualTo("patient", patientAutoId).WhereGreaterThanOrEqualTo("raisedDate", Timestamp.FromDateTime(ConvertedAppDate.Date)).WhereLessThan("raisedDate", Timestamp.FromDateTime(ConvertedAppDate.Date.AddDays(1))).WhereEqualTo("status", "Waiting");
                QuerySnapshot snap = await Qref1.GetSnapshotAsync();
                if (snap.Count > 0)
                {

                    ErrorMessage = "Appointment of " + PatientName + "(" + PatientUID + ") for " + appointmentDate + " already exists. ";
                    return Json(new { success = false, message = ErrorMessage });

                }
                else
                {

                    #region Code to create new appointment id for today

                    #region Check refer to is chemist

                    DocumentReference docRef = _db.Collection("clinics").Document(ClinicFirebaseDocumentId).Collection("user").Document(referto);
                    DocumentSnapshot docSnap = await docRef.GetSnapshotAsync();
                    User user = docSnap.ConvertTo<User>();

                    CollectionReference colAppountments = _db.Collection("clinics").Document(ClinicFirebaseDocumentId).Collection("appointments");

                    if (user.user_roles.Contains("Receptionist") || user.user_roles.Contains("Doctor"))
                    {
                        //string message = await TokenNumberValidation(appointmentDate, token);
                        //if (message != null)
                        //{
                        //    ErrorMessage = message;
                        //    return Json(new { success = false, message = ErrorMessage });
                        //}

                    }
                    else if (user.user_roles.Contains("Chemist"))
                    {
                        Query QrefPrescriptions = _db.Collection("clinics").Document(ClinicFirebaseDocumentId).Collection("patientList").Document(patientAutoId).Collection("prescriptions").OrderByDescending("timeStamp");


                        QuerySnapshot snapPres = await QrefPrescriptions.GetSnapshotAsync();
                        if (snapPres.Count == 0)
                        {
                            ErrorMessage = "Selected Patient has no Prescription";
                            return Json(new { success = false, message = ErrorMessage });
                        }

                        Query QrefCompletedAppointments = _db.Collection("clinics").Document(ClinicFirebaseDocumentId).Collection("appointments").WhereEqualTo("patient", patientAutoId).WhereEqualTo("status", "Completed");
                        QuerySnapshot snapCompletedAppointments = await QrefCompletedAppointments.GetSnapshotAsync();
                        if (snapCompletedAppointments.Count == 0)
                        {
                            ErrorMessage = "Selected Patient has not been checked by Doctor yet";
                            return Json(new { success = false, message = ErrorMessage });

                        }

                        CompletionDate = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);

                        //string message = await TokenNumberValidation(appointmentDate, token);
                        //if (message != null)
                        //{
                        //    ErrorMessage = message;
                        //    return Json(new { success = false, message = ErrorMessage });
                        //}

                    }

                    #endregion Check refer to is chemist or Doctor

                    #endregion Code to create new appointment id for today
                }

                #endregion

                return Json(new { success = true, message = "Appointment validated successfully!" });


            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }

        }

        //,,
        public async Task<JsonResult> CreatePatient(string PatientName, string CareOf, string MobileNumber,  string AppointmentDate, string TokenNumber,string City,string Age,string Disease,string Gender,string Severity,  string ReferBy, string ReferTo)
        {
            DateTime AppDate;
            AppDate = Convert.ToDateTime(AppointmentDate);
            AppDate = DateTime.SpecifyKind(AppDate, DateTimeKind.Utc);

            var authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            string savedString = "";
            string ClinicMobileNumber = "";
            string ClinicFirebaseDocumentId = "";
            string ClinicCode = "";
            string UserMobileNumber = "";
            string userId = "";
            string CreateAppointment = "Yes";
            if (authCookie != null)
            {
                var ticket = FormsAuthentication.Decrypt(authCookie.Value);
                if (ticket != null)
                {
                    savedString = ticket.Name; // Get the stored string
                    ClinicMobileNumber = savedString.Split('|')[3];
                    ClinicFirebaseDocumentId = savedString.Split('|')[4];
                    ClinicCode = savedString.Split('|')[5];
                    UserMobileNumber = savedString.Split('|')[0];
                }
            }

            List<SelectListItem> diseases = new List<SelectListItem>() {
                        new SelectListItem {
                            Text = "General -(fever/general acute complaints)", Value = "General"
                        },
                        new SelectListItem {
                            Text = "ENT (ear nose throat)", Value = "ENT(ear nose throat)"
                        },
                        new SelectListItem {
                            Text = "CNS (central nervous system", Value = "CNS(central nervous system)"
                        },
                        new SelectListItem {
                            Text = "DERMATO (skin/hair)", Value = "DERMATO./SKIN/hair"
                        },
                        new SelectListItem {
                            Text = "GIT (gastro intestinal track)", Value = "GIT(gastro intestinal track)"
                        },
                        new SelectListItem {
                            Text = "SURGERY", Value = "SURGERY"
                        },
                        new SelectListItem {
                            Text = "TUMORS/ONCOLOGY", Value = "TUMOURS/ONCOLOGY"
                        },
                        new SelectListItem {
                            Text = "HAEMATOLOGY (blood)", Value = "HAEMATOLOGY(blood)"
                        },
                        new SelectListItem {
                            Text = "ORTHOPADICS (bones & muscles)", Value = "ORTHOPADICS(bones & muscles)"
                        },
                        new SelectListItem {
                            Text = "PAEDIATRIC (child)", Value = "paediatric"
                        },
                        new SelectListItem {
                            Text = "OPTHALMOLOGISTS (EYE)", Value = "opthalmologists(eye)"
                        },
                        new SelectListItem {
                            Text = "CVS (cardio vascular system/heart)", Value = "CVS(cardio vascular system/heart)"
                        },
                        new SelectListItem {
                            Text = "ENDOCRINOLOGY", Value = "ENDOCRINOLOGY"
                        },
                        new SelectListItem {
                            Text = "GYNAECOLOGY & OBS", Value = "GYNAECOLOGY & OBS"
                        },
                        new SelectListItem {
                            Text = "GENETICS", Value = "GENETICS"
                        },
                    };
            ViewBag.DISEASES = diseases;

            List<SelectListItem> referby = new List<SelectListItem>() {
                new SelectListItem {
                    Text = "PATIENT", Value = "PATIENT"
                },
                new SelectListItem {
                    Text = "TV ADV", Value = "TV ADV"
                },
                new SelectListItem {
                    Text = "SOCIAL MEDIA", Value = "SOCIAL MEDIA"
                },
                new SelectListItem {
                    Text = "WALL PAINTING", Value = "WALL PAINTING"
                },
                new SelectListItem {
                    Text = "POSTERS", Value = "POSTERS"
                },
                new SelectListItem {
                    Text = "INTERNET SEARCH", Value = "INTERNET SEARCH"
                },
                new SelectListItem {
                    Text = "CALLS", Value = "CALLS"
                },
                new SelectListItem {
                    Text = "DOCTOR", Value = "DOCTOR"
                },
                new SelectListItem {
                    Text = "HOSPITAL", Value = "HOSPITAL"
                },
                new SelectListItem {
                    Text = "MAIL/MSG", Value = "MAILMSG"
                },
                new SelectListItem {
                    Text = "RANDOM", Value = "RANDOM"
                },

            };
            ViewBag.REFERBYS = referby;

            List<SelectListItem> UserList = new List<SelectListItem>();
            //string Path = AppDomain.CurrentDomain.BaseDirectory + @"greenpaperdev-firebase-adminsdk-8k2y5-fb46e63414.json";
            //Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path);
            //FirestoreDb db = FirestoreDb.Create("greenpaperdev");

            #region Code to get doctors list for refer to
            QuerySnapshot snapUsersDoctors = await _db.Collection("clinics").Document(ClinicFirebaseDocumentId).Collection("user").WhereArrayContains("user_roles", "Doctor").GetSnapshotAsync();
            foreach (DocumentSnapshot docsnapUsers in snapUsersDoctors)
            {
                SelectListItem user = new SelectListItem();
                user.Text = docsnapUsers.GetValue<string>("name");
                user.Value = docsnapUsers.Id;
                if (docsnapUsers.Exists)
                {
                    try
                    {
                        if (docsnapUsers.GetValue<bool>("user_deactivated") == false)
                        {
                            UserList.Add(user);
                        }
                    }
                    catch
                    {
                        UserList.Add(user);
                    }
                }
            }
            ViewBag.USERS = UserList;
            #endregion Code to get doctors list for refer to


            if (Session["sessionid"] == null)
            { Session["sessionid"] = "empty"; }

            try
            {



                string patientLastId = "";
                string patientLastIdDocId = "";

                Query QrefClinic = _db.Collection("clinics").WhereEqualTo("clinicmobilenumber", ClinicMobileNumber);
                QuerySnapshot snapClinics1 = await QrefClinic.GetSnapshotAsync();

                foreach (DocumentSnapshot docsnapClinics in snapClinics1)
                {
                    #region to get user documentid
                    QuerySnapshot snapUser = await docsnapClinics.Reference.Collection("user").WhereEqualTo("mobile_number", UserMobileNumber).GetSnapshotAsync();
                    DocumentSnapshot docUser = snapUser.Documents[0];

                    userId = docUser.Id;
                    #endregion
                }

                DocumentReference docRefClinicCity = _db.Collection("clinics").Document(ClinicFirebaseDocumentId);
                DocumentSnapshot docSnapClinicCity = await docRefClinicCity.GetSnapshotAsync();

                #region Code to checkduplicacy of patient on the basis of name and mobile number
                Query Qref = _db.Collection("clinics").Document(ClinicFirebaseDocumentId).Collection("patientList").WhereEqualTo("patient_name", PatientName).WhereEqualTo("patient_mobile_number", MobileNumber);
                QuerySnapshot snap = await Qref.GetSnapshotAsync();
                if (snap.Count > 0)
                {
                    //ViewBag.Message = "Patient " + patient.patient_name + " having Mobile number " + patient.patient_mobile_number + " already exists. ";
                    return Json(new { success = false, message = "Patient " + PatientName + " having Mobile number " + MobileNumber + " already exists. " }, JsonRequestBehavior.AllowGet);
                }
                #endregion Code to checkduplicacy of patient on the basis of name and mobile number
                else
                {
                    #region Code to generate new patient UID and update in paltientLastId collection
                    Query QrefPatientLastId = _db.Collection("clinics").Document(ClinicFirebaseDocumentId).Collection("patientLastId").Limit(1);
                    QuerySnapshot snapPatientLastId = await QrefPatientLastId.GetSnapshotAsync();
                    if (snapPatientLastId.Count > 0)
                    {
                        DocumentSnapshot docsnap2 = snapPatientLastId.Documents[0];
                        patientLastIdDocId = docsnap2.Id;

                        patientLastId = PatientLastId(docsnap2.GetValue<string>("id"));//Code to get plus one patientLastId

                        DocumentReference docRef = _db.Collection("clinics").Document(ClinicFirebaseDocumentId).Collection("patientLastId").Document(patientLastIdDocId);
                        DocumentSnapshot docSnap = await docRef.GetSnapshotAsync();

                        Dictionary<string, object> dataPatientLastId = new Dictionary<string, object>
                                    {
                                        {"id" ,patientLastId}
                                    };

                        if (docSnap.Exists)
                        {
                            await docRef.UpdateAsync(dataPatientLastId);
                        }

                    }
                    #endregion Code to generate new patient UID and update in paltientLastId collection
                    #region Code to create new Patient
                    CollectionReference col1 = _db.Collection("clinics").Document(ClinicFirebaseDocumentId).Collection("patientList");

                    Dictionary<string, object> data1 = new Dictionary<string, object>
                                {
                                    {"id" ,DateTime.Now.ToString("yyyyMMddHHmmssffff")},
                                    {"patient_name" ,PatientName.ToLower()},
                                    {"clinicCode" ,ClinicCode},
                                    {"clinicId" ,ClinicFirebaseDocumentId},
                                    {"age" ,Age},
                                    {"care_of" ,CareOf},
                                    {"city" ,City},
                                    {"creation_date" ,DateTime.SpecifyKind(DateTime.Now.AddHours(-5).AddMinutes(-30), DateTimeKind.Utc)},
                                    {"disease" ,Disease},
                                    {"gender" ,Gender},
                                    {"severity" ,Severity},
                                    {"patient_id" ,patientLastId},
                                    {"patient_mobile_number",MobileNumber},
                                    {"refer_by" ,ReferBy},
                                    {"refer_to_doctor" ,ReferTo},
                                    {"isCreated" ,true},
                                    {"isSynced" ,true},
                                    {"UpdatedAt" ,DateTime.SpecifyKind(DateTime.Now.AddHours(-5).AddMinutes(-30), DateTimeKind.Utc)},
                                    {"search_text" ,PatientName+MobileNumber+patientLastId}
                                };
                    await col1.Document().SetAsync(data1);
                    #endregion Code to create new Patient
                    #region Code to get newly created patient's auto id
                    Query QrefLatestPatient = _db.Collection("clinics").Document(ClinicFirebaseDocumentId).Collection("patientList").WhereEqualTo("patient_id", patientLastId).Limit(1);
                    QuerySnapshot snapLatestPatient = await QrefLatestPatient.GetSnapshotAsync();

                    DocumentSnapshot docSnapLatestPatient = snapLatestPatient.Documents[0];
                    #endregion Code to get newly created patient's auto id

                    #region Code to get latest token number, increament it, set in new appointment and update increamented token number back in collection (Commented)
                    //DocumentReference docRefTokenNumber = db.Collection("clinics").Document(GlobalSessionVariables.ClinicDocumentAutoId).Collection("tokenNumber").Document(DateTime.Now.ToString("dd_MM_yyyy"));
                    //DocumentSnapshot docsnapTokenNumber = await docRefTokenNumber.GetSnapshotAsync();

                    //if (docsnapTokenNumber.Exists)
                    //{
                    //    lastTokenNumber = docsnapTokenNumber.GetValue<string>("last_token");
                    //}
                    //else
                    //{
                    //    lastTokenNumber = "0";
                    //}

                    //lastTokenNumber = (Convert.ToInt32(lastTokenNumber) + 1).ToString();

                    //CollectionReference colTokenNumber = db.Collection("clinics").Document(GlobalSessionVariables.ClinicDocumentAutoId).Collection("tokenNumber");

                    //Dictionary<string, object> dataTokenNumber = new Dictionary<string, object>
                    //{
                    //    {"last_token" ,lastTokenNumber},
                    //    {"assigned_tokens" ,null}

                    //};
                    //await colTokenNumber.Document(DateTime.Now.ToString("dd_MM_yyyy")).SetAsync(dataTokenNumber);

                    #endregion Code to get latest token number, increament it, set in new appointment and update increamented token number back in collection

                    if (CreateAppointment == "Yes")
                    {
                        #region Code to check duplicate appointment for selected date having status waiting

                        DateTime ConvertedAppDate = DateTime.SpecifyKind(Convert.ToDateTime(AppointmentDate), DateTimeKind.Utc);

                        Query QrefduplicateApp = _db.Collection("clinics").Document(ClinicFirebaseDocumentId).Collection("appointments").WhereEqualTo("patient", docSnapLatestPatient.Id).WhereGreaterThanOrEqualTo("raisedDate", Timestamp.FromDateTime(ConvertedAppDate.Date)).WhereLessThan("raisedDate", Timestamp.FromDateTime(ConvertedAppDate.AddDays(1))).WhereEqualTo("status", "Waiting");
                        QuerySnapshot duplicateApp = await QrefduplicateApp.GetSnapshotAsync();
                        if (duplicateApp.Count > 0)
                        {
                            ViewBag.Message = "Appointment of " + PatientName + "(" + patientLastId + ") for " + AppointmentDate + " already exists. ";
                            //return View(patient);
                        }
                        #endregion Code to check duplicate appointment for selected date having status waiting
                        else
                        {

                            

                            string message = await UpdateTokenNumber(AppointmentDate.ToString(), TokenNumber);
                            if (message != null)
                            {
                                ViewBag.Message = message;
                                return Json(new { success = false, message = message }, JsonRequestBehavior.AllowGet);
                            }
                            #region Code to create new appointment id for today


                            CollectionReference colAppountments = _db.Collection("clinics").Document(ClinicFirebaseDocumentId).Collection("appointments");

                            Dictionary<string, object> dataAppointment = new Dictionary<string, object>
                                {
                                    {"bill_sms" ,false},
                                    {"clinic_id" ,ClinicFirebaseDocumentId},
                                    {"completionDate" ,null},
                                    {"completiondateChemist" ,null},
                                    {"completiondateCashier" ,null},
                                    {"statusChemist" ,null},
                                    {"statusCashier" ,null},
                                    {"date" ,""},
                                    {"days" ,""},
                                    {"fee" ,""},
                                    {"patient" ,docSnapLatestPatient.Id},
                                    {"patient_id" ,patientLastId},
                                    {"raisedDate",DateTime.SpecifyKind(AppDate.AddHours(-5).AddMinutes(-30), DateTimeKind.Utc)},
                                    {"reminder_sms" ,false},
                                    {"severity" ,"Low"},
                                    {"status" ,"Waiting"},
                                    {"UpdatedAt" ,DateTime.SpecifyKind(DateTime.Now.AddHours(-5).AddMinutes(-30), DateTimeKind.Utc)},
                                    {"token" ,TokenNumber},
                                    {"referTo" ,ReferTo},
                                    {"doctor" ,ReferTo},
                                    {"receptionist" ,userId},
                                    {"isCreated" ,true},
                                    {"isSynced" ,true},
                                };
                            await colAppountments.Document().SetAsync(dataAppointment);
                            #endregion Code to create new appointment id for today

                            //return RedirectToAction("Index", "Appointment");
                        }
                    }
                }
                
                return Json(new { success = true, message = "Patient and Appointment Created successfully." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }

            
        }

        private string PatientLastId(string databaselastid)
        {
            int numericValue = Convert.ToInt32(databaselastid.Substring(databaselastid.Length - 4));
            string alphaValue = databaselastid.Substring(0, databaselastid.Length - 4);
            if (numericValue == 9999)
            {
                numericValue = 1;
                switch (alphaValue)
                {
                    case "A":
                        alphaValue = "B000";
                        break;
                    case "B":
                        alphaValue = "C000";
                        break;
                    default:
                        // code block
                        break;
                }

            }
            else
            {
                numericValue = numericValue + 1;
                if (numericValue.ToString().Length == 1)
                {
                    alphaValue = alphaValue + "000";
                }
                if (numericValue.ToString().Length == 2)
                {
                    alphaValue = alphaValue + "00";
                }
                if (numericValue.ToString().Length == 3)
                {
                    alphaValue = alphaValue + "0";
                }
            }


            databaselastid = alphaValue + numericValue.ToString();
            return databaselastid;
        }

        //This function is called by ajax for Create Future Appointments
        public async Task<ActionResult> CreateFutureAppointment(string startdate, string Type, string Search, string datepicker, string patientAutoId, string token, string referto)
        {
            List<Appointment> AppointmentList = new List<Appointment>();
            List<Patient> PatientList = new List<Patient>();
            List<AppointmentPatientViewModel> AppointmentPatientList = new List<AppointmentPatientViewModel>();
            int totalPatientCount = 0;
            int totalTodayAppointmentsReceptionist = 0;

            DateTime SearchDate;
            if (startdate == null || startdate == "")
            {
                SearchDate = Convert.ToDateTime(DateTime.UtcNow);
            }
            else
            {
                SearchDate = Convert.ToDateTime(startdate);
                SearchDate = DateTime.SpecifyKind(SearchDate, DateTimeKind.Utc);
            }

            SearchDate = SearchDate.Date;
            Timestamp SearchDateFrom = Timestamp.FromDateTime(SearchDate.Date.AddHours(-5).AddMinutes(-30));
            Timestamp SearchDateTo = Timestamp.FromDateTime(SearchDate.Date.AddHours(-5).AddMinutes(-30).AddDays(1));

            string ClinicMobileNumber = "";
            string ClinicFirebaseDocumentId = "";
            string userId = "";
            string UserMobileNumber = "";

            string msg = "";

            var authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            string savedString = "";

            if (authCookie != null)
            {
                var ticket = FormsAuthentication.Decrypt(authCookie.Value);
                if (ticket != null)
                {
                    savedString = ticket.Name; // Get the stored string
                    ClinicMobileNumber = savedString.Split('|')[3];
                    ClinicFirebaseDocumentId = savedString.Split('|')[4];
                    UserMobileNumber = savedString.Split('|')[0];
                    msg = savedString;
                }
            }
            else
            {
                savedString = "Cookie is Blank";
                msg = savedString;
            }



            if (Session["sessionid"] == null)
            {
                Session["sessionid"] = "empty";
                msg = "Session empty";
            }
            else
            {
                msg = Session["sessionid"].ToString();
            }


            try
            {
                Query QrefClinic = _db.Collection("clinics").WhereEqualTo("clinicmobilenumber", ClinicMobileNumber);
                QuerySnapshot snapClinics1 = await QrefClinic.GetSnapshotAsync();

                foreach (DocumentSnapshot docsnapClinics in snapClinics1)
                {
                    #region to get user documentid
                    QuerySnapshot snapUser = await docsnapClinics.Reference.Collection("user").WhereEqualTo("mobile_number", UserMobileNumber).GetSnapshotAsync();
                    DocumentSnapshot docUser = snapUser.Documents[0];

                    userId = docUser.Id;
                    #endregion
                }

                #region Create Appointment
                string fee = "0";
                string days = "0";
                DateTime CompletionDate;//field used when refer to has Chemist role
                string appointmentDate = datepicker;
                DateTime ConvertedAppDate = DateTime.SpecifyKind(Convert.ToDateTime(appointmentDate).AddHours(-5).AddMinutes(-30), DateTimeKind.Utc);
                Timestamp AppointmentDateFrom = Timestamp.FromDateTime(ConvertedAppDate.Date.AddHours(-5).AddMinutes(-30));
                Timestamp AppointmentDateTo = Timestamp.FromDateTime(ConvertedAppDate.Date.AddHours(-5).AddMinutes(-30).AddDays(1));

                DocumentReference docRefPatientUID = _db.Collection("clinics").Document(ClinicFirebaseDocumentId).Collection("patientList").Document(patientAutoId);
                DocumentSnapshot docsnapPatientUID = await docRefPatientUID.GetSnapshotAsync();

                string PatientUID = docsnapPatientUID.GetValue<string>("patient_id");
                string severity = "";
                try { severity = docsnapPatientUID.GetValue<string>("severity"); }
                catch
                {
                    severity = "Low";
                }

                string PatientName = docsnapPatientUID.GetValue<string>("patient_name");

                Query Qref1 = _db.Collection("clinics").Document(ClinicFirebaseDocumentId).Collection("appointments").WhereEqualTo("patient", patientAutoId).WhereGreaterThanOrEqualTo("raisedDate", Timestamp.FromDateTime(ConvertedAppDate.Date)).WhereLessThan("raisedDate", Timestamp.FromDateTime(ConvertedAppDate.Date.AddDays(1))).WhereEqualTo("status", "Waiting");
                QuerySnapshot snap = await Qref1.GetSnapshotAsync();
                if (snap.Count > 0)
                {

                    TempData["Message"] = "Appointment of " + PatientName + "(" + PatientUID + ") for " + appointmentDate + " already exists. ";
                    ViewBag.ErrorMessage = "Appointment of " + PatientName + "(" + PatientUID + ") for " + appointmentDate + " already exists. ";
                    //return RedirectToAction("Index", "Patient");

                }
                else
                {

                    #region Code to create new appointment id for today

                    #region Check refer to is chemist

                    DocumentReference docRef = _db.Collection("clinics").Document(ClinicFirebaseDocumentId).Collection("user").Document(referto);
                    DocumentSnapshot docSnap = await docRef.GetSnapshotAsync();
                    User user = docSnap.ConvertTo<User>();

                    CollectionReference colAppountments = _db.Collection("clinics").Document(ClinicFirebaseDocumentId).Collection("appointments");
                    if (user.user_roles.Contains("Receptionist") || user.user_roles.Contains("Doctor"))
                    {
                        string message = await UpdateTokenNumber(appointmentDate, token);
                        if (message != null)
                        {
                            TempData["Message"] = message;
                            ViewBag.ErrorMessage = message;
                            //return RedirectToAction("Index", "Patient");
                        }

                        Dictionary<string, object> dataAppointment = new Dictionary<string, object>
                                {
                                    {"bill_sms" ,false},
                                    {"clinic_id" ,ClinicFirebaseDocumentId},
                                    {"completionDate" ,null},
                                    {"completiondateChemist" ,null},
                                    {"completiondateCashier" ,null},
                                    {"statusChemist" ,null},
                                    {"statusCashier" ,null},
                                    {"date" ,""},
                                    {"days" ,""},
                                    {"fee" ,""},
                                    {"patient" ,patientAutoId},
                                    {"patient_id" ,PatientUID},
                                    {"raisedDate",ConvertedAppDate},
                                    {"reminder_sms" ,false},
                                    {"severity" ,severity},
                                    {"status" ,"Waiting"},
                                    {"timeStamp" ,DateTime.SpecifyKind(DateTime.Now.AddHours(-5).AddMinutes(-30), DateTimeKind.Utc)},
                                    {"token" ,token},
                                    {"referTo" ,referto},
                                    {"doctor" ,referto},
                                    {"receptionist" ,userId},
                                    {"isCreated" ,true},
                                    {"isSynced" ,true},
                                    {"UpdatedAt" ,DateTime.SpecifyKind(DateTime.Now.AddHours(-5).AddMinutes(-30), DateTimeKind.Utc)},

                                };
                        await colAppountments.Document().SetAsync(dataAppointment);
                    }
                    else if (user.user_roles.Contains("Chemist"))
                    {
                        Query QrefPrescriptions = _db.Collection("clinics").Document(ClinicFirebaseDocumentId).Collection("patientList").Document(patientAutoId).Collection("prescriptions").OrderByDescending("timeStamp");


                        QuerySnapshot snapPres = await QrefPrescriptions.GetSnapshotAsync();
                        if (snapPres.Count == 0)
                        {
                            TempData["Message"] = "Selected Patient has no Prescription";
                            ViewBag.ErrorMessage = "Selected Patient has no Prescription";
                            //return RedirectToAction("Index", "Patient");

                        }
                        List<Appointment> AppointmentListChemist = new List<Appointment>();
                        Query QrefCompletedAppointments = _db.Collection("clinics").Document(ClinicFirebaseDocumentId).Collection("appointments").WhereEqualTo("patient", patientAutoId).WhereEqualTo("status", "Completed");
                        QuerySnapshot snapCompletedAppointments = await QrefCompletedAppointments.GetSnapshotAsync();
                        if (snapCompletedAppointments.Count > 0)
                        {
                            foreach (DocumentSnapshot docsnapCompletedAppointment in snapCompletedAppointments)
                            {
                                Appointment appointment = docsnapCompletedAppointment.ConvertTo<Appointment>();
                                AppointmentListChemist.Add(appointment);
                            }

                            AppointmentListChemist = AppointmentListChemist.OrderByDescending(a => a.raisedDate).ToList();

                            Appointment app = AppointmentListChemist.FirstOrDefault();

                            fee = app.fee;
                            days = app.days;

                        }
                        else
                        {
                            TempData["Message"] = "Selected Patient has not been checked by Doctor yet";
                            ViewBag.ErrorMessage = "Selected Patient has not been checked by Doctor yet";
                            //return RedirectToAction("Index", "Patient");
                        }

                        CompletionDate = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);

                        string message = await UpdateTokenNumber(appointmentDate, token);
                        if (message != null)
                        {
                            TempData["Message"] = message;
                            //return RedirectToAction("Index", "Patient");
                        }

                        Dictionary<string, object> dataAppointment = new Dictionary<string, object>
                                {
                                    {"bill_sms" ,false},
                                    {"clinic_id" ,ClinicFirebaseDocumentId},
                                    {"completionDate" ,CompletionDate},
                                    {"completiondateChemist" ,null},
                                    {"completiondateCashier" ,null},
                                    {"statusChemist" ,null},
                                    {"statusCashier" ,null},
                                    {"date" ,""},
                                    {"days" ,days},
                                    {"fee" ,fee},
                                    {"patient" ,patientAutoId},
                                    {"patient_id" ,PatientUID},
                                    {"raisedDate",CompletionDate},
                                    {"reminder_sms" ,false},
                                    {"severity" ,severity},
                                    {"status" ,"Completed"},
                                    {"timeStamp" ,DateTime.SpecifyKind(DateTime.Now.AddHours(-5).AddMinutes(-30), DateTimeKind.Utc)},
                                    {"token" ,token},
                                    {"referTo" ,referto},
                                    {"doctor" ,referto},
                                    {"receptionist" ,userId},
                                    {"isCreated" ,true},
                                    {"isSynced" ,true},
                                    {"updatedAt" ,DateTime.SpecifyKind(DateTime.Now.AddHours(-5).AddMinutes(-30), DateTimeKind.Utc)}



                                };
                        await colAppountments.Document().SetAsync(dataAppointment);
                    }

                    #endregion Check refer to is chemist or Doctor

                    #endregion Code to create new appointment id for today
                }

                #endregion

                //string Path = AppDomain.CurrentDomain.BaseDirectory + @"greenpaperdev-firebase-adminsdk-8k2y5-fb46e63414.json";
                //Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path);
                //FirestoreDb db = FirestoreDb.Create("greenpaperdev");

                //Query Qref = db.Collection("Students").WhereEqualTo("StudentName","Suvidhi");
                Query Qref = _db.Collection("clinics").WhereEqualTo("clinicmobilenumber", ClinicMobileNumber);
                QuerySnapshot snapClinics = await Qref.GetSnapshotAsync();



                string WhoFirst = "Cashier";
                int i = 0;
                foreach (DocumentSnapshot docsnapClinics in snapClinics)
                {
                    QuerySnapshot snapPatientList = await docsnapClinics.Reference.Collection("patientList").GetSnapshotAsync();
                    totalPatientCount += snapPatientList.Count;



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

                    QuerySnapshot snapAppointments = await docsnapClinics.Reference.Collection("appointments").WhereGreaterThanOrEqualTo("raisedDate", SearchDateFrom).WhereLessThan("raisedDate", SearchDateTo).WhereEqualTo("status", "Waiting").GetSnapshotAsync();
                    totalTodayAppointmentsReceptionist += snapAppointments.Count;

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
                            try { appointment.tokenIteger = Convert.ToInt32(docsnapAppointments.GetValue<string>("token")); }
                            catch { appointment.tokenIteger = i + 1; }

                            AppointmentList.Add(appointment);
                        }
                    }

                }
                AppointmentList = AppointmentList.OrderByDescending(a => a.tokenIteger).ToList();
                ViewBag.Message = SearchDate.Date;
                ViewBag.Type = "Appointments";
                ViewBag.TotalPatientCount = totalPatientCount;
                ViewBag.TotalTodayAppointmentsReceptionist = totalTodayAppointmentsReceptionist;
                //Thread.Sleep(1000);
                //ModelState.AddModelError("", "-- " + Session["sessionid"].ToString() + " -- " + "Succesfully Found Post records.");
                var model = new AppointmentPatientViewModel
                {
                    Patients = PatientList, // Replace with actual data fetching logic
                    Appointments = AppointmentList
                };
                AppointmentPatientList.Add(model);
                //return View(AppointmentPatientList);
                return PartialView("_AppointmentListPartial", AppointmentPatientList);


            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                var model = new AppointmentPatientViewModel
                {
                    Patients = PatientList, // Replace with actual data fetching logic
                    Appointments = AppointmentList
                };
                AppointmentPatientList.Add(model);
                //return View(AppointmentPatientList);

                return PartialView("_AppointmentListPartial", AppointmentPatientList);
            }
        }

        
        //This is called by formsubmission and is not working and replaced by above Search() function 
        [AccessDeniedAuthorize(Roles = "Receptionist, Doctor, Chemist, Cashier")]
        //[Authorize(Roles = "Receptionist")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<ActionResult> Index(string startdate,string Type,string Search)
        {
            
            List<Appointment> AppointmentList = new List<Appointment>();
            List<Patient> PatientList = new List<Patient>();
            List<AppointmentPatientViewModel> AppointmentPatientList = new List<AppointmentPatientViewModel>();
            int totalPatientCount = 0;
            int totalTodayAppointmentsReceptionist = 0;

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

            SearchDate = SearchDate.Date;
            Timestamp SearchDateFrom = Timestamp.FromDateTime(SearchDate.Date.AddHours(-5).AddMinutes(-30));
            Timestamp SearchDateTo = Timestamp.FromDateTime(SearchDate.Date.AddHours(-5).AddMinutes(-30).AddDays(1));

            string ClinicMobileNumber = "";
            string ClinicFirebaseDocumentId = "";

            string msg = "";

            var authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            string savedString = "";

            if (authCookie != null)
            {
                var ticket = FormsAuthentication.Decrypt(authCookie.Value);
                if (ticket != null)
                {
                    savedString = ticket.Name; // Get the stored string
                    ClinicMobileNumber = savedString.Split('|')[3];
                    ClinicFirebaseDocumentId = savedString.Split('|')[4];

                    msg = savedString;
                }
            }
            else
            {
                savedString = "Cookie is Blank";
                msg = savedString;
            }



            if (Session["sessionid"] == null)
            {
                Session["sessionid"] = "empty";
                msg = "Session empty";
            }
            else
            {
                msg = Session["sessionid"].ToString();
            }

            if (Type == "Patients") 
            {
                
                List<SelectListItem> UserList = new List<SelectListItem>();

                try
                {

                    string Path = AppDomain.CurrentDomain.BaseDirectory + @"greenpaperdev-firebase-adminsdk-8k2y5-fb46e63414.json";
                    Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path);
                    FirestoreDb db = FirestoreDb.Create("greenpaperdev");


                    //Query Qref = db.Collection("Students").WhereEqualTo("StudentName","Suvidhi");
                    Query Qref = db.Collection("clinics").WhereEqualTo("clinicmobilenumber", ClinicMobileNumber);
                    QuerySnapshot snap = await Qref.GetSnapshotAsync();


                    foreach (DocumentSnapshot docsnap in snap)
                    {
                        QuerySnapshot snapPatientList = await docsnap.Reference.Collection("patientList").GetSnapshotAsync();
                        totalPatientCount += snapPatientList.Count;

                        QuerySnapshot snapAppointments = await docsnap.Reference.Collection("appointments").WhereGreaterThanOrEqualTo("raisedDate", SearchDateFrom).WhereLessThan("raisedDate", SearchDateTo).WhereEqualTo("status", "Waiting").GetSnapshotAsync();
                        totalTodayAppointmentsReceptionist += snapAppointments.Count;

                        Clinic clinic = docsnap.ConvertTo<Clinic>();

                        #region Code to get doctors list for refer to
                        QuerySnapshot snapUsersDoctors = await docsnap.Reference.Collection("user").WhereArrayContainsAny("user_roles", new string[] { "Doctor", "Chemist" }).GetSnapshotAsync();
                        foreach (DocumentSnapshot docsnapUsers in snapUsersDoctors)
                        {
                            SelectListItem user = new SelectListItem();
                            user.Text = docsnapUsers.GetValue<string>("name");
                            user.Value = docsnapUsers.Id;
                            if (docsnapUsers.Exists)
                            {
                                try
                                {
                                    if (docsnapUsers.GetValue<bool>("user_deactivated") == false)
                                    {
                                        UserList.Add(user);
                                    }
                                }
                                catch
                                {
                                    UserList.Add(user);
                                }


                            }
                        }



                        #endregion Code to Code to get doctors list for refer to

                        if (Search != null && Search != "")
                        {
                            string searchInputToLower = Search.ToLower();
                            string searchInputToUpper = Search.ToUpper();
                            QuerySnapshot snapPatientId = await docsnap.Reference.Collection("patientList").OrderBy("patient_id").StartAt(searchInputToUpper).EndAt(searchInputToUpper + "\uf8ff").GetSnapshotAsync();

                            if (snapPatientId.Count == 0)
                            {
                                QuerySnapshot snapPatientMobileNumber = await docsnap.Reference.Collection("patientList").OrderBy("patient_mobile_number").StartAt(searchInputToLower).EndAt(searchInputToLower + "\uf8ff").GetSnapshotAsync();
                                if (snapPatientMobileNumber.Count == 0)
                                {
                                    QuerySnapshot snapPatientName = await docsnap.Reference.Collection("patientList").OrderBy("patient_name").StartAt(searchInputToLower).EndAt(searchInputToLower + "\uf8ff").GetSnapshotAsync();
                                    if (snapPatientName.Count > 0)
                                    {
                                        foreach (DocumentSnapshot docsnap2 in snapPatientName)
                                        {
                                            Patient patient = docsnap2.ConvertTo<Patient>();
                                            patient.id = docsnap2.Id;

                                            if (docsnap2.Exists)
                                            {
                                                patient.clinic_name = clinic.clinicname;
                                                PatientList.Add(patient);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    foreach (DocumentSnapshot docsnap2 in snapPatientMobileNumber)
                                    {
                                        Patient patient = docsnap2.ConvertTo<Patient>();
                                        patient.id = docsnap2.Id;

                                        if (docsnap2.Exists)
                                        {
                                            patient.clinic_name = clinic.clinicname;
                                            PatientList.Add(patient);
                                        }
                                    }
                                }

                            }
                            else
                            {
                                foreach (DocumentSnapshot docsnap2 in snapPatientId)
                                {
                                    Patient patient = docsnap2.ConvertTo<Patient>();
                                    patient.id = docsnap2.Id;

                                    if (docsnap2.Exists)
                                    {
                                        patient.clinic_name = clinic.clinicname;
                                        PatientList.Add(patient);
                                    }
                                }
                            }
                        }
                        else
                        {

                            QuerySnapshot snap2 = await docsnap.Reference.Collection("patientList").OrderByDescending("patient_id").Limit(10).GetSnapshotAsync();
                            if (snap2.Count > 0)
                            {
                                foreach (DocumentSnapshot docsnap2 in snap2)
                                {
                                    Patient patient = docsnap2.ConvertTo<Patient>();
                                    patient.id = docsnap2.Id;

                                    if (docsnap2.Exists)
                                    {
                                        patient.clinic_name = clinic.clinicname;
                                        PatientList.Add(patient);
                                    }
                                }
                            }
                        }
                    }
                    if (UserList == null || !UserList.Any())
                    {
                        UserList.Add(new SelectListItem
                        {
                            Text = "No users available",
                            Value = "0" // You can assign a specific value for your custom item
                        });
                    }
                    ViewBag.USERS = UserList;
                    ViewBag.ErrorMessage = msg;
                    ViewBag.Message = SearchDate.Date;
                    ViewBag.Type = "Patients";
                    ViewBag.TotalPatientCount = totalPatientCount;
                    ViewBag.TotalTodayAppointmentsReceptionist = totalTodayAppointmentsReceptionist;
                    //ModelState.AddModelError("", "-- " + Session["sessionid"].ToString() + " --" + "Succesfully Found Get records.");
                    var model = new AppointmentPatientViewModel
                    {
                        Patients = PatientList, // Replace with actual data fetching logic
                        Appointments = AppointmentList
                    };
                    AppointmentPatientList.Add(model);
                    //return View(AppointmentPatientList);
                    return PartialView("_PatientListPartial", AppointmentPatientList);
                }
                catch (Exception ex)
                {

                    UserList.Add(new SelectListItem
                    {
                        Text = "in catch error",
                        Value = "0" // You can assign a specific value for your custom item
                    });


                    ViewBag.USERS = UserList;
                    ViewBag.ErrorMessage = ex.Message;
                    ViewBag.Message = SearchDate.Date;
                    ViewBag.Type = "Patients";
                    ViewBag.TotalPatientCount = totalPatientCount;
                    ViewBag.TotalTodayAppointmentsReceptionist = totalTodayAppointmentsReceptionist;
                    ModelState.AddModelError("", ex.Message + " in catch");
                    var model = new AppointmentPatientViewModel
                    {
                        Patients = PatientList, // Replace with actual data fetching logic
                        Appointments = AppointmentList

                    };
                    AppointmentPatientList.Add(model);
                    //return View(AppointmentPatientList);
                    return PartialView("_PatientListPartial", AppointmentPatientList);

                }

            }
            else
            {

                
                
                List<string> statusList = new List<string>();

                try
                {
                   

                    string Path = AppDomain.CurrentDomain.BaseDirectory + @"greenpaperdev-firebase-adminsdk-8k2y5-fb46e63414.json";
                    Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path);
                    FirestoreDb db = FirestoreDb.Create("greenpaperdev");

                    //Query Qref = db.Collection("Students").WhereEqualTo("StudentName","Suvidhi");
                    Query Qref = db.Collection("clinics").WhereEqualTo("clinicmobilenumber", ClinicMobileNumber);
                    QuerySnapshot snapClinics = await Qref.GetSnapshotAsync();
                    string WhoFirst = "Cashier";
                    int i = 0;
                    foreach (DocumentSnapshot docsnapClinics in snapClinics)
                    {
                        QuerySnapshot snapPatientList = await docsnapClinics.Reference.Collection("patientList").GetSnapshotAsync();
                        totalPatientCount += snapPatientList.Count;



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

                        QuerySnapshot snapAppointments = await docsnapClinics.Reference.Collection("appointments").WhereGreaterThanOrEqualTo("raisedDate", SearchDateFrom).WhereLessThan("raisedDate", SearchDateTo).WhereEqualTo("status", "Waiting").GetSnapshotAsync();
                        totalTodayAppointmentsReceptionist += snapAppointments.Count;

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
                                try { appointment.tokenIteger = Convert.ToInt32(docsnapAppointments.GetValue<string>("token")); }
                                catch { appointment.tokenIteger = i + 1; }

                                AppointmentList.Add(appointment);
                            }
                        }

                    }
                    AppointmentList = AppointmentList.OrderByDescending(a => a.tokenIteger).ToList();
                    ViewBag.Message = SearchDate.Date;
                    ViewBag.Type = "Appointments";
                    ViewBag.TotalPatientCount = totalPatientCount;
                    ViewBag.TotalTodayAppointmentsReceptionist = totalTodayAppointmentsReceptionist;
                    //Thread.Sleep(1000);
                    //ModelState.AddModelError("", "-- " + Session["sessionid"].ToString() + " -- " + "Succesfully Found Post records.");
                    var model = new AppointmentPatientViewModel
                    {
                        Patients = PatientList, // Replace with actual data fetching logic
                        Appointments = AppointmentList
                    };
                    AppointmentPatientList.Add(model);
                    //return View(AppointmentPatientList);
                    return PartialView("_AppointmentListPartial", AppointmentPatientList);


                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                    var model = new AppointmentPatientViewModel
                    {
                        Patients = PatientList, // Replace with actual data fetching logic
                        Appointments = AppointmentList
                    };
                    AppointmentPatientList.Add(model);
                    //return View(AppointmentPatientList);

                    return PartialView("_AppointmentListPartial", AppointmentPatientList);
                }


            }
        
        }

        #region Old Index Method commented
        //[AccessDeniedAuthorize(Roles = "Receptionist")]
        //public async Task<ActionResult> Index2(string startdate)
        //{

        //    if (Session["sessionid"] == null)
        //    { Session["sessionid"] = "empty"; }

        //    // check to see if your ID in the Logins table has 
        //    // LoggedIn = true - if so, continue, otherwise, redirect to Login page.
        //    if (await IsYourLoginStillTrue(System.Web.HttpContext.Current.User.Identity.Name.Split('-')[0], Session["sessionid"].ToString()))
        //    {
        //        // check to see if your user ID is being used elsewhere under a different session ID
        //        if (!await IsUserLoggedOnElsewhere(System.Web.HttpContext.Current.User.Identity.Name.Split('-')[0], Session["sessionid"].ToString()))
        //        {
        //            DateTime SearchDate;
        //            if (startdate == null)
        //            {
        //                SearchDate = Convert.ToDateTime(DateTime.UtcNow);
        //            }
        //            else
        //            {
        //                SearchDate = Convert.ToDateTime(startdate);
        //                SearchDate = DateTime.SpecifyKind(SearchDate, DateTimeKind.Utc);
        //            }

        //            SearchDate = SearchDate.Date;
        //            Timestamp SearchDateFrom = Timestamp.FromDateTime(SearchDate.Date.AddHours(-5).AddMinutes(-30));
        //            Timestamp SearchDateTo = Timestamp.FromDateTime(SearchDate.Date.AddHours(-5).AddMinutes(-30).AddDays(1));

        //            string ClinicMobileNumber = GlobalSessionVariables.ClinicMobileNumber;
        //            string Path = AppDomain.CurrentDomain.BaseDirectory + @"greenpaperdev-firebase-adminsdk-8k2y5-fb46e63414.json";
        //            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path);
        //            FirestoreDb db = FirestoreDb.Create("greenpaperdev");


        //            List<Appointment> AppointmentList = new List<Appointment>();
        //            List<string> statusList = new List<string>();


        //            //Query Qref = db.Collection("Students").WhereEqualTo("StudentName","Suvidhi");
        //            Query Qref = db.Collection("clinics").WhereEqualTo("clinicmobilenumber", ClinicMobileNumber);
        //            QuerySnapshot snapClinics = await Qref.GetSnapshotAsync();
        //            QuerySnapshot snapAppointments;
        //            string WhoFirst = "Cashier";
        //            int i = 0;
        //            foreach (DocumentSnapshot docsnapClinics in snapClinics)
        //            {
        //                Clinic clinic = docsnapClinics.ConvertTo<Clinic>();

        //                QuerySnapshot snapSettings = await docsnapClinics.Reference.Collection("settings").Limit(1).GetSnapshotAsync();
        //                if (snapSettings.Count > 0)
        //                {
        //                    DocumentSnapshot docSnapSettings = snapSettings.Documents[0];

        //                    if (docSnapSettings.Exists)
        //                    {
        //                        WhoFirst = docSnapSettings.GetValue<string>("whofirst");
        //                    }
        //                }

        //                snapAppointments = await docsnapClinics.Reference.Collection("appointments").WhereGreaterThanOrEqualTo("raisedDate", SearchDateFrom).WhereLessThan("raisedDate", SearchDateTo).WhereEqualTo("status", "Waiting").GetSnapshotAsync();

        //                foreach (DocumentSnapshot docsnapAppointments in snapAppointments)
        //                {


        //                    Appointment appointment = docsnapAppointments.ConvertTo<Appointment>();

        //                    QuerySnapshot snapPatient = await docsnapClinics.Reference.Collection("patientList").WhereEqualTo("patient_id", appointment.patient_id).Limit(1).GetSnapshotAsync();
        //                    DocumentSnapshot docsnapPatient = snapPatient.Documents[0];

        //                    Patient patient = docsnapPatient.ConvertTo<Patient>();
        //                    if (docsnapAppointments.Exists)
        //                    {
        //                        appointment.clinic_name = clinic.clinicname;
        //                        appointment.patient_name = patient.patient_name;
        //                        appointment.patient_care_of = patient.care_of;
        //                        appointment.patient_gender = patient.gender;
        //                        appointment.patient_age = patient.age;
        //                        appointment.patient_mobile = patient.patient_mobile_number;
        //                        appointment.id = docsnapAppointments.Id;
        //                        try { appointment.tokenIteger = Convert.ToInt32(docsnapAppointments.GetValue<string>("token")); }
        //                        catch { appointment.tokenIteger = i + 1; }

        //                        AppointmentList.Add(appointment);
        //                    }
        //                }

        //            }
        //            AppointmentList = AppointmentList.OrderByDescending(a => a.tokenIteger).ToList();
        //            ViewBag.Message = SearchDate.Date;
        //            Thread.Sleep(1000);
        //            return View(AppointmentList);


        //        }
        //        else
        //        {
        //            // if it is being used elsewhere, update all their 
        //            // Logins records to LoggedIn = false, except for your session ID
        //            LogEveryoneElseOut(System.Web.HttpContext.Current.User.Identity.Name.Split('-')[0], Session["sessionid"].ToString());
        //            DateTime SearchDate;
        //            if (startdate == null)
        //            {
        //                SearchDate = Convert.ToDateTime(DateTime.UtcNow);
        //            }
        //            else
        //            {
        //                SearchDate = Convert.ToDateTime(startdate);
        //                SearchDate = DateTime.SpecifyKind(SearchDate, DateTimeKind.Utc);
        //            }

        //            SearchDate = SearchDate.Date;
        //            Timestamp SearchDateFrom = Timestamp.FromDateTime(SearchDate.Date.AddHours(-5).AddMinutes(-30));
        //            Timestamp SearchDateTo = Timestamp.FromDateTime(SearchDate.Date.AddHours(-5).AddMinutes(-30).AddDays(1));

        //            string ClinicMobileNumber = GlobalSessionVariables.ClinicMobileNumber;
        //            string Path = AppDomain.CurrentDomain.BaseDirectory + @"greenpaperdev-firebase-adminsdk-8k2y5-fb46e63414.json";
        //            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path);
        //            FirestoreDb db = FirestoreDb.Create("greenpaperdev");


        //            List<Appointment> AppointmentList = new List<Appointment>();
        //            List<string> statusList = new List<string>();


        //            //Query Qref = db.Collection("Students").WhereEqualTo("StudentName","Suvidhi");
        //            Query Qref = db.Collection("clinics").WhereEqualTo("clinicmobilenumber", ClinicMobileNumber);
        //            QuerySnapshot snapClinics = await Qref.GetSnapshotAsync();
        //            QuerySnapshot snapAppointments;
        //            string WhoFirst = "Cashier";
        //            int i = 0;
        //            foreach (DocumentSnapshot docsnapClinics in snapClinics)
        //            {
        //                Clinic clinic = docsnapClinics.ConvertTo<Clinic>();

        //                QuerySnapshot snapSettings = await docsnapClinics.Reference.Collection("settings").Limit(1).GetSnapshotAsync();
        //                if (snapSettings.Count > 0)
        //                {
        //                    DocumentSnapshot docSnapSettings = snapSettings.Documents[0];

        //                    if (docSnapSettings.Exists)
        //                    {
        //                        WhoFirst = docSnapSettings.GetValue<string>("whofirst");
        //                    }
        //                }

        //                snapAppointments = await docsnapClinics.Reference.Collection("appointments").WhereGreaterThanOrEqualTo("raisedDate", SearchDateFrom).WhereLessThan("raisedDate", SearchDateTo).WhereEqualTo("status", "Waiting").GetSnapshotAsync();

        //                foreach (DocumentSnapshot docsnapAppointments in snapAppointments)
        //                {


        //                    Appointment appointment = docsnapAppointments.ConvertTo<Appointment>();

        //                    QuerySnapshot snapPatient = await docsnapClinics.Reference.Collection("patientList").WhereEqualTo("patient_id", appointment.patient_id).Limit(1).GetSnapshotAsync();
        //                    DocumentSnapshot docsnapPatient = snapPatient.Documents[0];

        //                    Patient patient = docsnapPatient.ConvertTo<Patient>();
        //                    if (docsnapAppointments.Exists)
        //                    {
        //                        appointment.clinic_name = clinic.clinicname;
        //                        appointment.patient_name = patient.patient_name;
        //                        appointment.patient_care_of = patient.care_of;
        //                        appointment.patient_gender = patient.gender;
        //                        appointment.patient_age = patient.age;
        //                        appointment.patient_mobile = patient.patient_mobile_number;
        //                        appointment.id = docsnapAppointments.Id;
        //                        try { appointment.tokenIteger = Convert.ToInt32(docsnapAppointments.GetValue<string>("token")); }
        //                        catch { appointment.tokenIteger = i + 1; }
        //                        AppointmentList.Add(appointment);
        //                    }
        //                }

        //            }
        //            AppointmentList = AppointmentList.OrderByDescending(a => a.tokenIteger).ToList();
        //            ViewBag.Message = SearchDate.Date;
        //            Thread.Sleep(1000);
        //            return View(AppointmentList);

        //        }
        //    }
        //    else
        //    {
        //        FormsAuthentication.SignOut();
        //        return RedirectToAction("Login", "Home");
        //    }
        //}

        //Kept as a sample to prevent multiple logins of a user
        //public async Task<ActionResult> Index1(string startdate)
        //{

        //    if (Session["sessionid"] == null)
        //    { Session["sessionid"] = "empty"; }

        //    // check to see if your ID in the Logins table has 
        //    // LoggedIn = true - if so, continue, otherwise, redirect to Login page.
        //    if (await IsYourLoginStillTrue(System.Web.HttpContext.Current.User.Identity.Name.Split('-')[0], Session["sessionid"].ToString()))
        //    {
        //        // check to see if your user ID is being used elsewhere under a different session ID
        //        if (!await IsUserLoggedOnElsewhere(System.Web.HttpContext.Current.User.Identity.Name.Split('-')[0], Session["sessionid"].ToString()))
        //        {
        //            return View();
        //        }
        //        else
        //        {
        //            // if it is being used elsewhere, update all their 
        //            // Logins records to LoggedIn = false, except for your session ID
        //            LogEveryoneElseOut(System.Web.HttpContext.Current.User.Identity.Name.Split('-')[0], Session["sessionid"].ToString());
        //            return View();
        //        }
        //    }
        //    else
        //    {
        //        FormsAuthentication.SignOut();
        //        return RedirectToAction("Login", "Home");
        //    }
        //}
        #endregion

        [AccessDeniedAuthorize(Roles = "Chemist,Cashier")]
        [HttpGet]
        public async Task<ActionResult> Waiting()
        {
            var authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            string savedString = "";
            string ClinicMobileNumber = "";
            string ClinicFirebaseDocumentId = "";
            if (authCookie != null)
            {
                var ticket = FormsAuthentication.Decrypt(authCookie.Value);
                if (ticket != null)
                {
                    savedString = ticket.Name; // Get the stored string
                    ClinicMobileNumber = savedString.Split('|')[3];
                    ClinicFirebaseDocumentId = savedString.Split('|')[4];
                }
            }

            if (Session["sessionid"] == null)
            { Session["sessionid"] = "empty"; }

            DateTime SearchDate;
            SearchDate = Convert.ToDateTime(DateTime.UtcNow);
            SearchDate = DateTime.SpecifyKind(SearchDate, DateTimeKind.Utc);

            SearchDate = SearchDate.Date;

            Timestamp SearchDateFrom = Timestamp.FromDateTime(SearchDate.Date.AddHours(-5).AddMinutes(-30));
            Timestamp SearchDateTo = Timestamp.FromDateTime(SearchDate.Date.AddHours(-5).AddMinutes(-30).AddDays(1));

            //string ClinicMobileNumber = GlobalSessionVariables.ClinicMobileNumber;
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
            int i = 0;
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
                        TempData["inventoryon"] = docSnapSettings.GetValue<bool>("inventoryon");
                    }
                }
                else
                {
                    TempData["inventoryon"] = "false";
                }

                snapAppointments = await docsnapClinics.Reference.Collection("appointments").WhereGreaterThanOrEqualTo("raisedDate", SearchDateFrom).WhereLessThan("raisedDate", SearchDateTo).WhereEqualTo("status", "Completed").GetSnapshotAsync();




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
                                try { appointment.tokenIteger = Convert.ToInt32(docsnapAppointments.GetValue<string>("token")); }
                                catch { appointment.tokenIteger = i + 1; }
                                AppointmentList.Add(appointment);
                            }

                        }
                        else if (User.IsInRole("Cashier"))
                        {

                            if (WhoFirst == "Cashier")
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
                            else
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
                        else if (User.IsInRole("Chemist"))
                        {
                            if (WhoFirst == "Cashier")
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
                            else
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

            }
            AppointmentList = AppointmentList.OrderByDescending(a => a.tokenIteger).ToList();
            ViewBag.Message = SearchDate.Date;
            if (SearchDate.Date < DateTime.Now.Date)
            {
                ViewData["DateType"] = "OldDate";
            }
            else
            {
                ViewData["DateType"] = "CurrentDate";
            }
            Thread.Sleep(1000);
            return View(AppointmentList);
        }

        [AccessDeniedAuthorize(Roles = "Chemist,Cashier")]
        [HttpPost]
        public async Task<ActionResult> Waiting(string startdate)
        {
            var authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            string savedString = "";
            string ClinicMobileNumber = "";
            string ClinicFirebaseDocumentId = "";
            if (authCookie != null)
            {
                var ticket = FormsAuthentication.Decrypt(authCookie.Value);
                if (ticket != null)
                {
                    savedString = ticket.Name; // Get the stored string
                    ClinicMobileNumber = savedString.Split('|')[3];
                    ClinicFirebaseDocumentId = savedString.Split('|')[4];
                }
            }

            if (Session["sessionid"] == null)
            { Session["sessionid"] = "empty"; }

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
            SearchDate = SearchDate.Date;

            Timestamp SearchDateFrom = Timestamp.FromDateTime(SearchDate.Date.AddHours(-5).AddMinutes(-30));
            Timestamp SearchDateTo = Timestamp.FromDateTime(SearchDate.Date.AddHours(-5).AddMinutes(-30).AddDays(1));

            //string ClinicMobileNumber = GlobalSessionVariables.ClinicMobileNumber;
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
            int i = 0;
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
                        TempData["inventoryon"] = docSnapSettings.GetValue<bool>("inventoryon");
                    }
                }
                else
                {
                    TempData["inventoryon"] = "false";
                }

                snapAppointments = await docsnapClinics.Reference.Collection("appointments").WhereGreaterThanOrEqualTo("raisedDate", SearchDateFrom).WhereLessThan("raisedDate", SearchDateTo).WhereEqualTo("status", "Completed").GetSnapshotAsync();




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
                                try { appointment.tokenIteger = Convert.ToInt32(docsnapAppointments.GetValue<string>("token")); }
                                catch { appointment.tokenIteger = i + 1; }
                                AppointmentList.Add(appointment);
                            }

                        }
                        else if (User.IsInRole("Cashier"))
                        {

                            if (WhoFirst == "Cashier")
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
                            else
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
                        else if (User.IsInRole("Chemist"))
                        {
                            if (WhoFirst == "Cashier")
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
                            else
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

            }
            AppointmentList = AppointmentList.OrderByDescending(a => a.tokenIteger).ToList();
            ViewBag.Message = SearchDate.Date;
            if (SearchDate.Date < DateTime.Now.Date)
            {
                ViewData["DateType"] = "OldDate";
            }
            else
            {
                ViewData["DateType"] = "CurrentDate";
            }
            Thread.Sleep(1000);
            return View(AppointmentList);
        }

        //[AccessDeniedAuthorize(Roles = "Chemist,Cashier")]
        //public async Task<ActionResult> Waiting1(string startdate)
        //{

        //    if (Session["sessionid"] == null)
        //    { Session["sessionid"] = "empty"; }

        //    // check to see if your ID in the Logins table has 
        //    // LoggedIn = true - if so, continue, otherwise, redirect to Login page.
        //    if (await IsYourLoginStillTrue(System.Web.HttpContext.Current.User.Identity.Name.Split('-')[0], Session["sessionid"].ToString()))
        //    {
        //        // check to see if your user ID is being used elsewhere under a different session ID
        //        if (!await IsUserLoggedOnElsewhere(System.Web.HttpContext.Current.User.Identity.Name.Split('-')[0], Session["sessionid"].ToString()))
        //        {
        //            DateTime SearchDate;
        //            if (startdate == null)
        //            {
        //                SearchDate = Convert.ToDateTime(DateTime.UtcNow);
        //            }
        //            else
        //            {
        //                SearchDate = Convert.ToDateTime(startdate);
        //                SearchDate = DateTime.SpecifyKind(SearchDate, DateTimeKind.Utc);
        //            }
        //            SearchDate = SearchDate.Date;

        //            Timestamp SearchDateFrom = Timestamp.FromDateTime(SearchDate.Date.AddHours(-5).AddMinutes(-30));
        //            Timestamp SearchDateTo = Timestamp.FromDateTime(SearchDate.Date.AddHours(-5).AddMinutes(-30).AddDays(1));

        //            string ClinicMobileNumber = GlobalSessionVariables.ClinicMobileNumber;
        //            string Path = AppDomain.CurrentDomain.BaseDirectory + @"greenpaperdev-firebase-adminsdk-8k2y5-fb46e63414.json";
        //            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path);
        //            FirestoreDb db = FirestoreDb.Create("greenpaperdev");


        //            List<Appointment> AppointmentList = new List<Appointment>();
        //            List<string> statusList = new List<string>();


        //            //Query Qref = db.Collection("Students").WhereEqualTo("StudentName","Suvidhi");
        //            Query Qref = db.Collection("clinics").WhereEqualTo("clinicmobilenumber", ClinicMobileNumber);
        //            QuerySnapshot snapClinics = await Qref.GetSnapshotAsync();
        //            QuerySnapshot snapAppointments;
        //            string WhoFirst = "Cashier";
        //            string statusCashier = "";
        //            string statusChemist = "";
        //            int i = 0;
        //            foreach (DocumentSnapshot docsnapClinics in snapClinics)
        //            {
        //                Clinic clinic = docsnapClinics.ConvertTo<Clinic>();

        //                QuerySnapshot snapSettings = await docsnapClinics.Reference.Collection("settings").Limit(1).GetSnapshotAsync();
        //                if (snapSettings.Count > 0)
        //                {
        //                    DocumentSnapshot docSnapSettings = snapSettings.Documents[0];

        //                    if (docSnapSettings.Exists)
        //                    {
        //                        WhoFirst = docSnapSettings.GetValue<string>("whofirst");
        //                        TempData["inventoryon"] = docSnapSettings.GetValue<bool>("inventoryon");
        //                    }
        //                }
        //                else
        //                {
        //                    TempData["inventoryon"] = "false";
        //                }

        //                snapAppointments = await docsnapClinics.Reference.Collection("appointments").WhereGreaterThanOrEqualTo("raisedDate", SearchDateFrom).WhereLessThan("raisedDate", SearchDateTo).WhereEqualTo("status", "Completed").GetSnapshotAsync();




        //                foreach (DocumentSnapshot docsnapAppointments in snapAppointments)
        //                {


        //                    Appointment appointment = docsnapAppointments.ConvertTo<Appointment>();

        //                    QuerySnapshot snapPatient = await docsnapClinics.Reference.Collection("patientList").WhereEqualTo("patient_id", appointment.patient_id).Limit(1).GetSnapshotAsync();
        //                    DocumentSnapshot docsnapPatient = snapPatient.Documents[0];

        //                    Patient patient = docsnapPatient.ConvertTo<Patient>();
        //                    if (docsnapAppointments.Exists)
        //                    {
        //                        if (User.IsInRole("Cashier") && User.IsInRole("Chemist"))
        //                        {
        //                            try
        //                            {
        //                                statusCashier = docsnapAppointments.GetValue<string>("statusCashier");
        //                            }
        //                            catch
        //                            {
        //                                statusCashier = null;
        //                            }
        //                            if (statusCashier == null)
        //                            {
        //                                appointment.clinic_name = clinic.clinicname;
        //                                appointment.patient_name = patient.patient_name;
        //                                appointment.patient_care_of = patient.care_of;
        //                                appointment.patient_gender = patient.gender;
        //                                appointment.patient_age = patient.age;
        //                                appointment.patient_mobile = patient.patient_mobile_number;
        //                                appointment.id = docsnapAppointments.Id;
        //                                try { appointment.tokenIteger = Convert.ToInt32(docsnapAppointments.GetValue<string>("token")); }
        //                                catch { appointment.tokenIteger = i + 1; }
        //                                AppointmentList.Add(appointment);
        //                            }

        //                        }
        //                        else if (User.IsInRole("Cashier"))
        //                        {

        //                            if(WhoFirst == "Cashier")
        //                            {
        //                                try
        //                                {
        //                                    statusCashier = docsnapAppointments.GetValue<string>("statusCashier");
        //                                }
        //                                catch
        //                                {
        //                                    statusCashier = null;
        //                                }
        //                                if (statusCashier == null)
        //                                {
        //                                    appointment.clinic_name = clinic.clinicname;
        //                                    appointment.patient_name = patient.patient_name;
        //                                    appointment.patient_care_of = patient.care_of;
        //                                    appointment.patient_gender = patient.gender;
        //                                    appointment.patient_age = patient.age;
        //                                    appointment.patient_mobile = patient.patient_mobile_number;
        //                                    appointment.id = docsnapAppointments.Id;
        //                                    appointment.tokenIteger = Convert.ToInt32(docsnapAppointments.GetValue<string>("token"));
        //                                    AppointmentList.Add(appointment);
        //                                }
        //                            }
        //                            else
        //                            {
        //                                try
        //                                {
        //                                    statusChemist = docsnapAppointments.GetValue<string>("statusChemist");
        //                                }
        //                                catch
        //                                {
        //                                    statusChemist = null;
        //                                }
        //                                if (statusChemist != null)
        //                                {
        //                                    appointment.clinic_name = clinic.clinicname;
        //                                    appointment.patient_name = patient.patient_name;
        //                                    appointment.patient_care_of = patient.care_of;
        //                                    appointment.patient_gender = patient.gender;
        //                                    appointment.patient_age = patient.age;
        //                                    appointment.patient_mobile = patient.patient_mobile_number;
        //                                    appointment.id = docsnapAppointments.Id;
        //                                    appointment.tokenIteger = Convert.ToInt32(docsnapAppointments.GetValue<string>("token"));
        //                                    AppointmentList.Add(appointment);
        //                                }
        //                            }

        //                        }
        //                        else if (User.IsInRole("Chemist"))
        //                        {
        //                            if (WhoFirst == "Cashier")
        //                            {
        //                                try
        //                                {
        //                                    statusCashier = docsnapAppointments.GetValue<string>("statusCashier");
        //                                }
        //                                catch
        //                                {
        //                                    statusCashier = null;
        //                                }
        //                                if (statusCashier != null)
        //                                {
        //                                    appointment.clinic_name = clinic.clinicname;
        //                                    appointment.patient_name = patient.patient_name;
        //                                    appointment.patient_care_of = patient.care_of;
        //                                    appointment.patient_gender = patient.gender;
        //                                    appointment.patient_age = patient.age;
        //                                    appointment.patient_mobile = patient.patient_mobile_number;
        //                                    appointment.id = docsnapAppointments.Id;
        //                                    appointment.tokenIteger = Convert.ToInt32(docsnapAppointments.GetValue<string>("token"));
        //                                    AppointmentList.Add(appointment);
        //                                }
        //                            }
        //                            else
        //                            {
        //                                try
        //                                {
        //                                    statusChemist = docsnapAppointments.GetValue<string>("statusChemist");
        //                                }
        //                                catch
        //                                {
        //                                    statusChemist = null;
        //                                }
        //                                if (statusChemist == null)
        //                                {
        //                                    appointment.clinic_name = clinic.clinicname;
        //                                    appointment.patient_name = patient.patient_name;
        //                                    appointment.patient_care_of = patient.care_of;
        //                                    appointment.patient_gender = patient.gender;
        //                                    appointment.patient_age = patient.age;
        //                                    appointment.patient_mobile = patient.patient_mobile_number;
        //                                    appointment.id = docsnapAppointments.Id;
        //                                    appointment.tokenIteger = Convert.ToInt32(docsnapAppointments.GetValue<string>("token"));
        //                                    AppointmentList.Add(appointment);
        //                                }
        //                            }

        //                        }
        //                    }
        //                }

        //            }
        //            AppointmentList = AppointmentList.OrderByDescending(a => a.tokenIteger).ToList();
        //            ViewBag.Message = SearchDate.Date;
        //            if (SearchDate.Date < DateTime.Now.Date)
        //            {
        //                ViewData["DateType"] = "OldDate";
        //            }
        //            else
        //            {
        //                ViewData["DateType"] = "CurrentDate";
        //            }
        //            Thread.Sleep(1000);
        //            return View(AppointmentList);
        //        }
        //        else
        //        {
        //            // if it is being used elsewhere, update all their 
        //            // Logins records to LoggedIn = false, except for your session ID
        //            LogEveryoneElseOut(System.Web.HttpContext.Current.User.Identity.Name.Split('-')[0], Session["sessionid"].ToString());
        //            DateTime SearchDate;
        //            if (startdate == null)
        //            {
        //                SearchDate = Convert.ToDateTime(DateTime.UtcNow);
        //            }
        //            else
        //            {
        //                SearchDate = Convert.ToDateTime(startdate);
        //                SearchDate = DateTime.SpecifyKind(SearchDate, DateTimeKind.Utc);
        //            }
        //            SearchDate = SearchDate.Date;
        //            Timestamp SearchDateFrom = Timestamp.FromDateTime(SearchDate.Date.AddHours(-5).AddMinutes(-30));
        //            Timestamp SearchDateTo = Timestamp.FromDateTime(SearchDate.Date.AddHours(-5).AddMinutes(-30).AddDays(1));

        //            string ClinicMobileNumber = GlobalSessionVariables.ClinicMobileNumber;
        //            string Path = AppDomain.CurrentDomain.BaseDirectory + @"greenpaperdev-firebase-adminsdk-8k2y5-fb46e63414.json";
        //            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path);
        //            FirestoreDb db = FirestoreDb.Create("greenpaperdev");


        //            List<Appointment> AppointmentList = new List<Appointment>();
        //            List<string> statusList = new List<string>();


        //            //Query Qref = db.Collection("Students").WhereEqualTo("StudentName","Suvidhi");
        //            Query Qref = db.Collection("clinics").WhereEqualTo("clinicmobilenumber", ClinicMobileNumber);
        //            QuerySnapshot snapClinics = await Qref.GetSnapshotAsync();
        //            QuerySnapshot snapAppointments;
        //            string WhoFirst = "Cashier";
        //            string statusCashier = "";
        //            string statusChemist = "";
        //            int i = 0;
        //            foreach (DocumentSnapshot docsnapClinics in snapClinics)
        //            {
        //                Clinic clinic = docsnapClinics.ConvertTo<Clinic>();

        //                QuerySnapshot snapSettings = await docsnapClinics.Reference.Collection("settings").Limit(1).GetSnapshotAsync();
        //                if (snapSettings.Count > 0)
        //                {
        //                    DocumentSnapshot docSnapSettings = snapSettings.Documents[0];

        //                    if (docSnapSettings.Exists)
        //                    {
        //                        WhoFirst = docSnapSettings.GetValue<string>("whofirst");
        //                        TempData["inventoryon"] = docSnapSettings.GetValue<bool>("inventoryon");
        //                    }
        //                }
        //                else
        //                {
        //                    TempData["inventoryon"] = "false";
        //                }

        //                snapAppointments = await docsnapClinics.Reference.Collection("appointments").WhereGreaterThanOrEqualTo("raisedDate", SearchDateFrom).WhereLessThan("raisedDate", SearchDateTo).WhereEqualTo("status", "Completed").GetSnapshotAsync();




        //                foreach (DocumentSnapshot docsnapAppointments in snapAppointments)
        //                {


        //                    Appointment appointment = docsnapAppointments.ConvertTo<Appointment>();

        //                    QuerySnapshot snapPatient = await docsnapClinics.Reference.Collection("patientList").WhereEqualTo("patient_id", appointment.patient_id).Limit(1).GetSnapshotAsync();
        //                    DocumentSnapshot docsnapPatient = snapPatient.Documents[0];

        //                    Patient patient = docsnapPatient.ConvertTo<Patient>();
        //                    if (docsnapAppointments.Exists)
        //                    {
        //                        if (User.IsInRole("Cashier") && User.IsInRole("Chemist"))
        //                        {
        //                            try
        //                            {
        //                                statusCashier = docsnapAppointments.GetValue<string>("statusCashier");
        //                            }
        //                            catch
        //                            {
        //                                statusCashier = null;
        //                            }
        //                            if (statusCashier == null)
        //                            {
        //                                appointment.clinic_name = clinic.clinicname;
        //                                appointment.patient_name = patient.patient_name;
        //                                appointment.patient_care_of = patient.care_of;
        //                                appointment.patient_gender = patient.gender;
        //                                appointment.patient_age = patient.age;
        //                                appointment.patient_mobile = patient.patient_mobile_number;
        //                                appointment.id = docsnapAppointments.Id;
        //                                try { appointment.tokenIteger = Convert.ToInt32(docsnapAppointments.GetValue<string>("token")); }
        //                                catch { appointment.tokenIteger = i + 1; }
        //                                AppointmentList.Add(appointment);
        //                            }

        //                        }
        //                        else if (User.IsInRole("Cashier"))
        //                        {
        //                            if (WhoFirst == "Cashier")
        //                            {
        //                                try
        //                                {
        //                                    statusCashier = docsnapAppointments.GetValue<string>("statusCashier");
        //                                }
        //                                catch
        //                                {
        //                                    statusCashier = null;
        //                                }
        //                                if (statusCashier == null)
        //                                {
        //                                    appointment.clinic_name = clinic.clinicname;
        //                                    appointment.patient_name = patient.patient_name;
        //                                    appointment.patient_care_of = patient.care_of;
        //                                    appointment.patient_gender = patient.gender;
        //                                    appointment.patient_age = patient.age;
        //                                    appointment.patient_mobile = patient.patient_mobile_number;
        //                                    appointment.id = docsnapAppointments.Id;
        //                                    appointment.tokenIteger = Convert.ToInt32(docsnapAppointments.GetValue<string>("token"));
        //                                    AppointmentList.Add(appointment);
        //                                }
        //                            }
        //                            else
        //                            {
        //                                try
        //                                {
        //                                    statusChemist = docsnapAppointments.GetValue<string>("statusChemist");
        //                                }
        //                                catch
        //                                {
        //                                    statusChemist = null;
        //                                }
        //                                if (statusChemist != null)
        //                                {
        //                                    appointment.clinic_name = clinic.clinicname;
        //                                    appointment.patient_name = patient.patient_name;
        //                                    appointment.patient_care_of = patient.care_of;
        //                                    appointment.patient_gender = patient.gender;
        //                                    appointment.patient_age = patient.age;
        //                                    appointment.patient_mobile = patient.patient_mobile_number;
        //                                    appointment.id = docsnapAppointments.Id;
        //                                    appointment.tokenIteger = Convert.ToInt32(docsnapAppointments.GetValue<string>("token"));
        //                                    AppointmentList.Add(appointment);
        //                                }
        //                            }
        //                        }
        //                        else if (User.IsInRole("Chemist"))
        //                        {
        //                            if (WhoFirst == "Cashier")
        //                            {
        //                                try
        //                                {
        //                                    statusCashier = docsnapAppointments.GetValue<string>("statusCashier");
        //                                }
        //                                catch
        //                                {
        //                                    statusCashier = null;
        //                                }
        //                                if (statusCashier != null)
        //                                {
        //                                    appointment.clinic_name = clinic.clinicname;
        //                                    appointment.patient_name = patient.patient_name;
        //                                    appointment.patient_care_of = patient.care_of;
        //                                    appointment.patient_gender = patient.gender;
        //                                    appointment.patient_age = patient.age;
        //                                    appointment.patient_mobile = patient.patient_mobile_number;
        //                                    appointment.id = docsnapAppointments.Id;
        //                                    appointment.tokenIteger = Convert.ToInt32(docsnapAppointments.GetValue<string>("token"));
        //                                    AppointmentList.Add(appointment);
        //                                }
        //                            }
        //                            else
        //                            {
        //                                try
        //                                {
        //                                    statusChemist = docsnapAppointments.GetValue<string>("statusChemist");
        //                                }
        //                                catch
        //                                {
        //                                    statusChemist = null;
        //                                }
        //                                if (statusChemist == null)
        //                                {
        //                                    appointment.clinic_name = clinic.clinicname;
        //                                    appointment.patient_name = patient.patient_name;
        //                                    appointment.patient_care_of = patient.care_of;
        //                                    appointment.patient_gender = patient.gender;
        //                                    appointment.patient_age = patient.age;
        //                                    appointment.patient_mobile = patient.patient_mobile_number;
        //                                    appointment.id = docsnapAppointments.Id;
        //                                    appointment.tokenIteger = Convert.ToInt32(docsnapAppointments.GetValue<string>("token"));
        //                                    AppointmentList.Add(appointment);
        //                                }
        //                            }
        //                        }
        //                    }
        //                }

        //            }
        //            AppointmentList = AppointmentList.OrderByDescending(a => a.tokenIteger).ToList();
        //            ViewBag.Message = SearchDate.Date;
        //            if (SearchDate.Date < DateTime.Now.Date)
        //            {
        //                ViewData["DateType"] = "OldDate";
        //            }
        //            else
        //            {
        //                ViewData["DateType"] = "CurrentDate";
        //            }
        //            Thread.Sleep(1000);
        //            return View(AppointmentList);
        //        }
        //    }
        //    else
        //    {
        //        FormsAuthentication.SignOut();
        //        return RedirectToAction("Login", "Home");
        //    }
        //}

        [AccessDeniedAuthorize(Roles = "Chemist,Cashier")]
        [HttpGet]
        public async Task<ActionResult> Completed()
        {
            var authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            string savedString = "";
            string ClinicMobileNumber = "";
            string ClinicFirebaseDocumentId = "";
            if (authCookie != null)
            {
                var ticket = FormsAuthentication.Decrypt(authCookie.Value);
                if (ticket != null)
                {
                    savedString = ticket.Name; // Get the stored string
                    ClinicMobileNumber = savedString.Split('|')[3];
                    ClinicFirebaseDocumentId = savedString.Split('|')[4];
                }
            }

            if (Session["sessionid"] == null)
            { Session["sessionid"] = "empty"; }

            int totalfee = 0;
            int totalfeecash = 0;
            int totalfeeothers = 0;

            DateTime SearchDate;

            SearchDate = Convert.ToDateTime(DateTime.UtcNow);
            SearchDate = DateTime.SpecifyKind(SearchDate, DateTimeKind.Utc);
            
            SearchDate = SearchDate.Date;
            Timestamp SearchDateFrom = Timestamp.FromDateTime(SearchDate.Date.AddHours(-5).AddMinutes(-30));
            Timestamp SearchDateTo = Timestamp.FromDateTime(SearchDate.Date.AddHours(-5).AddMinutes(-30).AddDays(1));

            //string ClinicMobileNumber = GlobalSessionVariables.ClinicMobileNumber;
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
            int i = 0;

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

                snapAppointments = await docsnapClinics.Reference.Collection("appointments").WhereGreaterThanOrEqualTo("raisedDate", SearchDateFrom).WhereLessThan("raisedDate", SearchDateTo).WhereEqualTo("status", "Completed").GetSnapshotAsync();




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
                                try { appointment.tokenIteger = Convert.ToInt32(docsnapAppointments.GetValue<string>("token")); }
                                catch { appointment.tokenIteger = i + 1; }
                                AppointmentList.Add(appointment);
                                totalfee = totalfee + Convert.ToInt32(appointment.fee);
                                if (appointment.modeofpayment == "Cash")
                                {
                                    totalfeecash = totalfeecash + Convert.ToInt32(appointment.fee);
                                }
                                else
                                {
                                    totalfeeothers = totalfeeothers + Convert.ToInt32(appointment.fee);
                                }
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
                                try { appointment.tokenIteger = Convert.ToInt32(docsnapAppointments.GetValue<string>("token")); }
                                catch { appointment.tokenIteger = i + 1; }
                                AppointmentList.Add(appointment);
                                totalfee = totalfee + Convert.ToInt32(appointment.fee);
                                if (appointment.modeofpayment == "Cash")
                                {
                                    totalfeecash = totalfeecash + Convert.ToInt32(appointment.fee);
                                }
                                else
                                {
                                    totalfeeothers = totalfeeothers + Convert.ToInt32(appointment.fee);
                                }
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
                                try { appointment.tokenIteger = Convert.ToInt32(docsnapAppointments.GetValue<string>("token")); }
                                catch { appointment.tokenIteger = i + 1; }
                                AppointmentList.Add(appointment);
                            }
                        }

                    }
                }

            }

            ViewData["totalfee"] = totalfee;
            ViewData["totalfeecash"] = totalfeecash;
            ViewData["totalfeeothers"] = totalfeeothers;

            AppointmentList = AppointmentList.OrderByDescending(a => a.tokenIteger).ToList();
            ViewBag.Message = SearchDate.Date;
            Thread.Sleep(1000);
            return View(AppointmentList);

        }

        [AccessDeniedAuthorize(Roles = "Chemist,Cashier")]
        [HttpPost]
        public async Task<ActionResult> Completed(string startdate)
        {
            var authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            string savedString = "";
            string ClinicMobileNumber = "";
            string ClinicFirebaseDocumentId = "";
            if (authCookie != null)
            {
                var ticket = FormsAuthentication.Decrypt(authCookie.Value);
                if (ticket != null)
                {
                    savedString = ticket.Name; // Get the stored string
                    ClinicMobileNumber = savedString.Split('|')[3];
                    ClinicFirebaseDocumentId = savedString.Split('|')[4];
                }
            }

            if (Session["sessionid"] == null)
            { Session["sessionid"] = "empty"; }

            int totalfee = 0;
            int totalfeecash = 0;
            int totalfeeothers = 0;

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
            SearchDate = SearchDate.Date;
            Timestamp SearchDateFrom = Timestamp.FromDateTime(SearchDate.Date.AddHours(-5).AddMinutes(-30));
            Timestamp SearchDateTo = Timestamp.FromDateTime(SearchDate.Date.AddHours(-5).AddMinutes(-30).AddDays(1));

            //string ClinicMobileNumber = GlobalSessionVariables.ClinicMobileNumber;
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
            int i = 0;

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

                snapAppointments = await docsnapClinics.Reference.Collection("appointments").WhereGreaterThanOrEqualTo("raisedDate", SearchDateFrom).WhereLessThan("raisedDate", SearchDateTo).WhereEqualTo("status", "Completed").GetSnapshotAsync();




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
                                try { appointment.tokenIteger = Convert.ToInt32(docsnapAppointments.GetValue<string>("token")); }
                                catch { appointment.tokenIteger = i + 1; }
                                AppointmentList.Add(appointment);
                                totalfee = totalfee + Convert.ToInt32(appointment.fee);
                                if (appointment.modeofpayment == "Cash")
                                {
                                    totalfeecash = totalfeecash + Convert.ToInt32(appointment.fee);
                                }
                                else
                                {
                                    totalfeeothers = totalfeeothers + Convert.ToInt32(appointment.fee);
                                }
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
                                try { appointment.tokenIteger = Convert.ToInt32(docsnapAppointments.GetValue<string>("token")); }
                                catch { appointment.tokenIteger = i + 1; }
                                AppointmentList.Add(appointment);
                                totalfee = totalfee + Convert.ToInt32(appointment.fee);
                                if (appointment.modeofpayment == "Cash")
                                {
                                    totalfeecash = totalfeecash + Convert.ToInt32(appointment.fee);
                                }
                                else
                                {
                                    totalfeeothers = totalfeeothers + Convert.ToInt32(appointment.fee);
                                }
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
                                try { appointment.tokenIteger = Convert.ToInt32(docsnapAppointments.GetValue<string>("token")); }
                                catch { appointment.tokenIteger = i + 1; }
                                AppointmentList.Add(appointment);
                            }
                        }

                    }
                }

            }

            ViewData["totalfee"] = totalfee;
            ViewData["totalfeecash"] = totalfeecash;
            ViewData["totalfeeothers"] = totalfeeothers;

            AppointmentList = AppointmentList.OrderByDescending(a => a.tokenIteger).ToList();
            ViewBag.Message = SearchDate.Date;
            Thread.Sleep(1000);
            return View(AppointmentList);

        }

        //[AccessDeniedAuthorize(Roles = "Chemist,Cashier")]
        //public async Task<ActionResult> Completed1(string startdate)
        //{

        //    if (Session["sessionid"] == null)
        //    { Session["sessionid"] = "empty"; }

        //    int totalfee = 0;
        //    int totalfeecash = 0;
        //    int totalfeeothers = 0;
        //    // check to see if your ID in the Logins table has 
        //    // LoggedIn = true - if so, continue, otherwise, redirect to Login page.
        //    if (await IsYourLoginStillTrue(System.Web.HttpContext.Current.User.Identity.Name.Split('-')[0], Session["sessionid"].ToString()))
        //    {
        //        // check to see if your user ID is being used elsewhere under a different session ID
        //        if (!await IsUserLoggedOnElsewhere(System.Web.HttpContext.Current.User.Identity.Name.Split('-')[0], Session["sessionid"].ToString()))
        //        {
        //            DateTime SearchDate;
        //            if (startdate == null)
        //            {
        //                SearchDate = Convert.ToDateTime(DateTime.UtcNow);
        //            }
        //            else
        //            {
        //                SearchDate = Convert.ToDateTime(startdate);
        //                SearchDate = DateTime.SpecifyKind(SearchDate, DateTimeKind.Utc);
        //            }
        //            SearchDate = SearchDate.Date;
        //            Timestamp SearchDateFrom = Timestamp.FromDateTime(SearchDate.Date.AddHours(-5).AddMinutes(-30));
        //            Timestamp SearchDateTo = Timestamp.FromDateTime(SearchDate.Date.AddHours(-5).AddMinutes(-30).AddDays(1));

        //            string ClinicMobileNumber = GlobalSessionVariables.ClinicMobileNumber;
        //            string Path = AppDomain.CurrentDomain.BaseDirectory + @"greenpaperdev-firebase-adminsdk-8k2y5-fb46e63414.json";
        //            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path);
        //            FirestoreDb db = FirestoreDb.Create("greenpaperdev");


        //            List<Appointment> AppointmentList = new List<Appointment>();
        //            List<string> statusList = new List<string>();


        //            //Query Qref = db.Collection("Students").WhereEqualTo("StudentName","Suvidhi");
        //            Query Qref = db.Collection("clinics").WhereEqualTo("clinicmobilenumber", ClinicMobileNumber);
        //            QuerySnapshot snapClinics = await Qref.GetSnapshotAsync();
        //            QuerySnapshot snapAppointments;
        //            string WhoFirst = "Cashier";
        //            string statusCashier = "";
        //            string statusChemist;
        //            int i = 0;
                    
        //            foreach (DocumentSnapshot docsnapClinics in snapClinics)
        //            {
        //                Clinic clinic = docsnapClinics.ConvertTo<Clinic>();

        //                QuerySnapshot snapSettings = await docsnapClinics.Reference.Collection("settings").Limit(1).GetSnapshotAsync();
        //                if (snapSettings.Count > 0)
        //                {
        //                    DocumentSnapshot docSnapSettings = snapSettings.Documents[0];

        //                    if (docSnapSettings.Exists)
        //                    {
        //                        WhoFirst = docSnapSettings.GetValue<string>("whofirst");
        //                    }
        //                }

        //                snapAppointments = await docsnapClinics.Reference.Collection("appointments").WhereGreaterThanOrEqualTo("raisedDate", SearchDateFrom).WhereLessThan("raisedDate", SearchDateTo).WhereEqualTo("status", "Completed").GetSnapshotAsync();




        //                foreach (DocumentSnapshot docsnapAppointments in snapAppointments)
        //                {


        //                    Appointment appointment = docsnapAppointments.ConvertTo<Appointment>();

        //                    QuerySnapshot snapPatient = await docsnapClinics.Reference.Collection("patientList").WhereEqualTo("patient_id", appointment.patient_id).Limit(1).GetSnapshotAsync();
        //                    DocumentSnapshot docsnapPatient = snapPatient.Documents[0];

        //                    Patient patient = docsnapPatient.ConvertTo<Patient>();
        //                    if (docsnapAppointments.Exists)
        //                    {
        //                        if (User.IsInRole("Cashier") && User.IsInRole("Chemist"))
        //                        {
        //                            try
        //                            {
        //                                statusCashier = docsnapAppointments.GetValue<string>("statusCashier");
        //                            }
        //                            catch
        //                            {
        //                                statusCashier = null;
        //                            }
        //                            if (statusCashier != null)
        //                            {
        //                                appointment.clinic_name = clinic.clinicname;
        //                                appointment.patient_name = patient.patient_name;
        //                                appointment.patient_care_of = patient.care_of;
        //                                appointment.patient_gender = patient.gender;
        //                                appointment.patient_age = patient.age;
        //                                appointment.patient_mobile = patient.patient_mobile_number;
        //                                appointment.id = docsnapAppointments.Id;
        //                                try { appointment.tokenIteger = Convert.ToInt32(docsnapAppointments.GetValue<string>("token")); }
        //                                catch { appointment.tokenIteger = i + 1; }
        //                                AppointmentList.Add(appointment);
        //                                totalfee = totalfee + Convert.ToInt32(appointment.fee);
        //                                if (appointment.modeofpayment == "Cash")
        //                                {
        //                                    totalfeecash = totalfeecash + Convert.ToInt32(appointment.fee);
        //                                }
        //                                else
        //                                {
        //                                    totalfeeothers = totalfeeothers + Convert.ToInt32(appointment.fee);
        //                                }
        //                            }
        //                        }
        //                        else if (User.IsInRole("Cashier"))
        //                        {
        //                            try
        //                            {
        //                                statusCashier = docsnapAppointments.GetValue<string>("statusCashier");
        //                            }
        //                            catch
        //                            {
        //                                statusCashier = null;
        //                            }
        //                            if (statusCashier != null)
        //                            {
        //                                appointment.clinic_name = clinic.clinicname;
        //                                appointment.patient_name = patient.patient_name;
        //                                appointment.patient_care_of = patient.care_of;
        //                                appointment.patient_gender = patient.gender;
        //                                appointment.patient_age = patient.age;
        //                                appointment.patient_mobile = patient.patient_mobile_number;
        //                                appointment.id = docsnapAppointments.Id;
        //                                try { appointment.tokenIteger = Convert.ToInt32(docsnapAppointments.GetValue<string>("token")); }
        //                                catch { appointment.tokenIteger = i + 1; }
        //                                AppointmentList.Add(appointment);
        //                                totalfee = totalfee + Convert.ToInt32(appointment.fee);
        //                                if(appointment.modeofpayment == "Cash")
        //                                {
        //                                    totalfeecash = totalfeecash + Convert.ToInt32(appointment.fee);
        //                                }
        //                                else
        //                                {
        //                                    totalfeeothers = totalfeeothers + Convert.ToInt32(appointment.fee);
        //                                }
        //                            }
        //                        }
        //                        else if (User.IsInRole("Chemist"))
        //                        {
        //                            try
        //                            {
        //                                statusChemist = docsnapAppointments.GetValue<string>("statusChemist");
        //                            }
        //                            catch
        //                            {
        //                                statusChemist = null;
        //                            }
        //                            if (statusChemist != null)
        //                            {
        //                                appointment.clinic_name = clinic.clinicname;
        //                                appointment.patient_name = patient.patient_name;
        //                                appointment.patient_care_of = patient.care_of;
        //                                appointment.patient_gender = patient.gender;
        //                                appointment.patient_age = patient.age;
        //                                appointment.patient_mobile = patient.patient_mobile_number;
        //                                appointment.id = docsnapAppointments.Id;
        //                                try { appointment.tokenIteger = Convert.ToInt32(docsnapAppointments.GetValue<string>("token")); }
        //                                catch { appointment.tokenIteger = i + 1; }
        //                                AppointmentList.Add(appointment);
        //                            }
        //                        }

        //                    }
        //                }

        //            }

        //            ViewData["totalfee"] = totalfee;
        //            ViewData["totalfeecash"] = totalfeecash;
        //            ViewData["totalfeeothers"] = totalfeeothers;

        //            AppointmentList = AppointmentList.OrderByDescending(a => a.tokenIteger).ToList();
        //            ViewBag.Message = SearchDate.Date;
        //            Thread.Sleep(1000);
        //            return View(AppointmentList);
        //        }
        //        else
        //        {
        //            // if it is being used elsewhere, update all their 
        //            // Logins records to LoggedIn = false, except for your session ID
        //            LogEveryoneElseOut(System.Web.HttpContext.Current.User.Identity.Name.Split('-')[0], Session["sessionid"].ToString());
        //            DateTime SearchDate;
        //            if (startdate == null)
        //            {
        //                SearchDate = Convert.ToDateTime(DateTime.UtcNow);
        //            }
        //            else
        //            {
        //                SearchDate = Convert.ToDateTime(startdate);
        //                SearchDate = DateTime.SpecifyKind(SearchDate, DateTimeKind.Utc);
        //            }
        //            SearchDate = SearchDate.Date;
        //            Timestamp SearchDateFrom = Timestamp.FromDateTime(SearchDate.Date.AddHours(-5).AddMinutes(-30));
        //            Timestamp SearchDateTo = Timestamp.FromDateTime(SearchDate.Date.AddHours(-5).AddMinutes(-30).AddDays(1));

        //            string ClinicMobileNumber = GlobalSessionVariables.ClinicMobileNumber;
        //            string Path = AppDomain.CurrentDomain.BaseDirectory + @"greenpaperdev-firebase-adminsdk-8k2y5-fb46e63414.json";
        //            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path);
        //            FirestoreDb db = FirestoreDb.Create("greenpaperdev");


        //            List<Appointment> AppointmentList = new List<Appointment>();
        //            List<string> statusList = new List<string>();


        //            //Query Qref = db.Collection("Students").WhereEqualTo("StudentName","Suvidhi");
        //            Query Qref = db.Collection("clinics").WhereEqualTo("clinicmobilenumber", ClinicMobileNumber);
        //            QuerySnapshot snapClinics = await Qref.GetSnapshotAsync();
        //            QuerySnapshot snapAppointments;
        //            string WhoFirst = "Cashier";
        //            string statusCashier = "";
        //            string statusChemist;
        //            int i = 0;
        //            foreach (DocumentSnapshot docsnapClinics in snapClinics)
        //            {
        //                Clinic clinic = docsnapClinics.ConvertTo<Clinic>();

        //                QuerySnapshot snapSettings = await docsnapClinics.Reference.Collection("settings").Limit(1).GetSnapshotAsync();
        //                if (snapSettings.Count > 0)
        //                {
        //                    DocumentSnapshot docSnapSettings = snapSettings.Documents[0];

        //                    if (docSnapSettings.Exists)
        //                    {
        //                        WhoFirst = docSnapSettings.GetValue<string>("whofirst");
        //                    }
        //                }

        //                snapAppointments = await docsnapClinics.Reference.Collection("appointments").WhereGreaterThanOrEqualTo("raisedDate", SearchDateFrom).WhereLessThan("raisedDate", SearchDateTo).WhereEqualTo("status", "Completed").GetSnapshotAsync();




        //                foreach (DocumentSnapshot docsnapAppointments in snapAppointments)
        //                {


        //                    Appointment appointment = docsnapAppointments.ConvertTo<Appointment>();

        //                    QuerySnapshot snapPatient = await docsnapClinics.Reference.Collection("patientList").WhereEqualTo("patient_id", appointment.patient_id).Limit(1).GetSnapshotAsync();
        //                    DocumentSnapshot docsnapPatient = snapPatient.Documents[0];

        //                    Patient patient = docsnapPatient.ConvertTo<Patient>();
        //                    if (docsnapAppointments.Exists)
        //                    {
        //                        if (User.IsInRole("Cashier") && User.IsInRole("Chemist"))
        //                        {
        //                            try
        //                            {
        //                                statusCashier = docsnapAppointments.GetValue<string>("statusCashier");
        //                            }
        //                            catch
        //                            {
        //                                statusCashier = null;
        //                            }
        //                            if (statusCashier != null)
        //                            {
        //                                appointment.clinic_name = clinic.clinicname;
        //                                appointment.patient_name = patient.patient_name;
        //                                appointment.patient_care_of = patient.care_of;
        //                                appointment.patient_gender = patient.gender;
        //                                appointment.patient_age = patient.age;
        //                                appointment.patient_mobile = patient.patient_mobile_number;
        //                                appointment.id = docsnapAppointments.Id;
        //                                try { appointment.tokenIteger = Convert.ToInt32(docsnapAppointments.GetValue<string>("token")); }
        //                                catch { appointment.tokenIteger = i + 1; }
        //                                AppointmentList.Add(appointment);
        //                                totalfee = totalfee + Convert.ToInt32(appointment.fee);
        //                                if (appointment.modeofpayment == "Cash")
        //                                {
        //                                    totalfeecash = totalfeecash + Convert.ToInt32(appointment.fee);
        //                                }
        //                                else
        //                                {
        //                                    totalfeeothers = totalfeeothers + Convert.ToInt32(appointment.fee);
        //                                }
        //                            }
        //                        }
        //                        else if (User.IsInRole("Cashier"))
        //                        {
        //                            try
        //                            {
        //                                statusCashier = docsnapAppointments.GetValue<string>("statusCashier");
        //                            }
        //                            catch
        //                            {
        //                                statusCashier = null;
        //                            }
        //                            if (statusCashier != null)
        //                            {
        //                                appointment.clinic_name = clinic.clinicname;
        //                                appointment.patient_name = patient.patient_name;
        //                                appointment.patient_care_of = patient.care_of;
        //                                appointment.patient_gender = patient.gender;
        //                                appointment.patient_age = patient.age;
        //                                appointment.patient_mobile = patient.patient_mobile_number;
        //                                appointment.id = docsnapAppointments.Id;
        //                                try { appointment.tokenIteger = Convert.ToInt32(docsnapAppointments.GetValue<string>("token")); }
        //                                catch { appointment.tokenIteger = i + 1; }
        //                                AppointmentList.Add(appointment);
        //                                totalfee = totalfee + Convert.ToInt32(appointment.fee);
        //                                if (appointment.modeofpayment == "Cash")
        //                                {
        //                                    totalfeecash = totalfeecash + Convert.ToInt32(appointment.fee);
        //                                }
        //                                else
        //                                {
        //                                    totalfeeothers = totalfeeothers + Convert.ToInt32(appointment.fee);
        //                                }
        //                            }
        //                        }
        //                        else if (User.IsInRole("Chemist"))
        //                        {
        //                            try
        //                            {
        //                                statusChemist = docsnapAppointments.GetValue<string>("statusChemist");
        //                            }
        //                            catch
        //                            {
        //                                statusChemist = null;
        //                            }
        //                            if (statusChemist != null)
        //                            {
        //                                appointment.clinic_name = clinic.clinicname;
        //                                appointment.patient_name = patient.patient_name;
        //                                appointment.patient_care_of = patient.care_of;
        //                                appointment.patient_gender = patient.gender;
        //                                appointment.patient_age = patient.age;
        //                                appointment.patient_mobile = patient.patient_mobile_number;
        //                                appointment.id = docsnapAppointments.Id;
        //                                try { appointment.tokenIteger = Convert.ToInt32(docsnapAppointments.GetValue<string>("token")); }
        //                                catch { appointment.tokenIteger = i + 1; }
        //                                AppointmentList.Add(appointment);
        //                            }
        //                        }

        //                    }
        //                }

        //            }
        //            ViewData["totalfee"] = totalfee;
        //            ViewData["totalfeecash"] = totalfeecash;
        //            ViewData["totalfeeothers"] = totalfeeothers;
        //            AppointmentList = AppointmentList.OrderByDescending(a => a.tokenIteger).ToList();
        //            ViewBag.Message = SearchDate.Date;
        //            Thread.Sleep(1000);
        //            return View(AppointmentList);
        //        }
        //    }
        //    else
        //    {
        //        FormsAuthentication.SignOut();
        //        return RedirectToAction("Login", "Home");
        //    }
        //}
        

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

        public async Task<ActionResult> Fee(string id, string patient, string fee)
        {

            if (Session["sessionid"] == null)
            { Session["sessionid"] = "empty"; }

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

        //public async Task<ActionResult> Fee2(string id, string patient, string fee)
        //{

        //    if (Session["sessionid"] == null)
        //    { Session["sessionid"] = "empty"; }

        //    // check to see if your ID in the Logins table has 
        //    // LoggedIn = true - if so, continue, otherwise, redirect to Login page.
        //    if (await IsYourLoginStillTrue(System.Web.HttpContext.Current.User.Identity.Name.Split('-')[0], Session["sessionid"].ToString()))
        //    {
        //        // check to see if your user ID is being used elsewhere under a different session ID
        //        if (!await IsUserLoggedOnElsewhere(System.Web.HttpContext.Current.User.Identity.Name.Split('-')[0], Session["sessionid"].ToString()))
        //        {
        //            TempData["appointmentAutoId"] = id;
        //            TempData["patientAutoId"] = patient;
        //            TempData["fee"] = fee;
        //            List<SelectListItem> paymentmode = new List<SelectListItem>() {
        //                new SelectListItem {
        //                    Text = "Cash", Value = "Cash"
        //                },
        //                new SelectListItem {
        //                    Text = "Paytm", Value = "Paytm"
        //                },
        //                new SelectListItem {
        //                    Text = "Credit Card", Value = "Credit Card"
        //                },
        //                new SelectListItem {
        //                    Text = "Debit Card", Value = "Debit Card"
        //                },
        //            };
        //            ViewBag.PAYMENTMODES = paymentmode;
        //            return View();
        //        }
        //        else
        //        {
        //            // if it is being used elsewhere, update all their 
        //            // Logins records to LoggedIn = false, except for your session ID
        //            LogEveryoneElseOut(System.Web.HttpContext.Current.User.Identity.Name.Split('-')[0], Session["sessionid"].ToString());
        //            TempData["appointmentAutoId"] = id;
        //            TempData["patientAutoId"] = patient;
        //            TempData["fee"] = fee;
        //            List<SelectListItem> paymentmode = new List<SelectListItem>() {
        //        new SelectListItem {
        //            Text = "Cash", Value = "Cash"
        //        },
        //        new SelectListItem {
        //            Text = "Paytm", Value = "Paytm"
        //        },
        //        new SelectListItem {
        //            Text = "Credit Card", Value = "Credit Card"
        //        },
        //        new SelectListItem {
        //            Text = "Debit Card", Value = "Debit Card"
        //        },
        //    };
        //            ViewBag.PAYMENTMODES = paymentmode;
        //            return View();
        //        }
        //    }
        //    else
        //    {
        //        FormsAuthentication.SignOut();
        //        return RedirectToAction("Login", "Home");
        //    }
        //}

        //public ActionResult Fee1(string id,string patient,string fee)
        //{
        //    TempData["appointmentAutoId"] = id;
        //    TempData["patientAutoId"] = patient;
        //    TempData["fee"] = fee;
        //    List<SelectListItem> paymentmode = new List<SelectListItem>() {
        //        new SelectListItem {
        //            Text = "Cash", Value = "Cash"
        //        },
        //        new SelectListItem {
        //            Text = "Paytm", Value = "Paytm"
        //        },
        //        new SelectListItem {
        //            Text = "Credit Card", Value = "Credit Card"
        //        },
        //        new SelectListItem {
        //            Text = "Debit Card", Value = "Debit Card"
        //        },
        //    };
        //    ViewBag.PAYMENTMODES = paymentmode;
        //    return View();
        //}


        [HttpPost]
        public async Task<ActionResult> Fee()
        {
            var authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            string savedString = "";
            string ClinicMobileNumber = "";
            string ClinicFirebaseDocumentId = "";
            if (authCookie != null)
            {
                var ticket = FormsAuthentication.Decrypt(authCookie.Value);
                if (ticket != null)
                {
                    savedString = ticket.Name; // Get the stored string
                    ClinicMobileNumber = savedString.Split('|')[3];
                    ClinicFirebaseDocumentId = savedString.Split('|')[4];
                }
            }
            if (Session["sessionid"] == null)
            { Session["sessionid"] = "empty"; }

            try
            {
                string patientAutoId = TempData["patientAutoId"].ToString();
                string appointmentAutoId = TempData["appointmentAutoId"].ToString();

                string Path = AppDomain.CurrentDomain.BaseDirectory + @"greenpaperdev-firebase-adminsdk-8k2y5-fb46e63414.json";
                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path);
                FirestoreDb db = FirestoreDb.Create("greenpaperdev");

                DocumentReference docRef = db.Collection("clinics").Document(ClinicFirebaseDocumentId).Collection("appointments").Document(appointmentAutoId);
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

        //[HttpPost]
        //public async Task<ActionResult> Fee2()
        //{
        //    var authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
        //    string savedString = "";
        //    string ClinicMobileNumber = "";
        //    string ClinicFirebaseDocumentId = "";
        //    if (authCookie != null)
        //    {
        //        var ticket = FormsAuthentication.Decrypt(authCookie.Value);
        //        if (ticket != null)
        //        {
        //            savedString = ticket.Name; // Get the stored string
        //            ClinicMobileNumber = savedString.Split('|')[3];
        //            ClinicFirebaseDocumentId = savedString.Split('|')[4];
        //        }
        //    }
        //    if (Session["sessionid"] == null)
        //    { Session["sessionid"] = "empty"; }

        //    // check to see if your ID in the Logins table has 
        //    // LoggedIn = true - if so, continue, otherwise, redirect to Login page.
        //    if (await IsYourLoginStillTrue(System.Web.HttpContext.Current.User.Identity.Name.Split('-')[0], Session["sessionid"].ToString()))
        //    {
        //        // check to see if your user ID is being used elsewhere under a different session ID
        //        if (!await IsUserLoggedOnElsewhere(System.Web.HttpContext.Current.User.Identity.Name.Split('-')[0], Session["sessionid"].ToString()))
        //        {
        //            try
        //            {
        //                string patientAutoId = TempData["patientAutoId"].ToString();
        //                string appointmentAutoId = TempData["appointmentAutoId"].ToString();

        //                string Path = AppDomain.CurrentDomain.BaseDirectory + @"greenpaperdev-firebase-adminsdk-8k2y5-fb46e63414.json";
        //                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path);
        //                FirestoreDb db = FirestoreDb.Create("greenpaperdev");

        //                DocumentReference docRef = db.Collection("clinics").Document(ClinicFirebaseDocumentId).Collection("appointments").Document(appointmentAutoId);
        //                DocumentSnapshot docSnap = await docRef.GetSnapshotAsync();

        //                if (docSnap.Exists)
        //                {
        //                    Dictionary<string, object> data1 = new Dictionary<string, object>
        //                {
        //                    {"completiondateCashier" ,DateTime.UtcNow},
        //                    {"statusCashier" ,"Completed"}

        //                };


        //                    await docRef.UpdateAsync(data1);

        //                }
        //                // TODO: Add delete logic here

        //                return RedirectToAction("Index");
        //            }
        //            catch
        //            {
        //                return View();
        //            }
        //        }
        //        else
        //        {
        //            // if it is being used elsewhere, update all their 
        //            // Logins records to LoggedIn = false, except for your session ID
        //            LogEveryoneElseOut(System.Web.HttpContext.Current.User.Identity.Name.Split('-')[0], Session["sessionid"].ToString());
        //            try
        //            {
        //                string patientAutoId = TempData["patientAutoId"].ToString();
        //                string appointmentAutoId = TempData["appointmentAutoId"].ToString();

        //                string Path = AppDomain.CurrentDomain.BaseDirectory + @"greenpaperdev-firebase-adminsdk-8k2y5-fb46e63414.json";
        //                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path);
        //                FirestoreDb db = FirestoreDb.Create("greenpaperdev");

        //                DocumentReference docRef = db.Collection("clinics").Document(ClinicFirebaseDocumentId).Collection("appointments").Document(appointmentAutoId);
        //                DocumentSnapshot docSnap = await docRef.GetSnapshotAsync();

        //                if (docSnap.Exists)
        //                {
        //                    Dictionary<string, object> data1 = new Dictionary<string, object>
        //                {
        //                    {"completiondateCashier" ,DateTime.UtcNow},
        //                    {"statusCashier" ,"Completed"}

        //                };


        //                    await docRef.UpdateAsync(data1);

        //                }
        //                // TODO: Add delete logic here

        //                return RedirectToAction("Index");
        //            }
        //            catch
        //            {
        //                return View();
        //            }
        //        }
        //    }
        //    else
        //    {
        //        FormsAuthentication.SignOut();
        //        return RedirectToAction("Login", "Home");
        //    }
        //}

        // POST: Appointment/Create
        //[HttpPost]
        //public async Task<ActionResult> Fee1()
        //{
        //    try
        //    {
        //        string patientAutoId = TempData["patientAutoId"].ToString();
        //        string appointmentAutoId = TempData["appointmentAutoId"].ToString();

        //        string Path = AppDomain.CurrentDomain.BaseDirectory + @"greenpaperdev-firebase-adminsdk-8k2y5-fb46e63414.json";
        //        Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path);
        //        FirestoreDb db = FirestoreDb.Create("greenpaperdev");

        //        DocumentReference docRef = db.Collection("clinics").Document(GlobalSessionVariables.ClinicDocumentAutoId).Collection("appointments").Document(appointmentAutoId);
        //        DocumentSnapshot docSnap = await docRef.GetSnapshotAsync();

        //        if (docSnap.Exists)
        //        {
        //            Dictionary<string, object> data1 = new Dictionary<string, object>
        //                {
        //                    {"completiondateCashier" ,DateTime.UtcNow},
        //                    {"statusCashier" ,"Completed"}

        //                };


        //            await docRef.UpdateAsync(data1);

        //        }
        //        // TODO: Add delete logic here

        //        return RedirectToAction("Index");
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}

        [HttpPost]
        public async Task<ActionResult> SubmitCashier(FormCollection collection)
        {
            var authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            string savedString = "";
            string ClinicMobileNumber = "";
            string ClinicFirebaseDocumentId = "";
            if (authCookie != null)
            {
                var ticket = FormsAuthentication.Decrypt(authCookie.Value);
                if (ticket != null)
                {
                    savedString = ticket.Name; // Get the stored string
                    ClinicMobileNumber = savedString.Split('|')[3];
                    ClinicFirebaseDocumentId = savedString.Split('|')[4];
                }
            }

            if (Session["sessionid"] == null)
            { Session["sessionid"] = "empty"; }

            try
            {

                string appointmentAutoId = collection["appointmentAutoIdFee"];

                string Path = AppDomain.CurrentDomain.BaseDirectory + @"greenpaperdev-firebase-adminsdk-8k2y5-fb46e63414.json";
                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path);
                FirestoreDb db = FirestoreDb.Create("greenpaperdev");

                DocumentReference docRef = db.Collection("clinics").Document(ClinicFirebaseDocumentId).Collection("appointments").Document(appointmentAutoId);
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

        //[HttpPost]
        //public async Task<ActionResult> SubmitCashier1(FormCollection collection)
        //{
        //    var authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
        //    string savedString = "";
        //    string ClinicMobileNumber = "";
        //    string ClinicFirebaseDocumentId = "";
        //    if (authCookie != null)
        //    {
        //        var ticket = FormsAuthentication.Decrypt(authCookie.Value);
        //        if (ticket != null)
        //        {
        //            savedString = ticket.Name; // Get the stored string
        //            ClinicMobileNumber = savedString.Split('|')[3];
        //            ClinicFirebaseDocumentId = savedString.Split('|')[4];
        //        }
        //    }

        //    if (Session["sessionid"] == null)
        //    { Session["sessionid"] = "empty"; }

        //    // check to see if your ID in the Logins table has 
        //    // LoggedIn = true - if so, continue, otherwise, redirect to Login page.
        //    if (await IsYourLoginStillTrue(System.Web.HttpContext.Current.User.Identity.Name.Split('-')[0], Session["sessionid"].ToString()))
        //    {
        //        // check to see if your user ID is being used elsewhere under a different session ID
        //        if (!await IsUserLoggedOnElsewhere(System.Web.HttpContext.Current.User.Identity.Name.Split('-')[0], Session["sessionid"].ToString()))
        //        {
        //            try
        //            {

        //                string appointmentAutoId = collection["appointmentAutoIdFee"];

        //                string Path = AppDomain.CurrentDomain.BaseDirectory + @"greenpaperdev-firebase-adminsdk-8k2y5-fb46e63414.json";
        //                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path);
        //                FirestoreDb db = FirestoreDb.Create("greenpaperdev");

        //                DocumentReference docRef = db.Collection("clinics").Document(ClinicFirebaseDocumentId).Collection("appointments").Document(appointmentAutoId);
        //                DocumentSnapshot docSnap = await docRef.GetSnapshotAsync();

        //                if (docSnap.Exists)
        //                {
        //                    Dictionary<string, object> data1 = new Dictionary<string, object>
        //                {
        //                    {"completiondateCashier" ,DateTime.UtcNow},
        //                    {"statusCashier" ,"Completed"},
        //                    {"modeofpayment",collection["modeofpaymentFee"]}
        //                };


        //                    await docRef.UpdateAsync(data1);

        //                }
        //                // TODO: Add delete logic here

        //                return RedirectToAction("Waiting");
        //            }
        //            catch
        //            {
        //                return View();
        //            }
        //        }
        //        else
        //        {
        //            // if it is being used elsewhere, update all their 
        //            // Logins records to LoggedIn = false, except for your session ID
        //            LogEveryoneElseOut(System.Web.HttpContext.Current.User.Identity.Name.Split('-')[0], Session["sessionid"].ToString());
        //            try
        //            {

        //                string appointmentAutoId = collection["appointmentAutoIdFee"];

        //                string Path = AppDomain.CurrentDomain.BaseDirectory + @"greenpaperdev-firebase-adminsdk-8k2y5-fb46e63414.json";
        //                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path);
        //                FirestoreDb db = FirestoreDb.Create("greenpaperdev");

        //                DocumentReference docRef = db.Collection("clinics").Document(ClinicFirebaseDocumentId).Collection("appointments").Document(appointmentAutoId);
        //                DocumentSnapshot docSnap = await docRef.GetSnapshotAsync();

        //                if (docSnap.Exists)
        //                {
        //                    Dictionary<string, object> data1 = new Dictionary<string, object>
        //                {
        //                    {"completiondateCashier" ,DateTime.UtcNow},
        //                    {"statusCashier" ,"Completed"},
        //                    {"modeofpayment",collection["modeofpaymentFee"]}
        //                };


        //                    await docRef.UpdateAsync(data1);

        //                }
        //                // TODO: Add delete logic here

        //                return RedirectToAction("Waiting");
        //            }
        //            catch
        //            {
        //                return View();
        //            }
        //        }
        //    }
        //    else
        //    {
        //        FormsAuthentication.SignOut();
        //        return RedirectToAction("Login", "Home");
        //    }
        //}

        [HttpPost]
        public async Task<ActionResult> SubmitChemist(FormCollection collection)
        {
            var authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            string savedString = "";
            string ClinicMobileNumber = "";
            string ClinicFirebaseDocumentId = "";
            if (authCookie != null)
            {
                var ticket = FormsAuthentication.Decrypt(authCookie.Value);
                if (ticket != null)
                {
                    savedString = ticket.Name; // Get the stored string
                    ClinicMobileNumber = savedString.Split('|')[3];
                    ClinicFirebaseDocumentId = savedString.Split('|')[4];
                }
            }

            if (Session["sessionid"] == null)
            { Session["sessionid"] = "empty"; }

            try
            {

                string appointmentAutoId = collection["appid"]; ;

                string Path = AppDomain.CurrentDomain.BaseDirectory + @"greenpaperdev-firebase-adminsdk-8k2y5-fb46e63414.json";
                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path);
                FirestoreDb db = FirestoreDb.Create("greenpaperdev");

                DocumentReference docRef = db.Collection("clinics").Document(ClinicFirebaseDocumentId).Collection("appointments").Document(appointmentAutoId);
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
        public async Task<ActionResult> SubmitChemistInventoryOn1(FormCollection collection)
        {
            var authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            string savedString = "";
            string ClinicMobileNumber = "";
            string ClinicFirebaseDocumentId = "";
            if (authCookie != null)
            {
                var ticket = FormsAuthentication.Decrypt(authCookie.Value);
                if (ticket != null)
                {
                    savedString = ticket.Name; // Get the stored string
                    ClinicMobileNumber = savedString.Split('|')[3];
                    ClinicFirebaseDocumentId = savedString.Split('|')[4];
                }
            }

            if (Session["sessionid"] == null)
            { Session["sessionid"] = "empty"; }

            try
            {

                string appointmentAutoId = collection["appointmentAutoIdMedicine"];

                string Path = AppDomain.CurrentDomain.BaseDirectory + @"greenpaperdev-firebase-adminsdk-8k2y5-fb46e63414.json";
                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path);
                FirestoreDb db = FirestoreDb.Create("greenpaperdev");

                DocumentReference docRef = db.Collection("clinics").Document(ClinicFirebaseDocumentId).Collection("appointments").Document(appointmentAutoId);
                DocumentSnapshot docSnap = await docRef.GetSnapshotAsync();

                if (docSnap.Exists)
                {
                    Dictionary<string, object> data1 = new Dictionary<string, object>
                        {
                            {"completiondateChemist" ,DateTime.UtcNow},
                            {"statusChemist" ,"Completed"},
                            {"modeofpaymentChemist",collection["modeofpaymentMedicine"]}

                        };


                    await docRef.UpdateAsync(data1);

                }
                // TODO: Add delete logic here

                return RedirectToAction("Waiting");
            }
            catch
            {
                return RedirectToAction("Waiting");
            }


        }

        [HttpPost]
        public async Task<ActionResult> CashierChemistUpdate(FormCollection collection)
        {
            var authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            string savedString = "";
            string ClinicMobileNumber = "";
            string ClinicFirebaseDocumentId = "";
            if (authCookie != null)
            {
                var ticket = FormsAuthentication.Decrypt(authCookie.Value);
                if (ticket != null)
                {
                    savedString = ticket.Name; // Get the stored string
                    ClinicMobileNumber = savedString.Split('|')[3];
                    ClinicFirebaseDocumentId = savedString.Split('|')[4];
                }
            }

            if (Session["sessionid"] == null)
            { Session["sessionid"] = "empty"; }

            try
            {

                string appointmentAutoId = collection["appointmentAutoId"];

                string Path = AppDomain.CurrentDomain.BaseDirectory + @"greenpaperdev-firebase-adminsdk-8k2y5-fb46e63414.json";
                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path);
                FirestoreDb db = FirestoreDb.Create("greenpaperdev");

                DocumentReference docRef = db.Collection("clinics").Document(ClinicFirebaseDocumentId).Collection("appointments").Document(appointmentAutoId);
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

        //[HttpPost]
        //public async Task<ActionResult> SubmitChemist1(FormCollection collection)
        //{
        //    var authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
        //    string savedString = "";
        //    string ClinicMobileNumber = "";
        //    string ClinicFirebaseDocumentId = "";
        //    if (authCookie != null)
        //    {
        //        var ticket = FormsAuthentication.Decrypt(authCookie.Value);
        //        if (ticket != null)
        //        {
        //            savedString = ticket.Name; // Get the stored string
        //            ClinicMobileNumber = savedString.Split('|')[3];
        //            ClinicFirebaseDocumentId = savedString.Split('|')[4];
        //        }
        //    }

        //    if (Session["sessionid"] == null)
        //    { Session["sessionid"] = "empty"; }

        //    // check to see if your ID in the Logins table has 
        //    // LoggedIn = true - if so, continue, otherwise, redirect to Login page.
        //    if (await IsYourLoginStillTrue(System.Web.HttpContext.Current.User.Identity.Name.Split('-')[0], Session["sessionid"].ToString()))
        //    {
        //        // check to see if your user ID is being used elsewhere under a different session ID
        //        if (!await IsUserLoggedOnElsewhere(System.Web.HttpContext.Current.User.Identity.Name.Split('-')[0], Session["sessionid"].ToString()))
        //        {
        //            try
        //            {

        //                string appointmentAutoId = collection["appid"]; ;

        //                string Path = AppDomain.CurrentDomain.BaseDirectory + @"greenpaperdev-firebase-adminsdk-8k2y5-fb46e63414.json";
        //                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path);
        //                FirestoreDb db = FirestoreDb.Create("greenpaperdev");

        //                DocumentReference docRef = db.Collection("clinics").Document(ClinicFirebaseDocumentId).Collection("appointments").Document(appointmentAutoId);
        //                DocumentSnapshot docSnap = await docRef.GetSnapshotAsync();

        //                if (docSnap.Exists)
        //                {
        //                    Dictionary<string, object> data1 = new Dictionary<string, object>
        //                {
        //                    {"completiondateChemist" ,DateTime.UtcNow},
        //                    {"statusChemist" ,"Completed"}
        //                };


        //                    await docRef.UpdateAsync(data1);

        //                }
        //                // TODO: Add delete logic here

        //                return RedirectToAction("Waiting");
        //            }
        //            catch
        //            {
        //                return View();
        //            }
        //        }
        //        else
        //        {
        //            // if it is being used elsewhere, update all their 
        //            // Logins records to LoggedIn = false, except for your session ID
        //            LogEveryoneElseOut(System.Web.HttpContext.Current.User.Identity.Name.Split('-')[0], Session["sessionid"].ToString());
        //            try
        //            {

        //                string appointmentAutoId = collection["appid"]; ;

        //                string Path = AppDomain.CurrentDomain.BaseDirectory + @"greenpaperdev-firebase-adminsdk-8k2y5-fb46e63414.json";
        //                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path);
        //                FirestoreDb db = FirestoreDb.Create("greenpaperdev");

        //                DocumentReference docRef = db.Collection("clinics").Document(ClinicFirebaseDocumentId).Collection("appointments").Document(appointmentAutoId);
        //                DocumentSnapshot docSnap = await docRef.GetSnapshotAsync();

        //                if (docSnap.Exists)
        //                {
        //                    Dictionary<string, object> data1 = new Dictionary<string, object>
        //                {
        //                    {"completiondateChemist" ,DateTime.UtcNow},
        //                    {"statusChemist" ,"Completed"}
        //                };


        //                    await docRef.UpdateAsync(data1);

        //                }
        //                // TODO: Add delete logic here

        //                return RedirectToAction("Waiting");
        //            }
        //            catch
        //            {
        //                return View();
        //            }
        //        }
        //    }
        //    else
        //    {
        //        FormsAuthentication.SignOut();
        //        return RedirectToAction("Login", "Home");
        //    }
        //}


        //[HttpPost]
        //public async Task<ActionResult> SubmitChemistInventoryOn1(FormCollection collection)
        //{
        //    var authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
        //    string savedString = "";
        //    string ClinicMobileNumber = "";
        //    string ClinicFirebaseDocumentId = "";
        //    if (authCookie != null)
        //    {
        //        var ticket = FormsAuthentication.Decrypt(authCookie.Value);
        //        if (ticket != null)
        //        {
        //            savedString = ticket.Name; // Get the stored string
        //            ClinicMobileNumber = savedString.Split('|')[3];
        //            ClinicFirebaseDocumentId = savedString.Split('|')[4];
        //        }
        //    }

        //    if (Session["sessionid"] == null)
        //    { Session["sessionid"] = "empty"; }

        //    // check to see if your ID in the Logins table has 
        //    // LoggedIn = true - if so, continue, otherwise, redirect to Login page.
        //    if (await IsYourLoginStillTrue(System.Web.HttpContext.Current.User.Identity.Name.Split('-')[0], Session["sessionid"].ToString()))
        //    {
        //        // check to see if your user ID is being used elsewhere under a different session ID
        //        if (!await IsUserLoggedOnElsewhere(System.Web.HttpContext.Current.User.Identity.Name.Split('-')[0], Session["sessionid"].ToString()))
        //        {
        //            try
        //            {

        //                string appointmentAutoId = collection["appointmentAutoIdMedicine"]; 

        //                string Path = AppDomain.CurrentDomain.BaseDirectory + @"greenpaperdev-firebase-adminsdk-8k2y5-fb46e63414.json";
        //                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path);
        //                FirestoreDb db = FirestoreDb.Create("greenpaperdev");

        //                DocumentReference docRef = db.Collection("clinics").Document(ClinicFirebaseDocumentId).Collection("appointments").Document(appointmentAutoId);
        //                DocumentSnapshot docSnap = await docRef.GetSnapshotAsync();

        //                if (docSnap.Exists)
        //                {
        //                    Dictionary<string, object> data1 = new Dictionary<string, object>
        //                {
        //                    {"completiondateChemist" ,DateTime.UtcNow},
        //                    {"statusChemist" ,"Completed"},
        //                    {"modeofpaymentChemist",collection["modeofpaymentMedicine"]}

        //                };


        //                    await docRef.UpdateAsync(data1);

        //                }
        //                // TODO: Add delete logic here

        //                return RedirectToAction("Waiting");
        //            }
        //            catch
        //            {
        //                return RedirectToAction("Waiting");
        //            }
        //        }
        //        else
        //        {
        //            // if it is being used elsewhere, update all their 
        //            // Logins records to LoggedIn = false, except for your session ID
        //            LogEveryoneElseOut(System.Web.HttpContext.Current.User.Identity.Name.Split('-')[0], Session["sessionid"].ToString());
        //            try
        //            {

        //                string appointmentAutoId = collection["appointmentAutoId"]; ;

        //                string Path = AppDomain.CurrentDomain.BaseDirectory + @"greenpaperdev-firebase-adminsdk-8k2y5-fb46e63414.json";
        //                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path);
        //                FirestoreDb db = FirestoreDb.Create("greenpaperdev");

        //                DocumentReference docRef = db.Collection("clinics").Document(ClinicFirebaseDocumentId).Collection("appointments").Document(appointmentAutoId);
        //                DocumentSnapshot docSnap = await docRef.GetSnapshotAsync();

        //                if (docSnap.Exists)
        //                {
        //                    Dictionary<string, object> data1 = new Dictionary<string, object>
        //                {
        //                    {"completiondateChemist" ,DateTime.UtcNow},
        //                    {"statusChemist" ,"Completed"},
        //                    {"modeofpaymentChemist",collection["modeofpaymentMedicine"]}
        //                };


        //                    await docRef.UpdateAsync(data1);

        //                }
        //                // TODO: Add delete logic here

        //                return RedirectToAction("Waiting");
        //            }
        //            catch
        //            {
        //                return RedirectToAction("Waiting");
        //            }
        //        }
        //    }
        //    else
        //    {
        //        FormsAuthentication.SignOut();
        //        return RedirectToAction("Login", "Home");
        //    }
        //}

        //[HttpPost]
        //public async Task<ActionResult> CashierChemistUpdate1(FormCollection collection)
        //{
        //    var authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
        //    string savedString = "";
        //    string ClinicMobileNumber = "";
        //    string ClinicFirebaseDocumentId = "";
        //    if (authCookie != null)
        //    {
        //        var ticket = FormsAuthentication.Decrypt(authCookie.Value);
        //        if (ticket != null)
        //        {
        //            savedString = ticket.Name; // Get the stored string
        //            ClinicMobileNumber = savedString.Split('|')[3];
        //            ClinicFirebaseDocumentId = savedString.Split('|')[4];
        //        }
        //    }

        //    if (Session["sessionid"] == null)
        //    { Session["sessionid"] = "empty"; }

        //    // check to see if your ID in the Logins table has 
        //    // LoggedIn = true - if so, continue, otherwise, redirect to Login page.
        //    if (await IsYourLoginStillTrue(System.Web.HttpContext.Current.User.Identity.Name.Split('-')[0], Session["sessionid"].ToString()))
        //    {
        //        // check to see if your user ID is being used elsewhere under a different session ID
        //        if (!await IsUserLoggedOnElsewhere(System.Web.HttpContext.Current.User.Identity.Name.Split('-')[0], Session["sessionid"].ToString()))
        //        {
        //            try
        //            {

        //                string appointmentAutoId = collection["appointmentAutoId"];

        //                string Path = AppDomain.CurrentDomain.BaseDirectory + @"greenpaperdev-firebase-adminsdk-8k2y5-fb46e63414.json";
        //                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path);
        //                FirestoreDb db = FirestoreDb.Create("greenpaperdev");

        //                DocumentReference docRef = db.Collection("clinics").Document(ClinicFirebaseDocumentId).Collection("appointments").Document(appointmentAutoId);
        //                DocumentSnapshot docSnap = await docRef.GetSnapshotAsync();

        //                if (docSnap.Exists)
        //                {
        //                    Dictionary<string, object> data1 = new Dictionary<string, object>
        //                {
        //                    {"completiondateCashier" ,DateTime.UtcNow},
        //                    {"statusCashier" ,"Completed"},
        //                    {"completiondateChemist" ,DateTime.UtcNow},
        //                    {"statusChemist" ,"Completed"},
        //                    {"modeofpayment",collection["modeofpayment"]}
        //                };


        //                    await docRef.UpdateAsync(data1);

        //                }
        //                // TODO: Add delete logic here

        //                return RedirectToAction("Waiting");
        //            }
        //            catch
        //            {
        //                return View();
        //            }
        //        }
        //        else
        //        {
        //            // if it is being used elsewhere, update all their 
        //            // Logins records to LoggedIn = false, except for your session ID
        //            LogEveryoneElseOut(System.Web.HttpContext.Current.User.Identity.Name.Split('-')[0], Session["sessionid"].ToString());
        //            try
        //            {

        //                string appointmentAutoId = collection["appointmentAutoId"];

        //                string Path = AppDomain.CurrentDomain.BaseDirectory + @"greenpaperdev-firebase-adminsdk-8k2y5-fb46e63414.json";
        //                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path);
        //                FirestoreDb db = FirestoreDb.Create("greenpaperdev");

        //                DocumentReference docRef = db.Collection("clinics").Document(ClinicFirebaseDocumentId).Collection("appointments").Document(appointmentAutoId);
        //                DocumentSnapshot docSnap = await docRef.GetSnapshotAsync();

        //                if (docSnap.Exists)
        //                {
        //                    Dictionary<string, object> data1 = new Dictionary<string, object>
        //                {
        //                    {"completiondateCashier" ,DateTime.UtcNow},
        //                    {"statusCashier" ,"Completed"},
        //                    {"completiondateChemist" ,DateTime.UtcNow},
        //                    {"statusChemist" ,"Completed"},
        //                    {"modeofpayment",collection["modeofpayment"]}
        //                };


        //                    await docRef.UpdateAsync(data1);

        //                }
        //                // TODO: Add delete logic here

        //                return RedirectToAction("Waiting");
        //            }
        //            catch
        //            {
        //                return View();
        //            }
        //        }
        //    }
        //    else
        //    {
        //        FormsAuthentication.SignOut();
        //        return RedirectToAction("Login", "Home");
        //    }
        //}

        [HttpPost]
        public async Task<ActionResult> CashierChemistUpdateInvOn(FormCollection collection)
        {
            var authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            string savedString = "";
            string ClinicMobileNumber = "";
            string ClinicFirebaseDocumentId = "";
            if (authCookie != null)
            {
                var ticket = FormsAuthentication.Decrypt(authCookie.Value);
                if (ticket != null)
                {
                    savedString = ticket.Name; // Get the stored string
                    ClinicMobileNumber = savedString.Split('|')[3];
                    ClinicFirebaseDocumentId = savedString.Split('|')[4];
                }
            }


            if (Session["sessionid"] == null)
            { Session["sessionid"] = "empty"; }

            try
            {

                string appointmentAutoId = collection["appId"];

                string Path = AppDomain.CurrentDomain.BaseDirectory + @"greenpaperdev-firebase-adminsdk-8k2y5-fb46e63414.json";
                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path);
                FirestoreDb db = FirestoreDb.Create("greenpaperdev");

                DocumentReference docRef = db.Collection("clinics").Document(ClinicFirebaseDocumentId).Collection("appointments").Document(appointmentAutoId);
                DocumentSnapshot docSnap = await docRef.GetSnapshotAsync();

                if (docSnap.Exists)
                {
                    Dictionary<string, object> data1 = new Dictionary<string, object>
                        {
                            {"completiondateCashier" ,DateTime.UtcNow},
                            {"statusCashier" ,"Completed"},
                            {"completiondateChemist" ,DateTime.UtcNow},
                            {"statusChemist" ,"Completed"},
                            {"modeofpayment",collection["modeofpayment"]},
                            {"modeofpaymentChemist",collection["modeofpayment"]}
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

        //[HttpPost]
        //public async Task<ActionResult> CashierChemistUpdateInvOn1(FormCollection collection)
        //{
        //    var authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
        //    string savedString = "";
        //    string ClinicMobileNumber = "";
        //    string ClinicFirebaseDocumentId = "";
        //    if (authCookie != null)
        //    {
        //        var ticket = FormsAuthentication.Decrypt(authCookie.Value);
        //        if (ticket != null)
        //        {
        //            savedString = ticket.Name; // Get the stored string
        //            ClinicMobileNumber = savedString.Split('|')[3];
        //            ClinicFirebaseDocumentId = savedString.Split('|')[4];
        //        }
        //    }


        //    if (Session["sessionid"] == null)
        //    { Session["sessionid"] = "empty"; }

        //    // check to see if your ID in the Logins table has 
        //    // LoggedIn = true - if so, continue, otherwise, redirect to Login page.
        //    if (await IsYourLoginStillTrue(System.Web.HttpContext.Current.User.Identity.Name.Split('-')[0], Session["sessionid"].ToString()))
        //    {
        //        // check to see if your user ID is being used elsewhere under a different session ID
        //        if (!await IsUserLoggedOnElsewhere(System.Web.HttpContext.Current.User.Identity.Name.Split('-')[0], Session["sessionid"].ToString()))
        //        {
        //            try
        //            {

        //                string appointmentAutoId = collection["appId"];

        //                string Path = AppDomain.CurrentDomain.BaseDirectory + @"greenpaperdev-firebase-adminsdk-8k2y5-fb46e63414.json";
        //                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path);
        //                FirestoreDb db = FirestoreDb.Create("greenpaperdev");

        //                DocumentReference docRef = db.Collection("clinics").Document(ClinicFirebaseDocumentId).Collection("appointments").Document(appointmentAutoId);
        //                DocumentSnapshot docSnap = await docRef.GetSnapshotAsync();

        //                if (docSnap.Exists)
        //                {
        //                    Dictionary<string, object> data1 = new Dictionary<string, object>
        //                {
        //                    {"completiondateCashier" ,DateTime.UtcNow},
        //                    {"statusCashier" ,"Completed"},
        //                    {"completiondateChemist" ,DateTime.UtcNow},
        //                    {"statusChemist" ,"Completed"},
        //                    {"modeofpayment",collection["modeofpayment"]},
        //                    {"modeofpaymentChemist",collection["modeofpayment"]}
        //                };


        //                    await docRef.UpdateAsync(data1);

        //                }
        //                // TODO: Add delete logic here

        //                return RedirectToAction("Waiting");
        //            }
        //            catch
        //            {
        //                return View();
        //            }
        //        }
        //        else
        //        {
        //            // if it is being used elsewhere, update all their 
        //            // Logins records to LoggedIn = false, except for your session ID
        //            LogEveryoneElseOut(System.Web.HttpContext.Current.User.Identity.Name.Split('-')[0], Session["sessionid"].ToString());
        //            try
        //            {

        //                string appointmentAutoId = collection["appointmentAutoId"];

        //                string Path = AppDomain.CurrentDomain.BaseDirectory + @"greenpaperdev-firebase-adminsdk-8k2y5-fb46e63414.json";
        //                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path);
        //                FirestoreDb db = FirestoreDb.Create("greenpaperdev");

        //                DocumentReference docRef = db.Collection("clinics").Document(ClinicFirebaseDocumentId).Collection("appointments").Document(appointmentAutoId);
        //                DocumentSnapshot docSnap = await docRef.GetSnapshotAsync();

        //                if (docSnap.Exists)
        //                {
        //                    Dictionary<string, object> data1 = new Dictionary<string, object>
        //                {
        //                    {"completiondateCashier" ,DateTime.UtcNow},
        //                    {"statusCashier" ,"Completed"},
        //                    {"completiondateChemist" ,DateTime.UtcNow},
        //                    {"statusChemist" ,"Completed"},
        //                    {"modeofpayment",collection["modeofpayment"]},
        //                    {"modeofpaymentChemist",collection["modeofpayment"]}
        //                };


        //                    await docRef.UpdateAsync(data1);

        //                }
        //                // TODO: Add delete logic here

        //                return RedirectToAction("Waiting");
        //            }
        //            catch
        //            {
        //                return View();
        //            }
        //        }
        //    }
        //    else
        //    {
        //        FormsAuthentication.SignOut();
        //        return RedirectToAction("Login", "Home");
        //    }
        //}

        public async static Task<bool> IsYourLoginStillTrue(string userId, string sid)
        {
            //string Path = AppDomain.CurrentDomain.BaseDirectory + @"greenpaperdev-firebase-adminsdk-8k2y5-fb46e63414.json";
            //Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path);
            //FirestoreDb db = FirestoreDb.Create("greenpaperdev");

            //Query QrefPatientLastId = db.Collection("clinics").Document(GlobalSessionVariables.ClinicDocumentAutoId).Collection("logins").WhereEqualTo("userid", userId).WhereEqualTo("sessionid", sid).WhereEqualTo("loggedin", true).Limit(1);
            //QuerySnapshot snapPatientLastId = await QrefPatientLastId.GetSnapshotAsync();
            //if (snapPatientLastId.Count > 0)
            //{ 
            //    return true;
            //}

            //return false;

            return true;
        }

        public async static Task<bool> IsUserLoggedOnElsewhere(string userId, string sid)
        {
            //int i = 0;
            //string Path = AppDomain.CurrentDomain.BaseDirectory + @"greenpaperdev-firebase-adminsdk-8k2y5-fb46e63414.json";
            //Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path);
            //FirestoreDb db = FirestoreDb.Create("greenpaperdev");

            //Query QrefUserLoggedInElseWhere = db.Collection("clinics").Document(GlobalSessionVariables.ClinicDocumentAutoId).Collection("logins").WhereEqualTo("userid", userId).WhereEqualTo("loggedin", true);
            //QuerySnapshot snapUserLoggedInElseWhere = await QrefUserLoggedInElseWhere.GetSnapshotAsync();
            //if (snapUserLoggedInElseWhere.Count > 0)
            //{

            //    foreach (DocumentSnapshot docsnapLoggedInUsers in snapUserLoggedInElseWhere)
            //    {
            //        if(docsnapLoggedInUsers.GetValue<string>("sessionid") != sid)
            //        {
            //            i = i + 1;
            //        }
            //    }
            //}
            //if (i > 0)
            //{ return true; }
            //else
            //{ return false; }

            return false;
            
        }

        private async Task<string> UpdateTokenNumber(string appointmentDate, string token)
        {
            var authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            string savedString = "";
            string ClinicMobileNumber = "";
            string ClinicFirebaseDocumentId = "";
            string ClinicCode = ""; 
            
            if (authCookie != null)
            {
                var ticket = FormsAuthentication.Decrypt(authCookie.Value);
                if (ticket != null)
                {
                    savedString = ticket.Name; // Get the stored string
                    ClinicMobileNumber = savedString.Split('|')[3];
                    ClinicFirebaseDocumentId = savedString.Split('|')[4];
                    ClinicCode = savedString.Split('|')[5];
                }
            }

            string lastTokenNumber = "";
            string flag = "Y";
            string Path = AppDomain.CurrentDomain.BaseDirectory + @"greenpaperdev-firebase-adminsdk-8k2y5-fb46e63414.json";
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path);
            FirestoreDb db = FirestoreDb.Create("greenpaperdev");
            DateTime futAppDate = Convert.ToDateTime(appointmentDate);

            #region Code to get latest token number, increament it, set in new appointment and update increamented token number back in collection
            DocumentReference docRefTokenNumber = db.Collection("clinics").Document(ClinicFirebaseDocumentId).Collection("tokenNumber").Document(futAppDate.ToString("dd_MM_yyyy"));
            DocumentSnapshot docsnapTokenNumber = await docRefTokenNumber.GetSnapshotAsync();


            if (docsnapTokenNumber.Exists)
            {
                lastTokenNumber = docsnapTokenNumber.GetValue<string>("last_token");
            }
            else
            {
                lastTokenNumber = "0";
                CollectionReference colTokenNumber = db.Collection("clinics").Document(ClinicFirebaseDocumentId).Collection("tokenNumber");

                Dictionary<string, object> dataTokenNumber = new Dictionary<string, object>
                            {
                                {"last_token" ,"0"},
                                {"assigned_tokens" ,null},
                                {"clinicCode" ,ClinicCode},
                                {"clinicId" ,ClinicFirebaseDocumentId},
                                {"isCreated" ,true},
                                {"isSynced" ,true},
                                {"UpdatedAt" ,DateTime.SpecifyKind(DateTime.Now.AddHours(-5).AddMinutes(-30), DateTimeKind.Utc)},
                            };
                await colTokenNumber.Document(futAppDate.ToString("dd_MM_yyyy")).SetAsync(dataTokenNumber);
            }

            string lastTokenNumberReturned = "";

            if (lastTokenNumber != "0")
            {
                string[] assignedtokens = docsnapTokenNumber.GetValue<string[]>("assigned_tokens");


                if (assignedtokens != null && assignedtokens.Count() != 0)
                {
                    int[] assignedtokensInt = Array.ConvertAll(assignedtokens, int.Parse);

                    Array.Sort(assignedtokensInt);
                    int j = 0;
                    for (int i = 0; i < assignedtokens.Length; i++)
                    {
                        if (Convert.ToInt32(lastTokenNumber) + 1 > assignedtokensInt[i])
                        {
                            lastTokenNumberReturned = (Convert.ToInt32(lastTokenNumber) + 1).ToString();
                            continue;
                        }
                        else if (Convert.ToInt32(lastTokenNumber) + 1 + j == assignedtokensInt[i])
                        {
                            j++;
                            lastTokenNumberReturned = (Convert.ToInt32(lastTokenNumber) + 1 + j).ToString();
                        }
                        else
                        {
                            lastTokenNumberReturned = (Convert.ToInt32(lastTokenNumber) + 1 + j).ToString();
                            break;
                        }

                    }
                }
                else
                {
                    lastTokenNumberReturned = (Convert.ToInt32(lastTokenNumber) + 1).ToString();
                }

            }
            else
            {
                lastTokenNumberReturned = (Convert.ToInt32(lastTokenNumber) + 1).ToString();
            }

            try
            {
                if (Convert.ToInt32(token) < Convert.ToInt32(lastTokenNumberReturned))
                {
                    ViewBag.Message = "Token Number " + token + " is already assigned.";

                }
            }
            catch (Exception ex)
            {

            }


            if (Convert.ToInt32(token) < 1)
            {
                ViewBag.Message = "Token Number can not be negative.";

            }
            if (Convert.ToInt32(token) >= Convert.ToInt32(lastTokenNumberReturned))
            {
                if (Convert.ToInt32(token) == 1)
                {

                    DocumentReference docRefTokenNumber2 = db.Collection("clinics").Document(ClinicFirebaseDocumentId).Collection("tokenNumber").Document(futAppDate.ToString("dd_MM_yyyy"));
                    DocumentSnapshot docsnapTokenNumber2 = await docRefTokenNumber2.GetSnapshotAsync();
                    string[] assignedtokens = docsnapTokenNumber2.GetValue<string[]>("assigned_tokens");

                    List<string> assignedtokenList = new List<string>();

                    if (assignedtokens != null)
                    {
                        for (int i = 0; i < assignedtokens.Length; i++)
                        {
                            if (token == assignedtokens[i].ToString())
                            {
                                TempData["Message"] = "Token Number " + token + " is already assigned.";
                                flag = "N";
                            }

                        }
                        assignedtokenList = assignedtokens.ToList();
                    }


                    if (flag == "Y")
                    {
                        CollectionReference colTokenNumber = db.Collection("clinics").Document(ClinicFirebaseDocumentId).Collection("tokenNumber");
                        if (assignedtokens != null)
                        {
                            Dictionary<string, object> dataTokenNumber = new Dictionary<string, object>
                            {
                                {"last_token" ,token},
                                {"assigned_tokens" ,assignedtokenList.ToArray()},
                                {"clinicCode" ,ClinicCode},
                                {"clinicId" ,ClinicFirebaseDocumentId},
                                {"isCreated" ,true},
                                {"isSynced" ,true},
                                {"UpdatedAt" ,DateTime.SpecifyKind(DateTime.Now.AddHours(-5).AddMinutes(-30), DateTimeKind.Utc)},
                            };
                            await colTokenNumber.Document(futAppDate.ToString("dd_MM_yyyy")).SetAsync(dataTokenNumber);
                        }
                        else
                        {
                            Dictionary<string, object> dataTokenNumber = new Dictionary<string, object>
                            {
                                {"last_token" ,token},
                                {"assigned_tokens" ,null},
                                {"clinicCode" ,ClinicCode},
                                {"clinicId" ,ClinicFirebaseDocumentId},
                                {"isCreated" ,true},
                                {"isSynced" ,true},
                                {"UpdatedAt" ,DateTime.SpecifyKind(DateTime.Now.AddHours(-5).AddMinutes(-30), DateTimeKind.Utc)},
                            };
                            await colTokenNumber.Document(futAppDate.ToString("dd_MM_yyyy")).SetAsync(dataTokenNumber);
                        }
                    }




                }
                else if (Convert.ToInt32(token) == Convert.ToInt32(lastTokenNumberReturned))
                {

                    string[] assignedtokens = docsnapTokenNumber.GetValue<string[]>("assigned_tokens");
                    List<string> assignedtokenList = new List<string>();
                    if (assignedtokens != null && assignedtokens.Count() != 0)
                    {
                        for (int i = 0; i < assignedtokens.Length; i++)
                        {
                            if (token == assignedtokens[i].ToString())
                            {
                                ViewBag.Message = "Token Number " + token + " is already assigned.";
                                flag = "N";
                            }

                        }
                        assignedtokenList = assignedtokens.ToList();

                        if (flag == "Y")
                        {
                            CollectionReference colTokenNumber = db.Collection("clinics").Document(ClinicFirebaseDocumentId).Collection("tokenNumber");

                            Dictionary<string, object> dataTokenNumber = new Dictionary<string, object>
                            {
                                {"last_token" ,token},
                                {"assigned_tokens" ,assignedtokenList.ToArray()},
                                {"clinicCode" ,ClinicCode},
                                {"clinicId" ,ClinicFirebaseDocumentId},
                                {"isCreated" ,true},
                                {"isSynced" ,true},
                                {"UpdatedAt" ,DateTime.SpecifyKind(DateTime.Now.AddHours(-5).AddMinutes(-30), DateTimeKind.Utc)},


                            };
                            await colTokenNumber.Document(futAppDate.ToString("dd_MM_yyyy")).SetAsync(dataTokenNumber);
                        }

                    }
                    else
                    {
                        CollectionReference colTokenNumber = db.Collection("clinics").Document(ClinicFirebaseDocumentId).Collection("tokenNumber");

                        Dictionary<string, object> dataTokenNumber = new Dictionary<string, object>
                            {
                                {"last_token" ,token},
                                {"assigned_tokens" ,null},
                                {"clinicCode" ,ClinicCode},
                                {"clinicId" ,ClinicFirebaseDocumentId},
                                {"isCreated" ,true},
                                {"isSynced" ,true},
                                {"UpdatedAt" ,DateTime.SpecifyKind(DateTime.Now.AddHours(-5).AddMinutes(-30), DateTimeKind.Utc)},
                            };
                        await colTokenNumber.Document(futAppDate.ToString("dd_MM_yyyy")).SetAsync(dataTokenNumber);
                    }
                }
                else
                {
                    DocumentReference docRefTokenNumber1 = db.Collection("clinics").Document(ClinicFirebaseDocumentId).Collection("tokenNumber").Document(futAppDate.ToString("dd_MM_yyyy"));
                    DocumentSnapshot docsnapTokenNumber1 = await docRefTokenNumber1.GetSnapshotAsync();
                    string[] assignedtokens = docsnapTokenNumber1.GetValue<string[]>("assigned_tokens");

                    List<string> assignedtokenList = new List<string>();

                    if (assignedtokens != null)
                    {
                        for (int i = 0; i < assignedtokens.Length; i++)
                        {
                            if (token == assignedtokens[i].ToString())
                            {
                                ViewBag.Message = "Token Number " + token + " is already assigned.";
                                flag = "N";
                            }

                        }
                        assignedtokenList = assignedtokens.ToList();
                    }



                    assignedtokenList.Add(token);

                    if (Convert.ToInt32(token) != Convert.ToInt32(lastTokenNumberReturned))
                    {
                        token = lastTokenNumber;
                    }

                    if (flag == "Y")
                    {
                        CollectionReference colTokenNumber = db.Collection("clinics").Document(ClinicFirebaseDocumentId).Collection("tokenNumber");

                        Dictionary<string, object> dataTokenNumber = new Dictionary<string, object>
                        {
                            {"last_token" ,token},
                            {"assigned_tokens" ,assignedtokenList.ToArray()},
                            {"clinicCode" ,ClinicCode},
                            {"clinicId" ,ClinicFirebaseDocumentId},
                            {"isCreated" ,true},
                            {"isSynced" ,true},
                            {"UpdatedAt" ,DateTime.SpecifyKind(DateTime.Now.AddHours(-5).AddMinutes(-30), DateTimeKind.Utc)},
                        };
                        await colTokenNumber.Document(futAppDate.ToString("dd_MM_yyyy")).SetAsync(dataTokenNumber);
                    }


                }
            }

            return ViewBag.Message;
            #endregion Code to get latest token number, increament it
        }

        //private async Task<string> UpdateTokenNumberValidation(string appointmentDate, string token)
        //{
        //    var authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
        //    string savedString = "";
        //    string ClinicMobileNumber = "";
        //    string ClinicFirebaseDocumentId = "";
        //    string ClinicCode = "";

        //    if (authCookie != null)
        //    {
        //        var ticket = FormsAuthentication.Decrypt(authCookie.Value);
        //        if (ticket != null)
        //        {
        //            savedString = ticket.Name; // Get the stored string
        //            ClinicMobileNumber = savedString.Split('|')[3];
        //            ClinicFirebaseDocumentId = savedString.Split('|')[4];
        //            ClinicCode = savedString.Split('|')[5];
        //        }
        //    }

        //    string lastTokenNumber = "";
        //    string flag = "Y";
        //    //string Path = AppDomain.CurrentDomain.BaseDirectory + @"greenpaperdev-firebase-adminsdk-8k2y5-fb46e63414.json";
        //    //Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path);
        //    //FirestoreDb db = FirestoreDb.Create("greenpaperdev");
        //    DateTime futAppDate = Convert.ToDateTime(appointmentDate);

        //    #region Code to get latest token number, increament it, set in new appointment and update increamented token number back in collection
        //    DocumentReference docRefTokenNumber = _db.Collection("clinics").Document(ClinicFirebaseDocumentId).Collection("tokenNumber").Document(futAppDate.ToString("dd_MM_yyyy"));
        //    DocumentSnapshot docsnapTokenNumber = await docRefTokenNumber.GetSnapshotAsync();


        //    if (docsnapTokenNumber.Exists)
        //    {
        //        lastTokenNumber = docsnapTokenNumber.GetValue<string>("last_token");
        //    }
        //    else
        //    {
        //        lastTokenNumber = "0";
        //    }

        //    string lastTokenNumberReturned = "";

        //    if (lastTokenNumber != "0")
        //    {
        //        string[] assignedtokens = docsnapTokenNumber.GetValue<string[]>("assigned_tokens");


        //        if (assignedtokens != null && assignedtokens.Count() != 0)
        //        {
        //            int[] assignedtokensInt = Array.ConvertAll(assignedtokens, int.Parse);

        //            Array.Sort(assignedtokensInt);
        //            int j = 0;
        //            for (int i = 0; i < assignedtokens.Length; i++)
        //            {
        //                if (Convert.ToInt32(lastTokenNumber) + 1 > assignedtokensInt[i])
        //                {
        //                    lastTokenNumberReturned = (Convert.ToInt32(lastTokenNumber) + 1).ToString();
        //                    continue;
        //                }
        //                else if (Convert.ToInt32(lastTokenNumber) + 1 + j == assignedtokensInt[i])
        //                {
        //                    j++;
        //                    lastTokenNumberReturned = (Convert.ToInt32(lastTokenNumber) + 1 + j).ToString();
        //                }
        //                else
        //                {
        //                    lastTokenNumberReturned = (Convert.ToInt32(lastTokenNumber) + 1 + j).ToString();
        //                    break;
        //                }

        //            }
        //        }
        //        else
        //        {
        //            lastTokenNumberReturned = (Convert.ToInt32(lastTokenNumber) + 1).ToString();
        //        }

        //    }
        //    else
        //    {
        //        lastTokenNumberReturned = (Convert.ToInt32(lastTokenNumber) + 1).ToString();
        //    }

        //    try
        //    {
        //        if (Convert.ToInt32(token) < Convert.ToInt32(lastTokenNumberReturned))
        //        {
        //            ViewBag.Message = "Token Number " + token + " is already assigned.";

        //        }
        //    }
        //    catch (Exception ex)
        //    {

        //    }


        //    if (Convert.ToInt32(token) < 1)
        //    {
        //        ViewBag.Message = "Token Number can not be negative.";

        //    }

        //    return ViewBag.Message;
        //    #endregion Code to get latest token number, increament it
        //}

        public async Task<JsonResult> TokenNumberValidation(string appointmentDate, string token)
        {
            if (Convert.ToInt32(token) < 1)
            {
                ViewBag.Message = "Token Number can not be negative.";
                return Json(new { success = false, message = "Token Number can not be negative." }, JsonRequestBehavior.AllowGet);
            }

            var authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            string savedString = "";
            string ClinicMobileNumber = "";
            string ClinicFirebaseDocumentId = "";
            string ClinicCode = "";

            if (authCookie != null)
            {
                var ticket = FormsAuthentication.Decrypt(authCookie.Value);
                if (ticket != null)
                {
                    savedString = ticket.Name; // Get the stored string
                    ClinicMobileNumber = savedString.Split('|')[3];
                    ClinicFirebaseDocumentId = savedString.Split('|')[4];
                    ClinicCode = savedString.Split('|')[5];
                }
            }

            string lastTokenNumber = "";
            string lastTokenNumberReturned = "";


            DateTime futAppDate = Convert.ToDateTime(appointmentDate);


            DocumentReference docRefTokenNumber = _db.Collection("clinics").Document(ClinicFirebaseDocumentId).Collection("tokenNumber").Document(futAppDate.ToString("dd_MM_yyyy"));
            DocumentSnapshot docsnapTokenNumber = await docRefTokenNumber.GetSnapshotAsync();


            if (docsnapTokenNumber.Exists)
            {
                lastTokenNumber = docsnapTokenNumber.GetValue<string>("last_token");
            }
            else
            {
                lastTokenNumber = "0";
            }

            

            if (lastTokenNumber != "0")
            {
                string[] assignedtokens = docsnapTokenNumber.GetValue<string[]>("assigned_tokens");


                if (assignedtokens != null && assignedtokens.Count() != 0)
                {
                    int[] assignedtokensInt = Array.ConvertAll(assignedtokens, int.Parse);

                    Array.Sort(assignedtokensInt);
                    int j = 0;
                    for (int i = 0; i < assignedtokens.Length; i++)
                    {
                        if (Convert.ToInt32(lastTokenNumber) + 1 > assignedtokensInt[i])
                        {
                            lastTokenNumberReturned = (Convert.ToInt32(lastTokenNumber) + 1).ToString();
                            continue;
                        }
                        else if (Convert.ToInt32(lastTokenNumber) + 1 + j == assignedtokensInt[i])
                        {
                            j++;
                            lastTokenNumberReturned = (Convert.ToInt32(lastTokenNumber) + 1 + j).ToString();
                        }
                        else
                        {
                            lastTokenNumberReturned = (Convert.ToInt32(lastTokenNumber) + 1 + j).ToString();
                            break;
                        }

                    }
                }
                else
                {
                    lastTokenNumberReturned = (Convert.ToInt32(lastTokenNumber) + 1).ToString();
                }

            }
            else
            {
                lastTokenNumberReturned = (Convert.ToInt32(lastTokenNumber) + 1).ToString();
            }

            try
            {
                if (Convert.ToInt32(token) < Convert.ToInt32(lastTokenNumberReturned))
                {
                    return Json(new { success = false, message = "Token Number " + token + " is already assigned." }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = true, message = "Token is ok" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpGet]
        public async Task<JsonResult> GetPatientImages(string patientId)
        {
            string patientDocumentId = "";
            var authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            string savedString = "";
            string ClinicMobileNumber = "";
            string ClinicFirebaseDocumentId = "";
            if (authCookie != null)
            {
                var ticket = FormsAuthentication.Decrypt(authCookie.Value);
                if (ticket != null)
                {
                    savedString = ticket.Name; // Get the stored string
                    ClinicMobileNumber = savedString.Split('|')[3];
                    ClinicFirebaseDocumentId = savedString.Split('|')[4];
                }
            }
            // Call your specific method here
            //string ClinicMobileNumber = GlobalSessionVariables.ClinicMobileNumber;
            //string Path = AppDomain.CurrentDomain.BaseDirectory + @"greenpaperdev-firebase-adminsdk-8k2y5-fb46e63414.json";
            //Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path);
            //FirestoreDb db = FirestoreDb.Create("greenpaperdev");

            List<ImageViewModel> ImageList = new List<ImageViewModel>();

            Query Qref = _db.Collection("clinics").WhereEqualTo("clinicmobilenumber", ClinicMobileNumber);
            QuerySnapshot snap = await Qref.GetSnapshotAsync();
            foreach (DocumentSnapshot docsnap in snap)
            {
                try
                {
                    Clinic clinic = docsnap.ConvertTo<Clinic>();
                    string ClinicCode = clinic.clinic_code;

                    #region to get patient documentid
                    QuerySnapshot snapPatient = await docsnap.Reference.Collection("patientList").WhereEqualTo("patient_id", patientId).WhereEqualTo("clinicCode", ClinicCode).GetSnapshotAsync();
                    DocumentSnapshot docPatient = snapPatient.Documents[0];

                    patientDocumentId = docPatient.Id;
                    #endregion

                    int i = 1;

                    Query QrefPrescriptions = _db.Collection("clinics").Document(ClinicFirebaseDocumentId).Collection("prescriptions").WhereEqualTo("patientId", patientDocumentId).OrderByDescending("timeStamp").Limit(5); 

                    QuerySnapshot snapPres = await QrefPrescriptions.GetSnapshotAsync();
                    if (snapPres.Count > 0)
                    {
                        foreach (DocumentSnapshot docsnapPres in snapPres)
                        {

                            if (docsnapPres.Exists)
                            {
                                ImageViewModel img = new ImageViewModel();
                                img.Id = i;
                                img.ImageUrl = "data:image/png;base64," + docsnapPres.GetValue<string>("file");
                                ImageList.Add(img);
                                i++;
                            }

                        }
                        //_objuserloginmodel.SelectedImage = ImageList[0];
                    }
                }
                catch (Exception ex)
                {

                    string str = ex.Message;
                }

            }

            // Example: Fetch image URLs from database or storage

            //var images = new List<string>
            //    {
            //        Url.Content("~/Images/logo copy.png"),
            //        Url.Content("~/Images/logo copy1.png"),
            //        Url.Content("~/Images/Downloading.gif")
            //    };

            return Json(new { ImageList = ImageList }, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public async Task<JsonResult> SaveCanvasImage(CanvasImageModal model)
        {
            if (model?.Base64Image != null)
            {
                // Remove the data URL scheme prefix if it exists
                string base64 = Regex.Replace(model.Base64Image, @"^data:image\/[a-zA-Z]+;base64,", string.Empty);
                string patientDocumentId = "";
                string UserMobileNumber = "";

                try
                {
                    //byte[] imageBytes = Convert.FromBase64String(base64);
                    var authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
                    string savedString = "";
                    string ClinicMobileNumber = "";
                    string ClinicFirebaseDocumentId = "";
                    if (authCookie != null)
                    {
                        var ticket = FormsAuthentication.Decrypt(authCookie.Value);
                        if (ticket != null)
                        {
                            savedString = ticket.Name; // Get the stored string
                            ClinicMobileNumber = savedString.Split('|')[3];
                            ClinicFirebaseDocumentId = savedString.Split('|')[4];
                            UserMobileNumber = savedString.Split('|')[0];
                        }
                    }
                    Query Qref = _db.Collection("clinics").WhereEqualTo("clinicmobilenumber", ClinicMobileNumber);
                    QuerySnapshot snap = await Qref.GetSnapshotAsync();
                    foreach (DocumentSnapshot docsnap in snap)
                    {
                        Clinic clinic = docsnap.ConvertTo<Clinic>();
                        string ClinicCode = clinic.clinic_code;

                        #region to get patient documentid
                        QuerySnapshot snapPatient = await docsnap.Reference.Collection("patientList").WhereEqualTo("patient_id", model.PatientId).WhereEqualTo("clinicCode", ClinicCode).GetSnapshotAsync();
                        DocumentSnapshot docPatient = snapPatient.Documents[0];

                        patientDocumentId = docPatient.Id;
                        #endregion

                        #region to get user documentid
                        QuerySnapshot snapUser = await docsnap.Reference.Collection("user").WhereEqualTo("mobile_number", UserMobileNumber).GetSnapshotAsync();
                        DocumentSnapshot docUser = snapUser.Documents[0];

                        string userId = docUser.Id;
                        #endregion

                        CollectionReference colPrescriptions = _db.Collection("clinics").Document(ClinicFirebaseDocumentId).Collection("prescriptions");
                        Dictionary<string, object> dataAppointment = new Dictionary<string, object>
                                {
                                    {"chemist" ,""},
                                    {"cashier" ,""},
                                    {"clinicCode" ,ClinicCode},
                                    {"clinicId" ,ClinicFirebaseDocumentId},
                                    {"date" ,""},
                                    {"file" ,base64},
                                    {"fileUrl" ,""},
                                    {"days" ,"7"},
                                    {"fee" ,"100"},
                                    {"patientId" ,patientDocumentId},
                                    {"timeStamp" ,DateTime.SpecifyKind(DateTime.Now.AddHours(-5).AddMinutes(-30), DateTimeKind.Utc)},
                                    {"doctor" ,userId},
                                    {"receptionist" ,""},
                                    {"isPrescription" ,"true"},
                                    {"isCreated" ,true},
                                    {"isSynced" ,true},
                                    {"isDeleted" ,false},
                                    {"UpdatedAt" ,DateTime.SpecifyKind(DateTime.Now.AddHours(-5).AddMinutes(-30), DateTimeKind.Utc)},

                                };
                        await colPrescriptions.Document().SetAsync(dataAppointment);
                    }

                    DocumentReference docRef = _db.Collection("clinics").Document(ClinicFirebaseDocumentId).Collection("appointments").Document(model.AppointmentDocId);
                    DocumentSnapshot docSnap = await docRef.GetSnapshotAsync();

                    if (docSnap.Exists)
                    {
                        Dictionary<string, object> data1 = new Dictionary<string, object>
                        {
                            {"completionDate" ,DateTime.UtcNow},
                            {"status" ,"Completed"},
                            {"UpdatedAt",DateTime.UtcNow},
                            {"days" ,"7"},
                            {"fee" ,"100"},
                        };


                        await docRef.UpdateAsync(data1);

                    }

                    return Json(new { success = true, message = "Image saved successfully" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = "Error saving image: " + ex.Message }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { success = false, message = "Invalid image data" }, JsonRequestBehavior.AllowGet);
        }


    }
}
