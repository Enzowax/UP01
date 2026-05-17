using System.Collections.Generic;

namespace ShutIKrol.Models
{
    public class Book
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string CoverPath { get; set; } = "";
        public int AuthorId { get; set; }
        public string AuthorName { get; set; } = "";
        public bool IsFrozen { get; set; }
        public double AvgRating { get; set; }
        public List<string> Genres { get; set; } = new();
    }
}
