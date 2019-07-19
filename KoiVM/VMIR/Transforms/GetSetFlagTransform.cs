using System;
using KoiVM.AST.IR;

namespace KoiVM.VMIR.Transforms {
	public class GetSetFlagTransform : ITransform {
		public void Initialize(IRTransformer tr) {
		}

		public void Transform(IRTransformer tr) {
			tr.Instructions.VisitInstrs(VisitInstr, tr);
		}

		void VisitInstr(IRInstrList instrs, IRInstruction instr, ref int index, IRTransformer tr) {
			if (instr.OpCode == IROpCode.__GETF) {
				instrs.Replace(index, new[] {
					new IRInstruction(IROpCode.MOV, instr.Operand1, IRRegister.FL, instr),
					new IRInstruction(IROpCode.__AND, instr.Operand1, instr.Operand2, instr)
				});
			}
			else if (instr.OpCode == IROpCode.__SETF) {
				instrs.Replace(index, new[] {
					new IRInstruction(IROpCode.__OR, IRRegister.FL, instr.Operand1, instr)
				});
			}
		}
	}
}