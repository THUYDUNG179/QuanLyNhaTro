using NhaTro.Application.DTOs;
using NhaTro.Application.Interfaces;
using NhaTro.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NhaTro.Infrastructure;

namespace NhaTro.Application.Services
{
    public class ContractService : IContractService
    {
        private readonly IRepository<Contract> _contractRepository;
        private readonly IRepository<Room> _roomRepository;
        private readonly IRepository<Motel> _motelRepository;
        private readonly IRepository<User> _userRepository;

        public ContractService(
            IRepository<Contract> contractRepository,
            IRepository<Room> roomRepository,
            IRepository<Motel> motelRepository,
            IRepository<User> userRepository)
        {
            _contractRepository = contractRepository;
            _roomRepository = roomRepository;
            _motelRepository = motelRepository;
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<ContractDto>> GetAllContractsAsync()
        {
            var contracts = await _contractRepository.GetAll()
                                                    .Include(c => c.Room)
                                                    .Include(c => c.Tenant)
                                                    .ToListAsync();

            return contracts.Select(c => new ContractDto
            {
                ContractId = c.ContractId,
                RoomId = c.RoomId,
                RoomName = c.Room?.RoomName,
                TenantId = c.TenantId,
                TenantName = c.Tenant?.FullName,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                DepositAmount = c.DepositAmount ?? 0,
                Status = c.Status,
                Notes = c.Notes,
                FileContractPath = c.FileContractPath,
                CreatedAt = c.CreatedAt
            }).ToList();
        }

        public async Task<ContractDto> GetByIdAsync(int contractId)
        {
            if (contractId <= 0)
            {
                throw new ArgumentException("ID hợp đồng không hợp lệ.", nameof(contractId));
            }

            var contract = await _contractRepository.GetAll()
                                                     .Include(c => c.Room)
                                                     .Include(c => c.Tenant)
                                                     .FirstOrDefaultAsync(c => c.ContractId == contractId);
            if (contract == null)
            {
                return null;
            }

            return new ContractDto
            {
                ContractId = contract.ContractId,
                RoomId = contract.RoomId,
                RoomName = contract.Room?.RoomName,
                TenantId = contract.TenantId,
                TenantName = contract.Tenant?.FullName,
                StartDate = contract.StartDate,
                EndDate = contract.EndDate,
                DepositAmount = contract.DepositAmount ?? 0,
                Status = contract.Status,
                Notes = contract.Notes,
                FileContractPath = contract.FileContractPath,
                CreatedAt = contract.CreatedAt
            };
        }

        public async Task AddContractAsync(ContractDto contractDto)
        {
            if (contractDto.RoomId <= 0 || contractDto.TenantId <= 0)
            {
                throw new ArgumentException("ID Phòng hoặc ID Người thuê không hợp lệ.", "RoomId/TenantId");
            }
            if (contractDto.StartDate > (contractDto.EndDate ?? DateOnly.MaxValue))
            {
                throw new ArgumentException("Ngày bắt đầu không được lớn hơn ngày kết thúc.", nameof(contractDto.StartDate));
            }

            var room = await _roomRepository.GetByIdAsync(contractDto.RoomId);
            if (room == null) throw new KeyNotFoundException($"Phòng với ID {contractDto.RoomId} không tìm thấy.");
            var tenant = await _userRepository.GetByIdAsync(contractDto.TenantId);
            if (tenant == null) throw new KeyNotFoundException($"Người thuê với ID {contractDto.TenantId} không tìm thấy.");

            var contract = new Contract
            {
                RoomId = contractDto.RoomId,
                TenantId = contractDto.TenantId,
                StartDate = contractDto.StartDate,
                EndDate = contractDto.EndDate,
                DepositAmount = contractDto.DepositAmount,
                Status = "Active",
                Notes = contractDto.Notes,
                FileContractPath = contractDto.FileContractPath,
                CreatedAt = DateTime.UtcNow
            };

            await _contractRepository.AddAsync(contract);
            await _contractRepository.SaveChangesAsync();
        }

        public async Task UpdateContractAsync(ContractDto contractDto)
        {
            if (contractDto.ContractId <= 0)
            {
                throw new ArgumentException("ID hợp đồng không hợp lệ.", nameof(contractDto.ContractId));
            }
            if (contractDto.RoomId <= 0 || contractDto.TenantId <= 0)
            {
                throw new ArgumentException("ID Phòng hoặc ID Người thuê không hợp lệ.", "RoomId/TenantId");
            }
            if (contractDto.StartDate > (contractDto.EndDate ?? DateOnly.MaxValue))
            {
                throw new ArgumentException("Ngày bắt đầu không được lớn hơn ngày kết thúc.", nameof(contractDto.StartDate));
            }

            var contract = await _contractRepository.GetByIdAsync(contractDto.ContractId);
            if (contract == null)
                throw new KeyNotFoundException($"Contract with ID {contractDto.ContractId} not found.");

            var room = await _roomRepository.GetByIdAsync(contractDto.RoomId);
            if (room == null) throw new KeyNotFoundException($"Phòng với ID {contractDto.RoomId} không tìm thấy.");
            var tenant = await _userRepository.GetByIdAsync(contractDto.TenantId);
            if (tenant == null) throw new KeyNotFoundException($"Người thuê với ID {contractDto.TenantId} không tìm thấy.");

            contract.RoomId = contractDto.RoomId;
            contract.TenantId = contractDto.TenantId;
            contract.StartDate = contractDto.StartDate;
            contract.EndDate = contractDto.EndDate;
            contract.DepositAmount = contractDto.DepositAmount;
            contract.Status = contractDto.Status;
            contract.Notes = contractDto.Notes;
            contract.FileContractPath = contractDto.FileContractPath;

            await _contractRepository.UpdateAsync(contract);
            await _contractRepository.SaveChangesAsync();
        }

        public async Task DeleteContractAsync(int contractId)
        {
            if (contractId <= 0)
            {
                throw new ArgumentException("ID hợp đồng không hợp lệ.", nameof(contractId));
            }

            var contract = await _contractRepository.GetByIdAsync(contractId);
            if (contract == null)
                throw new KeyNotFoundException($"Contract with ID {contractId} not found.");

            await _contractRepository.DeleteAsync(contract);
            await _contractRepository.SaveChangesAsync();
        }

        public async Task<ContractDto> GetActiveContractByRoomIdAsync(int roomId)
        {
            var contract = await _contractRepository.GetAll()
                .Include(c => c.Tenant)
                .FirstOrDefaultAsync(c =>
                    c.RoomId == roomId &&
                    c.EndDate.HasValue &&
                    c.EndDate.Value >= DateOnly.FromDateTime(DateTime.Today));

            if (contract == null) return null;

            return new ContractDto
            {
                ContractId = contract.ContractId,
                RoomId = contract.RoomId,
                TenantId = contract.TenantId,
                TenantName = contract.Tenant?.FullName,
            };
        }

        public async Task<IEnumerable<UserDto>> GetAllTenantsForOwnerAsync(int ownerId)
        {
            // Bước 1: Lấy tất cả các MotelId thuộc về chủ nhà hiện tại
            var ownerMotelIds = await _motelRepository.GetAll()
                .Where(m => m.OwnerId == ownerId)
                .Select(m => m.MotelId)
                .ToListAsync();

            if (!ownerMotelIds.Any())
            {
                return new List<UserDto>();
            }

            // Bước 2: Lấy tất cả các RoomId từ các nhà trọ đó
            var ownerRoomIds = await _roomRepository.GetAll()
                .Where(r => ownerMotelIds.Contains(r.MotelId))
                .Select(r => r.RoomId)
                .ToListAsync();

            if (!ownerRoomIds.Any())
            {
                return new List<UserDto>();
            }

            // Bước 3: Lấy tất cả các TenantId từ các hợp đồng còn hiệu lực trong các phòng đó
            var tenantIds = await _contractRepository.GetAll()
                .Where(c => ownerRoomIds.Contains(c.RoomId) && c.EndDate.HasValue && c.EndDate.Value >= DateOnly.FromDateTime(DateTime.Today))
                .Select(c => c.TenantId)
                .Distinct()
                .ToListAsync();

            if (!tenantIds.Any())
            {
                return new List<UserDto>();
            }

            // Bước 4: Lấy thông tin người dùng từ các TenantId đã có
            var tenants = await _userRepository.GetAll()
                .Where(u => tenantIds.Contains(u.UserId))
                .Select(u => new UserDto
                {
                    UserId = u.UserId,
                    FullName = u.FullName,
                    // Thêm các thuộc tính khác của User nếu cần
                })
                .ToListAsync();

            return tenants;
        }
        public async Task<IEnumerable<ContractDto>> GetContractsByTenantIdAsync(int tenantId)
        {
            var contracts = await _contractRepository.GetAll()
                .Where(c => c.TenantId == tenantId)
                .Include(c => c.Room)
                .ToListAsync();

            return contracts.Select(c => new ContractDto
            {
                ContractId = c.ContractId,
                RoomId = c.RoomId,
                RoomName = c.Room?.RoomName,
                TenantId = c.TenantId,
                TenantName = c.Tenant?.FullName,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                DepositAmount = c.DepositAmount ?? 0,
                Status = c.Status,
                Notes = c.Notes,
                FileContractPath = c.FileContractPath,
                CreatedAt = c.CreatedAt
            }).ToList();
        }

    }
}