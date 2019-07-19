using System;
using System.Diagnostics;
using dnlib.DotNet.Emit;
using KoiVM.AST;
using KoiVM.AST.ILAST;
using KoiVM.AST.IR;

namespace KoiVM.VMIR.Translation {
	public class AddHandler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Add; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			Debug.Assert(expr.Arguments.Length == 2);
			var ret = tr.Context.AllocateVRegister(expr.Type.Value);
			tr.Instructions.Add(new IRInstruction(IROpCode.MOV) {
				Operand1 = ret,
				Operand2 = tr.Translate(expr.Arguments[0])
			});
			tr.Instructions.Add(new IRInstruction(IROpCode.ADD) {
				Operand1 = ret,
				Operand2 = tr.Translate(expr.Arguments[1])
			});
			return ret;
		}
	}

	public class AddOvfHandler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Add_Ovf; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			Debug.Assert(expr.Arguments.Length == 2);
			var ret = tr.Context.AllocateVRegister(expr.Type.Value);
			tr.Instructions.Add(new IRInstruction(IROpCode.MOV) {
				Operand1 = ret,
				Operand2 = tr.Translate(expr.Arguments[0])
			});
			tr.Instructions.Add(new IRInstruction(IROpCode.ADD) {
				Operand1 = ret,
				Operand2 = tr.Translate(expr.Arguments[1])
			});

			var ecallId = tr.VM.Runtime.VMCall.CKOVERFLOW;
			var fl = tr.Context.AllocateVRegister(expr.Type.Value);
			tr.Instructions.Add(new IRInstruction(IROpCode.__GETF) {
				Operand1 = fl,
				Operand2 = IRConstant.FromI4(1 << tr.Arch.Flags.OVERFLOW)
			});
			tr.Instructions.Add(new IRInstruction(IROpCode.VCALL, IRConstant.FromI4(ecallId), fl));
			return ret;
		}
	}

	public class AddOvfUnHandler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Add_Ovf_Un; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			Debug.Assert(expr.Arguments.Length == 2);
			var ret = tr.Context.AllocateVRegister(expr.Type.Value);
			tr.Instructions.Add(new IRInstruction(IROpCode.MOV) {
				Operand1 = ret,
				Operand2 = tr.Translate(expr.Arguments[0])
			});
			tr.Instructions.Add(new IRInstruction(IROpCode.ADD) {
				Operand1 = ret,
				Operand2 = tr.Translate(expr.Arguments[1])
			});

			var ecallId = tr.VM.Runtime.VMCall.CKOVERFLOW;
			var fl = tr.Context.AllocateVRegister(expr.Type.Value);
			tr.Instructions.Add(new IRInstruction(IROpCode.__GETF) {
				Operand1 = fl,
				Operand2 = IRConstant.FromI4(1 << tr.Arch.Flags.CARRY)
			});
			tr.Instructions.Add(new IRInstruction(IROpCode.VCALL, IRConstant.FromI4(ecallId), fl));
			return ret;
		}
	}

	public class SubHandler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Sub; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			Debug.Assert(expr.Arguments.Length == 2);
			var ret = tr.Context.AllocateVRegister(expr.Type.Value);
			if (expr.Type != null && (expr.Type.Value == ASTType.R4 || expr.Type.Value == ASTType.R8)) {
				tr.Instructions.Add(new IRInstruction(IROpCode.MOV) {
					Operand1 = ret,
					Operand2 = tr.Translate(expr.Arguments[0])
				});
				tr.Instructions.Add(new IRInstruction(IROpCode.SUB) {
					Operand1 = ret,
					Operand2 = tr.Translate(expr.Arguments[1])
				});
			}
			else {
				// A - B = A + (-B) = A + (~B + 1) = A + ~B + 1
				var tmp = tr.Context.AllocateVRegister(expr.Type.Value);
				tr.Instructions.Add(new IRInstruction(IROpCode.MOV) {
					Operand1 = ret,
					Operand2 = tr.Translate(expr.Arguments[0])
				});
				tr.Instructions.Add(new IRInstruction(IROpCode.MOV) {
					Operand1 = tmp,
					Operand2 = tr.Translate(expr.Arguments[1])
				});
				tr.Instructions.Add(new IRInstruction(IROpCode.__NOT) {
					Operand1 = tmp
				});
				tr.Instructions.Add(new IRInstruction(IROpCode.ADD) {
					Operand1 = ret,
					Operand2 = tmp
				});
				tr.Instructions.Add(new IRInstruction(IROpCode.ADD) {
					Operand1 = ret,
					Operand2 = IRConstant.FromI4(1)
				});
			}
			return ret;
		}
	}

	public class SubOvfHandler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Sub_Ovf; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			Debug.Assert(expr.Arguments.Length == 2);
			var ret = tr.Context.AllocateVRegister(expr.Type.Value);
			if (expr.Type != null && (expr.Type.Value == ASTType.R4 || expr.Type.Value == ASTType.R8)) {
				tr.Instructions.Add(new IRInstruction(IROpCode.MOV) {
					Operand1 = ret,
					Operand2 = tr.Translate(expr.Arguments[0])
				});
				tr.Instructions.Add(new IRInstruction(IROpCode.SUB) {
					Operand1 = ret,
					Operand2 = tr.Translate(expr.Arguments[1])
				});
			}
			else {
				// A - B = A + (-B) = A + (~B + 1) = A + ~B + 1
				var tmp = tr.Context.AllocateVRegister(expr.Type.Value);
				tr.Instructions.Add(new IRInstruction(IROpCode.MOV) {
					Operand1 = ret,
					Operand2 = tr.Translate(expr.Arguments[0])
				});
				tr.Instructions.Add(new IRInstruction(IROpCode.MOV) {
					Operand1 = tmp,
					Operand2 = tr.Translate(expr.Arguments[1])
				});
				tr.Instructions.Add(new IRInstruction(IROpCode.__NOT) {
					Operand1 = tmp
				});
				tr.Instructions.Add(new IRInstruction(IROpCode.ADD) {
					Operand1 = tmp,
					Operand2 = IRConstant.FromI4(1)
				});
				tr.Instructions.Add(new IRInstruction(IROpCode.ADD) {
					Operand1 = ret,
					Operand2 = tmp
				});
			}

			var ecallId = tr.VM.Runtime.VMCall.CKOVERFLOW;
			var fl = tr.Context.AllocateVRegister(expr.Type.Value);
			tr.Instructions.Add(new IRInstruction(IROpCode.__GETF) {
				Operand1 = fl,
				Operand2 = IRConstant.FromI4(1 << tr.Arch.Flags.OVERFLOW)
			});
			tr.Instructions.Add(new IRInstruction(IROpCode.VCALL, IRConstant.FromI4(ecallId), fl));
			return ret;
		}
	}

	public class SubOvfUnHandler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Sub_Ovf_Un; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			Debug.Assert(expr.Arguments.Length == 2);
			var ret = tr.Context.AllocateVRegister(expr.Type.Value);
			if (expr.Type != null && (expr.Type.Value == ASTType.R4 || expr.Type.Value == ASTType.R8)) {
				tr.Instructions.Add(new IRInstruction(IROpCode.MOV) {
					Operand1 = ret,
					Operand2 = tr.Translate(expr.Arguments[0])
				});
				tr.Instructions.Add(new IRInstruction(IROpCode.SUB) {
					Operand1 = ret,
					Operand2 = tr.Translate(expr.Arguments[1])
				});
			}
			else {
				// A - B = A + (-B) = A + (~B + 1) = A + ~B + 1
				var tmp = tr.Context.AllocateVRegister(expr.Type.Value);
				tr.Instructions.Add(new IRInstruction(IROpCode.MOV) {
					Operand1 = ret,
					Operand2 = tr.Translate(expr.Arguments[0])
				});
				tr.Instructions.Add(new IRInstruction(IROpCode.MOV) {
					Operand1 = tmp,
					Operand2 = tr.Translate(expr.Arguments[1])
				});
				tr.Instructions.Add(new IRInstruction(IROpCode.__NOT) {
					Operand1 = tmp
				});
				tr.Instructions.Add(new IRInstruction(IROpCode.ADD) {
					Operand1 = ret,
					Operand2 = tmp
				});
				tr.Instructions.Add(new IRInstruction(IROpCode.ADD) {
					Operand1 = ret,
					Operand2 = IRConstant.FromI4(1)
				});
			}

			var ecallId = tr.VM.Runtime.VMCall.CKOVERFLOW;
			var fl = tr.Context.AllocateVRegister(expr.Type.Value);
			tr.Instructions.Add(new IRInstruction(IROpCode.__GETF) {
				Operand1 = fl,
				Operand2 = IRConstant.FromI4(1 << tr.Arch.Flags.CARRY)
			});
			tr.Instructions.Add(new IRInstruction(IROpCode.VCALL, IRConstant.FromI4(ecallId), fl));
			return ret;
		}
	}

	public class MulHandler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Mul; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			Debug.Assert(expr.Arguments.Length == 2);
			var ret = tr.Context.AllocateVRegister(expr.Type.Value);
			tr.Instructions.Add(new IRInstruction(IROpCode.MOV) {
				Operand1 = ret,
				Operand2 = tr.Translate(expr.Arguments[0])
			});
			tr.Instructions.Add(new IRInstruction(IROpCode.MUL) {
				Operand1 = ret,
				Operand2 = tr.Translate(expr.Arguments[1])
			});
			return ret;
		}
	}

	public class MulOvfHandler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Mul_Ovf; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			Debug.Assert(expr.Arguments.Length == 2);
			var ret = tr.Context.AllocateVRegister(expr.Type.Value);
			tr.Instructions.Add(new IRInstruction(IROpCode.MOV) {
				Operand1 = ret,
				Operand2 = tr.Translate(expr.Arguments[0])
			});
			tr.Instructions.Add(new IRInstruction(IROpCode.MUL) {
				Operand1 = ret,
				Operand2 = tr.Translate(expr.Arguments[1])
			});

			var ecallId = tr.VM.Runtime.VMCall.CKOVERFLOW;
			var fl = tr.Context.AllocateVRegister(expr.Type.Value);
			tr.Instructions.Add(new IRInstruction(IROpCode.__GETF) {
				Operand1 = fl,
				Operand2 = IRConstant.FromI4(1 << tr.Arch.Flags.OVERFLOW)
			});
			tr.Instructions.Add(new IRInstruction(IROpCode.VCALL, IRConstant.FromI4(ecallId), fl));
			return ret;
		}
	}

	public class MulOvfUnHandler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Mul_Ovf_Un; }
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
			tr.Instructions.Add(new IRInstruction(IROpCode.MUL) {
				Operand1 = ret,
				Operand2 = tr.Translate(expr.Arguments[1])
			});

			var ecallId = tr.VM.Runtime.VMCall.CKOVERFLOW;
			var fl = tr.Context.AllocateVRegister(expr.Type.Value);
			tr.Instructions.Add(new IRInstruction(IROpCode.__GETF) {
				Operand1 = fl,
				Operand2 = IRConstant.FromI4(1 << tr.Arch.Flags.CARRY)
			});
			tr.Instructions.Add(new IRInstruction(IROpCode.VCALL, IRConstant.FromI4(ecallId), fl));
			return ret;
		}
	}

	public class DivHandler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Div; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			Debug.Assert(expr.Arguments.Length == 2);
			var ret = tr.Context.AllocateVRegister(expr.Type.Value);
			tr.Instructions.Add(new IRInstruction(IROpCode.MOV) {
				Operand1 = ret,
				Operand2 = tr.Translate(expr.Arguments[0])
			});
			tr.Instructions.Add(new IRInstruction(IROpCode.DIV) {
				Operand1 = ret,
				Operand2 = tr.Translate(expr.Arguments[1])
			});
			return ret;
		}
	}

	public class DivUnHandler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Div_Un; }
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
			tr.Instructions.Add(new IRInstruction(IROpCode.DIV) {
				Operand1 = ret,
				Operand2 = tr.Translate(expr.Arguments[1])
			});
			return ret;
		}
	}

	public class NegHandler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Neg; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			Debug.Assert(expr.Arguments.Length == 1);
			var ret = tr.Context.AllocateVRegister(expr.Type.Value);
			if (expr.Type != null && (expr.Type.Value == ASTType.R4 || expr.Type.Value == ASTType.R8)) {
				tr.Instructions.Add(new IRInstruction(IROpCode.MOV) {
					Operand1 = ret,
					Operand2 = IRConstant.FromI4(0)
				});
				tr.Instructions.Add(new IRInstruction(IROpCode.SUB) {
					Operand1 = ret,
					Operand2 = tr.Translate(expr.Arguments[0])
				});
			}
			else {
				// -A = ~A + 1
				tr.Instructions.Add(new IRInstruction(IROpCode.MOV) {
					Operand1 = ret,
					Operand2 = tr.Translate(expr.Arguments[0])
				});
				tr.Instructions.Add(new IRInstruction(IROpCode.__NOT) {
					Operand1 = ret
				});
				tr.Instructions.Add(new IRInstruction(IROpCode.ADD) {
					Operand1 = ret,
					Operand2 = IRConstant.FromI4(1)
				});
			}
			return ret;
		}
	}

	public class RemHandler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Rem; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			Debug.Assert(expr.Arguments.Length == 2);
			var ret = tr.Context.AllocateVRegister(expr.Type.Value);
			tr.Instructions.Add(new IRInstruction(IROpCode.MOV) {
				Operand1 = ret,
				Operand2 = tr.Translate(expr.Arguments[0])
			});
			tr.Instructions.Add(new IRInstruction(IROpCode.REM) {
				Operand1 = ret,
				Operand2 = tr.Translate(expr.Arguments[1])
			});
			return ret;
		}
	}

	public class RemUnHandler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Rem_Un; }
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
			tr.Instructions.Add(new IRInstruction(IROpCode.REM) {
				Operand1 = ret,
				Operand2 = tr.Translate(expr.Arguments[1])
			});
			return ret;
		}
	}
}