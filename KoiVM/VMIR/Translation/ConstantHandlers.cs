using System;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using KoiVM.AST.ILAST;
using KoiVM.AST.IR;

namespace KoiVM.VMIR.Translation {
	public class LdcI4Handler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Ldc_I4; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			return IRConstant.FromI4((int)expr.Operand);
		}
	}

	public class LdcI8Handler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Ldc_I8; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			return IRConstant.FromI8((long)expr.Operand);
		}
	}

	public class LdcR4Handler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Ldc_R4; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			return IRConstant.FromR4((float)expr.Operand);
		}
	}

	public class LdcR8Handler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Ldc_R8; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			return IRConstant.FromR8((double)expr.Operand);
		}
	}

	public class LdnullHandler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Ldnull; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			return IRConstant.Null();
		}
	}

	public class LdstrHandler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Ldstr; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			return IRConstant.FromString((string)expr.Operand);
		}
	}

	public class LdtokenHandler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Ldtoken; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			var retVar = tr.Context.AllocateVRegister(expr.Type.Value);
			var refId = (int)tr.VM.Data.GetId((IMemberRef)expr.Operand);
			var ecallId = tr.VM.Runtime.VMCall.TOKEN;
			tr.Instructions.Add(new IRInstruction(IROpCode.VCALL, IRConstant.FromI4(ecallId), IRConstant.FromI4(refId)));
			tr.Instructions.Add(new IRInstruction(IROpCode.POP, retVar));

			return retVar;
		}
	}
}