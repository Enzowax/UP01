using Microsoft.EntityFrameworkCore;
using ShutIKrol.Data.Entities;

namespace ShutIKrol.Data
{
    /// <summary>
    /// EF Core DbContext — отображение таблиц БД UP_ShutIKrol на классы-сущности.
    /// </summary>
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // ── DbSet'ы (по одному на таблицу) ─────────────────────────────────
        public DbSet<RoleEntity>        Roles        => Set<RoleEntity>();
        public DbSet<UserEntity>        Users        => Set<UserEntity>();
        public DbSet<BookEntity>        Books        => Set<BookEntity>();
        public DbSet<GenreEntity>       Genres       => Set<GenreEntity>();
        public DbSet<BookGenreEntity>   BookGenres   => Set<BookGenreEntity>();
        public DbSet<ChapterEntity>     Chapters     => Set<ChapterEntity>();
        public DbSet<ReviewEntity>      Reviews      => Set<ReviewEntity>();
        public DbSet<ComplaintEntity>   Complaints   => Set<ComplaintEntity>();
        public DbSet<ReadStatusEntity>  ReadStatuses => Set<ReadStatusEntity>();
        public DbSet<ReadListEntity>    ReadLists    => Set<ReadListEntity>();
        public DbSet<RequestTypeEntity> RequestTypes => Set<RequestTypeEntity>();
        public DbSet<RequestEntity>     Requests     => Set<RequestEntity>();

        protected override void OnModelCreating(ModelBuilder mb)
        {
            // ── BookGenres: составной первичный ключ ─────────────────────────
            mb.Entity<BookGenreEntity>()
              .HasKey(bg => new { bg.BookId, bg.GenreId });

            // ── Users → Roles (restrict, не каскад) ─────────────────────────
            mb.Entity<UserEntity>()
              .HasOne(u => u.Role)
              .WithMany(r => r.Users)
              .HasForeignKey(u => u.RoleId)
              .OnDelete(DeleteBehavior.Restrict);

            // ── Books → Users (автор) ────────────────────────────────────────
            mb.Entity<BookEntity>()
              .HasOne(b => b.Author)
              .WithMany(u => u.Books)
              .HasForeignKey(b => b.AuthorId)
              .OnDelete(DeleteBehavior.Restrict);

            // ── BookGenres → Books / Genres ──────────────────────────────────
            mb.Entity<BookGenreEntity>()
              .HasOne(bg => bg.Book)
              .WithMany(b => b.BookGenres)
              .HasForeignKey(bg => bg.BookId);

            mb.Entity<BookGenreEntity>()
              .HasOne(bg => bg.Genre)
              .WithMany(g => g.BookGenres)
              .HasForeignKey(bg => bg.GenreId);

            // ── Chapters → Books ─────────────────────────────────────────────
            mb.Entity<ChapterEntity>()
              .HasOne(c => c.Book)
              .WithMany(b => b.Chapters)
              .HasForeignKey(c => c.BookId);

            // ── Reviews → Users / Books ──────────────────────────────────────
            mb.Entity<ReviewEntity>()
              .HasOne(r => r.User)
              .WithMany(u => u.Reviews)
              .HasForeignKey(r => r.UserId)
              .OnDelete(DeleteBehavior.Restrict);

            mb.Entity<ReviewEntity>()
              .HasOne(r => r.Book)
              .WithMany(b => b.Reviews)
              .HasForeignKey(r => r.BookId)
              .OnDelete(DeleteBehavior.Cascade);

            // ── Complaints → Users / Books ───────────────────────────────────
            mb.Entity<ComplaintEntity>()
              .HasOne(c => c.User)
              .WithMany(u => u.Complaints)
              .HasForeignKey(c => c.UserId)
              .OnDelete(DeleteBehavior.Restrict);

            mb.Entity<ComplaintEntity>()
              .HasOne(c => c.Book)
              .WithMany()
              .HasForeignKey(c => c.BookId)
              .OnDelete(DeleteBehavior.Restrict)
              .IsRequired(false);

            // ── ReadList → Users / Books / ReadStatuses ──────────────────────
            mb.Entity<ReadListEntity>()
              .HasOne(rl => rl.User)
              .WithMany(u => u.ReadLists)
              .HasForeignKey(rl => rl.UserId)
              .OnDelete(DeleteBehavior.Restrict);

            mb.Entity<ReadListEntity>()
              .HasOne(rl => rl.Book)
              .WithMany(b => b.ReadLists)
              .HasForeignKey(rl => rl.BookId)
              .OnDelete(DeleteBehavior.Cascade);

            mb.Entity<ReadListEntity>()
              .HasOne(rl => rl.Status)
              .WithMany(s => s.ReadLists)
              .HasForeignKey(rl => rl.StatusId)
              .OnDelete(DeleteBehavior.Restrict);

            // ── Requests → RequestTypes / Users ──────────────────────────────
            mb.Entity<RequestEntity>()
              .HasOne(r => r.Type)
              .WithMany(t => t.Requests)
              .HasForeignKey(r => r.TypeId);

            mb.Entity<RequestEntity>()
              .HasOne(r => r.User)
              .WithMany(u => u.Requests)
              .HasForeignKey(r => r.UserId)
              .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
