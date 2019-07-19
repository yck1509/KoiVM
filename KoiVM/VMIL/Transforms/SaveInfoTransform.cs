using System;
using KoiVM.AST.IL;
using KoiVM.VM;

namespace KoiVM.VMIL.Transforms {
	public class SaveInfoTransform : ITransform {
		VMMethodInfo methodInfo;

		public void Initialize(ILTransformer tr) {
			methodInfo = tr.VM.Data.LookupInfo(tr.Method);
			methodInfo.RootScope = tr.RootScope;
			tr.VM.Data.SetInfo(tr.Method, methodInfo);
		}

		public void Transform(ILTransformer tr) {
			tr.Instructions.VisitInstrs(VisitInstr, tr);
		}

		void VisitInstr(ILInstrList instrs, ILInstruction instr, ref int index, ILTransformer tr) {
			if (instr.Operand is ILRegister) {
				var reg = ((ILRegister)instr.Operand).Register;
				if (reg.IsGPR())
					methodInfo.UsedRegister.Add(reg);
			}
		}
	}
}