using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using System.Diagnostics;
using System.Threading;

namespace MosaicBuilder
{
    class Program
    {
        static void Main(string[] args)
        {            
            var file = @""; //Path to image
            var tileFolder = @""; //Folder with images
            var outputfileName = ""; //Optional. Default name is TestImage

            Console.WriteLine("Starting...");

            if (file == "" || file == null || !File.Exists(file)) //ToDo: Check if file exists
            {
                Console.WriteLine("Image could not be found. Check the path. \nExiting...");
                Thread.Sleep(3000);
                Environment.Exit(0);
            }
           
            if (tileFolder == "" || tileFolder == null || !Directory.Exists(tileFolder))
            {
                Console.WriteLine("The image tile directory you provided does not exist or is empty. \nExiting...");
                Thread.Sleep(3000);
                Environment.Exit(0);
            }
            
            BuildMosaic buildMosaic = new BuildMosaic(Image.FromFile(file), tileFolder);

            //Provide a name for the output Image
            buildMosaic.RenderImage(outputfileName);
        }

    }
}





