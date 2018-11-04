using System.Collections.Generic;
namespace ImagePacker
{
    internal class Atlas : List<Slice>
    {
        public int Width;
        public int Height;
        public float Scale;
        public Atlas(int width, int height, float scale)
        {
            Width = width;
            Height = height;
            Scale = scale;
        }
    }
}


