using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage;

namespace TryHardForum.Services
{
    class UploadService : IUploadService
    {
        public CloudBlobContainer GetBlobContainer(string connectionString)
        {
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();

            // profile-images является названием блоба на облачном сервисе.
            return blobClient.GetContainerReference("images");
        }
    }
}