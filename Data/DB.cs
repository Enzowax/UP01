using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ShutIKrol.Data.Entities;
using ShutIKrol.Models;

namespace ShutIKrol.Data
{
    /// <summary>
    /// Статический фасад к базе данных.
    /// Все методы создают короткоживущий AppDbContext,
    /// выполняют LINQ-запрос и возвращают DTO-модели (Models.*).
    /// </summary>
    public static class DB
    {
        public static string ConnectionString =
            "Data Source=.\\SQLEXPRESS;Initial Catalog=UP_ShutIKrol;" +
            "Integrated Security=True;TrustServerCertificate=True;";

        // Фабрика контекста — вызывается в каждом методе
        private static AppDbContext Ctx() =>
            new AppDbContext(
                new DbContextOptionsBuilder<AppDbContext>()
                    .UseSqlServer(ConnectionString)
                    .Options);

        // ── Маппинг entity → model ───────────────────────────────────────────

        private static User MapUser(UserEntity e) => new User
        {
            Id = e.Id, Name = e.Name, Login = e.Login, Password = e.Password,
            Email = e.Email, RoleId = e.RoleId, RoleName = e.Role?.Name ?? "",
            IsFrozen = e.IsFrozen, RegistrationDate = e.RegistrationDate
        };

        private static Book MapBook(BookEntity e) => new Book
        {
            Id       = e.Id,
            Name     = e.Name,
            CoverPath = e.CoverPath,
            AuthorId  = e.AuthorId,
            AuthorName = e.Author?.Name ?? "",
            IsFrozen  = e.IsFrozen,
            AvgRating = e.Reviews.Any()
                            ? e.Reviews.Average(r => (double)r.Rate)
                            : 0.0,
            Genres = e.BookGenres.Select(bg => bg.Genre.Name).ToList()
        };

        private static Review MapReview(ReviewEntity e) => new Review
        {
            Id = e.Id, UserId = e.UserId, UserName = e.User?.Name ?? "",
            BookId = e.BookId, Text = e.Text,
            Rate = e.Rate, CreationDate = e.CreationDate
        };

        private static Models.Chapter MapChapter(ChapterEntity e) => new Models.Chapter
        {
            Id = e.Id, BookId = e.BookId, Number = e.Number,
            Name = e.Name, Path = e.Path
        };

        private static Complaint MapComplaint(ComplaintEntity e) => new Complaint
        {
            Id = e.Id, UserId = e.UserId, UserName = e.User?.Name ?? "",
            BookId = e.BookId, BookName = e.Book?.Name ?? "",
            ReviewId = e.ReviewId, ReasonText = e.ReasonText
        };

        private static Request MapRequest(RequestEntity e) => new Request
        {
            Id = e.Id, TypeId = e.TypeId, TypeName = e.Type?.TypeName ?? "",
            UserId = e.UserId, UserName = e.User?.Name ?? "",
            Comment = e.Comment
        };

        // ═══════════════════════════════════════════════════════════════════
        //  AUTH
        // ═══════════════════════════════════════════════════════════════════

        public static User? Login(string login, string password)
        {
            using var ctx = Ctx();
            var e = ctx.Users
                       .Include(u => u.Role)
                       .FirstOrDefault(u => u.Login == login && u.Password == password);
            return e is null ? null : MapUser(e);
        }

        public static bool LoginExists(string login)
        {
            using var ctx = Ctx();
            return ctx.Users.Any(u => u.Login == login);
        }

        public static User Register(string name, string login, string password, string email)
        {
            using var ctx = Ctx();
            var e = new UserEntity
            {
                Name = name, Login = login, Password = password, Email = email,
                RoleId = 6, IsFrozen = false, RegistrationDate = DateTime.Now
            };
            ctx.Users.Add(e);
            ctx.SaveChanges();

            // Загружаем роль для маппинга
            ctx.Entry(e).Reference(u => u.Role).Load();
            return MapUser(e);
        }

        // ═══════════════════════════════════════════════════════════════════
        //  BOOKS
        // ═══════════════════════════════════════════════════════════════════

        public static List<Book> GetBooks(string search = "", int genreId = 0,
                                          string sortBy = "name", bool includeAll = false)
        {
            using var ctx = Ctx();
            var query = ctx.Books
                           .Include(b => b.Author)
                           .Include(b => b.Reviews)
                           .Include(b => b.BookGenres).ThenInclude(bg => bg.Genre)
                           .AsQueryable();

            if (!includeAll)
                query = query.Where(b => !b.IsFrozen);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(b => b.Name.Contains(search) ||
                                         b.Author.Name.Contains(search));

            if (genreId > 0)
                query = query.Where(b => b.BookGenres.Any(bg => bg.GenreId == genreId));

            var list = query.ToList(); // выполнить SQL, дальше — в памяти

            return (sortBy == "rating"
                        ? list.OrderByDescending(b => b.Reviews.Any()
                                  ? b.Reviews.Average(r => (double)r.Rate) : 0)
                        : list.OrderBy(b => b.Name))
                   .Select(MapBook).ToList();
        }

        public static Book? GetBook(int id)
        {
            using var ctx = Ctx();
            var e = ctx.Books
                       .Include(b => b.Author)
                       .Include(b => b.Reviews)
                       .Include(b => b.BookGenres).ThenInclude(bg => bg.Genre)
                       .FirstOrDefault(b => b.Id == id);
            return e is null ? null : MapBook(e);
        }

        public static List<string> GetBookGenreNames(int bookId)
        {
            using var ctx = Ctx();
            return ctx.BookGenres
                      .Include(bg => bg.Genre)
                      .Where(bg => bg.BookId == bookId)
                      .Select(bg => bg.Genre.Name)
                      .ToList();
        }

        public static List<Book> GetBooksByAuthor(int authorId, bool includeFrozen = true)
        {
            using var ctx = Ctx();
            var list = ctx.Books
                          .Include(b => b.Author)
                          .Include(b => b.Reviews)
                          .Include(b => b.BookGenres).ThenInclude(bg => bg.Genre)
                          .Where(b => b.AuthorId == authorId &&
                                      (includeFrozen || !b.IsFrozen))
                          .OrderBy(b => b.Name)
                          .ToList();
            return list.Select(MapBook).ToList();
        }

        public static int AddBook(string name, string coverPath, int authorId, List<int> genreIds)
        {
            using var ctx = Ctx();
            var book = new BookEntity
            {
                Name = name, CoverPath = coverPath,
                AuthorId = authorId, IsFrozen = false
            };
            ctx.Books.Add(book);
            ctx.SaveChanges();

            foreach (var gId in genreIds)
                ctx.BookGenres.Add(new BookGenreEntity { BookId = book.Id, GenreId = gId });
            ctx.SaveChanges();

            return book.Id;
        }

        public static void UpdateBook(int id, string name, string coverPath, List<int> genreIds)
        {
            using var ctx = Ctx();
            var book = ctx.Books.Find(id);
            if (book is null) return;

            book.Name      = name;
            book.CoverPath = coverPath;

            // Заменяем жанры
            var old = ctx.BookGenres.Where(bg => bg.BookId == id).ToList();
            ctx.BookGenres.RemoveRange(old);
            foreach (var gId in genreIds)
                ctx.BookGenres.Add(new BookGenreEntity { BookId = id, GenreId = gId });

            ctx.SaveChanges();
        }

        public static void FreezeBook(int id, bool frozen)
        {
            using var ctx = Ctx();
            var book = ctx.Books.Find(id);
            if (book is null) return;
            book.IsFrozen = frozen;
            ctx.SaveChanges();
        }

        // ═══════════════════════════════════════════════════════════════════
        //  CHAPTERS
        // ═══════════════════════════════════════════════════════════════════

        public static List<Models.Chapter> GetChapters(int bookId)
        {
            using var ctx = Ctx();
            return ctx.Chapters
                      .Where(c => c.BookId == bookId)
                      .OrderBy(c => c.Number)
                      .ToList()
                      .Select(MapChapter).ToList();
        }

        public static int AddChapter(int bookId, int number, string name, string path)
        {
            using var ctx = Ctx();
            var ch = new ChapterEntity
            {
                BookId = bookId, Number = number, Name = name, Path = path
            };
            ctx.Chapters.Add(ch);
            ctx.SaveChanges();
            return ch.Id;
        }

        // ═══════════════════════════════════════════════════════════════════
        //  GENRES
        // ═══════════════════════════════════════════════════════════════════

        public static List<Genre> GetGenres()
        {
            using var ctx = Ctx();
            return ctx.Genres
                      .OrderBy(g => g.Name)
                      .Select(g => new Genre
                      {
                          Id = g.Id, Name = g.Name, Description = g.Description
                      })
                      .ToList();
        }

        // ═══════════════════════════════════════════════════════════════════
        //  REVIEWS
        // ═══════════════════════════════════════════════════════════════════

        public static List<Review> GetReviews(int bookId)
        {
            using var ctx = Ctx();
            return ctx.Reviews
                      .Include(r => r.User)
                      .Where(r => r.BookId == bookId)
                      .OrderByDescending(r => r.CreationDate)
                      .ToList()
                      .Select(MapReview).ToList();
        }

        public static List<Review> GetUserReviews(int userId)
        {
            using var ctx = Ctx();
            return ctx.Reviews
                      .Include(r => r.User)
                      .Where(r => r.UserId == userId)
                      .OrderByDescending(r => r.CreationDate)
                      .ToList()
                      .Select(MapReview).ToList();
        }

        public static void AddReview(int userId, int bookId, string text, int rate)
        {
            using var ctx = Ctx();
            ctx.Reviews.Add(new ReviewEntity
            {
                UserId = userId, BookId = bookId,
                Text = text, Rate = (byte)rate,
                CreationDate = DateTime.Now
            });
            ctx.SaveChanges();
        }

        public static bool HasReview(int userId, int bookId)
        {
            using var ctx = Ctx();
            return ctx.Reviews.Any(r => r.UserId == userId && r.BookId == bookId);
        }

        public static void DeleteReview(int id)
        {
            using var ctx = Ctx();
            // Сначала удаляем связанные жалобы
            var complaints = ctx.Complaints.Where(c => c.ReviewId == id).ToList();
            ctx.Complaints.RemoveRange(complaints);

            var review = ctx.Reviews.Find(id);
            if (review != null) ctx.Reviews.Remove(review);

            ctx.SaveChanges();
        }

        // ═══════════════════════════════════════════════════════════════════
        //  COMPLAINTS
        // ═══════════════════════════════════════════════════════════════════

        public static List<Complaint> GetComplaints()
        {
            using var ctx = Ctx();
            return ctx.Complaints
                      .Include(c => c.User)
                      .Include(c => c.Book)
                      .OrderByDescending(c => c.Id)
                      .ToList()
                      .Select(MapComplaint).ToList();
        }

        public static void AddComplaint(int userId, int? bookId, int? reviewId, string reason)
        {
            using var ctx = Ctx();
            ctx.Complaints.Add(new ComplaintEntity
            {
                UserId = userId, BookId = bookId,
                ReviewId = reviewId, ReasonText = reason
            });
            ctx.SaveChanges();
        }

        public static void DeleteComplaint(int id)
        {
            using var ctx = Ctx();
            var c = ctx.Complaints.Find(id);
            if (c != null) { ctx.Complaints.Remove(c); ctx.SaveChanges(); }
        }

        // ═══════════════════════════════════════════════════════════════════
        //  READING LIST
        // ═══════════════════════════════════════════════════════════════════

        public static List<Book> GetReadList(int userId, int statusId,
                                             string search = "", int genreId = 0,
                                             string sortBy = "name")
        {
            using var ctx = Ctx();
            var query = ctx.ReadLists
                           .Include(rl => rl.Book).ThenInclude(b => b.Author)
                           .Include(rl => rl.Book).ThenInclude(b => b.Reviews)
                           .Include(rl => rl.Book).ThenInclude(b => b.BookGenres)
                               .ThenInclude(bg => bg.Genre)
                           .Where(rl => rl.UserId == userId && rl.StatusId == statusId);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(rl => rl.Book.Name.Contains(search) ||
                                          rl.Book.Author.Name.Contains(search));

            if (genreId > 0)
                query = query.Where(rl =>
                    rl.Book.BookGenres.Any(bg => bg.GenreId == genreId));

            var books = query.Select(rl => rl.Book).ToList();

            return (sortBy == "rating"
                        ? books.OrderByDescending(b => b.Reviews.Any()
                                   ? b.Reviews.Average(r => (double)r.Rate) : 0)
                        : books.OrderBy(b => b.Name))
                   .Select(MapBook).ToList();
        }

        public static void SetReadListStatus(int userId, int bookId, int statusId)
        {
            using var ctx = Ctx();
            var existing = ctx.ReadLists
                              .FirstOrDefault(rl => rl.UserId == userId && rl.BookId == bookId);
            if (existing != null)
                existing.StatusId = statusId;
            else
                ctx.ReadLists.Add(new ReadListEntity
                {
                    UserId = userId, BookId = bookId, StatusId = statusId
                });
            ctx.SaveChanges();
        }

        public static void RemoveFromReadList(int userId, int bookId)
        {
            using var ctx = Ctx();
            var e = ctx.ReadLists
                       .FirstOrDefault(rl => rl.UserId == userId && rl.BookId == bookId);
            if (e != null) { ctx.ReadLists.Remove(e); ctx.SaveChanges(); }
        }

        public static int GetReadListStatus(int userId, int bookId)
        {
            using var ctx = Ctx();
            return ctx.ReadLists
                      .Where(rl => rl.UserId == userId && rl.BookId == bookId)
                      .Select(rl => rl.StatusId)
                      .FirstOrDefault();
        }

        // ═══════════════════════════════════════════════════════════════════
        //  REQUESTS
        // ═══════════════════════════════════════════════════════════════════

        public static List<Request> GetRequests(int typeId)
        {
            using var ctx = Ctx();
            return ctx.Requests
                      .Include(r => r.Type)
                      .Include(r => r.User)
                      .Where(r => r.TypeId == typeId)
                      .OrderByDescending(r => r.Id)
                      .ToList()
                      .Select(MapRequest).ToList();
        }

        public static void AddRequest(int typeId, int userId, string comment)
        {
            using var ctx = Ctx();
            ctx.Requests.Add(new RequestEntity
            {
                TypeId = typeId, UserId = userId, Comment = comment
            });
            ctx.SaveChanges();
        }

        public static void DeleteRequest(int id)
        {
            using var ctx = Ctx();
            var r = ctx.Requests.Find(id);
            if (r != null) { ctx.Requests.Remove(r); ctx.SaveChanges(); }
        }

        public static bool HasPendingRequest(int userId, int typeId)
        {
            using var ctx = Ctx();
            return ctx.Requests.Any(r => r.UserId == userId && r.TypeId == typeId);
        }

        public static int EnsureRequestType(string typeName)
        {
            using var ctx = Ctx();
            var existing = ctx.RequestTypes.FirstOrDefault(rt => rt.TypeName == typeName);
            if (existing != null) return existing.Id;

            var newType = new RequestTypeEntity { TypeName = typeName };
            ctx.RequestTypes.Add(newType);
            ctx.SaveChanges();
            return newType.Id;
        }

        // ═══════════════════════════════════════════════════════════════════
        //  USERS
        // ═══════════════════════════════════════════════════════════════════

        public static List<User> GetUsers()
        {
            using var ctx = Ctx();
            return ctx.Users
                      .Include(u => u.Role)
                      .OrderBy(u => u.Name)
                      .ToList()
                      .Select(MapUser).ToList();
        }

        public static User? GetUser(int id)
        {
            using var ctx = Ctx();
            var e = ctx.Users.Include(u => u.Role).FirstOrDefault(u => u.Id == id);
            return e is null ? null : MapUser(e);
        }

        public static void FreezeUser(int id, bool frozen)
        {
            using var ctx = Ctx();
            var u = ctx.Users.Find(id);
            if (u is null) return;
            u.IsFrozen = frozen;
            ctx.SaveChanges();
        }

        public static void SetUserRole(int userId, int roleId)
        {
            using var ctx = Ctx();
            var u = ctx.Users.Find(userId);
            if (u is null) return;
            u.RoleId = roleId;
            ctx.SaveChanges();
        }

        public static void ChangePassword(int userId, string newPassword)
        {
            using var ctx = Ctx();
            var u = ctx.Users.Find(userId);
            if (u is null) return;
            u.Password = newPassword;
            ctx.SaveChanges();
        }

        public static List<User> GetFrozenUsers()
        {
            using var ctx = Ctx();
            return ctx.Users
                      .Include(u => u.Role)
                      .Where(u => u.IsFrozen)
                      .ToList()
                      .Select(MapUser).ToList();
        }

        public static List<Book> GetFrozenBooks()
        {
            using var ctx = Ctx();
            var list = ctx.Books
                          .Include(b => b.Author)
                          .Include(b => b.Reviews)
                          .Include(b => b.BookGenres).ThenInclude(bg => bg.Genre)
                          .Where(b => b.IsFrozen)
                          .ToList();
            return list.Select(MapBook).ToList();
        }
    }
}
