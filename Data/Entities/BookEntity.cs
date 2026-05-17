using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShutIKrol.Data.Entities
{
    [Table("Books")]
    public class BookEntity
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string Name { get; set; } = "";

        [MaxLength(50)]
        public string CoverPath { get; set; } = "";

        public int AuthorId { get; set; }

        public bool IsFrozen { get; set; }

        // Navigation
        [ForeignKey(nameof(AuthorId))]
        public UserEntity Author { get; set; } = null!;

        public ICollection<BookGenreEntity> BookGenres { get; set; } = new List<BookGenreEntity>();
        public ICollection<ChapterEntity>   Chapters   { get; set; } = new List<ChapterEntity>();
        public ICollection<ReviewEntity>    Reviews    { get; set; } = new List<ReviewEntity>();
        public ICollection<ReadListEntity>  ReadLists  { get; set; } = new List<ReadListEntity>();
    }
}
