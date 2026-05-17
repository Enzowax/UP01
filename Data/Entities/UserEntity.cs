using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShutIKrol.Data.Entities
{
    [Table("Users")]
    public class UserEntity
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string Name { get; set; } = "";

        [Required, MaxLength(50)]
        public string Login { get; set; } = "";

        [Required, MaxLength(50)]
        public string Password { get; set; } = "";

        [Required, MaxLength(50)]
        public string Email { get; set; } = "";

        public int RoleId { get; set; }

        public bool IsFrozen { get; set; }

        public DateTime RegistrationDate { get; set; }

        // Navigation
        [ForeignKey(nameof(RoleId))]
        public RoleEntity Role { get; set; } = null!;

        public ICollection<BookEntity>      Books      { get; set; } = new List<BookEntity>();
        public ICollection<ReviewEntity>    Reviews    { get; set; } = new List<ReviewEntity>();
        public ICollection<ComplaintEntity> Complaints { get; set; } = new List<ComplaintEntity>();
        public ICollection<RequestEntity>   Requests   { get; set; } = new List<RequestEntity>();
        public ICollection<ReadListEntity>  ReadLists  { get; set; } = new List<ReadListEntity>();
    }
}
