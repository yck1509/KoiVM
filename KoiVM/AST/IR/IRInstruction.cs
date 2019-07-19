using System;
using System.Text;
using KoiVM.AST.ILAST;
using KoiVM.VMIR;

namespace KoiVM.AST.IR {
	public class IRInstruction : ASTNode {
		public IRInstruction(IROpCode opCode) {
			OpCode = opCode;
		}

		public IRInstruction(IROpCode opCode, IIROperand op1) {
			OpCode = opCode;
			Operand1 = op1;
		}

		public IRInstruction(IROpCode opCode, IIROperand op1, IIROperand op2) {
			OpCode = opCode;
			Operand1 = op1;
			Operand2 = op2;
		}

		public IRInstruction(IROpCode opCode, IIROperand op1, IIROperand op2, object annotation) {
			OpCode = opCode;
			Operand1 = op1;
			Operand2 = op2;
			Annotation = annotation;
		}

		public IRInstruction(IROpCode opCode, IIROperand op1, IIROperand op2, IRInstruction origin) {
			OpCode = opCode;
			Operand1 = op1;
			Operand2 = op2;
			Annotation = origin.Annotation;
			ILAST = origin.ILAST;
		}

		public IROpCode OpCode { get; set; }
		public IILASTStatement ILAST { get; set; }
		public IIROperand Operand1 { get; set; }
		public IIROperand Operand2 { get; set; }
		public object Annotation { get; set; }

		public override string ToString() {
			var ret = new StringBuilder();
			ret.AppendFormat("{0}", OpCode.ToString().PadLeft(16));
			if (Operand1 != null) {
				ret.AppendFormat(" {0}", Operand1);
				if (Operand2 != null)
					ret.AppendFormat(", {0}", Operand2);
			}
			if (Annotation != null)
				ret.AppendFormat("    ; {0}", Annotation);
			return ret.ToString();
		}
	}
}