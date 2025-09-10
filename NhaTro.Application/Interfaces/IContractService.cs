using NhaTro.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NhaTro.Application.Interfaces
{
    public interface IContractService
    {
        Task<IEnumerable<ContractDto>> GetAllContractsAsync();
        Task<ContractDto> GetByIdAsync(int contractId);
        Task AddContractAsync(ContractDto contractDto);
        Task UpdateContractAsync(ContractDto contractDto);
        Task DeleteContractAsync(int contractId);

        // --- Các phương thức mới cần thêm ---
        Task<ContractDto> GetActiveContractByRoomIdAsync(int roomId);
        Task<IEnumerable<UserDto>> GetAllTenantsForOwnerAsync(int ownerId);
        Task<IEnumerable<ContractDto>> GetContractsByTenantIdAsync(int tenantId);
    }
}