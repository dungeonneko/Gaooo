using Guooo;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GaoooRuntime
{
    public partial class Game1 : Game
    {
        private void drawGUIComponent(GuoooComponent comp)
        {
            if (comp.PaintEvent == null)
            {
                paintComponent(comp);
            }
            else
            {
                comp.PaintEvent(comp);
            }

            foreach (var c in comp.Children)
            {
                drawGUIComponent(c);
            }
        }

        private void paintComponent(GuoooComponent comp)
        {
            if (comp.BlankTextHide && string.IsNullOrEmpty(comp.Text))
            {
                return;
            }

            var rect = comp.Rect.ToXna();
            var color = comp.MulColor.ToXna();

            if (!string.IsNullOrEmpty(comp.WindowTexture))
            {
                var tex = getTexture(comp.WindowTexture);
                if (tex != null)
                {
                    drawWindow(tex, rect, color);
                }
            }

            if (!string.IsNullOrEmpty(comp.Image))
            {
                var tex = getTexture(comp.Image);
                if (tex != null)
                {
                    _spriteBatch.Draw(tex, new Vector2(comp.X, comp.Y), color);
                }
            }

            if (!string.IsNullOrEmpty(comp.Text))
            {
                var font = getFont(comp.Font, comp.FontSize);
                if (font != null)
                {
                    _spriteBatch.End();
                    GraphicsDevice.ScissorRectangle = new Rectangle(rect.X, rect.Y, rect.Width + 1, rect.Height + 1);
                    var offsetY = 0.0f;
                    _spriteBatch.Begin(samplerState: SamplerState.PointClamp, rasterizerState: _scissor);
                    foreach (var line in comp.Text.Split("\\n"))
                    {
                        drawText(font, line, rect.X, rect.Y + comp.TextOffsetY + offsetY);
                        offsetY += font.LineHeight;
                    }
                    _spriteBatch.End();
                    _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
                }
            }
        }

        private void paintMessage(GuoooComponent comp, GameText text)
        {
            paintComponent(comp);

            var font = getFont(comp.Font, comp.FontSize);
            if (font != null)
            {
                var rect = comp.Rect.ToXna();
                _spriteBatch.End();
                GraphicsDevice.ScissorRectangle = new Rectangle(rect.X, rect.Y, rect.Width + 1, rect.Height + 1);
                _spriteBatch.Begin(samplerState: SamplerState.PointClamp, rasterizerState: _scissor);
                drawText(font, text.Text, rect.X, rect.Y - (float)(text.ScrollRate * font.LineHeight));
                _spriteBatch.End();
                _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            }
        }
    }
}