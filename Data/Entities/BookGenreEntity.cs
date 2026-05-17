using System.ComponentModel.DataAnnotations.Schema;

namespace ShutIKrol.Data.Entities
{
    [Table("BookGenres")]
    public class BookGenreEntity
    {
        public int BookId  { get; set; }
        public int GenreId { get; set; }

        [ForeignKey(nameof(BookId))]
        public BookEntity  Book  { get; set; } = null!;

        [ForeignKey(nameof(GenreId))]
        public GenreEntity Genre { get; set; } = null!;
    }
}
