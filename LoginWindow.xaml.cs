using System.Windows;
using ShutIKrol.Data;
using ShutIKrol.Helpers;

namespace ShutIKrol
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            LoginError.Text = "";
            string login = LoginBox.Text.Trim();
            string password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                LoginError.Text = "Введите логин и пароль.";
                return;
            }

            try
            {
                var user = DB.Login(login, password);
                if (user == null)
                {
                    LoginError.Text = "Неверный логин или пароль.";
                    return;
                }
                AppState.CurrentUser = user;
                var main = new MainWindow();
                main.Show();
                Close();
            }
            catch (Exception ex)
            {
                LoginError.Text = "Ошибка подключения к БД: " + ex.Message;
            }
        }

        private void DbSettings_Click(object sender, RoutedEventArgs e)
        {
            new ConnectionSettingsWindow().ShowDialog();
        }

        private void RegisterBtn_Click(object sender, RoutedEventArgs e)
        {
            RegError.Text = "";
            string name = RegName.Text.Trim();
            string login = RegLogin.Text.Trim();
            string email = RegEmail.Text.Trim();
            string password = RegPassword.Password;

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(login) ||
                string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                RegError.Text = "Заполните все поля.";
                return;
            }
            if (password.Length < 4)
            {
                RegError.Text = "Пароль должен быть не менее 4 символов.";
                return;
            }

            try
            {
                if (DB.LoginExists(login))
                {
                    RegError.Text = "Такой логин уже существует.";
                    return;
                }
                var user = DB.Register(name, login, password, email);
                AppState.CurrentUser = user;
                var main = new MainWindow();
                main.Show();
                Close();
            }
            catch (Exception ex)
            {
                RegError.Text = "Ошибка: " + ex.Message;
            }
        }
    }
}
