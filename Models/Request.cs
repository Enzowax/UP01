namespace ShutIKrol.Models
{
    public class Request
    {
        public int Id { get; set; }
        public int TypeId { get; set; }
        public string TypeName { get; set; } = "";
        public int UserId { get; set; }
        public string UserName { get; set; } = "";
        public string Comment { get; set; } = "";
    }
}
