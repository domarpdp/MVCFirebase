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

namespace MVCFirebase.Controllers
{[Authorize]
    public class HomeController : Controller
    {
        [Authorize]
        public ActionResult Index()
        {
            return View();
        }
        [AllowAnonymous]
        public ActionResult Login()
        {
            return View();
        }

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
            

            if (user.clinicmobilenumber == "" || user.clinicmobilenumber == null)
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
                            FormsAuthentication.SetAuthCookie(superuser.UserName, superuser.RememberMe);
                            return RedirectToAction("Index");
                        }
                        else
                        {
                            message = "Username and/or password is incorrect.";
                        }



                    }
                }
                else if (user.mobile_number == "administrator" && user.password == "jAy@4231")
                {
                    FormsAuthentication.SetAuthCookie("Admin", user.RememberMe);
                    return RedirectToAction("Index");
                }
                else
                {
                    message = "Username and/or password is incorrect.";
                }
            }
            else
            {
                //DocumentReference docref = db.Collection("clinics").Document("");
                //DocumentSnapshot docsnap = await docref.GetSnapshotAsync();

                Query Qref = db.Collection("clinics").WhereEqualTo("clinicmobilenumber", user.clinicmobilenumber).Limit(1);
                QuerySnapshot snapClinic = await Qref.GetSnapshotAsync();

                if (snapClinic.Count > 0)
                {
                    DocumentSnapshot docsnapClinic = snapClinic.Documents[0];
                    
                    Clinic clinic = docsnapClinic.ConvertTo<Clinic>();
                    clinicPlan = clinic.selected_plan;

                    GlobalSessionVariables.ClinicMobileNumber = clinic.clinicmobilenumber;
                    GlobalSessionVariables.ClinicDocumentAutoId = docsnapClinic.Id; 

                    QuerySnapshot snapUser = await docsnapClinic.Reference.Collection("user").WhereEqualTo("mobile_number", user.mobile_number).GetSnapshotAsync();

                    if (snapUser.Count > 0)
                    {
                        foreach (DocumentSnapshot docsnapUsers in snapUser)
                        {

                            User userLoggedIn = docsnapUsers.ConvertTo<User>();
                            QuerySnapshot snapUserPassword = await docsnapClinic.Reference.Collection("user").WhereEqualTo("password", user.password).GetSnapshotAsync();

                            if (snapUserPassword.Count > 0)
                            {
                                FormsAuthentication.SetAuthCookie(userLoggedIn.name, user.RememberMe);
                                return RedirectToAction("Index","Patient");
                            }
                            else
                            {
                                message = "Password for user " + user.mobile_number + " is incorrect.";
                            }
                        }
                    }
                    else
                    {
                        message = "User Id " + user.mobile_number + " does not exist for " + clinic.clinicname + " Clinic.";
                    }
                }
                else
                {
                    message = "Sorry,Clinic mobile number is not valid.";
                }
            }
            
            ViewBag.Message = message;

            //UsersEntities usersEntities = new UsersEntities();
            //int? userId = usersEntities.ValidateUser(user.Username, user.Password).FirstOrDefault();

            //string message = string.Empty;
            //switch (userId.Value)
            //{
            //    case -1:
            //        message = "Username and/or password is incorrect.";
            //        break;
            //    case -2:
            //        message = "Account has not been activated.";
            //        break;
            //    default:
            //        FormsAuthentication.SetAuthCookie(user.Username, user.RememberMe);
            //        return RedirectToAction("Profile");
            //}

            //ViewBag.Message = message;
            //if (user.Username == "administrator" && user.Password == "jAy@4231")
            //{
            //    FormsAuthentication.SetAuthCookie(user.Username, user.RememberMe);
            //    return RedirectToAction("Index");
            //}
            

            return View(user);
        }

        [HttpPost]
        [Authorize]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Abandon();
            return RedirectToAction("Login");
        }
        [Authorize]
        public async Task<ActionResult> About()
        {
            
            var userId = "amangbhatia";
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
        [Authorize]
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}