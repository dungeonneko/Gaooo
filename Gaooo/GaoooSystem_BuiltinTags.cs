using System;

namespace Gaooo
{
    public partial class GaoooSystem
    {
        private GaoooTask? onBreak(GaoooTag tag)
        {
            // find while from callstack, and back to the line, then skip tab
            var whileTag = _callstack.Peek().ExitStatement();
            while (whileTag.Caller.Name != "while")
            {
                whileTag = _callstack.Peek().ExitStatement();
            }
            if (whileTag != null)
            {
                _scenario.Jump(whileTag, false);
                _scenario.SkipTab();
            }
            return null;
        }

        private GaoooTask? onBacklay(GaoooTag tag)
        {
            var copy = tag.GetAttrValue("copy", false);
            if (copy)
            {
                LayerFront.CopyTo(LayerBack);
            }
            _inBackLayer = true;
            return null;
        }

        private GaoooTask onCall(GaoooTag tag)
        {
            var target = GaoooTarget.FromTargetTag(tag);
            Console.WriteLine(">> " + target.Label);
            return Call(tag, target);
        }

        private GaoooTask? onCallMacro(GaoooTag tag)
        {
            var key = tag.GetAttrValue<string>("__call_macro_target__");
            if (!MacroTargets.ContainsKey(key))
            {
                Console.Write($"Error: Macro not found: {key}");
                return null;
            }

            var file = new GaoooFilePath(this, MacroTargets[key]);
            var target = new GaoooTarget(file, "*" + key);
            return Call(tag, target);
        }

        private GaoooTask onElif(GaoooTag tag)
        {
            var val = tag.GetAttrValue("exp", false);
            if (true == val)
            {
                return onEnterStatement(tag, CurrentLine);
            }
            else
            {
                return new GaoooTaskExecuteOnce(tag, _scenario.NextIf);
            }
        }

        private GaoooTask onElse(GaoooTag tag)
        {
            return onEnterStatement(tag, CurrentLine);
        }

        private GaoooTask? onEmb(GaoooTag tag)
        {
            tag["text"] = new GaoooValueString(tag.GetAttrValue<string>("exp"));
            return ProcCommand["ch"].Invoke(tag);
        }

        private GaoooTask? onEndTrack(GaoooTag tag)
        {
            _currentTrack = null;
            return null;
        }

        private GaoooTask? onEval(GaoooTag tag)
        {
            tag.Eval("exp");
            return null;
        }

        private GaoooTask onIf(GaoooTag tag)
        {
            var val = tag.GetAttrValue("exp", false);
            if (val)
            {
                return onEnterStatement(tag, CurrentLine);
            }
            else
            {
                return new GaoooTaskExecuteOnce(tag, _scenario.NextIf);
            }
        }

        private GaoooTask? onJump(GaoooTag tag)
        {
            var target = GaoooTarget.FromTargetTag(tag);
            _scenario.Jump(target, true);
            return null;
        }

        private GaoooTask? onKey(GaoooTag tag)
        {
            if (_currentTrack != null)
            {
                _currentTrack.KeyFrames.Add(tag);
            }
            return null;
        }

        private GaoooTask onLabel(GaoooTag tag)
        {
            return new GaoooTaskExecuteOnce(tag, _scenario.SkipTab);
        }

        private GaoooTask onPopTab(GaoooTag tag)
        {
            return new GaoooTaskExecuteOnce(tag, () =>
            {
                if (_callstack.Count > 0)
                {
                    if (_callstack.Peek().InStatement())
                    {
                        var ret = _callstack.Peek().ExitStatement();
                        switch (ret.Caller.Name)
                        {
                            case "if":
                            case "elif":
                            case "else":
                                _scenario.EndIf(ret.Caller);
                                break;
                            case "while":
                                _scenario.Jump(ret, true);
                                break;
                        }
                    }
                    else
                    {
                        var ret = _callstack.Pop();
                        if (_callstack.Count == 0)
                        {
                            Error("Script is finished");
                            return;
                        }
                        _scenario.Jump(new GaoooTarget(ret.Caller), false);
                    }
                }
                else
                {
                    Error("Pop tab on empty callstack");
                }
            });
        }

        private GaoooTask? onTrack(GaoooTag tag)
        {
            var id = tag.Id;
            var obj = LayerCurrent.RootObject.FindObject(id);
            if (obj != null)
            {
                _currentTrack = obj.AddTrack(tag);
            }
            return null;
        }

        private GaoooTask? onTrans(GaoooTag tag)
        {
            (LayerFront, LayerBack) = (LayerBack, LayerFront);
            _inBackLayer = false;
            Transition.Start(tag);
            return null;
        }

        private GaoooTask? onWhile(GaoooTag tag)
        {
            if (tag.GetAttrValue("exp", false))
            {
                return onEnterStatement(tag, CurrentLine);
            }
            else
            {
                _scenario.SkipTab();
                return null;
            }
        }
    }
}
