using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RunEF.WebServer.Infrastructure.Data;
using RunEF.WebServer.Domain.Entities;
using System.Security.Cryptography;
using System.Text;

namespace RunEF.WebServer.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AccountManagementController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountManagementController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var accounts = await _context.DataRunEFAccountWebs
                .OrderBy(a => a.Username)
                .ToListAsync();
            
            ViewBag.CurrentUsername = User.Identity?.Name;
            return View(accounts);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAccount(int id)
        {
            try
            {
                var account = await _context.DataRunEFAccountWebs.FindAsync(id);
                if (account == null)
                {
                    return Json(new { success = false, message = "Tài khoản không tồn tại." });
                }

                // Không cho phép xóa tài khoản đang đăng nhập
                if (account.Username == User.Identity?.Name)
                {
                    return Json(new { success = false, message = "Không thể xóa tài khoản đang đăng nhập." });
                }

                _context.DataRunEFAccountWebs.Remove(account);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Xóa tài khoản thành công." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(int id, string newPassword)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
                {
                    return Json(new { success = false, message = "Mật khẩu phải có ít nhất 6 ký tự." });
                }

                var account = await _context.DataRunEFAccountWebs.FindAsync(id);
                if (account == null)
                {
                    return Json(new { success = false, message = "Tài khoản không tồn tại." });
                }

                // Hash mật khẩu mới
                account.PasswordHash = HashPassword(newPassword);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Đổi mật khẩu thành công." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
}