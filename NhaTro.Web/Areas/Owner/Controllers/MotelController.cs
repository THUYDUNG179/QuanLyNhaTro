using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NhaTro.Application.Interfaces;
using NhaTro.Application.DTOs;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Linq; // Thêm dòng này cho .Where()
using System; // Thêm dòng này cho InvalidOperationException

namespace NhaTro.Web.Areas.Owner.Controllers
{
    [Area("Owner")]
    [Authorize(Roles = "Owner")]
    public class MotelController : Controller
    {
        private readonly IMotelService _motelService;

        public MotelController(IMotelService motelService)
        {
            _motelService = motelService;
        }

        // --- Helper để lấy OwnerId (giúp code sạch hơn) ---
        private int GetCurrentOwnerId()
        {
            var ownerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (ownerIdClaim == null || !int.TryParse(ownerIdClaim.Value, out int ownerId))
            {
                // Thay vì ném exception, bạn có thể chuyển hướng về trang lỗi hoặc login
                throw new InvalidOperationException("Không thể xác định Owner ID của người dùng hiện tại.");
            }
            return ownerId;
        }


        // GET: Owner/Motel/Index
        public async Task<IActionResult> Index()
        {
            try
            {
                var ownerId = GetCurrentOwnerId();
                var motels = await _motelService.GetAllMotelsAsync();
                var ownerMotels = motels.Where(m => m.OwnerId == ownerId).ToList(); // .ToList() để thực thi truy vấn
                return View(ownerMotels);
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Login", "Account", new { area = "" }); // Quay về trang login nếu lỗi ownerId
            }
        }

        // GET: Owner/Motel/Details/5
        public async Task<IActionResult> Details(int? id) // Sử dụng int?
        {
            if (id == null || id == 0) // Kiểm tra id hợp lệ
            {
                return NotFound();
            }

            try
            {
                var ownerId = GetCurrentOwnerId();
                var motel = await _motelService.GetByIdAsync(id.Value); // Sử dụng .Value cho int?

                if (motel == null || motel.OwnerId != ownerId) // Kiểm tra quyền sở hữu
                {
                    return NotFound();
                }
                return View(motel);
            }
            catch (InvalidOperationException ex) // Bắt lỗi từ GetCurrentOwnerId
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Login", "Account", new { area = "" });
            }
        }

        // GET: Owner/Motel/Create
        public IActionResult Create()
        {
            try
            {
                // GÁN OwnerId ngay khi khởi tạo model cho View
                var model = new CreateMotelDto
                {
                    OwnerId = GetCurrentOwnerId()
                };
                return View(model);
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Login", "Account", new { area = "" });
            }
        }

        // POST: Owner/Motel/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateMotelDto motelDto)
        {
            try
            {
                // Lấy OwnerId từ Claim một cách an toàn và gán lại vào DTO
                // Điều này giúp ngăn chặn người dùng thay đổi OwnerId trong hidden field
                motelDto.OwnerId = GetCurrentOwnerId();

                if (!ModelState.IsValid)
                {
                    // Khi ModelState.IsValid false, bạn có thể muốn đặt breakpoint ở đây
                    // để kiểm tra ModelState.Values và ModelState.Errors
                    TempData["Error"] = "Vui lòng kiểm tra lại thông tin đã nhập.";
                    return View(motelDto);
                }

                await _motelService.AddMotelAsync(motelDto);
                TempData["Success"] = "Tạo nhà trọ mới thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex) // Bắt lỗi từ GetCurrentOwnerId hoặc các lỗi khác
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                TempData["Error"] = "Đã xảy ra lỗi: " + ex.Message;
                return View(motelDto);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi không mong muốn khi tạo nhà trọ: " + ex.Message);
                TempData["Error"] = "Đã xảy ra lỗi không mong muốn. Vui lòng thử lại.";
                return View(motelDto);
            }
        }

        // GET: Owner/Motel/Edit/5
        public async Task<IActionResult> Edit(int? id) // Sử dụng int?
        {
            if (id == null || id == 0) // Kiểm tra id hợp lệ
            {
                return NotFound();
            }

            try
            {
                var ownerId = GetCurrentOwnerId();
                var motel = await _motelService.GetByIdAsync(id.Value);

                if (motel == null || motel.OwnerId != ownerId) // Kiểm tra quyền sở hữu
                {
                    return NotFound();
                }
                return View(motel);
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Login", "Account", new { area = "" });
            }
        }

        // POST: Owner/Motel/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(MotelDto motelDto)
        {
            try
            {
                // Lấy OwnerId từ Claim và gán lại để đảm bảo bảo mật và đúng quyền
                motelDto.OwnerId = GetCurrentOwnerId();

                if (!ModelState.IsValid)
                {
                    TempData["Error"] = "Vui lòng kiểm tra lại thông tin đã nhập.";
                    return View(motelDto);
                }

                if (motelDto.MotelId == 0) // Đảm bảo ID chỉnh sửa là hợp lệ
                {
                    return NotFound();
                }

                // Quyền sở hữu đã được kiểm tra ở GetByIdAsync trong service nếu bạn thay đổi nó.
                // Nếu không, bạn cần kiểm tra lại ở đây:
                // var existingMotel = await _motelService.GetByIdAsync(motelDto.MotelId);
                // if (existingMotel == null || existingMotel.OwnerId != motelDto.OwnerId)
                // {
                //    return Unauthorized(); // Hoặc NotFound
                // }

                await _motelService.UpdateMotelAsync(motelDto);
                TempData["Success"] = "Cập nhật nhà trọ thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                TempData["Error"] = "Đã xảy ra lỗi: " + ex.Message;
                return View(motelDto);
            }
            catch (KeyNotFoundException ex) // Bắt KeyNotFoundException nếu service vẫn ném khi update/delete
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return NotFound(); // Hoặc View với lỗi
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi không mong muốn khi cập nhật nhà trọ: " + ex.Message);
                TempData["Error"] = "Đã xảy ra lỗi không mong muốn. Vui lòng thử lại.";
                return View(motelDto);
            }
        }

        // GET: Owner/Motel/Delete/5 (nếu bạn muốn hiển thị trang xác nhận xóa)
        public async Task<IActionResult> Delete(int? id) // Sử dụng int?
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            try
            {
                var ownerId = GetCurrentOwnerId();
                var motel = await _motelService.GetByIdAsync(id.Value);
                if (motel == null || motel.OwnerId != ownerId)
                {
                    return NotFound();
                }
                return View(motel); // Trả về view xác nhận xóa
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Login", "Account", new { area = "" });
            }
        }

        // POST: Owner/Motel/Delete/5 (action thực hiện xóa)
        [HttpPost, ActionName("Delete")] // ActionName để tránh trùng tên với GET Delete
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id) // Nhận id từ form
        {
            if (id == 0)
            {
                return NotFound();
            }

            try
            {
                var ownerId = GetCurrentOwnerId();
                var motel = await _motelService.GetByIdAsync(id); // Lấy để kiểm tra quyền
                if (motel == null || motel.OwnerId != ownerId)
                {
                    return NotFound(); // Hoặc Unauthorized()
                }

                await _motelService.DeleteMotelAsync(id);
                TempData["Success"] = "Xóa nhà trọ thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            catch (KeyNotFoundException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return NotFound(); // Hoặc RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Đã xảy ra lỗi không mong muốn khi xóa nhà trọ: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
