using System;
using System.Diagnostics;
using dnlib.DotNet.Emit;
using KoiVM.AST.ILAST;
using KoiVM.AST.IR;

namespace KoiVM.VMIR.Translation {
	public class AndHandler : ITranslationHandler {
		public Code ILCode {
			get { return Code.And; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			Debug.Assert(expr.Arguments.Length == 2);
			var ret = tr.Context.AllocateVRegister(expr.Type.Value);
			tr.Instructions.Add(new IRInstruction(IROpCode.MOV) {
				Operand1 = ret,
				Operand2 = tr.Translate(expr.Arguments[0])
			});
			tr.Instructions.Add(new IRInstruction(IROpCode.__AND) {
				Operand1 = ret,
				Operand2 = tr.Translate(expr.Arguments[1])
			});
			return ret;
		}
	}

	public class OrHandler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Or; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			Debug.Assert(expr.Arguments.Length == 2);
			var ret = tr.Context.AllocateVRegister(expr.Type.Value);
			tr.Instructions.Add(new IRInstruction(IROpCode.MOV) {
				Operand1 = ret,
				Operand2 = tr.Translate(expr.Arguments[0])
			});
			tr.Instructions.Add(new IRInstruction(IROpCode.__OR) {
				Operand1 = ret,
				Operand2 = tr.Translate(expr.Arguments[1])
			});
			return ret;
		}
	}

	public class XorHandler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Xor; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			Debug.Assert(expr.Arguments.Length == 2);
			var ret = tr.Context.AllocateVRegister(expr.Type.Value);
			tr.Instructions.Add(new IRInstruction(IROpCode.MOV) {
				Operand1 = ret,
				Operand2 = tr.Translate(expr.Arguments[0])
			});
			tr.Instructions.Add(new IRInstruction(IROpCode.__XOR) {
				Operand1 = ret,
				Operand2 = tr.Translate(expr.Arguments[1])
			});
			return ret;
		}
	}

	public class NotHandler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Not; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			Debug.Assert(expr.Arguments.Length == 1);
			var ret = tr.Context.AllocateVRegister(expr.Type.Value);
			tr.Instructions.Add(new IRInstruction(IROpCode.MOV) {
				Operand1 = ret,
				Operand2 = tr.Translate(expr.Arguments[0])
			});
			tr.Instructions.Add(new IRInstruction(IROpCode.__NOT) {
				Operand1 = ret
			});
			return ret;
		}
	}

	public class ShlHandler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Shl; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			Debug.Assert(expr.Arguments.Length == 2);
			var ret = tr.Context.AllocateVRegister(expr.Type.Value);
			tr.Instructions.Add(new IRInstruction(IROpCode.MOV) {
				Operand1 = ret,
				Operand2 = tr.Translate(expr.Arguments[0])
			});
			tr.Instructions.Add(new IRInstruction(IROpCode.SHL) {
				Operand1 = ret,
				Operand2 = tr.Translate(expr.Arguments[1])
			});
			return ret;
		}
	}

	public class ShrHandler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Shr; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			Debug.Assert(expr.Arguments.Length == 2);
			var ret = tr.Context.AllocateVRegister(expr.Type.Value);
			tr.Instructions.Add(new IRInstruction(IROpCode.MOV) {
				Operand1 = ret,
				Operand2 = tr.Translate(expr.Arguments[0])
			});
			tr.Instructions.Add(new IRInstruction(IROpCode.SHR) {
				Operand1 = ret,
				Operand2 = tr.Translate(expr.Arguments[1])
			});
			return ret;
		}
	}

	public class ShrUnHandler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Shr_Un; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			Debug.Assert(expr.Arguments.Length == 2);
			var ret = tr.Context.AllocateVRegister(expr.Type.Value);
			tr.Instructions.Add(new IRInstruction(IROpCode.MOV) {
				Operand1 = ret,
				Operand2 = tr.Translate(expr.Arguments[0])
			});
			tr.Instructions.Add(new IRInstruction(IROpCode.__SETF) {
				Operand1 = IRConstant.FromI4(1 << tr.Arch.Flags.UNSIGNED)
			});
			tr.Instructions.Add(new IRInstruction(IROpCode.SHR) {
				Operand1 = ret,
				Operand2 = tr.Translate(expr.Arguments[1])
			});
			return ret;
		}
	}
}