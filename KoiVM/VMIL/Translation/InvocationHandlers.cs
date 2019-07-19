using System;
using KoiVM.AST.IL;
using KoiVM.AST.IR;
using KoiVM.VMIR;

namespace KoiVM.VMIL.Translation {
	public class CallHandler : ITranslationHandler {
		public IROpCode IRCode {
			get { return IROpCode.CALL; }
		}

		public void Translate(IRInstruction instr, ILTranslator tr) {
			tr.PushOperand(instr.Operand1);
			tr.Instructions.Add(new ILInstruction(ILOpCode.CALL) { Annotation = instr.Annotation });
		}
	}

	public class RetHandler : ITranslationHandler {
		public IROpCode IRCode {
			get { return IROpCode.RET; }
		}

		public void Translate(IRInstruction instr, ILTranslator tr) {
			tr.Instructions.Add(new ILInstruction(ILOpCode.RET));
		}
	}
}