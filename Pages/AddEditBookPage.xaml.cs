using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ShutIKrol.Data;
using ShutIKrol.Helpers;
using ShutIKrol.Models;

namespace ShutIKrol.Pages
{
    public partial class AddEditBookPage : Page
    {
        private readonly int? _bookId;
        private Book? _book;
        private List<Chapter> _chapters = new();
        private int _nextChapterNum = 1;

        public AddEditBookPage(int? bookId)
        {
            _bookId = bookId;
            InitializeComponent();
            Loaded += AddEditBookPage_Loaded;
        }

        private void AddEditBookPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadGenres();
            if (_bookId.HasValue)
            {
                PageTitle.Text = "Редактировать книгу";
                _book = DB.GetBook(_bookId.Value);
                if (_book != null)
                {
                    NameBox.Text = _book.Name;
                    CoverBox.Text = _book.CoverPath;
                    _chapters = DB.GetChapters(_bookId.Value);
                    _nextChapterNum = _chapters.Count > 0 ? _chapters.Max(c => c.Number) + 1 : 1;
                    RefreshChaptersList();
                    // Pre-select genres
                    var bookGenreNames = _book.Genres;
                    foreach (ListBoxItem item in GenresList.Items)
                        if (bookGenreNames.Contains(item.Content?.ToString() ?? ""))
                            item.IsSelected = true;
                }
            }
        }

        private void LoadGenres()
        {
            GenresList.Items.Clear();
            foreach (var g in DB.GetGenres())
                GenresList.Items.Add(new ListBoxItem { Content = g.Name, Tag = g.Id, Foreground = System.Windows.Media.Brushes.White });
        }

        private void RefreshChaptersList()
        {
            ChaptersControl.ItemsSource = null;
            ChaptersControl.ItemsSource = _chapters;
        }

        private void AddChapterBtn_Click(object sender, RoutedEventArgs e)
        {
            string name = ChNameBox.Text.Trim();
            string path = ChPathBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(name)) { ErrorText.Text = "Введите название главы."; return; }
            if (string.IsNullOrWhiteSpace(path)) path = $"/chapters/{_bookId ?? 0}/ch{_nextChapterNum}.txt";

            if (_bookId.HasValue)
            {
                int id = DB.AddChapter(_bookId.Value, _nextChapterNum, name, path);
                _chapters.Add(new Chapter { Id = id, BookId = _bookId.Value, Number = _nextChapterNum, Name = name, Path = path });
            }
            else
            {
                _chapters.Add(new Chapter { Id = 0, BookId = 0, Number = _nextChapterNum, Name = name, Path = path });
            }
            _nextChapterNum++;
            ChNameBox.Clear(); ChPathBox.Clear();
            RefreshChaptersList();
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            ErrorText.Text = "";
            string name = NameBox.Text.Trim();
            string cover = CoverBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(name)) { ErrorText.Text = "Введите название книги."; return; }
            if (string.IsNullOrWhiteSpace(cover)) cover = "/covers/default.jpg";

            var selectedGenreIds = GenresList.SelectedItems.Cast<ListBoxItem>()
                .Where(i => i.Tag is int).Select(i => (int)i.Tag!).ToList();

            try
            {
                if (_bookId.HasValue)
                {
                    DB.UpdateBook(_bookId.Value, name, cover, selectedGenreIds);
                }
                else
                {
                    int newId = DB.AddBook(name, cover, AppState.CurrentUser!.Id, selectedGenreIds);
                    // Add pending chapters
                    int num = 1;
                    foreach (var ch in _chapters)
                        DB.AddChapter(newId, num++, ch.Name, ch.Path.Replace("/0/", $"/{newId}/"));
                }
                MessageBox.Show("Книга сохранена!", "OK", MessageBoxButton.OK, MessageBoxImage.Information);
                if (Window.GetWindow(this) is MainWindow mw)
                    mw.NavigateTo(new AuthorPage());
            }
            catch (Exception ex)
            {
                ErrorText.Text = "Ошибка: " + ex.Message;
            }
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow mw)
                mw.NavigateTo(new AuthorPage());
        }
    }
}
