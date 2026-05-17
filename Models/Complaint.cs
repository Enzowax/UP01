namespace ShutIKrol.Models
{
    public class Complaint
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = "";
        public int? BookId { get; set; }
        public string BookName { get; set; } = "";
        public int? ReviewId { get; set; }
        public string ReasonText { get; set; } = "";
        public string TargetType => BookId.HasValue ? "Книга" : "Отзыв";
    }
}
