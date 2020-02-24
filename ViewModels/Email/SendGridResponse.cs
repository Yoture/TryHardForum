using System.Collections.Generic;

namespace TryHardForum.ViewModels.Email
{
    // Модель представления.
    public class SendGridResponse
    {
        public List<SendGridResponseError> Errors { get; set; }
    }
}