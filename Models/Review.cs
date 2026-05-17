using System;

namespace ShutIKrol.Models
{
    public class Review
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = "";
        public int BookId { get; set; }
        public string Text { get; set; } = "";
        public int Rate { get; set; }
        public DateTime? CreationDate { get; set; }
    }
}
