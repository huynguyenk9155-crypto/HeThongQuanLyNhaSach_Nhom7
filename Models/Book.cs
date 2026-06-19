using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tuan6.Models
{
    public class Book
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên sách là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên sách không được vượt quá 100 ký tự")]
        [Display(Name = "Tên sách")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên tác giả là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên tác giả không được vượt quá 100 ký tự")]
        [Display(Name = "Tác giả")]
        public string Author { get; set; } = string.Empty;

        [Range(1000, 100000000, ErrorMessage = "Giá bán phải nằm trong khoảng từ 1.000đ đến 100.000.000đ")]
        [Display(Name = "Giá bán")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; }

        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [Display(Name = "Hình ảnh")]
        public string? ImageUrl { get; set; }

        [Required(ErrorMessage = "Số lượng tồn kho là bắt buộc")]
        [Range(0, 100000, ErrorMessage = "Số lượng tồn kho phải từ 0 đến 100.000")]
        [Display(Name = "Số lượng tồn kho")]
        public int StockQuantity { get; set; }

        [ForeignKey("Category")]
        [Display(Name = "Mã danh mục")]
        public int CategoryId { get; set; }

        [Display(Name = "Danh mục")]
        public Category? Category { get; set; }

        [ForeignKey("AuthorRelation")]
        [Display(Name = "Tác giả")]
        public int? AuthorId { get; set; }

        [Display(Name = "Thông tin tác giả")]
        public Author? AuthorRelation { get; set; }

        [ForeignKey("Publisher")]
        [Display(Name = "Nhà xuất bản")]
        public int? PublisherId { get; set; }

        [Display(Name = "Thông tin nhà xuất bản")]
        public Publisher? Publisher { get; set; }

        public List<BookImage>? Images { get; set; }
        
        public List<OrderDetail>? OrderDetails { get; set; }
    }
}
