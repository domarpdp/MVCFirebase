using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Hangfire;
using MVCFirebase.Models;

namespace MVCFirebase.Controllers
{
    public class SmsController : Controller
    {
        private readonly SmsService _smsService;

        public SmsController()
        {
            _smsService = new SmsService();
        }
        // GET: Sms
        public ActionResult Index()
        {
            return View();
        }
        public async Task<ActionResult> ScheduleSms(string toPhoneNumber, string message)
        {
            // Schedule the SMS to be sent at the specified time (sendAt)
            //BackgroundJob.Schedule(() => _smsService.SendSmsAsync(toPhoneNumber, message), sendAt);
            //return Content("SMS scheduled successfully!");

            try
            {
                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Post, "https://2factor.in/API/R1/");

                var collection = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("module", "TRANS_SMS"),
                    new KeyValuePair<string, string>("apikey", "4441a817-0863-11eb-9fa5-0200cd936042"),
                    new KeyValuePair<string, string>("to", toPhoneNumber),
                    new KeyValuePair<string, string>("from", "GPTECH"),
                    new KeyValuePair<string, string>("msg", "Hi Pramod (Domarp). Get Well Soon For e-prescription: TEST Fee Rs. Test4 Test5 GPTech"),
                    //new KeyValuePair<string, string>("scheduletime", "2022-01-01 13:27:00"),
                    //new KeyValuePair<string, string>("peid", "DLT Registration Number"),
                    //new KeyValuePair<string, string>("ctid", "DLT Content Template Id"),
                    //new KeyValuePair<string, string>("campaignname", "Name For a Click Tracking Campaign"),
                    //new KeyValuePair<string, string>("campaignwebhook", "URL for receiving Webhook notification on shortlink clicks")
                };

                request.Content = new FormUrlEncodedContent(collection);

                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadAsStringAsync();
                //Console.WriteLine(result);
                return Content(result);



                //// Prepare the request URL
                //string requestUrl = $"https://2factor.in/API/V1/{apiKey}/SMS/{toPhoneNumber}/{message}";

                //// Send the request
                //HttpResponseMessage response = await client.GetAsync(requestUrl);

                //// Check if the request was successful
                //if (response.IsSuccessStatusCode)
                //{
                //    Console.WriteLine("SMS sent successfully.");
                //}
                //else
                //{
                //    Console.WriteLine($"Failed to send SMS. Status Code: {response.StatusCode}");
                //}
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
                //Console.WriteLine($"Error occurred: {ex.Message}");
            }
        }

        // Immediate SMS sending (without scheduling)
        public async Task<ActionResult> SendSmsNow(string toPhoneNumber, string message)
        {
            await _smsService.SendSmsAsync(toPhoneNumber, message);
            return Content("SMS sent immediately!");
        }
    }
}