using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace ImagePacker
{
    class Slice
    {
        public int Area;
        public string Name;
        public Bitmap Bitmap;
        public Point Dimentions;
        public Rectangle DestRect;
        public Rectangle ColoredRect;

        public Slice(string path, string prifix, int folderNameLen, bool trimmed, float scale)
        {
            Name = prifix + path.Substring(folderNameLen, path.Length - folderNameLen - 4).Replace('\\', '/');
            Bitmap = (Bitmap)Image.FromFile(path);

            if( trimmed )
                ColoredRect = GetColoredFast(Bitmap, 0);
            else
                ColoredRect = new Rectangle(0, 0, Bitmap.Width, Bitmap.Height);

            Area = ColoredRect.Width * ColoredRect.Height;
            DestRect = new Rectangle(0, 0, (int) Math.Round(ColoredRect.Width * scale), (int) Math.Round(ColoredRect.Height * scale));
            Dimentions = new Point((int)Math.Round(Bitmap.Width * scale), (int)Math.Round(Bitmap.Height * scale));
        }

        static Rectangle GetColoredFast(Bitmap source, int threshhold)
        {
            Rectangle srcRect = default(Rectangle);
            BitmapData data = null;
            try
            {
                data = source.LockBits(new Rectangle(0, 0, source.Width, source.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                byte[] buffer = new byte[data.Height * data.Stride];
                Marshal.Copy(data.Scan0, buffer, 0, buffer.Length);
                int xMin = int.MaxValue;
                int xMax = 0;
                int yMin = int.MaxValue;
                int yMax = 0;
                for (int y = 0; y < data.Height; y++)
                {
                    for (int x = 0; x < data.Width; x++)
                    {
                        byte alpha = buffer[y * data.Stride + 4 * x + 3];
                        if (alpha > threshhold)
                        {
                            if (x < xMin) xMin = x;
                            if (x > xMax) xMax = x;
                            if (y < yMin) yMin = y;
                            if (y > yMax) yMax = y;
                        }
                    }
                }
                if (xMax < xMin || yMax < yMin)
                {
                    // Image is empty...
                    return Rectangle.Empty;
                }
                srcRect = Rectangle.FromLTRB(xMin, yMin, xMax+1, yMax);
            }
            finally
            {
                if (data != null)
                    source.UnlockBits(data);
            }
            return srcRect;
        }

        /*static public Rectangle GetColored(Bitmap bitmap, int width, int height, int threshhold)
        {
            int leftOffset = 0;
            int topOffset = 0;
            int bottomOffset = 0;
            int rightOffset = 0;

            bool notFound = true;
            // Get left bounds to trim
            while (leftOffset < width && notFound)
            {
                for (int y = 0; y < height && notFound; y++)
                {
                    Color color = bitmap.GetPixel(leftOffset, y);
                    if (color.A > threshhold)
                        notFound = false;
                }
                leftOffset += 1;
            }

            // Get top bounds to trim
            notFound = true;
            while (topOffset < height && notFound)
            {
                for (int x = 0; x < width && notFound; x++)
                {
                    Color color = bitmap.GetPixel(x, topOffset);
                    if (color.A > threshhold)
                        notFound = false;
                }
                topOffset += 1;
            }

            // Get right bounds to trim
            notFound = true;
            while (rightOffset < width && notFound)
            {
                for (int y = 0; y < height && notFound; y++)
                {
                    Color color = bitmap.GetPixel(width - rightOffset - 1, y);
                    if (color.A > threshhold)
                        notFound = false;
                }
                rightOffset += 1;
            }

            // Get bottom bounds to trim
            notFound = true;
            while (bottomOffset < height && notFound)
            {
                for (int x = 0; x < width && notFound; x++)
                {
                    Color color = bitmap.GetPixel(x, height - bottomOffset - 1);
                    if (color.A > threshhold)
                        notFound = false;
                }
                bottomOffset += 1;
            }

            return new Rectangle(leftOffset, topOffset, width - leftOffset - rightOffset, height - topOffset - bottomOffset);
        }*/

    }
}
