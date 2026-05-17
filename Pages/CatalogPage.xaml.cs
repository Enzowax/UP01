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
    public partial class CatalogPage : Page
    {
        private List<Genre> _genres = new();
        private bool _loaded;

        public CatalogPage()
        {
            InitializeComponent();
            Loaded += CatalogPage_Loaded;
        }

        private void CatalogPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (_loaded) return;
            _loaded = true;
            ShowFrozen.Visibility = AppState.IsAdmin ? Visibility.Visible : Visibility.Collapsed;
            LoadGenres();
            LoadBooks();
        }

        private void LoadGenres()
        {
            _genres = DB.GetGenres();
            GenreBox.Items.Clear();
            var all = new ComboBoxItem { Content = "Все жанры", Tag = 0 };
            GenreBox.Items.Add(all);
            foreach (var g in _genres)
                GenreBox.Items.Add(new ComboBoxItem { Content = g.Name, Tag = g.Id });
            GenreBox.SelectedIndex = 0;
        }

        private void LoadBooks()
        {
            string search = SearchBox.Text.Trim();
            int genreId = 0;
            if (GenreBox.SelectedItem is ComboBoxItem ci && ci.Tag is int gid) genreId = gid;
            string sort = SortBox.SelectedIndex == 1 ? "rating" : "name";
            bool includeAll = AppState.IsAdmin && ShowFrozen.IsChecked == true;

            List<Book> books;
            try { books = DB.GetBooks(search, genreId, sort, includeAll); }
            catch { books = new List<Book>(); }

            BooksPanel.Children.Clear();
            foreach (var b in books)
                BooksPanel.Children.Add(CreateBookCard(b));
        }

        private Border CreateBookCard(Book b)
        {
            var card = new Border
            {
                Width = 160, Margin = new Thickness(8),
                Background = new SolidColorBrush(Color.FromRgb(0x31, 0x32, 0x44)),
                CornerRadius = new CornerRadius(10),
                Cursor = System.Windows.Input.Cursors.Hand
            };

            var stack = new StackPanel { Margin = new Thickness(10) };

            // Cover placeholder
            var cover = new Border
            {
                Height = 110, Background = new SolidColorBrush(Color.FromRgb(0x45, 0x47, 0x5A)),
                CornerRadius = new CornerRadius(6), Margin = new Thickness(0, 0, 0, 8)
            };
            try
            {
                string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, b.CoverPath.TrimStart('/').Replace('/', '\\'));
                if (System.IO.File.Exists(path))
                {
                    var img = new Image { Stretch = Stretch.UniformToFill };
                    img.Source = new BitmapImage(new Uri(path));
                    cover.Child = img;
                }
                else
                    cover.Child = new TextBlock { Text = "📚", FontSize = 40, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            }
            catch
            {
                cover.Child = new TextBlock { Text = "📚", FontSize = 40, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            }
            stack.Children.Add(cover);

            // Frozen badge
            if (b.IsFrozen)
            {
                var badge = new Border { Background = new SolidColorBrush(Color.FromRgb(0xF3, 0x8B, 0xA8)), CornerRadius = new CornerRadius(4), Margin = new Thickness(0, 0, 0, 4), Padding = new Thickness(4, 2, 4, 2) };
                badge.Child = new TextBlock { Text = "❄ Заморожена", Foreground = Brushes.White, FontSize = 10 };
                stack.Children.Add(badge);
            }

            stack.Children.Add(new TextBlock { Text = b.Name, Foreground = new SolidColorBrush(Color.FromRgb(0xCD, 0xD6, 0xF4)), FontWeight = FontWeights.Bold, FontSize = 13, TextWrapping = TextWrapping.Wrap });
            stack.Children.Add(new TextBlock { Text = b.AuthorName, Foreground = new SolidColorBrush(Color.FromRgb(0xA6, 0xAD, 0xC8)), FontSize = 11, Margin = new Thickness(0, 2, 0, 2) });
            stack.Children.Add(new TextBlock { Text = $"⭐ {b.AvgRating:F1}", Foreground = new SolidColorBrush(Color.FromRgb(0xF9, 0xE2, 0xAF)), FontSize = 12 });

            // Buttons row
            var btnRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 8, 0, 0) };
            var openBtn = CreateButton("Открыть", "#89B4FA");
            openBtn.Click += (_, _) => OpenBook(b.Id);
            btnRow.Children.Add(openBtn);

            var addBtn = CreateButton("+ Список", "#A6E3A1");
            addBtn.Margin = new Thickness(4, 0, 0, 0);
            addBtn.Click += (_, _) => ShowAddToListMenu(addBtn, b.Id);
            btnRow.Children.Add(addBtn);

            stack.Children.Add(btnRow);
            card.Child = stack;
            card.MouseLeftButtonUp += (_, _) => OpenBook(b.Id);
            return card;
        }

        private Button CreateButton(string text, string color)
        {
            var btn = new Button { FontSize = 11, Cursor = System.Windows.Input.Cursors.Hand, BorderThickness = new Thickness(0), Padding = new Thickness(6, 3, 6, 3) };
            var bc = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(color);
            btn.Background = new SolidColorBrush(bc);
            btn.Foreground = new SolidColorBrush(Color.FromRgb(0x1E, 0x1E, 0x2E));
            btn.FontWeight = FontWeights.SemiBold;
            btn.Content = text;
            var tpl = new ControlTemplate(typeof(Button));
            var bd = new FrameworkElementFactory(typeof(Border));
            bd.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(Button.BackgroundProperty));
            bd.SetValue(Border.CornerRadiusProperty, new CornerRadius(4));
            bd.SetValue(Border.PaddingProperty, new TemplateBindingExtension(Button.PaddingProperty));
            var cp = new FrameworkElementFactory(typeof(ContentPresenter));
            cp.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            cp.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
            bd.AppendChild(cp);
            tpl.VisualTree = bd;
            btn.Template = tpl;
            return btn;
        }

        private void OpenBook(int id)
        {
            if (Window.GetWindow(this) is MainWindow mw)
                mw.NavigateTo(new BookPage(id));
        }

        private void ShowAddToListMenu(Button btn, int bookId)
        {
            var menu = new ContextMenu();
            var statuses = new[] { (5, "📌 В планах"), (6, "📖 Читаю"), (7, "✅ Прочитано"), (8, "🗑 Заброшено") };
            foreach (var (sid, name) in statuses)
            {
                var item = new MenuItem { Header = name };
                int statusId = sid;
                item.Click += (_, _) =>
                {
                    DB.SetReadListStatus(AppState.CurrentUser!.Id, bookId, statusId);
                    MessageBox.Show("Книга добавлена в список!", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
                };
                menu.Items.Add(item);
            }
            btn.ContextMenu = menu;
            menu.IsOpen = true;
        }

        private void Filter_Changed(object sender, EventArgs e)
        {
            if (_loaded) LoadBooks();
        }
    }
}
