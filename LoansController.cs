using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class LoansController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LoansController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var loans = await _context.Loans
                .Include(l => l.Book)
                .Include(l => l.Member)
                .OrderByDescending(l => l.IssueDate)
                .ToListAsync();

            return View(loans);
        }

        public async Task<IActionResult> Issue()
        {
            ViewBag.Books = await _context.Books.Where(b => b.AvailableCopies > 0).ToListAsync();
            ViewBag.Members = await _context.Members.ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Issue(int bookId, int memberId, DateTime dueDate)
        {
            if (dueDate.Date < DateTime.Today)
            {
                TempData["Error"] = "Due date must be today or future date.";
                return RedirectToAction(nameof(Issue));
            }

            var book = await _context.Books.FindAsync(bookId);
            if (book == null || book.AvailableCopies <= 0)
            {
                TempData["Error"] = "Book not available.";
                return RedirectToAction(nameof(Index));
            }

            var member = await _context.Members.FindAsync(memberId);
            if (member == null)
            {
                TempData["Error"] = "Member not found.";
                return RedirectToAction(nameof(Issue));
            }

            var loan = new Loan
            {
                BookId = bookId,
                MemberId = memberId,
                DueDate = dueDate.Date,
                IssueDate = DateTime.UtcNow
            };

            book.AvailableCopies -= 1;

            _context.Loans.Add(loan);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Book issued successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Return(int id)
        {
            var loan = await _context.Loans
                .Include(l => l.Book)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (loan == null || loan.ReturnDate != null) return NotFound();

            loan.ReturnDate = DateTime.UtcNow;

            if (loan.Book != null)
                loan.Book.AvailableCopies += 1;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Book returned successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}