using System;
using KoiVM.Runtime.Dynamic;
using KoiVM.Runtime.Execution;

namespace KoiVM.Runtime.VCalls {
	internal class Exit : IVCall {
		public byte Code {
			get { return Constants.VCALL_EXIT; }
		}

		public void Run(VMContext ctx, out ExecutionState state) {
			state = ExecutionState.Exit;
		}
	}
}