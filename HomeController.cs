using LibraryManagementSystem.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalBooks = await _context.Books.CountAsync();
            ViewBag.TotalMembers = await _context.Members.CountAsync();
            ViewBag.TotalIssued = await _context.Loans.CountAsync(l => l.ReturnDate == null);
            ViewBag.TotalAvailable = await _context.Books.SumAsync(b => b.AvailableCopies);

            var latestBooks = await _context.Books
                .OrderByDescending(b => b.CreatedAt)
                .Take(6)
                .ToListAsync();

            return View(latestBooks);
        }

        public async Task<IActionResult> Books(string? search, int? categoryId, int page = 1)
        {
            const int pageSize = 12;

            var query = _context.Books
                .AsNoTracking()
                .Include(b => b.Category)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(b =>
                    b.Title.Contains(search) ||
                    b.Author.Contains(search) ||
                    (b.Category != null && b.Category.Name.Contains(search)));
            }

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(b => b.CategoryId == categoryId.Value);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(b => b.Title)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Search = search ?? "";
            ViewBag.CategoryId = categoryId ?? 0;
            ViewBag.Categories = await _context.Categories.AsNoTracking().OrderBy(c => c.Name).ToListAsync();

            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;
            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return View(items);
        }

        public async Task<IActionResult> BookDetails(int id)
        {
            var book = await _context.Books
                .AsNoTracking()
                .Include(b => b.Category)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null) return NotFound();
            return View(book);
        }

        public IActionResult Privacy() => View();
        public IActionResult Error() => View();
    }
}