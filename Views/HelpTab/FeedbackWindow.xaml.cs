using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;

namespace KannadaNudiEditor.Views.HelpTab
{
    public partial class HelpFeedbackWindow : Window
    {
        private readonly HttpClient _httpClient = new HttpClient();

        public HelpFeedbackWindow()
        {
            InitializeComponent();
        }

        private async void SubmitFeedback_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailTextBox.Text.Trim();
            string feedback = FeedbackTextBox.Text.Trim();

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(feedback))
            {
                MessageBox.Show("Please fill in both Email and Feedback fields.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var payload = new
            {
                username = email,
                feedback = feedback
            };

            string json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");


            try
            {
                var response = await _httpClient.PostAsync("https://kagapa.com/speech-to-text/save_feedback", content);
                string result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var responseObject = JsonConvert.DeserializeObject<dynamic>(result);

                    if (responseObject != null && responseObject.message != null)
                    {
                        string message = responseObject.message.ToString();
                        MessageBox.Show(message, "Submitted", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Feedback submitted, but no confirmation message received.", "Submitted", MessageBoxButton.OK, MessageBoxImage.Information);
                    }

                    this.Close();
                }
                else
                {
                    MessageBox.Show("Failed to submit feedback. Please try again later.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error submitting feedback:\n" + ex.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }





        }

        private void CloseWindow_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
