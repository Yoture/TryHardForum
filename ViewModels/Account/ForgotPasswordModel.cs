using System.ComponentModel.DataAnnotations;

namespace TryHardForum.ViewModels.Account
{
    public class ForgotPasswordModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}