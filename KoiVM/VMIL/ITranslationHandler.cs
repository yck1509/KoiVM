using System;
using KoiVM.AST.IR;
using KoiVM.VMIR;

namespace KoiVM.VMIL {
	public interface ITranslationHandler {
		IROpCode IRCode { get; }
		void Translate(IRInstruction instr, ILTranslator tr);
	}
}