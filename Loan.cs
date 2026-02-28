using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagementSystem.Models
{
    public class Loan : BaseEntity
    {
        [Required]
        public int BookId { get; set; }
        public Book? Book { get; set; }

        [Required]
        public int MemberId { get; set; }
        public Member? Member { get; set; }

        public DateTime IssueDate { get; set; } = DateTime.UtcNow;
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }

        [NotMapped]
        public bool IsReturned => ReturnDate != null;

        [NotMapped]
        public bool IsOverdue =>
            ReturnDate == null && DueDate.Date < DateTime.Today;
    }
}