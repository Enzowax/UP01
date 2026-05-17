using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShutIKrol.Data.Entities
{
    [Table("Requests")]
    public class RequestEntity
    {
        [Key]
        public int Id { get; set; }

        public int TypeId  { get; set; }
        public int UserId  { get; set; }

        [Required, MaxLength(255)]
        public string Comment { get; set; } = "";

        [ForeignKey(nameof(TypeId))]
        public RequestTypeEntity Type { get; set; } = null!;

        [ForeignKey(nameof(UserId))]
        public UserEntity User { get; set; } = null!;
    }
}
