using System;
using System.Diagnostics;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using KoiVM.AST.ILAST;
using KoiVM.AST.IR;

namespace KoiVM.VMIR.Translation {
	public class IsinstHandler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Isinst; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			Debug.Assert(expr.Arguments.Length == 1);
			var value = tr.Translate(expr.Arguments[0]);

			var retVar = tr.Context.AllocateVRegister(expr.Type.Value);
			var typeId = (int)tr.VM.Data.GetId((ITypeDefOrRef)expr.Operand);
			var ecallId = tr.VM.Runtime.VMCall.CAST;
			tr.Instructions.Add(new IRInstruction(IROpCode.PUSH, value));
			tr.Instructions.Add(new IRInstruction(IROpCode.VCALL, IRConstant.FromI4(ecallId), IRConstant.FromI4(typeId)));
			tr.Instructions.Add(new IRInstruction(IROpCode.POP, retVar));

			return retVar;
		}
	}

	public class CastclassHandler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Castclass; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			Debug.Assert(expr.Arguments.Length == 1);
			var value = tr.Translate(expr.Arguments[0]);

			var retVar = tr.Context.AllocateVRegister(expr.Type.Value);
			var typeId = (int)(tr.VM.Data.GetId((ITypeDefOrRef)expr.Operand) | 0x80000000);
			var ecallId = tr.VM.Runtime.VMCall.CAST;
			tr.Instructions.Add(new IRInstruction(IROpCode.PUSH, value));
			tr.Instructions.Add(new IRInstruction(IROpCode.VCALL, IRConstant.FromI4(ecallId), IRConstant.FromI4(typeId)));
			tr.Instructions.Add(new IRInstruction(IROpCode.POP, retVar));

			return retVar;
		}
	}
}