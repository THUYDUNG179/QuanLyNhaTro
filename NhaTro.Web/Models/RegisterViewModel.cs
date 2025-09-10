namespace NhaTro.Web.Models
{
    public class RegisterViewModel
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Phone { get; set; }
        public string ConfirmPassword { get; set; }
        public int RoleId { get; set; } = 3; // Mặc định là Tenant
    }
}
