using System;
using System.Diagnostics;
using dnlib.DotNet.Emit;
using KoiVM.AST;
using KoiVM.AST.ILAST;
using KoiVM.AST.IR;

namespace KoiVM.VMIR.Translation {
	public class ConvI4Handler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Conv_I4; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			Debug.Assert(expr.Arguments.Length == 1);
			var value = tr.Translate(expr.Arguments[0]);

			var valueType = value.Type;
			if (valueType == ASTType.I4) // no conversion needed.
				return value;
			var retVar = tr.Context.AllocateVRegister(ASTType.I4);
			switch (valueType) {
				case ASTType.I8:
				case ASTType.Ptr:
					tr.Instructions.Add(new IRInstruction(IROpCode.MOV, retVar, value));
					break;
				case ASTType.R4:
				case ASTType.R8:
					var tmpVar = tr.Context.AllocateVRegister(ASTType.I8);
					tr.Instructions.Add(new IRInstruction(IROpCode.ICONV, tmpVar, value));
					tr.Instructions.Add(new IRInstruction(IROpCode.MOV, retVar, tmpVar));
					break;
				default:
					throw new NotSupportedException();
			}
			return retVar;
		}
	}

	public class ConvI8Handler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Conv_I8; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			Debug.Assert(expr.Arguments.Length == 1);
			var value = tr.Translate(expr.Arguments[0]);

			var valueType = value.Type;
			if (valueType == ASTType.I8) // no conversion needed.
				return value;
			var retVar = tr.Context.AllocateVRegister(ASTType.I8);
			switch (valueType) {
				case ASTType.I4:
					tr.Instructions.Add(new IRInstruction(IROpCode.SX, retVar, value));
					break;
				case ASTType.Ptr:
					tr.Instructions.Add(new IRInstruction(IROpCode.MOV, retVar, value));
					break;
				case ASTType.R4:
				case ASTType.R8:
					tr.Instructions.Add(new IRInstruction(IROpCode.ICONV, retVar, value));
					break;
				default:
					throw new NotSupportedException();
			}
			return retVar;
		}
	}

	public class ConvU1Handler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Conv_U1; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			Debug.Assert(expr.Arguments.Length == 1);
			var value = tr.Translate(expr.Arguments[0]);

			var valueType = value.Type;
			var retVar = tr.Context.AllocateVRegister(ASTType.I4);
			retVar.RawType = tr.Context.Method.Module.CorLibTypes.Byte;
			switch (valueType) {
				case ASTType.I4:
				case ASTType.I8:
				case ASTType.Ptr:
					tr.Instructions.Add(new IRInstruction(IROpCode.MOV, retVar, value));
					break;
				case ASTType.R4:
				case ASTType.R8:
					var tmp = tr.Context.AllocateVRegister(ASTType.I8);
					tr.Instructions.Add(new IRInstruction(IROpCode.ICONV, tmp, value));
					tr.Instructions.Add(new IRInstruction(IROpCode.MOV, retVar, tmp));
					break;
				default:
					throw new NotSupportedException();
			}
			return retVar;
		}
	}

	public class ConvI1Handler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Conv_I1; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			Debug.Assert(expr.Arguments.Length == 1);
			var value = tr.Translate(expr.Arguments[0]);

			var valueType = value.Type;
			var t = tr.Context.AllocateVRegister(ASTType.I4);
			var retVar = tr.Context.AllocateVRegister(ASTType.I4);
			t.RawType = tr.Context.Method.Module.CorLibTypes.SByte;
			switch (valueType) {
				case ASTType.I4:
				case ASTType.I8:
				case ASTType.Ptr:
					tr.Instructions.Add(new IRInstruction(IROpCode.MOV, t, value));
					tr.Instructions.Add(new IRInstruction(IROpCode.SX, retVar, t));
					break;
				case ASTType.R4:
				case ASTType.R8:
					var tmp = tr.Context.AllocateVRegister(ASTType.I8);
					tr.Instructions.Add(new IRInstruction(IROpCode.ICONV, tmp, value));
					tr.Instructions.Add(new IRInstruction(IROpCode.MOV, t, tmp));
					tr.Instructions.Add(new IRInstruction(IROpCode.SX, retVar, t));
					break;
				default:
					throw new NotSupportedException();
			}
			return retVar;
		}
	}

	public class ConvU2Handler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Conv_U2; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			Debug.Assert(expr.Arguments.Length == 1);
			var value = tr.Translate(expr.Arguments[0]);

			var valueType = value.Type;
			var retVar = tr.Context.AllocateVRegister(ASTType.I4);
			retVar.RawType = tr.Context.Method.Module.CorLibTypes.UInt16;
			switch (valueType) {
				case ASTType.I4:
				case ASTType.I8:
				case ASTType.Ptr:
					tr.Instructions.Add(new IRInstruction(IROpCode.MOV, retVar, value));
					break;
				case ASTType.R4:
				case ASTType.R8:
					var tmp = tr.Context.AllocateVRegister(ASTType.I8);
					tr.Instructions.Add(new IRInstruction(IROpCode.ICONV, tmp, value));
					tr.Instructions.Add(new IRInstruction(IROpCode.MOV, retVar, tmp));
					break;
				default:
					throw new NotSupportedException();
			}
			return retVar;
		}
	}

	public class ConvI2Handler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Conv_I2; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			Debug.Assert(expr.Arguments.Length == 1);
			var value = tr.Translate(expr.Arguments[0]);

			var valueType = value.Type;
			var t = tr.Context.AllocateVRegister(ASTType.I4);
			var retVar = tr.Context.AllocateVRegister(ASTType.I4);
			t.RawType = tr.Context.Method.Module.CorLibTypes.Int16;
			switch (valueType) {
				case ASTType.I4:
				case ASTType.I8:
				case ASTType.Ptr:
					tr.Instructions.Add(new IRInstruction(IROpCode.MOV, t, value));
					tr.Instructions.Add(new IRInstruction(IROpCode.SX, retVar, t));
					break;
				case ASTType.R4:
				case ASTType.R8:
					var tmp = tr.Context.AllocateVRegister(ASTType.I8);
					tr.Instructions.Add(new IRInstruction(IROpCode.ICONV, tmp, value));
					tr.Instructions.Add(new IRInstruction(IROpCode.MOV, t, tmp));
					tr.Instructions.Add(new IRInstruction(IROpCode.SX, retVar, t));
					break;
				default:
					throw new NotSupportedException();
			}
			return retVar;
		}
	}

	public class ConvU4Handler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Conv_U4; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			Debug.Assert(expr.Arguments.Length == 1);
			var value = tr.Translate(expr.Arguments[0]);

			var valueType = value.Type;
			if (valueType == ASTType.I4) // no conversion needed.
				return value;
			var retVar = tr.Context.AllocateVRegister(ASTType.I4);
			switch (valueType) {
				case ASTType.I8:
				case ASTType.Ptr:
					tr.Instructions.Add(new IRInstruction(IROpCode.MOV, retVar, value));
					break;
				case ASTType.R4:
				case ASTType.R8:
					var tmp = tr.Context.AllocateVRegister(ASTType.I8);
					tr.Instructions.Add(new IRInstruction(IROpCode.ICONV, tmp, value));
					tr.Instructions.Add(new IRInstruction(IROpCode.MOV, retVar, tmp));
					break;
				default:
					throw new NotSupportedException();
			}
			return retVar;
		}
	}

	public class ConvU8Handler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Conv_U8; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			Debug.Assert(expr.Arguments.Length == 1);
			var value = tr.Translate(expr.Arguments[0]);

			var valueType = value.Type;
			if (valueType == ASTType.I8) // no conversion needed.
				return value;
			var retVar = tr.Context.AllocateVRegister(ASTType.I8);
			switch (valueType) {
				case ASTType.I4:
				case ASTType.Ptr:
					tr.Instructions.Add(new IRInstruction(IROpCode.MOV, retVar, value));
					break;
				case ASTType.R4:
				case ASTType.R8:
					var tmp = tr.Context.AllocateVRegister(ASTType.I8);
					tr.Instructions.Add(new IRInstruction(IROpCode.__SETF) {
						Operand1 = IRConstant.FromI4(1 << tr.Arch.Flags.UNSIGNED)
					});
					tr.Instructions.Add(new IRInstruction(IROpCode.ICONV, tmp, value));
					break;
				default:
					throw new NotSupportedException();
			}
			return retVar;
		}
	}

	public class ConvUHandler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Conv_U; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			Debug.Assert(expr.Arguments.Length == 1);
			var value = tr.Translate(expr.Arguments[0]);

			var valueType = value.Type;
			if (valueType == ASTType.Ptr || valueType == ASTType.I4) // no conversion needed.
				return value;
			var retVar = tr.Context.AllocateVRegister(ASTType.Ptr);
			switch (valueType) {
				case ASTType.I8:
					tr.Instructions.Add(new IRInstruction(IROpCode.MOV, retVar, value));
					break;
				case ASTType.R4:
				case ASTType.R8:
					var tmp = tr.Context.AllocateVRegister(ASTType.I8);
					tr.Instructions.Add(new IRInstruction(IROpCode.__SETF) {
						Operand1 = IRConstant.FromI4(1 << tr.Arch.Flags.UNSIGNED)
					});
					tr.Instructions.Add(new IRInstruction(IROpCode.ICONV, tmp, value));
					tr.Instructions.Add(new IRInstruction(IROpCode.MOV, retVar, tmp));
					break;
				default:
					throw new NotSupportedException();
			}
			return retVar;
		}
	}

	public class ConvIHandler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Conv_I; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			Debug.Assert(expr.Arguments.Length == 1);
			var value = tr.Translate(expr.Arguments[0]);

			var valueType = value.Type;
			if (valueType == ASTType.Ptr || valueType == ASTType.I4) // no conversion needed.
				return value;
			var retVar = tr.Context.AllocateVRegister(ASTType.Ptr);
			switch (valueType) {
				case ASTType.I8:
					tr.Instructions.Add(new IRInstruction(IROpCode.MOV, retVar, value));
					break;
				case ASTType.R4:
				case ASTType.R8:
					var tmp = tr.Context.AllocateVRegister(ASTType.I8);
					tr.Instructions.Add(new IRInstruction(IROpCode.ICONV, tmp, value));
					tr.Instructions.Add(new IRInstruction(IROpCode.MOV, retVar, tmp));
					break;
				default:
					throw new NotSupportedException();
			}
			return retVar;
		}
	}

	public class ConvR4Handler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Conv_R4; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			Debug.Assert(expr.Arguments.Length == 1);
			var value = tr.Translate(expr.Arguments[0]);

			var valueType = value.Type;
			if (valueType == ASTType.R4) // no conversion needed.
				return value;
			var retVar = tr.Context.AllocateVRegister(ASTType.R4);
			switch (valueType) {
				case ASTType.I4:
					var tmpVar = tr.Context.AllocateVRegister(ASTType.I8);
					tr.Instructions.Add(new IRInstruction(IROpCode.SX, tmpVar, value));
					tr.Instructions.Add(new IRInstruction(IROpCode.FCONV, retVar, tmpVar));
					break;

				case ASTType.I8:
					tr.Instructions.Add(new IRInstruction(IROpCode.FCONV, retVar, value));
					break;

				case ASTType.R8:
					tr.Instructions.Add(new IRInstruction(IROpCode.FCONV, retVar, value));
					break;

				default:
					throw new NotSupportedException();
			}
			return retVar;
		}
	}

	public class ConvR8Handler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Conv_R8; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			Debug.Assert(expr.Arguments.Length == 1);
			var value = tr.Translate(expr.Arguments[0]);

			var valueType = value.Type;
			if (valueType == ASTType.R8) // no conversion needed.
				return value;
			var retVar = tr.Context.AllocateVRegister(ASTType.R8);
			switch (valueType) {
				case ASTType.I4:
					var tmpVar = tr.Context.AllocateVRegister(ASTType.I8);
					tr.Instructions.Add(new IRInstruction(IROpCode.SX, tmpVar, value));
					tr.Instructions.Add(new IRInstruction(IROpCode.FCONV, retVar, tmpVar));
					break;

				case ASTType.I8:
					tr.Instructions.Add(new IRInstruction(IROpCode.FCONV, retVar, value));
					break;

				case ASTType.R4:
					tr.Instructions.Add(new IRInstruction(IROpCode.FCONV, retVar, value));
					break;

				default:
					throw new NotSupportedException();
			}
			return retVar;
		}
	}

	public class ConvRUnHandler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Conv_R_Un; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			Debug.Assert(expr.Arguments.Length == 1);
			var value = tr.Translate(expr.Arguments[0]);

			var valueType = value.Type;
			var retVar = tr.Context.AllocateVRegister(ASTType.R8);
			switch (valueType) {
				case ASTType.I4:
					var tmpVar = tr.Context.AllocateVRegister(ASTType.I8);
					tr.Instructions.Add(new IRInstruction(IROpCode.SX, tmpVar, value));
					tr.Instructions.Add(new IRInstruction(IROpCode.__SETF) {
						Operand1 = IRConstant.FromI4(1 << tr.Arch.Flags.UNSIGNED)
					});
					tr.Instructions.Add(new IRInstruction(IROpCode.FCONV, retVar, tmpVar));
					break;

				case ASTType.I8:
					tr.Instructions.Add(new IRInstruction(IROpCode.__SETF) {
						Operand1 = IRConstant.FromI4(1 << tr.Arch.Flags.UNSIGNED)
					});
					tr.Instructions.Add(new IRInstruction(IROpCode.FCONV, retVar, value));
					break;

				default:
					throw new NotSupportedException();
			}
			return retVar;
		}
	}
}