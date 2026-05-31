using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tuan6.Models
{
    public class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên sản phẩm là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên sản phẩm không được vượt quá 100 ký tự")]
        [Display(Name = "Tên sản phẩm")]
        public string Name { get; set; } = string.Empty;

        [Range(1000, 100000000, ErrorMessage = "Giá bán phải nằm trong khoảng từ 1.000 đến 100.000.000")]
        [Display(Name = "Giá bán")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; }

        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [Display(Name = "Hình ảnh")]
        public string? ImageUrl { get; set; }

        [ForeignKey("Category")]
        [Display(Name = "Mã danh mục")]
        public int CategoryId { get; set; }

        [Display(Name = "Danh mục")]
        public Category? Category { get; set; }

        public List<ProductImage>? Images { get; set; }
    }
}
