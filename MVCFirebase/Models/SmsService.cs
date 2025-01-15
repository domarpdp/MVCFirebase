using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Http;
using System.Threading.Tasks;

namespace MVCFirebase.Models
{
    public class SmsService
    {
        //private static readonly HttpClient client = new HttpClient();
        //private readonly string apiKey = "4441a817-0863-11eb-9fa5-0200cd936042";  // Replace with your actual API key

        public async Task SendSmsAsync(string toPhoneNumber, string message)
        {
            try
            {
                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Post, "https://2factor.in/API/R1/");

                var collection = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("module", "TRANS_SMS"),
                    new KeyValuePair<string, string>("apikey", "4441a817-0863-11eb-9fa5-0200cd936042"),
                    new KeyValuePair<string, string>("to", toPhoneNumber),
                    new KeyValuePair<string, string>("from", "HEADER"),
                    new KeyValuePair<string, string>("msg", "Hi Amang (Bhatia). Get Well Soon For e-prescription: TEST Fee Rs. Test4 Test5 GPTech"),
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
                Console.WriteLine(result);




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
                Console.WriteLine($"Error occurred: {ex.Message}");
            }
        }
    }
}