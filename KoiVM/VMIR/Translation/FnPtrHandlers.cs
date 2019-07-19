using System;
using System.Diagnostics;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using KoiVM.AST.ILAST;
using KoiVM.AST.IR;

namespace KoiVM.VMIR.Translation {
	public class LdftnHandler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Ldftn; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			var retVar = tr.Context.AllocateVRegister(expr.Type.Value);

			var method = ((IMethod)expr.Operand).ResolveMethodDef();

			bool intraLinking = method != null && tr.VM.Settings.IsVirtualized(method);
			var ecallId = tr.VM.Runtime.VMCall.LDFTN;
			if (intraLinking) {
				var sigId = (int)tr.VM.Data.GetId(method.DeclaringType, method.MethodSig);
				uint entryKey = tr.VM.Data.LookupInfo(method).EntryKey;
				entryKey = ((uint)tr.VM.Random.Next() & 0xffffff00) | entryKey;
				tr.Instructions.Add(new IRInstruction(IROpCode.PUSH, IRConstant.FromI4((int)entryKey)));
				tr.Instructions.Add(new IRInstruction(IROpCode.PUSH, IRConstant.FromI4(sigId)));
				tr.Instructions.Add(new IRInstruction(IROpCode.PUSH, new IRMetaTarget(method) { LateResolve = true }));
				tr.Instructions.Add(new IRInstruction(IROpCode.VCALL, IRConstant.FromI4(ecallId)));
			}
			else {
				var methodId = (int)tr.VM.Data.GetId((IMethod)expr.Operand);
				tr.Instructions.Add(new IRInstruction(IROpCode.PUSH, IRConstant.FromI4(0)));
				tr.Instructions.Add(new IRInstruction(IROpCode.VCALL, IRConstant.FromI4(ecallId), IRConstant.FromI4(methodId)));
			}
			tr.Instructions.Add(new IRInstruction(IROpCode.POP, retVar));
			return retVar;
		}
	}

	public class LdvirtftnHandler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Ldvirtftn; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			Debug.Assert(expr.Arguments.Length == 1);
			var retVar = tr.Context.AllocateVRegister(expr.Type.Value);
			var obj = tr.Translate(expr.Arguments[0]);

			var method = (IMethod)expr.Operand;
			var methodId = (int)tr.VM.Data.GetId(method);

			var ecallId = tr.VM.Runtime.VMCall.LDFTN;
			tr.Instructions.Add(new IRInstruction(IROpCode.PUSH, obj));
			tr.Instructions.Add(new IRInstruction(IROpCode.VCALL, IRConstant.FromI4(ecallId), IRConstant.FromI4(methodId)));
			tr.Instructions.Add(new IRInstruction(IROpCode.POP, retVar));
			return retVar;
		}
	}
}