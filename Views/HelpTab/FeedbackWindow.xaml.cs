using System.Windows;

namespace KannadaNudiEditor.Views.HelpTab
{
    public partial class HelpFeedbackWindow : Window
    {
        public HelpFeedbackWindow()
        {
            InitializeComponent();
        }

        private void SubmitFeedback_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailTextBox.Text.Trim();
            string feedback = FeedbackTextBox.Text.Trim();

            // Check if fields are not empty
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(feedback))
            {
                MessageBox.Show("Please fill in both Email and Feedback fields.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Print the values (this could be replaced with logging or saving to a file)
            Console.WriteLine("Email: " + email);
            Console.WriteLine("Feedback: " + feedback);

            // Show a confirmation message
            MessageBox.Show("Thank you for your feedback!", "Submitted", MessageBoxButton.OK, MessageBoxImage.Information);

            // Close the feedback window
            this.Close();
        }



        private void CloseWindow_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
