using System;
using System.Diagnostics;
using KoiVM.AST;
using KoiVM.AST.IR;

namespace KoiVM.VMIR.Transforms {
	public class ConstantTypePromotionTransform : ITransform {
		public void Initialize(IRTransformer tr) {
		}

		public void Transform(IRTransformer tr) {
			tr.Instructions.VisitInstrs(VisitInstr, tr);
		}

		void VisitInstr(IRInstrList instrs, IRInstruction instr, ref int index, IRTransformer tr) {
			switch (instr.OpCode) {
				case IROpCode.MOV:
				case IROpCode.NOR:
				case IROpCode.CMP:
				case IROpCode.ADD:
				case IROpCode.MUL:
				case IROpCode.DIV:
				case IROpCode.REM:
				case IROpCode.__OR:
				case IROpCode.__AND:
				case IROpCode.__XOR:
				case IROpCode.__GETF:
					break;
				default:
					return;
			}
			Debug.Assert(instr.Operand1 != null && instr.Operand2 != null);
			if (instr.Operand1 is IRConstant) {
				instr.Operand1 = PromoteConstant((IRConstant)instr.Operand1, instr.Operand2.Type);
			}
			if (instr.Operand2 is IRConstant) {
				instr.Operand2 = PromoteConstant((IRConstant)instr.Operand2, instr.Operand1.Type);
			}
		}

		static IIROperand PromoteConstant(IRConstant value, ASTType type) {
			switch (type) {
				case ASTType.I8:
					return PromoteConstantI8(value);
				case ASTType.R4:
					return PromoteConstantR4(value);
				case ASTType.R8:
					return PromoteConstantR8(value);
				default:
					return value;
			}
		}

		static IIROperand PromoteConstantI8(IRConstant value) {
			if (value.Type.Value == ASTType.I4) {
				value.Type = ASTType.I8;
				value.Value = (long)(int)value.Value;
			}
			else if (value.Type.Value != ASTType.I8)
				throw new InvalidProgramException();
			return value;
		}

		static IIROperand PromoteConstantR4(IRConstant value) {
			if (value.Type.Value == ASTType.I4) {
				value.Type = ASTType.R4;
				value.Value = (float)(int)value.Value;
			}
			else if (value.Type.Value != ASTType.R4)
				throw new InvalidProgramException();
			return value;
		}

		static IIROperand PromoteConstantR8(IRConstant value) {
			if (value.Type.Value == ASTType.I4) {
				value.Type = ASTType.R8;
				value.Value = (double)(int)value.Value;
			}
			else if (value.Type.Value == ASTType.I8) {
				value.Type = ASTType.R8;
				value.Value = (double)(long)value.Value;
			}
			else if (value.Type.Value == ASTType.R4) {
				value.Type = ASTType.R8;
				value.Value = (double)(float)value.Value;
			}
			else if (value.Type.Value != ASTType.R8)
				throw new InvalidProgramException();
			return value;
		}
	}
}