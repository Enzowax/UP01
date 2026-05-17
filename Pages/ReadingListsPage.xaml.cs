using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ShutIKrol.Data;
using ShutIKrol.Helpers;
using ShutIKrol.Models;

namespace ShutIKrol.Pages
{
    public partial class ReadingListsPage : Page
    {
        private bool _loaded;
        private int _currentTab = 5; // default: Want to read

        public ReadingListsPage()
        {
            InitializeComponent();
            Loaded += (_, _) =>
            {
                if (_loaded) return;
                _loaded = true;
                LoadGenres();
                LoadCurrentTab();
            };
        }

        private void LoadGenres()
        {
            GenreBox.Items.Clear();
            GenreBox.Items.Add(new ComboBoxItem { Content = "Все жанры", Tag = 0 });
            foreach (var g in DB.GetGenres())
                GenreBox.Items.Add(new ComboBoxItem { Content = g.Name, Tag = g.Id });
            GenreBox.SelectedIndex = 0;
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_loaded) return;
            if (sender is TabControl tc && tc.SelectedItem is TabItem ti &&
                ti.Tag is string tagStr && int.TryParse(tagStr, out int sid))
            {
                _currentTab = sid;
                LoadCurrentTab();
            }
        }

        private void LoadCurrentTab()
        {
            string search = SearchBox.Text.Trim();
            int genreId = 0;
            if (GenreBox.SelectedItem is ComboBoxItem ci && ci.Tag is int gid) genreId = gid;
            string sort = SortBox.SelectedIndex == 1 ? "rating" : "name";

            var books = DB.GetReadList(AppState.CurrentUser!.Id, _currentTab, search, genreId, sort);
            var panel = _currentTab switch
            {
                5 => PanelWant,
                6 => PanelReading,
                7 => PanelFinished,
                _ => PanelDropped
            };
            panel.Children.Clear();
            foreach (var b in books)
                panel.Children.Add(CreateCard(b));
        }

        private Border CreateCard(Book b)
        {
            var card = new Border
            {
                Width = 160, Margin = new Thickness(8),
                Background = new SolidColorBrush(Color.FromRgb(0x31, 0x32, 0x44)),
                CornerRadius = new CornerRadius(10)
            };
            var stack = new StackPanel { Margin = new Thickness(10) };

            var cover = new Border { Height = 100, Background = new SolidColorBrush(Color.FromRgb(0x45, 0x47, 0x5A)), CornerRadius = new CornerRadius(6), Margin = new Thickness(0, 0, 0, 6) };
            cover.Child = new TextBlock { Text = "📚", FontSize = 36, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            stack.Children.Add(cover);

            stack.Children.Add(new TextBlock { Text = b.Name, Foreground = new SolidColorBrush(Color.FromRgb(0xCD, 0xD6, 0xF4)), FontWeight = FontWeights.Bold, FontSize = 12, TextWrapping = TextWrapping.Wrap });
            stack.Children.Add(new TextBlock { Text = b.AuthorName, Foreground = new SolidColorBrush(Color.FromRgb(0xA6, 0xAD, 0xC8)), FontSize = 11, Margin = new Thickness(0, 2, 0, 4) });
            stack.Children.Add(new TextBlock { Text = $"⭐ {b.AvgRating:F1}", Foreground = new SolidColorBrush(Color.FromRgb(0xF9, 0xE2, 0xAF)), FontSize = 12 });

            // Buttons
            var openBtn = MakeBtn("Открыть", "#89B4FA");
            openBtn.Click += (_, _) =>
            {
                if (Window.GetWindow(this) is MainWindow mw)
                    mw.NavigateTo(new BookPage(b.Id));
            };

            var moveBtn = MakeBtn("Переместить", "#A6E3A1");
            moveBtn.Margin = new Thickness(0, 4, 0, 0);
            moveBtn.Click += (_, _) => ShowMoveMenu(moveBtn, b.Id);

            var removeBtn = MakeBtn("Убрать", "#F38BA8");
            removeBtn.Margin = new Thickness(0, 4, 0, 0);
            removeBtn.Click += (_, _) =>
            {
                DB.RemoveFromReadList(AppState.CurrentUser!.Id, b.Id);
                LoadCurrentTab();
            };

            stack.Children.Add(openBtn);
            stack.Children.Add(moveBtn);
            stack.Children.Add(removeBtn);
            card.Child = stack;
            return card;
        }

        private Button MakeBtn(string text, string color)
        {
            var btn = new Button
            {
                Content = text, FontSize = 11, BorderThickness = new Thickness(0),
                Padding = new Thickness(6, 3, 6, 3), Cursor = System.Windows.Input.Cursors.Hand,
                FontWeight = FontWeights.SemiBold
            };
            var c = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(color);
            btn.Background = new SolidColorBrush(c);
            btn.Foreground = new SolidColorBrush(Color.FromRgb(0x1E, 0x1E, 0x2E));
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

        private void ShowMoveMenu(Button btn, int bookId)
        {
            var menu = new ContextMenu();
            var statuses = new[] { (5, "📌 В планах"), (6, "📖 Читаю"), (7, "✅ Прочитано"), (8, "🗑 Заброшено") };
            foreach (var (sid, name) in statuses)
            {
                if (sid == _currentTab) continue;
                var item = new MenuItem { Header = name };
                int statusId = sid;
                item.Click += (_, _) =>
                {
                    DB.SetReadListStatus(AppState.CurrentUser!.Id, bookId, statusId);
                    LoadCurrentTab();
                };
                menu.Items.Add(item);
            }
            btn.ContextMenu = menu;
            menu.IsOpen = true;
        }

        private void Filter_Changed(object sender, EventArgs e)
        {
            if (_loaded) LoadCurrentTab();
        }
    }
}
