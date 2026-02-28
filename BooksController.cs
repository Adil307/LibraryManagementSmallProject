using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace LibraryManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class BooksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public BooksController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Index(string? search)
        {
            var query = _context.Books.Include(b => b.Category).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(b => b.Title.Contains(search) || b.Author.Contains(search));
            }

            return View(await query.ToListAsync());
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Book book, IFormFile? imageFile)
        {
            if (!ModelState.IsValid)
                return View(book);

            if (imageFile != null && imageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "books");

                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await imageFile.CopyToAsync(stream);

                book.ImagePath = "/images/books/" + fileName;
            }

            _context.Add(book);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == id);
            if (book == null) return NotFound();
            return View(book);
        }

        // POST: /Books/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Book input, IFormFile? imageFile)
        {
            if (id != input.Id) return NotFound();
            if (!ModelState.IsValid) return View(input);

            var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == id);
            if (book == null) return NotFound();

            // ✅ issued copies = Total - Available (never negative)
            var issued = book.TotalCopies - book.AvailableCopies;
            if (issued < 0) issued = 0;

            // ✅ update basic fields
            book.Title = input.Title;
            book.Author = input.Author;
            book.ISBN = input.ISBN;
            book.CategoryId = input.CategoryId;

            // ✅ keep totals valid + preserve issued copies logic
            book.TotalCopies = input.TotalCopies < 0 ? 0 : input.TotalCopies;
            book.AvailableCopies = Math.Max(0, book.TotalCopies - issued);

            // ✅ handle image upload (optional)
            if (imageFile != null && imageFile.Length > 0)
            {
                var folder = Path.Combine(_env.WebRootPath, "images", "books");
                Directory.CreateDirectory(folder);

                var fileName = $"{Guid.NewGuid():N}{Path.GetExtension(imageFile.FileName)}";
                var fullPath = Path.Combine(folder, fileName);

                using var stream = new FileStream(fullPath, FileMode.Create);
                await imageFile.CopyToAsync(stream);

                // optional: delete old file
                if (!string.IsNullOrWhiteSpace(book.ImagePath))
                {
                    var old = book.ImagePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString());
                    var oldPath = Path.Combine(_env.WebRootPath, old);
                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);
                }

                book.ImagePath = $"/images/books/{fileName}";
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Book updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == id);
            if (book == null) return NotFound();

            // if any loan exists for this book, block delete
            var hasLoans = await _context.Loans.AnyAsync(l => l.BookId == id);
            if (hasLoans)
            {
                TempData["Error"] = "Cannot delete this book because it has loan records. You can mark it as inactive instead.";
                return RedirectToAction(nameof(Index));
            }

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Book deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

    }
}