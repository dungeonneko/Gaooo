using FontStashSharp;
using Gaooo;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GaoooRuntime
{
    public partial class Game1 : Game
    {
        private Texture2D getTexture(string key)
        {
            if (!_textures.ContainsKey(key))
            {
                _textures[key] = loadTexture($"{key}.png");
            }

            return _textures[key];
        }

        private DynamicSpriteFont getFont(string name, float fontSize)
        {            
            var key = new KeyValuePair<string, float>(name, fontSize);
            if (!_fonts.ContainsKey(key))
            {
                _fonts[key] = loadFont(name, fontSize);
            }

            return _fonts[key];
        }

        private void drawLayer(GaoooLayer layer)
        {
            drawObject(layer.RootObject, 0.0f, 0.0f, 0.0f);
        }

        private void drawObject(GaoooLayerObject obj, float parentX, float parentY, float parentZ)
        {
            var x = parentX + obj.GetPropertyOrDefault<float>("pos_x", 0.0f);
            var y = parentY + obj.GetPropertyOrDefault<float>("pos_y", 0.0f);
            var z = parentZ + obj.GetPropertyOrDefault<float>("pos_z", 0.0f);
            var rotZ = obj.GetPropertyOrDefault<float>("rot_z", 0.0f);
            var scaleX = obj.GetPropertyOrDefault<float>("scale_x", 1.0f);
            var scaleY = obj.GetPropertyOrDefault<float>("scale_y", 1.0f);
            if (obj.Tag.Type == "sprite")
            {
                var img = obj.GetProperty<string>("image");
                var tex = getTexture(img);
                if (tex != null)
                {
                    var width = tex.Width * scaleX;
                    var height = tex.Height * scaleY;
                    var pivotX = tex.Width / 2;
                    var pivotY = tex.Height / 2;

                    var srcRect = new Rectangle(0, 0, tex.Width, tex.Height);
                    var dstRect = new Rectangle((int)(x), (int)(y), (int)width, (int)height);

                    _spriteBatch.Draw(tex, dstRect, srcRect, Color.White, rotZ * MathF.PI / 180.0f, new Vector2(pivotX, pivotY), SpriteEffects.None, 0.0f);
                }
            }

            foreach (var c in obj.Children)
            {
                drawObject(c.Value, x, y, z);
            }
        }

        private void drawText(DynamicSpriteFont font, string text, float px, float py)
        {
            var lines = text.Split('\n').ToList();
            var lastLine = lines.Count - 1;
            for (var line = 0; line <= lastLine; ++line)
            {
                var s = lines[line];
                _spriteBatch.DrawString(font, s, new Vector2(px, py), Color.White);
                py += font.LineHeight;
            }
        }

        private void drawWindow(Texture2D tex, Rectangle inner, Color color)
        {
            _spriteBatch.Draw(tex, inner, new Rectangle(8, 8, 8, 8), color);
            _spriteBatch.Draw(tex, new Rectangle(inner.X - 8, inner.Y - 8, 8, 8), new Rectangle(0, 0, 8, 8), color);
            _spriteBatch.Draw(tex, new Rectangle(inner.X + inner.Width, inner.Y - 8, 8, 8), new Rectangle(16, 0, 8, 8), color);
            _spriteBatch.Draw(tex, new Rectangle(inner.X - 8, inner.Y + inner.Height, 8, 8), new Rectangle(0, 16, 8, 8), color);
            _spriteBatch.Draw(tex, new Rectangle(inner.X + inner.Width, inner.Y + inner.Height, 8, 8), new Rectangle(16, 16, 8, 8), color);
            _spriteBatch.Draw(tex, new Rectangle(inner.X, inner.Y - 8, inner.Width, 8), new Rectangle(8, 0, 8, 8), color);
            _spriteBatch.Draw(tex, new Rectangle(inner.X, inner.Y + inner.Height, inner.Width, 8), new Rectangle(8, 16, 8, 8), color);
            _spriteBatch.Draw(tex, new Rectangle(inner.X - 8, inner.Y, 8, inner.Height), new Rectangle(0, 8, 8, 8), color);
            _spriteBatch.Draw(tex, new Rectangle(inner.X + inner.Width, inner.Y, 8, inner.Height), new Rectangle(16, 8, 8, 8), color);
        }

        //private void drawBg()
        //{
        //    var srcRect = GraphicsDevice.Viewport.Bounds;
        //    srcRect.X = _bgScroll.X;
        //    srcRect.Y = _bgScroll.Y;
        //    var tex = getTexture(_bgScroll.Texture);
        //    _spriteBatch.Draw(tex, GraphicsDevice.Viewport.Bounds, srcRect, Color.White);
        //}
    }
}