using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;

namespace ThumbnailGenerator
{
    public static class ImageResizer
    {
        [FunctionName("ResizeImage")]
        public static void Run(
            [BlobTrigger("mlsdoctors/{name}", Connection = "AzureWebJobsStorage")] Stream inputBlob,
            [Blob("thumbnail-200x200/{name}", FileAccess.Write)] Stream outputBlob,
            string name, ILogger log)
        {
            // Check the file extension
            string extension = Path.GetExtension(name).ToLowerInvariant();
            if (extension != ".jpg" && extension != ".jpeg" && extension != ".png")
            {
                log.LogWarning($"Unsupported file extension: {extension}");
                return;
            }

            // Load the image and resize it
            using Image image = Image.Load(inputBlob);
            const int height = 200;
            int width = (int)Math.Round(height * (double)image.Width / image.Height);
            image.Mutate(x => x.Resize(width, height, KnownResamplers.Lanczos3));

            string fileName = Path.GetFileNameWithoutExtension(name);

            // Save the resized image to the output stream
            image.SaveAsPng(outputBlob, new PngEncoder());
        }
    }
}
