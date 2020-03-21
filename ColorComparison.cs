using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using System.Diagnostics;

namespace MosaicBuilder
{
    public class ColorComparison
    {
        Color _tile;
        List<TileProperty> _picTiles;

        public ColorComparison(Color tile, List<TileProperty> picTiles)
        {
            this._tile = tile;
            this._picTiles = picTiles;
        }

        public C_Lab RGBtoCLab(int R, int G, int B)
        {
            //RGB to XYZ
            var _R = (double)R / 255;
            var _G = (double)G / 255;
            var _B = (double)B / 255;

            if (_R > 0.04045)
                _R = Math.Pow((_R + 0.055) / 1.055, 2.4);
            else
                _R /= 12.92;

            if (_G > 0.04045)
                _G = Math.Pow((_G + 0.055) / 1.055, 2.4);
            else
                _G /= 12.92;

            if (_B > 0.04045)
                _B = Math.Pow((_B + 0.055) / 1.055, 2.4);
            else
                _B /= 12.92;

            _R = _R * 100;
            _G = _G * 100;
            _B = _B * 100;

            var x = (_R * 0.4124) + (_G * 0.3576) + (_B * 0.1805);
            var y = (_R * 0.2126) + (_G * 0.7152) + (_B * 0.0722);
            var z = (_R * 0.0193) + (_G * 0.1192) + (_B * 0.9505);

            return XYZtoCLab(x, y, z);

        }


        public C_Lab XYZtoCLab(double X, double Y, double Z)
        {
            if (X > 0.008856)
                X = Math.Pow(X, (1 / 3.0));
            else
                X = (7.787 * X) + (16 / 116.0);
            if (Y > 0.008856)
                Y = Math.Pow(Y, (1 / 3.0));
            else
                Y = (7.787 * Y) + (16 / 116.0);
            if (Z > 0.008856)
                Z = Math.Pow(Z, (1 / 3.0));
            else
                Z = (7.787 * Z) + (16 / 116.0);

            return new C_Lab
            {
                Cie_L = (116 * Y) - 16,
                Cie_A = 500 * (X - Y),
                Cie_B = 200 * (Y - Z)

            };

        }

        public string Compare()
        {
            int shortestDist = 0;
            bool hasChanged = false;
            string imagepath = null;

            var c1 = RGBtoCLab(_tile.R, _tile.G, _tile.B);

            foreach (var pt in _picTiles)
            {
                var c2 = RGBtoCLab(pt.Color.R, pt.Color.G, pt.Color.B);

                double DeltaE = Math.Sqrt(Math.Pow((c1.Cie_L - c2.Cie_L), 2) + Math.Pow((c1.Cie_A - c2.Cie_A), 2) + Math.Pow((c1.Cie_B - c2.Cie_B), 2));

                if (!hasChanged)
                {
                    shortestDist = Convert.ToInt16(Math.Round(DeltaE));
                    imagepath = pt.Path;
                    hasChanged = true;
                }

                if (shortestDist > Math.Round(DeltaE))
                {
                    shortestDist = Convert.ToInt16(Math.Round(DeltaE));
                    imagepath = pt.Path;
                }

            }
            return imagepath;
        }

    }
}
