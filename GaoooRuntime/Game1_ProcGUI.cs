using Gaooo;
using Guooo;
using Microsoft.Xna.Framework;

namespace GaoooRuntime
{
    public partial class Game1 : Game
    {
        private GaoooTask onGuiLoad(GaoooTag tag)
        {
            var path = tag.GetAttrValue<string>("path");
            var reload = tag.GetAttrValue("reload", false);
            if (reload || !_guooo.IsLoaded(path))
            {
                _guooo.Load(path, loadXml(path + ".layout.xml"), loadXml(path + ".style.xml"));
            }
            var comp = _guooo.FindByTag("Message");
            if (comp != null)
            {
                comp.Properties["text"] = string.Empty;
                comp.PaintEvent = (self) => paintMessage(self, _text);
            }
            return null;
        }

        private GaoooTask onGuiUnload(GaoooTag tag)
        {
            var path = tag.GetAttrValue<string>("path");
            _guooo.Unload(path);
            return null;
        }

        private GaoooTask onGuiAdd(GaoooTag tag)
        {
            var name = tag.GetAttrValue<string>("tag");
            var parentKey = tag.GetAttrValue<string>("parent");
            var parent = _guooo.FindByTag(parentKey);
            if (parent != null)
            {
                var comp = new GuoooComponent(_guooo, parent, name, parent.StyleSheet);
                comp.Properties["id"] = tag.GetAttrValue<string>("id");
                comp.OnFocusedIn = () =>
                {
                    var hint = _guooo.FindByTag("HintText");
                    if (hint != null)
                    {
                        hint.Properties["text"] = comp.HintText;
                    }
                };
                comp.OnFocusedOut = () =>
                {
                    var hint = _guooo.FindByTag("HintText");
                    if (hint != null)
                    {
                        hint.Properties["text"] = string.Empty;
                    }
                };
                parent.Children.Add(comp);
            }
            else
            {
                _gaooo.Error($"UI component not found (tag={parentKey})");
            }
            return null;
        }

        private GaoooTask onGuiRemove(GaoooTag tag)
        {
            var id = tag.GetAttrValue<string>("id");
            var comp = _guooo.FindById(id);
            if (comp != null)
            {
                comp.Kill();
            }
            else
            {
                _gaooo.Error($"UI component not found (id={id})");
            }
            return null;
        }

        private GaoooTask onGuiSet(GaoooTag tag)
        {
            var id = tag.GetAttrValue<string>("id");
            var comp = _guooo.FindById(id);
            if (comp != null)
            {
                var key = tag.GetAttrValue<string>("key");
                var source = tag.GetEvaluatedTag();
                var target = GaoooTarget.FromTargetTag(tag);
                switch (key)
                {
                    case "on-left-clicked":
                        comp.OnLeftClicked = () =>
                        {
                            var task = _gaooo.Task as TaskUIBranch;
                            if (task != null)
                            {
                                task.Source = source;
                                task.Target = target;
                            }
                        };
                        break;
                    case "on-right-clicked":
                        comp.OnRightClicked = () =>
                        {
                            var task = _gaooo.Task as TaskUIBranch;
                            if (task != null)
                            {
                                task.Source = source;
                                task.Target = target;
                            }
                        };
                        break;
                    default:
                        comp.Properties[key] = tag.GetAttrValue("value");
                        break;
                }
            }
            else
            {
                _gaooo.Error($"UI component not found (id={id})");
            }
            return null;
        }

        private GaoooTask onGuiBranch(GaoooTag tag)
        {
            return new TaskUIBranch(tag);
        }
    }
}