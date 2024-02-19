using System.Collections.Generic;
using System.IO;

namespace Gaooo
{
    public class GaoooFilePath
    {
        public static readonly GaoooFilePath Empty = new GaoooFilePath(null, string.Empty);
        public string RelPath { get; init; }
        public string AbsPath { get; init; }
        public bool IsEmpty { get { return string.IsNullOrEmpty(RelPath); } }
        public override string ToString() { return RelPath; }

        public GaoooFilePath(GaoooSystem? gaosys, string path)
        {
            if (gaosys != null)
            {
                if (Path.IsPathRooted(path))
                {
                    path = gaosys.CallbackOnGetRelPath(path);
                }
                RelPath = path;
                AbsPath = gaosys.CallbackOnGetAbsPath(path);
            }
            else
            {
                RelPath = string.Empty;
                AbsPath = string.Empty;
            }
        }
    }

    class GaoooFilePathComparer : IEqualityComparer<GaoooFilePath>
    {
        public bool Equals(GaoooFilePath? x, GaoooFilePath? y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
            {
                return false;
            }

            return x.RelPath == y.RelPath;
        }

        public int GetHashCode(GaoooFilePath? obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return 0;
            }

            return obj.RelPath.GetHashCode();
        }
    }
}
