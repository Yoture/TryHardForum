using System.ComponentModel.DataAnnotations;

namespace TryHardForum.ViewModels.Manage
{
    public class ManageIndexModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }

        public bool IsEmailConfirmed { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }
        public string StatusMessage { get; set; }
    }
}