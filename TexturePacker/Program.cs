using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;

namespace ImagePacker
{
    class Program
    {
        static void Main(string[] args)
        {
            float _scale = 1;
            int _width = 1024;
            int _height = 1024;
            string _atlasImage = "atlas.png";
            string _atlasData = "atlas.xml";
            string _source = "images";
            bool _trim = true;
            bool _removeDuplicates = true;
            for ( var i = 0;  i < args.Length; i++ )
            {
                if( args[i].Substring(0, 1) != "-" )
                    continue;
                switch( args[i] )
                {
                    case "-atlas": _atlasImage = args[i + 1]; break;
                    case "-data": _atlasData = args[i + 1]; break;
                    case "-source": _source = args[i + 1]; break;
                    case "-width":  _width = int.Parse(args[i + 1]); break;
                    case "-height": _height = int.Parse(args[i + 1]); break;
                    case "-scale": _scale = float.Parse(args[i + 1]); break;
                    case "-trim": _trim = bool.Parse(args[i + 1]); break;
                    case "-removeDuplicates": _removeDuplicates = bool.Parse(args[i + 1]); break;
                }
            }

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            var imgs = Directory.GetFiles(_source, "*.*", SearchOption.AllDirectories).Where(s => s.EndsWith(".png") || s.EndsWith(".tif")).ToList();
            var atlas = new Atlas(_width, _height, _scale);
            Console.WriteLine("loading images ...");
            var prefix = _source.Length + 1;
            foreach (var i in imgs)
                atlas.Add(item: new Slice(i, prefix, _trim, atlas.Scale));

            Console.WriteLine("subtextures created " + stopwatch.ElapsedMilliseconds + " ms.");
            Engine.Sort(atlas);

            Console.WriteLine("packing started ");
            Bitmap atlasBMP = Engine.Pack(atlas, _removeDuplicates);

            Console.WriteLine(" in: " + stopwatch.ElapsedMilliseconds + " ms.");
            atlasBMP.Save(_atlasImage);
            
            File.WriteAllText(_atlasData, Engine.Serialize(atlas, _atlasImage));

            Console.WriteLine("finished.");
//            Console.ReadKey();
        }
    }
}
