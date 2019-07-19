using System;
using KoiVM.Runtime.Execution;

namespace KoiVM.Runtime.OpCodes {
	internal interface IOpCode {
		byte Code { get; }
		void Run(VMContext ctx, out ExecutionState state);
	}
}