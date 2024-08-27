using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Net.Http.Headers;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.DataHandler;
using MVCFirebase.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;


namespace MVCFirebase.Controllers
{


    public class PatientAPIController : ApiController
    {

        string constr = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;


        [JwtAuthorize(Roles = "user")]
        [HttpGet]
        [Route("api/PatientAPI/GetPatients")]
        public GenericAPIResult GetPatients(string cliniccode, int pagenumber, int pagesize,DateTime updatedat)
        {
            List<PatientAPI> patients = new List<PatientAPI>();
            GenericAPIResult result = new GenericAPIResult();
            string strUpdatedAt = updatedat.ToString("dd-MMM-yyyy HH:mm:ss");
            var dynamicDt = new List<dynamic>();

            try
            {

                using (SqlConnection conn = new SqlConnection(constr))
                {
                    SqlCommand sqlComm = new SqlCommand("usp_GetPatientsAll", conn);
                    sqlComm.Parameters.Add(new SqlParameter("@ClinicCode", cliniccode));
                    sqlComm.Parameters.Add(new SqlParameter("@PageNumber", pagenumber));
                    sqlComm.Parameters.Add(new SqlParameter("@PageSize", pagesize));
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
                                if (sdr.GetName(i) == "isCreated" || sdr.GetName(i) == "isSynced" || sdr.GetName(i) == "patientAppDownloaded")
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
                result.message = "Patients List fetched Successfully";
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



        [JwtAuthorize(Roles = "user")]
        [HttpGet]
        [Route("api/PatientAPI/GetPatient")]
        public GenericAPIResult GetPatientById(string searchstring, string cliniccode)
        {
            GenericAPIResult result = new GenericAPIResult();

            var dynamicDt = new List<dynamic>();

            try 
            {
                using (SqlConnection conn = new SqlConnection(constr))
                {
                    SqlCommand sqlComm = new SqlCommand("usp_GetPatientById", conn);
                    sqlComm.Parameters.Add(new SqlParameter("@SearchString", searchstring));//1,A1003,Amang,9811035028
                    sqlComm.Parameters.Add(new SqlParameter("@ClinicCode", cliniccode));//GP-101

                    
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
                                if (sdr.GetName(i) == "isCreated" || sdr.GetName(i) == "isSynced" || sdr.GetName(i) == "patientAppDownloaded")
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

                result.message = "Patient fetched Successfully";
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



        [JwtAuthorize(Roles = "user")]
        [HttpPost]
        [Route("api/PatientAPI/CreatePatient")]
        public async Task<GenericAPIResult> CreatePatient([FromBody] PatientAPI ObjPatient)
        {
            GenericAPIResult result = new GenericAPIResult();
            var dynamicDt = new List<dynamic>();
            string errorcode = "";
            string statuscode = "";


            string msg = "";
            
            int maxId = 0;

            if (ObjPatient is null)
            {
                msg = "Patient Data is Blank";
                statuscode = "201";
                errorcode = "true";
            }
            else if (ObjPatient.PatientId != 0)
            {
                msg = "PatientId is not Blank";
                statuscode = "201";
                errorcode = "true";

            }
            else if (ObjPatient.clinicCode == "" || ObjPatient.clinicCode is null)
            {

                msg = "Clinic Code is Blank";
                statuscode = "201";
                errorcode = "true";

            }
            else if (ObjPatient.patient_name == "" || ObjPatient.patient_name is null)
            {
                msg = "Patient Name is Blank";
                statuscode = "201";
                errorcode = "true";

            }
            else if (ObjPatient.updatedAt.ToString() == "01-01-0001 00:00:00")
            {
                msg = "updatedAt is invalid";
                statuscode = "201";
                errorcode = "true";

            }
            else if (ObjPatient.creation_date.ToString() == "01-01-0001 00:00:00")
            {
                msg = "creation_date is invalid";
                statuscode = "201";
                errorcode = "true";

            }
            else if (ObjPatient.loginAt.ToString() == "01-01-0001 00:00:00")
            {
                msg = "loginAt is invalid";
                statuscode = "201";
                errorcode = "true";

            }
            else if (ObjPatient.clinicCode != "")
            {
                using (SqlConnection conn = new SqlConnection(constr))
                {
                    DataTable dtCC = new DataTable();
                    SqlCommand sqlCommCC = new SqlCommand("Select * from clinics where clinic_code = '" + ObjPatient.clinicCode + "'", conn);
                    sqlCommCC.CommandType = CommandType.Text;
                    conn.Open();

                    dtCC.Load(sqlCommCC.ExecuteReader());
                    conn.Close();
                    if (dtCC.Rows.Count == 0)
                    {
                        msg = "Clinic Code " + ObjPatient.clinicCode + " does not exists.";
                        statuscode = "201";
                        errorcode = "true";
                    }
                    else
                    {
                        try
                        {
                            DataTable dtPatDuplicate = new DataTable();
                            SqlCommand sqlCommPatDuplicate = new SqlCommand("Select * from patients where (patient_mobile_number = '" + ObjPatient.patient_mobile_number + "' and patient_name = '" + ObjPatient.patient_name + "') or patient_id = '"+ ObjPatient.patient_id + "'", conn);
                            sqlCommPatDuplicate.CommandType = CommandType.Text;
                            conn.Open();

                            dtPatDuplicate.Load(sqlCommPatDuplicate.ExecuteReader());
                            conn.Close();
                            if (dtPatDuplicate.Rows.Count > 0)
                            {
                                msg = "Duplicate Patient, Patient Name "+ ObjPatient.patient_name  + " with Mobile number "+ ObjPatient.patient_mobile_number + " already exists / or /  patient_id " + ObjPatient.patient_id + " already exists";
                            }
                            else
                            {
                                DataTable dt = new DataTable();
                                SqlCommand sqlComm = new SqlCommand("Select Max(patient_id) as patient_id from patients where cliniccode = '" + ObjPatient.clinicCode + "'", conn);
                                sqlComm.CommandType = CommandType.Text;
                                conn.Open();

                                dt.Load(sqlComm.ExecuteReader());
                                conn.Close();


                                string patient_id = dt.Rows[0]["patient_id"].ToString();

                                patient_id = patient_id.Substring(1, 4);

                                int numericpatient_id = Convert.ToInt32(patient_id) + 1;

                                int len = numericpatient_id.ToString().Length;
                                if (len == 1)
                                { patient_id = "A000" + Convert.ToString(numericpatient_id); }
                                if (len == 2)
                                { patient_id = "A00" + Convert.ToString(numericpatient_id); }
                                if (len == 3)
                                { patient_id = "A0" + Convert.ToString(numericpatient_id); }
                                if (len == 4)
                                { patient_id = "A" + Convert.ToString(numericpatient_id); }


                                DataTable dtId = new DataTable();
                                SqlCommand sqlCommId = new SqlCommand("Select Max(PatientId) as Id from patients", conn);
                                sqlCommId.CommandType = CommandType.Text;
                                conn.Open();

                                dtId.Load(sqlCommId.ExecuteReader());
                                conn.Close();

                                maxId = Convert.ToInt32(dtId.Rows[0]["Id"].ToString()) + 1;

                                using (SqlCommand sqlCommPatientInsert = new SqlCommand(
                                " Insert into patients(" +
                                " PatientId, documentId, clinicId, clinicCode, search_text, care_of, patient_id, " +
                                "patient_name, gender, age, patient_mobile_number, refer_by, disease, refer_to_doctor," +
                                " id, city, severity, creation_date, updatedAt, isCreated, isSynced, createdBy, " +
                                "loginAt, patientAppDownloaded, dob" +
                                " ) values (" +
                                "@PatientId, @DocumentId, @clinicId, @clinicCode, @search_text, @care_of, @patient_id" +
                                ", @patient_name, @gender, @age,  @patient_mobile_number, @refer_by, @disease," +
                                " @refer_to_doctor, @id, @city, @severity, @creation_date, @updatedAt,@isCreated, @isSynced," +
                                " @createdBy,@loginAt,@patientAppDownloaded,@dob)", conn))
                                {
                                    sqlCommPatientInsert.CommandType = CommandType.Text;
                                    sqlCommPatientInsert.Parameters.AddWithValue("@PatientId", maxId);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@DocumentId", ObjPatient.documentId ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@clinicId", ObjPatient.clinicId ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@clinicCode", ObjPatient.clinicCode ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@search_text", ObjPatient.search_text ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@care_of", ObjPatient.care_of ?? (object)DBNull.Value);
                                    if (ObjPatient.patient_id is null || ObjPatient.patient_id == "")
                                    {
                                        sqlCommPatientInsert.Parameters.AddWithValue("@patient_id", patient_id);
                                    }
                                    else
                                    {
                                        sqlCommPatientInsert.Parameters.AddWithValue("@patient_id", ObjPatient.patient_id);
                                    }
                                    sqlCommPatientInsert.Parameters.AddWithValue("@patient_name", ObjPatient.patient_name ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@gender", ObjPatient.gender ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@age", ObjPatient.age ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@patient_mobile_number", ObjPatient.patient_mobile_number ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@refer_by", ObjPatient.refer_by ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@disease", ObjPatient.disease ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@refer_to_doctor", ObjPatient.refer_to_doctor ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@id", ObjPatient.id ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@city", ObjPatient.city ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@severity", ObjPatient.severity ?? (object)DBNull.Value);
                                    if (ObjPatient.creation_date is null || ObjPatient.creation_date.ToString() == "")
                                    {
                                        sqlCommPatientInsert.Parameters.AddWithValue("@creation_date", DateTime.Now);
                                    }
                                    else
                                    {
                                        sqlCommPatientInsert.Parameters.AddWithValue("@creation_date", ObjPatient.creation_date);
                                    }
                                    if (ObjPatient.updatedAt is null || ObjPatient.updatedAt.ToString() == "")
                                    {
                                        sqlCommPatientInsert.Parameters.AddWithValue("@updatedAt", DateTime.Now);
                                    }
                                    else
                                    {
                                        sqlCommPatientInsert.Parameters.AddWithValue("@updatedAt", ObjPatient.updatedAt);
                                    }

                                    sqlCommPatientInsert.Parameters.AddWithValue("@isCreated", ObjPatient.isCreated ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@isSynced", ObjPatient.isSynced ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@createdBy", ObjPatient.createdBy ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@loginAt", ObjPatient.loginAt ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@patientAppDownloaded", ObjPatient.patientAppDownloaded ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@dob", ObjPatient.dob ?? (object)DBNull.Value);
                                    conn.Open();
                                    sqlCommPatientInsert.ExecuteNonQuery();
                                }

                                msg = "Patient Successfully Created";


                                statuscode = "200";
                                errorcode = "false";
                            }



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


            #region Code to update Firebase Listener

            string Path = AppDomain.CurrentDomain.BaseDirectory + @"greenpaperdev-firebase-adminsdk-8k2y5-fb46e63414.json";
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path);
            FirestoreDb db = FirestoreDb.Create("greenpaperdev");


            try
            {
                //Query Qref = db.Collection("clinics").WhereEqualTo("clinic_code", ObjPatient.clinicCode).Limit(1);
                //QuerySnapshot snapClinic = await Qref.GetSnapshotAsync();

                //if (snapClinic.Count > 0)
                //{
                //    DocumentSnapshot docSnapClinic = snapClinic.Documents[0];
                //    Clinic clinic = docSnapClinic.ConvertTo<Clinic>();

                //    CollectionReference col1 = db.Collection("clinics").Document(docSnapClinic.Id).Collection("WebAPIResponse");

                //    Dictionary<string, object> data1 = new Dictionary<string, object>
                //    {
                //        {"CollectionName" ,"Patient" },
                //        {"UpdatedAt" ,DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc)},
                //    };

                //    await col1.Document().SetAsync(data1);
                //}

                CollectionReference col1 = db.Collection("WebAPIResponse");
                // Specify the document ID 'GP-101'
                DocumentReference doc1 = col1.Document(ObjPatient.clinicCode);

                // Delete the document if it exists
                await doc1.DeleteAsync();

                Dictionary<string, object> data1 = new Dictionary<string, object>
                        {
                            {"CollectionName" ,"Patient Created" },
                            {"UpdatedAt" ,DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc)},

                        };

                // Set the data for the document with the specified ID
                await doc1.SetAsync(data1);

            }
            catch (Exception ex)
            {
                msg = ex.Message;
                statuscode = "201";
                errorcode = "true";
            }

            #endregion

            result.message = msg;
            result.statusCode = statuscode;
            result.error = errorcode;
            result.data = dynamicDt;

            return result;
            //  return msg;
        }



        [JwtAuthorize(Roles = "user")]
        [HttpPost]
        [Route("api/PatientAPI/UpdatePatient")]
        public async Task<GenericAPIResult> UpdatePatient([FromBody] PatientAPI ObjPatient)
        {
            GenericAPIResult result = new GenericAPIResult();
            var dynamicDt = new List<dynamic>();
            string errorcode = "";
            string statuscode = "";

            string msg = "";


            if (ObjPatient is null)
            {
                msg = "Patient Data is Blank";
                statuscode = "201";
                errorcode = "true";
            }
            else if (ObjPatient.PatientId == 0)
            {
                msg = "PatientId is Blank";
                statuscode = "201";
                errorcode = "true";
            }
            else if (ObjPatient.clinicCode == "" || ObjPatient.clinicCode is null)
            {
                msg = "Clinic Code is Blank";
                statuscode = "201";
                errorcode = "true";
            }
            else if (ObjPatient.patient_name == "" || ObjPatient.patient_name is null)
            {
                msg = "Patient Name is Blank";
                statuscode = "201";
                errorcode = "true";

            }
            else if (ObjPatient.loginAt.ToString() == "01-01-0001 00:00:00")
            {
                msg = "loginAt is invalid";
                statuscode = "201";
                errorcode = "true";

            }
            else if (ObjPatient.clinicCode != "")
            {
                using (SqlConnection conn = new SqlConnection(constr))
                {
                    DataTable dtCC = new DataTable();
                    SqlCommand sqlComm = new SqlCommand("Select * from clinics where clinic_code = '" + ObjPatient.clinicCode + "'", conn);
                    sqlComm.CommandType = CommandType.Text;
                    conn.Open();

                    dtCC.Load(sqlComm.ExecuteReader());
                    conn.Close();
                    if (dtCC.Rows.Count == 0)
                    {
                        msg = "Clinic Code " + ObjPatient.clinicCode + " does not exists.";
                        statuscode = "201";
                        errorcode = "true";
                    }
                    else
                    {
                        try
                        {
                            DataTable dtPatDuplicate = new DataTable();
                            SqlCommand sqlCommPatDuplicate = new SqlCommand("Select * from patients where patient_mobile_number = '" + ObjPatient.patient_mobile_number + "' and patient_name = '" + ObjPatient.patient_name + "' and PatientId <> '" + ObjPatient.PatientId + "' and clinicCode = '"+ ObjPatient.clinicCode + "'", conn);
                            sqlCommPatDuplicate.CommandType = CommandType.Text;
                            conn.Open();

                            dtPatDuplicate.Load(sqlCommPatDuplicate.ExecuteReader());
                            conn.Close();
                            if (dtPatDuplicate.Rows.Count > 0)
                            {
                                msg = "Duplicate Patient, Patient Name "+ObjPatient.patient_name +" with Mobile number "+ObjPatient.patient_mobile_number+" already exists ";
                            }
                            else
                            {

                                using (SqlCommand sqlCommPatientInsert = new SqlCommand(
                                " Update patients set search_text = @search_text, care_of = @care_of, " +
                                "patient_name = @patient_name, gender = @gender, age = @age, patient_mobile_number = @patient_mobile_number, " +
                                "refer_by=@refer_by, disease=@disease, refer_to_doctor = @refer_to_doctor," +
                                " city=@city, severity=@severity, updatedAt=@updatedAt, isCreated=@isCreated, isSynced=@isSynced, " +
                                "loginAt=@loginAt, patientAppDownloaded=@patientAppDownloaded, dob=@dob where PatientId = @PatientId and Cliniccode = @clinicCode ", conn))
                                {
                                    sqlCommPatientInsert.CommandType = CommandType.Text;
                                    sqlCommPatientInsert.Parameters.AddWithValue("@PatientId", ObjPatient.PatientId);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@clinicCode", ObjPatient.clinicCode ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@search_text", ObjPatient.search_text ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@care_of", ObjPatient.care_of ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@patient_name", ObjPatient.patient_name ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@gender", ObjPatient.gender ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@age", ObjPatient.age ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@patient_mobile_number", ObjPatient.patient_mobile_number ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@refer_by", ObjPatient.refer_by ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@disease", ObjPatient.disease ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@refer_to_doctor", ObjPatient.refer_to_doctor ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@city", ObjPatient.city ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@severity", ObjPatient.severity ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@updatedAt", DateTime.Now);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@isCreated", ObjPatient.isCreated ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@isSynced", ObjPatient.isSynced ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@loginAt", ObjPatient.loginAt ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@patientAppDownloaded", ObjPatient.patientAppDownloaded ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@dob", ObjPatient.dob ?? (object)DBNull.Value);
                                    conn.Open();
                                    sqlCommPatientInsert.ExecuteNonQuery();
                                }

                                //SqlCommand sqlCommPatientUpdate = new SqlCommand("Update patients set search_text = '" + ObjPatient.search_text + "', care_of = '" + ObjPatient.care_of + "'," +
                                //    "patient_name = '" + ObjPatient.patient_name + "', gender = '" + ObjPatient.gender + "', age = '" + ObjPatient.age + "'," +
                                //    " patient_mobile_number = '" + ObjPatient.patient_mobile_number + "', refer_by = '" + ObjPatient.refer_by + "', disease = '" + ObjPatient.disease + "', refer_to_doctor = '" + ObjPatient.refer_to_doctor + "'," +
                                //    " city = '" + ObjPatient.city + "', severity = '" + ObjPatient.severity + "', updatedAt = '" + DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss") + "', isCreated = '" + ObjPatient.isCreated + "', isSynced = '" + ObjPatient.isSynced + "', createdBy = '" + ObjPatient.createdBy + "', " +
                                //    //"loginAt = '" + Convert.ToDateTime(ObjPatient.loginAt) + "', patientAppDownloaded = '" + ObjPatient.patientAppDownloaded + "', dob = '" + Convert.ToDateTime(ObjPatient.dob) + "'", conn);
                                //    "loginAt = '" + ObjPatient.loginAt + "', patientAppDownloaded = '" + ObjPatient.patientAppDownloaded + "' where PatientId = '" + ObjPatient.PatientId + "' and Cliniccode = '" + ObjPatient.clinicCode + "'", conn);
                                //sqlCommPatientUpdate.CommandType = CommandType.Text;
                                //conn.Open();

                                //sqlCommPatientUpdate.ExecuteNonQuery();

                                msg = "Patient Successfully Updated";
                                statuscode = "200";
                                errorcode = "false";
                            }



                        }
                        catch (Exception ex)
                        {
                            msg = "Error:" + ex.Message;
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


            //return JsonConvert.SerializeObject(msg);
            //  return msg;

            #region Code to update Firebase Listener

            string Path = AppDomain.CurrentDomain.BaseDirectory + @"greenpaperdev-firebase-adminsdk-8k2y5-fb46e63414.json";
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path);
            FirestoreDb db = FirestoreDb.Create("greenpaperdev");


            try
            {

                CollectionReference col1 = db.Collection("WebAPIResponse");
                // Specify the document ID 'GP-101'
                DocumentReference doc1 = col1.Document(ObjPatient.clinicCode);

                // Delete the document if it exists
                await doc1.DeleteAsync();

                Dictionary<string, object> data1 = new Dictionary<string, object>
                        {
                            {"CollectionName" ,"Patient Updated" },
                            {"UpdatedAt" ,DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc)},

                        };

                // Set the data for the document with the specified ID
                await doc1.SetAsync(data1);

            }
            catch (Exception ex)
            {
                msg = ex.Message;
                statuscode = "201";
                errorcode = "true";
            }

            #endregion

            result.message = msg;
            result.statusCode = statuscode;
            result.error = errorcode;
            result.data = dynamicDt;

            return result; ;

        }

        [HttpPost]
        [Route("api/PatientAPI/SubmittedData")]
        public string SubmittedData([FromBody] PatientAPI ObjPatient)
        {
            string msg = ObjPatient.creation_date.ToString();
            



            return JsonConvert.SerializeObject(msg);
            //  return msg;
        }


        [HttpGet]
        [Route("api/PatientAPI/GetPatientsJsonStringOnly")]
        public string GetPatientsJsonStringOnly()
        {
            string result = "";

            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(constr))
            {
                SqlCommand sqlComm = new SqlCommand("usp_GetPatientsAllJson", conn);
                sqlComm.CommandType = CommandType.StoredProcedure;
                conn.Open();

                dt.Load(sqlComm.ExecuteReader());
                conn.Close();
            }


            result = dt.Rows[0]["jsonvalue"].ToString();

            return result;
        }


    }
}
