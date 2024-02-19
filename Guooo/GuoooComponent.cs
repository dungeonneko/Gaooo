using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;

namespace Guooo
{
    public class GuoooComponent
    {
        public GuoooSystem Sys { get; init; } = null!;
        public GuoooComponent? Parent { get; init; } = null;
        public XmlNode Layout { get; init; }
        public XmlDocument StyleSheet { get; set; }
        public List<GuoooComponent> Children { get; init; } = new List<GuoooComponent>();
        public Action<GuoooComponent>? PaintEvent { get; set; } = null; // null means default paint event

        internal GuoooState State { get; private set; }
        internal bool IsKilled { get; set; } = false;

        // Properties
        public bool Clickable { get { return GetProperty("clickable", false); } }
        public string Font { get { return GetProperty("font", string.Empty); } }
        public float FontSize { get { return GetProperty("font-size", 10.0f); } }
        public string Text { get { return GetProperty("text", string.Empty); } }
        public float TextOffsetY { get { return GetProperty("text-offset-y", 0.0f); } }
        public string WindowTexture { get { return GetProperty("window-texture", string.Empty); } }
        public string Image { get { return GetProperty("image", string.Empty); } }
        public string LocalAlignH { get { return GetProperty("align-h", "Left"); } }
        public string LocalAlignV { get { return GetProperty("align-v", "Top"); } }
        public float LocalHeight { get { return GetProperty("height", -1.0f); } }
        public float LocalMarginTop { get { return GetProperty("margin-top", 0.0f); } }
        public float LocalMarginLeft { get { return GetProperty("margin-left", 0.0f); } }
        public float LocalMarginBottom { get { return GetProperty("margin-bottom", 0.0f); } }
        public float LocalMarginRight { get { return GetProperty("margin-right", 0.0f); } }
        public float LocalOffsetX { get { return GetProperty("offset-x", 0.0f); } }
        public float LocalOffsetY { get { return GetProperty("offset-y", 0.0f); } }
        public float LocalWidth { get { return GetProperty("width", -1.0f); } }
        public float LocalMulColorR { get { return GetProperty("mul-color-r", 1.0f); } }
        public float LocalMulColorG { get { return GetProperty("mul-color-g", 1.0f); } }
        public float LocalMulColorB { get { return GetProperty("mul-color-b", 1.0f); } }
        public float LocalMulColorA { get { return GetProperty("mul-color-a", 1.0f); } }
        public string ContentLayout { get { return GetProperty("content-layout", "None"); } }
        public string ContentAlignH { get { return GetProperty("content-align-h", "Left"); } }
        public string ContentAlignV { get { return GetProperty("content-align-v", "Top"); } }
        public string HintText { get { return GetProperty("hint-text", string.Empty); } }
        public bool BlankTextHide { get { return GetProperty("blank-text-hide", false); } }

        public Dictionary<string, object?> Properties = new ();

        // Auto update properties
        public float X { get; private set; }
        public float Y { get; private set; }
        public float Width { get { var w = LocalWidth; return w < 0.0f ? ParentWidth - (LocalMarginLeft + LocalMarginRight) : w; } }
        public float Height { get { var h = LocalHeight; return h < 0.0f ? ParentHeight - (LocalMarginTop + LocalMarginBottom) : h; } }
        public float ContentPosX { get; private set; }
        public float ContentPosY { get; private set; }
        public GuoooRect Rect { get { return new(X, Y, X + Width, Y + Height); } }
        public GuoooColor MulColor { get { return new(LocalMulColorR, LocalMulColorG, LocalMulColorB, LocalMulColorA); } }

        public float ParentX { get { return Parent != null ? Parent.X : 0; } }
        public float ParentY { get { return Parent != null ? Parent.Y : 0; } }
        public float ParentWidth { get { return Parent != null ? Parent.Width : Sys.ScreenWidth; } }
        public float ParentHeight { get { return Parent != null ? Parent.Height : Sys.ScreenHeight; } }

        public Action OnLeftClicked = () => { };
        public Action OnRightClicked = () => { };
        public Action OnFocusedIn = () => { };
        public Action OnFocusedOut = () => { };

        public GuoooComponent(GuoooSystem guosys, GuoooComponent? parent, XmlNode layout, XmlDocument style)
        {
            Sys = guosys;
            Parent = parent;
            Layout = layout;
            foreach (XmlAttribute attr in Layout.Attributes)
            {
                Properties[attr.Name] = attr.Value;
            }

            StyleSheet = style;
            State = new GuoooState();
            foreach (XmlNode elem in Layout.ChildNodes)
            {
                var child = new GuoooComponent(guosys, this, elem, style);
                Children.Add(child);
            }

            applyFirstState();
        }

        public GuoooComponent(GuoooSystem guosys, GuoooComponent parent, string name, XmlDocument style)
        {
            Sys = guosys;
            Parent = parent;
            Layout = parent.Layout.OwnerDocument.CreateElement(name);
            StyleSheet = style;
            State = new GuoooState();
            applyFirstState();
        }

        private void applyFirstState()
        {
            if (SetState("init"))
            {
                State.Update(0.0f);
                foreach (var p in State.Properties)
                {
                    Properties[p.Key] = p.Value;
                }
                if (!string.IsNullOrEmpty(State.NextState) && State.IsEnd)
                {
                    SetState(State.NextState);
                }
            }
            else
            {
                SetState("default");
            }
        }

        internal bool SetState(string stateName)
        {
            if (StyleSheet == null)
            {
                return false;
            }

            var style = StyleSheet.SelectSingleNode($"//style[@name=\'{Layout.Name}\']");
            if (style == null)
            {
                return false;
            }

            var state = style.SelectSingleNode($"state[@name=\'{stateName}\']");
            if (state == null)
            {
                return false;
            }

            State = new GuoooState(state);
            return true;
        }

        internal GuoooComponent? FindByTag(string name)
        {
            if (IsKilled)
            {
                return null;
            }

            if (Layout.Name == name)
            {
                return this;
            }

            foreach (var c in Children)
            {
                if (c.Layout.Name == name)
                {
                    return c;
                }
                var found = c.FindByTag(name);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        internal GuoooComponent? FindById(string id)
        {
            if (IsKilled)
            {
                return null;
            }

            if (GetProperty("id", string.Empty) == id)
            {
                return this;
            }

            foreach (var c in Children)
            {
                if (c.GetProperty("id", string.Empty) == id)
                {
                    return c;
                }
                var found = c.FindById(id);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        public GuoooComponent? CheckFocused(float x, float y)
        {
            if (IsKilled)
            {
                return null;
            }

            foreach (var c in Children)
            {
                var focused = c.CheckFocused(x, y);
                if (focused != null)
                {
                    return focused;
                }
            }

            if (GetProperty("clickable", false) && Rect.Contains(x, y))
            {
                return this;
            }

            return null;
        }

        public T GetProperty<T>(string name, T defaultValue)
        {
            if (Properties.ContainsKey(name))
            {
                var p = Properties[name];
                return p != null ? (T)p : defaultValue;
            }
            return defaultValue;
        }

        public void Kill()
        {
            IsKilled = true;
        }

        internal void FocuseOut()
        {            
            SetState("default");
            OnFocusedOut();
        }

        internal void FocuseIn()
        {
            SetState("focused");
            OnFocusedIn();
        }

        internal void Update(double dt)
        {
            updateState(dt);
            updateLayout();
            Children.RemoveAll(x => x.IsKilled);
        }

        private void updateState(double dt)
        {
            if (!string.IsNullOrEmpty(State.NextState) && State.IsEnd)
            {
                SetState(State.NextState);
            }
            State.Update(dt);
            foreach (var p in State.Properties)
            {
                Properties[p.Key] = p.Value;
            }

            foreach (var c in Children)
            {
                c.updateState(dt);
            }
        }

        private void updateLayout()
        {
            X = _calcGlobalPosX();
            Y = _calcGlobalPosY();

            _layoutChildren();

            foreach (var c in Children)
            {
                c.updateLayout();
            }
        }

        private float _calcGlobalPosX()
        {
            // Layout by parent
            if (Parent != null && Parent.ContentLayout != "None")
            {
                var OffsetX = GetProperty("offset-x", 0.0f);
                return ParentX + ContentPosX + OffsetX;
            }
            else
            {
                switch (LocalAlignH)
                {
                    case "Left":
                        return ParentX + LocalMarginLeft + LocalOffsetX;
                    case "Right":
                        return ParentX + ParentWidth - LocalMarginRight - Width + LocalOffsetX;
                    default:
                        return ParentX + ParentWidth / 2 - Width / 2 + LocalOffsetX;
                }
            }
        }

        private float _calcGlobalPosY()
        {
            // Layout by parent
            if (Parent != null && Parent.ContentLayout != "None")
            {
                return ParentY + ContentPosY + LocalOffsetY;
            }
            else
            {
                switch (LocalAlignV)
                {
                    case "Top":
                        return ParentY + LocalMarginTop + LocalOffsetY;
                    case "Bottom":
                        return ParentY + ParentHeight - LocalMarginBottom - Height + LocalOffsetY;
                    default:
                        return ParentY + ParentHeight / 2 - Height / 2 + LocalOffsetY;
                }
            }
        }

        private void _layoutChildren()
        {
            if (ContentLayout == "None")
            {
                // pass
            }
            else if (ContentLayout == "Horizontal")
            {
                switch (ContentAlignH)
                {
                    case "Left":
                        {
                            var x = 0.0f;
                            foreach (var c in Children)
                            {
                                c.ContentPosX = x;
                                x += c.Width;
                            }
                        }
                        break;
                    case "Center":
                        {
                            var x = 0.0f;
                            foreach (var c in Children)
                            {
                                c.ContentPosX = x;
                                x += c.Width;
                            }
                            var offset = (Width - x) / 2;
                            foreach (var c in Children)
                            {
                                c.ContentPosX += offset;
                            }
                        }
                        break;
                    case "Right":
                        {
                            var x = 0.0f;
                            foreach (var c in Children)
                            {
                                c.ContentPosX = x;
                                x += c.Width;
                            }
                            foreach (var c in Children)
                            {
                                c.ContentPosX += (Width - x);
                            }
                        }
                        break;
                }

                switch (ContentAlignV)
                {
                    case "Top":
                        foreach (var c in Children)
                        {
                            c.ContentPosY = 0.0f;
                        }
                        break;
                    case "Middle":
                        foreach (var c in Children)
                        {
                            c.ContentPosY = c.Height / 2;
                        }
                        break;
                    case "Bottom":
                        foreach (var c in Children)
                        {
                            c.ContentPosY = (Height - c.Height);
                        }
                        break;
                }
            }
        }
    }
}
