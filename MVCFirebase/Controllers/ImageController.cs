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
    public class ImageController : Controller
    {
        // GET: Image
        public async Task<ActionResult> Index(string patient)
        {
            ImageModel _objuserloginmodel = new ImageModel();
            ViewBag.SelectedId = 0;
            TempData["SelectedId"] = 0;
            TempData["patientAutoId"] = patient;

            List<ImageViewModel> ImageList = new List<ImageViewModel>();
            string ClinicMobileNumber = GlobalSessionVariables.ClinicMobileNumber;
            string Path = AppDomain.CurrentDomain.BaseDirectory + @"greenpaperdev-firebase-adminsdk-8k2y5-fb46e63414.json";
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path);
            FirestoreDb db = FirestoreDb.Create("greenpaperdev");
            int i = 1;
            

            Query QrefPrescriptions = db.Collection("clinics").Document(GlobalSessionVariables.ClinicDocumentAutoId).Collection("patientList").Document(patient).Collection("prescriptions").OrderByDescending("timeStamp");
            QuerySnapshot snapPres = await QrefPrescriptions.GetSnapshotAsync();
            if(snapPres.Count > 0)
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
            }

            //_objuserloginmodel.SelectedImage = _objuserloginmodel.GetList()[0];
            _objuserloginmodel.SelectedImage = ImageList[1];
            return View(_objuserloginmodel);

        }

        [HttpPost]
        public async Task<ActionResult> GetNextOrPrevImage(ImageViewModel SelectedImage, string ButtonType)
        {
            ImageModel _objuserloginmodel = new ImageModel();
            string patientAutoId = TempData["patientAutoId"].ToString();
            //List<ImageViewModel> GetList = _objuserloginmodel.GetList();

            List<ImageViewModel> GetList = new List<ImageViewModel>();

            string ClinicMobileNumber = GlobalSessionVariables.ClinicMobileNumber;
            string Path = AppDomain.CurrentDomain.BaseDirectory + @"greenpaperdev-firebase-adminsdk-8k2y5-fb46e63414.json";
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path);
            FirestoreDb db = FirestoreDb.Create("greenpaperdev");
            int i = 1;
            

            Query QrefPrescriptions = db.Collection("clinics").Document(GlobalSessionVariables.ClinicDocumentAutoId).Collection("patientList").Document(patientAutoId).Collection("prescriptions").OrderByDescending("timeStamp");
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
                        GetList.Add(img);
                        i++;
                    }
                   
                    

                }
            }

            int id = System.Convert.ToInt32(TempData["SelectedId"]);
            

            if (ButtonType.Trim() == ">")
                _objuserloginmodel.SelectedImage = GetList[++id < GetList.Count ? id : --id];
            else if (ButtonType.Trim() == "<")
                _objuserloginmodel.SelectedImage = GetList[--id > -1 ? id : ++id];

            TempData["SelectedId"] = id;
            TempData["patientAutoId"] = patientAutoId;


            return PartialView("_PartialImage", _objuserloginmodel);
        }
    }
}