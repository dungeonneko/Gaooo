using System;
using System.Collections.Generic;
using System.Xml;

namespace Guooo
{
    public class GuoooSystem
    {
        public int ScreenWidth { get; private set; } = 800;
        public int ScreenHeight { get; private set; } = 600;
        public Dictionary<string, GuoooComponent> Components { get; private set; } = new ();
        public GuoooComponent? FocusedComponent { get; private set; } = null;

        public void Load(string path, XmlDocument layout, XmlDocument style)
        {
            try
            {
                if (layout != null)
                {
                    var layoutNode = layout.SelectSingleNode("root");
                    if (layoutNode != null)
                    {
                        Components[path] = new GuoooComponent(this, null, layoutNode, style);
                    }
                }
            }
            catch (FormatException e)
            {
                Console.WriteLine(e);
                Console.WriteLine("failed to load ui: {0}", path);
            }
        }

        public bool IsLoaded(string path)
        {
            return Components.ContainsKey(path);
        }

        public void Unload(string path)
        {
            Components.Remove(path);
        }

        public void Resize(int width, int height)
        {
            ScreenWidth = width;
            ScreenHeight = height;
        }

        public void Update(double dt, float mouseX, float mouseY, bool leftClicked, bool rightClicked)
        {
            foreach (var kv in Components)
            {
                kv.Value.Update(dt);
            }

            GuoooComponent? focused = null;
            foreach (var kv in Components)
            {
                focused = kv.Value.CheckFocused(mouseX, mouseY);
                if (focused != null)
                {
                    break;
                }
            }

            if (FocusedComponent != focused)
            {
                if (FocusedComponent != null && FocusedComponent != focused)
                {
                    FocusedComponent.FocuseOut();
                }

                FocusedComponent = focused;

                if (FocusedComponent != null)
                {
                    FocusedComponent.FocuseIn();
                }
            }

            if (FocusedComponent != null)
            {
                if (leftClicked)
                {
                    FocusedComponent.OnLeftClicked();
                }
                else if (rightClicked)
                {
                    FocusedComponent.OnRightClicked();
                }
            }
        }

        public GuoooComponent? FindByTag(string name)
        {
            foreach (var kv in Components)
            {
                var found = kv.Value.FindByTag(name);
                if (found != null)
                {
                    return found;
                }
            }
            return null;
        }

        public GuoooComponent? FindById(string id)
        {
            foreach (var kv in Components)
            {
                var found = kv.Value.FindById(id);
                if (found != null)
                {
                    return found;
                }
            }
            return null;
        }
    }
}