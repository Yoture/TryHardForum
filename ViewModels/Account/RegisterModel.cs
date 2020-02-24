using System.ComponentModel.DataAnnotations;

namespace TryHardForum.ViewModels.Account
{
    public class RegisterModel
    {
        [Required(ErrorMessage = "Не указано имя")]
        [DataType(DataType.Text)]
        [Display(Name = "Имя")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Не указан адрес электронной почты")]
        [EmailAddress]
        [Display(Name = "Электронная почта")]
        public string Email { get; set; }
        
        [Required (ErrorMessage = "Не указан пароль")]
        [StringLength(20, ErrorMessage = "{0} должен быть как минимум {2} и как максимум {1} символов в длинну.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Подтверждение пароля")]
        [Compare("Password", ErrorMessage = "Пароль и подтверждение пароля не сходятся.")]
        public string ConfirmPassword { get; set; }
    }
}