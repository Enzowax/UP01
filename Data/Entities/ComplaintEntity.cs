using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShutIKrol.Data.Entities
{
    [Table("Complaints")]
    public class ComplaintEntity
    {
        [Key]
        public int Id { get; set; }

        public int  UserId   { get; set; }
        public int? BookId   { get; set; }
        public int? ReviewId { get; set; }

        [Required, MaxLength(255)]
        public string ReasonText { get; set; } = "";

        [ForeignKey(nameof(UserId))]
        public UserEntity User { get; set; } = null!;

        [ForeignKey(nameof(BookId))]
        public BookEntity? Book { get; set; }
    }
}
