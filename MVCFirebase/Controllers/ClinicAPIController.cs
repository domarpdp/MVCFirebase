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
        static DateTime utcTime = DateTime.UtcNow;
        static TimeZoneInfo istZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime istTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, istZone);

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
            else if (Obj.clinicname == "")
            {
                msg = "clinicname is Blank";
                statuscode = "201";
                errorcode = "true";

            }
            else if (Obj.clinicaddress == "")
            {
                msg = "clinicaddress is Blank";
                statuscode = "201";
                errorcode = "true";

            }
            else if (Obj.clinicstate == "")
            {
                msg = "clinicstate is Blank";
                statuscode = "201";
                errorcode = "true";

            }
            else if (Obj.cliniccity == "")
            {
                msg = "cliniccity is Blank";
                statuscode = "201";
                errorcode = "true";

            }
            else if (Obj.clinicemail == "")
            {
                msg = "clinicemail is Blank";
                statuscode = "201";
                errorcode = "true";

            }
            else if (Obj.clinicwebsite == "")
            {
                msg = "clinicwebsite is Blank";
                statuscode = "201";
                errorcode = "true";

            }
            else if (Obj.clinicmobilenumber == "")
            {

                msg = "clinicmobilenumber is Blank";
                statuscode = "201";
                errorcode = "true";

            }
            else if (Obj.selected_plan == "")
            {

                msg = "selected_plan is Blank";
                statuscode = "201";
                errorcode = "true";

            }
            else
            {
                using (SqlConnection conn = new SqlConnection(constr))
                {
                    try
                    {

                        string generatedClinicCode = "";
                        DataTable dt = new DataTable();
                        SqlCommand sqlComm = new SqlCommand("Select Max(clinic_code) as cliniccode from Clinics", conn);
                        sqlComm.CommandType = CommandType.Text;
                        conn.Open();

                        dt.Load(sqlComm.ExecuteReader());
                        conn.Close();


                        string cc = dt.Rows[0]["cliniccode"].ToString();

                        cc = cc.Substring(3);

                        int numericcc = Convert.ToInt32(cc.Trim()) + 1;

                        generatedClinicCode = "GP-" + numericcc.ToString();


                        DataTable dtId = new DataTable();
                        SqlCommand sqlCommId = new SqlCommand("Select Max(isnull(Id,0)) as Id from clinics", conn);
                        sqlCommId.CommandType = CommandType.Text;
                        conn.Open();

                        dtId.Load(sqlCommId.ExecuteReader());
                        conn.Close();

                        maxId = Convert.ToInt32(dtId.Rows[0]["Id"].ToString()) + 1;

                        using (SqlCommand sqlCommPatientInsert = new SqlCommand(
                        " Insert into clinics (" +
                        " Id, documentId, clinicname, clinicaddress, clinicsector, clinicstate, cliniccity," +
                        " clinicmobilenumber, mobilenumber, name, email, idproof, clinicemail, clinicwebsite, clinicstreet, " +
                        "clinicpincode, clinicadvertisement,selectidproofimage, userId, selected_plan, logo, created_on, " +
                        "subscription_start_date, subscription_end_date, clinic_code, registerd_by_number, free_trail_available, " +
                        "free_sms_available, clinic_info_completed, is_using_free_trial, free_trial_taken_date " +
                        " ) values (" +
                        "@Id, @DocumentId, @clinicname, @clinicaddress, @clinicsector, @clinicstate, @cliniccity" +
                        ", @clinicmobilenumber, @mobilenumber, @name,  @email, @idproof, @clinicemail, @clinicwebsite,@clinicstreet," +
                        " @clinicpincode, @clinicadvertisement, @selectidproofimage, @userId, @selected_plan, @logo,@created_on, " +
                        "@subscription_start_date,@subscription_end_date,@clinic_code,@registerd_by_number,@free_trail_available," +
                        "@free_sms_available, @clinic_info_completed, @is_using_free_trial, @free_trial_taken_date)", conn))
                        {
                            sqlCommPatientInsert.CommandType = CommandType.Text;
                            sqlCommPatientInsert.Parameters.AddWithValue("@Id", maxId);
                            sqlCommPatientInsert.Parameters.AddWithValue("@DocumentId", Obj.documentId ?? (object)DBNull.Value);
                            sqlCommPatientInsert.Parameters.AddWithValue("@clinicname", Obj.clinicname ?? (object)DBNull.Value);
                            sqlCommPatientInsert.Parameters.AddWithValue("@clinicaddress", Obj.clinicaddress ?? (object)DBNull.Value);
                            sqlCommPatientInsert.Parameters.AddWithValue("@clinicsector", Obj.clinicsector ?? (object)DBNull.Value);
                            sqlCommPatientInsert.Parameters.AddWithValue("@clinicstate", Obj.clinicstate ?? (object)DBNull.Value);
                            sqlCommPatientInsert.Parameters.AddWithValue("@cliniccity", Obj.cliniccity);
                            sqlCommPatientInsert.Parameters.AddWithValue("@clinicmobilenumber", Obj.clinicmobilenumber ?? (object)DBNull.Value);
                            sqlCommPatientInsert.Parameters.AddWithValue("@mobilenumber", Obj.mobilenumber ?? (object)DBNull.Value);
                            sqlCommPatientInsert.Parameters.AddWithValue("@name", Obj.name ?? (object)DBNull.Value);
                            sqlCommPatientInsert.Parameters.AddWithValue("@email", Obj.email ?? (object)DBNull.Value);
                            sqlCommPatientInsert.Parameters.AddWithValue("@idproof", Obj.idproof ?? (object)DBNull.Value);
                            sqlCommPatientInsert.Parameters.AddWithValue("@clinicemail", Obj.clinicemail ?? (object)DBNull.Value);
                            sqlCommPatientInsert.Parameters.AddWithValue("@clinicwebsite", Obj.clinicwebsite ?? (object)DBNull.Value);
                            sqlCommPatientInsert.Parameters.AddWithValue("@clinicstreet", Obj.clinicstreet ?? (object)DBNull.Value);
                            sqlCommPatientInsert.Parameters.AddWithValue("@clinicpincode", Obj.clinicpincode ?? (object)DBNull.Value);
                            sqlCommPatientInsert.Parameters.AddWithValue("@clinicadvertisement", Obj.clinicadvertisement ?? (object)DBNull.Value);
                            sqlCommPatientInsert.Parameters.AddWithValue("@selectidproofimage", Obj.selectidproofimage ?? (object)DBNull.Value);
                            sqlCommPatientInsert.Parameters.AddWithValue("@userId", Obj.userId ?? (object)DBNull.Value);
                            sqlCommPatientInsert.Parameters.AddWithValue("@selected_plan", Obj.selected_plan ?? (object)DBNull.Value);
                            sqlCommPatientInsert.Parameters.AddWithValue("@logo", Obj.logo ?? (object)DBNull.Value);
                            if (Obj.created_on is null || Obj.created_on.ToString() == "")
                            {
                                sqlCommPatientInsert.Parameters.AddWithValue("@created_on", istTime);
                            }
                            else
                            {
                                sqlCommPatientInsert.Parameters.AddWithValue("@created_on", Obj.created_on);
                            }
                            
                            sqlCommPatientInsert.Parameters.AddWithValue("@subscription_start_date", Obj.subscription_start_date ?? (object)DBNull.Value);
                            sqlCommPatientInsert.Parameters.AddWithValue("@subscription_end_date", Obj.subscription_end_date ?? (object)DBNull.Value);
                            if (Obj.clinic_code == "" || Obj.clinic_code is null) 
                            {
                                sqlCommPatientInsert.Parameters.AddWithValue("@clinic_code", generatedClinicCode);
                            }
                            else
                            {
                                sqlCommPatientInsert.Parameters.AddWithValue("@clinic_code", Obj.clinic_code ?? (object)DBNull.Value);
                            }
                            
                            sqlCommPatientInsert.Parameters.AddWithValue("@registerd_by_number", Obj.registerd_by_number ?? (object)DBNull.Value);
                            sqlCommPatientInsert.Parameters.AddWithValue("@free_trail_available", Obj.free_trail_available ?? (object)DBNull.Value);
                            sqlCommPatientInsert.Parameters.AddWithValue("@free_sms_available", Obj.free_sms_available ?? (object)DBNull.Value);
                            sqlCommPatientInsert.Parameters.AddWithValue("@clinic_info_completed", Obj.clinic_info_completed ?? (object)DBNull.Value);
                            sqlCommPatientInsert.Parameters.AddWithValue("@is_using_free_trial", Obj.is_using_free_trial ?? (object)DBNull.Value);
                            sqlCommPatientInsert.Parameters.AddWithValue("@free_trial_taken_date", Obj.free_trial_taken_date ?? (object)DBNull.Value);


                            conn.Open();
                            sqlCommPatientInsert.ExecuteNonQuery();
                        }


                        msg = "Clinic Successfully Created";


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

            result.message = msg;
            result.statusCode = statuscode;
            result.error = errorcode;
            result.data = dynamicDt;
            return result; ;

        }

    }
}