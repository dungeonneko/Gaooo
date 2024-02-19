using GaoooDebugger;

namespace Gaooo
{
    public class GaoooTarget
    {
        public GaoooTag Caller { get; init; }
        public GaoooFilePath FilePath { get; init; }
        public int LineNumber { get; init; }
        public string Label { get; init; }

        public GaoooTarget(GaoooSystem sys) {
            Caller = GaoooTag.Empty;
            FilePath = GaoooFilePath.Empty;
            LineNumber = -1;
            Label = string.Empty;
        }

        // jump / call / link target
        public GaoooTarget(GaoooFilePath filePath, string label)
        {
            Caller = GaoooTag.Empty;
            FilePath = filePath;            
            LineNumber = -1;
            Label = label;
        }

        // jump from program
        public GaoooTarget(GaoooFilePath filePath, int lineNumber)
        {
            Caller = GaoooTag.Empty;
            FilePath = filePath;            
            LineNumber = lineNumber;
            Label = string.Empty;
        }

        // callstack
        public GaoooTarget(GaoooTag caller)
        {
            Caller = caller;
            FilePath = caller.FilePath;
            LineNumber = caller.LineNumber;
            Label = string.Empty;
        }

        // load from data
        public GaoooTarget(GaoooFilePath filepPath, string label, int lineNumber)
        {
            Caller = GaoooTag.Empty;
            FilePath = filepPath;
            LineNumber = lineNumber;
            Label = label;
        }

        public static GaoooTarget FromTargetTag(GaoooTag tag)
        {
            var storage = tag.GetAttrValue("storage", tag.FilePath.RelPath);
            var label = tag.GetAttrValue("target", string.Empty);
            return new GaoooTarget(new GaoooFilePath(tag.Sys, storage), label);
        }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(Label))
            {
                return FilePath + " <" + Label + ">";
            }
            return FilePath + " (" + LineNumber.ToString() + ")";
        }
    }
}
