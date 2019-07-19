using System;
using System.Diagnostics;
using dnlib.DotNet.Emit;
using KoiVM.AST;
using KoiVM.AST.ILAST;
using KoiVM.AST.IR;

namespace KoiVM.VMIR.Translation {
	public class CeqHandler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Ceq; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			Debug.Assert(expr.Arguments.Length == 2);
			var ret = tr.Context.AllocateVRegister(ASTType.I4);

			TranslationHelpers.EmitCompareEq(tr, expr.Arguments[0].Type.Value,
				tr.Translate(expr.Arguments[0]), tr.Translate(expr.Arguments[1]));
			tr.Instructions.Add(new IRInstruction(IROpCode.__GETF) {
				Operand1 = ret,
				Operand2 = IRConstant.FromI4(1 << tr.Arch.Flags.ZERO)
			});
			return ret;
		}
	}

	public class CgtHandler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Cgt; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			Debug.Assert(expr.Arguments.Length == 2);

			tr.Instructions.Add(new IRInstruction(IROpCode.CMP) {
				Operand1 = tr.Translate(expr.Arguments[0]),
				Operand2 = tr.Translate(expr.Arguments[1])
			});
			// SF=OF & ZF=0
			// (FL=S|O) or (FL=0), FL=S|O|Z
			var ret = tr.Context.AllocateVRegister(ASTType.I4);
			var fl = tr.Context.AllocateVRegister(ASTType.I4);
			tr.Instructions.Add(new IRInstruction(IROpCode.__GETF) {
				Operand1 = fl,
				Operand2 = IRConstant.FromI4(1 << tr.Arch.Flags.OVERFLOW | 1 << tr.Arch.Flags.SIGN | 1 << tr.Arch.Flags.ZERO)
			});
			tr.Instructions.Add(new IRInstruction(IROpCode.MOV) {
				Operand1 = ret,
				Operand2 = fl
			});
			TranslationHelpers.EmitCompareEq(tr, ASTType.I4,
				ret, IRConstant.FromI4(1 << tr.Arch.Flags.OVERFLOW | 1 << tr.Arch.Flags.SIGN));
			tr.Instructions.Add(new IRInstruction(IROpCode.__GETF) {
				Operand1 = ret,
				Operand2 = IRConstant.FromI4(1 << tr.Arch.Flags.ZERO)
			});
			tr.Instructions.Add(new IRInstruction(IROpCode.__AND) {
				Operand1 = fl,
				Operand2 = fl
			});
			tr.Instructions.Add(new IRInstruction(IROpCode.__GETF) {
				Operand1 = fl,
				Operand2 = IRConstant.FromI4(1 << tr.Arch.Flags.ZERO)
			});
			tr.Instructions.Add(new IRInstruction(IROpCode.__OR) {
				Operand1 = ret,
				Operand2 = fl
			});
			return ret;
		}
	}

	public class CgtUnHandler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Cgt_Un; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			Debug.Assert(expr.Arguments.Length == 2);

			tr.Instructions.Add(new IRInstruction(IROpCode.CMP) {
				Operand1 = tr.Translate(expr.Arguments[0]),
				Operand2 = tr.Translate(expr.Arguments[1])
			});
			// CF=0 & ZF=0
			var ret = tr.Context.AllocateVRegister(ASTType.I4);
			tr.Instructions.Add(new IRInstruction(IROpCode.__GETF) {
				Operand1 = ret,
				Operand2 = IRConstant.FromI4(1 << tr.Arch.Flags.CARRY | 1 << tr.Arch.Flags.ZERO)
			});
			tr.Instructions.Add(new IRInstruction(IROpCode.__OR) {
				Operand1 = ret,
				Operand2 = ret
			});
			tr.Instructions.Add(new IRInstruction(IROpCode.__GETF) {
				Operand1 = ret,
				Operand2 = IRConstant.FromI4(1 << tr.Arch.Flags.ZERO)
			});
			return ret;
		}
	}

	public class CltHandler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Clt; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			Debug.Assert(expr.Arguments.Length == 2);

			tr.Instructions.Add(new IRInstruction(IROpCode.CMP) {
				Operand1 = tr.Translate(expr.Arguments[0]),
				Operand2 = tr.Translate(expr.Arguments[1])
			});
			// SF<>OF
			// !((FL=S|O) or (FL=0)), FL=S|O
			var ret = tr.Context.AllocateVRegister(ASTType.I4);
			var fl = tr.Context.AllocateVRegister(ASTType.I4);
			tr.Instructions.Add(new IRInstruction(IROpCode.__GETF) {
				Operand1 = fl,
				Operand2 = IRConstant.FromI4(1 << tr.Arch.Flags.OVERFLOW | 1 << tr.Arch.Flags.SIGN)
			});
			tr.Instructions.Add(new IRInstruction(IROpCode.MOV) {
				Operand1 = ret,
				Operand2 = fl
			});
			TranslationHelpers.EmitCompareEq(tr, ASTType.I4,
				ret, IRConstant.FromI4(1 << tr.Arch.Flags.OVERFLOW | 1 << tr.Arch.Flags.SIGN));
			tr.Instructions.Add(new IRInstruction(IROpCode.__GETF) {
				Operand1 = ret,
				Operand2 = IRConstant.FromI4(1 << tr.Arch.Flags.ZERO)
			});
			tr.Instructions.Add(new IRInstruction(IROpCode.__AND) {
				Operand1 = fl,
				Operand2 = fl
			});
			tr.Instructions.Add(new IRInstruction(IROpCode.__GETF) {
				Operand1 = fl,
				Operand2 = IRConstant.FromI4(1 << tr.Arch.Flags.ZERO)
			});
			tr.Instructions.Add(new IRInstruction(IROpCode.__OR) {
				Operand1 = ret,
				Operand2 = fl
			});
			tr.Instructions.Add(new IRInstruction(IROpCode.__GETF) {
				Operand1 = ret,
				Operand2 = IRConstant.FromI4(1 << tr.Arch.Flags.ZERO)
			});
			return ret;
		}
	}

	public class CltUnHandler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Clt_Un; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			Debug.Assert(expr.Arguments.Length == 2);

			tr.Instructions.Add(new IRInstruction(IROpCode.CMP) {
				Operand1 = tr.Translate(expr.Arguments[0]),
				Operand2 = tr.Translate(expr.Arguments[1])
			});
			// CF=1
			var ret = tr.Context.AllocateVRegister(ASTType.I4);
			tr.Instructions.Add(new IRInstruction(IROpCode.__GETF) {
				Operand1 = ret,
				Operand2 = IRConstant.FromI4(1 << tr.Arch.Flags.CARRY)
			});
			return ret;
		}
	}
}