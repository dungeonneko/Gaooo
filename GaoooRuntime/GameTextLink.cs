using Gaooo;

namespace GaoooRuntime
{
    public class GameTextLink
    {
        public int Index { get; private set; }
        public int Line { get; set; }
        public int Pos { get; private set; }
        public GaoooTag Tag { get; private set; }
        public string Text { get; set; }

        public GameTextLink(int index, int line, int pos, GaoooTag tag)
        {
            Index = index;
            Line = line;
            Pos = pos;
            Tag = tag;
            Text = string.Empty;
        }
    }
}
