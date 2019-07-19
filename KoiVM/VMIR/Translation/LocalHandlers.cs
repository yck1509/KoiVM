using System;
using System.Diagnostics;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using KoiVM.AST;
using KoiVM.AST.ILAST;
using KoiVM.AST.IR;

namespace KoiVM.VMIR.Translation {
	public class LdlocHandler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Ldloc; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			var local = tr.Context.ResolveLocal((Local)expr.Operand);
			var ret = tr.Context.AllocateVRegister(local.Type);
			tr.Instructions.Add(new IRInstruction(IROpCode.MOV, ret, local));

			if (local.RawType.ElementType == ElementType.I1 ||
			    local.RawType.ElementType == ElementType.I2) {
				ret.RawType = local.RawType;
				var r = tr.Context.AllocateVRegister(local.Type);
				tr.Instructions.Add(new IRInstruction(IROpCode.SX, r, ret));
				ret = r;
			}
			return ret;
		}
	}

	public class StlocHandler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Stloc; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			Debug.Assert(expr.Arguments.Length == 1);
			tr.Instructions.Add(new IRInstruction(IROpCode.MOV) {
				Operand1 = tr.Context.ResolveLocal((Local)expr.Operand),
				Operand2 = tr.Translate(expr.Arguments[0])
			});
			return null;
		}
	}

	public class LdlocaHandler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Ldloca; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			var local = tr.Context.ResolveLocal((Local)expr.Operand);
			var ret = tr.Context.AllocateVRegister(ASTType.ByRef);
			tr.Instructions.Add(new IRInstruction(IROpCode.__LEA, ret, local));
			return ret;
		}
	}
}