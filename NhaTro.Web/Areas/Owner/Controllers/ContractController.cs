using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NhaTro.Application.Interfaces;
using NhaTro.Application.DTOs;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Linq;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.IO;

namespace NhaTro.Web.Areas.Owner.Controllers
{
    [Area("Owner")]
    [Authorize(Roles = "Owner")] // Chỉ cho phép Owner truy cập controller này
    public class ContractController : Controller // KHÔNG KẾ THỪA OwnerBaseController
    {
        private readonly IContractService _contractService;
        private readonly IRoomService _roomService;
        private readonly IMotelService _motelService;
        private readonly IUserService _userService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ContractController(IContractService contractService,
                                IRoomService roomService,
                                IMotelService motelService,
                                IUserService userService,
                                IWebHostEnvironment webHostEnvironment)
        {
            _contractService = contractService;
            _roomService = roomService;
            _motelService = motelService;
            _userService = userService;
            _webHostEnvironment = webHostEnvironment;
        }

        // --- Phương thức lấy ID của chủ nhà hiện tại (Đã di chuyển từ OwnerBaseController) ---
        protected int GetCurrentOwnerId()
        {
            var ownerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (ownerIdClaim == null || !int.TryParse(ownerIdClaim.Value, out int ownerId))
            {
                throw new InvalidOperationException("Không thể xác định ID chủ nhà của người dùng hiện tại. Vui lòng đăng nhập lại.");
            }
            return ownerId;
        }

        // --- Phương thức xử lý lỗi khi không tìm thấy tài nguyên hoặc không có quyền truy cập (Đã di chuyển từ OwnerBaseController) ---
        protected IActionResult OwnerAccessDenied(string errorMessage = "Bạn không có quyền truy cập tài nguyên này.")
        {
            TempData["Error"] = errorMessage;
            return RedirectToAction("Index", "Motel", new { area = "Owner" });
        }

        // --- Phương thức xử lý chuyển hướng về trang đăng nhập khi OwnerId không hợp lệ (Đã di chuyển từ OwnerBaseController) ---
        protected IActionResult RedirectToLoginOnInvalidOwnerId(InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction("Login", "Account", new { area = "" });
        }


        // --- Helper Methods: Tải danh sách cho Dropdown ---
        private async Task LoadRoomsForOwner(int ownerId, int? selectedRoomId = null)
        {
            var allRooms = await _roomService.GetAllRoomsAsync();
            var allMotels = await _motelService.GetAllMotelsAsync();

            var ownerRooms = allRooms
                .Join(allMotels.Where(m => m.OwnerId == ownerId),
                      room => room.MotelId,
                      motel => motel.MotelId,
                      (room, motel) => new SelectListItem
                      {
                          Value = room.RoomId.ToString(),
                          Text = $"{motel.MotelName} - {room.RoomName} (Giá: {room.RentalPrice:N0} VNĐ)",
                          Selected = room.RoomId == selectedRoomId
                      })
                .OrderBy(item => item.Text)
                .ToList();

            ViewBag.Rooms = new SelectList(ownerRooms, "Value", "Text", selectedRoomId);
        }

        private async Task LoadTenants(int? selectedTenantId = null)
        {
            var tenants = (await _userService.GetAllUsersAsync())
                            .Where(u => u.RoleId == 3) // Giả sử RoleId = 3 là Tenant
                            .Select(u => new SelectListItem
                            {
                                Value = u.UserId.ToString(),
                                Text = $"{u.FullName} ({u.Email})",
                                Selected = u.UserId == selectedTenantId
                            })
                            .OrderBy(item => item.Text)
                            .ToList();

            ViewBag.Tenants = new SelectList(tenants, "Value", "Text", selectedTenantId);
        }
        // GET: Owner/Contract/Index?motelId=X&roomId=Y
        public async Task<IActionResult> Index(int? motelId, int? roomId)
        {
            try
            {
                var ownerId = GetCurrentOwnerId();
                var contracts = await _contractService.GetAllContractsAsync();

                var ownerContracts = new List<ContractDto>();
                foreach (var contract in contracts)
                {
                    var room = await _roomService.GetByIdAsync(contract.RoomId);
                    if (room != null)
                    {
                        var motel = await _motelService.GetByIdAsync(room.MotelId);
                        if (motel != null && motel.OwnerId == ownerId)
                        {
                            ownerContracts.Add(contract);
                        }
                    }
                }

                if (motelId.HasValue && motelId > 0)
                {
                    var motelRooms = (await _roomService.GetAllRoomsAsync()).Where(r => r.MotelId == motelId.Value).Select(r => r.RoomId).ToList();
                    ownerContracts = ownerContracts.Where(c => motelRooms.Contains(c.RoomId)).ToList();

                    var motel = await _motelService.GetByIdAsync(motelId.Value);
                    ViewBag.MotelName = motel?.MotelName;
                    ViewBag.MotelId = motelId; // Đảm bảo gán là int?
                }

                if (roomId.HasValue && roomId > 0)
                {
                    var room = await _roomService.GetByIdAsync(roomId.Value);
                    var motel = (room != null) ? await _motelService.GetByIdAsync(room.MotelId) : null;
                    if(motel != null && motel.OwnerId == ownerId)
                    {
                        ownerContracts = ownerContracts.Where(c => c.RoomId == roomId.Value).ToList();
                        ViewBag.RoomName = room?.RoomName;
                        ViewBag.RoomId = roomId; // Đảm bảo gán là int?
                    }
                    else
                    {
                        return OwnerAccessDenied();
                    }
                }

                if (!motelId.HasValue && roomId.HasValue && roomId > 0)
                {
                    var room = await _roomService.GetByIdAsync(roomId.Value);
                    if (room != null)
                    {
                        ViewBag.MotelId = (int?)room.MotelId; // Ép kiểu rõ ràng thành int?
                    }
                }
                
                return View(ownerContracts);
            }
            catch (InvalidOperationException ex)
            {
                return RedirectToLoginOnInvalidOwnerId(ex);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Đã xảy ra lỗi khi tải danh sách hợp đồng: " + ex.Message;
                return View(new List<ContractDto>());
            }
        }

        // GET: Owner/Contract/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || id <= 0)
            {
                return NotFound();
            }

            try
            {
                var ownerId = GetCurrentOwnerId();
                var contract = await _contractService.GetByIdAsync(id.Value);

                if (contract == null)
                {
                    return NotFound();
                }

                var room = await _roomService.GetByIdAsync(contract.RoomId);
                if (room == null)
                {
                    return OwnerAccessDenied("Phòng liên quan đến hợp đồng không tìm thấy.");
                }

                var motel = await _motelService.GetByIdAsync(room.MotelId);
                if (motel == null || motel.OwnerId != ownerId)
                {
                    return OwnerAccessDenied("Bạn không có quyền truy cập hợp đồng này.");
                }

                ViewBag.RoomId = (int?)contract.RoomId; // Ép kiểu rõ ràng thành int?
                ViewBag.MotelId = (int?)room.MotelId;    // Ép kiểu rõ ràng thành int?
                return View(contract);
            }
            catch (InvalidOperationException ex)
            {
                return RedirectToLoginOnInvalidOwnerId(ex);
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
            catch (KeyNotFoundException ex)
            {
                TempData["Error"] = ex.Message;
                return NotFound();
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Đã xảy ra lỗi khi tải chi tiết hợp đồng: " + ex.Message;
                return RedirectToAction(nameof(Index), new { roomId = ViewBag.RoomId ?? 0 });
            }
        }

        // GET: Owner/Contract/Create?roomId=X
        public async Task<IActionResult> Create(int? roomId)
        {
            try
            {
                var ownerId = GetCurrentOwnerId();
                await LoadRoomsForOwner(ownerId, roomId);
                await LoadTenants();

                if (roomId.HasValue && roomId > 0)
                {
                    var room = await _roomService.GetByIdAsync(roomId.Value);
                    var motel = (room != null) ? await _motelService.GetByIdAsync(room.MotelId) : null;
                    if (room == null || motel == null || motel.OwnerId != ownerId)
                    {
                        TempData["Error"] = "Không tìm thấy phòng hoặc bạn không có quyền tạo hợp đồng.";
                        return RedirectToAction("Index", "Motel");
                    }
                    ViewBag.RoomName = room.RoomName;
                }

                var model = new CreateContractDto
                {
                    RoomId = roomId ?? 0,
                    StartDate = DateOnly.FromDateTime(DateTime.Today)
                };

                return View(model);
            }
            catch (InvalidOperationException)
            {
                TempData["Error"] = "Không thể xác định ID chủ nhà. Vui lòng đăng nhập lại.";
                return RedirectToAction("Login", "Account", new { area = "" });
            }
        }

        // POST: Owner/Contract/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateContractDto contractDto)
        {
            int ownerId = 0;
            try
            {
                ownerId = GetCurrentOwnerId();

                // Kiểm tra các lỗi nghiệp vụ trước khi kiểm tra ModelState
                var room = await _roomService.GetByIdAsync(contractDto.RoomId);
                if (room == null)
                {
                    ModelState.AddModelError("RoomId", "Phòng liên quan không tìm thấy.");
                }
                else
                {
                    var motel = await _motelService.GetByIdAsync(room.MotelId);
                    if (motel == null || motel.OwnerId != ownerId)
                    {
                        ModelState.AddModelError(string.Empty, "Bạn không có quyền tạo hợp đồng cho phòng này.");
                    }
                }

                if (contractDto.EndDate.HasValue && contractDto.StartDate > contractDto.EndDate)
                {
                    ModelState.AddModelError("EndDate", "Ngày kết thúc phải lớn hơn hoặc bằng Ngày bắt đầu.");
                }

                // Kiểm tra ModelState sau khi đã thêm các lỗi nghiệp vụ
                if (!ModelState.IsValid)
                {
                    TempData["Error"] = "Vui lòng kiểm tra lại thông tin đã nhập.";
                    await LoadRoomsForOwner(ownerId, contractDto.RoomId);
                    await LoadTenants(contractDto.TenantId);
                    return View(contractDto);
                }

                string fileContractPath = null;
                if (contractDto.FileContract != null)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "contracts");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + contractDto.FileContract.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await contractDto.FileContract.CopyToAsync(fileStream);
                    }
                    fileContractPath = "/contracts/" + uniqueFileName;
                }

                // Tạo một DTO mới để lưu vào DB
                var contractToCreate = new ContractDto
                {
                    RoomId = contractDto.RoomId,
                    TenantId = contractDto.TenantId,
                    StartDate = contractDto.StartDate,
                    EndDate = contractDto.EndDate,
                    DepositAmount = contractDto.DepositAmount,
                    Notes = contractDto.Notes,
                    FileContractPath = fileContractPath // Gán đường dẫn file đã tạo
                };

                // Gọi Service để thêm hợp đồng vào DB
                await _contractService.AddContractAsync(contractToCreate);

                TempData["Success"] = "Tạo hợp đồng mới thành công!";

                return RedirectToAction(nameof(Index), new { roomId = contractDto.RoomId });
            }
            catch (InvalidOperationException)
            {
                TempData["Error"] = "Không thể xác định ID chủ nhà. Vui lòng đăng nhập lại.";
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi không mong muốn khi tạo hợp đồng: " + ex.Message);
                TempData["Error"] = "Đã xảy ra lỗi không mong muốn. Vui lòng thử lại.";
                await LoadRoomsForOwner(ownerId, contractDto.RoomId);
                await LoadTenants(contractDto.TenantId);
                return View(contractDto);
            }
        }

        // GET: Owner/Contract/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || id <= 0)
            {
                return NotFound();
            }

            try
            {
                var ownerId = GetCurrentOwnerId();
                var contract = await _contractService.GetByIdAsync(id.Value);

                if (contract == null)
                {
                    return NotFound();
                }
                var room = await _roomService.GetByIdAsync(contract.RoomId);
                var motel = (room != null) ? await _motelService.GetByIdAsync(room.MotelId) : null;

                if (motel == null || motel.OwnerId != ownerId)
                {
                    return OwnerAccessDenied("Bạn không có quyền chỉnh sửa hợp đồng này.");
                }

                await LoadRoomsForOwner(ownerId, contract.RoomId);
                await LoadTenants(contract.TenantId);

                // Ánh xạ ContractDto sang UpdateContractDto
                var model = new UpdateContractDto
                {
                    ContractId = contract.ContractId,
                    RoomId = contract.RoomId,
                    TenantId = contract.TenantId,
                    StartDate = contract.StartDate,
                    EndDate = contract.EndDate,
                    DepositAmount = contract.DepositAmount,
                    Notes = contract.Notes,
                    FileContractPath = contract.FileContractPath,
                    CreatedAt = contract.CreatedAt
                };

                return View(model);
            }
            catch (InvalidOperationException ex)
            {
                return RedirectToLoginOnInvalidOwnerId(ex);
            }
            catch (KeyNotFoundException ex)
            {
                TempData["Error"] = ex.Message;
                return NotFound();
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Đã xảy ra lỗi khi tải thông tin hợp đồng để chỉnh sửa: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Owner/Contract/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdateContractDto contractDto)
        {
            int ownerId = 0;
            try
            {
                ownerId = GetCurrentOwnerId();
                var room = await _roomService.GetByIdAsync(contractDto.RoomId);
                if (room == null)
                {
                    return OwnerAccessDenied("Phòng liên quan không tìm thấy.");
                }
                var motel = await _motelService.GetByIdAsync(room.MotelId);
                if (motel == null || motel.OwnerId != ownerId)
                {
                    return OwnerAccessDenied("Bạn không có quyền chỉnh sửa hợp đồng này.");
                }

                if (!ModelState.IsValid)
                {
                    TempData["Error"] = "Vui lòng kiểm tra lại thông tin đã nhập.";
                    await LoadRoomsForOwner(ownerId, contractDto.RoomId);
                    await LoadTenants(contractDto.TenantId);
                    return View(contractDto);
                }

                string newFileContractPath = contractDto.FileContractPath;
                if (contractDto.FileContract != null)
                {
                    // Xóa file cũ nếu có
                    if (!string.IsNullOrEmpty(contractDto.FileContractPath))
                    {
                        string oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, contractDto.FileContractPath.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    // Lưu file mới
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "contracts");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + contractDto.FileContract.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await contractDto.FileContract.CopyToAsync(fileStream);
                    }
                    newFileContractPath = "/contracts/" + uniqueFileName;
                }

                var contractToUpdate = new ContractDto
                {
                    ContractId = contractDto.ContractId,
                    RoomId = contractDto.RoomId,
                    TenantId = contractDto.TenantId,
                    StartDate = contractDto.StartDate,
                    EndDate = contractDto.EndDate,
                    DepositAmount = contractDto.DepositAmount,
                    Notes = contractDto.Notes,
                    FileContractPath = newFileContractPath,
                    CreatedAt = contractDto.CreatedAt
                };

                await _contractService.UpdateContractAsync(contractToUpdate);
                TempData["Success"] = "Cập nhật hợp đồng thành công!";
                return RedirectToAction(nameof(Index), new { roomId = contractDto.RoomId });
            }
            catch (InvalidOperationException ex)
            {
                return RedirectToLoginOnInvalidOwnerId(ex);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi không mong muốn khi cập nhật hợp đồng: " + ex.Message);
                TempData["Error"] = "Đã xảy ra lỗi không mong muốn. Vui lòng thử lại.";
                await LoadRoomsForOwner(ownerId, contractDto.RoomId);
                await LoadTenants(contractDto.TenantId);
                return View(contractDto);
            }
        }

        // GET: Owner/Contract/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || id <= 0)
            {
                return NotFound();
            }

            try
            {
                var ownerId = GetCurrentOwnerId();
                var contract = await _contractService.GetByIdAsync(id.Value);

                if (contract == null)
                {
                    return NotFound();
                }

                var room = await _roomService.GetByIdAsync(contract.RoomId);
                var motel = (room != null) ? await _motelService.GetByIdAsync(room.MotelId) : null;

                if (motel == null || motel.OwnerId != ownerId)
                {
                    return OwnerAccessDenied("Bạn không có quyền xóa hợp đồng này.");
                }

                ViewBag.RoomId = contract.RoomId;
                ViewBag.MotelId = room.MotelId;

                return View(contract);
            }
            catch (InvalidOperationException ex)
            {
                return RedirectToLoginOnInvalidOwnerId(ex);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Đã xảy ra lỗi khi tải thông tin hợp đồng để xóa: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Owner/Contract/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            if (id == null || id <= 0)
            {
                return NotFound();
            }

            int ownerId = 0;
            int? currentRoomId = null;
            int? currentMotelId = null;

            try
            {
                ownerId = GetCurrentOwnerId();
                var contract = await _contractService.GetByIdAsync(id.Value);

                if (contract == null)
                {
                    return NotFound();
                }
                currentRoomId = contract.RoomId;

                var room = await _roomService.GetByIdAsync(contract.RoomId);
                if (room == null)
                {
                    return OwnerAccessDenied("Phòng liên quan đến hợp đồng không tìm thấy.");
                }
                currentMotelId = room.MotelId;
                var motel = await _motelService.GetByIdAsync(room.MotelId);
                if (motel == null || motel.OwnerId != ownerId)
                {
                    return OwnerAccessDenied("Bạn không có quyền xóa hợp đồng này.");
                }

                await _contractService.DeleteContractAsync(id.Value);
                TempData["Success"] = "Xóa hợp đồng thành công!";
            }
            catch (InvalidOperationException ex)
            {
                return RedirectToLoginOnInvalidOwnerId(ex);
            }
            catch (KeyNotFoundException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                TempData["Error"] = "Lỗi: " + ex.Message;
                return NotFound();
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                TempData["Error"] = "Lỗi: " + ex.Message;
                return NotFound();
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Đã xảy ra lỗi không mong muốn khi xóa hợp đồng: " + ex.Message;
            }

            if (currentMotelId.HasValue && currentMotelId > 0)
            {
                return RedirectToAction(nameof(Index), new { motelId = currentMotelId });
            }
            else if (currentRoomId.HasValue && currentRoomId > 0)
            {
                return RedirectToAction(nameof(Index), new { roomId = currentRoomId });
            }
            return RedirectToAction(nameof(Index));
        }
    }
}