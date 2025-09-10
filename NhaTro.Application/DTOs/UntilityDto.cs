namespace NhaTro.Application.DTOs
{
    public class UtilityDto
    {
        public int UtilityId { get; set; }
        public string UtilityName { get; set; }
        public string Unit { get; set; }
    }

    public class CreateUtilityDto
    {
        public string UtilityName { get; set; }
        public string Unit { get; set; }
    }
}