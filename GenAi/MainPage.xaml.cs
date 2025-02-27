using GenAi.Response;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace GenAi
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private string base64Content { get; set; }
        private static string type { get; set; }

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
            if (string.IsNullOrEmpty(userInput))
            {
                AIResponse.Text = "Please enter a question.";
                return;
            }

            AIResponse.Text = "Thinking...";
            string response = await AiResponseManager.GetAIResponse(userInput, base64Content, type);
            
            AIResponse.Text = response;
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
    }
}
