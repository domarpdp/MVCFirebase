using Google.Cloud.Firestore;
using MVCFirebase.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace MVCFirebase.Controllers
{
    
    public class InventoryController : Controller
    {
        // GET: Inventory
        
        public async Task<ActionResult> Index()
        {
            //string ClinicMobileNumber = GlobalSessionVariables.ClinicMobileNumber;
            string ClinicMobileNumber = "9811035028";
            string Path = AppDomain.CurrentDomain.BaseDirectory + @"greenpaperdev-firebase-adminsdk-8k2y5-fb46e63414.json";
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path);
            FirestoreDb db = FirestoreDb.Create("greenpaperdev");
            List<Inventory> InventoryList = new List<Inventory>();


            //Query Qref = db.Collection("Students").WhereEqualTo("StudentName","Suvidhi");
            Query Qref = db.Collection("clinics").WhereEqualTo("clinicmobilenumber", ClinicMobileNumber);
            QuerySnapshot snapClinics = await Qref.GetSnapshotAsync();

            foreach (DocumentSnapshot docsnapClinics in snapClinics)
            {
                Clinic clinic = docsnapClinics.ConvertTo<Clinic>();
                QuerySnapshot snapMedicines = await docsnapClinics.Reference.Collection("inventory").OrderByDescending("medicinename").GetSnapshotAsync();

                foreach (DocumentSnapshot docsnapMedicines in snapMedicines)
                {


                    //Inventory inventory = docsnapMedicines.ConvertTo<Inventory>();
                    Inventory inventory = new Inventory();
                    inventory.id = docsnapMedicines.Id;
                    
                    inventory.shortname = docsnapMedicines.GetValue<string>("shortname");
                    inventory.quantitypurchased = docsnapMedicines.GetValue<int>("quantitypurchased");
                    inventory.quantitygiven = docsnapMedicines.GetValue<int>("quantitygiven");
                    inventory.quantitybalance = docsnapMedicines.GetValue<int>("quantitybalance");
                    inventory.medicinename = docsnapMedicines.GetValue<string>("medicinename");
                    inventory.unitmrp = docsnapMedicines.GetValue<string>("unitmrp");
                    inventory.purchasedunitprice = docsnapMedicines.GetValue<string>("purchasedunitprice");
                    inventory.expirydate = docsnapMedicines.GetValue<Timestamp>("expirydate").ToDateTime().ToString("MM/dd/yyyy");
                    inventory.dateadded = docsnapMedicines.GetValue<Timestamp>("dateadded").ToDateTime().ToString("MM/dd/yyyy");
                    inventory.vendorname = docsnapMedicines.GetValue<string>("vendorname");
                    inventory.vendormobilenumber = docsnapMedicines.GetValue<string>("vendormobilenumber");
                    //QuerySnapshot snapPatient = await docsnapClinics.Reference.Collection("patientList").WhereEqualTo("patient_id", appointment.patient_id).Limit(1).GetSnapshotAsync();
                    //DocumentSnapshot docsnapPatient = snapPatient.Documents[0];

                    //Patient patient = docsnapPatient.ConvertTo<Patient>();
                    if (docsnapMedicines.Exists)
                    {
                        InventoryList.Add(inventory);
                    }
                }

            }

            return View(InventoryList);
        }

        // GET: Inventory/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Inventory/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Inventory/Create
        [HttpPost]
        public async Task<ActionResult> Create(Inventory inventory)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    
                    string Path = AppDomain.CurrentDomain.BaseDirectory + @"greenpaperdev-firebase-adminsdk-8k2y5-fb46e63414.json";
                    Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path);
                    FirestoreDb db = FirestoreDb.Create("greenpaperdev");


                    //CollectionReference col1 = db.Collection("clinics").Document(GlobalSessionVariables.ClinicDocumentAutoId).Collection("patientList").Document("test");
                    CollectionReference col1 = db.Collection("clinics").Document("ly0N6C9cO0crz0s6LMUi").Collection("inventory");


                    Dictionary<string, object> data1 = new Dictionary<string, object>
                    {
                        {"shortname" ,inventory.shortname},
                        {"quantitypurchased" ,inventory.quantitypurchased},
                        {"medicinename" ,inventory.medicinename},
                        {"unitmrp" ,inventory.unitmrp.ToString()},
                        {"dateadded" ,DateTime.UtcNow},
                        {"expirydate" ,DateTime.SpecifyKind(Convert.ToDateTime(inventory.expirydate), DateTimeKind.Utc)},
                        {"purchasedunitprice" ,inventory.purchasedunitprice.ToString()},
                        {"vendorname",inventory.vendorname},
                        {"vendormobilenumber" ,inventory.vendormobilenumber},
                        {"quantitygiven" ,0},
                        {"quantitybalance" ,inventory.quantitypurchased}
                    };

                    await col1.Document().SetAsync(data1);



                    return RedirectToAction("Index");
                }
                else
                {
                    return View(inventory);
                }
                // TODO: Add insert logic here

                
            }
            catch (Exception ex)
            {
                return View(inventory);
            }
        }

        // GET: Inventory/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Inventory/Edit/5
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

        // GET: Inventory/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Inventory/Delete/5
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

        public ActionResult AddMedicine()
        {
            List<Medicine> medicine = new List<Medicine>();

            TempData["medicine"] = medicine;
            TempData.Keep();
            return View();

            //DataTable dt = new DataTable();
            //dt.Columns.AddRange(new DataColumn[3] { new DataColumn("Id"), new DataColumn("Name"), new DataColumn("Country") });
            //dt.Rows.Add(1, "John Hammond", "United States");
            //dt.Rows.Add(2, "Mudassar Khan", "India");
            //dt.Rows.Add(3, "Suzanne Mathews", "France");
            //dt.Rows.Add(4, "Robert Schidner", "Russia");
            //TempData["abc"] = dt.AsEnumerable();
            
            //ViewData.Model = dt.AsEnumerable();
            //TempData.Keep();
            //return View();
        }

        // POST: Inventory/Delete/5
        [HttpPost]
        public ActionResult AddMedicine(FormCollection collection)
        {
            try
            {
                int serialnoCount = 0;
                List<Medicine> medicine = new List<Medicine>();
                medicine = TempData["medicine"] as List<Medicine>;
                medicine = medicine.OrderByDescending(a => a.serialno).ToList();

                if (medicine.Count > 0)
                {
                    serialnoCount = medicine.FirstOrDefault().serialno;
                }
                Medicine med = new Medicine();
                med.serialno = serialnoCount + 1;
                med.medicinename = collection["Medicine"];
                med.Quantity = collection["Quantity"];

                medicine.Add(med);

                TempData["medicine"] = medicine;
                //string blah = myDataSet.Tables[0].Rows[0]["Name"].ToString();

                // TODO: Add delete logic here
                TempData.Keep();
                return View();
            }
            catch
            {
                return View();
            }
        }

        // POST: Inventory/Delete/5
        [HttpPost]
        public ActionResult DeleteMedicine(FormCollection collection)
        {
            try
            {
                
                List<Medicine> medicine = new List<Medicine>();
                medicine = TempData["medicine"] as List<Medicine>;
                string www = collection["serialno"].ToString().Split(',').Last();
                int serialnoremove = Convert.ToInt32(collection["serialno"].ToString().Split(',').Last());

                var itemToRemove = medicine.Single(r => r.serialno == serialnoremove);
                medicine.Remove(itemToRemove);

                TempData["medicine"] = medicine;
                //string blah = myDataSet.Tables[0].Rows[0]["Name"].ToString();

                // TODO: Add delete logic here
                TempData.Keep();
                return RedirectToAction("AddMedicine");
            }
            catch (Exception ex)
            {
                return View();
            }
        }
    }
}
