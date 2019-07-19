using System;
using System.Diagnostics;
using dnlib.DotNet;
using KoiVM.AST;
using KoiVM.AST.IL;
using KoiVM.AST.IR;
using KoiVM.VMIR;

namespace KoiVM.VMIL.Translation {
	public class NorHandler : ITranslationHandler {
		public IROpCode IRCode {
			get { return IROpCode.NOR; }
		}

		public void Translate(IRInstruction instr, ILTranslator tr) {
			tr.PushOperand(instr.Operand1);
			tr.PushOperand(instr.Operand2);
			var type = TypeInference.InferIntegerOp(instr.Operand1.Type, instr.Operand2.Type);
			switch (type) {
				case ASTType.I4:
					tr.Instructions.Add(new ILInstruction(ILOpCode.NOR_DWORD));
					break;
				case ASTType.I8:
				case ASTType.Ptr:
					tr.Instructions.Add(new ILInstruction(ILOpCode.NOR_QWORD));
					break;
				default:
					throw new NotSupportedException();
			}
			tr.PopOperand(instr.Operand1);
		}
	}

	public class ShlHandler : ITranslationHandler {
		public IROpCode IRCode {
			get { return IROpCode.SHL; }
		}

		public void Translate(IRInstruction instr, ILTranslator tr) {
			tr.PushOperand(instr.Operand1);
			tr.PushOperand(instr.Operand2);
			var type = TypeInference.InferShiftOp(instr.Operand1.Type, instr.Operand2.Type);
			switch (type) {
				case ASTType.I4:
					tr.Instructions.Add(new ILInstruction(ILOpCode.SHL_DWORD));
					break;
				case ASTType.I8:
				case ASTType.Ptr:
					tr.Instructions.Add(new ILInstruction(ILOpCode.SHL_QWORD));
					break;
				default:
					throw new NotSupportedException();
			}
			tr.PopOperand(instr.Operand1);
		}
	}

	public class ShrHandler : ITranslationHandler {
		public IROpCode IRCode {
			get { return IROpCode.SHR; }
		}

		public void Translate(IRInstruction instr, ILTranslator tr) {
			tr.PushOperand(instr.Operand1);
			tr.PushOperand(instr.Operand2);
			var type = TypeInference.InferShiftOp(instr.Operand1.Type, instr.Operand2.Type);
			switch (type) {
				case ASTType.I4:
					tr.Instructions.Add(new ILInstruction(ILOpCode.SHR_DWORD));
					break;
				case ASTType.I8:
				case ASTType.Ptr:
					tr.Instructions.Add(new ILInstruction(ILOpCode.SHR_QWORD));
					break;
				default:
					throw new NotSupportedException();
			}
			tr.PopOperand(instr.Operand1);
		}
	}

	public class AddHandler : ITranslationHandler {
		public IROpCode IRCode {
			get { return IROpCode.ADD; }
		}

		public void Translate(IRInstruction instr, ILTranslator tr) {
			tr.PushOperand(instr.Operand1);
			tr.PushOperand(instr.Operand2);
			var type = TypeInference.InferBinaryOp(instr.Operand1.Type, instr.Operand2.Type);
			switch (type) {
				case ASTType.I4:
					tr.Instructions.Add(new ILInstruction(ILOpCode.ADD_DWORD));
					break;
				case ASTType.I8:
				case ASTType.Ptr:
				case ASTType.ByRef:
					tr.Instructions.Add(new ILInstruction(ILOpCode.ADD_QWORD));
					break;
				case ASTType.R4:
					tr.Instructions.Add(new ILInstruction(ILOpCode.ADD_R32));
					break;
				case ASTType.R8:
					tr.Instructions.Add(new ILInstruction(ILOpCode.ADD_R64));
					break;
				default:
					throw new NotSupportedException();
			}
			tr.PopOperand(instr.Operand1);
		}
	}

	public class SubHandler : ITranslationHandler {
		public IROpCode IRCode {
			get { return IROpCode.SUB; }
		}

		public void Translate(IRInstruction instr, ILTranslator tr) {
			tr.PushOperand(instr.Operand1);
			tr.PushOperand(instr.Operand2);
			var type = TypeInference.InferBinaryOp(instr.Operand1.Type, instr.Operand2.Type);
			switch (type) {
				case ASTType.R4:
					tr.Instructions.Add(new ILInstruction(ILOpCode.SUB_R32));
					break;
				case ASTType.R8:
					tr.Instructions.Add(new ILInstruction(ILOpCode.SUB_R64));
					break;
				default:
					throw new NotSupportedException();
			}
			tr.PopOperand(instr.Operand1);
		}
	}

	public class MulHandler : ITranslationHandler {
		public IROpCode IRCode {
			get { return IROpCode.MUL; }
		}

		public void Translate(IRInstruction instr, ILTranslator tr) {
			tr.PushOperand(instr.Operand1);
			tr.PushOperand(instr.Operand2);
			var type = TypeInference.InferBinaryOp(instr.Operand1.Type, instr.Operand2.Type);
			switch (type) {
				case ASTType.I4:
					tr.Instructions.Add(new ILInstruction(ILOpCode.MUL_DWORD));
					break;
				case ASTType.I8:
				case ASTType.Ptr:
				case ASTType.ByRef:
					tr.Instructions.Add(new ILInstruction(ILOpCode.MUL_QWORD));
					break;
				case ASTType.R4:
					tr.Instructions.Add(new ILInstruction(ILOpCode.MUL_R32));
					break;
				case ASTType.R8:
					tr.Instructions.Add(new ILInstruction(ILOpCode.MUL_R64));
					break;
				default:
					throw new NotSupportedException();
			}
			tr.PopOperand(instr.Operand1);
		}
	}

	public class DivHandler : ITranslationHandler {
		public IROpCode IRCode {
			get { return IROpCode.DIV; }
		}

		public void Translate(IRInstruction instr, ILTranslator tr) {
			tr.PushOperand(instr.Operand1);
			tr.PushOperand(instr.Operand2);
			var type = TypeInference.InferBinaryOp(instr.Operand1.Type, instr.Operand2.Type);
			switch (type) {
				case ASTType.I4:
					tr.Instructions.Add(new ILInstruction(ILOpCode.DIV_DWORD));
					break;
				case ASTType.I8:
				case ASTType.Ptr:
				case ASTType.ByRef:
					tr.Instructions.Add(new ILInstruction(ILOpCode.DIV_QWORD));
					break;
				case ASTType.R4:
					tr.Instructions.Add(new ILInstruction(ILOpCode.DIV_R32));
					break;
				case ASTType.R8:
					tr.Instructions.Add(new ILInstruction(ILOpCode.DIV_R64));
					break;
				default:
					throw new NotSupportedException();
			}
			tr.PopOperand(instr.Operand1);
		}
	}

	public class RemHandler : ITranslationHandler {
		public IROpCode IRCode {
			get { return IROpCode.REM; }
		}

		public void Translate(IRInstruction instr, ILTranslator tr) {
			tr.PushOperand(instr.Operand1);
			tr.PushOperand(instr.Operand2);
			var type = TypeInference.InferBinaryOp(instr.Operand1.Type, instr.Operand2.Type);
			switch (type) {
				case ASTType.I4:
					tr.Instructions.Add(new ILInstruction(ILOpCode.REM_DWORD));
					break;
				case ASTType.I8:
				case ASTType.Ptr:
					tr.Instructions.Add(new ILInstruction(ILOpCode.REM_QWORD));
					break;
				case ASTType.R4:
					tr.Instructions.Add(new ILInstruction(ILOpCode.REM_R32));
					break;
				case ASTType.R8:
					tr.Instructions.Add(new ILInstruction(ILOpCode.REM_R64));
					break;
				default:
					throw new NotSupportedException();
			}
			tr.PopOperand(instr.Operand1);
		}
	}

	public class SxHandler : ITranslationHandler {
		public IROpCode IRCode {
			get { return IROpCode.SX; }
		}

		public void Translate(IRInstruction instr, ILTranslator tr) {
			tr.PushOperand(instr.Operand2);
			switch (instr.Operand1.Type) {
				case ASTType.I4:
					if (instr.Operand1 is IRVariable) {
						var rawType = ((IRVariable)instr.Operand1).RawType.ElementType;
						if (rawType == ElementType.I2)
							tr.Instructions.Add(new ILInstruction(ILOpCode.SX_WORD));
					}
					tr.Instructions.Add(new ILInstruction(ILOpCode.SX_BYTE));
					break;
				case ASTType.I8:
					tr.Instructions.Add(new ILInstruction(ILOpCode.SX_DWORD));
					break;
				default:
					throw new NotSupportedException();
			}
			tr.PopOperand(instr.Operand1);
		}
	}

	public class FConvHandler : ITranslationHandler {
		public IROpCode IRCode {
			get { return IROpCode.FCONV; }
		}

		public void Translate(IRInstruction instr, ILTranslator tr) {
			tr.PushOperand(instr.Operand2);
			switch (instr.Operand2.Type) {
				case ASTType.R4:
					Debug.Assert(instr.Operand1.Type == ASTType.R8);
					tr.Instructions.Add(new ILInstruction(ILOpCode.FCONV_R32_R64));
					break;

				case ASTType.R8:
					Debug.Assert(instr.Operand1.Type == ASTType.R4);
					tr.Instructions.Add(new ILInstruction(ILOpCode.FCONV_R64_R32));
					break;

				default:
					Debug.Assert(instr.Operand2.Type == ASTType.I8);
					switch (instr.Operand1.Type) {
						case ASTType.R4:
							tr.Instructions.Add(new ILInstruction(ILOpCode.FCONV_R32));
							break;
						case ASTType.R8:
							tr.Instructions.Add(new ILInstruction(ILOpCode.FCONV_R64));
							break;
						default:
							throw new NotSupportedException();
					}
					break;
			}
			tr.PopOperand(instr.Operand1);
		}
	}

	public class IConvHandler : ITranslationHandler {
		public IROpCode IRCode {
			get { return IROpCode.ICONV; }
		}

		public void Translate(IRInstruction instr, ILTranslator tr) {
			tr.PushOperand(instr.Operand2);
			Debug.Assert(instr.Operand1.Type == ASTType.I8);
			switch (instr.Operand2.Type) {
				case ASTType.R4:
					tr.Instructions.Add(new ILInstruction(ILOpCode.FCONV_R32_R64));
					tr.Instructions.Add(new ILInstruction(ILOpCode.ICONV_R64));
					break;
				case ASTType.R8:
					tr.Instructions.Add(new ILInstruction(ILOpCode.ICONV_R64));
					break;
				case ASTType.Ptr:
					tr.Instructions.Add(new ILInstruction(ILOpCode.ICONV_PTR));
					break;
				default:
					throw new NotSupportedException();
			}
			tr.PopOperand(instr.Operand1);
		}
	}
}