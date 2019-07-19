using System;
using System.Diagnostics;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using KoiVM.AST;
using KoiVM.AST.ILAST;
using KoiVM.AST.IR;

namespace KoiVM.VMIR.Translation {
	public class LdargHandler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Ldarg; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			var param = tr.Context.ResolveParameter((Parameter)expr.Operand);
			var ret = tr.Context.AllocateVRegister(param.Type);
			tr.Instructions.Add(new IRInstruction(IROpCode.MOV, ret, param));

			if (param.RawType.ElementType == ElementType.I1 ||
			    param.RawType.ElementType == ElementType.I2) {
				ret.RawType = param.RawType;
				var r = tr.Context.AllocateVRegister(param.Type);
				tr.Instructions.Add(new IRInstruction(IROpCode.SX, r, ret));
				ret = r;
			}
			return ret;
		}
	}

	public class StargHandler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Starg; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			Debug.Assert(expr.Arguments.Length == 1);
			tr.Instructions.Add(new IRInstruction(IROpCode.MOV) {
				Operand1 = tr.Context.ResolveParameter((Parameter)expr.Operand),
				Operand2 = tr.Translate(expr.Arguments[0])
			});
			return null;
		}
	}

	public class LdargaHandler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Ldarga; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			var param = tr.Context.ResolveParameter((Parameter)expr.Operand);
			var ret = tr.Context.AllocateVRegister(ASTType.ByRef);
			tr.Instructions.Add(new IRInstruction(IROpCode.__LEA, ret, param));
			return ret;
		}
	}
}