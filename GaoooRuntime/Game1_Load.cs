using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using System.Text.Json;
using FontStashSharp;
using System.Xml;

namespace GaoooRuntime
{
    public partial class Game1 : Game
    {
        private string getWsPath(string path)
        {
            if (Path.IsPathRooted(path))
            {
                if (!path.StartsWith(_args.WorkSpace))
                {
                    Console.WriteLine("WARN: Path is not in workspace: " + path);
                }
                return path;
            }
            return Path.Join(_args.WorkSpace, path);
        }

        private JsonDocument loadJson(string path)
        {
            var wspath = getWsPath(path);
            try
            {
                return JsonDocument.Parse(File.ReadAllText(wspath));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.WriteLine("Failed to load json: " + path);
                return null;
            }
        }

        private XmlDocument loadXml(string path)
        {
            var wspath = getWsPath(path);
            try
            {
                var doc = new XmlDocument();
                doc.Load(wspath);
                return doc;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.WriteLine("Failed to load xml: " + path);
                return null;
            }
        }

        private Texture2D loadTexture(string path)
        {
            var wspath = getWsPath(path);
            try
            {
                return Texture2D.FromFile(GraphicsDevice, wspath);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.WriteLine("Failed to load texture: " + path);
                return null;
            }
        }

        private DynamicSpriteFont loadFont(string name, float fontSize)
        {
            var wspath = getWsPath(name);
            try
            {
                var fontsys = new FontSystem();
                fontsys.AddFont(File.ReadAllBytes(wspath));
                return fontsys.GetFont(fontSize);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.WriteLine("Failed to load font: " + name);
                return null;
            }
        }
    }
}
