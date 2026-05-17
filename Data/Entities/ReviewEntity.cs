using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShutIKrol.Data.Entities
{
    [Table("Reviews")]
    public class ReviewEntity
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }
        public int BookId { get; set; }

        [Required, MaxLength(255)]
        public string Text { get; set; } = "";

        public byte Rate { get; set; }

        public DateTime? CreationDate { get; set; }

        [ForeignKey(nameof(UserId))]
        public UserEntity User { get; set; } = null!;

        [ForeignKey(nameof(BookId))]
        public BookEntity Book { get; set; } = null!;
    }
}
