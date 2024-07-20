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

        #region Commented Code GetPatients
        //[JwtAuthorize(Roles = "user")]
        //[HttpGet]
        //[Route("api/PatientAPI/GetPatients")]
        //public List<PatientAPI> GetPatients(string cliniccode,int pagenumber,int pagesize)
        //{
        //    //string accessToken = "r8o8tCJkEWBTreCluikbgK6AktzteUMVGc1U6Y2kHVMCdGyHyxuJCOC5k_R9Mt2FnLXuhAje2En4we7f9I1sgqK3l1AmpsKRdpfoEXjFWcOjJ_vAzgFCkE5VXINQ1TGYv3lnj-3kqsgjW5sCFMihvrmYMu__dqt1YmBrywC9MBz8_lJ7Q9YThxbTGwgrYEHX9tudaqMRyvnFjcNIAxLiZUUJY3FAVtNFdlIMmhoSQ7pQiwTKCsuuXvWyVlt13bi4gmpU2uB0Q43E9ihqYny9sspROxkcUmYG90jAb9AXkXKOqAgz";
        //    //var secureDataFormat = new TicketDataFormat(new MachineKeyProtector());
        //    //AuthenticationTicket ticket = secureDataFormat.Unprotect(accessToken);

        //    DataTable dt = new DataTable();
        //    List<PatientAPI> patients = new List<PatientAPI>();
        //    using (SqlConnection conn = new SqlConnection(constr))
        //    {
        //        SqlCommand sqlComm = new SqlCommand("usp_GetPatientsAll", conn);
        //        sqlComm.Parameters.Add(new SqlParameter("@ClinicCode", cliniccode));
        //        sqlComm.Parameters.Add(new SqlParameter("@PageNumber", pagenumber));
        //        sqlComm.Parameters.Add(new SqlParameter("@PageSize", pagesize));
        //        sqlComm.CommandType = CommandType.StoredProcedure;
        //            conn.Open();

        //            using (SqlDataReader sdr = sqlComm.ExecuteReader())
        //            {
        //                while (sdr.Read())
        //                {
        //                    PatientAPI pat = new PatientAPI();
        //                    pat.documentId = sdr["documentId"].ToString();
        //                    pat.clinicId = sdr["clinicId"].ToString();
        //                    pat.clinicCode = sdr["clinicCode"].ToString();
        //                    pat.search_text = sdr["search_text"].ToString();
        //                    pat.care_of = sdr["care_of"].ToString();
        //                    pat.patient_id = sdr["patient_id"].ToString();
        //                    pat.patient_name = sdr["patient_name"].ToString();
        //                    pat.gender = sdr["gender"].ToString();
        //                    pat.age = sdr["age"].ToString();
        //                    pat.patient_mobile_number = sdr["patient_mobile_number"].ToString();
        //                    pat.refer_by = sdr["refer_by"].ToString();
        //                    pat.disease = sdr["disease"].ToString();
        //                    pat.refer_to_doctor = sdr["refer_to_doctor"].ToString();
        //                    pat.id = sdr["id"].ToString();
        //                    pat.city = sdr["city"].ToString();
        //                    pat.severity = sdr["severity"].ToString();
        //                    //creation_date = sdr["creation_date"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(sdr["creation_date"]),
        //                    if (sdr["creation_date"] != DBNull.Value)
        //                    {
        //                        pat.creation_date = Convert.ToDateTime(sdr["creation_date"]);
        //                    }
        //                    //pat.creation_date = Convert.ToDateTime(sdr["creation_date"]);
        //                    //updatedAt = sdr["updatedAt"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(sdr["updatedAt"]),
        //                    if (sdr["updatedAt"] != DBNull.Value)
        //                    {
        //                        pat.updatedAt = Convert.ToDateTime(sdr["updatedAt"]);
        //                    }

        //                    //pat.updatedAt = Convert.ToDateTime(sdr["updatedAt"]);
        //                    pat.isCreated = Convert.ToInt32(sdr["isCreated"]);
        //                    pat.isSynced = Convert.ToInt32(sdr["isSynced"]);
        //                    pat.createdBy = sdr["createdBy"].ToString();
        //                    //loginAt = sdr["loginAt"].ToString(),
        //                    //loginAt = sdr["loginAt"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(sdr["loginAt"]),
        //                    if(sdr["loginAt"] != DBNull.Value)
        //                    {
        //                        pat.loginAt = Convert.ToDateTime(sdr["loginAt"]);
        //                    }
        //                    pat.patientAppDownloaded = Convert.ToInt32(sdr["patientAppDownloaded"]);
        //                    pat.dob = sdr["dob"].ToString();
        //                    //dob = string.IsNullOrEmpty(sdr["dob"].ToString()) ? (DateTime?)null : Convert.ToDateTime(sdr["dob"]),
        //                    //dob = sdr["dob"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(sdr["dob"]),
        //                    pat.PatientId = int.Parse(sdr["PatientId"].ToString());
        //                    patients.Add(pat);

        //                }
        //            }
        //            conn.Close();
        //     }

        //    return patients;

        //}
        #endregion

        [JwtAuthorize(Roles = "user")]
        [HttpGet]
        [Route("api/PatientAPI/GetPatients")]
        public GenericAPIResult GetPatients(string cliniccode, int pagenumber, int pagesize)
        {
            List<PatientAPI> patients = new List<PatientAPI>();
            GenericAPIResult result = new GenericAPIResult();

            var dynamicDt = new List<dynamic>();

            try
            {

                using (SqlConnection conn = new SqlConnection(constr))
                {
                    SqlCommand sqlComm = new SqlCommand("usp_GetPatientsAll", conn);
                    sqlComm.Parameters.Add(new SqlParameter("@ClinicCode", cliniccode));
                    sqlComm.Parameters.Add(new SqlParameter("@PageNumber", pagenumber));
                    sqlComm.Parameters.Add(new SqlParameter("@PageSize", pagesize));
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

        #region Commented Code GetPatient
        //[JwtAuthorize(Roles = "user")]
        //[HttpGet]
        //[Route("api/PatientAPI/GetPatient")]
        //public List<PatientAPI> GetPatientById(int id,string cliniccode)
        //{
        //    DataTable dt = new DataTable();
        //    List<PatientAPI> patients = new List<PatientAPI>();
        //    using (SqlConnection conn = new SqlConnection(constr))
        //    {
        //        SqlCommand sqlComm = new SqlCommand("usp_GetPatientById", conn);
        //        sqlComm.Parameters.Add(new SqlParameter("@PatientId", id));
        //        sqlComm.Parameters.Add(new SqlParameter("@ClinicCode", cliniccode));
        //        sqlComm.CommandType = CommandType.StoredProcedure;
        //        conn.Open();

        //        using (SqlDataReader sdr = sqlComm.ExecuteReader())
        //        {
        //            while (sdr.Read())
        //            {
        //                PatientAPI pat = new PatientAPI();
        //                pat.documentId = sdr["documentId"].ToString();
        //                pat.clinicId = sdr["clinicId"].ToString();
        //                pat.clinicCode = sdr["clinicCode"].ToString();
        //                pat.search_text = sdr["search_text"].ToString();
        //                pat.care_of = sdr["care_of"].ToString();
        //                pat.patient_id = sdr["patient_id"].ToString();
        //                pat.patient_name = sdr["patient_name"].ToString();
        //                pat.gender = sdr["gender"].ToString();
        //                pat.age = sdr["age"].ToString();
        //                pat.patient_mobile_number = sdr["patient_mobile_number"].ToString();
        //                pat.refer_by = sdr["refer_by"].ToString();
        //                pat.disease = sdr["disease"].ToString();
        //                pat.refer_to_doctor = sdr["refer_to_doctor"].ToString();
        //                pat.id = sdr["id"].ToString();
        //                pat.city = sdr["city"].ToString();
        //                pat.severity = sdr["severity"].ToString();
        //                //creation_date = sdr["creation_date"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(sdr["creation_date"]),
        //                if (sdr["creation_date"] != DBNull.Value)
        //                {
        //                    pat.creation_date = Convert.ToDateTime(sdr["creation_date"]);
        //                }
        //                //pat.creation_date = Convert.ToDateTime(sdr["creation_date"]);
        //                //updatedAt = sdr["updatedAt"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(sdr["updatedAt"]),
        //                if (sdr["updatedAt"] != DBNull.Value)
        //                {
        //                    pat.updatedAt = Convert.ToDateTime(sdr["updatedAt"]);
        //                }
        //                //pat.updatedAt = Convert.ToDateTime(sdr["updatedAt"]);
        //                pat.isCreated = Convert.ToInt32(sdr["isCreated"]);
        //                pat.isSynced = Convert.ToInt32(sdr["isSynced"]);
        //                pat.createdBy = sdr["createdBy"].ToString();
        //                //loginAt = sdr["loginAt"].ToString(),
        //                //loginAt = sdr["loginAt"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(sdr["loginAt"]),
        //                if (sdr["loginAt"] != DBNull.Value)
        //                {
        //                    pat.loginAt = Convert.ToDateTime(sdr["loginAt"]);
        //                }
        //                pat.patientAppDownloaded = Convert.ToInt32(sdr["patientAppDownloaded"]);
        //                pat.dob = sdr["dob"].ToString();
        //                //dob = string.IsNullOrEmpty(sdr["dob"].ToString()) ? (DateTime?)null : Convert.ToDateTime(sdr["dob"]),
        //                //dob = sdr["dob"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(sdr["dob"]),
        //                pat.PatientId = int.Parse(sdr["PatientId"].ToString());
        //                patients.Add(pat);
        //                //patients.Add(new PatientAPI
        //                //{
        //                //    documentId = sdr["documentId"].ToString(),
        //                //    clinicId = sdr["clinicId"].ToString(),
        //                //    clinicCode = sdr["clinicCode"].ToString(),
        //                //    search_text = sdr["search_text"].ToString(),
        //                //    care_of = sdr["care_of"].ToString(),
        //                //    patient_id = sdr["patient_id"].ToString(),
        //                //    patient_name = sdr["patient_name"].ToString(),
        //                //    gender = sdr["gender"].ToString(),
        //                //    age = sdr["age"].ToString(),
        //                //    patient_mobile_number = sdr["patient_mobile_number"].ToString(),
        //                //    refer_by = sdr["refer_by"].ToString(),
        //                //    disease = sdr["disease"].ToString(),
        //                //    refer_to_doctor = sdr["refer_to_doctor"].ToString(),
        //                //    id = sdr["id"].ToString(),
        //                //    city = sdr["city"].ToString(),
        //                //    severity = sdr["severity"].ToString(),
        //                //    creation_date = Convert.ToDateTime(sdr["creation_date"]),
        //                //    updatedAt = Convert.ToDateTime(sdr["updatedAt"]),
        //                //    isCreated = Convert.ToInt32(sdr["isCreated"]),
        //                //    isSynced = Convert.ToInt32(sdr["isSynced"]),
        //                //    createdBy = sdr["createdBy"].ToString(),
        //                //    //loginAt = sdr["loginAt"].ToString(),
        //                //    loginAt = Convert.ToDateTime(sdr["loginAt"]),
        //                //    patientAppDownloaded = Convert.ToInt32(sdr["patientAppDownloaded"]),
        //                //    dob = sdr["dob"].ToString(),
        //                //    //dob = string.IsNullOrEmpty(sdr["dob"].ToString()) ? (DateTime?)null : Convert.ToDateTime(sdr["dob"]),
        //                //    //dob = sdr["dob"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(sdr["dob"]),
        //                //    PatientId = int.Parse(sdr["PatientId"].ToString()),
        //                //});
        //            }
        //        }
        //        conn.Close();
        //    }

        //    return patients;

        //}

        #endregion

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

        #region commented code CreatePatient
        //[JwtAuthorize(Roles = "user")]
        //[HttpPost]
        //[Route("api/PatientAPI/CreatePatient")]
        //public string CreatePatient([FromBody] PatientAPI ObjPatient)
        //{
        //    string msg = "";
        //    int maxId = 0;

        //    if (ObjPatient is null)
        //    {
        //        msg = "Patient Data is Blank";
        //    }
        //    else if (ObjPatient.PatientId != 0)
        //    {
        //        msg = "PatientId is not Blank";

        //    }
        //    else if (ObjPatient.clinicCode == "" || ObjPatient.clinicCode is null)
        //    {

        //        msg = "Clinic Code is Blank";

        //    }
        //    else if(ObjPatient.patient_name == "" || ObjPatient.patient_name is null)
        //    {
        //        msg = "Patient Name is Blank";

        //    }
        //    else if (ObjPatient.updatedAt.ToString() == "01-01-0001 00:00:00")
        //    {
        //        msg = "updatedAt is invalid";

        //    }
        //    else if (ObjPatient.creation_date.ToString() == "01-01-0001 00:00:00")
        //    {
        //        msg = "creation_date is invalid";

        //    }
        //    else if (ObjPatient.loginAt.ToString() == "01-01-0001 00:00:00")
        //    {
        //        msg = "loginAt is invalid";

        //    }
        //    else if (ObjPatient.clinicCode != "")
        //    {
        //        using (SqlConnection conn = new SqlConnection(constr))
        //        {
        //            DataTable dtCC = new DataTable();
        //            SqlCommand sqlCommCC = new SqlCommand("Select * from clinics where clinic_code = '" + ObjPatient.clinicCode + "'", conn);
        //            sqlCommCC.CommandType = CommandType.Text;
        //            conn.Open();

        //            dtCC.Load(sqlCommCC.ExecuteReader());
        //            conn.Close();
        //            if (dtCC.Rows.Count == 0)
        //            {
        //                msg = "Clinic Code " + ObjPatient.clinicCode + " does not exists.";
        //            }
        //            else
        //            {
        //                try
        //                {


        //                    DataTable dt = new DataTable();
        //                    SqlCommand sqlComm = new SqlCommand("Select Max(PatientId) as Id,Max(patient_id) as patient_id from patients where cliniccode = '" + ObjPatient.clinicCode + "'", conn);
        //                    sqlComm.CommandType = CommandType.Text;
        //                    conn.Open();

        //                    dt.Load(sqlComm.ExecuteReader());
        //                    conn.Close();

        //                    maxId = Convert.ToInt32(dt.Rows[0]["Id"].ToString()) + 1;
        //                    string patient_id = dt.Rows[0]["patient_id"].ToString();

        //                    patient_id = patient_id.Substring(1, 4);

        //                    int numericpatient_id = Convert.ToInt32(patient_id) + 1;

        //                    int len = numericpatient_id.ToString().Length;
        //                    if (len == 1)
        //                    { patient_id = "A000" + Convert.ToString(numericpatient_id); }
        //                    if (len == 2)
        //                    { patient_id = "A00" + Convert.ToString(numericpatient_id); }
        //                    if (len == 3)
        //                    { patient_id = "A0" + Convert.ToString(numericpatient_id); }
        //                    if (len == 4)
        //                    { patient_id = "A" + Convert.ToString(numericpatient_id); }





        //                    SqlCommand sqlCommPatientInsert = new SqlCommand("Insert into patients( " +
        //                    " PatientId, documentId, clinicId, clinicCode, search_text, care_of, patient_id, " +
        //                    "patient_name, gender, age, patient_mobile_number, refer_by, disease, refer_to_doctor," +
        //                    " id, city, severity, creation_date, updatedAt, isCreated, isSynced, createdBy, " +
        //                    "loginAt, patientAppDownloaded, dob" +
        //                    " ) values('" + maxId + "','" + ObjPatient.documentId + "','" + ObjPatient.clinicId
        //                    + "','" + ObjPatient.clinicCode + "','" + ObjPatient.search_text + "','" + ObjPatient.care_of
        //                    + "','" + patient_id + "','" + ObjPatient.patient_name + "','" + ObjPatient.gender
        //                    + "','" + ObjPatient.age + "','" + ObjPatient.patient_mobile_number + "','" + ObjPatient.refer_by
        //                    + "','" + ObjPatient.disease + "','" + ObjPatient.refer_to_doctor + "','" + ObjPatient.id +
        //                    "','" + ObjPatient.city + "','" + ObjPatient.severity + "','" + DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss") + "'" +
        //                    ",'" + DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss") + "','" + ObjPatient.isCreated + "','" + ObjPatient.isSynced +
        //                    "','" + ObjPatient.createdBy + "','" + ObjPatient.loginAt.ToString("dd-MMM-yyyy HH:mm:ss") + "','" + ObjPatient.patientAppDownloaded +
        //                    "','" + ObjPatient.dob + "')", conn);
        //                    sqlCommPatientInsert.CommandType = CommandType.Text;
        //                    conn.Open();

        //                    sqlCommPatientInsert.ExecuteNonQuery();

        //                    msg = "Patient Successfully Created";

        //                }
        //                catch (Exception ex)
        //                {
        //                    msg = "Error:" + ex.Message;

        //                }
        //                finally
        //                {
        //                    conn.Close();
        //                }
        //            }

        //        }


        //    }

        //    return JsonConvert.SerializeObject(msg);
        //    //  return msg;
        //}
        #endregion

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
                            SqlCommand sqlCommPatDuplicate = new SqlCommand("Select * from patients where patient_mobile_number = '" + ObjPatient.patient_mobile_number + "' and patient_name = '" + ObjPatient.patient_name + "'", conn);
                            sqlCommPatDuplicate.CommandType = CommandType.Text;
                            conn.Open();

                            dtPatDuplicate.Load(sqlCommPatDuplicate.ExecuteReader());
                            conn.Close();
                            if (dtPatDuplicate.Rows.Count > 0)
                            {
                                msg = "Duplicate Patient, Patient Name with Mobile number already exists ";
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



                                SqlCommand sqlCommPatientInsert = new SqlCommand("Insert into patients( " +
                                " PatientId, documentId, clinicId, clinicCode, search_text, care_of, patient_id, " +
                                "patient_name, gender, age, patient_mobile_number, refer_by, disease, refer_to_doctor," +
                                " id, city, severity, creation_date, updatedAt, isCreated, isSynced, createdBy, " +
                                "loginAt, patientAppDownloaded, dob" +
                                " ) values('" + maxId + "','" + ObjPatient.documentId + "','" + ObjPatient.clinicId
                                + "','" + ObjPatient.clinicCode + "','" + ObjPatient.search_text + "','" + ObjPatient.care_of
                                + "','" + patient_id + "','" + ObjPatient.patient_name + "','" + ObjPatient.gender
                                + "','" + ObjPatient.age + "','" + ObjPatient.patient_mobile_number + "','" + ObjPatient.refer_by
                                + "','" + ObjPatient.disease + "','" + ObjPatient.refer_to_doctor + "','" + ObjPatient.id +
                                "','" + ObjPatient.city + "','" + ObjPatient.severity + "','" + DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss") + "'" +
                                ",'" + DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss") + "','" + ObjPatient.isCreated + "','" + ObjPatient.isSynced +
                                "','" + ObjPatient.createdBy + "','" + ObjPatient.loginAt.ToString("dd-MMM-yyyy HH:mm:ss") + "','" + ObjPatient.patientAppDownloaded +
                                "','" + ObjPatient.dob + "')", conn);
                                sqlCommPatientInsert.CommandType = CommandType.Text;
                                conn.Open();

                                sqlCommPatientInsert.ExecuteNonQuery();

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
                Query Qref = db.Collection("clinics").WhereEqualTo("clinic_code", ObjPatient.clinicCode).Limit(1);
                QuerySnapshot snapClinic = await Qref.GetSnapshotAsync();

                if (snapClinic.Count > 0)
                {
                    DocumentSnapshot docSnapClinic = snapClinic.Documents[0];
                    Clinic clinic = docSnapClinic.ConvertTo<Clinic>();

                    CollectionReference col1 = db.Collection("clinics").Document(docSnapClinic.Id).Collection("WebAPIResponse");

                    Dictionary<string, object> data1 = new Dictionary<string, object>
                    {
                        {"CollectionName" ,"Patient" },
                        {"UpdatedAt" ,DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc)},
                    };

                    await col1.Document().SetAsync(data1);
                }

                

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
        [Route("api/PatientAPI/CreatePatient12345")]
        public GenericAPIResult CreatePatient12345([FromBody] PatientAPI ObjPatient)
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
                msg = "loginAt is invalid";
                statuscode = "201";
                errorcode = "true";
            }


            #region Code to update Firebase Listener

            string Path = AppDomain.CurrentDomain.BaseDirectory + @"greenpaperdev-firebase-adminsdk-8k2y5-fb46e63414.json";
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path);
            FirestoreDb db = FirestoreDb.Create("greenpaperdev");


            try
            {
                Query Qref = db.Collection("clinics").WhereEqualTo("clinic_code", ObjPatient.clinicCode).Limit(1);
                QuerySnapshot snapClinic = Qref.GetSnapshotAsync().Result;

                if (snapClinic.Count > 0)
                {
                    DocumentSnapshot docsnapClinic = snapClinic.Documents[0];

                    Clinic clinic = docsnapClinic.ConvertTo<Clinic>();

                    CollectionReference col1 = db.Collection("clinics").Document(docsnapClinic.Id).Collection("WebAPIResponse");

                    Dictionary<string, object> data1 = new Dictionary<string, object>
                    {
                        {"CollectionName" ,"Patient" },
                        {"UpdatedAt" ,DateTime.Now},
                    };

                    col1.Document().SetAsync(data1);


                    // TODO: Add insert logic here
                    //var result = await fireBaseClient.Child("Students").PostAsync(std);
                }



            }
            catch
            {

            }

            #endregion

            result.message = msg;
            result.statusCode = statuscode;
            result.error = errorcode;
            result.data = dynamicDt;

            return result;
            //  return msg;
        }

        #region Commented UpdatePatient
        //[JwtAuthorize(Roles = "user")]
        //[HttpPost]
        //[Route("api/PatientAPI/UpdatePatient")]
        //public string UpdatePatient([FromBody] PatientAPI ObjPatient)
        //{
        //    string msg = "";
        //    int maxId = 0;

        //    if (ObjPatient is null)
        //    {
        //        msg = "Patient Data is Blank";
        //    }
        //    else if (ObjPatient.PatientId == 0)
        //    {
        //        msg = "PatientId is Blank";
        //    }
        //    else if (ObjPatient.clinicCode == "" || ObjPatient.clinicCode is null)
        //    {
        //        msg = "Clinic Code is Blank";
        //    }
        //    else if (ObjPatient.patient_name == "" || ObjPatient.patient_name is null)
        //    {
        //        msg = "Patient Name is Blank";

        //    }
        //    else if (ObjPatient.loginAt.ToString() == "01-01-0001 00:00:00")
        //    {
        //        msg = "loginAt is invalid";

        //    }
        //    else if (ObjPatient.clinicCode != "")
        //    {
        //        using (SqlConnection conn = new SqlConnection(constr))
        //        {
        //            DataTable dtCC = new DataTable();
        //            SqlCommand sqlComm = new SqlCommand("Select * from clinics where clinic_code = '" + ObjPatient.clinicCode + "'", conn);
        //            sqlComm.CommandType = CommandType.Text;
        //            conn.Open();

        //            dtCC.Load(sqlComm.ExecuteReader());
        //            conn.Close();
        //            if (dtCC.Rows.Count == 0)
        //            {
        //                msg = "Clinic Code " + ObjPatient.clinicCode + " does not exists.";
        //            }
        //            else
        //            {
        //                try
        //                {

        //                    SqlCommand sqlCommPatientUpdate = new SqlCommand("Update patients set search_text = '" + ObjPatient.search_text + "', care_of = '" + ObjPatient.care_of + "'," +
        //                   "patient_name = '" + ObjPatient.patient_name + "', gender = '" + ObjPatient.gender + "', age = '" + ObjPatient.age + "'," +
        //                   " patient_mobile_number = '" + ObjPatient.patient_mobile_number + "', refer_by = '" + ObjPatient.refer_by + "', disease = '" + ObjPatient.disease + "', refer_to_doctor = '" + ObjPatient.refer_to_doctor + "'," +
        //                   " city = '" + ObjPatient.city + "', severity = '" + ObjPatient.severity + "', updatedAt = '" + DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss") + "', isCreated = '" + ObjPatient.isCreated + "', isSynced = '" + ObjPatient.isSynced + "', createdBy = '" + ObjPatient.createdBy + "', " +
        //                   //"loginAt = '" + Convert.ToDateTime(ObjPatient.loginAt) + "', patientAppDownloaded = '" + ObjPatient.patientAppDownloaded + "', dob = '" + Convert.ToDateTime(ObjPatient.dob) + "'", conn);
        //                   "loginAt = '" + ObjPatient.loginAt.ToString("dd-MMM-yyyy HH:mm:ss") + "', patientAppDownloaded = '" + ObjPatient.patientAppDownloaded + "' where PatientId = '" + ObjPatient.PatientId + "' and Cliniccode = '" + ObjPatient.clinicCode + "'", conn);
        //                    sqlCommPatientUpdate.CommandType = CommandType.Text;
        //                    conn.Open();

        //                    sqlCommPatientUpdate.ExecuteNonQuery();

        //                    msg = "Patient Successfully Updated";

        //                }
        //                catch (Exception ex)
        //                {
        //                    msg = "Error:" + ex.Message;

        //                }
        //                finally
        //                {
        //                    conn.Close();
        //                }
        //            }
        //        }
        //    }


        //    return JsonConvert.SerializeObject(msg);
        //    //  return msg;
        //}

        #endregion

        [JwtAuthorize(Roles = "user")]
        [HttpPost]
        [Route("api/PatientAPI/UpdatePatient")]
        public GenericAPIResult UpdatePatient([FromBody] PatientAPI ObjPatient)
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
                                SqlCommand sqlCommPatientUpdate = new SqlCommand("Update patients set search_text = '" + ObjPatient.search_text + "', care_of = '" + ObjPatient.care_of + "'," +
                                    "patient_name = '" + ObjPatient.patient_name + "', gender = '" + ObjPatient.gender + "', age = '" + ObjPatient.age + "'," +
                                    " patient_mobile_number = '" + ObjPatient.patient_mobile_number + "', refer_by = '" + ObjPatient.refer_by + "', disease = '" + ObjPatient.disease + "', refer_to_doctor = '" + ObjPatient.refer_to_doctor + "'," +
                                    " city = '" + ObjPatient.city + "', severity = '" + ObjPatient.severity + "', updatedAt = '" + DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss") + "', isCreated = '" + ObjPatient.isCreated + "', isSynced = '" + ObjPatient.isSynced + "', createdBy = '" + ObjPatient.createdBy + "', " +
                                    //"loginAt = '" + Convert.ToDateTime(ObjPatient.loginAt) + "', patientAppDownloaded = '" + ObjPatient.patientAppDownloaded + "', dob = '" + Convert.ToDateTime(ObjPatient.dob) + "'", conn);
                                    "loginAt = '" + ObjPatient.loginAt.ToString("dd-MMM-yyyy HH:mm:ss") + "', patientAppDownloaded = '" + ObjPatient.patientAppDownloaded + "' where PatientId = '" + ObjPatient.PatientId + "' and Cliniccode = '" + ObjPatient.clinicCode + "'", conn);
                                    sqlCommPatientUpdate.CommandType = CommandType.Text;
                                conn.Open();

                                sqlCommPatientUpdate.ExecuteNonQuery();

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
