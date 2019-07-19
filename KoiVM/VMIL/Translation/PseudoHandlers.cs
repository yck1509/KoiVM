using System;
using dnlib.DotNet;
using KoiVM.AST;
using KoiVM.AST.IL;
using KoiVM.AST.IR;
using KoiVM.VMIR;

namespace KoiVM.VMIL.Translation {
	public class EntryHandler : ITranslationHandler {
		public IROpCode IRCode {
			get { return IROpCode.__ENTRY; }
		}

		public void Translate(IRInstruction instr, ILTranslator tr) {
			tr.Instructions.Add(new ILInstruction(ILOpCode.__ENTRY));
		}
	}

	public class ExitHandler : ITranslationHandler {
		public IROpCode IRCode {
			get { return IROpCode.__EXIT; }
		}

		public void Translate(IRInstruction instr, ILTranslator tr) {
			tr.Instructions.Add(new ILInstruction(ILOpCode.__EXIT));
		}
	}

	public class BeginCallHandler : ITranslationHandler {
		public IROpCode IRCode {
			get { return IROpCode.__BEGINCALL; }
		}

		public void Translate(IRInstruction instr, ILTranslator tr) {
			tr.Instructions.Add(new ILInstruction(ILOpCode.__BEGINCALL) { Annotation = instr.Annotation });
		}
	}

	public class EndCallHandler : ITranslationHandler {
		public IROpCode IRCode {
			get { return IROpCode.__ENDCALL; }
		}

		public void Translate(IRInstruction instr, ILTranslator tr) {
			tr.Instructions.Add(new ILInstruction(ILOpCode.__ENDCALL) { Annotation = instr.Annotation });
		}
	}

	public class EHRetHandler : ITranslationHandler {
		public IROpCode IRCode {
			get { return IROpCode.__EHRET; }
		}

		public void Translate(IRInstruction instr, ILTranslator tr) {
			if (instr.Operand1 != null) {
				tr.PushOperand(instr.Operand1);
				tr.Instructions.Add(new ILInstruction(ILOpCode.POP, ILRegister.R0));
			}
			tr.Instructions.Add(new ILInstruction(ILOpCode.RET));
		}
	}

	public class LdobjHandler : ITranslationHandler {
		public IROpCode IRCode {
			get { return IROpCode.__LDOBJ; }
		}

		public void Translate(IRInstruction instr, ILTranslator tr) {
			tr.PushOperand(instr.Operand1);
			var rawType = ((PointerInfo)instr.Annotation).PointerType.ToTypeSig();
			tr.Instructions.Add(new ILInstruction(TranslationHelpers.GetLIND(instr.Operand2.Type, rawType)));
			tr.PopOperand(instr.Operand2);
		}
	}

	public class StobjHandler : ITranslationHandler {
		public IROpCode IRCode {
			get { return IROpCode.__STOBJ; }
		}

		public void Translate(IRInstruction instr, ILTranslator tr) {
			tr.PushOperand(instr.Operand2);
			tr.PushOperand(instr.Operand1);
			var rawType = ((PointerInfo)instr.Annotation).PointerType.ToTypeSig();
			tr.Instructions.Add(new ILInstruction(TranslationHelpers.GetSIND(instr.Operand2.Type, rawType)));
		}
	}
}