namespace Gaooo
{
    public class GaoooLayer
    {
        public GaoooSystem Sys { get; private set; }
        public GaoooLayerObject RootObject { get; private set; }

        public GaoooLayer(GaoooSystem sys)
        {
            Sys = sys;
            RootObject = new GaoooLayerObject(GaoooTag.Empty);
        }

        // Copy scene to another
        public void CopyTo(GaoooLayer scene)
        {
            scene.RootObject = RootObject.Copy();
        }

        // Add new object into scene
        public void Add(GaoooTag tag)
        {
            var id = tag.Id;
            if (RootObject.ContainsObject(id))
            {
                RootObject.RemoveObject(id);
            }

            RootObject.AddObject(tag);
        }

        // Remove object from scene
        public void Remove(GaoooTag tag)
        {
            var id = tag.Id;
            RootObject.RemoveObject(id);
        }

        // Clear scene
        public void Clear()
        {
            RootObject = new GaoooLayerObject(GaoooTag.Empty);
        }

        // Update scene
        public void Update(double dt)
        {
            RootObject.Update(dt);
        }
    }
}
