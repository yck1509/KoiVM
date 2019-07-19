using System;
using KoiVM.Runtime.Dynamic;
using KoiVM.Runtime.Execution;

namespace KoiVM.Runtime.VCalls {
	internal class Ckoverflow : IVCall {
		public byte Code {
			get { return Constants.VCALL_CKOVERFLOW; }
		}

		public unsafe void Run(VMContext ctx, out ExecutionState state) {
			var sp = ctx.Registers[Constants.REG_SP].U4;
			var fSlot = ctx.Stack[sp--];

			if (fSlot.U4 != 0)
				throw new OverflowException();

			ctx.Stack.SetTopPosition(sp);
			ctx.Registers[Constants.REG_SP].U4 = sp;
			state = ExecutionState.Next;
		}
	}
}