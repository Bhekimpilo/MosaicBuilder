using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Drawing.Imaging;

namespace MosaicBuilder
{
    public class BuildMosaic
    {
        private Image _image;
        private string _tileFolder;
        private Size _size = new Size(24, 24);

        public BuildMosaic(Image image, string tilefolder)
        {
            this._image = image;
            this._tileFolder = tilefolder;
        }

        public List<TileProperty> AnalysePictures(string tileFolder)
        {
            //This method extracts the information of each picture and stores it in a List

            var size = _size;
            var image = _image;
            string[] imageBatch = Directory.GetFiles(tileFolder, "*.jpg", SearchOption.AllDirectories);

            int resizeWidth = image.Width / size.Width, resizeHeight = image.Height / size.Height;
            int count = 0;

            //Returns a list of objects with average-color && image_path.
            //This way we do not keep analysing each image when it's time to compare
            List<TileProperty> imageProperties = new List<TileProperty>();

            foreach (var imagePath in imageBatch)
            {
                //Reducing image for better accuracy and faster color averaging
                var imageToResize = Image.FromFile(imagePath);

                var bmp = new Bitmap(resizeWidth, resizeHeight);
                using (var g = Graphics.FromImage(bmp))
                {
                    var rect = new Rectangle(0, 0, resizeWidth, resizeHeight);
                    g.DrawImage(imageToResize, rect);
                }

                imageProperties.Add(new TileProperty
                {
                    Color = GetAverageColor(bmp),
                    Path = imagePath
                });

                var total = imageBatch.Count();
                Console.WriteLine(String.Format("Analysing file {0} of {1}", ++count, total));
            }

            return imageProperties;
        }

        public Color[,] GenerateTiles(Image inputImage)
        {
            var size = _size;
            inputImage = _image;

            int tilewidth = inputImage.Width / size.Width;
            int tileHeight = inputImage.Height / size.Height;
            int count = 0;

            //Creating a color map to store each tile's position and average color
            Color[,] colorMap = new Color[size.Width, size.Height];

            Console.WriteLine("Generating tiles...");

            for (int x = 0; x < size.Width; x++)
            {
                for (int y = 0; y < size.Height; y++)
                {
                    int xPos = x * tilewidth, yPos = y * tileHeight;

                    var bmp = new Bitmap(tilewidth, tileHeight);
                    using (var gr = Graphics.FromImage(bmp))
                    {
                        var rect = new Rectangle(xPos, yPos, tilewidth, tileHeight);
                        gr.DrawImage(inputImage, new Rectangle(0, 0, tilewidth, tileHeight), rect, GraphicsUnit.Pixel);
                    }

                    var avCol = GetAverageColor(bmp);

                    colorMap[x, y] = Color.FromArgb(avCol.R, avCol.G, avCol.B);

                    Console.WriteLine(String.Format("Generated {0} tiles of {1}", ++count, size.Width * size.Height));
                }

            }

            return colorMap;
        }

        public Color GetAverageColor(Bitmap bmp)
        {
            int r = 0, g = 0, b = 0, count = 0;

            for (int x = 0; x < bmp.Width; x++)
            {
                for (int y = 0; y < bmp.Height; y++)
                {
                    var pixel = bmp.GetPixel(x, y);

                    r += pixel.R;
                    g += pixel.G;
                    b += pixel.B;
                    count++;
                }
            }

            return Color.FromArgb(r / count, g / count, b / count);
        }

        public void RenderImage(string outputFileName)
        {
            
            var size = _size;
            var albumDir = _tileFolder;
            var image = _image;
            
            var tiles = GenerateTiles((Bitmap)image);
            var pictureTiles = AnalysePictures(albumDir);
            int count = 0;

            int tileWidth = image.Width / size.Width, tileHeight = image.Height / size.Height;

            var mosaic = new Bitmap(image.Width - (image.Width % tileWidth), image.Height - (image.Height % tileWidth));
            var g = Graphics.FromImage(mosaic);

            

            for (int x = 0; x < tiles.GetLength(0); x++)
            {
                for (int y = 0; y < tiles.GetLength(1); y++)
                {
                    int xPos = x * tileWidth, yPos = y * tileHeight;

                    var tile = tiles[x, y];

                    var color = Color.FromArgb(tile.R, tile.G, tile.B);
                    var comp = new ColorComparison(color, pictureTiles);

                    var imagePath = comp.Compare();

                    using (var closestImage = Image.FromFile(imagePath))
                    {
                        var source = new Rectangle(0, 0, closestImage.Width, closestImage.Height);
                        var dest = new Rectangle(xPos, yPos, tileWidth, tileHeight);

                        g.DrawImage(closestImage, dest, source, GraphicsUnit.Pixel);
                    }

                    count++;
                    Console.WriteLine(String.Format("Matching tile {0} of {1}", count, tiles.GetLength(0) * tiles.GetLength(1)));
                }
            }
            g.Dispose();

            var outputFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

            if (outputFileName.Trim() == "" || outputFileName == null)
                outputFileName = "TestImage";

            var outputPath = outputFolder + @"\" + outputFileName  + ".jpg";

            mosaic.Save(outputPath);

            Console.WriteLine("\nImage saved to " + outputPath);
            Process.Start(outputPath);
            Console.Read();

        }

    }
}
