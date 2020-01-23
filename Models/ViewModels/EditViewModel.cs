using System.ComponentModel.DataAnnotations;

namespace TryHardForum.Models.ViewModels
{
    public class EditViewModel
    {
        public string Id { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}