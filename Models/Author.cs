using System.ComponentModel.DataAnnotations;

namespace Tuan6.Models
{
    public class Author
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên tác giả là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên tác giả không được vượt quá 100 ký tự")]
        [Display(Name = "Tên tác giả")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Tiểu sử")]
        public string? Biography { get; set; }

        public ICollection<Book>? Books { get; set; }
    }
}
