using System;
using System.Diagnostics;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using KoiVM.AST;
using KoiVM.AST.ILAST;
using KoiVM.AST.IR;

namespace KoiVM.VMIR.Translation {
	public class LdobjHandler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Ldobj; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			Debug.Assert(expr.Arguments.Length == 1);
			var addr = tr.Translate(expr.Arguments[0]);
			var retVar = tr.Context.AllocateVRegister(expr.Type.Value);
			tr.Instructions.Add(new IRInstruction(IROpCode.__LDOBJ, addr, retVar) {
				Annotation = new PointerInfo("LDOBJ", (ITypeDefOrRef)expr.Operand)
			});

			return retVar;
		}
	}

	public class StobjHandler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Stobj; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			Debug.Assert(expr.Arguments.Length == 2);
			var addr = tr.Translate(expr.Arguments[0]);
			var value = tr.Translate(expr.Arguments[1]);
			tr.Instructions.Add(new IRInstruction(IROpCode.__STOBJ, addr, value) {
				Annotation = new PointerInfo("STOBJ", (ITypeDefOrRef)expr.Operand)
			});

			return null;
		}
	}

	public class InitobjHandler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Initobj; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			Debug.Assert(expr.Arguments.Length == 1);
			var addr = tr.Translate(expr.Arguments[0]);

			var typeId = (int)tr.VM.Data.GetId((ITypeDefOrRef)expr.Operand);
			var ecallId = tr.VM.Runtime.VMCall.INITOBJ;
			tr.Instructions.Add(new IRInstruction(IROpCode.PUSH, addr));
			tr.Instructions.Add(new IRInstruction(IROpCode.VCALL, IRConstant.FromI4(ecallId), IRConstant.FromI4(typeId)));

			return null;
		}
	}

	public class LocallocHandler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Localloc; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			Debug.Assert(expr.Arguments.Length == 1);
			var size = tr.Translate(expr.Arguments[0]);

			var retVar = tr.Context.AllocateVRegister(expr.Type.Value);
			var ecallId = tr.VM.Runtime.VMCall.LOCALLOC;
			tr.Instructions.Add(new IRInstruction(IROpCode.VCALL, IRConstant.FromI4(ecallId), size));
			tr.Instructions.Add(new IRInstruction(IROpCode.POP, retVar));

			return retVar;
		}
	}

	public class SizeofHandler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Sizeof; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			var typeId = (int)tr.Runtime.Descriptor.Data.GetId((ITypeDefOrRef)expr.Operand);
			var retVar = tr.Context.AllocateVRegister(expr.Type.Value);
			var ecallId = tr.VM.Runtime.VMCall.SIZEOF;
			tr.Instructions.Add(new IRInstruction(IROpCode.VCALL, IRConstant.FromI4(ecallId), IRConstant.FromI4(typeId)));
			tr.Instructions.Add(new IRInstruction(IROpCode.POP, retVar));

			return retVar;
		}
	}
}