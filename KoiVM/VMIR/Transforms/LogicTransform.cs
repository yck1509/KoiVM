using System;
using KoiVM.AST.IR;

namespace KoiVM.VMIR.Transforms {
	public class LogicTransform : ITransform {
		public void Initialize(IRTransformer tr) {
		}

		public void Transform(IRTransformer tr) {
			tr.Instructions.VisitInstrs(VisitInstr, tr);
		}

		void VisitInstr(IRInstrList instrs, IRInstruction instr, ref int index, IRTransformer tr) {
			if (instr.OpCode == IROpCode.__NOT) {
				instrs.Replace(index, new[] {
					new IRInstruction(IROpCode.NOR, instr.Operand1, instr.Operand1, instr)
				});
			}
			else if (instr.OpCode == IROpCode.__AND) {
				var tmp = tr.Context.AllocateVRegister(instr.Operand2.Type);
				instrs.Replace(index, new[] {
					new IRInstruction(IROpCode.MOV, tmp, instr.Operand2, instr),
					new IRInstruction(IROpCode.NOR, instr.Operand1, instr.Operand1, instr),
					new IRInstruction(IROpCode.NOR, tmp, tmp, instr),
					new IRInstruction(IROpCode.NOR, instr.Operand1, tmp, instr)
				});
			}
			else if (instr.OpCode == IROpCode.__OR) {
				instrs.Replace(index, new[] {
					new IRInstruction(IROpCode.NOR, instr.Operand1, instr.Operand2, instr),
					new IRInstruction(IROpCode.NOR, instr.Operand1, instr.Operand1, instr)
				});
			}
			else if (instr.OpCode == IROpCode.__XOR) {
				var tmp1 = tr.Context.AllocateVRegister(instr.Operand2.Type);
				var tmp2 = tr.Context.AllocateVRegister(instr.Operand2.Type);
				instrs.Replace(index, new[] {
					new IRInstruction(IROpCode.MOV, tmp1, instr.Operand1, instr),
					new IRInstruction(IROpCode.NOR, tmp1, instr.Operand2, instr),
					new IRInstruction(IROpCode.MOV, tmp2, instr.Operand2, instr),
					new IRInstruction(IROpCode.NOR, instr.Operand1, instr.Operand1, instr),
					new IRInstruction(IROpCode.NOR, tmp2, tmp2, instr),
					new IRInstruction(IROpCode.NOR, instr.Operand1, tmp2, instr),
					new IRInstruction(IROpCode.NOR, instr.Operand1, tmp1, instr)
				});
			}
		}
	}
}