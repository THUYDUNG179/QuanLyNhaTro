namespace NhaTro.Application.DTOs
{
    public class UtilityReadingDto
    {
        public int ReadingId { get; set; }
        public int RoomId { get; set; }
        public int UtilityId { get; set; }
        public decimal ReadingValue { get; set; }
        public DateOnly ReadingDate { get; set; }
        public string? RoomName { get; set; }
        public string? MotelName { get; set; }
    }

    public class CreateUtilityReadingDto
    {
        public int RoomId { get; set; }
        public int UtilityId { get; set; }
        public decimal ReadingValue { get; set; }
        public DateOnly ReadingDate { get; set; }
    }
}