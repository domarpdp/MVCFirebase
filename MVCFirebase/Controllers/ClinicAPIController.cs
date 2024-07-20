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
using System.Web.Http;
using System.Web.Http.Results;

namespace MVCFirebase.Controllers
{
    public class ClinicAPIController : ApiController
    {
        string constr = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        [JwtAuthorize(Roles = "user")]
        [HttpGet]
        [Route("api/ClinicAPI/GetClinics")]
        public GenericAPIResult GetClinics(int pagenumber, int pagesize)
        {
            List<PatientAPI> patients = new List<PatientAPI>();
            GenericAPIResult result = new GenericAPIResult();

            var dynamicDt = new List<dynamic>();

            try
            {

                using (SqlConnection conn = new SqlConnection(constr))
                {
                    SqlCommand sqlComm = new SqlCommand("usp_GetClinicsAll", conn);
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
                                dictionary.Add(sdr.GetName(i), sdr.GetValue(i));
                            }

                            dynamicDt.Add(row);
                        }

                    }
                    conn.Close();
                }
                result.message = "Clinic List fetched Successfully";
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
        [Route("api/ClinicAPI/GetClinic")]
        public GenericAPIResult GetClinicByCliniccode(string cliniccode)
        {
            //DataTable dt = new DataTable();
            //List<PatientAPI> patients = new List<PatientAPI>();

            GenericAPIResult result = new GenericAPIResult();

            var dynamicDt = new List<dynamic>();

            try
            {
                using (SqlConnection conn = new SqlConnection(constr))
                {
                    SqlCommand sqlComm = new SqlCommand("usp_GetClinicByCliniccode", conn);
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
                                if (sdr.GetName(i) == "free_trail_available" || sdr.GetName(i) == "free_sms_available" || sdr.GetName(i) == "clinic_info_completed" || sdr.GetName(i) == "is_using_free_trial")
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

                result.message = "Clinic fetched Successfully";
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
        [Route("api/ClinicAPI/CreateClinic")]
        public GenericAPIResult CreateClinic([FromBody] ClinicAPI Obj)
        {
            GenericAPIResult result = new GenericAPIResult();
            var dynamicDt = new List<dynamic>();
            string errorcode = "";
            string statuscode = "";


            string msg = "";

            int maxId = 0;

            if (Obj is null)
            {
                msg = "Clinic Data is Blank";
                statuscode = "201";
                errorcode = "true";
            }
            //else if (Obj.PatientId != 0)
            //{
            //    msg = "PatientId is not Blank";
            //    statuscode = "201";
            //    errorcode = "true";

            //}
            //else if (ObjPatient.clinicCode == "" || ObjPatient.clinicCode is null)
            //{

            //    msg = "Clinic Code is Blank";
            //    statuscode = "201";
            //    errorcode = "true";

            //}
            //else if (ObjPatient.patient_name == "" || ObjPatient.patient_name is null)
            //{
            //    msg = "Patient Name is Blank";
            //    statuscode = "201";
            //    errorcode = "true";

            //}
            //else if (ObjPatient.updatedAt.ToString() == "01-01-0001 00:00:00")
            //{
            //    msg = "updatedAt is invalid";
            //    statuscode = "201";
            //    errorcode = "true";

            //}
            //else if (ObjPatient.creation_date.ToString() == "01-01-0001 00:00:00")
            //{
            //    msg = "creation_date is invalid";
            //    statuscode = "201";
            //    errorcode = "true";

            //}
            //else if (ObjPatient.loginAt.ToString() == "01-01-0001 00:00:00")
            //{
            //    msg = "loginAt is invalid";
            //    statuscode = "201";
            //    errorcode = "true";

            //}
            //else if (ObjPatient.clinicCode != "")
            //{
            //    using (SqlConnection conn = new SqlConnection(constr))
            //    {
            //        DataTable dtCC = new DataTable();
            //        SqlCommand sqlCommCC = new SqlCommand("Select * from clinics where clinic_code = '" + ObjPatient.clinicCode + "'", conn);
            //        sqlCommCC.CommandType = CommandType.Text;
            //        conn.Open();

            //        dtCC.Load(sqlCommCC.ExecuteReader());
            //        conn.Close();
            //        if (dtCC.Rows.Count == 0)
            //        {
            //            msg = "Clinic Code " + ObjPatient.clinicCode + " does not exists.";
            //            statuscode = "201";
            //            errorcode = "true";
            //        }
            //        else
            //        {
            //            try
            //            {
            //                DataTable dtPatDuplicate = new DataTable();
            //                SqlCommand sqlCommPatDuplicate = new SqlCommand("Select * from patients where patient_mobile_number = '" + ObjPatient.patient_mobile_number + "' and patient_name = '" + ObjPatient.patient_name + "'", conn);
            //                sqlCommPatDuplicate.CommandType = CommandType.Text;
            //                conn.Open();

            //                dtPatDuplicate.Load(sqlCommPatDuplicate.ExecuteReader());
            //                conn.Close();
            //                if (dtPatDuplicate.Rows.Count > 0)
            //                {
            //                    msg = "Duplicate Patient, Patient Name with Mobile number already exists ";
            //                }
            //                else
            //                {
            //                    DataTable dt = new DataTable();
            //                    SqlCommand sqlComm = new SqlCommand("Select Max(patient_id) as patient_id from patients where cliniccode = '" + ObjPatient.clinicCode + "'", conn);
            //                    sqlComm.CommandType = CommandType.Text;
            //                    conn.Open();

            //                    dt.Load(sqlComm.ExecuteReader());
            //                    conn.Close();


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


            //                    DataTable dtId = new DataTable();
            //                    SqlCommand sqlCommId = new SqlCommand("Select Max(PatientId) as Id from patients", conn);
            //                    sqlCommId.CommandType = CommandType.Text;
            //                    conn.Open();

            //                    dtId.Load(sqlCommId.ExecuteReader());
            //                    conn.Close();

            //                    maxId = Convert.ToInt32(dtId.Rows[0]["Id"].ToString()) + 1;



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


            //                    statuscode = "200";
            //                    errorcode = "false";
            //                }



            //            }
            //            catch (Exception ex)
            //            {
            //                msg = ex.Message;
            //                statuscode = "201";
            //                errorcode = "true";

            //            }
            //            finally
            //            {
            //                conn.Close();
            //            }
            //        }

            //    }


            //}

            //result.message = msg;
            //result.statusCode = statuscode;
            //result.error = errorcode;
            //result.data = dynamicDt;

            return result; ;

        }

    }
}