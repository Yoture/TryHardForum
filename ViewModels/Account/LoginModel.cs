using System.ComponentModel.DataAnnotations;

namespace TryHardForum.ViewModels.Account
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Не указан адрес электронной почты")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Не указан пароль")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Запомнить меня")]
        public bool RememberMe { get; set; }
    }
}