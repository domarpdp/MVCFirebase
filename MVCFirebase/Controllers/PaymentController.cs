using Razorpay.Api;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace MVCFirebase.Controllers
{
    public class PaymentController : Controller
    {
        string constr = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        RazorpayClient client = new RazorpayClient(
                System.Configuration.ConfigurationManager.AppSettings["RazorpayKey"],
                System.Configuration.ConfigurationManager.AppSettings["RazorpaySecret"]);
        // GET: Payment
        public ActionResult Index(string ClinicCode,string OrderType,string PlanId)
        {
            

            decimal orderAmount = 0;
            SqlCommand sqlComm = new SqlCommand();
            using (SqlConnection conn = new SqlConnection(constr))
            {
                DataTable dt = new DataTable();
                if (OrderType == "SMS")
                {
                     sqlComm = new SqlCommand("Select amount as orderamount from SMSPlan where plan_id ='" + PlanId + "'", conn);
                }
                else
                {
                     sqlComm = new SqlCommand("Select plan_price as orderamount from subscriptionplans where plan_id ='"+ PlanId + "'", conn);
                }
                
                sqlComm.CommandType = CommandType.Text;
                conn.Open();

                dt.Load(sqlComm.ExecuteReader());
                conn.Close();


                orderAmount = Convert.ToDecimal(dt.Rows[0]["orderamount"].ToString());

                
            }

                // Convert amount to paise
                //decimal orderAmount = Convert.ToDecimal(amount) * 100;

            Dictionary<string, object> input = new Dictionary<string, object>();
            input.Add("amount", orderAmount * 100);
            input.Add("currency", "INR");
            input.Add("receipt", "order_rcptid_11");

            Razorpay.Api.Order order = client.Order.Create(input);

            var orderId = order["id"].ToString();

            ViewBag.ClinicCode = ClinicCode;
            ViewBag.OrderType = OrderType;
            ViewBag.PlanId = PlanId;
            ViewBag.OrderId = orderId;
            ViewBag.Amount = orderAmount;
            ViewBag.ClinicCode = ClinicCode;
            ViewBag.OrderType = OrderType;
            ViewBag.OrderAmount = orderAmount;
            ViewBag.Key = System.Configuration.ConfigurationManager.AppSettings["RazorpayKey"];

            return View("Payment");

            //return View();
        }

        //public ActionResult Payment(string ClinicCode, string OrderType, string PlanId)
        //{
        //    //ViewBag.ClinicCode = ClinicCode;
        //    ViewBag.ClinicCode = ClinicCode;
        //    ViewBag.OrderType = OrderType;
        //    ViewBag.PlanId = PlanId;
        //    return View();
        //}

        [System.Web.Http.HttpPost]
        public ActionResult CreateOrder(string amount, string ClinicCode,string OrderType)
        {
            // Convert amount to paise
            decimal orderAmount = Convert.ToDecimal(amount) * 100;

            Dictionary<string, object> input = new Dictionary<string, object>();
            input.Add("amount", orderAmount);
            input.Add("currency", "INR");
            input.Add("receipt", "order_rcptid_11");

            Razorpay.Api.Order order = client.Order.Create(input);

            var orderId = order["id"].ToString();

            ViewBag.OrderId = orderId;
            ViewBag.Amount = amount;
            ViewBag.ClinicCode = ClinicCode;
            ViewBag.OrderType = OrderType;
            ViewBag.OrderAmount = orderAmount;
            ViewBag.Key = System.Configuration.ConfigurationManager.AppSettings["RazorpayKey"];

            return View("Payment");
        }

        public ActionResult PaymentSuccess(string payment_id, string order_id, string signature, string clinicCode, string orderType, decimal orderAmount, string plan_id)
        {
            // Validate the signature and complete the payment process
            try
            {


                //var client = new RazorpayClient(
                //    System.Configuration.ConfigurationManager.AppSettings["RazorpayKey"],
                //    System.Configuration.ConfigurationManager.AppSettings["RazorpaySecret"]);

                var attributes = new Dictionary<string, string>();
                attributes.Add("razorpay_payment_id", payment_id);
                attributes.Add("razorpay_order_id", order_id);
                attributes.Add("razorpay_signature", signature);
                //attributes.Add("razorpay_clinicCode", clinicCode);

                Utils.verifyPaymentSignature(attributes);
                // Payment signature is valid, you can process further (like saving data to DB)

                ViewBag.PaymentId = payment_id;
                ViewBag.OrderId = order_id;
                ViewBag.ClinicCode = clinicCode;
                ViewBag.OrderType = orderType;
                ViewBag.PlanId = plan_id;

                ViewBag.Amount = orderAmount;

                using (SqlConnection conn = new SqlConnection(constr))
                {
                    DataTable dt = new DataTable();
                    SqlCommand sqlComm = new SqlCommand("Select isnull(Max(Id),0 ) as Id from payments", conn);
                    sqlComm.CommandType = CommandType.Text;
                    conn.Open();

                    dt.Load(sqlComm.ExecuteReader());
                    conn.Close();


                    string Id = dt.Rows[0]["Id"].ToString();

                    int maxid = Convert.ToInt32(Id) + 1;

                    using (SqlCommand sqlCommPatientInsert = new SqlCommand(
                                                "INSERT INTO payments (" +
                                                "Id, ClinicCode, CreationDate, OrderId, PaymentId, Status, OrderType, Amount,plan_id" +
                                                ") VALUES (" +
                                                "@Id, @clinicCode, @CreationDate, @OrderId, @PaymentId, @Status, @OrderType,@Amount,@plan_id)", conn))
                    {
                        sqlCommPatientInsert.CommandType = CommandType.Text;

                        sqlCommPatientInsert.Parameters.AddWithValue("@Id", maxid);
                         sqlCommPatientInsert.Parameters.AddWithValue("@clinicCode", clinicCode ?? (object)DBNull.Value);
                        sqlCommPatientInsert.Parameters.AddWithValue("@CreationDate", DateTime.Now);
                        sqlCommPatientInsert.Parameters.AddWithValue("@OrderId", order_id ?? (object)DBNull.Value);
                        sqlCommPatientInsert.Parameters.AddWithValue("@PaymentId", payment_id ?? (object)DBNull.Value);
                        sqlCommPatientInsert.Parameters.AddWithValue("@Status", "Success" ?? (object)DBNull.Value);
                        sqlCommPatientInsert.Parameters.AddWithValue("@OrderType", orderType ?? (object)DBNull.Value);
                        sqlCommPatientInsert.Parameters.AddWithValue("@Amount", orderAmount);
                        sqlCommPatientInsert.Parameters.AddWithValue("@plan_id", plan_id);
                        conn.Open();
                        sqlCommPatientInsert.ExecuteNonQuery();
                    }
                }

                return RedirectToAction("Success", new { payment_id = payment_id, order_id = order_id, ClinicCode = clinicCode, OrderType = orderType, Amount = orderAmount, PlanId = plan_id });

            }
            catch (Exception ex)
            {
                using (SqlConnection conn = new SqlConnection(constr))
                {
                    DataTable dt = new DataTable();
                    SqlCommand sqlComm = new SqlCommand("Select isnull(Max(Id),0 ) as Id from payments", conn);
                    sqlComm.CommandType = CommandType.Text;
                    conn.Open();

                    dt.Load(sqlComm.ExecuteReader());
                    conn.Close();


                    string Id = dt.Rows[0]["Id"].ToString();

                    int maxid = Convert.ToInt32(Id) + 1;

                    using (SqlCommand sqlCommPatientInsert = new SqlCommand(
                                                "INSERT INTO payments (" +
                                                "Id, ClinicCode, CreationDate, OrderId, PaymentId, Status, OrderType, Amount,plan_id" +
                                                ") VALUES (" +
                                                "@Id, @clinicCode, @CreationDate, @OrderId, @PaymentId, @Status, @OrderType, @Amount,@plan_id)", conn))
                    {
                        sqlCommPatientInsert.CommandType = CommandType.Text;

                        sqlCommPatientInsert.Parameters.AddWithValue("@Id", maxid);
                        sqlCommPatientInsert.Parameters.AddWithValue("@clinicCode", clinicCode ?? (object)DBNull.Value);
                        sqlCommPatientInsert.Parameters.AddWithValue("@CreationDate", DateTime.Now);
                        sqlCommPatientInsert.Parameters.AddWithValue("@OrderId", order_id ?? (object)DBNull.Value);
                        sqlCommPatientInsert.Parameters.AddWithValue("@PaymentId", payment_id ?? (object)DBNull.Value);
                        sqlCommPatientInsert.Parameters.AddWithValue("@Status", "Failed" ?? (object)DBNull.Value);
                        sqlCommPatientInsert.Parameters.AddWithValue("@OrderType", orderType ?? (object)DBNull.Value);
                        sqlCommPatientInsert.Parameters.AddWithValue("@Amount", orderAmount);
                        sqlCommPatientInsert.Parameters.AddWithValue("@plan_id", plan_id);
                        conn.Open();
                        sqlCommPatientInsert.ExecuteNonQuery();
                    }
                }

                return RedirectToAction("Error", new { payment_id = payment_id, order_id = order_id, ClinicCode = clinicCode, OrderType = orderType, Amount = orderAmount, PlanId = plan_id });
            }

            
        }

        public ActionResult Success(string payment_id, string order_id, string ClinicCode, string OrderType, decimal Amount,string PlanId)
        {
            ViewBag.PaymentId = payment_id;
            ViewBag.OrderId = order_id;
            ViewBag.ClinicCode = ClinicCode;
            ViewBag.OrderType = OrderType;
            ViewBag.Amount = Amount;
            ViewBag.PlanId = PlanId;
            return View();
        }

        // This method will check the order status using the Razorpay API
        public ActionResult CheckOrderStatus(string orderId)
        {
            try
            {
                // Create Razorpay client instance
                var client = new RazorpayClient(
                    System.Configuration.ConfigurationManager.AppSettings["RazorpayKey"],
                    System.Configuration.ConfigurationManager.AppSettings["RazorpaySecret"]);

                // Fetch the order by its ID
                Order order = client.Order.Fetch(orderId);
                //Order order = client.Order.Fetch("order_P5JL3JG90iujuZ");
                
                // Get the order status
                string status = order["status"].ToString();

                ViewBag.Status = status;
                ViewBag.OrderId = orderId;

                return View("OrderStatus");
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View("Error");
            }
        }


    }
}
