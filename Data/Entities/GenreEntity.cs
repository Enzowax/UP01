using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShutIKrol.Data.Entities
{
    [Table("Genres")]
    public class GenreEntity
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string Name { get; set; } = "";

        [MaxLength(255)]
        public string Description { get; set; } = "";

        public ICollection<BookGenreEntity> BookGenres { get; set; } = new List<BookGenreEntity>();
    }
}
