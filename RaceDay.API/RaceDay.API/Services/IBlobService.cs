using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace RaceDay.API.Services
{
    public interface IBlobService
    {
        Task<string> UploadImageAsync(Stream content, string contentType, string folder);
    }
}
