using System;
using System.Text;
using KoiVM.AST.IR;
using KoiVM.VMIL;

namespace KoiVM.AST.IL {
	public class ILInstruction : ASTNode, IHasOffset {
		public ILInstruction(ILOpCode opCode) {
			OpCode = opCode;
		}

		public ILInstruction(ILOpCode opCode, IILOperand operand) {
			OpCode = opCode;
			Operand = operand;
		}

		public ILInstruction(ILOpCode opCode, IILOperand operand, object annotation) {
			OpCode = opCode;
			Operand = operand;
			Annotation = annotation;
		}

		public ILInstruction(ILOpCode opCode, IILOperand operand, ILInstruction origin) {
			OpCode = opCode;
			Operand = operand;
			Annotation = origin.Annotation;
			IR = origin.IR;
		}

		public uint Offset { get; set; }
		public IRInstruction IR { get; set; }
		public ILOpCode OpCode { get; set; }
		public IILOperand Operand { get; set; }
		public object Annotation { get; set; }

		public override string ToString() {
			var ret = new StringBuilder();
			ret.AppendFormat("{0}", OpCode.ToString().PadLeft(16));
			if (Operand != null) {
				ret.AppendFormat(" {0}", Operand);
			}
			if (Annotation != null)
				ret.AppendFormat("    ; {0}", Annotation);
			return ret.ToString();
		}
	}
}