using System;
using System.Diagnostics;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using KoiVM.AST.ILAST;
using KoiVM.AST.IR;

namespace KoiVM.VMIR.Translation {
	public class LdfldHandler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Ldfld; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			Debug.Assert(expr.Arguments.Length == 1);
			var obj = tr.Translate(expr.Arguments[0]);

			var retVar = tr.Context.AllocateVRegister(expr.Type.Value);
			var fieldId = (int)tr.VM.Data.GetId((IField)expr.Operand);
			var ecallId = tr.VM.Runtime.VMCall.LDFLD;
			tr.Instructions.Add(new IRInstruction(IROpCode.PUSH, obj));
			tr.Instructions.Add(new IRInstruction(IROpCode.VCALL, IRConstant.FromI4(ecallId), IRConstant.FromI4(fieldId)));
			tr.Instructions.Add(new IRInstruction(IROpCode.POP, retVar));
			return retVar;
		}
	}

	public class StfldHandler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Stfld; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			Debug.Assert(expr.Arguments.Length == 2);
			var obj = tr.Translate(expr.Arguments[0]);
			var val = tr.Translate(expr.Arguments[1]);

			var fieldId = (int)tr.VM.Data.GetId((IField)expr.Operand);
			var ecallId = tr.VM.Runtime.VMCall.STFLD;
			tr.Instructions.Add(new IRInstruction(IROpCode.PUSH, obj));
			tr.Instructions.Add(new IRInstruction(IROpCode.PUSH, val));
			tr.Instructions.Add(new IRInstruction(IROpCode.VCALL, IRConstant.FromI4(ecallId), IRConstant.FromI4(fieldId)));
			return null;
		}
	}

	public class LdsfldHandler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Ldsfld; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			var retVar = tr.Context.AllocateVRegister(expr.Type.Value);
			var fieldId = (int)tr.VM.Data.GetId((IField)expr.Operand);
			var ecallId = tr.VM.Runtime.VMCall.LDFLD;
			tr.Instructions.Add(new IRInstruction(IROpCode.PUSH, IRConstant.Null()));
			tr.Instructions.Add(new IRInstruction(IROpCode.VCALL, IRConstant.FromI4(ecallId), IRConstant.FromI4(fieldId)));
			tr.Instructions.Add(new IRInstruction(IROpCode.POP, retVar));
			return retVar;
		}
	}

	public class StsfldHandler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Stsfld; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			Debug.Assert(expr.Arguments.Length == 1);
			var val = tr.Translate(expr.Arguments[0]);

			var fieldId = (int)tr.VM.Data.GetId((IField)expr.Operand);
			var ecallId = tr.VM.Runtime.VMCall.STFLD;
			tr.Instructions.Add(new IRInstruction(IROpCode.PUSH, IRConstant.Null()));
			tr.Instructions.Add(new IRInstruction(IROpCode.PUSH, val));
			tr.Instructions.Add(new IRInstruction(IROpCode.VCALL, IRConstant.FromI4(ecallId), IRConstant.FromI4(fieldId)));
			return null;
		}
	}

	public class LdfldaHandler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Ldflda; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			Debug.Assert(expr.Arguments.Length == 1);
			var obj = tr.Translate(expr.Arguments[0]);

			var retVar = tr.Context.AllocateVRegister(expr.Type.Value);
			var fieldId = (int)(tr.VM.Data.GetId((IField)expr.Operand) | 0x80000000);
			var ecallId = tr.VM.Runtime.VMCall.LDFLD;
			tr.Instructions.Add(new IRInstruction(IROpCode.PUSH, obj));
			tr.Instructions.Add(new IRInstruction(IROpCode.VCALL, IRConstant.FromI4(ecallId), IRConstant.FromI4(fieldId)));
			tr.Instructions.Add(new IRInstruction(IROpCode.POP, retVar));
			return retVar;
		}
	}

	public class LdsfldaHandler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Ldsflda; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			var retVar = tr.Context.AllocateVRegister(expr.Type.Value);
			var fieldId = (int)(tr.VM.Data.GetId((IField)expr.Operand) | 0x80000000);
			var ecallId = tr.VM.Runtime.VMCall.LDFLD;
			tr.Instructions.Add(new IRInstruction(IROpCode.PUSH, IRConstant.Null()));
			tr.Instructions.Add(new IRInstruction(IROpCode.VCALL, IRConstant.FromI4(ecallId), IRConstant.FromI4(fieldId)));
			tr.Instructions.Add(new IRInstruction(IROpCode.POP, retVar));
			return retVar;
		}
	}
}