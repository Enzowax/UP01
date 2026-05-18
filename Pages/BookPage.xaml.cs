using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ShutIKrol.Data;
using ShutIKrol.Helpers;
using ShutIKrol.Models;

namespace ShutIKrol.Pages
{
    public partial class BookPage : Page
    {
        private readonly int _bookId;
        private Book? _book;

        public BookPage(int bookId)
        {
            _bookId = bookId;
            InitializeComponent();
            Loaded += BookPage_Loaded;
        }

        private void BookPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadBook();
        }

        private void LoadBook()
        {
            _book = DB.GetBook(_bookId);
            if (_book == null) { MessageBox.Show("Книга не найдена."); return; }

            TitleText.Text = _book.Name;
            AuthorText.Text = $"Автор: {_book.AuthorName}";
            GenresText.Text = "Жанры: " + (_book.Genres.Count > 0 ? string.Join(", ", _book.Genres) : "—");
            RatingText.Text = $"⭐ Средняя оценка: {_book.AvgRating:F1} / 10";

            LoadCover(_book.CoverPath);

            FrozenBanner.Visibility = _book.IsFrozen ? Visibility.Visible : Visibility.Collapsed;

            // Admin buttons
            if (AppState.IsAdmin)
            {
                FreezeBookBtn.Visibility = Visibility.Visible;
                FreezeBookBtn.Content = _book.IsFrozen ? "🔓 Разморозить книгу" : "❄ Заморозить книгу";
            }

            // Rating ComboBox
            RatingBox.Items.Clear();
            for (int i = 1; i <= 10; i++) RatingBox.Items.Add(i);
            RatingBox.SelectedIndex = 9;

            // Hide review form if already reviewed or frozen user
            if (AppState.IsFrozen || DB.HasReview(AppState.CurrentUser!.Id, _bookId))
                ReviewForm.Visibility = Visibility.Collapsed;

            LoadChapters();
            LoadReviews();
        }

        private void LoadCover(string coverPath)
        {
            if (string.IsNullOrWhiteSpace(coverPath)) return;
            try
            {
                string full = System.IO.Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    coverPath.TrimStart('/').Replace('/', System.IO.Path.DirectorySeparatorChar));

                if (!System.IO.File.Exists(full)) return;

                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.UriSource = new Uri(full);
                bmp.EndInit();
                bmp.Freeze();

                CoverBorder.Background = new ImageBrush(bmp) { Stretch = Stretch.UniformToFill };
                CoverPlaceholder.Visibility = Visibility.Collapsed;
            }
            catch { }
        }

        private void LoadChapters()
        {
            var chapters = DB.GetChapters(_bookId);
            ChaptersList.ItemsSource = chapters;
        }

        private void LoadReviews()
        {
            ReviewsList.ItemsSource = DB.GetReviews(_bookId);
        }

        private void BackBtn_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow mw)
                mw.NavigateTo(new CatalogPage());
        }

        private void ReadChapter_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Chapter ch)
            {
                var win = new ChapterReadWindow(ch);
                win.ShowDialog();
            }
        }

        private void AddToListBtn_Click(object sender, RoutedEventArgs e)
        {
            if (AppState.IsFrozen) { MessageBox.Show("Ваш аккаунт заморожен."); return; }
            var menu = new ContextMenu();
            var statuses = new[] { (5, "📌 В планах"), (6, "📖 Читаю"), (7, "✅ Прочитано"), (8, "🗑 Заброшено") };
            foreach (var (sid, name) in statuses)
            {
                var item = new MenuItem { Header = name };
                int statusId = sid;
                item.Click += (_, _) =>
                {
                    DB.SetReadListStatus(AppState.CurrentUser!.Id, _bookId, statusId);
                    MessageBox.Show("Добавлено в список!", "OK", MessageBoxButton.OK, MessageBoxImage.Information);
                };
                menu.Items.Add(item);
            }
            AddToListBtn.ContextMenu = menu;
            menu.IsOpen = true;
        }

        private void ComplainBookBtn_Click(object sender, RoutedEventArgs e)
        {
            if (AppState.IsFrozen) { MessageBox.Show("Ваш аккаунт заморожен."); return; }
            var dlg = new ComplaintDialog("Жалоба на книгу");
            if (dlg.ShowDialog() == true && !string.IsNullOrWhiteSpace(dlg.Reason))
            {
                DB.AddComplaint(AppState.CurrentUser!.Id, _bookId, null, dlg.Reason);
                MessageBox.Show("Жалоба отправлена.", "OK", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ComplainAuthorBtn_Click(object sender, RoutedEventArgs e)
        {
            if (AppState.IsFrozen) { MessageBox.Show("Ваш аккаунт заморожен."); return; }
            var dlg = new ComplaintDialog("Жалоба на автора");
            if (dlg.ShowDialog() == true && !string.IsNullOrWhiteSpace(dlg.Reason))
            {
                DB.AddComplaint(AppState.CurrentUser!.Id, null, null, $"[Автор: {_book?.AuthorName}] " + dlg.Reason);
                MessageBox.Show("Жалоба отправлена.", "OK", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void FreezeBookBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_book == null) return;
            bool newState = !_book.IsFrozen;
            DB.FreezeBook(_bookId, newState);
            LoadBook();
        }

        private void ComplainReview_Click(object sender, RoutedEventArgs e)
        {
            if (AppState.IsFrozen) { MessageBox.Show("Ваш аккаунт заморожен."); return; }
            if (sender is Button btn && btn.Tag is int reviewId)
            {
                var dlg = new ComplaintDialog("Жалоба на отзыв");
                if (dlg.ShowDialog() == true && !string.IsNullOrWhiteSpace(dlg.Reason))
                {
                    DB.AddComplaint(AppState.CurrentUser!.Id, null, reviewId, dlg.Reason);
                    MessageBox.Show("Жалоба на отзыв отправлена.", "OK", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void DeleteReview_Click(object sender, RoutedEventArgs e)
        {
            if (!AppState.IsAdmin) return;
            if (sender is Button btn && btn.Tag is int reviewId)
            {
                if (MessageBox.Show("Удалить отзыв?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    DB.DeleteReview(reviewId);
                    LoadReviews();
                }
            }
        }

        private void SubmitReview_Click(object sender, RoutedEventArgs e)
        {
            string text = ReviewText.Text.Trim();
            if (string.IsNullOrWhiteSpace(text)) { MessageBox.Show("Введите текст отзыва."); return; }
            int rate = RatingBox.SelectedItem != null ? (int)RatingBox.SelectedItem : 5;
            DB.AddReview(AppState.CurrentUser!.Id, _bookId, text, rate);
            ReviewText.Clear();
            ReviewForm.Visibility = Visibility.Collapsed;
            LoadReviews();
            MessageBox.Show("Отзыв добавлен!", "OK", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
