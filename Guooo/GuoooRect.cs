using System;

namespace Guooo
{
    public struct GuoooRect
    {
        public float Left;
        public float Top;
        public float Right;
        public float Bottom;
        public float Width { get { return Right - Left; } }
        public float Height { get { return Bottom - Top; } }

        public static GuoooRect Parse(string str)
        {
            var parts = str.Split(',');
            if (parts.Length != 4)
            {
                throw new Exception("Invalid rect format");
            }
            return new GuoooRect(
                float.Parse(parts[0]),
                float.Parse(parts[1]),
                float.Parse(parts[2]),
                float.Parse(parts[3]));
        }

        public GuoooRect(float l, float t, float r, float b)
        {
            Left = l;
            Top = t;
            Right = r;
            Bottom = b;
        }

        public bool Contains(float x, float y)
        {
            return x >= Left && x <= Right && y >= Top && y <= Bottom;
        }
    }
}
