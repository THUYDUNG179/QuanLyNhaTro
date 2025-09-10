namespace NhaTro.Application.DTOs
{
    public class LogDto
    {
        public int LogId { get; set; }
        public int? UserId { get; set; }
        public string Action { get; set; }
        public string TableName { get; set; }
        public int? RecordId { get; set; }
        public string Detail { get; set; }
        public string IPAddress { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateLogDto
    {
        public int? UserId { get; set; }
        public string Action { get; set; }
        public string TableName { get; set; }
        public int? RecordId { get; set; }
        public string Detail { get; set; }
        public string IPAddress { get; set; }
    }
}