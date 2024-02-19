namespace Gaooo
{
    public abstract class GaoooTask
    {
        public GaoooSystem? Sys { get { return Tag.Sys; } }
        public GaoooTag Tag { get; private set; }
        public GaoooTask(GaoooTag tag)
        {
            Tag = tag;
        }
        public virtual void Update(double dt) {}
        public virtual bool IsEnd() { return true; }
        public virtual GaoooTask? NextTask() { return null; }
    }
}
