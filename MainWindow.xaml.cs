using System.Windows;
using ShutIKrol.Helpers;
using ShutIKrol.Pages;

namespace ShutIKrol
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            SetupSidebar();
            NavigateTo(new CatalogPage());
        }

        private void SetupSidebar()
        {
            var u = AppState.CurrentUser!;
            BtnAuthor.Visibility = (u.RoleId == 5) ? Visibility.Visible : Visibility.Collapsed;
            BtnAdmin.Visibility  = (u.RoleId == 4) ? Visibility.Visible : Visibility.Collapsed;
            BtnFrozen.Visibility = u.IsFrozen       ? Visibility.Visible : Visibility.Collapsed;
        }

        public void NavigateTo(System.Windows.Controls.Page page)
        {
            MainFrame.Navigate(page);
        }

        public void RefreshSidebar()
        {
            SetupSidebar();
        }

        private void BtnCatalog_Click(object sender, RoutedEventArgs e) => NavigateTo(new CatalogPage());
        private void BtnLists_Click(object sender, RoutedEventArgs e)   => NavigateTo(new ReadingListsPage());
        private void BtnAuthor_Click(object sender, RoutedEventArgs e)  => NavigateTo(new AuthorPage());
        private void BtnAdmin_Click(object sender, RoutedEventArgs e)   => NavigateTo(new AdminPage());
        private void BtnProfile_Click(object sender, RoutedEventArgs e) => NavigateTo(new ProfilePage());
    }
}
