using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShutIKrol.Data.Entities
{
    [Table("ReadList")]
    public class ReadListEntity
    {
        [Key]
        public int Id { get; set; }

        public int UserId   { get; set; }
        public int BookId   { get; set; }
        public int StatusId { get; set; }

        [ForeignKey(nameof(UserId))]
        public UserEntity User { get; set; } = null!;

        [ForeignKey(nameof(BookId))]
        public BookEntity Book { get; set; } = null!;

        [ForeignKey(nameof(StatusId))]
        public ReadStatusEntity Status { get; set; } = null!;
    }
}
