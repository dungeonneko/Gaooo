using Gaooo;

namespace GaoooRuntime
{
    internal class TaskUIBranch : GaoooTask
    {
        public GaoooTag Source = null;
        public GaoooTarget Target = null;

        public TaskUIBranch(GaoooTag tag) : base(tag)
        {
        }

        public override bool IsEnd()
        {
            return Target != null;
        }

        public override GaoooTask NextTask()
        {
            if (Source != null)
            {
                foreach (var kv in Source)
                {
                    Tag[kv.Key] = kv.Value;
                }
            }

            return Sys.Call(Tag, Target);
        }
    }
}
