using System;
using System.Diagnostics;
using dnlib.DotNet.Emit;
using KoiVM.AST;
using KoiVM.AST.ILAST;
using KoiVM.AST.IR;

namespace KoiVM.VMIR.Translation {
	public class DupHandler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Dup; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			Debug.Assert(expr.Arguments.Length == 1);
			var ret = tr.Context.AllocateVRegister(expr.Type.Value);
			tr.Instructions.Add(new IRInstruction(IROpCode.MOV) {
				Operand1 = ret,
				Operand2 = tr.Translate(expr.Arguments[0])
			});
			return ret;
		}
	}

	public class NopHandler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Nop; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			tr.Instructions.Add(new IRInstruction(IROpCode.NOP));
			return null;
		}
	}

	public class BreakHandler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Break; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			var ecallId = tr.VM.Runtime.VMCall.BREAK;
			tr.Instructions.Add(new IRInstruction(IROpCode.VCALL, IRConstant.FromI4(ecallId)));
			return null;
		}
	}

	public class CkfiniteHandler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Ckfinite; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			Debug.Assert(expr.Arguments.Length == 1);
			var value = tr.Translate(expr.Arguments[0]);
			var ecallId = tr.VM.Runtime.VMCall.CKFINITE;
			if (value.Type == ASTType.R4) {
				tr.Instructions.Add(new IRInstruction(IROpCode.__SETF) {
					Operand1 = IRConstant.FromI4(1 << tr.Arch.Flags.UNSIGNED)
				});
			}
			tr.Instructions.Add(new IRInstruction(IROpCode.VCALL, IRConstant.FromI4(ecallId), value));
			return value;
		}
	}

	public class PopHandler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Pop; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			Debug.Assert(expr.Arguments.Length == 1);
			tr.Translate(expr.Arguments[0]);
			return null;
		}
	}
}