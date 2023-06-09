using System;
using System.Collections.Generic;
using System.Drawing;
using Vintagestory.API.Common;
using Vintagestory.API.Util;

namespace jopainting
{
    public class PaintingBitmap : IBitmap
    {
        int width = 32;
        int height = 32;

        public int Width => width;

        public int Height => height;

        public Bitmap bitmapRed;
        public Bitmap bitmapGreen;
        public Bitmap bitmapBlue;

        public byte[] pixelsRed;
        public byte[] pixelsGreen;
        public byte[] pixelsBlue;

        public int[] Pixels => GetBitmapAsInts();

        public PaintingBitmap()
        {
            bitmapRed = new Bitmap(width, height);
            bitmapGreen = new Bitmap(width, height);
            bitmapBlue = new Bitmap(width, height);

            pixelsRed = new byte[width * height];
            pixelsGreen = new byte[width * height];
            pixelsBlue = new byte[width * height];
        }

        public Color GetPixel(int x, int y)
        {
            return Color.FromArgb(bitmapRed.GetPixel(Math.Min(x, bitmapRed.Width - 1), Math.Min(y, bitmapRed.Height - 1)).R * (byte)2,
                bitmapGreen.GetPixel(Math.Min(x, bitmapGreen.Width - 1), Math.Min(y, bitmapGreen.Height - 1)).G * (byte)2,
                bitmapBlue.GetPixel(Math.Min(x, bitmapBlue.Width - 1), Math.Min(y, bitmapBlue.Height - 1)).B * (byte)2);
        }

        public Color GetPixelRel(float x, float y)
        {
            return GetPixel((int)((float)width * x), (int)((float)height * x));
        }

        public int[] GetPixelsTransformed(int rot = 0, int alpha = 100)
        {
            return GetBitmapAsInts();
        }

        int[] GetBitmapAsInts()
        {
            List<int> pixels = new();
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    pixels.Add(GetPixel(x, y).ToArgb());
                }
            }
            return pixels.ToArray();
        }

        public void SetBitmapRGB(Bitmap bmpR, Bitmap bmpG, Bitmap bmpB)
        {
            width = bmpR.Width;
            height = bmpR.Height;

            int edge = Math.Min(width, height);
            int longEdge = Math.Max(width, height);
            bool isHeightEdge = height <= width;
            int deadshift = (longEdge - edge) / 2;

            List<byte> pixelsByteR = new();
            List<byte> pixelsByteG = new();
            List<byte> pixelsByteB = new();

            if (isHeightEdge)
            {
                width = height;
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < height; x++)
                    {
                        pixelsByteR.Add((byte)(bmpR.GetPixel(x + deadshift, y).R / (byte)2));
                        pixelsByteG.Add((byte)(bmpG.GetPixel(x + deadshift, y).R / (byte)2));
                        pixelsByteB.Add((byte)(bmpB.GetPixel(x + deadshift, y).R / (byte)2));
                    }
                }
            }

            pixelsRed = pixelsByteR.ToArray();
            pixelsGreen = pixelsByteG.ToArray();
            pixelsBlue = pixelsByteB.ToArray();

            bitmapRed = BitmapUtil.GrayscaleBitmapFromPixels(pixelsRed, width, height);
            bitmapGreen = BitmapUtil.GrayscaleBitmapFromPixels(pixelsGreen, width, height);
            bitmapBlue = BitmapUtil.GrayscaleBitmapFromPixels(pixelsBlue, width, height);
        }

        public void SetBitmap(Bitmap bmp)
        {
            width = bmp.Width;
            height = bmp.Height;

            int edge = Math.Min(width, height);
            int longEdge = Math.Max(width, height);
            bool isHeightEdge = height <= width;
            int deadshift = (longEdge - edge) / 2;

            List<byte> pixelsByteR = new();
            List<byte> pixelsByteG = new();
            List<byte> pixelsByteB = new();

            if (isHeightEdge)
            {
                width = height;
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < height; x++)
                    {
                        pixelsByteR.Add((byte)(bmp.GetPixel(x + deadshift, y).R / (byte)2));
                        pixelsByteG.Add((byte)(bmp.GetPixel(x + deadshift, y).G / (byte)2));
                        pixelsByteB.Add((byte)(bmp.GetPixel(x + deadshift, y).B / (byte)2));
                    }
                }
            }

            pixelsRed = pixelsByteR.ToArray();
            pixelsGreen = pixelsByteG.ToArray();
            pixelsBlue = pixelsByteB.ToArray();

            bitmapRed = BitmapUtil.GrayscaleBitmapFromPixels(pixelsRed, width, height);
            bitmapGreen = BitmapUtil.GrayscaleBitmapFromPixels(pixelsGreen, width, height);
            bitmapBlue = BitmapUtil.GrayscaleBitmapFromPixels(pixelsBlue, width, height);
        }
    }
}
