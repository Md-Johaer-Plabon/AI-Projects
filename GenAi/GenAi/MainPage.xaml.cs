using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace GenAi
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private readonly string apiKey = "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"; // Replace with your API key
        private readonly string endpoint = "https://api-inference.huggingface.co/models/google/flan-t5-large";

        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void AskAI_Click(object sender, RoutedEventArgs e)
        {
            string userInput = UserInput.Text;
            if (!string.IsNullOrWhiteSpace(userInput))
            {
                AIResponse.Text = "Thinking...";
                string aiReply = await GetAIResponse(userInput);
                AIResponse.Text = aiReply;
            }
        }

        private async Task<string> GetAIResponse(string prompt)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                var requestData = new { inputs = prompt };
                var content = new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json");


                HttpResponseMessage response = await client.PostAsync(endpoint, content);

                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();
                    dynamic jsonResponse = JsonConvert.DeserializeObject(result);
                    //var myObject = JsonConvert.DeserializeObject<MyObject>(result);
                    string res = jsonResponse.First.First.Value.Value.ToString();
                    return res;
                }
                else
                {
                    return "Error: Could not retrieve response.";
                }
            }
        }

        public class MyObject
        {
            public string Name { get; set; }
        }
    }
}
