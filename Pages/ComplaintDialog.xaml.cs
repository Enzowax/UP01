using System.Windows;

namespace ShutIKrol.Pages
{
    public partial class ComplaintDialog : Window
    {
        public string Reason => ReasonBox.Text.Trim();

        public ComplaintDialog(string title)
        {
            InitializeComponent();
            Title = title;
            TitleLabel.Text = title;
        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ReasonBox.Text)) { MessageBox.Show("Введите причину."); return; }
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
