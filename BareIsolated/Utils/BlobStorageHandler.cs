using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace BookToSpeech.FunctionsApp.Utils
{
    public class BlobStorageHandler
    {
        private CloudBlobContainer container;

        public BlobStorageHandler(string name, string containerName)
        {
            string connectionString = Environment.GetEnvironmentVariable(name);
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            container = blobClient.GetContainerReference(containerName);
        }

        public async Task UploadByPath(string filePath, string blobName)
        {
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobName);
            await blockBlob.UploadFromFileAsync(filePath);
        }

        public async Task UploadContent(string content, string blobName)
        {
            await container.CreateIfNotExistsAsync();
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobName);
            await blockBlob.UploadTextAsync(content);
        }

        public async Task UploadContent(Stream content, string blobName)
        {
            await container.CreateIfNotExistsAsync();
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobName);
            await blockBlob.UploadFromStreamAsync(content);
        }

        public async Task<string> DownloadTextAsync(string blobName)
        {
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobName);
            return await blockBlob.DownloadTextAsync();
        }
    }
}
