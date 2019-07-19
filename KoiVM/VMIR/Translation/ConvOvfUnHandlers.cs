using System;
using System.Diagnostics;
using dnlib.DotNet.Emit;
using KoiVM.AST;
using KoiVM.AST.ILAST;
using KoiVM.AST.IR;

namespace KoiVM.VMIR.Translation {
	public class ConvOvfI4UnHandler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Conv_Ovf_I4_Un; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			Debug.Assert(expr.Arguments.Length == 1);
			var value = tr.Translate(expr.Arguments[0]);

			var valueType = value.Type;
			var retVar = tr.Context.AllocateVRegister(ASTType.I4);

			var rangechk = tr.VM.Runtime.VMCall.RANGECHK;
			var ckovf = tr.VM.Runtime.VMCall.CKOVERFLOW;
			switch (valueType) {
				case ASTType.I4:
				case ASTType.I8:
				case ASTType.Ptr:
					tr.Instructions.Add(new IRInstruction(IROpCode.PUSH, IRConstant.FromI8(uint.MinValue)));
					tr.Instructions.Add(new IRInstruction(IROpCode.PUSH, IRConstant.FromI8(int.MaxValue)));
					tr.Instructions.Add(new IRInstruction(IROpCode.VCALL, IRConstant.FromI4(rangechk), value));
					tr.Instructions.Add(new IRInstruction(IROpCode.VCALL, IRConstant.FromI4(ckovf)));

					tr.Instructions.Add(new IRInstruction(IROpCode.MOV, retVar, value));
					break;
				case ASTType.R4:
				case ASTType.R8:
					var tmpVar = tr.Context.AllocateVRegister(ASTType.I8);
					var fl = tr.Context.AllocateVRegister(ASTType.I4);
					tr.Instructions.Add(new IRInstruction(IROpCode.__SETF) {
						Operand1 = IRConstant.FromI4(1 << tr.Arch.Flags.UNSIGNED)
					});
					tr.Instructions.Add(new IRInstruction(IROpCode.ICONV, tmpVar, value));
					tr.Instructions.Add(new IRInstruction(IROpCode.__GETF) {
						Operand1 = fl,
						Operand2 = IRConstant.FromI4(1 << tr.Arch.Flags.OVERFLOW)
					});
					tr.Instructions.Add(new IRInstruction(IROpCode.VCALL, IRConstant.FromI4(ckovf), fl));
					value = tmpVar;
					goto case ASTType.I8;
				default:
					throw new NotSupportedException();
			}
			return retVar;
		}
	}

	public class ConvOvfI8UnHandler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Conv_Ovf_I8_Un; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			Debug.Assert(expr.Arguments.Length == 1);
			var value = tr.Translate(expr.Arguments[0]);

			var valueType = value.Type;
			var retVar = tr.Context.AllocateVRegister(ASTType.I8);

			var rangechk = tr.VM.Runtime.VMCall.RANGECHK;
			var ckovf = tr.VM.Runtime.VMCall.CKOVERFLOW;
			switch (valueType) {
				case ASTType.I4:
				case ASTType.I8:
				case ASTType.Ptr:
					tr.Instructions.Add(new IRInstruction(IROpCode.PUSH, IRConstant.FromI8((long)ulong.MinValue)));
					tr.Instructions.Add(new IRInstruction(IROpCode.PUSH, IRConstant.FromI8(long.MaxValue)));
					tr.Instructions.Add(new IRInstruction(IROpCode.VCALL, IRConstant.FromI4(rangechk), value));
					tr.Instructions.Add(new IRInstruction(IROpCode.VCALL, IRConstant.FromI4(ckovf)));

					tr.Instructions.Add(new IRInstruction(IROpCode.MOV, retVar, value));
					break;
				case ASTType.R4:
				case ASTType.R8:
					var tmpVar = tr.Context.AllocateVRegister(ASTType.I8);
					var fl = tr.Context.AllocateVRegister(ASTType.I4);
					tr.Instructions.Add(new IRInstruction(IROpCode.__SETF) {
						Operand1 = IRConstant.FromI4(1 << tr.Arch.Flags.UNSIGNED)
					});
					tr.Instructions.Add(new IRInstruction(IROpCode.ICONV, tmpVar, value));
					tr.Instructions.Add(new IRInstruction(IROpCode.__GETF) {
						Operand1 = fl,
						Operand2 = IRConstant.FromI4(1 << tr.Arch.Flags.OVERFLOW)
					});
					tr.Instructions.Add(new IRInstruction(IROpCode.VCALL, IRConstant.FromI4(ckovf), fl));
					value = tmpVar;
					goto case ASTType.I8;
				default:
					throw new NotSupportedException();
			}
			return retVar;
		}
	}

	public class ConvOvfU1UnHandler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Conv_Ovf_U1_Un; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			Debug.Assert(expr.Arguments.Length == 1);
			var value = tr.Translate(expr.Arguments[0]);

			var valueType = value.Type;
			var retVar = tr.Context.AllocateVRegister(ASTType.I4);
			retVar.RawType = tr.Context.Method.Module.CorLibTypes.Byte;

			var rangechk = tr.VM.Runtime.VMCall.RANGECHK;
			var ckovf = tr.VM.Runtime.VMCall.CKOVERFLOW;
			switch (valueType) {
				case ASTType.I4:
				case ASTType.I8:
				case ASTType.Ptr:
					tr.Instructions.Add(new IRInstruction(IROpCode.PUSH, IRConstant.FromI8(byte.MinValue)));
					tr.Instructions.Add(new IRInstruction(IROpCode.PUSH, IRConstant.FromI8(byte.MaxValue)));
					tr.Instructions.Add(new IRInstruction(IROpCode.VCALL, IRConstant.FromI4(rangechk), value));
					tr.Instructions.Add(new IRInstruction(IROpCode.VCALL, IRConstant.FromI4(ckovf)));

					tr.Instructions.Add(new IRInstruction(IROpCode.MOV, retVar, value));
					break;

				case ASTType.R4:
				case ASTType.R8:
					var tmpVar = tr.Context.AllocateVRegister(ASTType.I8);
					var fl = tr.Context.AllocateVRegister(ASTType.I4);
					tr.Instructions.Add(new IRInstruction(IROpCode.__SETF) {
						Operand1 = IRConstant.FromI4(1 << tr.Arch.Flags.UNSIGNED)
					});
					tr.Instructions.Add(new IRInstruction(IROpCode.ICONV, tmpVar, value));
					tr.Instructions.Add(new IRInstruction(IROpCode.__GETF) {
						Operand1 = fl,
						Operand2 = IRConstant.FromI4(1 << tr.Arch.Flags.OVERFLOW)
					});
					tr.Instructions.Add(new IRInstruction(IROpCode.VCALL, IRConstant.FromI4(ckovf), fl));
					value = tmpVar;
					goto case ASTType.I8;
				default:
					throw new NotSupportedException();
			}
			return retVar;
		}
	}

	public class ConvOvfI1UnHandler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Conv_Ovf_I1_Un; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			Debug.Assert(expr.Arguments.Length == 1);
			var value = tr.Translate(expr.Arguments[0]);

			var valueType = value.Type;
			var t = tr.Context.AllocateVRegister(ASTType.I4);
			var retVar = tr.Context.AllocateVRegister(ASTType.I4);
			t.RawType = tr.Context.Method.Module.CorLibTypes.SByte;

			var rangechk = tr.VM.Runtime.VMCall.RANGECHK;
			var ckovf = tr.VM.Runtime.VMCall.CKOVERFLOW;
			switch (valueType) {
				case ASTType.I4:
				case ASTType.I8:
				case ASTType.Ptr:
					tr.Instructions.Add(new IRInstruction(IROpCode.PUSH, IRConstant.FromI8(byte.MinValue)));
					tr.Instructions.Add(new IRInstruction(IROpCode.PUSH, IRConstant.FromI8(sbyte.MaxValue)));
					tr.Instructions.Add(new IRInstruction(IROpCode.VCALL, IRConstant.FromI4(rangechk), value));
					tr.Instructions.Add(new IRInstruction(IROpCode.VCALL, IRConstant.FromI4(ckovf)));

					tr.Instructions.Add(new IRInstruction(IROpCode.MOV, t, value));
					tr.Instructions.Add(new IRInstruction(IROpCode.SX, retVar, t));
					break;

				case ASTType.R4:
				case ASTType.R8:
					var tmpVar = tr.Context.AllocateVRegister(ASTType.I8);
					var fl = tr.Context.AllocateVRegister(ASTType.I4);
					tr.Instructions.Add(new IRInstruction(IROpCode.__SETF) {
						Operand1 = IRConstant.FromI4(1 << tr.Arch.Flags.UNSIGNED)
					});
					tr.Instructions.Add(new IRInstruction(IROpCode.ICONV, tmpVar, value));
					tr.Instructions.Add(new IRInstruction(IROpCode.__GETF) {
						Operand1 = fl,
						Operand2 = IRConstant.FromI4(1 << tr.Arch.Flags.OVERFLOW)
					});
					tr.Instructions.Add(new IRInstruction(IROpCode.VCALL, IRConstant.FromI4(ckovf), fl));
					value = tmpVar;
					goto case ASTType.I8;
				default:
					throw new NotSupportedException();
			}
			return retVar;
		}
	}

	public class ConvOvfU2UnHandler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Conv_Ovf_U2_Un; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			Debug.Assert(expr.Arguments.Length == 1);
			var value = tr.Translate(expr.Arguments[0]);

			var valueType = value.Type;
			var retVar = tr.Context.AllocateVRegister(ASTType.I4);
			retVar.RawType = tr.Context.Method.Module.CorLibTypes.UInt16;

			var rangechk = tr.VM.Runtime.VMCall.RANGECHK;
			var ckovf = tr.VM.Runtime.VMCall.CKOVERFLOW;
			switch (valueType) {
				case ASTType.I4:
				case ASTType.I8:
				case ASTType.Ptr:
					tr.Instructions.Add(new IRInstruction(IROpCode.PUSH, IRConstant.FromI8(ushort.MinValue)));
					tr.Instructions.Add(new IRInstruction(IROpCode.PUSH, IRConstant.FromI8(ushort.MaxValue)));
					tr.Instructions.Add(new IRInstruction(IROpCode.VCALL, IRConstant.FromI4(rangechk), value));
					tr.Instructions.Add(new IRInstruction(IROpCode.VCALL, IRConstant.FromI4(ckovf)));

					tr.Instructions.Add(new IRInstruction(IROpCode.MOV, retVar, value));
					break;

				case ASTType.R4:
				case ASTType.R8:
					var tmpVar = tr.Context.AllocateVRegister(ASTType.I8);
					var fl = tr.Context.AllocateVRegister(ASTType.I4);
					tr.Instructions.Add(new IRInstruction(IROpCode.__SETF) {
						Operand1 = IRConstant.FromI4(1 << tr.Arch.Flags.UNSIGNED)
					});
					tr.Instructions.Add(new IRInstruction(IROpCode.ICONV, tmpVar, value));
					tr.Instructions.Add(new IRInstruction(IROpCode.__GETF) {
						Operand1 = fl,
						Operand2 = IRConstant.FromI4(1 << tr.Arch.Flags.OVERFLOW)
					});
					tr.Instructions.Add(new IRInstruction(IROpCode.VCALL, IRConstant.FromI4(ckovf), fl));
					value = tmpVar;
					goto case ASTType.I8;
				default:
					throw new NotSupportedException();
			}
			return retVar;
		}
	}

	public class ConvOvfI2UnHandler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Conv_Ovf_I2_Un; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			Debug.Assert(expr.Arguments.Length == 1);
			var value = tr.Translate(expr.Arguments[0]);

			var valueType = value.Type;
			var t = tr.Context.AllocateVRegister(ASTType.I4);
			var retVar = tr.Context.AllocateVRegister(ASTType.I4);
			t.RawType = tr.Context.Method.Module.CorLibTypes.Int16;

			var rangechk = tr.VM.Runtime.VMCall.RANGECHK;
			var ckovf = tr.VM.Runtime.VMCall.CKOVERFLOW;
			switch (valueType) {
				case ASTType.I4:
				case ASTType.I8:
				case ASTType.Ptr:
					tr.Instructions.Add(new IRInstruction(IROpCode.PUSH, IRConstant.FromI8(ushort.MinValue)));
					tr.Instructions.Add(new IRInstruction(IROpCode.PUSH, IRConstant.FromI8(short.MaxValue)));
					tr.Instructions.Add(new IRInstruction(IROpCode.VCALL, IRConstant.FromI4(rangechk), value));
					tr.Instructions.Add(new IRInstruction(IROpCode.VCALL, IRConstant.FromI4(ckovf)));

					tr.Instructions.Add(new IRInstruction(IROpCode.MOV, t, value));
					tr.Instructions.Add(new IRInstruction(IROpCode.SX, retVar, t));
					break;

				case ASTType.R4:
				case ASTType.R8:
					var tmpVar = tr.Context.AllocateVRegister(ASTType.I8);
					var fl = tr.Context.AllocateVRegister(ASTType.I4);
					tr.Instructions.Add(new IRInstruction(IROpCode.__SETF) {
						Operand1 = IRConstant.FromI4(1 << tr.Arch.Flags.UNSIGNED)
					});
					tr.Instructions.Add(new IRInstruction(IROpCode.ICONV, tmpVar, value));
					tr.Instructions.Add(new IRInstruction(IROpCode.__GETF) {
						Operand1 = fl,
						Operand2 = IRConstant.FromI4(1 << tr.Arch.Flags.OVERFLOW)
					});
					tr.Instructions.Add(new IRInstruction(IROpCode.VCALL, IRConstant.FromI4(ckovf), fl));
					value = tmpVar;
					goto case ASTType.I8;
				default:
					throw new NotSupportedException();
			}
			return retVar;
		}
	}

	public class ConvOvfU4UnHandler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Conv_Ovf_U4_Un; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			Debug.Assert(expr.Arguments.Length == 1);
			var value = tr.Translate(expr.Arguments[0]);

			var valueType = value.Type;
			var retVar = tr.Context.AllocateVRegister(ASTType.I4);

			var rangechk = tr.VM.Runtime.VMCall.RANGECHK;
			var ckovf = tr.VM.Runtime.VMCall.CKOVERFLOW;
			switch (valueType) {
				case ASTType.I4:
				case ASTType.I8:
				case ASTType.Ptr:
					tr.Instructions.Add(new IRInstruction(IROpCode.PUSH, IRConstant.FromI8(uint.MinValue)));
					tr.Instructions.Add(new IRInstruction(IROpCode.PUSH, IRConstant.FromI8(uint.MaxValue)));
					tr.Instructions.Add(new IRInstruction(IROpCode.VCALL, IRConstant.FromI4(rangechk), value));
					tr.Instructions.Add(new IRInstruction(IROpCode.VCALL, IRConstant.FromI4(ckovf)));

					tr.Instructions.Add(new IRInstruction(IROpCode.MOV, retVar, value));
					break;

				case ASTType.R4:
				case ASTType.R8:
					var tmpVar = tr.Context.AllocateVRegister(ASTType.I8);
					var fl = tr.Context.AllocateVRegister(ASTType.I4);
					tr.Instructions.Add(new IRInstruction(IROpCode.__SETF) {
						Operand1 = IRConstant.FromI4(1 << tr.Arch.Flags.UNSIGNED)
					});
					tr.Instructions.Add(new IRInstruction(IROpCode.ICONV, tmpVar, value));
					tr.Instructions.Add(new IRInstruction(IROpCode.__GETF) {
						Operand1 = fl,
						Operand2 = IRConstant.FromI4(1 << tr.Arch.Flags.OVERFLOW)
					});
					tr.Instructions.Add(new IRInstruction(IROpCode.VCALL, IRConstant.FromI4(ckovf), fl));
					value = tmpVar;
					goto case ASTType.I8;
				default:
					throw new NotSupportedException();
			}
			return retVar;
		}
	}

	public class ConvOvfU8UnHandler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Conv_Ovf_U8_Un; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			Debug.Assert(expr.Arguments.Length == 1);
			var value = tr.Translate(expr.Arguments[0]);
			// TODO: no exception possible?
			return value;
		}
	}

	public class ConvOvfUUnHandler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Conv_Ovf_U_Un; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			// TODO: overflow?
			Debug.Assert(expr.Arguments.Length == 1);
			var value = tr.Translate(expr.Arguments[0]);

			var valueType = value.Type;
			if (valueType == ASTType.Ptr || valueType == ASTType.I4) // no conversion needed.
				return value;
			var retVar = tr.Context.AllocateVRegister(ASTType.Ptr);
			switch (valueType) {
				case ASTType.R4:
				case ASTType.R8:
					var tmp = tr.Context.AllocateVRegister(ASTType.I8);
					tr.Instructions.Add(new IRInstruction(IROpCode.__SETF) {
						Operand1 = IRConstant.FromI4(1 << tr.Arch.Flags.UNSIGNED)
					});
					tr.Instructions.Add(new IRInstruction(IROpCode.ICONV, tmp, value));
					tr.Instructions.Add(new IRInstruction(IROpCode.MOV, retVar, tmp));
					break;
				case ASTType.I8:
					tr.Instructions.Add(new IRInstruction(IROpCode.MOV, retVar, value));
					break;
				default:
					throw new NotSupportedException();
			}
			return retVar;
		}
	}

	public class ConvOvfIUnHandler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Conv_Ovf_I_Un; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			// TODO: overflow?

			Debug.Assert(expr.Arguments.Length == 1);
			var value = tr.Translate(expr.Arguments[0]);

			var valueType = value.Type;
			if (valueType == ASTType.Ptr || valueType == ASTType.I4) // no conversion needed.
				return value;
			var retVar = tr.Context.AllocateVRegister(ASTType.Ptr);
			switch (valueType) {
				case ASTType.R4:
				case ASTType.R8:
					var tmp = tr.Context.AllocateVRegister(ASTType.I8);
					tr.Instructions.Add(new IRInstruction(IROpCode.__SETF) {
						Operand1 = IRConstant.FromI4(1 << tr.Arch.Flags.UNSIGNED)
					});
					tr.Instructions.Add(new IRInstruction(IROpCode.ICONV, tmp, value));
					tr.Instructions.Add(new IRInstruction(IROpCode.MOV, retVar, tmp));
					break;
				case ASTType.I8:
					tr.Instructions.Add(new IRInstruction(IROpCode.MOV, retVar, value));
					break;
				default:
					throw new NotSupportedException();
			}
			return retVar;
		}
	}
}