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
    public class UserAPIController : ApiController
    {
        string constr = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        [JwtAuthorize(Roles = "user")]
        [HttpGet]
        [Route("api/UserAPI/GetUsers")]
        public GenericAPIResult GetUsers(string cliniccode, int pagenumber, int pagesize,DateTime updatedat)
        {

            List<UserAPI> patients = new List<UserAPI>();
            GenericAPIResult result = new GenericAPIResult();
            string strUpdatedAt = updatedat.ToString("dd-MMM-yyyy");
            var dynamicDt = new List<dynamic>();

            try
            {

                using (SqlConnection conn = new SqlConnection(constr))
                {
                    SqlCommand sqlComm = new SqlCommand("usp_GetUsersAll", conn);
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
                                if (sdr.GetName(i) == "user_roles")
                                {
                                    //string userRolesString = sdr.GetValue(i).ToString();
                                    //dynamic row1 = new ExpandoObject();
                                    //var dictionary1 = (IDictionary<string, object>)row1;

                                    //var userRoles = JsonConvert.DeserializeObject<List<string>>(userRolesString);

                                    //for (int j = 0; j < userRoles.Count; j++)
                                    //{
                                    //    dictionary1.Add(j.ToString(), userRoles[j]);
                                    //}

                                    //dictionary.Add("user_roles", dictionary1);


                                    string userRolesString = sdr.GetValue(i).ToString();

                                    // Deserialize the JSON string to a list of strings
                                    var userRoles = JsonConvert.DeserializeObject<List<string>>(userRolesString);

                                    // Add the list of user roles to the dictionary
                                    dictionary.Add("user_roles", userRoles.ToArray());

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
                                    //string userRolesString = sdr.GetValue(i).ToString();
                                    //dynamic row1 = new ExpandoObject();
                                    //var dictionary1 = (IDictionary<string, object>)row1;

                                    //var userRoles = JsonConvert.DeserializeObject<List<string>>(userRolesString);

                                    //for (int j = 0; j < userRoles.Count; j++)
                                    //{
                                    //    dictionary1.Add(j.ToString(), userRoles[j]);
                                    //}

                                    //dictionary.Add("user_roles", dictionary1);

                                    string userRolesString = sdr.GetValue(i).ToString();

                                    // Deserialize the JSON string to a list of strings
                                    var userRoles = JsonConvert.DeserializeObject<List<string>>(userRolesString);

                                    // Add the list of user roles to the dictionary
                                    dictionary.Add("user_roles", userRoles.ToArray());

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
        public async Task<GenericAPIResult> CreateUser([FromBody] UserAPI Obj)
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
                //string userRolesJsonArrayString = Obj.GetUserRolesAsJsonArrayString();
                string userRolesJsonArrayString = JsonConvert.SerializeObject(Obj.user_roles);
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
                                            "documentId, clinicId, clinicCode, email, name, password, mobile_number, user_roles, user_qualification, idproof, signature, stats_enable, creation_date, user_deactivated, updatedat" +
                                            ") VALUES (" +
                                            "@documentId, @clinicId, @clinicCode, @email, @name, @password, @mobile_number, @user_roles, @user_qualification, @idproof, @signature, " +
                                            "@stats_enable, @creation_date, @user_deactivated,@updatedAt)", conn))
                                {
                                    sqlCommPatientInsert.CommandType = CommandType.Text;

                                    sqlCommPatientInsert.Parameters.AddWithValue("@documentId", Obj.documentId ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@clinicId", Obj.clinicid ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@clinicCode", Obj.clinicCode ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@email", Obj.email ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@name", Obj.name ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@password", Obj.password ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@mobile_number", Obj.mobile_number ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@user_roles", userRolesJsonArrayString ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@user_qualification", Obj.user_qualification ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@idproof", Obj.idproof);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@signature", Obj.signature ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@stats_enable", Obj.stats_enable ?? (object)DBNull.Value);

                                    if (Obj.creation_date is null || Obj.creation_date.ToString() == "")
                                    {
                                        sqlCommPatientInsert.Parameters.AddWithValue("@creation_date", DateTime.Now);
                                    }
                                    else
                                    {
                                        sqlCommPatientInsert.Parameters.AddWithValue("@creation_date", Obj.creation_date);
                                    }

                                    sqlCommPatientInsert.Parameters.AddWithValue("@user_deactivated", Obj.user_deactivated ?? (object)DBNull.Value);

                                    if (Obj.updatedAt is null || Obj.updatedAt.ToString() == "")
                                    {
                                        sqlCommPatientInsert.Parameters.AddWithValue("@updatedAt", DateTime.Now);
                                    }
                                    else
                                    {
                                        sqlCommPatientInsert.Parameters.AddWithValue("@updatedAt", Obj.updatedAt);
                                    }


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

                #region Code to update Firebase Listener

                string Path = AppDomain.CurrentDomain.BaseDirectory + @"greenpaperdev-firebase-adminsdk-8k2y5-fb46e63414.json";
                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path);
                FirestoreDb db = FirestoreDb.Create("greenpaperdev");


                try
                {

                    CollectionReference col1 = db.Collection("WebAPIResponse");
                    // Specify the document ID 'GP-101'
                    DocumentReference doc1 = col1.Document(Obj.clinicCode);

                    // Delete the document if it exists
                    await doc1.DeleteAsync();

                    Dictionary<string, object> data1 = new Dictionary<string, object>
                        {
                            {"CollectionName" ,"User Created" },
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
        public async Task<GenericAPIResult> UpdateUser([FromBody] UserAPI Obj)
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
                //string userRolesJsonArrayString = Obj.GetUserRolesAsJsonArrayString();
                string userRolesJsonArrayString = JsonConvert.SerializeObject(Obj.user_roles);
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
                                            "set email = @email, name = @name, password=@password, mobile_number=@mobile_number, user_roles=@user_roles, user_qualification=@user_qualification, idproof=@idproof, signature = @signature, " +
                                            "updatedAt = @updatedAt,stats_enable=@stats_enable, user_deactivated = @user_deactivated where UserId = '"+Obj.UserId+"'", conn))
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
                                    sqlCommPatientInsert.Parameters.AddWithValue("@updatedAt", DateTime.Now);
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

                #region Code to update Firebase Listener

                string Path = AppDomain.CurrentDomain.BaseDirectory + @"greenpaperdev-firebase-adminsdk-8k2y5-fb46e63414.json";
                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path);
                FirestoreDb db = FirestoreDb.Create("greenpaperdev");


                try
                {

                    CollectionReference col1 = db.Collection("WebAPIResponse");
                    // Specify the document ID 'GP-101'
                    DocumentReference doc1 = col1.Document(Obj.clinicCode);

                    // Delete the document if it exists
                    await doc1.DeleteAsync();

                    Dictionary<string, object> data1 = new Dictionary<string, object>
                        {
                            {"CollectionName" ,"User Updated" },
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