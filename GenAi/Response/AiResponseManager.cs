using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace GenAi.Response
{
    public static class AiResponseManager
    {
        private const string apiKey = "AIzaSyCxATOqKYc7pAYO8Zx4gylesJ9487R0zdI";
        private const string endpoint = "https://generativelanguage.googleapis.com/v1/models/gemini-1.5-flash:generateContent?key=" + apiKey;

        public static async Task<string> GetAIResponse(string userInput, string base64Content, string type)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string json = GetRequestBody(userInput, base64Content, type);
                    HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync(endpoint, content);
                    string responseString = await response.Content.ReadAsStringAsync();

                    dynamic result = JsonConvert.DeserializeObject(responseString);
                    return result?.candidates[0]?.content?.parts[0]?.text ?? "No response from AI";
                }
            }
            catch (Exception ex)
            {
                return "Error: " + ex.Message;
            }
        }


        private static string GetRequestBody(string userInput, string base64Content, string type)
        {
            string json = "";

            if (string.IsNullOrEmpty(base64Content))
            {
                var requestBody = new
                {
                    contents = new[]
                        {
                            new
                            {
                                parts = new[]
                                {
                                    new { text = userInput }
                                }
                            }
                        }
                };

                json = JsonConvert.SerializeObject(requestBody);
            }
            else
            {
                var requestBody = new
                {
                    contents = new[]
                        {
                        new
                        {
                            parts = new object[]
                            {
                                new { text = userInput },
                                new
                                {
                                    inlineData = new
                                    {
                                        mimeType = type,
                                        data = base64Content
                                    }
                                }
                            }
                        }
                    }

                };
                json = JsonConvert.SerializeObject(requestBody);
            }

            return json;
        }
    }
}
