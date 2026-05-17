namespace ShutIKrol.Models
{
    public class Chapter
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public int Number { get; set; }
        public string Name { get; set; } = "";
        public string Path { get; set; } = "";
    }
}
