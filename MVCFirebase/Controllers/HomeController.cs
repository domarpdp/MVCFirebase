using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Firebase.Database;

using Firebase.Database.Query;
using System.Data.Linq;
using System.Threading.Tasks;
using MVCFirebase.Models;
using System.Web.Security;
using Google.Cloud.Firestore;
using WebGrease.Css.Ast.Selectors;
using System.Net;
using System.Data;
using System.Security.Claims;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.Owin.Security;
using Microsoft.AspNetCore.Mvc;

namespace MVCFirebase.Controllers
{
    public class HomeController : Controller
    {
        [AllowAnonymous]
        public ActionResult Index()
        {
            return View();
        }
        [AllowAnonymous]
        public ActionResult Privacy()
        {
            return View();
        }

        public JsonResult KeepAlive()
        {
            return Json(new { success = true, message = "User is not authenticated" });
        }

        [AllowAnonymous]
        public ActionResult term()
        {
            return View();
        }
        [AllowAnonymous]
        public ActionResult patientterm()
        {
            return View();
        }
        [AllowAnonymous]
        public ActionResult patientprivacy()
        {
            return View();
        }
        [AllowAnonymous]
        public ActionResult Report()
        {
            return View();
        }
        [AllowAnonymous]
        public ActionResult Login()
        {
            var model = new User
            {
                cliniccode = "GP-107",
                mobile_number = "9811035028"
            };
            var authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];

            if (authCookie != null)
            {
                return RedirectToAction("Index", "Appointment");
            }
            else
            {
                return View(model);
            }
            
            

        }

        //[HttpPost]
        //[AllowAnonymous]
        //public async Task<ActionResult> Login1(User user)
        //{
        //    //string Path = AppDomain.CurrentDomain.BaseDirectory + @"myfastingapp-bd6ec.json";
        //    string Path = AppDomain.CurrentDomain.BaseDirectory + @"greenpaperdev-firebase-adminsdk-8k2y5-fb46e63414.json";
        //    Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path);
        //    FirestoreDb db = FirestoreDb.Create("greenpaperdev");
        //    string message = string.Empty;
        //    string clinicPlan = "";

        //    if (user.cliniccode == "" || user.cliniccode == null)
        //    {

        //        try
        //        {
        //            //Query Qref = db.Collection("Students").WhereEqualTo("StudentName","Suvidhi");
        //            Query Qref = db.Collection("SuperUsers").WhereEqualTo("UserId", user.mobile_number).WhereEqualTo("Password", user.password).Limit(1);
        //            QuerySnapshot snap = await Qref.GetSnapshotAsync();
        //            if (snap.Count > 0)
        //            {
        //                foreach (DocumentSnapshot docsnap in snap)
        //                {
        //                    SuperUser superuser = docsnap.ConvertTo<SuperUser>();
        //                    if (docsnap.Exists)
        //                    {
        //                        //GlobalSessionVariables.UserRoles = "SuperAdmin";
        //                        FormsAuthentication.SetAuthCookie(superuser.UserName + "-" + superuser.UserName, superuser.RememberMe);

        //                        return RedirectToAction("Index");
        //                    }
        //                    else
        //                    {
        //                        message = "Username and/or password is incorrect.";
        //                        ModelState.AddModelError("", message);
        //                        ViewBag.Message = message;
        //                        return View(user);
        //                    }



        //                }
        //            }
        //            else
        //            {
        //                message = "Username and/or password is incorrect.";
        //                ModelState.AddModelError("", message);
        //                ViewBag.Message = message;
        //                return View(user);
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            ModelState.AddModelError("", $"An error occurred: {ex.Message}");
        //            ViewBag.Message = ex.Message;
        //            return View(user);
        //        }

        //    }
        //    else
        //    {

        //        try
        //        {
        //            Query Qref = db.Collection("clinics").WhereEqualTo("clinic_code", user.cliniccode).Limit(1);
        //            QuerySnapshot snapClinic = await Qref.GetSnapshotAsync();

        //            if (snapClinic.Count > 0)
        //            {
        //                DocumentSnapshot docsnapClinic = snapClinic.Documents[0];

        //                Clinic clinic = docsnapClinic.ConvertTo<Clinic>();
        //                clinicPlan = clinic.selected_plan;
                      

        //                //GlobalSessionVariables.ClinicMobileNumber = clinic.clinicmobilenumber;
        //                //GlobalSessionVariables.ClinicDocumentAutoId = docsnapClinic.Id;
        //                //GlobalSessionVariables.ClinicCode = clinic.clinic_code;

        //                QuerySnapshot snapUser = await docsnapClinic.Reference.Collection("user").WhereEqualTo("mobile_number", user.mobile_number).GetSnapshotAsync();

        //                if (snapUser.Count > 0)
        //                {
        //                    foreach (DocumentSnapshot docsnapUsers in snapUser)
        //                    {

        //                        User userLoggedIn = docsnapUsers.ConvertTo<User>();
        //                        QuerySnapshot snapUserPassword = await docsnapClinic.Reference.Collection("user").WhereEqualTo("mobile_number", user.mobile_number).WhereEqualTo("password", user.password).GetSnapshotAsync();

        //                        if (snapUserPassword.Count > 0)
        //                        {

                                    

        //                            Session["sessionid"] = System.Web.HttpContext.Current.Session.SessionID;
        //                            //Session["ClinicMobileNumber"] = clinic.clinicmobilenumber;
        //                            //Session["ClinicDocumentAutoId"] = docsnapClinic.Id;
        //                            //Session["ClinicDocumentAutoId"] = clinic.clinic_code;

        //                            DocumentSnapshot docsnapUser = snapUserPassword.Documents[0];

        //                            User userForRoles = docsnapUser.ConvertTo<User>();
        //                            //GlobalSessionVariables.UserRoles = string.Join(",", userForRoles.user_roles);

        //                            //var claims = new List<Claim>
        //                            //{
        //                            //    new Claim(ClaimTypes.Name, userLoggedIn.name),
        //                            //    new Claim("ClinicMobileNumber", clinic.clinicmobilenumber),
        //                            //    new Claim("ClinicDocumentAutoId", docsnapClinic.Id),
        //                            //    new Claim("ClinicCode", user.cliniccode)
        //                            //};

        //                            //// Add roles to claims
        //                            //foreach (var role in userForRoles.user_roles)
        //                            //{
        //                            //    claims.Add(new Claim(ClaimTypes.Role, role));
        //                            //}


        //                            // Create identity and sign in
        //                            //var identity = new ClaimsIdentity(claims, DefaultAuthenticationTypes.ApplicationCookie);
        //                            //var authManager = HttpContext.GetOwinContext().Authentication;
        //                            //authManager.SignIn(new Microsoft.Owin.Security.AuthenticationProperties { IsPersistent = true }, identity);

        //                            FormsAuthentication.SetAuthCookie(userLoggedIn.mobile_number + "|" + userLoggedIn.name + "|" + string.Join(",", userForRoles.user_roles) + "|" + clinic.clinicmobilenumber + "|" + docsnapClinic.Id + "|" + user.cliniccode, user.RememberMe);
        //                            //if(User.IsInRole("Receptionist"))

        //                            if (userForRoles.user_roles.Contains("Receptionist"))
        //                            {
        //                                return RedirectToAction("Index", "Patient");
        //                            }
        //                            else if (userForRoles.user_roles.Contains("Admin"))
        //                            {
        //                                return RedirectToAction("Index", "User");
        //                            }
        //                            else
        //                            {
        //                                return RedirectToAction("Waiting", "Appointment");
        //                            }

        //                        }
        //                        else
        //                        {
        //                            message = "Password for user " + user.mobile_number + " is incorrect.";
        //                            ModelState.AddModelError("", message);
        //                            ViewBag.Message = message;
        //                            return View(user);
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    message = "User Id " + user.mobile_number + " does not exist for " + clinic.clinicname + " Clinic.";
        //                    ModelState.AddModelError("", message);
        //                    ViewBag.Message = message;
        //                    return View(user);
        //                }
        //            }
        //            else
        //            {
        //                message = "Sorry,Clinic mobile number is not valid.";
        //                ModelState.AddModelError("", message);
        //                ViewBag.Message = message;
        //                return View(user);
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            ViewBag.Message = ex.Message;
        //            ModelState.AddModelError("", $"An error occurred: {ex.Message}");
        //            return View(user);
        //        }

        //    }

        //    // Add a fallback return statement
        //    ModelState.AddModelError("", "Unexpected error occurred.");
        //    return View(user);

        //}

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> Login(User user)
        {
            //string Path = AppDomain.CurrentDomain.BaseDirectory + @"myfastingapp-bd6ec.json";
            string Path = AppDomain.CurrentDomain.BaseDirectory + @"greenpaperdev-firebase-adminsdk-8k2y5-fb46e63414.json";
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path);
            FirestoreDb db = FirestoreDb.Create("greenpaperdev");
            string message = string.Empty;
            string clinicPlan = "";
            string numericcliniccode = user.cliniccode.Split('-')[1];

            if (user.cliniccode == "" || user.cliniccode == null)
            {

                try {
                    //Query Qref = db.Collection("Students").WhereEqualTo("StudentName","Suvidhi");
                    Query Qref = db.Collection("SuperUsers").WhereEqualTo("UserId", user.mobile_number).WhereEqualTo("Password", user.password).Limit(1);
                    QuerySnapshot snap = await Qref.GetSnapshotAsync();
                    if (snap.Count > 0)
                    {
                        foreach (DocumentSnapshot docsnap in snap)
                        {
                            SuperUser superuser = docsnap.ConvertTo<SuperUser>();
                            if (docsnap.Exists)
                            {
                                //GlobalSessionVariables.UserRoles = "SuperAdmin";
                                FormsAuthentication.SetAuthCookie(superuser.UserName + "-" + superuser.UserName, superuser.RememberMe);

                                return RedirectToAction("Index");
                            }
                            else
                            {
                                message = "Username and/or password is incorrect.";
                                ModelState.AddModelError("", message);
                                ViewBag.Message = message;
                                return View(user);
                            }



                        }
                    }
                    else
                    {
                        message = "Username and/or password is incorrect.";
                        ModelState.AddModelError("", message);
                        ViewBag.Message = message;
                        return View(user);
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"An error occurred: {ex.Message}");
                    ViewBag.Message = ex.Message;
                    return View(user);
                }
                
            }
            else
            {

                try 
                {
                    Query Qref = db.Collection("clinics").WhereEqualTo("clinic_code", user.cliniccode).Limit(1);
                    QuerySnapshot snapClinic = await Qref.GetSnapshotAsync();

                    if (snapClinic.Count > 0)
                    {
                        DocumentSnapshot docsnapClinic = snapClinic.Documents[0];

                        Clinic clinic = docsnapClinic.ConvertTo<Clinic>();
                        clinicPlan = clinic.selected_plan;

                        QuerySnapshot snapUser = await docsnapClinic.Reference.Collection("user").WhereEqualTo("mobile_number", user.mobile_number).GetSnapshotAsync();

                        if (snapUser.Count > 0)
                        {
                            foreach (DocumentSnapshot docsnapUsers in snapUser)
                            {

                                User userLoggedIn = docsnapUsers.ConvertTo<User>();
                                QuerySnapshot snapUserPassword = await docsnapClinic.Reference.Collection("user").WhereEqualTo("mobile_number", user.mobile_number).WhereEqualTo("password", user.password).GetSnapshotAsync();

                                if (snapUserPassword.Count > 0)
                                {
                                    Session["sessionid"] = System.Web.HttpContext.Current.Session.SessionID;

                                    DocumentSnapshot docsnapUser = snapUserPassword.Documents[0];

                                    User userForRoles = docsnapUser.ConvertTo<User>();

                                    FormsAuthentication.SetAuthCookie(userLoggedIn.mobile_number + "|" + userLoggedIn.name + "|" + string.Join("_", userForRoles.user_roles) + "|" + clinic.clinicmobilenumber + "|" + docsnapClinic.Id + "|" + user.cliniccode, true);
                                    
                                    if (userForRoles.user_roles.Contains("Receptionist") || userForRoles.user_roles.Contains("Doctor") || userForRoles.user_roles.Contains("Chemist") || userForRoles.user_roles.Contains("Cashier"))
                                    {
                                        if (userForRoles.user_roles.Contains("Chemist") || userForRoles.user_roles.Contains("Cashier")) 
                                        {
                                            return RedirectToAction("Index", "Appointment");//return RedirectToAction("Waiting", "Appointment");
                                        }
                                        else
                                        {
                                            return RedirectToAction("Index", "Appointment");
                                        }
                                    }
                                    else //for admin
                                    {
                                        return RedirectToAction("Index", "User");
                                    }
                                }
                                else
                                {
                                    message = "Password for user " + user.mobile_number + " is incorrect.";
                                    ModelState.AddModelError("", message);
                                    ViewBag.Message = message;
                                    return View(user);
                                }
                            }
                        }
                        else
                        {
                            message = "User Id " + user.mobile_number + " does not exist for " + clinic.clinicname + " Clinic.";
                            ModelState.AddModelError("", message);
                            ViewBag.Message = message;
                            return View(user);
                        }
                    }
                    else
                    {
                        message = "Sorry,Clinic mobile number is not valid.";
                        ModelState.AddModelError("", message);
                        ViewBag.Message = message;
                        return View(user);
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.Message = ex.Message;
                    ModelState.AddModelError("", $"An error occurred: {ex.Message}");
                    return View(user);
                }

            }

            // Add a fallback return statement
            ModelState.AddModelError("", "Unexpected error occurred.");
            return View(user);

        }


        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> Login1(User user)
        {
            //string Path = AppDomain.CurrentDomain.BaseDirectory + @"myfastingapp-bd6ec.json";
            string Path = AppDomain.CurrentDomain.BaseDirectory + @"greenpaperdev-firebase-adminsdk-8k2y5-fb46e63414.json";
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path);
            FirestoreDb db = FirestoreDb.Create("greenpaperdev");
            string message = string.Empty;
            string clinicPlan = "";
            string numericcliniccode = user.cliniccode.Split('-')[1];

            if (user.cliniccode == "" || user.cliniccode == null)
            {

                try
                {
                    //Query Qref = db.Collection("Students").WhereEqualTo("StudentName","Suvidhi");
                    Query Qref = db.Collection("SuperUsers").WhereEqualTo("UserId", user.mobile_number).WhereEqualTo("Password", user.password).Limit(1);
                    QuerySnapshot snap = await Qref.GetSnapshotAsync();
                    if (snap.Count > 0)
                    {
                        foreach (DocumentSnapshot docsnap in snap)
                        {
                            SuperUser superuser = docsnap.ConvertTo<SuperUser>();
                            if (docsnap.Exists)
                            {
                                //GlobalSessionVariables.UserRoles = "SuperAdmin";
                                FormsAuthentication.SetAuthCookie(superuser.UserName + "-" + superuser.UserName, superuser.RememberMe);

                                return RedirectToAction("Index");
                            }
                            else
                            {
                                message = "Username and/or password is incorrect.";
                                ModelState.AddModelError("", message);
                                ViewBag.Message = message;
                                return View(user);
                            }



                        }
                    }
                    else
                    {
                        message = "Username and/or password is incorrect.";
                        ModelState.AddModelError("", message);
                        ViewBag.Message = message;
                        return View(user);
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"An error occurred: {ex.Message}");
                    ViewBag.Message = ex.Message;
                    return View(user);
                }

            }
            else
            {

                try
                {
                    Query Qref = db.Collection("clinics").WhereEqualTo("clinic_code", user.cliniccode).Limit(1);
                    QuerySnapshot snapClinic = await Qref.GetSnapshotAsync();

                    if (snapClinic.Count > 0)
                    {
                        DocumentSnapshot docsnapClinic = snapClinic.Documents[0];

                        Clinic clinic = docsnapClinic.ConvertTo<Clinic>();
                        clinicPlan = clinic.selected_plan;

                        //GlobalSessionVariables.ClinicMobileNumber = clinic.clinicmobilenumber;
                        //GlobalSessionVariables.ClinicDocumentAutoId = docsnapClinic.Id;
                        //GlobalSessionVariables.ClinicCode = clinic.clinic_code;

                        QuerySnapshot snapUser = await docsnapClinic.Reference.Collection("user").WhereEqualTo("mobile_number", user.mobile_number).GetSnapshotAsync();

                        if (snapUser.Count > 0)
                        {
                            foreach (DocumentSnapshot docsnapUsers in snapUser)
                            {

                                User userLoggedIn = docsnapUsers.ConvertTo<User>();
                                QuerySnapshot snapUserPassword = await docsnapClinic.Reference.Collection("user").WhereEqualTo("mobile_number", user.mobile_number).WhereEqualTo("password", user.password).GetSnapshotAsync();

                                if (snapUserPassword.Count > 0)
                                {
                                    Session["sessionid"] = System.Web.HttpContext.Current.Session.SessionID;
                                    //Session["ClinicMobileNumber"] = clinic.clinicmobilenumber;
                                    //Session["ClinicDocumentAutoId"] = docsnapClinic.Id;
                                    //Session["ClinicDocumentAutoId"] = clinic.clinic_code;

                                    DocumentSnapshot docsnapUser = snapUserPassword.Documents[0];

                                    User userForRoles = docsnapUser.ConvertTo<User>();
                                    //GlobalSessionVariables.UserRoles = string.Join(",", userForRoles.user_roles);


                                    //var authTicket = new FormsAuthenticationTicket(1, userLoggedIn.mobile_number + "|" + userLoggedIn.name + "|" + string.Join("_", userForRoles.user_roles) + "|" + clinic.clinicmobilenumber + "|" + docsnapClinic.Id + "|" + numericcliniccode, DateTime.Now, DateTime.Now.AddMinutes(30), false, ""); // UserData is optional
                                    //var encryptedTicket = FormsAuthentication.Encrypt(authTicket);
                                    //var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket)
                                    //{
                                    //    HttpOnly = true
                                    //};
                                    //Response.Cookies.Add(cookie);


                                    FormsAuthentication.SetAuthCookie(userLoggedIn.mobile_number + "|" + userLoggedIn.name + "|" + string.Join("_", userForRoles.user_roles) + "|" + clinic.clinicmobilenumber + "|" + docsnapClinic.Id + "|" + numericcliniccode, true);
                                    //FormsAuthentication.SetAuthCookie(userLoggedIn.mobile_number, true);
                                    //if(User.IsInRole("Receptionist"))

                                    //// Create identity and add claims
                                    //var claims = new List<Claim>
                                    //{
                                    //    new Claim(ClaimTypes.Name, userLoggedIn.name),
                                    //    new Claim("mobile_number", userLoggedIn.mobile_number), // Custom claim for Mobile Number
                                    //    new Claim("clinicmobilenumber", clinic.clinicmobilenumber),         // Custom claim for Clinic ID
                                    //    new Claim("documentid", docsnapClinic.Id),
                                    //    new Claim("cliniccode", user.cliniccode)         // Custom claim for Clinic ID
                                    //};

                                    //foreach (var role in userForRoles.user_roles)
                                    //{
                                    //    claims.Add(new Claim(ClaimTypes.Role, role));
                                    //}

                                    //var identity = new ClaimsIdentity(claims, DefaultAuthenticationTypes.ApplicationCookie);
                                    //// Sign in the user
                                    //var authManager = HttpContext.GetOwinContext().Authentication;
                                    //authManager.SignIn(new Microsoft.Owin.Security.AuthenticationProperties { IsPersistent = true }, identity);


                                    if (userForRoles.user_roles.Contains("Receptionist"))
                                    {
                                        if (userForRoles.user_roles.Contains("Chemist") || userForRoles.user_roles.Contains("Cashier"))
                                        {
                                            return RedirectToAction("Waiting", "Appointment");
                                        }
                                        else
                                        {
                                            return RedirectToAction("Index", "Appointment");
                                        }
                                    }
                                    else if (userForRoles.user_roles.Contains("Admin"))
                                    {
                                        return RedirectToAction("Index", "User");
                                    }
                                    else
                                    {
                                        return RedirectToAction("Waiting", "Appointment");
                                    }

                                }
                                else
                                {
                                    message = "Password for user " + user.mobile_number + " is incorrect.";
                                    ModelState.AddModelError("", message);
                                    ViewBag.Message = message;
                                    return View(user);
                                }
                            }
                        }
                        else
                        {
                            message = "User Id " + user.mobile_number + " does not exist for " + clinic.clinicname + " Clinic.";
                            ModelState.AddModelError("", message);
                            ViewBag.Message = message;
                            return View(user);
                        }
                    }
                    else
                    {
                        message = "Sorry,Clinic mobile number is not valid.";
                        ModelState.AddModelError("", message);
                        ViewBag.Message = message;
                        return View(user);
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.Message = ex.Message;
                    ModelState.AddModelError("", $"An error occurred: {ex.Message}");
                    return View(user);
                }

            }

            // Add a fallback return statement
            ModelState.AddModelError("", "Unexpected error occurred.");
            return View(user);

        }

        [HttpGet]

        public ActionResult Logout()
        {
            //var authManager = HttpContext.GetOwinContext().Authentication;
            //authManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            //Session.Clear(); // Clear session data
            //Session.Abandon(); // Abandon the session
            //return RedirectToAction("Login", "Home");


            FormsAuthentication.SignOut(); // Sign out the authentication cookie
            Session.Clear(); // Clear all session data
            Session.Abandon(); // Abandon the session
            Response.Cookies.Add(new HttpCookie(FormsAuthentication.FormsCookieName, "") { Expires = DateTime.Now.AddDays(-1) }); // Expire the auth cookie
            return RedirectToAction("Login", "Home");
        }

        public async Task<ActionResult> About()
        {
            
            var userId = "xyz";
            var currentLoginTime = DateTime.UtcNow.ToString("dd-MMM-yyyy HH:mm:ss");

            //Save Non identifying data to firebase
            var currentUserLogin = new LoginData() { TimestampUtc = currentLoginTime };
            var fireBaseClient = new FirebaseClient("https://myfastingapp-bd6ec.firebaseio.com/");
            //Database Secret = AExdT14bkJYfzKsfBxsFoddWEcLiiju1pKtbUj58
            var result = await fireBaseClient.Child("Test/" + userId + "/Logins").PostAsync(currentUserLogin);

            //Retrieve data from Database
            var dbLogins = await fireBaseClient.Child("Test").Child(userId).Child("Logins").OnceAsync<LoginData>();
            //var dbLogins = await fireBaseClient.Child("Test").OnceAsync<LoginData>();

            var timeStampList = new List<string>();

            foreach(var login in dbLogins)
            {
                timeStampList.Add(login.Object.TimestampUtc);

            }

            //Pass Data to View

            ViewBag.CurrentUser = userId;
            ViewBag.Logins = timeStampList.OrderByDescending(x => x);

            return View();

        }
        [AllowAnonymous]
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Denied()
        {
            return View();
        }
    }
}