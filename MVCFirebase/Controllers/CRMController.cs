using Google.Cloud.Firestore;
using MVCFirebase.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using PagedList;

namespace MVCFirebase.Controllers
{
    [AccessDeniedAuthorize(Roles = "Salesman")]
    public class CRMController : Controller
    {
        // GET: CRM
        public async Task<ActionResult> Index(string sortOrder, string currentFilter,string searchString, int? page)
        {
            ViewBag.CurrentSort = sortOrder;

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }
            ViewBag.CurrentFilter = searchString;

            string Path = AppDomain.CurrentDomain.BaseDirectory + @"greenpaperdev-firebase-adminsdk-8k2y5-fb46e63414.json";
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path);
            FirestoreDb db = FirestoreDb.Create("greenpaperdev");

            DateTime SearchDate;
    
            SearchDate = Convert.ToDateTime(DateTime.UtcNow);
            SearchDate = SearchDate.AddDays(1);
            
            Timestamp SearchDateTo = Timestamp.FromDateTime(SearchDate.Date);

            List<CRM> CRMList = new List<CRM>();

            Query Qref;

            Qref = db.Collection("CRM").WhereLessThanOrEqualTo("nextmeetingdate", SearchDateTo);

            QuerySnapshot snap = await Qref.GetSnapshotAsync();

            foreach (DocumentSnapshot docsnap in snap)
            {
                CRM crm = docsnap.ConvertTo<CRM>();
                if (docsnap.Exists)
                {
                    crm.Id = docsnap.Id;
                    CRMList.Add(crm);
                }
            }

            ViewBag.DateSortParm = sortOrder == "Date" ? "date_desc" : "Date";
            ViewBag.ClinicTypeSortParm = sortOrder == "ct_ascd" ? "ct_desc" : "ct_ascd";
            ViewBag.statusSortParm = sortOrder == "status_ascd" ? "status_desc" : "status_ascd";

            switch (sortOrder)
            {
                case "status_ascd":
                    CRMList = CRMList.OrderBy(a => a.status).ToList();

                    break;
                case "status_desc":
                    CRMList = CRMList.OrderByDescending(a => a.status).ToList();

                    break;
                case "ct_ascd":
                    CRMList = CRMList.OrderBy(a => a.clinictype).ToList();

                    break;
                case "ct_desc":
                    CRMList = CRMList.OrderByDescending(a => a.clinictype).ToList();

                    break;

                case "Date":
                    CRMList = CRMList.OrderBy(a => a.nextmeetingdate).ToList();
                    
                    
                    break;
                case "date_desc":
                    CRMList = CRMList.OrderByDescending(a => a.nextmeetingdate).ToList();
                    
                    
                    break;
                default:
                    CRMList = CRMList.OrderByDescending(a => a.nextmeetingdate).ToList();

                    break;
            }


            if (!String.IsNullOrEmpty(searchString))
            {
                CRMList = CRMList.Where(x => x.referedby.Contains(searchString) || x.clinicname.Contains(searchString) || x.doctorname.Contains(searchString)|| x.contact.Contains(searchString)).ToList();
                //Qref = db.("CRM").OrderBy("referedby").StartAt(searchString).EndAt(searchString + "\uf8ff");
            }

            int pageSize = 25;
            int pageNumber = (page ?? 1);
            return View(CRMList.ToPagedList(pageNumber, pageSize));

        }

        // GET: CRM/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: CRM/Create
        public ActionResult Create()
        {
            List<SelectListItem> clinictype = new List<SelectListItem>() {
                        new SelectListItem {
                            Text = "Alopathy", Value = "Alopathy"
                        },
                        new SelectListItem {
                            Text = "Homeopathy", Value = "Homeopathy"
                        },
                        new SelectListItem {
                            Text = "Ayurvedic", Value = "Ayurvedic"
                        },
                        new SelectListItem {
                            Text = "Dental", Value = "Dental"
                        },
                        new SelectListItem {
                            Text = "Other", Value = "Other"
                        },

                    };
            ViewBag.CLINICTYPE = clinictype;

            List<SelectListItem> status = new List<SelectListItem>() {
                        new SelectListItem {
                            Text = "Active", Value = "Active"
                        },
                        new SelectListItem {
                            Text = "Closed", Value = "Closed"
                        },
                        new SelectListItem {
                            Text = "Not Started", Value = "Not Started"
                        },

                    };
            ViewBag.STATUS = status;
            return View();
        }

        // POST: CRM/Create
        [HttpPost]
        public ActionResult Create(CRM crm)
        {

            List<SelectListItem> clinictype = new List<SelectListItem>() {
                        new SelectListItem {
                            Text = "Alopathy", Value = "Alopathy"
                        },
                        new SelectListItem {
                            Text = "Homeopathy", Value = "Homeopathy"
                        },
                        new SelectListItem {
                            Text = "Ayurvedic", Value = "Ayurvedic"
                        },
                        new SelectListItem {
                            Text = "Dental", Value = "Dental"
                        },
                        new SelectListItem {
                            Text = "Other", Value = "Other"
                        },

                    };
            ViewBag.CLINICTYPE = clinictype;

            List<SelectListItem> status = new List<SelectListItem>() {
                        new SelectListItem {
                            Text = "Active", Value = "Active"
                        },
                        new SelectListItem {
                            Text = "Closed", Value = "Closed"
                        },
                        new SelectListItem {
                            Text = "Not Started", Value = "Not Started"
                        },

                    };
            ViewBag.STATUS = status;

            string Path = AppDomain.CurrentDomain.BaseDirectory + @"greenpaperdev-firebase-adminsdk-8k2y5-fb46e63414.json";
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path);
            FirestoreDb db = FirestoreDb.Create("greenpaperdev");


            try
            {

                CollectionReference col1 = db.Collection("CRM");
                Dictionary<string, object> data1 = new Dictionary<string, object>
                {
                    {"referedby" ,crm.referedby },
                    {"referedon" ,DateTime.UtcNow},
                    {"clinictype",crm.clinictype},
                    {"address1",crm.address1},
                    {"address2",crm.address2},
                    {"address3",crm.address3},
                    {"city",crm.city},
                    {"state",crm.state},
                    {"pin",crm.pin},
                    {"country","India"},
                    {"clinicname",crm.clinicname},
                    {"doctorname",crm.doctorname},
                    {"contact",crm.contact},
                    {"nextmeetingdate",DateTime.SpecifyKind(crm.nextmeetingdate, DateTimeKind.Utc)},
                    {"comments",crm.comments},
                    {"status",crm.status}

                };

                col1.AddAsync(data1);


                // TODO: Add insert logic here
                //var result = await fireBaseClient.Child("Students").PostAsync(std);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return View();
            }
        }

        // GET: CRM/Edit/5
        public async Task<ActionResult> Edit(string id)
        {
            List<SelectListItem> clinictype = new List<SelectListItem>() {
                        new SelectListItem {
                            Text = "Alopathy", Value = "Alopathy"
                        },
                        new SelectListItem {
                            Text = "Homeopathy", Value = "Homeopathy"
                        },
                        new SelectListItem {
                            Text = "Ayurvedic", Value = "Ayurvedic"
                        },
                        new SelectListItem {
                            Text = "Dental", Value = "Dental"
                        },
                        new SelectListItem {
                            Text = "Other", Value = "Other"
                        },

                    };
            ViewBag.CLINICTYPES = clinictype;

            List<SelectListItem> status = new List<SelectListItem>() {
                        new SelectListItem {
                            Text = "Active", Value = "Active"
                        },
                        new SelectListItem {
                            Text = "Closed", Value = "Closed"
                        },
                        new SelectListItem {
                            Text = "Not Started", Value = "Not Started"
                        },

                    };
            ViewBag.STATUSES = status;

            string Path = AppDomain.CurrentDomain.BaseDirectory + @"greenpaperdev-firebase-adminsdk-8k2y5-fb46e63414.json";
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path);
            FirestoreDb db = FirestoreDb.Create("greenpaperdev");


            DocumentReference docRef = db.Collection("CRM").Document(id);
            DocumentSnapshot docsnapSettings = await docRef.GetSnapshotAsync();

            CRM crm = docsnapSettings.ConvertTo<CRM>();

            return View(crm);
            
        }

        // POST: CRM/Edit/5
        [HttpPost]
        public async Task<ActionResult> Edit(string id, CRM crm)
        {
            List<SelectListItem> clinictype = new List<SelectListItem>() {
                        new SelectListItem {
                            Text = "Alopathy", Value = "Alopathy"
                        },
                        new SelectListItem {
                            Text = "Homeopathy", Value = "Homeopathy"
                        },
                        new SelectListItem {
                            Text = "Ayurvedic", Value = "Ayurvedic"
                        },
                        new SelectListItem {
                            Text = "Dental", Value = "Dental"
                        },
                        new SelectListItem {
                            Text = "Other", Value = "Other"
                        },

                    };
            ViewBag.CLINICTYPES = clinictype;

            List<SelectListItem> status = new List<SelectListItem>() {
                        new SelectListItem {
                            Text = "Active", Value = "Active"
                        },
                        new SelectListItem {
                            Text = "Closed", Value = "Closed"
                        },
                        new SelectListItem {
                            Text = "Not Started", Value = "Not Started"
                        },

                    };
            ViewBag.STATUSES = status;

            try
            {
                if (ModelState.IsValid)
                {
                    string Path = AppDomain.CurrentDomain.BaseDirectory + @"greenpaperdev-firebase-adminsdk-8k2y5-fb46e63414.json";
                    Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path);
                    FirestoreDb db = FirestoreDb.Create("greenpaperdev");

                    DocumentReference docRef = db.Collection("CRM").Document(id);
                    DocumentSnapshot docSnap = await docRef.GetSnapshotAsync();



                    Dictionary<string, object> data1 = new Dictionary<string, object>
                            {
                                {"referedby" ,crm.referedby },
                                {"referedon" ,DateTime.UtcNow},
                                {"clinictype",crm.clinictype},
                                {"address1",crm.address1},
                                {"address2",crm.address2},
                                {"address3",crm.address3},
                                {"city",crm.city},
                                {"state",crm.state},
                                {"pin",crm.pin},
                                {"country","India"},
                                {"clinicname",crm.clinicname},
                                {"doctorname",crm.doctorname},
                                {"contact",crm.contact},
                                {"nextmeetingdate",DateTime.SpecifyKind(crm.nextmeetingdate, DateTimeKind.Utc)},
                                {"comments",crm.comments},
                                {"status",crm.status}
                            };


                    if (docSnap.Exists)
                    {
                        await docRef.UpdateAsync(data1);
                    }

                    return RedirectToAction("Index");
                }
                else
                {
                    return View(crm);
                }
                // TODO: Add update logic here

                
            }
            catch
            {
                return View(crm);
            }
        }

        // GET: CRM/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: CRM/Delete/5
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

        [HttpPost]
        public JsonResult cityautocomplete(string city)//I think that the id that you are passing here needs to be the search term. You may not have to change anything here, but you do in the $.ajax() call
        {
            List<city> cityList = new List<city>();
            string json = System.IO.File.ReadAllText(Server.MapPath(@"~/Content/JsonFiles/cities.json"));
            cityList = JsonConvert.DeserializeObject<List<city>>(json);
            cityList = cityList.Where(a => a.name.StartsWith(city.ToLower())).ToList();
            return Json(cityList);
        }
    }
}
