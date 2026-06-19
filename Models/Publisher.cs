using System.ComponentModel.DataAnnotations;

namespace Tuan6.Models
{
    public class Publisher
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên nhà xuất bản là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên nhà xuất bản không được vượt quá 100 ký tự")]
        [Display(Name = "Tên nhà xuất bản")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Địa chỉ")]
        public string? Address { get; set; }

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        public ICollection<Book>? Books { get; set; }
    }
}
