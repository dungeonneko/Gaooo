using Gaooo;
using Microsoft.Xna.Framework;

namespace GaoooRuntime
{
    public partial class Game1 : Game
    {
        private GaoooTask onTextWaitNextParagraph(GaoooTag tag)
        {
            return new TaskWaitUserInput(onGetOK, tag);
        }

        private GaoooTask onTextWaitNextPage(GaoooTag tag)
        {
            return new TaskWaitUserInput(onGetOK, tag);
        }

        private GaoooTask onTextNewLine(GaoooTag tag)
        {
            _text.Add('\n');
            return null;
        }

        private GaoooTask onTextPut(GaoooTag tag)
        {
            return new TaskText(onGetOK, _text, tag, tag.GetAttrValue<string>("text"));
        }

        private GaoooTask onTextClear(GaoooTag tag)
        {
            _text.Clear();
            return null;
        }

        private GaoooTask onTextBeginBranch(GaoooTag tag)
        {
            _text.BranchBegin(tag);
            return null;
        }

        private GaoooTask onTextEndBranch(GaoooTag tag)
        {
            return new TaskBranch(_input, _text, tag);
        }

        private GaoooTask onTextBeginLink(GaoooTag tag)
        {
            _text.LinkBegin(tag);
            return null;
        }

        private GaoooTask onTextEndLink(GaoooTag tag)
        {
            _text.LinkEnd(tag);
            return null;
        }
    }
}
