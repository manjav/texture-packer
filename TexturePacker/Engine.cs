using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;

namespace ImagePacker
{
    internal class Engine
    {
        static bool[,] tileMap;
        static public void Sort(Atlas atlas)
        {
            atlas.Sort(Comparison);
        }
        static int Comparison(Slice x, Slice y)
        {
            if( y.Area == x.Area )
                return string.Compare(x.Name, y.Name);
            return y.Area - x.Area;
        }

        static public Bitmap Pack(Atlas atlas, bool removeDuplicates, int packing)
        {
            tileMap = new bool[atlas.Width, atlas.Height];
            var atlasLen = atlas.Count;
            var i = 0;
            var d = 0;
            var ret = new Bitmap(atlas.Width, atlas.Height);
            using (Graphics g = Graphics.FromImage(ret))
            {
                foreach (var s in atlas)
                {
                    if( removeDuplicates )
                    {
                        Slice same = FindSameSlice(atlas, s);
                        if( same != null )
                        {
                            s.DestRect = same.DestRect;
                           // Console.WriteLine(s.Name + " and " + same.Name + " are the same!");
                            continue;
                        }
                    }

                    s.DestRect = FindEmptyPoint(atlas, s.DestRect, packing);
                    if( s.DestRect.X < 0 )
                    {
                        Console.WriteLine(s.Name + " " + s.ColoredRect + " not fit in atlas");
                        continue;
                    }
                    g.DrawImage(s.Bitmap, s.DestRect, s.ColoredRect.X, s.ColoredRect.Y, s.ColoredRect.Width, s.ColoredRect.Height, GraphicsUnit.Pixel);
                    i++;
                    if( i > atlasLen * 0.1 * d )
                    {
                        Console.Write(".");
                        d++;
                    }
                }
            }

            /*var kk = passeds.Keys;
            foreach (string k in kk)
                Console.Write(k + ":" + passeds[k] + " ");*/

            return ret;
        }

        static Slice FindSameSlice(Atlas atlas, Slice slice)
        {
            foreach (var s in atlas)
            {
                if (s.Equals(slice))
                    return null;

                if (CompareMemCmp(s.Bitmap, slice.Bitmap))
                    return s;
            }
            return null;
        }
        static Dictionary<string, int> passeds = new Dictionary<string, int>();
        static Point lastReadyPoint = new Point();
        static int tileH = 0;
        static Rectangle FindEmptyPoint(Atlas atlas, Rectangle rect, int packing)
        {
            int w = rect.Width + 1;
            int h = rect.Height + 1;
            if (packing == 0 && lastReadyPoint.Y < atlas.Height)
            {
                //Console.WriteLine(lastReadyPoint.X + " " + rect.Width + " " + atlas.Width);
                if (lastReadyPoint.X + rect.Width >= atlas.Width)
                {
                    lastReadyPoint.X = 0;
                    lastReadyPoint.Y += tileH;
                    tileH = 0;
                    while (lastReadyPoint.Y < atlas.Height && tileMap[lastReadyPoint.X, lastReadyPoint.Y])
                        lastReadyPoint.Y++;
                }
                if (lastReadyPoint.Y + rect.Height < atlas.Height)
                {
                    FillMap(lastReadyPoint.X, lastReadyPoint.Y, lastReadyPoint.X + w, lastReadyPoint.Y + h);
                    rect.X = lastReadyPoint.X;
                    rect.Y = lastReadyPoint.Y;
                    lastReadyPoint.X += w;
                    tileH = Math.Max(tileH, h);
                    return rect;
                }
            }

            string key = w + "x" + h;
            bool exists = passeds.ContainsKey(key);
            for (int j = exists ? passeds[key] : 0 ; j < atlas.Height;  j++)
            {
                for (int i = 0; i < atlas.Width; i++)
                {
                    if( IlligealRegion(i, j, w, h, atlas.Width, atlas.Height) )
                        continue;

                    FillMap(i, j, i + w, j + h);
                    rect.X = i;
                    rect.Y = j;
                    passeds[key] = j;
                    return rect;
                }
            }
            
            rect.X = -1;
            return rect;
        }

        static bool IlligealRegion(int x, int y, int rectWidth, int rectHeight, int width, int height)
        {
            if( tileMap[x, y] || (y + rectHeight) > height || (x + rectWidth) > width )
                return true;
            if( tileMap[x + rectWidth - 1, y + rectHeight - 1] || tileMap[x, y + rectHeight - 1] || tileMap[x + rectWidth - 1, y] )
                return true;
            for (int i = x; i < x + rectWidth; i += 10)
                for (int j = y; j < y + rectHeight; j += 10)
                    if (tileMap[i, j])
                        return true;
            return false;
        }

        static void FillMap(int si, int sj, int di, int dj)
        {
            for (var i = si; i < di; i++)
                for (var j = sj; j < dj; j++)
                    tileMap[i, j] = true;
        }


        [DllImport("msvcrt.dll")]
        private static extern int memcmp(IntPtr b1, IntPtr b2, long count);

        public static bool CompareMemCmp(Bitmap b1, Bitmap b2)
        {
            if ((b1 == null) != (b2 == null)) return false;
            if (b1.Size != b2.Size) return false;

            var bd1 = b1.LockBits(new Rectangle(new Point(0, 0), b1.Size), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            var bd2 = b2.LockBits(new Rectangle(new Point(0, 0), b2.Size), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            try
            {
                IntPtr bd1scan0 = bd1.Scan0;
                IntPtr bd2scan0 = bd2.Scan0;

                int stride = bd1.Stride;
                int len = stride * b1.Height;

                return memcmp(bd1scan0, bd2scan0, len) == 0;
            }
            finally
            {
                b1.UnlockBits(bd1);
                b2.UnlockBits(bd2);
            }
        }


        static public string Serialize(Atlas atlas, String name)
        {
            StringBuilder sb = new StringBuilder("<?xml version=\"1.0\" encoding=\"UTF - 8\"?>\n<TextureAtlas imagePath=\"" + name + "\" width=\"" + atlas.Width + "\" height=\"" + atlas.Height + "\">");
            foreach ( var s in atlas )
            {
                sb.Append("\n\t<SubTexture name=\"");
                sb.Append(s.Name);
                sb.Append("\" x=\"");
                sb.Append(s.DestRect.X);
                sb.Append("\" y=\"");
                sb.Append(s.DestRect.Y);
                sb.Append("\" width=\"");
                sb.Append(s.DestRect.Width);
                sb.Append("\" height=\"");
                sb.Append(s.DestRect.Height);
                
                if( s.DestRect.Width != s.Dimentions.X || s.DestRect.Height != s.Dimentions.Y )
                {
                    sb.Append("\" frameX=\"");
                    sb.Append(-(int)Math.Round(s.ColoredRect.X * atlas.Scale));
                    sb.Append("\" frameY=\"");
                    sb.Append(-(int)Math.Round(s.ColoredRect.Y  * atlas.Scale));
                    sb.Append("\" frameWidth=\"");
                    sb.Append(s.Dimentions.X);
                    sb.Append("\" frameHeight=\"");
                    sb.Append(s.Dimentions.Y);
                }

                sb.Append("\"/>");
            }

            sb.Append("\n</TextureAtlas>");
            return sb.ToString();
        }


        //streaks = new List<Point>;
        //    streaks.Add("0-0", new Point(0, 0));
        //private static Point findEmptyPoint(RectangleF rect, int width, int height)
        //{
        //    Point ret = Point.Empty;
        //    Point foundPoint = Point.Empty;
        //    var sValues = streaks.Values;
        //    foreach (var s in sValues)
        //    {
        //        if (s.X + rect.Width > width || s.Y + rect.Height > height)
        //            continue;
        //        ret.X = s.X;
        //        ret.y = s.Y;
        //        foundPoint = s;
        //        break;
        //    }

        //    if (!foundPoint.Equals(Point.Empty))
        //    {
        //        string key;
        //        for (var i = 0; i < rect.Width; i++)
        //        {
        //            remove old horizontal line
        //            key = (foundPoint.X + i) + "-" + foundPoint.Y;
        //            if (streaks.ContainsKey(key))
        //                streaks.Remove(key);

        //            add new horizontal line
        //            streaks.Add()
        //        }

        //        for (var j = 0; j < rect.Height; j++)
        //        {
        //            remove vertical line
        //           key = foundPoint.X + "-" + (foundPoint.Y + j);
        //            if (streaks.ContainsKey(key))
        //                streaks.Remove(key);

        //            add new horizontal line

        //        }
        //    }
        //    return ret;
        //}


    }

    public class ints
    {
        public int x, y;

        public ints(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }
}