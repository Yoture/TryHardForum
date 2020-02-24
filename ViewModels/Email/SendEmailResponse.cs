using System.Collections.Generic;

namespace TryHardForum.ViewModels.Email
{
    // Это обычная модель представления.
    public class SendEmailResponse
    {
        // True если письмо успешно доставлено
        public bool Successful => !(Errors?.Count > 0);
        
        // Сообщение об ошибке, если отправка письма не произошла.
        public List<string> Errors { get; set; }
    }
}