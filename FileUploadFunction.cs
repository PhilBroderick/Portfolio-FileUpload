using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;

namespace BlobFileUpload
{
    public static class FileUploadFuncton
    {
        [FunctionName("FileUploadFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Admin, "post", Route = null)] HttpRequest req,
            [Blob("images", FileAccess.Write)] CloudBlobContainer blobContainer,
            ILogger log)
        {
            var formFile = req.Form.Files["upload"];

            if (formFile is null)
                return new BadRequestObjectResult("No image found in request");

            var fileName = formFile.FileName;
            if (fileName == "image.png")
                fileName = $"image-{Guid.NewGuid().ToString()}.png";
            
            await using (var stream = formFile.OpenReadStream())
            {
                var blob = blobContainer.GetBlockBlobReference(fileName);
                await blob.UploadFromStreamAsync(stream);
            }

            var imageStorageContainer = Environment.GetEnvironmentVariable("ImageStorageContainer");
            return new OkObjectResult(new { url= $"{imageStorageContainer}/{fileName}" });
        }
    }
}
