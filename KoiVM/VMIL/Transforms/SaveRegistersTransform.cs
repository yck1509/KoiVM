using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using KoiVM.AST;
using KoiVM.AST.IL;
using KoiVM.AST.IR;
using KoiVM.VM;

namespace KoiVM.VMIL.Transforms {
	public class SaveRegistersTransform : IPostTransform {
		HashSet<VMRegisters> saveRegs;

		public void Initialize(ILPostTransformer tr) {
			saveRegs = tr.Runtime.Descriptor.Data.LookupInfo(tr.Method).UsedRegister;
		}

		public void Transform(ILPostTransformer tr) {
			tr.Instructions.VisitInstrs(VisitInstr, tr);
		}

		void VisitInstr(ILInstrList instrs, ILInstruction instr, ref int index, ILPostTransformer tr) {
			if (instr.OpCode != ILOpCode.__BEGINCALL && instr.OpCode != ILOpCode.__ENDCALL)
				return;

			var callInfo = (InstrCallInfo)instr.Annotation;
			if (callInfo.IsECall) {
				instrs.RemoveAt(index);
				index--;
				return;
			}

			var saving = new HashSet<VMRegisters>(saveRegs);
			var retVar = (IRVariable)callInfo.ReturnValue;
			// R0 = return register, need to save if retVar register is not R0
			Debug.Assert(!(retVar == null ^ (callInfo.ReturnRegister == null ^ callInfo.ReturnSlot == null)));
			if (retVar != null) {
				if (callInfo.ReturnSlot == null) {
					var retReg = callInfo.ReturnRegister.Register;
					saving.Remove(retReg);
					if (retReg != VMRegisters.R0)
						saving.Add(VMRegisters.R0);
				}
				else
					saving.Add(VMRegisters.R0);
			}
			else
				saving.Add(VMRegisters.R0);

			if (instr.OpCode == ILOpCode.__BEGINCALL) {
				instrs.Replace(index, saving
					.Select(reg => new ILInstruction(ILOpCode.PUSHR_OBJECT, ILRegister.LookupRegister(reg), instr)));
			}
			else {
				instrs.Replace(index, saving
					.Select(reg => new ILInstruction(ILOpCode.POP, ILRegister.LookupRegister(reg), instr))
					.Reverse());
			}
			index--;
		}
	}
}