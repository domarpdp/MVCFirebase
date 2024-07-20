using MVCFirebase.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.IO;
using System.Linq;
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

        [JwtAuthorize(Roles = "user")]
        [HttpPost]
        [Route("api/PrescriptionAPI/CreatePrescription")]
        public GenericAPIResult CreatePrescription([FromBody] PrescriptionAPI Obj)
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
            else if (Obj.timeStamp is null)
            {

                msg = "timeStamp is Blank";
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

                                var folderPath = Path.Combine(rootPath, "Prescriptions", Obj.clinicCode, Obj.patientId);
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
                                
                                var filePath = Path.Combine(folderPath, fileName);

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
                                    sqlCommPatientInsert.Parameters.AddWithValue("@timeStamp", Obj.timeStamp ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@updatedAt", Obj.updatedAt ?? (object)DBNull.Value);

                                    conn.Open();
                                    sqlCommPatientInsert.ExecuteNonQuery();
                                }

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
        public GenericAPIResult GetPrescriptionsPatientWise(string cliniccode, string patientid)
        {


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