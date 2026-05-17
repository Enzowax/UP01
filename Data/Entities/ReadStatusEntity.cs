using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShutIKrol.Data.Entities
{
    [Table("ReadStatuses")]
    public class ReadStatusEntity
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string Name { get; set; } = "";

        public ICollection<ReadListEntity> ReadLists { get; set; } = new List<ReadListEntity>();
    }
}
