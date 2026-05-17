using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShutIKrol.Data.Entities
{
    [Table("Chapters")]
    public class ChapterEntity
    {
        [Key]
        public int Id { get; set; }

        public int BookId { get; set; }

        public int Number { get; set; }

        [Required, MaxLength(50)]
        public string Name { get; set; } = "";

        [MaxLength(50)]
        public string Path { get; set; } = "";

        [ForeignKey(nameof(BookId))]
        public BookEntity Book { get; set; } = null!;
    }
}
