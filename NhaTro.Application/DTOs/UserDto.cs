namespace NhaTro.Application.DTOs
{
    public class UserDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Phone { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; } // Thêm RoleName
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public object UserName { get; set; }
    }

    public class CreateUserDto
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Phone { get; set; }
        public int RoleId { get; set; }
        public bool IsActive { get; set; } = true;
    }
    public class LoginDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}