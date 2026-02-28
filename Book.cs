using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.Models
{
    public class Book : BaseEntity
    {
        [Required, MaxLength(200)]
        public string Title { get; set; } = "";

        [Required, MaxLength(150)]
        public string Author { get; set; } = "";

        [MaxLength(100)]
        public string? ISBN { get; set; }

        public int TotalCopies { get; set; } = 1;

        public int AvailableCopies { get; set; } = 1;

        // ✅ Category relationship (because your views/controllers use it)
        public int? CategoryId { get; set; }
        public Category? Category { get; set; }

        // ✅ Book Image (NEW)
        [MaxLength(300)]
        public string? ImagePath { get; set; }
    }
}