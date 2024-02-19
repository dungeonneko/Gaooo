using System;
using System.Collections.Generic;
using System.Linq;

namespace Gaooo
{
    internal class GaoooScenarioStream : IDisposable
    {
        private class CommandAndArgs
        {
            public string Name = string.Empty;
            public GaoooParams Args = new();
        }

        public GaoooSystem Sys { get; private set; }
        public GaoooFilePath FilePath { get { return _stream.Path; } }
        public bool EndOfStream { get { return _stream.EndOfStream; } }
        public int LineNumber { get { return _stream.Line; } }

        private GaoooPeekableStream _stream;
        private int _tabLevel;
        private Queue<GaoooTag> _queue;

        public GaoooScenarioStream(GaoooSystem sys)
        {
            Sys = sys;
            _stream = new GaoooPeekableStream();
            _tabLevel = 0;
            _queue = new Queue<GaoooTag>();
        }

        public GaoooScenarioStream(GaoooSystem sys, GaoooFilePath filepath)
        {
            Sys = sys;
            _stream = new GaoooPeekableStream(filepath);
            _tabLevel = 0;
            _queue = new Queue<GaoooTag>();
        }

        public void Dispose()
        {
            if (_stream != null)
            {
                _stream.Dispose();
            }
        }

        private void getLine(string line, out int tabLevel, out string lineText)
        {
            tabLevel = line.TakeWhile(c => c == '\t').Count();
            lineText = line.TrimStart('\t');
        }

        public void SkipTo(int lineNumber, bool procLine)
        {
            _queue.Clear();

            if (procLine)
            {
                while (_stream.Line < lineNumber - 1)
                {
                    getLine(_stream.ReadLine(), out _tabLevel, out var line);
                }

                getLine(_stream.PeekLine(), out _tabLevel, out var line2);
            }
            else
            {
                while (_stream.Line < lineNumber)
                {
                    getLine(_stream.ReadLine(), out _tabLevel, out var line);
                }
            }
        }

        public void SkipTo(string label)
        {
            _queue.Clear();

            while (!_stream.EndOfStream)
            {
                getLine(_stream.ReadLine(), out _tabLevel, out var line);
                if (line.Split('|')[0] == label)
                {
                    break;
                }
            }

            if (_stream.EndOfStream)
            {
                Sys.Error("Label not found: " + label);
            }
        }

        public void SkipToNextTag(HashSet<string> tag)
        {
            var tab = _tabLevel;
            while (true) {
                var pop = Pop();
                if (pop == null)
                {
                    break;
                }
                else if (tab < pop.Tab)
                {
                    continue;
                }
                else if (tab > pop.Tab)
                {
                    throw new Exception("Syntax error: tab level is not correct.");
                }
                else if (tag.Contains(pop.Name))
                {
                    break;
                }
            }
        }

        public void SkipTab()
        {
            while (_queue.Count > 0)
            {
                if (_queue.Peek().Tab < _tabLevel)
                {
                    _queue.Dequeue();
                }
                else
                {
                    break;
                }
            }

            if (_queue.Count == 0)
            {
                while (!_stream.EndOfStream)
                {
                    getLine(_stream.PeekLine(), out var tabLevel, out var lineText);
                    if (lineText.StartsWith(";") || string.IsNullOrEmpty(lineText))
                    {
                    }
                    else if (tabLevel <= _tabLevel)
                    {
                        break;
                    }

                    _stream.ReadLine();
                }
            }
        }

        public (int, Queue<GaoooTag>) Peek()
        {
            if (_queue.Count > 0)
            {
                return (_tabLevel, new Queue<GaoooTag>(_queue));
            }

            var queue = new Queue<GaoooTag>();
            getLine(_stream.PeekLine(), out var tabLevel, out var lineText);
            var lineNumber = _stream.Line + 1;

            // comment
            if (lineText.StartsWith(";"))
            {
                return (_tabLevel, queue);
            }

            // tab level changed
            if (tabLevel > _tabLevel)
            {
                var n = tabLevel - _tabLevel;
                for (int i = 0; i < n; ++i)
                {
                    queue.Enqueue(new GaoooTag(Sys, "pushtab", _tabLevel + i, _stream.Path, lineNumber));
                }
            }
            else if (tabLevel < _tabLevel)
            {
                var n = _tabLevel - tabLevel;
                for (int i = 0; i < n; ++i)
                {
                    queue.Enqueue(new GaoooTag(Sys, "poptab", _tabLevel - i, _stream.Path, lineNumber));
                }
            }

            // label
            if (lineText.StartsWith("*"))
            {
                var chunk = lineText.Substring(1).Split('|');
                var id = new GaoooValueString(chunk[0]);
                var text = new GaoooValueString(chunk.Length <= 1 ? string.Empty : chunk[1].Trim());
                var tag = new GaoooTag(
                    Sys,
                    "label",
                    tabLevel,
                    _stream.Path,
                    lineNumber,
                    new GaoooParams {
                        { "id", id },
                        { "text", text }
                    });
                queue.Enqueue(tag);
            }
            // command
            else if (lineText.StartsWith("@"))
            {
                var com = parseCommand(lineText.Substring(1));
                var tag = new GaoooTag(
                    Sys,
                    com.Name,
                    tabLevel,
                    _stream.Path,
                    lineNumber,
                    com.Args);
                queue.Enqueue(tag);
            }
            // call macro
            else if (lineText.StartsWith("#"))
            {
                var com = parseCommand(lineText.Substring(1));
                com.Args["__call_macro_target__"] = new GaoooValueString(com.Name);
                var tag = new GaoooTag(
                    Sys,
                    "callmacro",
                    tabLevel,
                    _stream.Path,
                    lineNumber,
                    com.Args);
                queue.Enqueue(tag);
            }
            // expression
            else if (lineText.StartsWith("|"))
            {
                var exp = lineText.Substring(1).Trim();
                var tag = new GaoooTag(Sys,
                    "eval",
                    tabLevel,
                    _stream.Path,
                    lineNumber,
                    new GaoooParams(){
                        { "exp", new GaoooValueExpression(exp) }
                    });
                queue.Enqueue(tag);
            }
            // text or inline command(s)
            else
            {
                while (!string.IsNullOrEmpty(lineText))
                {
                    var i = lineText.IndexOfAny(new char[] { '[', '\\' });

                    // text
                    // 特殊文字が出現するまで平文として処理
                    if (i > 0 || i < 0)
                    {
                        var tag = new GaoooTag(
                            Sys,
                            "ch",
                            tabLevel,
                            _stream.Path,
                            lineNumber,
                            new GaoooParams {
                                { "text", new GaoooValueString(i > 0 ? lineText.Substring(0, i) : lineText) }
                            });
                        queue.Enqueue(tag);
                        lineText = i > 0 ? lineText.Substring(i) : string.Empty;
                        continue;
                    }

                    // escaped characters
                    if (lineText.Length >= 1 && (lineText.StartsWith("[[") || lineText[0] == '\\'))
                    {
                        var tag = new GaoooTag(
                            Sys,
                            "ch",
                            tabLevel,
                            _stream.Path,
                            lineNumber,
                            new GaoooParams {
                                { "text", new GaoooValueString(lineText[1].ToString()) }
                            });
                        queue.Enqueue(tag);
                        lineText = lineText.Substring(2);
                        continue;
                    }

                    // inline command
                    {
                        var pair = parseInlineCommand(lineText);
                        var com = pair.Item1;
                        lineText = pair.Item2;
                        var tag = new GaoooTag(
                            Sys,
                            com.Name,
                            tabLevel,
                            _stream.Path,
                            lineNumber,
                            com.Args);
                        queue.Enqueue(tag);
                    }
                }
            }

            return (tabLevel, queue);
        }

        public GaoooTag? Pop()
        {
            if (_queue.Count > 0)
            {
                return _queue.Dequeue();
            }

            while (!_stream.EndOfStream && _queue.Count == 0)
            {
                (_tabLevel, _queue) = Peek();
                _stream.ReadLine();
            }

            if (_stream.EndOfStream)
            {
                return null;
            }

            return _queue.Dequeue();
        }

        private (string, string) parseUntil(string exp, char quotes)
        {
            var start = 0;

            while (true) {
                var i = exp.IndexOfAny(new char[] { quotes, '\\' }, start);
                if (i < 0)
                {
                    throw new Exception("Syntax Error");
                }
                var c = exp[i];
                if (c == '\\')
                {
                    start = i + 2;
                    continue;
                }
                //  c == quotes
                else
                {                    
                    return (exp.Substring(0, i), exp.Substring(i + 1));
                }
            }
        }

        private (string, string) parseArgumentValue(string exp)
        {
            var i = exp.IndexOfAny(new char[] { ' ', '\"', '\'', '{' });
            if (i < 0)
            {
                return (exp, string.Empty);
            }

            var head = exp.Substring(0, i);
            var tail = exp.Substring(i + 1);
            var c = exp[i];
            switch (c)
            {
                case '\'':
                case '\"':
                    {
                        var chunk = parseUntil(tail, c);
                        head = c.ToString() + chunk.Item1 + c.ToString();
                        tail = chunk.Item2;
                    }
                    break;
                case '{':
                    {
                        var chunk = parseUntil(tail, '}');
                        head = "{" + chunk.Item1 + "}";
                        tail = chunk.Item2;
                    }
                    break;
            }
            return (head, tail);
        }

        private GaoooParams parseArguments(string exp)
        {
            var ret = new GaoooParams();
            while (!string.IsNullOrWhiteSpace(exp))
            {
                var chunk = exp.Split('=', 2);
                var key = chunk[0].Trim();
                var rest = chunk.Length > 1 ? chunk[1].TrimStart() : string.Empty;
                var pair = parseArgumentValue(rest);
                ret[key] = pair.Item1.ObjectToGaoooValue();
                exp = pair.Item2;
            }
            return ret;
        }

        private CommandAndArgs parseCommand(string exp)
        {
            var ret = new CommandAndArgs();
            var chunk = exp.Split(' ', 2);
            ret.Name = chunk[0];
            ret.Args = parseArguments(chunk.Length > 1 ? chunk[1].Trim() : string.Empty);
            return ret;
        }

        private (CommandAndArgs, string) parseInlineCommand(string exp)
        {
            if (exp[0] != '[')
            {
                throw new ArgumentException("exp must begin with '['");
            }

            var c = new System.Text.StringBuilder();
            var s = exp.Substring(1);
            var i = s.IndexOf(']');
            while (i < 0)
            {
                c.Append(s);
                s = _stream.ReadLine().TrimStart('\t');
                i = s.IndexOf(']');
            }
            c.Append(s.Substring(0, i));
            return (parseCommand(c.ToString()), s.Substring(i + 1));
        }
    }
}
