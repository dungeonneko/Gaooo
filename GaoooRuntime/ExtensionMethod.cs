using Microsoft.Xna.Framework;
using Guooo;

namespace GaoooRuntime
{
    public static class ExtensionMethod
    {
        public static Rectangle ToXna(this GuoooRect rect)
        {
            return new Rectangle((int)rect.Left, (int)rect.Top, (int)rect.Width, (int)rect.Height);
        }

        public static Color ToXna(this GuoooColor color)
        {
            return new Color(color.R, color.G, color.B, color.A);
        }
    }
}
