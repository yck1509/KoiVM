using System;
using System.Collections.Generic;
using KoiVM.AST.IL;
using KoiVM.VM;

namespace KoiVM.VMIL.Transforms {
	public class FixMethodRefTransform : IPostTransform {
		HashSet<VMRegisters> saveRegs;

		public void Initialize(ILPostTransformer tr) {
			saveRegs = tr.Runtime.Descriptor.Data.LookupInfo(tr.Method).UsedRegister;
		}

		public void Transform(ILPostTransformer tr) {
			tr.Instructions.VisitInstrs(VisitInstr, tr);
		}

		void VisitInstr(ILInstrList instrs, ILInstruction instr, ref int index, ILPostTransformer tr) {
			var rel = instr.Operand as ILRelReference;
			if (rel == null)
				return;

			var methodRef = rel.Target as ILMethodTarget;
			if (methodRef == null)
				return;

			methodRef.Resolve(tr.Runtime);
		}
	}
}