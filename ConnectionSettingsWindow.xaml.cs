using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using ShutIKrol.Data;

namespace ShutIKrol
{
    public partial class ConnectionSettingsWindow : Window
    {
        public ConnectionSettingsWindow()
        {
            InitializeComponent();
            LoadSettings();
            UpdateAuthFields();
        }

        private void LoadSettings()
        {
            var s = DbSettings.Load();
            ServerBox.Text   = s.Server;
            DatabaseBox.Text = s.Database;
            WinAuthBtn.IsChecked  = s.WinAuth;
            SqlAuthBtn.IsChecked  = !s.WinAuth;
            LoginBox.Text    = s.Login;
            PasswordBox.Password = s.Password;
        }

        private void AuthMode_Changed(object sender, RoutedEventArgs e) => UpdateAuthFields();

        private void UpdateAuthFields()
        {
            if (LoginLabel == null) return; // called during XAML init — controls not ready yet
            bool win = WinAuthBtn.IsChecked == true;
            LoginLabel.Opacity    = win ? 0.35 : 1;
            PasswordLabel.Opacity = win ? 0.35 : 1;
            LoginBox.IsEnabled    = !win;
            PasswordBox.IsEnabled = !win;
        }

        private DbSettings BuildSettings() => new DbSettings
        {
            Server   = ServerBox.Text.Trim(),
            Database = DatabaseBox.Text.Trim(),
            WinAuth  = WinAuthBtn.IsChecked == true,
            Login    = LoginBox.Text.Trim(),
            Password = PasswordBox.Password
        };

        private void Test_Click(object sender, RoutedEventArgs e)
        {
            StatusText.Text = "⏳ Проверяю подключение...";
            StatusText.Foreground = System.Windows.Media.Brushes.Gray;

            var cs = BuildSettings().ToConnectionString();
            try
            {
                var opts = new DbContextOptionsBuilder<AppDbContext>()
                    .UseSqlServer(cs).Options;
                using var ctx = new AppDbContext(opts);
                if (ctx.Database.CanConnect())
                {
                    StatusText.Text = "✅ Подключение успешно!";
                    StatusText.Foreground = System.Windows.Media.Brushes.LightGreen;
                }
                else
                {
                    StatusText.Text = "❌ Сервер недоступен или БД не найдена.";
                    StatusText.Foreground = System.Windows.Media.Brushes.Tomato;
                }
            }
            catch (Exception ex)
            {
                StatusText.Text = "❌ Ошибка: " + ex.Message;
                StatusText.Foreground = System.Windows.Media.Brushes.Tomato;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var s = BuildSettings();
            var cs = s.ToConnectionString();
            try
            {
                var opts = new DbContextOptionsBuilder<AppDbContext>()
                    .UseSqlServer(cs).Options;
                using var ctx = new AppDbContext(opts);
                if (!ctx.Database.CanConnect()) throw new Exception("Сервер не отвечает.");
            }
            catch (Exception ex)
            {
                var r = MessageBox.Show(
                    $"Не удалось подключиться:\n{ex.Message}\n\nВсё равно сохранить?",
                    "Ошибка подключения", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (r != MessageBoxResult.Yes) return;
            }

            s.Save();
            DB.ConnectionString = cs;
            DialogResult = true;
            Close();
        }
    }
}
