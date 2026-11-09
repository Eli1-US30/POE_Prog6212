using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace RaceDay.API.Services
{
    public class BlobService : IBlobService
    {
        private readonly BlobContainerClient _container;

        public BlobService(IConfiguration config)
        {
            var options = new BlobClientOptions();
            options.Retry.MaxRetries = 1;                          // fail fast if storage is down
            options.Retry.NetworkTimeout = TimeSpan.FromSeconds(10);

            var service = new BlobServiceClient(config["AzureStorage:ConnectionString"], options);
            _container = service.GetBlobContainerClient(config["AzureStorage:ContainerName"] ?? "raceday-images");
        }

        public async Task<string> UploadImageAsync(Stream content, string contentType, string folder)
        {
            // Public read access so the browser can show the image straight from its URL
            await _container.CreateIfNotExistsAsync(PublicAccessType.Blob);

            var extension = contentType switch { "image/png" => ".png", "image/webp" => ".webp", _ => ".jpg" };
            var blob = _container.GetBlobClient($"{folder}/{Guid.NewGuid()}{extension}");

            await blob.UploadAsync(content, new BlobHttpHeaders { ContentType = contentType });
            return blob.Uri.ToString();
        }
    }
}