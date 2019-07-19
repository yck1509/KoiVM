using System;
using KoiVM.Runtime.Execution;

namespace KoiVM.Runtime.VCalls {
	internal interface IVCall {
		byte Code { get; }
		void Run(VMContext ctx, out ExecutionState state);
	}
}