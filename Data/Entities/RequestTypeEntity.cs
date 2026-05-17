using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShutIKrol.Data.Entities
{
    [Table("RequestTypes")]
    public class RequestTypeEntity
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string TypeName { get; set; } = "";

        public ICollection<RequestEntity> Requests { get; set; } = new List<RequestEntity>();
    }
}
