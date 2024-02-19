using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GaoooDebugger;

namespace Gaooo
{
    internal class GaoooDebugService : Runtime.RuntimeBase
    {
        private GaoooSystem _sys;

        internal GaoooDebugService(GaoooSystem sys)
        {
            _sys = sys;
        }

        public override Task<EchoResponse> Echo(EchoRequest request, Grpc.Core.ServerCallContext context)
        {
            var res = new EchoResponse();
            res.Text = request.Text;
            return Task.FromResult(res);
        }

        public override Task<RunResponse> Run(RunRequest request, Grpc.Core.ServerCallContext context)
        {
            _sys.Jump(request.Filepath, request.Line + 1);
            return Task.FromResult(new RunResponse());
        }

        public override Task<BreakpointsResponse> Breakpoints(BreakpointsRequest request, Grpc.Core.ServerCallContext context)
        {
            _sys.Breakpoints = request.BreakpointsPerFile.ToDictionary(
                x => new GaoooFilePath(_sys, x.Key),
                x => new HashSet<int>(x.Value.Breakpoints.Select(x => x.Line).ToList()),
                comparer: new GaoooFilePathComparer());
            return Task.FromResult(new BreakpointsResponse());
        }
    }
}
