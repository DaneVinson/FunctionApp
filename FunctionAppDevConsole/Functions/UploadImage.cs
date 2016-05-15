using Microsoft.Azure;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace FunctionAppDevConsole.Functions
{
    public static class UploadImage
    {
        public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
        {
            var imagePath = req.GetQueryNameValuePairs()
                                    .FirstOrDefault(q => "image".Equals(q.Key, StringComparison.OrdinalIgnoreCase))
                                    .Value;
            log.Info($"Image URI query argument: {imagePath}");

            // Validate the image URI argument.
            Uri imageUri;
            if (!Uri.TryCreate(imagePath, UriKind.Absolute, out imageUri))
            {
                string message = $"The image URI {imagePath} is not valid.";
                log.Error(message);
                return req.CreateResponse(HttpStatusCode.BadRequest, message);
            }

            // Validate an image name and rename for uniqueness.
            string[] nameParts = imagePath.Split('/').Last().Split('.');
            string extension = nameParts.Last();
            if (!(new string[] { "bmp", "gif", "jpg", "png" }).Any(e => e.Equals(extension)))
            {
                string message = $"The resource is not an image.";
                log.Error(message);
                return req.CreateResponse(HttpStatusCode.BadRequest, message);
            }
            string blobName = $"{String.Join(".", nameParts.Take(nameParts.Length - 1))}_{Guid.NewGuid().ToString()}.{extension}".ToLower();

            // Open the HttpClient, download the image then upload it to Azure Storage.
            CloudBlockBlob blockBlob = GetCloudBlockBlob(blobName);
            using (var httpClient = new HttpClient())
            using (var response = await httpClient.GetAsync(imageUri))
            using (var stream = await response.Content.ReadAsStreamAsync())
            {
                await blockBlob.UploadFromStreamAsync(stream);
            }

            // Return the new blob URI.
            string msg = $"BLOB added: {blockBlob.Uri}";
            log.Info(msg);
            return req.CreateResponse(HttpStatusCode.OK, msg);
        }

        private static CloudBlockBlob GetCloudBlockBlob(string blobName)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("images");
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobName);
            return blockBlob;
        }
    }
}
