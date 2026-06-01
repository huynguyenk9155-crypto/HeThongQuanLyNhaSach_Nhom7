using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tuan6.Models
{
    public class Order
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string? UserId { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        [Display(Name = "Ngày đặt hàng")]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Họ tên người nhận là bắt buộc")]
        [StringLength(100, ErrorMessage = "Họ tên không được vượt quá 100 ký tự")]
        [Display(Name = "Họ tên người nhận")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Địa chỉ nhận hàng là bắt buộc")]
        [StringLength(255, ErrorMessage = "Địa chỉ không được vượt quá 255 ký tự")]
        [Display(Name = "Địa chỉ nhận hàng")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại nhận hàng là bắt buộc")]
        [StringLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [Display(Name = "Số điện thoại nhận hàng")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Display(Name = "Ghi chú")]
        public string? Notes { get; set; }

        [Required]
        [Display(Name = "Trạng thái đơn hàng")]
        public string Status { get; set; } = "Pending"; // Pending, Shipping, Completed, Cancelled

        [Display(Name = "Tổng số tiền")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalAmount { get; set; }

        public List<OrderDetail>? OrderDetails { get; set; }
    }
}
