using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ShutIKrol.Data;
using ShutIKrol.Models;

namespace ShutIKrol.Pages
{
    public partial class AdminPage : Page
    {
        private int _authorRoleTypeId;
        private int _bookUnfreezeTypeId;
        private int _accountUnfreezeTypeId;

        public AdminPage()
        {
            InitializeComponent();
            Loaded += (_, _) => LoadAll();
        }

        private void LoadAll()
        {
            _authorRoleTypeId   = DB.EnsureRequestType("AuthorRoleRequest");
            _bookUnfreezeTypeId = DB.EnsureRequestType("BookUnfrozing");
            _accountUnfreezeTypeId = DB.EnsureRequestType("AccountUnfrozing");

            RefreshAllTabs();
        }

        private void RefreshAllTabs()
        {
            ComplaintsGrid.ItemsSource = DB.GetComplaints();
            UnfreezeGrid.ItemsSource = DB.GetRequests(_bookUnfreezeTypeId).Concat(DB.GetRequests(_accountUnfreezeTypeId)).ToList();
            AuthorReqGrid.ItemsSource = DB.GetRequests(_authorRoleTypeId);
            FrozenBooksGrid.ItemsSource = DB.GetFrozenBooks();
            FrozenUsersGrid.ItemsSource = DB.GetFrozenUsers();
            UsersGrid.ItemsSource = DB.GetUsers();
        }

        // Tabs data loaded in LoadAll()

        // ── Complaints ──────────────────────────────────────────────────────

        private void AcceptComplaint_Click(object sender, RoutedEventArgs e)
        {
            if (ComplaintsGrid.SelectedItem is not Complaint c) { MessageBox.Show("Выберите жалобу."); return; }
            if (c.BookId.HasValue)
                DB.FreezeBook(c.BookId.Value, true);
            else if (c.ReviewId.HasValue)
                DB.DeleteReview(c.ReviewId.Value);
            DB.DeleteComplaint(c.Id);
            RefreshAllTabs();
            MessageBox.Show("Жалоба принята. Объект заморожен/удалён.", "OK", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void RejectComplaint_Click(object sender, RoutedEventArgs e)
        {
            if (ComplaintsGrid.SelectedItem is not Complaint c) { MessageBox.Show("Выберите жалобу."); return; }
            if (MessageBox.Show("Отклонить и удалить жалобу?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                DB.DeleteComplaint(c.Id);
                RefreshAllTabs();
            }
        }

        private void RefreshComplaints_Click(object sender, RoutedEventArgs e) => ComplaintsGrid.ItemsSource = DB.GetComplaints();

        // ── Unfreeze Requests ───────────────────────────────────────────────

        private void AcceptUnfreeze_Click(object sender, RoutedEventArgs e)
        {
            if (UnfreezeGrid.SelectedItem is not Request req) { MessageBox.Show("Выберите заявку."); return; }
            if (req.TypeId == _bookUnfreezeTypeId)
            {
                // Extract bookId from comment
                int bookId = ExtractBookId(req.Comment);
                if (bookId > 0) DB.FreezeBook(bookId, false);
                else MessageBox.Show("Не удалось определить ID книги из заявки.");
            }
            else if (req.TypeId == _accountUnfreezeTypeId)
            {
                DB.FreezeUser(req.UserId, false);
            }
            DB.DeleteRequest(req.Id);
            RefreshAllTabs();
            MessageBox.Show("Заявка принята. Объект разморожен.", "OK", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void RejectUnfreeze_Click(object sender, RoutedEventArgs e)
        {
            if (UnfreezeGrid.SelectedItem is not Request req) { MessageBox.Show("Выберите заявку."); return; }
            DB.DeleteRequest(req.Id);
            RefreshAllTabs();
            MessageBox.Show("Заявка отклонена.", "OK", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void RefreshUnfreeze_Click(object sender, RoutedEventArgs e)
        {
            UnfreezeGrid.ItemsSource = DB.GetRequests(_bookUnfreezeTypeId).Concat(DB.GetRequests(_accountUnfreezeTypeId)).ToList();
        }

        // ── Author Role Requests ────────────────────────────────────────────

        private void AcceptAuthorReq_Click(object sender, RoutedEventArgs e)
        {
            if (AuthorReqGrid.SelectedItem is not Request req) { MessageBox.Show("Выберите заявку."); return; }
            DB.SetUserRole(req.UserId, 5); // Author role
            DB.DeleteRequest(req.Id);
            RefreshAllTabs();
            MessageBox.Show($"Пользователь {req.UserName} получил роль Автора.", "OK", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void RejectAuthorReq_Click(object sender, RoutedEventArgs e)
        {
            if (AuthorReqGrid.SelectedItem is not Request req) { MessageBox.Show("Выберите заявку."); return; }
            DB.DeleteRequest(req.Id);
            RefreshAllTabs();
            MessageBox.Show("Заявка отклонена.", "OK", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void RefreshAuthorReq_Click(object sender, RoutedEventArgs e) => AuthorReqGrid.ItemsSource = DB.GetRequests(_authorRoleTypeId);

        // ── Frozen Items ────────────────────────────────────────────────────

        private void UnfreezeBook_Click(object sender, RoutedEventArgs e)
        {
            if (FrozenBooksGrid.SelectedItem is not Book b) { MessageBox.Show("Выберите книгу."); return; }
            DB.FreezeBook(b.Id, false);
            FrozenBooksGrid.ItemsSource = DB.GetFrozenBooks();
            MessageBox.Show("Книга разморожена.", "OK", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void UnfreezeUser_Click(object sender, RoutedEventArgs e)
        {
            if (FrozenUsersGrid.SelectedItem is not User u) { MessageBox.Show("Выберите пользователя."); return; }
            DB.FreezeUser(u.Id, false);
            FrozenUsersGrid.ItemsSource = DB.GetFrozenUsers();
            MessageBox.Show("Пользователь разморожен.", "OK", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // ── Users Management ────────────────────────────────────────────────

        private void RefreshUsers_Click(object sender, RoutedEventArgs e) => UsersGrid.ItemsSource = DB.GetUsers();

        private void SetRole_Click(object sender, RoutedEventArgs e)
        {
            if (UsersGrid.SelectedItem is not User u) { MessageBox.Show("Выберите пользователя."); return; }
            var menu = new ContextMenu();
            var roles = new[] { (4, "Admin"), (5, "Author"), (6, "Reader") };
            foreach (var (rid, rname) in roles)
            {
                var item = new MenuItem { Header = rname };
                int roleId = rid;
                item.Click += (_, _) =>
                {
                    DB.SetUserRole(u.Id, roleId);
                    UsersGrid.ItemsSource = DB.GetUsers();
                    MessageBox.Show($"Роль {rname} назначена.", "OK", MessageBoxButton.OK, MessageBoxImage.Information);
                };
                menu.Items.Add(item);
            }
            (sender as Button)!.ContextMenu = menu;
            menu.IsOpen = true;
        }

        private void FreezeUser_Click(object sender, RoutedEventArgs e)
        {
            if (UsersGrid.SelectedItem is not User u) { MessageBox.Show("Выберите пользователя."); return; }
            DB.FreezeUser(u.Id, true);
            UsersGrid.ItemsSource = DB.GetUsers();
            MessageBox.Show($"Пользователь {u.Name} заморожен.", "OK", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void UnfreezeUserInList_Click(object sender, RoutedEventArgs e)
        {
            if (UsersGrid.SelectedItem is not User u) { MessageBox.Show("Выберите пользователя."); return; }
            DB.FreezeUser(u.Id, false);
            UsersGrid.ItemsSource = DB.GetUsers();
            MessageBox.Show($"Пользователь {u.Name} разморожен.", "OK", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            if (UsersGrid.SelectedItem is not User u) { MessageBox.Show("Выберите пользователя."); return; }
            var dlg = new ChangePasswordDialog();
            if (dlg.ShowDialog() == true && !string.IsNullOrWhiteSpace(dlg.NewPassword))
            {
                DB.ChangePassword(u.Id, dlg.NewPassword);
                MessageBox.Show("Пароль изменён.", "OK", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private static int ExtractBookId(string comment)
        {
            try
            {
                int start = comment.IndexOf("[BookId:") + 8;
                int end = comment.IndexOf("]", start);
                if (start > 7 && end > start)
                    return int.Parse(comment.Substring(start, end - start).Trim());
            }
            catch { }
            return 0;
        }
    }
}
