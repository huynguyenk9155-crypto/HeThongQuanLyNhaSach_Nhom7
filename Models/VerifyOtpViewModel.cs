using System.ComponentModel.DataAnnotations;

namespace Tuan6.Models
{
    public class VerifyOtpViewModel
    {
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mã xác nhận là bắt buộc")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Mã xác nhận phải gồm 6 chữ số")]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "Mã xác nhận chỉ bao gồm chữ số")]
        public string OtpCode { get; set; } = string.Empty;
    }
}
