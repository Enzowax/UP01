using System.Windows;

namespace ShutIKrol.Pages
{
    public partial class ChangePasswordDialog : Window
    {
        public string NewPassword => PwdBox.Password;

        public ChangePasswordDialog()
        {
            InitializeComponent();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(PwdBox.Password)) { MessageBox.Show("Введите пароль."); return; }
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
