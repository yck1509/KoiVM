using System;
using System.Diagnostics;
using dnlib.DotNet.Emit;
using KoiVM.AST;
using KoiVM.AST.ILAST;
using KoiVM.AST.IR;
using KoiVM.CFG;

namespace KoiVM.VMIR.Translation {
	public class BrHandler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Br; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			tr.Instructions.Add(new IRInstruction(IROpCode.JMP) {
				Operand1 = new IRBlockTarget((IBasicBlock)expr.Operand)
			});
			return null;
		}
	}

	public class BrtrueHandler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Brtrue; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			Debug.Assert(expr.Arguments.Length == 1);

			var val = tr.Translate(expr.Arguments[0]);
			TranslationHelpers.EmitCompareEq(tr, expr.Arguments[0].Type.Value, val, IRConstant.FromI4(0));
			var tmp = tr.Context.AllocateVRegister(ASTType.I4);
			tr.Instructions.Add(new IRInstruction(IROpCode.__GETF) {
				Operand1 = tmp,
				Operand2 = IRConstant.FromI4(1 << tr.Arch.Flags.ZERO)
			});
			tr.Instructions.Add(new IRInstruction(IROpCode.JZ) {
				Operand1 = new IRBlockTarget((IBasicBlock)expr.Operand),
				Operand2 = tmp
			});
			return null;
		}
	}

	public class BrfalseHandler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Brfalse; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			Debug.Assert(expr.Arguments.Length == 1);

			var val = tr.Translate(expr.Arguments[0]);
			TranslationHelpers.EmitCompareEq(tr, expr.Arguments[0].Type.Value, val, IRConstant.FromI4(0));
			var tmp = tr.Context.AllocateVRegister(ASTType.I4);
			tr.Instructions.Add(new IRInstruction(IROpCode.__GETF) {
				Operand1 = tmp,
				Operand2 = IRConstant.FromI4(1 << tr.Arch.Flags.ZERO)
			});
			tr.Instructions.Add(new IRInstruction(IROpCode.JNZ) {
				Operand1 = new IRBlockTarget((IBasicBlock)expr.Operand),
				Operand2 = tmp
			});
			return null;
		}
	}

	public class SwitchHandler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Switch; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			Debug.Assert(expr.Arguments.Length == 1);

			var val = tr.Translate(expr.Arguments[0]);
			tr.Instructions.Add(new IRInstruction(IROpCode.SWT) {
				Operand1 = new IRJumpTable((IBasicBlock[])expr.Operand),
				Operand2 = val
			});
			return null;
		}
	}
}