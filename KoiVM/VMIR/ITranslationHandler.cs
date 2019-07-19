using System;
using dnlib.DotNet.Emit;
using KoiVM.AST.ILAST;
using KoiVM.AST.IR;

namespace KoiVM.VMIR {
	public interface ITranslationHandler {
		Code ILCode { get; }
		IIROperand Translate(ILASTExpression expr, IRTranslator tr);
	}
}