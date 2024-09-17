using Google.Cloud.Firestore;
using MVCFirebase.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Http;
//using System.Web.Mvc;

namespace MVCFirebase.Controllers
{
    public class PrescriptionAPIController : ApiController
    {
        // GET: PrescriptionAPI
        //public IHttpActionResult Get()
        //{
        //    // Get the root path of the web application
        //    string rootPath = HostingEnvironment.MapPath("~");

        //    // Now you can use rootPath to access files within the wwwroot folder
        //    string filePath = System.IO.Path.Combine(rootPath, "yourfile.txt");

        //    return Ok(filePath);
        //}

        string constr = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        static DateTime utcTime = DateTime.UtcNow;
        static TimeZoneInfo istZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime istTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, istZone);

        [JwtAuthorize(Roles = "user")]
        [HttpPost]
        [Route("api/PrescriptionAPI/CreatePrescription")]
        public async Task<GenericAPIResult> CreatePrescription([FromBody] PrescriptionAPI Obj)
        {
            GenericAPIResult result = new GenericAPIResult();
            var dynamicDt = new List<dynamic>();
            string errorcode = "";
            string statuscode = "";


            string msg = "";

            

            if (Obj is null)
            {
                msg = "Prescription Data is Blank";
                statuscode = "201";
                errorcode = "true";
            }
            else if (Obj.patientId == "")
            {
                msg = "patient_id is Blank";
                statuscode = "201";
                errorcode = "true";

            }

            else if (Obj.clinicCode == "" || Obj.clinicCode is null)
            {

                msg = "clinicCode is Blank";
                statuscode = "201";
                errorcode = "true";

            }
            else if (Obj.clinicId == "" || Obj.clinicId is null)
            {

                msg = "clinicId is Blank";
                statuscode = "201";
                errorcode = "true";

            }
            else if (Obj.clinicCode != "")
            {
                using (SqlConnection conn = new SqlConnection(constr))
                {
                    DataTable dtCC = new DataTable();
                    SqlCommand sqlCommCC = new SqlCommand("Select * from clinics where clinic_code = '" + Obj.clinicCode + "'", conn);
                    sqlCommCC.CommandType = CommandType.Text;
                    conn.Open();

                    dtCC.Load(sqlCommCC.ExecuteReader());
                    conn.Close();
                    if (dtCC.Rows.Count == 0)
                    {
                        msg = "Clinic Code " + Obj.clinicCode + " does not exists.";
                        statuscode = "201";
                        errorcode = "true";
                    }
                    else
                    {
                        DataTable dtPatient = new DataTable();
                        SqlCommand sqlCommPatient = new SqlCommand("Select * from patients where patient_id = '" + Obj.patientId + "' and clinicCode = '" + Obj.clinicCode + "'", conn);
                        sqlCommPatient.CommandType = CommandType.Text;
                        conn.Open();

                        dtPatient.Load(sqlCommPatient.ExecuteReader());
                        conn.Close();
                        if (dtPatient.Rows.Count == 0)
                        {
                            msg = "Patient Id " + Obj.patientId + " does not exists.";
                            statuscode = "201";
                            errorcode = "true";
                        }
                        else
                        {


                            try
                            {
                                int maxId = 0;



                                // Get the root path of the web application
                                string rootPath = HostingEnvironment.MapPath("~");

                                var folderPath = System.IO.Path.Combine(rootPath, "Prescriptions", Obj.clinicCode, Obj.patientId);
                                //var folderPath = Path.Combine(rootPath, "Prescriptions");

                                // Check if directory exists
                                if (!Directory.Exists(folderPath))
                                {
                                    // If not, create it
                                    Directory.CreateDirectory(folderPath);
                                    
                                }

                                //long timestamp = (long)Obj.timeStampLong;
                                //DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(timestamp);
                                //DateTime dateTime = dateTimeOffset.UtcDateTime;

                                string fileName = "";

                                //if (Obj.timeStamp == null)
                                //{
                                //    fileName = $"{Obj.patientId}_{DateTime.Now.Date:yyyyMMdd}";
                                //    createdOn = DateTime.Now;
                                //}
                                //else
                                //{
                                //    fileName = $"{Obj.patientId}_{Obj.timeStamp:yyyyMMdd}";
                                //    createdOn = (DateTime)Obj.timeStamp;
                                //}

                                fileName = $"{Obj.patientId}_{Obj.timeStamp:yyyyMMdd}";

                                var request = HttpContext.Current.Request;
                                string baseUrl = $"{request.Url.Scheme}://{request.Url.Authority}/";


                                fileName = fileName + ".JPEG";
                                
                                var filePath = System.IO.Path.Combine(folderPath, fileName);

                                byte[] fileBytes = Convert.FromBase64String(Obj.file);

                                System.IO.File.WriteAllBytes(filePath, fileBytes);

                                string fileNameWithBaseURL = baseUrl + "Prescriptions/" + Obj.clinicCode + "/" + Obj.patientId + "/" + fileName;


                                DataTable dt = new DataTable();
                                SqlCommand sqlComm = new SqlCommand("Select isnull(Max(Id),0) as Id from prescriptions", conn);
                                sqlComm.CommandType = CommandType.Text;
                                conn.Open();

                                dt.Load(sqlComm.ExecuteReader());
                                conn.Close();

                                maxId = Convert.ToInt32(dt.Rows[0]["Id"].ToString()) + 1;

                                using (SqlCommand sqlCommPatientInsert = new SqlCommand(
                                "INSERT INTO prescriptions (" +
                                "Id, documentId, receptionist, chemist, cashier, doctor, days, fee, date, isPrescription,  fileUrl, patientId, clinicId, clinicCode, isCreated, isSynced, isDeleted, timeStamp, updatedAt " +
                                ") VALUES (" +
                                "@Id, @DocumentId, @receptionist, @chemist, @cashier, @doctor, @days, @fee, @date, @isPrescription,  @fileUrl, @patientId, @clinicId, @clinicCode, @isCreated, @isSynced, @isDeleted, @timeStamp, @updatedAt)", conn))
                                {
                                    sqlCommPatientInsert.CommandType = CommandType.Text;

                                    sqlCommPatientInsert.Parameters.AddWithValue("@Id", maxId);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@DocumentId", Obj.documentId ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@receptionist", Obj.receptionist ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@chemist", Obj.chemist ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@cashier", Obj.cashier ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@doctor", Obj.doctor ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@days", Obj.days ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@fee", Obj.fee ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@date", Obj.date ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@isPrescription", Obj.isPrescription ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@fileUrl", fileNameWithBaseURL);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@patientId", Obj.patientId ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@clinicId", Obj.clinicId ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@clinicCode", Obj.clinicCode);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@isCreated", Obj.isCreated ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@isSynced", Obj.isSynced ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@isDeleted", Obj.isDeleted ?? (object)DBNull.Value);

                                    if (Obj.timeStamp is null || Obj.timeStamp.ToString() == "")
                                    {
                                        sqlCommPatientInsert.Parameters.AddWithValue("@timeStamp", istTime);
                                    }
                                    else
                                    {
                                        sqlCommPatientInsert.Parameters.AddWithValue("@timeStamp", Obj.timeStamp);
                                    }
                                    if (Obj.updatedAt is null || Obj.updatedAt.ToString() == "")
                                    {
                                        sqlCommPatientInsert.Parameters.AddWithValue("@updatedAt", istTime);
                                    }
                                    else
                                    {
                                        sqlCommPatientInsert.Parameters.AddWithValue("@updatedAt", Obj.updatedAt);
                                    }


                                    conn.Open();
                                    sqlCommPatientInsert.ExecuteNonQuery();
                                }


                                #region Code to update Firebase Listener

                                string Path1 = AppDomain.CurrentDomain.BaseDirectory + @"greenpaperdev-firebase-adminsdk-8k2y5-fb46e63414.json";
                                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path1);
                                FirestoreDb db = FirestoreDb.Create("greenpaperdev");


                                try
                                {

                                    CollectionReference col1 = db.Collection("WebAPIResponse");
                                    // Specify the document ID 'GP-101'
                                    DocumentReference doc1 = col1.Document(Obj.clinicId);

                                    // Delete the document if it exists
                                    await doc1.DeleteAsync();

                                    Dictionary<string, object> data1 = new Dictionary<string, object>
                                    {
                                        {"CollectionName" ,"Prescription" },
                                        {"UpdatedAt" ,DateTime.SpecifyKind(DateTime.Now.AddHours(-5).AddMinutes(-30), DateTimeKind.Utc)},

                                    };

                                    // Set the data for the document with the specified ID
                                    await doc1.SetAsync(data1);

                                    msg = "Prescription Successfully Created";
                                    statuscode = "200";
                                    errorcode = "false";

                                }
                                catch (Exception ex)
                                {
                                    msg = ex.Message;
                                    statuscode = "201";
                                    errorcode = "true";
                                }

                                #endregion



                            }
                            catch (Exception ex)
                            {
                                msg = ex.Message;
                                statuscode = "201";
                                errorcode = "true";

                            }
                            finally
                            {
                                conn.Close();
                            }


                        }
                    }


                }


            }

            result.message = msg;
            result.statusCode = statuscode;
            result.error = errorcode;
            result.data = dynamicDt;

            return result; ;
            //  return msg;
        }


        [JwtAuthorize(Roles = "user")]
        [HttpGet]
        [Route("api/PrescriptionAPI/GetPrescriptionsPatientWise")]
        //public GenericAPIResult GetAppointments(string cliniccode,string statussearch int pagenumber, int pagesize, DateTime date)
        public GenericAPIResult GetPrescriptionsPatientWise(string cliniccode, string patientid,DateTime updatedat)
        {

            string strUpdatedAt = updatedat.ToString("dd-MMM-yyyy");
            List<AppointmentAPI> patients = new List<AppointmentAPI>();
            GenericAPIResult result = new GenericAPIResult();

            var dynamicDt = new List<dynamic>();

            try
            {

                using (SqlConnection conn = new SqlConnection(constr))
                {
                    SqlCommand sqlComm = new SqlCommand("usp_GetPrescriptionsPatientWise", conn);
                    sqlComm.Parameters.Add(new SqlParameter("@ClinicCode", cliniccode));
                    sqlComm.Parameters.Add(new SqlParameter("@PatientId", patientid));
                    sqlComm.Parameters.Add(new SqlParameter("@UpdatedAt", strUpdatedAt));

                    sqlComm.CommandType = CommandType.StoredProcedure;
                    conn.Open();

                    using (SqlDataReader sdr = sqlComm.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            dynamic row = new ExpandoObject();
                            var dictionary = (IDictionary<string, object>)row;

                            for (int i = 0; i < sdr.FieldCount; i++)
                            {
                                if (sdr.GetName(i) == "isPrescription" || sdr.GetName(i) == "isCreated" || sdr.GetName(i) == "isSynced" || sdr.GetName(i) == "isDeleted")
                                {
                                    // Convert 1 or 0 to boolean true or false
                                    bool fieldValue = Convert.ToInt32(sdr.GetValue(i)) == 1;
                                    dictionary.Add(sdr.GetName(i), fieldValue);
                                }
                                else
                                {
                                    dictionary.Add(sdr.GetName(i), sdr.GetValue(i));
                                }
                            }

                            dynamicDt.Add(row);
                        }

                    }
                    conn.Close();
                }
                result.message = "Prescription List for Patient " + patientid + " fetched Successfully";
                result.statusCode = "200";
                result.error = "false";
                result.data = dynamicDt;
            }
            catch (Exception ex)
            {
                result.message = ex.Message;
                result.statusCode = "201";
                result.error = "true";
                result.data = dynamicDt;
            }

            return result;

        }

    }
}