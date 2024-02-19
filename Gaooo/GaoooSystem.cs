using GaoooDebugger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Gaooo
{
    public partial class GaoooSystem
    {
        public Func<string, string> CallbackOnGetAbsPath = (rel) => throw new InvalidOperationException("Callback functions must be set before calling this method.");
        public Func<string, string> CallbackOnGetRelPath = (abs) => throw new InvalidOperationException("Callback functions must be set before calling this method.");

        public Dictionary<string, Func<GaoooTag, GaoooTask?>> ProcCommand;
        public Dictionary<string, Dictionary<string, Type>> PropSchema;
        public Dictionary<Type, Func<string, object>> PropConverter;
        public HashSet<Type> NumericTypes;
        public Dictionary<GaoooFilePath, HashSet<int>>? Breakpoints = null;
        public bool IsPaused { get; set; } = false; // Need first jump
        public bool IsEnd { get { return _scenario.IsEnd; } }

        public GaoooLayer LayerFront { get; private set; }
        public GaoooLayer LayerBack { get; private set; }
        public GaoooLayer LayerCurrent { get { return _inBackLayer ? LayerBack : LayerFront; } }
        public GaoooTransition Transition { get; private set; } = new();
        public GaoooTarget CurrentLine { get { return new GaoooTarget(_scenario.FilePath, _scenario.LineNumber); } }
        public GaoooTag CurrentTag { get { return Task != null ? Task.Tag : GaoooTag.Empty; } }
        public GaoooTask? Task { get; private set; } = null;

        internal Random Random { get; init; }
        internal GaoooDb Db { get; init; }
        internal Dictionary<string, string> MacroTargets { get; init; }

        bool _inBackLayer = false;
        GaoooScenario _scenario;
        Stack<GaoooStackFrame> _callstack = new ();
        Dictionary<string, GaoooValue> _variables = new();
        GaoooLayerTrack? _currentTrack = null;
        Grpc.Core.Server? _server = null;
        GaoooDebugClient? _client = null;
        readonly object _lock = new object();

        public GaoooSystem(bool debug)
        {
            _scenario = new GaoooScenario(this);
            Random = new Random();
            Db = new GaoooDb(this);
            MacroTargets = new Dictionary<string, string>();

            ProcCommand = new() {
                { "add", tag => { LayerCurrent.Add(tag); return null; } },
                { "backlay", onBacklay },
                { "branch", _ => null },
                { "break", onBreak },
                { "call", onCall },
                { "callmacro", onCallMacro },
                { "ch", _ => null },
                { "clr", tag => { LayerCurrent.Clear(); return null; } },
                { "cm", _ => null },
                { "elif", onElif },
                { "else", onElse },
                { "emb", onEmb },
                { "endbranch", _ => null },
                { "endlink", _ => null },
                { "endtrack", onEndTrack },
                { "eval", onEval },
                { "if", onIf },
                { "jump", onJump },
                { "key", onKey },
                { "l", _ => null },
                { "label", onLabel },
                { "link",  _ => null },
                { "p", _ => null },
                { "r", _ => null },
                { "poptab", onPopTab },
                { "pushtab", _ => null },
                { "playbgm", _ => null },
                { "stopbgm", _ => null },
                { "playse", _ => null },
                { "rem", tag => { LayerCurrent.Remove(tag); return null; } },
                { "track", onTrack },
                { "trans", onTrans },
                { "wait", tag => new GaoooTaskWait(tag) },
                { "while", onWhile },
                { "ws", _ => null },
                { "wt", tag => new GaoooTaskWt(tag) },
            };
            PropSchema = new () {
                { "sprite", new () {
                    { "pos_x", typeof(float) },
                    { "pos_y", typeof(float) },
                    { "pos_z", typeof(float) },
                    { "rot_z", typeof(float) },
                    { "scale_x", typeof(float) },
                    { "scale_y", typeof(float) },
                } }
            };
            PropConverter = new () {
                { typeof(bool), (x) => bool.Parse(x) },
                { typeof(double), (x) => double.Parse(x) },
                { typeof(float), (x) => float.Parse(x) },
                { typeof(int), (x) => int.Parse(x) },
                { typeof(string), (x) => x.Trim('\"') },
            };
            NumericTypes = new () {
                typeof(bool),
                typeof(int),
                typeof(float),
                typeof(double),
                typeof(decimal)
            };
            LayerFront = new(this);
            LayerBack = new(this);

            if (debug)
            {
                Breakpoints = new();
                _client = new GaoooDebugClient();
                _server = new Grpc.Core.Server
                {
                    Services = { GaoooDebugger.Runtime.BindService(new GaoooDebugService(this)) },
                    Ports = { new Grpc.Core.ServerPort("127.0.0.1", 58000, Grpc.Core.ServerCredentials.Insecure) }
                };
                _server.Start();                
            }
        }

        public void Jump(string path, int line)
        {
            lock (_lock)
            {
                var target = new GaoooTarget(new GaoooFilePath(this, path), line);
                _callstack.Clear();
                _callstack.Push(new GaoooStackFrame(this, GaoooTag.Empty, target));
                _scenario.Jump(target, true);
                Task = null;
                IsPaused = false;
            }
        }

        public GaoooTaskExecuteOnce Call(GaoooTag from, GaoooTarget target)
        {
            return new GaoooTaskExecuteOnce(from, () => {
                expandArguments(from);
                _callstack.Push(new GaoooStackFrame(this, from, target));
                _scenario.Jump(target, false);
            });
        }

        /*
        public string Serialize()
        {
            lock (_lock)
            {
                var data = new GaoooPlayData
                {
                    CurrentPos = new GaoooTarget(_scenario.FilePath, _scenario.Label, _scenario.LineNumber),
                    CallStack = _callstack,
                    Variables = _variables
                };
                return JsonSerializer.Serialize(data);
            }
        }

        public void Deserialize(string json)
        {
            object? ConvertValue(object value)
            {
                if (value is JsonElement jsonElement)
                {
                    switch (jsonElement.ValueKind)
                    {
                        case JsonValueKind.String:
                            return jsonElement.GetString();
                        case JsonValueKind.Number:
                            if (jsonElement.TryGetInt32(out int intValue))
                            {
                                return intValue;
                            }
                            else if (jsonElement.TryGetDouble(out double doubleValue))
                            {
                                return doubleValue;
                            }
                            break;
                        case JsonValueKind.True:
                            return true;
                        case JsonValueKind.False:
                            return false;
                        case JsonValueKind.Null:
                            return null;
                        default:
                            break;
                    }
                }
                return value;
            }

            var data = JsonSerializer.Deserialize<GaoooPlayData>(json);
            lock (_lock)
            {
                _scenario.Jump(data.CurrentPos, true);
                _callstack = data.CallStack;
                _variables = data.Variables.ToDictionary(x => x.Key, x => ConvertValue(x.Value));
            }
        }
        */

        public GaoooValue GetArgument(string key)
        {
            lock (_lock)
            {
                var top = _callstack.Count > 0 ? _callstack.Peek() : null;
                return top != null ? top.GetArgument(key) : new GaoooValueNull();
            }
        }

        internal GaoooValueList? GetVariableAsList(string key)
        {
            var var = GetVariable(key) as GaoooValueList;
            if (var == null)
            {
                Error(key + " is not a list");
                return null;
            }
            return var;
        }

        public GaoooValue GetVariable(string key)
        {
            lock (_lock)
            {
                if (!key.Contains("."))
                {
                    var top = _callstack.Count > 0 ? _callstack.Peek() : null;
                    if (top == null)
                    {
                        Error("Local variable not found: " + key);
                        return new GaoooValueNull();
                    }
                    return top.GetLocalVariable(key);
                }
                else
                {
                    if (!_variables.ContainsKey(key))
                    {
                        Error("Variable not found: " + key);
                        return new GaoooValueNull();
                    }
                    return _variables[key];
                }
            }
        }

        public void SetVariable(string key, GaoooValue value)
        {
            lock (_lock)
            {
                if (string.IsNullOrEmpty(key))
                {
                    Error("invalid variable name");
                    return;
                }

                if (key.StartsWith("$"))
                {
                    Error("invalid variable name");
                    return;
                }

                if (!key.Contains("."))
                {
                    _callstack.Peek().SetLocalVariable(key, value.Clone());
                }
                else
                {
                    _variables[key] = value.Clone();
                }
            }
        }

        public Type GetPropType(string objType, string attr)
        {
            lock (_lock)
            {
                if (PropSchema.ContainsKey(objType))
                {
                    var schema = PropSchema[objType];
                    if (schema.ContainsKey(attr))
                    {
                        return schema[attr];
                    }
                }
                return typeof(string);
            }
        }

        public T GetConvertedValue<T>(string raw, Type ty)
        {
            lock (_lock)
            {
                if (PropConverter.ContainsKey(ty))
                {
                    var conv = PropConverter[ty];
                    var obj = conv(raw);
                    return (T)Convert.ChangeType(obj, ty);
                }

                return (T)Convert.ChangeType(raw, ty);
            }
        }

        public T GetLerpValue<T>(object lhs, object rhs, Type ty, double t)
        {
            lock (_lock)
            {
                if (ty.IsEnum || NumericTypes.Contains(ty))
                {
                    var x = Convert.ToDouble(lhs);
                    var y = Convert.ToDouble(rhs);
                    var v = x + (y - x) * t;
                    return (T)Convert.ChangeType(v, ty);
                }

                var nonnumeric = Convert.ChangeType(lhs, ty);
                if (nonnumeric == null)
                {
                    throw new Exception("Failed to get lerp value.");
                }
                return (T)nonnumeric;
            }
        }

        public void AddMacro(string script)
        {
            lock (_lock)
            {
                var stream = new GaoooScenarioStream(this, new GaoooFilePath(this, script));
                while (true)
                {
                    var tag = stream.Pop();
                    if (tag == null)
                    {
                        break;
                    }

                    if (tag.Tab == 0 && tag.Name == "label")
                    {
                        var key = tag.Id.ToString();
                        if (MacroTargets.ContainsKey(key))
                        {
                            Console.Write("Warning: macro label is overwritten: " + key + "(path=" + script + ")");
                        }
                        MacroTargets[key] = script;
                    }
                }
            }
        }

        public void Update(double dt)
        {
            lock (_lock)
            {
                if (IsPaused)
                {
                    return;
                }

                while (true) {
                    var taskIsExecuted = false;
                    while (Task != null)
                    {
                        Task.Update(dt);
                        if (Task.IsEnd())
                        {
                            Task = Task.NextTask();
                        }
                        else
                        {
                            taskIsExecuted = true;
                            break;
                        }
                    }

                    if (taskIsExecuted)
                    {
                        break;
                    }

                    if (Task == null && _scenario != null)
                    {
                        while (Task == null && !IsPaused)
                        {
                            if (_scenario.IsEnd)
                            {
                                if (_callstack.Count > 0)
                                {
                                    onProcessCommand(new GaoooTag(this, "poptab", 0, _scenario.FilePath, _scenario.LineNumber));
                                }
                                else
                                {
                                    break;
                                }
                            }
                            else
                            {
                                var tag = _scenario.Update();
                                if (tag != null)
                                {
                                    onProcessCommand(tag);
                                }
                                onLineChanged();
                            }
                        }
                        onScenarioUpdated();
                    }

                    // Task is executed
                    if (Task != null && false == (Task is GaoooTaskExecuteOnce))
                    {
                        break;
                    }
                    // Finish if no task and no scenario
                    if ((_scenario == null || _scenario.IsEnd) && _callstack.Count == 0)
                    {
                        break;
                    }
                    // Paused
                    if (IsPaused)
                    {
                        break;
                    }
                }

                LayerFront.Update(dt);
                LayerBack.Update(dt);

                if (!Transition.IsEnd)
                {
                    Transition.Update(dt);
                }

                if (_scenario == null || _scenario.IsEnd)
                {
                    Error("Script is finished.");
                }
            }
        }

        public void Error(string message)
        {
            Console.WriteLine($"ERROR: {message} in {_scenario.FilePath} ({_scenario.LineNumber})");
            if (_client != null)
            {
                IsPaused = true;
                _client.Update(_scenario.FilePath, _scenario.LineNumber, _variables, _callstack, IsPaused);
                _client.Error(message);
            }            
        }

        private void expandArguments(GaoooTag tag)
        {
            if (!tag.ContainsKey("storage"))
            {
                var storageFromArgument = GetArgument("storage");
                if (null != storageFromArgument.ToObject())
                {
                    tag["storage"] = storageFromArgument;
                }
                else
                {
                    tag["storage"] = new GaoooValueString(_scenario.FilePath.RelPath);
                }
            }

            foreach (var kv in tag.ToDictionary(x => x.Key, x => x.Value))
            {
                var val = tag.Eval(kv.Key);
                if (val != null)
                {
                    tag[kv.Key] = val;
                }
                else
                {
                    tag.Remove(kv.Key);
                }
            }
            if (tag.ContainsKey("*"))
            {
                var top = _callstack.Where(x => x.Caller.Name == "call").FirstOrDefault();
                if (top != null)
                {
                    foreach (var kv2 in top.Caller)
                    {
                        // Don't overwrite existing variables
                        if (!tag.ContainsKey(kv2.Key))
                        {
                            tag[kv2.Key] = kv2.Value;
                        }
                    }
                }
                tag.Remove("*");
            }
        }

        private void onProcessCommand(GaoooTag tag)
        {
            if (ProcCommand.ContainsKey(tag.Name))
            {
                Task = ProcCommand[tag.Name](tag);
            }
            else
            {
                Error("Unknown command: " + tag.Name);
            }
        }

        private GaoooTaskExecuteOnce onEnterStatement(GaoooTag from, GaoooTarget target)
        {
            return new GaoooTaskExecuteOnce(from, () => {
                _callstack.Peek().EnterStatement(from);
                _scenario.Jump(target, false);
            });
        }

        private void onLineChanged()
        {
            if (Breakpoints != null)
            {
                if (Breakpoints.ContainsKey(_scenario.FilePath))
                {
                    if (Breakpoints[_scenario.FilePath].Contains(_scenario.LineNumber))
                    {
                        IsPaused = true;
                    }
                }
            }
        }

        private void onScenarioUpdated()
        {
            if (_client != null)
            {
                Console.WriteLine(" - " + _scenario.FilePath + " : " + _scenario.LineNumber.ToString());
                _client.Update(_scenario.FilePath, _scenario.LineNumber, _variables, _callstack, IsPaused);
            }
        }
    }
}
