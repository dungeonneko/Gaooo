using System.Collections.Generic;
using System.Linq;
using Grpc.Core;
using GaoooDebugger;

namespace Gaooo
{
    internal class GaoooDebugClient
    {
        public Target Convert(GaoooStackFrame frame)
        {
            return new Target
            {
                Relpath = frame.Caller.FilePath.RelPath,
                Line = frame.Caller.LineNumber,
                Label = frame.Target.Label,
            };
        }

        public void Update(
            GaoooFilePath filepath, int line,
            Dictionary<string, GaoooValue> varibales,
            Stack<GaoooStackFrame> callstack,
            bool paused)
        {
            var channel = new Channel("localhost:58001", ChannelCredentials.Insecure);
            var client = new Editor.EditorClient(channel);
            var req = new UpdateRequest
            {
                Relpath = filepath.RelPath,
                Line = line,
                Paused = paused,
            };
            req.Variables.Add(varibales.ToDictionary(x => x.Key, x => x.Value.ToString()));
            if (callstack.Count > 0)
            {
                req.Locals.Add(callstack.Peek().Caller.ToDictionary(x => x.Key, x => x.Value.ToString()));
                req.Locals.Add(callstack.Peek().LocalVariables.ToDictionary(x => x.Key, x => x.Value.ToString()));                
            }
            req.Callstack.Add(new Target() { Relpath = filepath.RelPath, Line = line, Label = "" });
            req.Callstack.Add(callstack.Select(x => Convert(x)));            
            client.UpdateAsync(req);
        }

        public void Error(string message)
        {
            var channel = new Channel("localhost:58001", ChannelCredentials.Insecure);
            var client = new Editor.EditorClient(channel);
            var req = new ErrorRequest
            {
                Message = message,
            };
            client.ErrorAsync(req);
        }
    }
}
