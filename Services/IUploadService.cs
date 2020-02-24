using Microsoft.WindowsAzure.Storage.Blob;

namespace TryHardForum.Services
{
    public interface IUploadService
    {
        // Тут полный треш, и потому я буду просто писать код.
        CloudBlobContainer GetBlobContainer(string connectionString);
    }
}