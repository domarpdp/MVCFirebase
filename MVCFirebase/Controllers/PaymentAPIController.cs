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
    public class PaymentAPIController : ApiController
    {
        // GET: PaymentAPI

        string constr = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        static DateTime utcTime = DateTime.UtcNow;
        static TimeZoneInfo istZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime istTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, istZone);


        [JwtAuthorize(Roles = "user")]
        [HttpGet]
        [Route("api/PaymentAPI/GetPayments")]
        public GenericAPIResult GetPayments(string cliniccode, int pagenumber, int pagesize)
        {
            List<PaymentAPI> payments = new List<PaymentAPI>();
            GenericAPIResult result = new GenericAPIResult();
            
            var dynamicDt = new List<dynamic>();

            try
            {

                using (SqlConnection conn = new SqlConnection(constr))
                {
                    SqlCommand sqlComm = new SqlCommand("usp_GetPayments", conn);
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
                result.message = "Payments List fetched Successfully";
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
        [Route("api/PaymentAPI/GetSubscriptions")]
        public GenericAPIResult GetSubscriptions(int pagenumber, int pagesize)
        {
            List<SubscriptionAPI> payments = new List<SubscriptionAPI>();
            GenericAPIResult result = new GenericAPIResult();

            var dynamicDt = new List<dynamic>();

            try
            {

                using (SqlConnection conn = new SqlConnection(constr))
                {
                    SqlCommand sqlComm = new SqlCommand("usp_GetSubscriptions", conn);
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
                                if (sdr.GetName(i) == "plan_benefits" || sdr.GetName(i) == "plan_other_benefits")
                                {
                                    string StringArray = sdr.GetValue(i)?.ToString();

                                    if (!string.IsNullOrEmpty(StringArray))
                                    {
                                        // Deserialize the JSON string to a list of strings
                                        var stringArrayValues = JsonConvert.DeserializeObject<List<string>>(StringArray);

                                        // Add the list of user roles to the dictionary
                                        dictionary.Add(sdr.GetName(i), stringArrayValues.ToArray());
                                    }
                                    else
                                    {
                                        // Handle null or blank value, e.g., add an empty array
                                        dictionary.Add(sdr.GetName(i), new string[] { });
                                    }

                                }
                                else if (sdr.GetName(i) == "showPlan")
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
                result.message = "Subscriptions List fetched Successfully";
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
        [Route("api/PaymentAPI/GetSMSPlans")]
        public GenericAPIResult GetSMSPlans(int pagenumber, int pagesize)
        {
            List<SMSPlansAPI> payments = new List<SMSPlansAPI>();
            GenericAPIResult result = new GenericAPIResult();

            var dynamicDt = new List<dynamic>();

            try
            {

                using (SqlConnection conn = new SqlConnection(constr))
                {
                    SqlCommand sqlComm = new SqlCommand("usp_GetSMSPlans", conn);
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
                                if (sdr.GetName(i) == "defaultSelect")
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
                result.message = "SMSPlans List fetched Successfully";
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