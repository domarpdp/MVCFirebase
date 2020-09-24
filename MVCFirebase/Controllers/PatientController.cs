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
    
    
    [AccessDeniedAuthorize(Roles = "Doctor,Receptionist")]
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
                QuerySnapshot snap2 = await docsnap.Reference.Collection("patientList").OrderByDescending("patient_id").Limit(10).GetSnapshotAsync();

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

            

            return View(PatientList);
            
        }

        // GET: Patient/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Patient/Create
        public async Task<ActionResult> Create()
        {
            List<SelectListItem> diseases = new List<SelectListItem>() {
                new SelectListItem {
                    Text = "General -(fever/general acute complaints)", Value = "GENERAL"
                },
                new SelectListItem {
                    Text = "ENT (ear nose throat)", Value = "ENT"
                },
                new SelectListItem {
                    Text = "CNS (central nervous system", Value = "CNS"
                },
                new SelectListItem {
                    Text = "DERMATO (skin/hair)", Value = "DERMATO"
                },
                new SelectListItem {
                    Text = "GIT (gastro intestinal track) SURGERY", Value = "GIT"
                },
                new SelectListItem {
                    Text = "TUMORS/ONCOLOGY", Value = "TUMOURS"
                },
                new SelectListItem {
                    Text = "HAEMATOLOGY (blood)", Value = "HAEMATOLOGY"
                },
                new SelectListItem {
                    Text = "ORTHOPADICS (bones & muscles)", Value = "ORTHOPADICS"
                },
                new SelectListItem {
                    Text = "PAEDIATRIC (child)", Value = "PAEDIATRIC"
                },
                new SelectListItem {
                    Text = "OPTHALMOLOGISTS (EYE)", Value = "EYE"
                },
                new SelectListItem {
                    Text = "CVS (cardio vascular system/heart)", Value = "CVS"
                },
                new SelectListItem {
                    Text = "ENDOCRINOLOGY", Value = "ENDO"
                },
                new SelectListItem {
                    Text = "GYNAECOLOGY & OBS", Value = "GYNAE"
                },
                new SelectListItem {
                    Text = "GENETICS", Value = "GENETICS"
                },
            };
            ViewBag.DISEASES = diseases;

            List<SelectListItem> cities = new List<SelectListItem>() {
                        new SelectListItem {
                            Text = "Faridabad", Value = "Faridabad"
                        },
                        new SelectListItem {
                            Text = "Ghaziabad", Value = "Ghaziabad"
                        },
                        new SelectListItem {
                            Text = "New Delhi", Value = "New Delhi"
                        },
                        new SelectListItem {
                            Text = "Bahadurgarh", Value = "Bahadurgarh"
                        },
                        new SelectListItem {
                            Text = "Mathura", Value = "Mathura"
                        },
                        new SelectListItem {
                            Text = "Agra", Value = "Agra"
                        },
                        new SelectListItem {
                            Text = "Ballabgarh", Value = "Ballabgarh"
                        },
                        new SelectListItem {
                            Text = "Gurdaspur", Value = "Gurdaspur"
                        },
                        new SelectListItem {
                            Text = "Amritsar", Value = "Amritsar"
                        },
                        new SelectListItem {
                            Text = "Batala", Value = "Batala"
                        },
                        new SelectListItem {
                            Text = "Jallandhar", Value = "Jallandhar"
                        },
                        new SelectListItem {
                            Text = "Pathankot", Value = "Pathankot"
                        },
                        new SelectListItem {
                            Text = "Bathinda", Value = "Bathinda"
                        },
                        new SelectListItem {
                            Text = "Ambala", Value = "Ambala"
                        },
                    };
            ViewBag.CITIES = cities;

            List<SelectListItem> gender = new List<SelectListItem>() {
                new SelectListItem {
                    Text = "Male", Value = "Male"
                },
                new SelectListItem {
                    Text = "Female", Value = "Female"
                },
                new SelectListItem {
                    Text = "Other", Value = "Other"
                },
                
            };
            ViewBag.GENDER = gender;

            List<SelectListItem> severity = new List<SelectListItem>() {
                new SelectListItem {
                    Text = "High", Value = "High"
                },
                new SelectListItem {
                    Text = "Medium", Value = "Medium"
                },
                new SelectListItem {
                    Text = "Low", Value = "Low"
                },

            };
            ViewBag.SEVERITIES = severity;

            string Path = AppDomain.CurrentDomain.BaseDirectory + @"greenpaperdev-firebase-adminsdk-8k2y5-fb46e63414.json";
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path);
            FirestoreDb db = FirestoreDb.Create("greenpaperdev");
            DocumentReference docRefClinicCity = db.Collection("clinics").Document(GlobalSessionVariables.ClinicDocumentAutoId);
            DocumentSnapshot docSnapClinicCity = await docRefClinicCity.GetSnapshotAsync();
            Clinic clinic = docSnapClinicCity.ConvertTo<Clinic>();
            Patient patient = new Patient();
            patient.city = clinic.cliniccity;
            patient.appointment_date = DateTime.Now;
            patient.tokenNumber = await getLatestToken(DateTime.Now.ToString("MM/dd/yyyy"));
            return View(patient);
        }

        // POST: Patient/Create
        [HttpPost]
        public async Task<ActionResult> Create(Patient patient)
        {
            try
            {
                string patientLastId = "";
                string patientLastIdDocId = "";
                
                List<SelectListItem> diseases = new List<SelectListItem>() {
                        new SelectListItem {
                            Text = "General -(fever/general acute complaints)", Value = "GENERAL"
                        },
                        new SelectListItem {
                            Text = "ENT (ear nose throat)", Value = "ENT"
                        },
                        new SelectListItem {
                            Text = "CNS (central nervous system", Value = "CNS"
                        },
                        new SelectListItem {
                            Text = "DERMATO (skin/hair)", Value = "DERMATO"
                        },
                        new SelectListItem {
                            Text = "GIT (gastro intestinal track) SURGERY", Value = "GIT"
                        },
                        new SelectListItem {
                            Text = "TUMORS/ONCOLOGY", Value = "TUMOURS"
                        },
                        new SelectListItem {
                            Text = "HAEMATOLOGY (blood)", Value = "HAEMATOLOGY"
                        },
                        new SelectListItem {
                            Text = "ORTHOPADICS (bones & muscles)", Value = "ORTHOPADICS"
                        },
                        new SelectListItem {
                            Text = "PAEDIATRIC (child)", Value = "PAEDIATRIC"
                        },
                        new SelectListItem {
                            Text = "OPTHALMOLOGISTS (EYE)", Value = "EYE"
                        },
                        new SelectListItem {
                            Text = "CVS (cardio vascular system/heart)", Value = "CVS"
                        },
                        new SelectListItem {
                            Text = "ENDOCRINOLOGY", Value = "ENDO"
                        },
                        new SelectListItem {
                            Text = "GYNAECOLOGY & OBS", Value = "GYNAE"
                        },
                        new SelectListItem {
                            Text = "GENETICS", Value = "GENETICS"
                        },
                    };
                ViewBag.DISEASES = diseases;

                List<SelectListItem> cities = new List<SelectListItem>() {
                        new SelectListItem {
                            Text = "Faridabad", Value = "Faridabad"
                        },
                        new SelectListItem {
                            Text = "Ghaziabad", Value = "Ghaziabad"
                        },
                        new SelectListItem {
                            Text = "New Delhi", Value = "New Delhi"
                        },
                        new SelectListItem {
                            Text = "Bahadurgarh", Value = "Bahadurgarh"
                        },
                        new SelectListItem {
                            Text = "Mathura", Value = "Mathura"
                        },
                        new SelectListItem {
                            Text = "Agra", Value = "Agra"
                        },
                        new SelectListItem {
                            Text = "Ballabgarh", Value = "Ballabgarh"
                        },
                        new SelectListItem {
                            Text = "Gurdaspur", Value = "Gurdaspur"
                        },
                        new SelectListItem {
                            Text = "Amritsar", Value = "Amritsar"
                        },
                        new SelectListItem {
                            Text = "Batala", Value = "Batala"
                        },
                        new SelectListItem {
                            Text = "Jallandhar", Value = "Jallandhar"
                        },
                        new SelectListItem {
                            Text = "Pathankot", Value = "Pathankot"
                        },
                        new SelectListItem {
                            Text = "Bathinda", Value = "Bathinda"
                        },
                        new SelectListItem {
                            Text = "Ambala", Value = "Ambala"
                        },
                    };
                ViewBag.CITIES = cities;

                List<SelectListItem> genders = new List<SelectListItem>() {
                        new SelectListItem {
                            Text = "Male", Value = "Male"
                        },
                        new SelectListItem {
                            Text = "Female", Value = "Female"
                        },
                        new SelectListItem {
                            Text = "Other", Value = "Other"
                        },

                    };
                ViewBag.GENDERS = genders;
                List<SelectListItem> severity = new List<SelectListItem>() {
                new SelectListItem {
                    Text = "High", Value = "High"
                },
                new SelectListItem {
                    Text = "Medium", Value = "Medium"
                },
                new SelectListItem {
                    Text = "Low", Value = "Low"
                },

            };
                ViewBag.SEVERITIES = severity;

                if (ModelState.IsValid)
                {
                    string message = await UpdateTokenNumber(patient.appointment_date.ToString(), patient.tokenNumber);
                    if (message != null)
                    {
                        ViewBag.Message = message;
                        return View(patient);
                    }

                    //string lastTokenNumber = "0";
                    string Path = AppDomain.CurrentDomain.BaseDirectory + @"greenpaperdev-firebase-adminsdk-8k2y5-fb46e63414.json";
                    Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path);
                    FirestoreDb db = FirestoreDb.Create("greenpaperdev");
                    DocumentReference docRefClinicCity = db.Collection("clinics").Document(GlobalSessionVariables.ClinicDocumentAutoId);
                    DocumentSnapshot docSnapClinicCity = await docRefClinicCity.GetSnapshotAsync();

                    #region Code to checkduplicacy of patient on the basis of name and mobile number
                    Query Qref = db.Collection("clinics").Document(GlobalSessionVariables.ClinicDocumentAutoId).Collection("patientList").WhereEqualTo("patient_name", patient.patient_name).WhereEqualTo("patient_mobile_number", patient.patient_mobile_number);
                    QuerySnapshot snap = await Qref.GetSnapshotAsync();
                    if (snap.Count > 0)
                    {
                        ViewBag.Message = "Patient " + patient.patient_name + " having Mobile number " + patient.patient_mobile_number + " already exists. " ;
                    }
                    #endregion Code to checkduplicacy of patient on the basis of name and mobile number
                    #region Code to generate new patient UID and update in paltientLastId collection
                    else
                    {
                        Query QrefPatientLastId = db.Collection("clinics").Document(GlobalSessionVariables.ClinicDocumentAutoId).Collection("patientLastId").Limit(1);
                        QuerySnapshot snapPatientLastId = await QrefPatientLastId.GetSnapshotAsync();
                        if (snapPatientLastId.Count > 0)
                        {
                            DocumentSnapshot docsnap2 = snapPatientLastId.Documents[0];
                            patientLastIdDocId = docsnap2.Id;

                            patientLastId = PatientLastId(docsnap2.GetValue<string>("id"));//Code to get plus one patientLastId

                            DocumentReference docRef = db.Collection("clinics").Document(GlobalSessionVariables.ClinicDocumentAutoId).Collection("patientLastId").Document(patientLastIdDocId);
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
                        CollectionReference col1 = db.Collection("clinics").Document(GlobalSessionVariables.ClinicDocumentAutoId).Collection("patientList");

                        Dictionary<string, object> data1 = new Dictionary<string, object>
                        {
                            {"patient_name" ,patient.patient_name},
                            {"age" ,patient.age},
                            {"care_of" ,patient.care_of},
                            {"city" ,patient.city},
                            {"creation_date" ,DateTime.UtcNow},
                            {"disease" ,patient.disease},
                            {"gender" ,patient.gender},
                            {"severity" ,patient.severity},
                            {"patient_id" ,patientLastId},
                            {"patient_mobile_number",patient.patient_mobile_number},
                            {"refer_by" ,patient.refer_by},
                            {"refer_to_doctor" ,patient.refer_to_doctor},
                            {"search_text" ,patient.patient_name+patient.patient_mobile_number+patientLastId}
                        };
                        await col1.Document().SetAsync(data1);
                        #endregion Code to create new Patient
                        #region Code to get newly created patient's auto id
                        Query QrefLatestPatient = db.Collection("clinics").Document(GlobalSessionVariables.ClinicDocumentAutoId).Collection("patientList").WhereEqualTo("patient_id", patientLastId).Limit(1);
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

                        #region Code to check duplicate appointment for selected date having status waiting

                        DateTime ConvertedAppDate = DateTime.SpecifyKind(Convert.ToDateTime(patient.appointment_date), DateTimeKind.Utc);

                        Query QrefduplicateApp = db.Collection("clinics").Document(GlobalSessionVariables.ClinicDocumentAutoId).Collection("appointments").WhereEqualTo("patient_id", patientLastId).WhereGreaterThanOrEqualTo("raisedDate", Timestamp.FromDateTime(ConvertedAppDate.Date)).WhereLessThan("raisedDate", Timestamp.FromDateTime(ConvertedAppDate.AddDays(1))).WhereEqualTo("status", "Waiting");
                        QuerySnapshot duplicateApp = await QrefduplicateApp.GetSnapshotAsync();
                        if (duplicateApp.Count > 0)
                        {
                            ViewBag.Message = "Appointment of " + patient.patient_name + "(" + patient.patient_id + ") for " + patient.appointment_date + " already exists. ";
                            return View(patient);
                        }
                        else
                        {
                        #endregion Code to check duplicate appointment for selected date having status waiting
                        #region Code to create new appointment id for today


                            CollectionReference colAppountments = db.Collection("clinics").Document(GlobalSessionVariables.ClinicDocumentAutoId).Collection("appointments");

                            Dictionary<string, object> dataAppointment = new Dictionary<string, object>
                            {
                                {"bill_sms" ,false},
                                {"clinic_id" ,GlobalSessionVariables.ClinicDocumentAutoId},
                                {"complitiondate" ,""},
                                {"date" ,""},
                                {"days" ,""},
                                {"fee" ,""},
                                {"patient" ,docSnapLatestPatient.Id},
                                {"patient_id" ,patientLastId},
                                {"raisedDate",DateTime.SpecifyKind(patient.appointment_date, DateTimeKind.Utc)},
                                {"reminder_sms" ,false},
                                {"severity" ,"Low"},
                                {"status" ,"Waiting"},
                                {"timeStamp" ,DateTime.UtcNow},
                                {"token" ,patient.tokenNumber}
                            };
                            await colAppountments.Document().SetAsync(dataAppointment);
                            #endregion Code to create new appointment id for today

                            return RedirectToAction("Index", "Appointment");
                        }

                            
                    }



                    return View(patient);

                    
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
        public async Task<ActionResult> Edit(string id)
        {
            string Path = AppDomain.CurrentDomain.BaseDirectory + @"greenpaperdev-firebase-adminsdk-8k2y5-fb46e63414.json";
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path);
            FirestoreDb db = FirestoreDb.Create("greenpaperdev");
            List<SelectListItem> diseases = new List<SelectListItem>() {
                        new SelectListItem {
                            Text = "General -(fever/general acute complaints)", Value = "GENERAL"
                        },
                        new SelectListItem {
                            Text = "ENT (ear nose throat)", Value = "ENT"
                        },
                        new SelectListItem {
                            Text = "CNS (central nervous system", Value = "CNS"
                        },
                        new SelectListItem {
                            Text = "DERMATO (skin/hair)", Value = "DERMATO"
                        },
                        new SelectListItem {
                            Text = "GIT (gastro intestinal track) SURGERY", Value = "GIT"
                        },
                        new SelectListItem {
                            Text = "TUMORS/ONCOLOGY", Value = "TUMOURS"
                        },
                        new SelectListItem {
                            Text = "HAEMATOLOGY (blood)", Value = "HAEMATOLOGY"
                        },
                        new SelectListItem {
                            Text = "ORTHOPADICS (bones & muscles)", Value = "ORTHOPADICS"
                        },
                        new SelectListItem {
                            Text = "PAEDIATRIC (child)", Value = "PAEDIATRIC"
                        },
                        new SelectListItem {
                            Text = "OPTHALMOLOGISTS (EYE)", Value = "EYE"
                        },
                        new SelectListItem {
                            Text = "CVS (cardio vascular system/heart)", Value = "CVS"
                        },
                        new SelectListItem {
                            Text = "ENDOCRINOLOGY", Value = "ENDO"
                        },
                        new SelectListItem {
                            Text = "GYNAECOLOGY & OBS", Value = "GYNAE"
                        },
                        new SelectListItem {
                            Text = "GENETICS", Value = "GENETICS"
                        },
                    };
            ViewBag.DISEASES = diseases;
            List<SelectListItem> cities = new List<SelectListItem>() {
                        new SelectListItem {
                            Text = "Faridabad", Value = "Faridabad"
                        },
                        new SelectListItem {
                            Text = "Ghaziabad", Value = "Ghaziabad"
                        },
                        new SelectListItem {
                            Text = "New Delhi", Value = "New Delhi"
                        },
                        new SelectListItem {
                            Text = "Bahadurgarh", Value = "Bahadurgarh"
                        },
                        new SelectListItem {
                            Text = "Mathura", Value = "Mathura"
                        },
                        new SelectListItem {
                            Text = "Agra", Value = "Agra"
                        },
                        new SelectListItem {
                            Text = "Ballabgarh", Value = "Ballabgarh"
                        },
                        new SelectListItem {
                            Text = "Gurdaspur", Value = "Gurdaspur"
                        },
                        new SelectListItem {
                            Text = "Amritsar", Value = "Amritsar"
                        },
                        new SelectListItem {
                            Text = "Batala", Value = "Batala"
                        },
                        new SelectListItem {
                            Text = "Jallandhar", Value = "Jallandhar"
                        },
                        new SelectListItem {
                            Text = "Pathankot", Value = "Pathankot"
                        },
                        new SelectListItem {
                            Text = "Bathinda", Value = "Bathinda"
                        },
                        new SelectListItem {
                            Text = "Ambala", Value = "Ambala"
                        },
                    };
            ViewBag.CITIES = cities;
            List<SelectListItem> genders = new List<SelectListItem>() {
                        new SelectListItem {
                            Text = "Male", Value = "Male"
                        },
                        new SelectListItem {
                            Text = "Female", Value = "Female"
                        },
                        new SelectListItem {
                            Text = "Other", Value = "Other"
                        },

                    };
            ViewBag.GENDERS = genders;

            List<SelectListItem> severity = new List<SelectListItem>() {
                new SelectListItem {
                    Text = "High", Value = "High"
                },
                new SelectListItem {
                    Text = "Medium", Value = "Medium"
                },
                new SelectListItem {
                    Text = "Low", Value = "Low"
                },

            };
            ViewBag.SEVERITIES = severity;

            //CollectionReference col1 = db.Collection("clinics").Document(GlobalSessionVariables.ClinicDocumentAutoId).Collection("patientList").Document("test");
            DocumentReference docRef = db.Collection("clinics").Document(GlobalSessionVariables.ClinicDocumentAutoId).Collection("patientList").Document(id);
            DocumentSnapshot docSnap = await docRef.GetSnapshotAsync();
            Patient patient = docSnap.ConvertTo<Patient>();
            patient.appointment_date = DateTime.UtcNow;
            patient.tokenNumber = "0";
            //patient.id = id;
            

            return View(patient);
        }

        // POST: Patient/Edit/5
        [HttpPost]
        public async Task<ActionResult> Edit(string id, Patient patient)
        {
            try
            {
                List<SelectListItem> diseases = new List<SelectListItem>() {
                        new SelectListItem {
                            Text = "General -(fever/general acute complaints)", Value = "GENERAL"
                        },
                        new SelectListItem {
                            Text = "ENT (ear nose throat)", Value = "ENT"
                        },
                        new SelectListItem {
                            Text = "CNS (central nervous system", Value = "CNS"
                        },
                        new SelectListItem {
                            Text = "DERMATO (skin/hair)", Value = "DERMATO"
                        },
                        new SelectListItem {
                            Text = "GIT (gastro intestinal track) SURGERY", Value = "GIT"
                        },
                        new SelectListItem {
                            Text = "TUMORS/ONCOLOGY", Value = "TUMOURS"
                        },
                        new SelectListItem {
                            Text = "HAEMATOLOGY (blood)", Value = "HAEMATOLOGY"
                        },
                        new SelectListItem {
                            Text = "ORTHOPADICS (bones & muscles)", Value = "ORTHOPADICS"
                        },
                        new SelectListItem {
                            Text = "PAEDIATRIC (child)", Value = "PAEDIATRIC"
                        },
                        new SelectListItem {
                            Text = "OPTHALMOLOGISTS (EYE)", Value = "EYE"
                        },
                        new SelectListItem {
                            Text = "CVS (cardio vascular system/heart)", Value = "CVS"
                        },
                        new SelectListItem {
                            Text = "ENDOCRINOLOGY", Value = "ENDO"
                        },
                        new SelectListItem {
                            Text = "GYNAECOLOGY & OBS", Value = "GYNAE"
                        },
                        new SelectListItem {
                            Text = "GENETICS", Value = "GENETICS"
                        },
                    };
                ViewBag.DISEASES = diseases;
                List<SelectListItem> cities = new List<SelectListItem>() {
                        new SelectListItem {
                            Text = "Faridabad", Value = "Faridabad"
                        },
                        new SelectListItem {
                            Text = "Ghaziabad", Value = "Ghaziabad"
                        },
                        new SelectListItem {
                            Text = "New Delhi", Value = "New Delhi"
                        },
                        new SelectListItem {
                            Text = "Bahadurgarh", Value = "Bahadurgarh"
                        },
                        new SelectListItem {
                            Text = "Mathura", Value = "Mathura"
                        },
                        new SelectListItem {
                            Text = "Agra", Value = "Agra"
                        },
                        new SelectListItem {
                            Text = "Ballabgarh", Value = "Ballabgarh"
                        },
                        new SelectListItem {
                            Text = "Gurdaspur", Value = "Gurdaspur"
                        },
                        new SelectListItem {
                            Text = "Amritsar", Value = "Amritsar"
                        },
                        new SelectListItem {
                            Text = "Batala", Value = "Batala"
                        },
                        new SelectListItem {
                            Text = "Jallandhar", Value = "Jallandhar"
                        },
                        new SelectListItem {
                            Text = "Pathankot", Value = "Pathankot"
                        },
                        new SelectListItem {
                            Text = "Bathinda", Value = "Bathinda"
                        },
                        new SelectListItem {
                            Text = "Ambala", Value = "Ambala"
                        },
                    };
                ViewBag.CITIES = cities;
                List<SelectListItem> genders = new List<SelectListItem>() {
                        new SelectListItem {
                            Text = "Male", Value = "Male"
                        },
                        new SelectListItem {
                            Text = "Female", Value = "Female"
                        },
                        new SelectListItem {
                            Text = "Other", Value = "Other"
                        },

                    };
                ViewBag.GENDERS = genders;

                List<SelectListItem> severity = new List<SelectListItem>() {
                new SelectListItem {
                    Text = "High", Value = "High"
                },
                new SelectListItem {
                    Text = "Medium", Value = "Medium"
                },
                new SelectListItem {
                    Text = "Low", Value = "Low"
                },

            };
                ViewBag.SEVERITIES = severity;

                if (ModelState.IsValid)
                {
                    string Path = AppDomain.CurrentDomain.BaseDirectory + @"greenpaperdev-firebase-adminsdk-8k2y5-fb46e63414.json";
                    Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path);
                    FirestoreDb db = FirestoreDb.Create("greenpaperdev");

                    DocumentReference docRef = db.Collection("clinics").Document(GlobalSessionVariables.ClinicDocumentAutoId).Collection("patientList").Document(id);
                    DocumentSnapshot docSnap = await docRef.GetSnapshotAsync();

                    Dictionary<string, object> data1 = new Dictionary<string, object>
                        {
                            {"patient_name" ,patient.patient_name},
                            {"age" ,patient.age},
                            {"care_of" ,patient.care_of},
                            {"city" ,patient.city},
                            {"creation_date" ,DateTime.SpecifyKind(patient.creation_date, DateTimeKind.Utc)},
                            {"disease" ,patient.disease},
                            {"gender" ,patient.gender},
                            {"severity" ,patient.severity},
                            {"patient_id" ,patient.patient_id},
                            {"patient_mobile_number",patient.patient_mobile_number},
                            {"refer_by" ,patient.refer_by},
                            {"refer_to_doctor" ,patient.refer_to_doctor},
                            {"search_text" ,patient.patient_name+patient.patient_mobile_number+patient.patient_id}
                        };
                    

                    if (docSnap.Exists)
                    {
                        await docRef.UpdateAsync(data1);
                    }

                    return RedirectToAction("Index");
                }
                else
                {
                    
                    return View(patient);
                }


            }
            catch
            {
                return View(patient);
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
                if(numericValue.ToString().Length == 1)
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

        // POST: Patient/Delete/5
        [HttpPost]
        public async Task<ActionResult> CreateFutureAppointmentold(FormCollection collection)
        {
            try
            {
                

                string lastTokenNumber = "";
                

                string patientAutoId = collection["patientAutoId"];
                string token = collection["tokennumber"];
                string appointmentDate = collection["datepicker"];
                

                string Path = AppDomain.CurrentDomain.BaseDirectory + @"greenpaperdev-firebase-adminsdk-8k2y5-fb46e63414.json";
                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path);
                FirestoreDb db = FirestoreDb.Create("greenpaperdev");
                DateTime futAppDate = Convert.ToDateTime(appointmentDate);

                #region Code to get latest token number, increament it, set in new appointment and update increamented token number back in collection
                DocumentReference docRefTokenNumber = db.Collection("clinics").Document(GlobalSessionVariables.ClinicDocumentAutoId).Collection("tokenNumber").Document(futAppDate.ToString("dd_MM_yyyy"));
                DocumentSnapshot docsnapTokenNumber = await docRefTokenNumber.GetSnapshotAsync();


                if (docsnapTokenNumber.Exists)
                {
                    lastTokenNumber = docsnapTokenNumber.GetValue<string>("last_token");
                }
                else
                {
                    lastTokenNumber = "0";
                    CollectionReference colTokenNumber = db.Collection("clinics").Document(GlobalSessionVariables.ClinicDocumentAutoId).Collection("tokenNumber");

                    Dictionary<string, object> dataTokenNumber = new Dictionary<string, object>
                            {
                                {"last_token" ,"0"},
                                {"assigned_tokens" ,null}
                            };
                    await colTokenNumber.Document(futAppDate.ToString("dd_MM_yyyy")).SetAsync(dataTokenNumber);
                }

                string lastTokenNumberReturned = "";

                if (lastTokenNumber != "0")
                {
                    string[] assignedtokens = docsnapTokenNumber.GetValue<string[]>("assigned_tokens");

                    
                    if (assignedtokens != null)
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



                if (Convert.ToInt32(token) < Convert.ToInt32(lastTokenNumberReturned))
                {
                    ViewBag.Message = "Token Number " + token + " is already assigned.";
                    return RedirectToAction("Index", "Patient");
                }
                if (Convert.ToInt32(token) < 1)
                {
                    ViewBag.Message = "Token Number can not be negative.";
                    return RedirectToAction("Index", "Patient");
                }
                if (Convert.ToInt32(token) >= Convert.ToInt32(lastTokenNumberReturned))
                {
                    if(Convert.ToInt32(token) == 1)
                    {

                        DocumentReference docRefTokenNumber2 = db.Collection("clinics").Document(GlobalSessionVariables.ClinicDocumentAutoId).Collection("tokenNumber").Document(futAppDate.ToString("dd_MM_yyyy"));
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
                                    return RedirectToAction("Index", "Patient");
                                }

                            }
                            assignedtokenList = assignedtokens.ToList();
                        }



                        CollectionReference colTokenNumber = db.Collection("clinics").Document(GlobalSessionVariables.ClinicDocumentAutoId).Collection("tokenNumber");
                        if(assignedtokens != null)
                        {
                            Dictionary<string, object> dataTokenNumber = new Dictionary<string, object>
                            {
                                {"last_token" ,token},
                                {"assigned_tokens" ,assignedtokenList.ToArray()}
                            };
                            await colTokenNumber.Document(futAppDate.ToString("dd_MM_yyyy")).SetAsync(dataTokenNumber);
                        }
                        else
                        {
                            Dictionary<string, object> dataTokenNumber = new Dictionary<string, object>
                            {
                                {"last_token" ,token},
                                {"assigned_tokens" ,null}
                            };
                            await colTokenNumber.Document(futAppDate.ToString("dd_MM_yyyy")).SetAsync(dataTokenNumber);
                        }
                        
                        

                    }
                    else if (Convert.ToInt32(token) == Convert.ToInt32(lastTokenNumberReturned))
                    {
                        
                        string[] assignedtokens = docsnapTokenNumber.GetValue<string[]>("assigned_tokens");
                        List<string> assignedtokenList = new List<string>();
                        if (assignedtokens != null)
                        {
                            for (int i = 0; i < assignedtokens.Length; i++)
                            {
                                if (token == assignedtokens[i].ToString())
                                {
                                    ViewBag.Message = "Token Number " + token + " is already assigned.";
                                    return RedirectToAction("Index", "Patient");
                                }
                                
                            }
                            assignedtokenList = assignedtokens.ToList();

                            CollectionReference colTokenNumber = db.Collection("clinics").Document(GlobalSessionVariables.ClinicDocumentAutoId).Collection("tokenNumber");

                            Dictionary<string, object> dataTokenNumber = new Dictionary<string, object>
                            {
                                {"last_token" ,token},
                                {"assigned_tokens" ,assignedtokenList.ToArray()}
                            };
                            await colTokenNumber.Document(futAppDate.ToString("dd_MM_yyyy")).SetAsync(dataTokenNumber);
                        }
                        else
                        {
                            CollectionReference colTokenNumber = db.Collection("clinics").Document(GlobalSessionVariables.ClinicDocumentAutoId).Collection("tokenNumber");

                            Dictionary<string, object> dataTokenNumber = new Dictionary<string, object>
                            {
                                {"last_token" ,token},
                                {"assigned_tokens" ,null}
                            };
                            await colTokenNumber.Document(futAppDate.ToString("dd_MM_yyyy")).SetAsync(dataTokenNumber);
                        }
                    }
                    else
                    {
                        DocumentReference docRefTokenNumber1 = db.Collection("clinics").Document(GlobalSessionVariables.ClinicDocumentAutoId).Collection("tokenNumber").Document(futAppDate.ToString("dd_MM_yyyy"));
                        DocumentSnapshot docsnapTokenNumber1 = await docRefTokenNumber1.GetSnapshotAsync();
                        string[] assignedtokens = docsnapTokenNumber1.GetValue<string[]>("assigned_tokens");

                        List<string> assignedtokenList = new List<string>();

                        if (assignedtokens != null)
                        {
                            for (int i = 0; i < assignedtokens.Length; i++)
                            {
                                if (token == assignedtokens[i].ToString())
                                {
                                    TempData["Message"] = "Token Number " + token + " is already assigned.";
                                    return RedirectToAction("Index", "Patient");
                                }

                            }
                            assignedtokenList = assignedtokens.ToList();
                        }
                        
                        

                        assignedtokenList.Add(token);

                        if (Convert.ToInt32(token) != Convert.ToInt32(lastTokenNumberReturned))
                        {
                            token = lastTokenNumber;
                        }

                        CollectionReference colTokenNumber = db.Collection("clinics").Document(GlobalSessionVariables.ClinicDocumentAutoId).Collection("tokenNumber");

                        Dictionary<string, object> dataTokenNumber = new Dictionary<string, object>
                        {
                            {"last_token" ,token},
                            {"assigned_tokens" ,assignedtokenList.ToArray()}
                        };
                        await colTokenNumber.Document(futAppDate.ToString("dd_MM_yyyy")).SetAsync(dataTokenNumber);
                    }
                }


                #endregion Code to get latest token number, increament it
                #region Code to create new appointment id for today

                DocumentReference docRefPatientUID = db.Collection("clinics").Document(GlobalSessionVariables.ClinicDocumentAutoId).Collection("patientList").Document(patientAutoId);
                DocumentSnapshot docsnapPatientUID = await docRefPatientUID.GetSnapshotAsync();

                string PatientUID = docsnapPatientUID.GetValue<string>("patient_id");
                string severity = docsnapPatientUID.GetValue<string>("severity");
                if (severity == null)
                {
                    severity = "Low";
                }
                


                CollectionReference colAppountments = db.Collection("clinics").Document(GlobalSessionVariables.ClinicDocumentAutoId).Collection("appointments");

                Dictionary<string, object> dataAppointment = new Dictionary<string, object>
                        {
                            {"bill_sms" ,false},
                            {"clinic_id" ,GlobalSessionVariables.ClinicDocumentAutoId},
                            {"complitiondate" ,""},
                            {"date" ,""},
                            {"days" ,""},
                            {"fee" ,""},
                            {"patient" ,patientAutoId},
                            {"patient_id" ,PatientUID},
                            {"raisedDate",DateTime.SpecifyKind(Convert.ToDateTime(appointmentDate), DateTimeKind.Utc)},
                            {"reminder_sms" ,false},
                            {"severity" ,severity},
                            {"status" ,"Waiting"},
                            {"timeStamp" ,DateTime.UtcNow},
                            {"token" ,token}
                        };
                await colAppountments.Document().SetAsync(dataAppointment);
                #endregion Code to create new appointment id for today



                return RedirectToAction("Index", "Patient");
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Patient");
            }
        }

        // POST: Patient/Delete/5
        [HttpPost]
        public async Task<ActionResult> CreateFutureAppointment(FormCollection collection)
        {
            try
            {

                string patientAutoId = collection["patientAutoId"];
                string token = collection["tokennumber"];
                string appointmentDate = collection["datepicker"];

                DateTime ConvertedAppDate = DateTime.SpecifyKind(Convert.ToDateTime(appointmentDate), DateTimeKind.Utc);

                string message = await UpdateTokenNumber(appointmentDate, token);
                if(message != null)
                {
                    TempData["Message"] = message;
                    return RedirectToAction("Index", "Patient");
                }
                

                string Path = AppDomain.CurrentDomain.BaseDirectory + @"greenpaperdev-firebase-adminsdk-8k2y5-fb46e63414.json";
                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path);
                FirestoreDb db = FirestoreDb.Create("greenpaperdev");

                DocumentReference docRefPatientUID = db.Collection("clinics").Document(GlobalSessionVariables.ClinicDocumentAutoId).Collection("patientList").Document(patientAutoId);
                DocumentSnapshot docsnapPatientUID = await docRefPatientUID.GetSnapshotAsync();

                string PatientUID = docsnapPatientUID.GetValue<string>("patient_id");
                string severity = docsnapPatientUID.GetValue<string>("severity");
                string PatientName = docsnapPatientUID.GetValue<string>("patient_name");
                if (severity == null)
                {
                    severity = "Low";
                }

                Query Qref = db.Collection("clinics").Document(GlobalSessionVariables.ClinicDocumentAutoId).Collection("appointments").WhereEqualTo("patient", patientAutoId).WhereGreaterThanOrEqualTo("raisedDate", Timestamp.FromDateTime(ConvertedAppDate.Date)).WhereLessThan("raisedDate", Timestamp.FromDateTime(ConvertedAppDate.AddDays(1))).WhereEqualTo("status","Waiting");
                QuerySnapshot snap = await Qref.GetSnapshotAsync();
                if (snap.Count > 0)
                {
                    TempData["Message"] = "Appointment of " + PatientName + "(" + PatientUID + ") for " + appointmentDate + " already exists. "; ;
                    return RedirectToAction("Index", "Patient");
                    
                }
                else
                {
                    #region Code to create new appointment id for today

                    CollectionReference colAppountments = db.Collection("clinics").Document(GlobalSessionVariables.ClinicDocumentAutoId).Collection("appointments");

                    Dictionary<string, object> dataAppointment = new Dictionary<string, object>
                        {
                            {"bill_sms" ,false},
                            {"clinic_id" ,GlobalSessionVariables.ClinicDocumentAutoId},
                            {"complitiondate" ,""},
                            {"date" ,""},
                            {"days" ,""},
                            {"fee" ,""},
                            {"patient" ,patientAutoId},
                            {"patient_id" ,PatientUID},
                            {"raisedDate",DateTime.SpecifyKind(Convert.ToDateTime(appointmentDate), DateTimeKind.Utc)},
                            {"reminder_sms" ,false},
                            {"severity" ,severity},
                            {"status" ,"Waiting"},
                            {"timeStamp" ,DateTime.UtcNow},
                            {"token" ,token}
                        };
                    await colAppountments.Document().SetAsync(dataAppointment);
                    #endregion Code to create new appointment id for today
                }





                return RedirectToAction("Index", "Patient");
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Patient");
            }
        }

        private async Task<string> UpdateTokenNumber(string appointmentDate, string token)
        {
            string lastTokenNumber = "";
            string flag = "Y";
            string Path = AppDomain.CurrentDomain.BaseDirectory + @"greenpaperdev-firebase-adminsdk-8k2y5-fb46e63414.json";
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path);
            FirestoreDb db = FirestoreDb.Create("greenpaperdev");
            DateTime futAppDate = Convert.ToDateTime(appointmentDate);

            #region Code to get latest token number, increament it, set in new appointment and update increamented token number back in collection
            DocumentReference docRefTokenNumber = db.Collection("clinics").Document(GlobalSessionVariables.ClinicDocumentAutoId).Collection("tokenNumber").Document(futAppDate.ToString("dd_MM_yyyy"));
            DocumentSnapshot docsnapTokenNumber = await docRefTokenNumber.GetSnapshotAsync();


            if (docsnapTokenNumber.Exists)
            {
                lastTokenNumber = docsnapTokenNumber.GetValue<string>("last_token");
            }
            else
            {
                lastTokenNumber = "0";
                CollectionReference colTokenNumber = db.Collection("clinics").Document(GlobalSessionVariables.ClinicDocumentAutoId).Collection("tokenNumber");

                Dictionary<string, object> dataTokenNumber = new Dictionary<string, object>
                            {
                                {"last_token" ,"0"},
                                {"assigned_tokens" ,null}
                            };
                await colTokenNumber.Document(futAppDate.ToString("dd_MM_yyyy")).SetAsync(dataTokenNumber);
            }

            string lastTokenNumberReturned = "";

            if (lastTokenNumber != "0")
            {
                string[] assignedtokens = docsnapTokenNumber.GetValue<string[]>("assigned_tokens");


                if (assignedtokens != null)
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



            if (Convert.ToInt32(token) < Convert.ToInt32(lastTokenNumberReturned))
            {
                ViewBag.Message = "Token Number " + token + " is already assigned.";

            }
            if (Convert.ToInt32(token) < 1)
            {
                ViewBag.Message = "Token Number can not be negative.";

            }
            if (Convert.ToInt32(token) >= Convert.ToInt32(lastTokenNumberReturned))
            {
                if (Convert.ToInt32(token) == 1)
                {

                    DocumentReference docRefTokenNumber2 = db.Collection("clinics").Document(GlobalSessionVariables.ClinicDocumentAutoId).Collection("tokenNumber").Document(futAppDate.ToString("dd_MM_yyyy"));
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


                    if(flag=="Y")
                    {
                        CollectionReference colTokenNumber = db.Collection("clinics").Document(GlobalSessionVariables.ClinicDocumentAutoId).Collection("tokenNumber");
                        if (assignedtokens != null)
                        {
                            Dictionary<string, object> dataTokenNumber = new Dictionary<string, object>
                            {
                                {"last_token" ,token},
                                {"assigned_tokens" ,assignedtokenList.ToArray()}
                            };
                            await colTokenNumber.Document(futAppDate.ToString("dd_MM_yyyy")).SetAsync(dataTokenNumber);
                        }
                        else
                        {
                            Dictionary<string, object> dataTokenNumber = new Dictionary<string, object>
                            {
                                {"last_token" ,token},
                                {"assigned_tokens" ,null}
                            };
                            await colTokenNumber.Document(futAppDate.ToString("dd_MM_yyyy")).SetAsync(dataTokenNumber);
                        }
                    }
                    



                }
                else if (Convert.ToInt32(token) == Convert.ToInt32(lastTokenNumberReturned))
                {

                    string[] assignedtokens = docsnapTokenNumber.GetValue<string[]>("assigned_tokens");
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

                        if(flag == "Y")
                        {
                            CollectionReference colTokenNumber = db.Collection("clinics").Document(GlobalSessionVariables.ClinicDocumentAutoId).Collection("tokenNumber");

                            Dictionary<string, object> dataTokenNumber = new Dictionary<string, object>
                            {
                                {"last_token" ,token},
                                {"assigned_tokens" ,assignedtokenList.ToArray()}
                            };
                            await colTokenNumber.Document(futAppDate.ToString("dd_MM_yyyy")).SetAsync(dataTokenNumber);
                        }
                        
                    }
                    else
                    {
                        CollectionReference colTokenNumber = db.Collection("clinics").Document(GlobalSessionVariables.ClinicDocumentAutoId).Collection("tokenNumber");

                        Dictionary<string, object> dataTokenNumber = new Dictionary<string, object>
                            {
                                {"last_token" ,token},
                                {"assigned_tokens" ,null}
                            };
                        await colTokenNumber.Document(futAppDate.ToString("dd_MM_yyyy")).SetAsync(dataTokenNumber);
                    }
                }
                else
                {
                    DocumentReference docRefTokenNumber1 = db.Collection("clinics").Document(GlobalSessionVariables.ClinicDocumentAutoId).Collection("tokenNumber").Document(futAppDate.ToString("dd_MM_yyyy"));
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

                    if(flag == "Y")
                    {
                        CollectionReference colTokenNumber = db.Collection("clinics").Document(GlobalSessionVariables.ClinicDocumentAutoId).Collection("tokenNumber");

                        Dictionary<string, object> dataTokenNumber = new Dictionary<string, object>
                        {
                            {"last_token" ,token},
                            {"assigned_tokens" ,assignedtokenList.ToArray()}
                        };
                        await colTokenNumber.Document(futAppDate.ToString("dd_MM_yyyy")).SetAsync(dataTokenNumber);
                    }

                    
                }
            }

            return ViewBag.Message;
            #endregion Code to get latest token number, increament it
        }

        [HttpPost]
        public async Task<string> getLatestToken(string futureAppointmentDate)
        {
            string lastTokenNumber = "";
            string Path = AppDomain.CurrentDomain.BaseDirectory + @"greenpaperdev-firebase-adminsdk-8k2y5-fb46e63414.json";
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path);
            FirestoreDb db = FirestoreDb.Create("greenpaperdev");

            DateTime futAppDate = Convert.ToDateTime(futureAppointmentDate);

            #region Code to get latest token number, increament it, set in new appointment and update increamented token number back in collection
            DocumentReference docRefTokenNumber = db.Collection("clinics").Document(GlobalSessionVariables.ClinicDocumentAutoId).Collection("tokenNumber").Document(futAppDate.ToString("dd_MM_yyyy"));
            DocumentSnapshot docsnapTokenNumber = await docRefTokenNumber.GetSnapshotAsync();

            if (docsnapTokenNumber.Exists)
            {
                lastTokenNumber = docsnapTokenNumber.GetValue<string>("last_token");
            }
            else
            {
                lastTokenNumber = "0";
            }

            string lastTokenNumberReturned = "";

            if (lastTokenNumber != "0")
            {
                string[] assignedtokens = docsnapTokenNumber.GetValue<string[]>("assigned_tokens");

                
                if (assignedtokens != null)
                {
                    int[] assignedtokensInt = Array.ConvertAll(assignedtokens, int.Parse);

                    Array.Sort(assignedtokensInt);

                    int j = 0;
                    for (int i = 0; i < assignedtokens.Length; i++)
                    {
                        if(Convert.ToInt32(lastTokenNumber) + 1 > assignedtokensInt[i])
                        {
                            lastTokenNumberReturned = (Convert.ToInt32(lastTokenNumber) + 1).ToString();
                            continue;
                        }
                        else if(Convert.ToInt32(lastTokenNumber) + 1 + j == assignedtokensInt[i])
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

            



            #endregion Code to get latest token number, increament it
            ViewData["tokenNumber"] = lastTokenNumberReturned;
            
            
            return lastTokenNumberReturned;
        }
    }
}
