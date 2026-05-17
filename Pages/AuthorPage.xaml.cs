using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ShutIKrol.Data;
using ShutIKrol.Helpers;
using ShutIKrol.Models;

namespace ShutIKrol.Pages
{
    public partial class AuthorPage : Page
    {
        public AuthorPage()
        {
            InitializeComponent();
            Loaded += (_, _) => LoadBooks();
        }

        private void LoadBooks()
        {
            var all = DB.GetBooksByAuthor(AppState.CurrentUser!.Id, true);
            PublishedPanel.Children.Clear();
            FrozenPanel.Children.Clear();
            foreach (var b in all)
            {
                if (b.IsFrozen)
                    FrozenPanel.Children.Add(CreateCard(b, frozen: true));
                else
                    PublishedPanel.Children.Add(CreateCard(b, frozen: false));
            }
        }

        private Border CreateCard(Book b, bool frozen)
        {
            var card = new Border
            {
                Width = 180, Margin = new Thickness(8),
                Background = new SolidColorBrush(Color.FromRgb(0x31, 0x32, 0x44)),
                CornerRadius = new CornerRadius(10)
            };
            var stack = new StackPanel { Margin = new Thickness(12) };

            var cover = new Border { Height = 110, Background = new SolidColorBrush(Color.FromRgb(0x45, 0x47, 0x5A)), CornerRadius = new CornerRadius(6), Margin = new Thickness(0, 0, 0, 8) };
            cover.Child = new TextBlock { Text = "📚", FontSize = 40, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            stack.Children.Add(cover);

            stack.Children.Add(new TextBlock { Text = b.Name, Foreground = new SolidColorBrush(Color.FromRgb(0xCD, 0xD6, 0xF4)), FontWeight = FontWeights.Bold, FontSize = 13, TextWrapping = TextWrapping.Wrap });
            stack.Children.Add(new TextBlock { Text = $"⭐ {b.AvgRating:F1}", Foreground = new SolidColorBrush(Color.FromRgb(0xF9, 0xE2, 0xAF)), FontSize = 12, Margin = new Thickness(0, 4, 0, 8) });

            if (frozen)
            {
                // Appeal button
                var appealBtn = MakeBtn("📝 Оспорить", "#89DCEB");
                appealBtn.Click += (_, _) =>
                {
                    int typeId = DB.EnsureRequestType("BookUnfrozing");
                    if (DB.HasPendingRequest(AppState.CurrentUser!.Id, typeId))
                    { MessageBox.Show("Заявка уже подана."); return; }
                    var dlg = new ComplaintDialog("Заявка на разморозку книги");
                    if (dlg.ShowDialog() == true)
                    {
                        DB.AddRequest(typeId, AppState.CurrentUser!.Id, $"[BookId:{b.Id}] " + dlg.Reason);
                        MessageBox.Show("Заявка отправлена администратору.");
                    }
                };
                stack.Children.Add(appealBtn);
            }
            else
            {
                // Edit button
                var editBtn = MakeBtn("✏ Редактировать", "#89B4FA");
                editBtn.Click += (_, _) =>
                {
                    if (Window.GetWindow(this) is MainWindow mw)
                        mw.NavigateTo(new AddEditBookPage(b.Id));
                };
                stack.Children.Add(editBtn);

                // View button
                var viewBtn = MakeBtn("👁 Просмотр", "#A6ADC8");
                viewBtn.Margin = new Thickness(0, 4, 0, 0);
                viewBtn.Click += (_, _) =>
                {
                    if (Window.GetWindow(this) is MainWindow mw)
                        mw.NavigateTo(new BookPage(b.Id));
                };
                stack.Children.Add(viewBtn);
            }
            card.Child = stack;
            return card;
        }

        private Button MakeBtn(string text, string color)
        {
            var btn = new Button
            {
                Content = text, FontSize = 11, BorderThickness = new Thickness(0),
                Padding = new Thickness(8, 4, 8, 4), Cursor = System.Windows.Input.Cursors.Hand,
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

        private void AddBookBtn_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow mw)
                mw.NavigateTo(new AddEditBookPage(null));
        }
    }
}
