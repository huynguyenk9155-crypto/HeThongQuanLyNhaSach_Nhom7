using System.ComponentModel.DataAnnotations;

namespace Tuan6.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Tên đăng nhập hoặc Email là bắt buộc")]
        [Display(Name = "Tên đăng nhập / Email")]
        public string UserNameOrEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Nhớ mật khẩu?")]
        public bool RememberMe { get; set; }
    }
}
