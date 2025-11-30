using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using KannadaNudiEditor.Views.Loading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq; // Added for safer JSON handling [web:25][web:29]

namespace KannadaNudiEditor.Views.HelpTab
{
    public partial class HelpFeedbackWindow : Window
    {
        // Static HttpClient following best practices [web:6][web:21][web:24][web:26]
        private static readonly HttpClient _httpClient;

        static HelpFeedbackWindow()
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(30)
            };
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "KannadaNudiEditor/1.0");
        }

        public HelpFeedbackWindow()
        {
            InitializeComponent();
            SimpleLogger.Log("HelpFeedbackWindow opened.");
        }

        private async void SubmitFeedback_Click(object sender, RoutedEventArgs e)
        {
            SimpleLogger.Log("Submit button clicked.");

            // UI validation and feedback [memory:40]
            if (string.IsNullOrWhiteSpace(EmailTextBox.Text))
            {
                ShowValidationError("ದಯವಿಟ್ಟು ಇಮೇಲ್ ಠಿಕ್ಕಣೆ ನಮೂದಿಸಿ.", EmailTextBox);
                return;
            }

            if (!IsValidEmail(EmailTextBox.Text))
            {
                ShowValidationError("ದಯವಿಟ್ಟು ಸರಿಯಾದ ಇಮೇಲ್ ಠಿಕ್ಕಣೆ ನಮೂದಿಸಿ.", EmailTextBox);
                return;
            }

            if (string.IsNullOrWhiteSpace(FeedbackTextBox.Text))
            {
                ShowValidationError("ದಯವಿಟ್ಟು ನಿಮ್ಮ ಅಭಿಪ್ರಾಯವನ್ನು ಬರೆಯಿರಿ.", FeedbackTextBox);
                return;
            }

            if (FeedbackTextBox.Text.Length < 10)
            {
                ShowValidationError("ಅಭಿಪ್ರಾಯ ಕನಿಷ್ಠ 10 ಅಕ್ಷರಗಳಿರಬೇಕು.", FeedbackTextBox);
                return;
            }

            LoadingView.Show();
            try
            {
                var payload = new
                {
                    username = EmailTextBox.Text.Trim(),
                    feedback = FeedbackTextBox.Text.Trim(),
                    app_version = "1.0", // Add app context
                    timestamp = DateTime.UtcNow.ToString("o")
                };

                string json = JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("https://kagapa.com/speech-to-text/save_feedback", content);
                string responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    SimpleLogger.Log($"Feedback API error: {response.StatusCode} - {responseContent}");
                    ShowError("ಪ್ರತಿಕ್ರಿಯೆ ಸಲ್ಲಿಕೆ ವಿಫಲವಾಯಿತು. ನಂತರ ಪ್ರಯತ್ನಿಸಿ.");
                    return;
                }

                // Safer JSON parsing [web:25][web:29]
                string message = "ಪ್ರತಿಕ್ರಿಯೆ ಯಶಸ್ವಿಯಾಗಿ ಸಲ್ಲಿಕೆಯಾಯಿತು!";
                try
                {
                    var jsonResponse = JObject.Parse(responseContent);
                    message = jsonResponse["message"]?.ToString() ?? message;
                }
                catch (JsonException jsonEx)
                {
                    SimpleLogger.Log($"JSON parse warning: {jsonEx.Message}");
                }

                MessageBox.Show(message, "ಸಲ್ಲಿಕೆಯಾಯಿತು",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                SimpleLogger.Log("Feedback submitted successfully.");
                DialogResult = true;
                Close();
            }
            catch (HttpRequestException httpEx)
            {
                SimpleLogger.Log($"Network error: {httpEx.Message}");
                ShowError("ನೆಟ್‌ವರ್ಕ್ ಸಮಸ್ಯೆ. ಆನ್‌ಲೈನ್ ಆಗಿರಿ ಎಂದು ಖಚಿತಪಡಿಸಿಕೊಳ್ಳಿ.");
            }
            catch (TaskCanceledException)
            {
                ShowError("ಸರ್ವರ್ ಸ್ಪಂದನೆ ಕಾಲಾಯಿತು. ನಂತರ ಪ್ರಯತ್ನಿಸಿ.");
            }
            catch (Exception ex)
            {
                SimpleLogger.Log($"Unexpected error: {ex}");
                ShowError($"ತಪ್ಪು: {ex.Message}");
            }
            finally
            {
                LoadingView.Hide();
            }
        }

        private void CloseWindow_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private void ShowValidationError(string message, UIElement focusElement)
        {
            MessageBox.Show(message, " ಮಾಹಿತಿ", MessageBoxButton.OK, MessageBoxImage.Warning);
            focusElement?.Focus();
        }

        private void ShowError(string message)
        {
            MessageBox.Show(message, " ತಪ್ಪು", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
