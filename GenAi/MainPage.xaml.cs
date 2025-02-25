using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.IO;
using Windows.UI.Xaml.Media.Imaging;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace GenAi
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private const string apiKey = "AIzaSyCxATOqKYc7pAYO8Zx4gylesJ9487R0zdI"; // Replace with your API key
        private const string endpoint = "https://generativelanguage.googleapis.com/v1/models/gemini-1.5-flash:generateContent?key=" + apiKey;
        private string base64Content { get; set; } = null;
        private string type { get; set; }

        public MainPage()
        {
            this.InitializeComponent();
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            AIResponse.Text = "";
            SelectedImage.Source = null;
        }
        
        private async void SelectImage_Click(object sender, RoutedEventArgs e)
        {
            await PickImage();
            if (base64Content != null)
            {
                AIResponse.Text = "Image selected!";
                type = "image/png";
            }
            else
            {
                AIResponse.Text = "No image selected.";
            }
        }

        private async void SelectAudio_Click(object sender, RoutedEventArgs e)
        {
            base64Content = await ConvertAudioToBase64();
            if (base64Content != null)
            {
                AIResponse.Text = "Audio selected!";
                type = "audio/wav";
            }
            else
            {
                AIResponse.Text = "No audio selected.";
            }
        }

        private async void AskAI_Click(object sender, RoutedEventArgs e)
        {
            string userInput = UserInput.Text;
            if (string.IsNullOrWhiteSpace(userInput))
            {
                AIResponse.Text = "Please enter a question.";
                return;
            }

            AIResponse.Text = "Thinking...";
            string response = await GetAIResponse(userInput, base64Content);
            AIResponse.Text = response;
        }

        public async Task<string> GetAIResponse(string userInput, string base64Content)
        {
            try
            {
                using (HttpClient client = new HttpClient())
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

                    string json = JsonConvert.SerializeObject(requestBody);
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

        private async Task PickImage()
        {
            FileOpenPicker picker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.PicturesLibrary
            };
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".png");

            StorageFile file = await picker.PickSingleFileAsync();
            if (file == null)
            {
                AIResponse.Text = "No image selected.";
                return;
            }

            using (IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.Read))
            {
                BitmapImage bitmapImage = new BitmapImage();
                await bitmapImage.SetSourceAsync(fileStream);
                SelectedImage.Source = bitmapImage;
            }

            base64Content = await ConvertImageToBase64(file);
            AIResponse.Text = "Image selected!";
        }

        private async Task<string> ConvertImageToBase64(StorageFile file)
        {
            using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.Read))
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    await stream.AsStreamForRead().CopyToAsync(memoryStream);
                    byte[] bytes = memoryStream.ToArray();
                    return Convert.ToBase64String(bytes);
                }
            }
        }

        public static async Task<string> ConvertAudioToBase64()
        {
            FileOpenPicker picker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.MusicLibrary
            };
            picker.FileTypeFilter.Add(".wav");
            picker.FileTypeFilter.Add(".mp3");
            picker.FileTypeFilter.Add(".m4a");

            StorageFile file = await picker.PickSingleFileAsync();
            if (file == null) return null;

            using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.Read))
            {
                using (DataReader reader = new DataReader(stream))
                {
                    byte[] bytes = new byte[stream.Size];
                    await reader.LoadAsync((uint)stream.Size);
                    reader.ReadBytes(bytes);
                    return Convert.ToBase64String(bytes);
                }
            }
        }

        public class Root
        {
            public string generated_text { get; set; }
        }

    }
}
