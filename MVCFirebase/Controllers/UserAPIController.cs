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
    public class UserAPIController : ApiController
    {
        string constr = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        [JwtAuthorize(Roles = "user")]
        [HttpGet]
        [Route("api/UserAPI/GetUsers")]
        public GenericAPIResult GetUsers(string cliniccode, int pagenumber, int pagesize)
        {

            List<UserAPI> patients = new List<UserAPI>();
            GenericAPIResult result = new GenericAPIResult();

            var dynamicDt = new List<dynamic>();

            try
            {

                using (SqlConnection conn = new SqlConnection(constr))
                {
                    SqlCommand sqlComm = new SqlCommand("usp_GetUsersAll", conn);
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
                                if (sdr.GetName(i) == "user_roles")
                                {
                                    string userRolesString = sdr.GetValue(i).ToString();
                                    dynamic row1 = new ExpandoObject();
                                    var dictionary1 = (IDictionary<string, object>)row1;

                                    var userRoles = JsonConvert.DeserializeObject<List<string>>(userRolesString);

                                    for (int j = 0; j < userRoles.Count; j++)
                                    {
                                        dictionary1.Add(j.ToString(), userRoles[j]);
                                    }

                                    dictionary.Add("user_roles", dictionary1);

                                }
                                else if (sdr.GetName(i) == "stats_enable" || sdr.GetName(i) == "user_deactivated")
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

                result.message = "User List fetched Successfully";
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
        [Route("api/UserAPI/GetUserBySearchString")]
        public GenericAPIResult GetUserBySearchString(string cliniccode, string searchstring)
        {

            List<UserAPI> patients = new List<UserAPI>();
            GenericAPIResult result = new GenericAPIResult();

            var dynamicDt = new List<dynamic>();

            try
            {

                using (SqlConnection conn = new SqlConnection(constr))
                {
                    SqlCommand sqlComm = new SqlCommand("usp_GetUserBySearchString", conn);
                    sqlComm.Parameters.Add(new SqlParameter("@ClinicCode", cliniccode));
                    sqlComm.Parameters.Add(new SqlParameter("@SearchString", searchstring));


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
                                if (sdr.GetName(i) == "user_roles")
                                {
                                    string userRolesString = sdr.GetValue(i).ToString();
                                    dynamic row1 = new ExpandoObject();
                                    var dictionary1 = (IDictionary<string, object>)row1;

                                    var userRoles = JsonConvert.DeserializeObject<List<string>>(userRolesString);

                                    for (int j = 0; j < userRoles.Count; j++)
                                    {
                                        dictionary1.Add(j.ToString(), userRoles[j]);
                                    }

                                    dictionary.Add("user_roles", dictionary1);

                                }
                                else if (sdr.GetName(i) == "stats_enable" || sdr.GetName(i) == "user_deactivated")
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
                if (dynamicDt.Count() > 0)
                {
                    result.message = "User is fetched Successfully";
                    result.statusCode = "200";
                    result.error = "false";
                    result.data = dynamicDt;
                }
                else
                {
                    result.message = "No Such User exists";
                    result.statusCode = "200";
                    result.error = "false";
                    result.data = dynamicDt;
                }

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
        [Route("api/UserAPI/CreateUser")]
        public GenericAPIResult CreateUser([FromBody] UserAPI Obj)
        {
            GenericAPIResult result = new GenericAPIResult();
            var dynamicDt = new List<dynamic>();
            string errorcode = "";
            string statuscode = "";


            string msg = "";

            int maxId = 0;

            if (Obj is null)
            {
                msg = "User Data is Blank";
                statuscode = "201";
                errorcode = "true";
            }
            else if (Obj.clinicCode == "" || Obj.clinicCode is null)
            {

                msg = "Clinic Code is Blank";
                statuscode = "201";
                errorcode = "true";

            }
            else if (Obj.name == "" || Obj.name is null)
            {
                msg = "Name is Blank";
                statuscode = "201";
                errorcode = "true";

            }
            else if (Obj.mobile_number == "" || Obj.mobile_number is null)
            {
                msg = "Mobile Number is Blank";
                statuscode = "201";
                errorcode = "true";

            }
            else if (Obj.user_roles.Count == 0)
            {
                msg = "User Roles are Blank";
                statuscode = "201";
                errorcode = "true";

            }
            else if (Obj.user_qualification == "" || Obj.user_qualification is null)
            {
                msg = "User Qualification is Blank";
                statuscode = "201";
                errorcode = "true";

            }
            else 
            {
                string userRolesJsonArrayString = Obj.GetUserRolesAsJsonArrayString();
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
                        try
                        {
                            DataTable dtPatDuplicate = new DataTable();
                            SqlCommand sqlCommPatDuplicate = new SqlCommand("Select * from [user] where mobile_number = '" + Obj.mobile_number + "' and cliniccode = '" + Obj.clinicCode + "'", conn);
                            sqlCommPatDuplicate.CommandType = CommandType.Text;
                            conn.Open();

                            dtPatDuplicate.Load(sqlCommPatDuplicate.ExecuteReader());
                            conn.Close();
                            if (dtPatDuplicate.Rows.Count > 0)
                            {
                                msg = "Duplicate User, User with Mobile number "+Obj.mobile_number+" already exists ";
                            }
                            else
                            {
                                using (SqlCommand sqlCommPatientInsert = new SqlCommand(
                                            "INSERT INTO [user] (" +
                                            "documentId, clinicId, clinicCode, email, name, password, mobile_number, user_roles, user_qualification, idproof, signature, stats_enable, creation_date, user_deactivated" +
                                            ") VALUES (" +
                                            "@documentId, @clinicId, @clinicCode, @email, @name, @password, @mobile_number, @user_roles, @user_qualification, @idproof, @signature, " +
                                            "@stats_enable, @creation_date, @user_deactivated)", conn))
                                {
                                    sqlCommPatientInsert.CommandType = CommandType.Text;

                                    sqlCommPatientInsert.Parameters.AddWithValue("@documentId", Obj.documentId ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@clinicId", Obj.clinicid ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@clinicCode", Obj.clinicCode ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@email", Obj.email ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@name", Obj.name ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@password", Obj.password ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@mobile_number", Obj.mobile_number ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@user_roles", Obj.GetUserRolesAsJsonArrayString() ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@user_qualification", Obj.user_qualification ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@idproof", Obj.idproof);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@signature", Obj.signature ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@stats_enable", Obj.stats_enable ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@creation_date", DateTime.Now);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@user_deactivated", Obj.user_deactivated ?? (object)DBNull.Value);

                                    conn.Open();
                                    sqlCommPatientInsert.ExecuteNonQuery();
                                }

                                msg = "User Successfully Created";


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

            result.message = msg;
            result.statusCode = statuscode;
            result.error = errorcode;
            result.data = dynamicDt;

            return result; ;
            //  return msg;
        }


        [JwtAuthorize(Roles = "user")]
        [HttpPost]
        [Route("api/UserAPI/UpdateUser")]
        public GenericAPIResult UpdateUser([FromBody] UserAPI Obj)
        {
            GenericAPIResult result = new GenericAPIResult();
            var dynamicDt = new List<dynamic>();
            string errorcode = "";
            string statuscode = "";


            string msg = "";

            

            if (Obj is null)
            {
                msg = "User Data is Blank";
                statuscode = "201";
                errorcode = "true";
            }
            else if (Obj.UserId == 0)
            {

                msg = "UserId is Blank";
                statuscode = "201";
                errorcode = "true";

            }
            else if (Obj.clinicCode == "" || Obj.clinicCode is null)
            {

                msg = "Clinic Code is Blank";
                statuscode = "201";
                errorcode = "true";

            }
            else if (Obj.name == "" || Obj.name is null)
            {
                msg = "Name is Blank";
                statuscode = "201";
                errorcode = "true";

            }
            else if (Obj.mobile_number == "" || Obj.mobile_number is null)
            {
                msg = "Mobile Number is Blank";
                statuscode = "201";
                errorcode = "true";

            }
            else if (Obj.user_roles.Count == 0 )
            {
                msg = "User Roles are Blank";
                statuscode = "201";
                errorcode = "true";

            }
            else if (Obj.user_qualification == "" || Obj.user_qualification is null)
            {
                msg = "User Qualification is Blank";
                statuscode = "201";
                errorcode = "true";

            }
            else
            {
                string userRolesJsonArrayString = Obj.GetUserRolesAsJsonArrayString();
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
                        try
                        {
                            DataTable dtPatDuplicate = new DataTable();
                            SqlCommand sqlCommPatDuplicate = new SqlCommand("Select * from [user] where mobile_number = '" + Obj.mobile_number + "' and cliniccode = '" + Obj.clinicCode + "' and UserId <> '"+Obj.UserId+"'", conn);
                            sqlCommPatDuplicate.CommandType = CommandType.Text;
                            conn.Open();

                            dtPatDuplicate.Load(sqlCommPatDuplicate.ExecuteReader());
                            conn.Close();
                            if (dtPatDuplicate.Rows.Count > 0)
                            {
                                msg = "Duplicate User, User with Mobile number " + Obj.mobile_number + " already exists ";
                            }
                            else
                            {
                                using (SqlCommand sqlCommPatientInsert = new SqlCommand(
                                            "Update [user] " +
                                            "set email = @email, name = @name, password=@password, mobile_number=@mobile_number, user_roles=@user_roles, user_qualification=@user_qualification, idproof=@idproof, signature = @signature, stats_enable=@stats_enable, user_deactivated = @user_deactivated where UserId = '"+Obj.UserId+"'", conn))
                                {
                                    sqlCommPatientInsert.CommandType = CommandType.Text;

                                    sqlCommPatientInsert.Parameters.AddWithValue("@email", Obj.email ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@name", Obj.name ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@password", Obj.password ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@mobile_number", Obj.mobile_number ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@user_roles", userRolesJsonArrayString ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@user_qualification", Obj.user_qualification ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@idproof", Obj.idproof);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@signature", Obj.signature ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@stats_enable", Obj.stats_enable ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@user_deactivated", Obj.user_deactivated ?? (object)DBNull.Value);

                                    conn.Open();
                                    sqlCommPatientInsert.ExecuteNonQuery();
                                }

                                msg = "User Successfully updated.";


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

            result.message = msg;
            result.statusCode = statuscode;
            result.error = errorcode;
            result.data = dynamicDt;

            return result; ;
            //  return msg;
        }

    }
}