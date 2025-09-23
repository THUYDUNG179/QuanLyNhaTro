using NhaTro.Application.DTOs;
using NhaTro.Application.Interfaces;
using NhaTro.Domain.Interfaces;
using NhaTro.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NhaTro.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<Role> _roleRepository;
        private readonly IRepository<Motel> _motelRepository;
        private readonly IRepository<Room> _roomRepository;
        private readonly IRepository<Contract> _contractRepository;

        public UserService(IRepository<User> userRepository, IRepository<Role> roleRepository, 
            IRepository<Motel> motelRepository, IRepository<Room> roomRepository, 
            IRepository<Contract> contractRepository)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _motelRepository = motelRepository;
            _roomRepository = roomRepository;
            _contractRepository = contractRepository;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllAsync();
            var roles = await _roleRepository.GetAllAsync();
            return users.Select(u => new UserDto
            {
                UserId = u.UserId,
                FullName = u.FullName,
                Email = u.Email,
                PasswordHash = u.PasswordHash,
                Phone = u.Phone,
                RoleId = u.RoleId,
                RoleName = roles.FirstOrDefault(r => r.RoleId == u.RoleId)?.RoleName,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt
            }).ToList();
        }

        public async Task<UserDto> GetByIdAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException($"Người dùng với ID {userId} không tìm thấy.");

            var role = await _roleRepository.GetByIdAsync(user.RoleId);
            return new UserDto
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                PasswordHash = user.PasswordHash,
                Phone = user.Phone,
                RoleId = user.RoleId,
                RoleName = role?.RoleName,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };
        }

        public async Task AddUserAsync(CreateUserDto userDto)
        {
            var existingUsers = await _userRepository.GetAllAsync();
            if (existingUsers.Any(u => u.Email == userDto.Email))
                throw new InvalidOperationException("Email đã tồn tại.");

            var role = await _roleRepository.GetByIdAsync(userDto.RoleId);
            if (role == null)
                throw new InvalidOperationException($"Vai trò với ID {userDto.RoleId} không tồn tại.");

            var user = new User
            {
                FullName = userDto.FullName,
                Email = userDto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(userDto.Password),
                Phone = userDto.Phone,
                RoleId = userDto.RoleId,
                IsActive = userDto.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();
        }

        public async Task UpdateUserAsync(UserDto userDto)
        {
            var user = await _userRepository.GetByIdAsync(userDto.UserId);
            if (user == null)
                throw new KeyNotFoundException($"Người dùng với ID {userDto.UserId} không tìm thấy.");

            var role = await _roleRepository.GetByIdAsync(userDto.RoleId);
            if (role == null)
                throw new InvalidOperationException($"Vai trò với ID {userDto.RoleId} không tồn tại.");

            user.FullName = userDto.FullName;
            user.Email = userDto.Email;
            user.PasswordHash = userDto.PasswordHash; // Giả định đã mã hóa nếu thay đổi
            user.Phone = userDto.Phone;
            user.RoleId = userDto.RoleId;
            user.IsActive = userDto.IsActive;

            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();
        }

        public async Task DeleteUserAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException($"Người dùng với ID {userId} không tìm thấy.");

            await _userRepository.DeleteAsync(user);
            await _userRepository.SaveChangesAsync();
        }

        public async Task<UserDto> AuthenticateAsync(string email, string password)
        {
            var users = await _userRepository.GetAllAsync();
            var user = users.FirstOrDefault(u => u.Email == email && BCrypt.Net.BCrypt.Verify(password, u.PasswordHash));
            if (user == null)
                return null;

            var role = await _roleRepository.GetByIdAsync(user.RoleId);
            return new UserDto
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                PasswordHash = user.PasswordHash,
                Phone = user.Phone,
                RoleId = user.RoleId,
                RoleName = role?.RoleName,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };
        }
        public async Task<int> GetTotalTenantsCountAsync()
        {
            var allUsers = await _userRepository.GetAllAsync();
            // Giả định rằng bạn có một cách để xác định người thuê (ví dụ: vai trò, hoặc loại người dùng)
            return allUsers.Count(u => u.Role.RoleName == "Tenant");
        }
        public async Task<IEnumerable<UserDto>> GetAllTenantsForOwnerAsync(int ownerId)
        {
            // Lấy tất cả các nhà trọ thuộc về chủ nhà
            var motels = await _motelRepository.GetAllAsync();
            var ownerMotelIds = motels.Where(m => m.OwnerId == ownerId).Select(m => m.MotelId).ToHashSet();

            // Lấy tất cả các phòng thuộc về các nhà trọ đó
            var rooms = await _roomRepository.GetAllAsync();
            var ownerRoomIds = rooms.Where(r => ownerMotelIds.Contains(r.MotelId)).Select(r => r.RoomId).ToHashSet();

            // Lấy tất cả các hợp đồng liên quan đến các phòng đó
            var contracts = await _contractRepository.GetAllAsync();
            var relevantContracts = contracts.Where(c => ownerRoomIds.Contains(c.RoomId));

            // Lấy danh sách ID người thuê từ các hợp đồng
            var tenantIds = relevantContracts.Select(c => c.TenantId).Distinct().ToList();

            // Lấy thông tin người dùng (người thuê)
            var users = await _userRepository.GetAllAsync();
            var tenants = users.Where(u => tenantIds.Contains(u.UserId)).ToList();

            // Ánh xạ sang UserDto
            var roles = await _roleRepository.GetAllAsync();
            return tenants.Select(t => new UserDto
            {
                UserId = t.UserId,
                FullName = t.FullName,
                Email = t.Email,
                Phone = t.Phone,
                RoleId = t.RoleId,
                RoleName = roles.FirstOrDefault(r => r.RoleId == t.RoleId)?.RoleName,
                IsActive = t.IsActive,
                CreatedAt = t.CreatedAt
            });
        }
    }
}