using System;
using System.Diagnostics;
using KoiVM.AST;
using KoiVM.AST.IR;

namespace KoiVM.VMIR {
	public static class TranslationHelpers {
		public static void EmitCompareEq(IRTranslator tr, ASTType type, IIROperand a, IIROperand b) {
			if (type == ASTType.O || type == ASTType.ByRef ||
			    type == ASTType.R4 || type == ASTType.R8) {
				tr.Instructions.Add(new IRInstruction(IROpCode.CMP, a, b));
			}
			else {
				// I4/I8/Ptr
				Debug.Assert(type == ASTType.I4 || type == ASTType.I8 || type == ASTType.Ptr);
				tr.Instructions.Add(new IRInstruction(IROpCode.CMP, a, b));
			}
		}
	}
}