namespace Gaooo
{
    internal class GaoooTaskWt : GaoooTask
    {
        internal GaoooTaskWt(GaoooTag tag) : base(tag)
        {
        }

        public override void Update(double dt)
        {
        }

        public override bool IsEnd()
        {
            return Sys != null ? Sys.Transition.IsEnd : true;
        }
    }
}
