using ImageResizer;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FunctionAppDevConsole.Functions
{
    public static class MakeThumbnail
    {
        public static void Run(Stream image, Stream thumbnailImage)
        {
            ImageBuilder.Current.Build(
                                    image, 
                                    thumbnailImage,
                                    new ResizeSettings(128, 128, FitMode.Crop, null));
        }
    }
}
