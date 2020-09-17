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
                QuerySnapshot snap2 = await docsnap.Reference.Collection("patientList").OrderByDescending("patient_id").Limit(100).GetSnapshotAsync();

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
            string Path = AppDomain.CurrentDomain.BaseDirectory + @"greenpaperdev-firebase-adminsdk-8k2y5-fb46e63414.json";
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path);
            FirestoreDb db = FirestoreDb.Create("greenpaperdev");
            DocumentReference docRefClinicCity = db.Collection("clinics").Document(GlobalSessionVariables.ClinicDocumentAutoId);
            DocumentSnapshot docSnapClinicCity = await docRefClinicCity.GetSnapshotAsync();
            Clinic clinic = docSnapClinicCity.ConvertTo<Clinic>();
            Patient patient = new Patient();
            patient.city = clinic.cliniccity;
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

                if (ModelState.IsValid)
                {
                    string Path = AppDomain.CurrentDomain.BaseDirectory + @"greenpaperdev-firebase-adminsdk-8k2y5-fb46e63414.json";
                    Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path);
                    FirestoreDb db = FirestoreDb.Create("greenpaperdev");
                    DocumentReference docRefClinicCity = db.Collection("clinics").Document(GlobalSessionVariables.ClinicDocumentAutoId);
                    DocumentSnapshot docSnapClinicCity = await docRefClinicCity.GetSnapshotAsync();


                    Query Qref = db.Collection("clinics").Document(GlobalSessionVariables.ClinicDocumentAutoId).Collection("patientList").WhereEqualTo("patient_name", patient.patient_name).WhereEqualTo("patient_mobile_number", patient.patient_mobile_number);
                    QuerySnapshot snap = await Qref.GetSnapshotAsync();
                    if (snap.Count > 0)
                    {
                        ViewBag.Message = "Patient " + patient.patient_name + " having Mobile number " + patient.patient_mobile_number + " already exists. " ;
                    }
                    else
                    {
                        Query QrefPatientLastId = db.Collection("clinics").Document(GlobalSessionVariables.ClinicDocumentAutoId).Collection("patientLastId").Limit(1);
                        QuerySnapshot snapPatientLastId = await QrefPatientLastId.GetSnapshotAsync();
                        if (snapPatientLastId.Count > 0)
                        {
                            DocumentSnapshot docsnap2 = snapPatientLastId.Documents[0];
                            patientLastIdDocId = docsnap2.Id;

                            patientLastId = PatientLastId(docsnap2.GetValue<string>("id"));

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
                            {"patient_id" ,patientLastId},
                            {"patient_mobile_number",patient.patient_mobile_number},
                            {"refer_by" ,patient.refer_by},
                            {"refer_to_doctor" ,patient.refer_to_doctor},
                            {"search_text" ,patient.patient_name+patient.patient_mobile_number+patientLastId}
                        };
                        await col1.Document().SetAsync(data1);


                        return RedirectToAction("Index");
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

            //CollectionReference col1 = db.Collection("clinics").Document(GlobalSessionVariables.ClinicDocumentAutoId).Collection("patientList").Document("test");
            DocumentReference docRef = db.Collection("clinics").Document(GlobalSessionVariables.ClinicDocumentAutoId).Collection("patientList").Document(id);
            DocumentSnapshot docSnap = await docRef.GetSnapshotAsync();
            Patient patient = docSnap.ConvertTo<Patient>();
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
    }
}
