namespace TryHardForum.ViewModels.Email
{
    // модель представления?
    public class SendGridResponseError
    {
        // Сообщение об ошибке.
        public string Message { get; set; }
        
        // Поле внутри письма. Сюда же относятся ошибки.
        public string Field { get; set; }
        
        // Полезная информация о том, как исправить ошибку.
        public string Help { get; set; }
    }
}