 using System.ComponentModel.DataAnnotations;

          namespace LibraryManagementSystem.Models

{
   

    public class Member : BaseEntity
    {
        [Required, StringLength(120)]
        public string FullName { get; set; } = "";

        [Required, StringLength(30)]
        public string Phone { get; set; } = "";

        [StringLength(150)]
        public string? Email { get; set; }

        [StringLength(50)]
        public string? MemberCode { get; set; } // e.g. STU-001
    }
}
