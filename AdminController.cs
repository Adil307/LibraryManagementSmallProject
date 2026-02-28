using LibraryManagementSystem.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalBooks = await _context.Books.CountAsync();
            ViewBag.TotalMembers = await _context.Members.CountAsync();
            ViewBag.IssuedBooks = await _context.Loans.CountAsync(l => l.ReturnDate == null);

            var today = DateTime.UtcNow.Date;
            ViewBag.OverdueBooks = await _context.Loans.CountAsync(l => l.ReturnDate == null && l.DueDate.Date < today);

            // ✅ RECENT LOANS with Book + Member loaded
            var recentLoans = await _context.Loans
                .Include(l => l.Book)
                .Include(l => l.Member)
                .OrderByDescending(l => l.IssueDate)
                .Take(10)
                .ToListAsync();

            return View(recentLoans);
        }
    }
}