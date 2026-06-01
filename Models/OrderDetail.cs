using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tuan6.Models
{
    public class OrderDetail
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Order")]
        public int OrderId { get; set; }

        public Order? Order { get; set; }

        [Required]
        [ForeignKey("Book")]
        public int BookId { get; set; }

        public Book? Book { get; set; }

        [Required(ErrorMessage = "Số lượng mua là bắt buộc")]
        [Range(1, 1000, ErrorMessage = "Số lượng phải từ 1 đến 1000")]
        [Display(Name = "Số lượng")]
        public int Quantity { get; set; }

        [Required]
        [Display(Name = "Đơn giá")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; }
    }
}
