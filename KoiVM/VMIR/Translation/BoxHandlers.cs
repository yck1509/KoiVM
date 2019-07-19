using System;
using System.Diagnostics;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using KoiVM.AST.ILAST;
using KoiVM.AST.IR;

namespace KoiVM.VMIR.Translation {
	public class BoxHandler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Box; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			Debug.Assert(expr.Arguments.Length == 1);
			var value = tr.Translate(expr.Arguments[0]);

			var targetType = ((ITypeDefOrRef)expr.Operand).ToTypeSig();
			var boxType = ((ITypeDefOrRef)expr.Operand).ResolveTypeDef();
			if (!targetType.GetElementType().IsPrimitive() && (boxType == null || !boxType.IsEnum)) {
				// Non-primitive types => already boxed in VM
				if (targetType.ElementType != ElementType.String) // Box is used to resolve string ID
					return value;
			}

			var retVar = tr.Context.AllocateVRegister(expr.Type.Value);
			var typeId = (int)tr.VM.Data.GetId((ITypeDefOrRef)expr.Operand);
			var ecallId = tr.VM.Runtime.VMCall.BOX;
			tr.Instructions.Add(new IRInstruction(IROpCode.PUSH, value));
			tr.Instructions.Add(new IRInstruction(IROpCode.VCALL, IRConstant.FromI4(ecallId), IRConstant.FromI4(typeId)));
			tr.Instructions.Add(new IRInstruction(IROpCode.POP, retVar));

			return retVar;
		}
	}

	public class UnboxAnyHandler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Unbox_Any; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			Debug.Assert(expr.Arguments.Length == 1);
			var value = tr.Translate(expr.Arguments[0]);

			var targetType = ((ITypeDefOrRef)expr.Operand).ToTypeSig();
			if (!targetType.GetElementType().IsPrimitive() &&
			    targetType.ElementType != ElementType.Object &&
			    !targetType.ToTypeDefOrRef().ResolveTypeDefThrow().IsEnum) {
				// Non-primitive types => already boxed in VM
				return value;
			}

			var retVar = tr.Context.AllocateVRegister(expr.Type.Value);
			var typeId = (int)tr.VM.Data.GetId((ITypeDefOrRef)expr.Operand);
			var ecallId = tr.VM.Runtime.VMCall.UNBOX;
			tr.Instructions.Add(new IRInstruction(IROpCode.PUSH, value));
			tr.Instructions.Add(new IRInstruction(IROpCode.VCALL, IRConstant.FromI4(ecallId), IRConstant.FromI4(typeId)));
			tr.Instructions.Add(new IRInstruction(IROpCode.POP, retVar));

			return retVar;
		}
	}

	public class UnboxHandler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Unbox; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			Debug.Assert(expr.Arguments.Length == 1);
			var value = tr.Translate(expr.Arguments[0]);

			var targetType = ((ITypeDefOrRef)expr.Operand).ToTypeSig();

			var retVar = tr.Context.AllocateVRegister(expr.Type.Value);
			var typeId = (int)(tr.VM.Data.GetId((ITypeDefOrRef)expr.Operand) | 0x80000000);
			var ecallId = tr.VM.Runtime.VMCall.UNBOX;
			tr.Instructions.Add(new IRInstruction(IROpCode.PUSH, value));
			tr.Instructions.Add(new IRInstruction(IROpCode.VCALL, IRConstant.FromI4(ecallId), IRConstant.FromI4(typeId)));
			tr.Instructions.Add(new IRInstruction(IROpCode.POP, retVar));

			return retVar;
		}
	}
}