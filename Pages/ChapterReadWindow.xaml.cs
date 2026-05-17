using System;
using System.IO;
using System.Windows;
using ShutIKrol.Models;

namespace ShutIKrol.Pages
{
    public partial class ChapterReadWindow : Window
    {
        public ChapterReadWindow(Chapter chapter)
        {
            InitializeComponent();
            Title = $"Глава {chapter.Number}: {chapter.Name}";
            ChapterTitle.Text = $"Глава {chapter.Number}: {chapter.Name}";

            string text = LoadChapterText(chapter.Path);
            ChapterText.Text = text;
        }

        private string LoadChapterText(string relativePath)
        {
            try
            {
                // Try to load from app directory
                string full = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                    relativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (File.Exists(full))
                    return File.ReadAllText(full, System.Text.Encoding.UTF8);
            }
            catch { }

            // Placeholder text
            return "Текст главы недоступен.\n\n" +
                   "Разместите файлы глав в папке приложения по указанному пути, " +
                   "либо обратитесь к автору книги для получения контента.\n\n" +
                   "Формат хранения: текстовые файлы (.txt) в кодировке UTF-8.\n\n" +
                   "────────────────────────────────────────\n\n" +
                   "Пример содержания главы:\n\n" +
                   "Жил-был маленький автор, который очень хотел, " +
                   "чтобы его книгу читали все вокруг. Однажды он узнал о платформе " +
                   "«Читай, Пиши и не спиши» и решил опубликовать своё первое произведение...";
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();
    }
}
