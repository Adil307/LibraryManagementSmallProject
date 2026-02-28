  using System.ComponentModel.DataAnnotations;

  namespace LibraryManagementSystem.Models

{
  

    public class Category : BaseEntity
    {
        [Required, StringLength(80)]
        public string Name { get; set; } = "";
    }
}
