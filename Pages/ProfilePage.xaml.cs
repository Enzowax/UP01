using System.Windows;
using System.Windows.Controls;
using ShutIKrol.Data;
using ShutIKrol.Helpers;

namespace ShutIKrol.Pages
{
    public partial class ProfilePage : Page
    {
        public ProfilePage()
        {
            InitializeComponent();
            Loaded += (_, _) => LoadProfile();
        }

        private void LoadProfile()
        {
            var u = AppState.CurrentUser!;

            UserName.Text = u.Name;
            UserLogin.Text = "Логин: " + u.Login;
            UserEmail.Text = "Email: " + u.Email;
            UserRole.Text = "Роль: " + u.RoleName;
            UserRegDate.Text = "Дата регистрации: " + u.RegistrationDate.ToString("dd.MM.yyyy");

            // Freeze banner
            if (u.IsFrozen)
            {
                FrozenBanner.Visibility = Visibility.Visible;
                FreezeReasonText.Text = "Ваш аккаунт заморожен. Подайте заявку на снятие заморозки.";
                bool hasPending = DB.HasPendingRequest(u.Id, DB.EnsureRequestType("AccountUnfrozing"));
                AppealBtn.IsEnabled = !hasPending;
                AppealBtn.Content = hasPending ? "✔ Заявка уже подана" : "📝 Оспорить заморозку";
            }
            else
            {
                FrozenBanner.Visibility = Visibility.Collapsed;
            }

            // Author request button for readers
            if (u.RoleId == 6)
            {
                RequestAuthorBtn.Visibility = Visibility.Visible;
                int typeId = DB.EnsureRequestType("AuthorRoleRequest");
                bool hasPending = DB.HasPendingRequest(u.Id, typeId);
                RequestAuthorBtn.IsEnabled = !hasPending;
                RequestAuthorBtn.Content = hasPending ? "✔ Заявка отправлена" : "✍️ Стать автором";
            }
            else
            {
                RequestAuthorBtn.Visibility = Visibility.Collapsed;
            }

            // Reviews
            var reviews = DB.GetUserReviews(u.Id);
            ReviewsList.ItemsSource = reviews;
            NoReviews.Visibility = reviews.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void AppealBtn_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new ComplaintDialog("Заявка на снятие заморозки аккаунта");
            if (dlg.ShowDialog() == true && !string.IsNullOrWhiteSpace(dlg.Reason))
            {
                int typeId = DB.EnsureRequestType("AccountUnfrozing");
                DB.AddRequest(typeId, AppState.CurrentUser!.Id, dlg.Reason);
                MessageBox.Show("Заявка отправлена администратору.", "OK", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadProfile();
            }
        }

        private void RequestAuthorBtn_Click(object sender, RoutedEventArgs e)
        {
            int typeId = DB.EnsureRequestType("AuthorRoleRequest");
            DB.AddRequest(typeId, AppState.CurrentUser!.Id, "Хочу стать автором.");
            MessageBox.Show("Заявка на роль Автора отправлена!", "OK", MessageBoxButton.OK, MessageBoxImage.Information);
            LoadProfile();
        }

        private void LogoutBtn_Click(object sender, RoutedEventArgs e)
        {
            AppState.Logout();
            var login = new LoginWindow();
            login.Show();
            Window.GetWindow(this)?.Close();
        }
    }
}
