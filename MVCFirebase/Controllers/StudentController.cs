
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
using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;

namespace MVCFirebase.Controllers
{
    public class StudentController : Controller
    {
            
        

        FirebaseClient fireBaseClient = new FirebaseClient("https://myfastingapp-bd6ec.firebaseio.com/");

                       IList<Student> studentList = new List<Student>{
                            new Student() { StudentId = 1, StudentName = "John", Age = 18 } ,
                            new Student() { StudentId = 2, StudentName = "Steve",  Age = 21 } ,
                            new Student() { StudentId = 3, StudentName = "Bill",  Age = 25 } ,
                            new Student() { StudentId = 4, StudentName = "Ram" , Age = 20 } ,
                            new Student() { StudentId = 5, StudentName = "Ron" , Age = 31 } ,
                            new Student() { StudentId = 4, StudentName = "Chris" , Age = 17 } ,
                            new Student() { StudentId = 4, StudentName = "Rob" , Age = 19 }
                        };

        // GET: Student
        [Authorize]
        public async Task<ActionResult> Index()
        {
            ////Retrieve data from firebase RealTime Database 
            //var students = await fireBaseClient.Child("Students").OnceAsync<Student>();


            //List<Student> studentList2 = new List<Student>();

            //foreach (var s in students)
            //{
            //    Student stu = new Student();
            //    stu.StudentId = s.Object.StudentId;
            //    stu.StudentName = s.Object.StudentName;
            //    stu.Age = s.Object.Age;
            //    studentList2.Add(stu);

            //}
            //return View(studentList2);

            //Retrieve Data from Cloud FireStore

            string Path = AppDomain.CurrentDomain.BaseDirectory + @"greenpaperdev-firebase-adminsdk-8k2y5-fb46e63414.json";
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path);
            FirestoreDb db = FirestoreDb.Create("greenpaperdev-firebase-adminsdk-8k2y5-fb46e63414");


            List<Student> studentList3 = new List<Student>();
            //Query Qref = db.Collection("Students").WhereEqualTo("StudentName","Suvidhi");
            Query Qref = db.Collection("Students");
            QuerySnapshot snap = await Qref.GetSnapshotAsync();

            foreach (DocumentSnapshot docsnap in snap)
            {
                Student std = docsnap.ConvertTo<Student>();
                if(docsnap.Exists)
                {
                    studentList3.Add(std);
                }
            }

            return View(studentList3);
        }
        [Authorize]
        // GET: Student/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Student/Create
        public ActionResult Create()
        {
            return View();
        }
        
        [Authorize]
        // POST: Student/Create
        [HttpPost]
        public ActionResult Create(Student std)
        {
            string Path = AppDomain.CurrentDomain.BaseDirectory + @"greenpaperdev-firebase-adminsdk-8k2y5-fb46e63414.json";
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path);
            FirestoreDb db = FirestoreDb.Create("greenpaperdev");


            try
            {
                //CollectionReference col1 = db.Collection("Students").Document().Collection("Courses");
                //Dictionary<string, object> data1 = new Dictionary<string, object>
                //{
                //    {"CourseId" ,"1" },
                //    {"CourseName" ,"Hindi"}

                //};


                CollectionReference col1 = db.Collection("Students");
                Dictionary<string, object> data1 = new Dictionary<string, object>
                {
                    {"StudentId" ,std.StudentId },
                    {"StudentName" ,std.StudentName},
                    {"Age",std.Age}
                };

                col1.AddAsync(data1);


                // TODO: Add insert logic here
                //var result = await fireBaseClient.Child("Students").PostAsync(std);

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
        [Authorize]
        // GET: Student/Edit/5
        public ActionResult Edit(int id)
        {
            var std = studentList.Where(s => s.StudentId == id).FirstOrDefault();

            return View(std);
        }

        // POST: Student/Edit/5
        [HttpPost]
        [Authorize]
        public ActionResult Edit(Student std)
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

        // GET: Student/Delete/5
        [Authorize]
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Student/Delete/5
        [HttpPost]
        [Authorize]
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
