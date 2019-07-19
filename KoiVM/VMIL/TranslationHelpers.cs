using System;
using dnlib.DotNet;
using KoiVM.AST;
using KoiVM.AST.IL;
using KoiVM.AST.IR;

namespace KoiVM.VMIL {
	public static class TranslationHelpers {
		public static ILOpCode GetLIND(ASTType type, TypeSig rawType) {
			if (rawType != null) {
				switch (rawType.ElementType) {
					case ElementType.I1:
					case ElementType.U1:
					case ElementType.Boolean:
						return ILOpCode.LIND_BYTE;

					case ElementType.I2:
					case ElementType.U2:
					case ElementType.Char:
						return ILOpCode.LIND_WORD;

					case ElementType.I4:
					case ElementType.U4:
					case ElementType.R4:
						return ILOpCode.LIND_DWORD;

					case ElementType.I8:
					case ElementType.U8:
					case ElementType.R8:
						return ILOpCode.LIND_QWORD;

					case ElementType.Ptr:
					case ElementType.I:
					case ElementType.U:
						return ILOpCode.LIND_PTR;

					default:
						return ILOpCode.LIND_OBJECT;
				}
			}
			switch (type) {
				case ASTType.I4:
				case ASTType.R4:
					return ILOpCode.LIND_DWORD;
				case ASTType.I8:
				case ASTType.R8:
					return ILOpCode.LIND_QWORD;
				case ASTType.Ptr:
					return ILOpCode.LIND_PTR;
				default:
					return ILOpCode.LIND_OBJECT;
			}
		}

		public static ILOpCode GetLIND(this IRRegister reg) {
			return GetLIND(reg.Type, reg.SourceVariable == null ? null : reg.SourceVariable.RawType);
		}

		public static ILOpCode GetLIND(this IRPointer ptr) {
			return GetLIND(ptr.Type, ptr.SourceVariable == null ? null : ptr.SourceVariable.RawType);
		}

		public static ILOpCode GetSIND(ASTType type, TypeSig rawType) {
			if (rawType != null) {
				switch (rawType.ElementType) {
					case ElementType.I1:
					case ElementType.U1:
					case ElementType.Boolean:
						return ILOpCode.SIND_BYTE;

					case ElementType.I2:
					case ElementType.U2:
					case ElementType.Char:
						return ILOpCode.SIND_WORD;

					case ElementType.I4:
					case ElementType.U4:
					case ElementType.R4:
						return ILOpCode.SIND_DWORD;

					case ElementType.I8:
					case ElementType.U8:
					case ElementType.R8:
						return ILOpCode.SIND_QWORD;

					case ElementType.Ptr:
					case ElementType.I:
					case ElementType.U:
						return ILOpCode.SIND_PTR;

					default:
						return ILOpCode.SIND_OBJECT;
				}
			}
			switch (type) {
				case ASTType.I4:
				case ASTType.R4:
					return ILOpCode.SIND_DWORD;
				case ASTType.I8:
				case ASTType.R8:
					return ILOpCode.SIND_QWORD;
				case ASTType.Ptr:
					return ILOpCode.SIND_PTR;
				default:
					return ILOpCode.SIND_OBJECT;
			}
		}

		public static ILOpCode GetSIND(this IRRegister reg) {
			return GetSIND(reg.Type, reg.SourceVariable == null ? null : reg.SourceVariable.RawType);
		}

		public static ILOpCode GetSIND(this IRPointer ptr) {
			return GetSIND(ptr.Type, ptr.SourceVariable == null ? null : ptr.SourceVariable.RawType);
		}

		public static ILOpCode GetPUSHR(ASTType type, TypeSig rawType) {
			if (rawType != null) {
				switch (rawType.ElementType) {
					case ElementType.I1:
					case ElementType.U1:
					case ElementType.Boolean:
						return ILOpCode.PUSHR_BYTE;

					case ElementType.I2:
					case ElementType.U2:
					case ElementType.Char:
						return ILOpCode.PUSHR_WORD;

					case ElementType.I4:
					case ElementType.U4:
					case ElementType.R4:
						return ILOpCode.PUSHR_DWORD;

					case ElementType.I8:
					case ElementType.U8:
					case ElementType.R8:
					case ElementType.Ptr:
						return ILOpCode.PUSHR_QWORD;

					default:
						// ldobj won't use it, so only references, no pointers
						return ILOpCode.PUSHR_OBJECT;
				}
			}
			switch (type) {
				case ASTType.I4:
				case ASTType.R4:
					return ILOpCode.PUSHR_DWORD;
				case ASTType.I8:
				case ASTType.R8:
				case ASTType.Ptr:
					return ILOpCode.PUSHR_QWORD;
				default:
					return ILOpCode.PUSHR_OBJECT;
			}
		}

		public static ILOpCode GetPUSHR(this IRRegister reg) {
			return GetPUSHR(reg.Type, reg.SourceVariable == null ? null : reg.SourceVariable.RawType);
		}

		public static ILOpCode GetPUSHR(this IRPointer ptr) {
			return GetPUSHR(ptr.Type, ptr.SourceVariable == null ? null : ptr.SourceVariable.RawType);
		}

		public static ILOpCode GetPUSHI(this ASTType type) {
			switch (type) {
				case ASTType.I4:
				case ASTType.R4:
					return ILOpCode.PUSHI_DWORD;
				case ASTType.I8:
				case ASTType.R8:
				case ASTType.Ptr:
					return ILOpCode.PUSHI_QWORD;
			}
			throw new NotSupportedException();
		}

		public static void PushOperand(this ILTranslator tr, IIROperand operand) {
			if (operand is IRRegister) {
				var reg = ILRegister.LookupRegister(((IRRegister)operand).Register);
				tr.Instructions.Add(new ILInstruction(((IRRegister)operand).GetPUSHR(), reg));
			}
			else if (operand is IRPointer) {
				var pointer = (IRPointer)operand;
				var reg = ILRegister.LookupRegister(pointer.Register.Register);
				tr.Instructions.Add(new ILInstruction(pointer.Register.GetPUSHR(), reg));
				if (pointer.Offset != 0) {
					tr.Instructions.Add(new ILInstruction(ILOpCode.PUSHI_DWORD, ILImmediate.Create(pointer.Offset, ASTType.I4)));
					if (pointer.Register.Type == ASTType.I4)
						tr.Instructions.Add(new ILInstruction(ILOpCode.ADD_DWORD));
					else
						tr.Instructions.Add(new ILInstruction(ILOpCode.ADD_QWORD));
				}
				tr.Instructions.Add(new ILInstruction(pointer.GetLIND()));
			}
			else if (operand is IRConstant) {
				var constant = (IRConstant)operand;
				if (constant.Value == null)
					tr.Instructions.Add(new ILInstruction(ILOpCode.PUSHI_DWORD, ILImmediate.Create(0, ASTType.O)));
				else
					tr.Instructions.Add(new ILInstruction(constant.Type.Value.GetPUSHI(),
						ILImmediate.Create(constant.Value, constant.Type.Value)));
			}
			else if (operand is IRMetaTarget) {
				var method = (MethodDef)((IRMetaTarget)operand).MetadataItem;
				tr.Instructions.Add(new ILInstruction(ILOpCode.PUSHI_DWORD, new ILMethodTarget(method)));
			}
			else if (operand is IRBlockTarget) {
				var target = ((IRBlockTarget)operand).Target;
				tr.Instructions.Add(new ILInstruction(ILOpCode.PUSHI_DWORD, new ILBlockTarget(target)));
			}
			else if (operand is IRJumpTable) {
				var targets = ((IRJumpTable)operand).Targets;
				tr.Instructions.Add(new ILInstruction(ILOpCode.PUSHI_DWORD, new ILJumpTable(targets)));
			}
			else if (operand is IRDataTarget) {
				var target = ((IRDataTarget)operand).Target;
				tr.Instructions.Add(new ILInstruction(ILOpCode.PUSHI_DWORD, new ILDataTarget(target)));
			}
			else
				throw new NotSupportedException();
		}

		public static void PopOperand(this ILTranslator tr, IIROperand operand) {
			if (operand is IRRegister) {
				var reg = ILRegister.LookupRegister(((IRRegister)operand).Register);
				tr.Instructions.Add(new ILInstruction(ILOpCode.POP, reg));
			}
			else if (operand is IRPointer) {
				var pointer = (IRPointer)operand;
				var reg = ILRegister.LookupRegister(pointer.Register.Register);
				tr.Instructions.Add(new ILInstruction(pointer.Register.GetPUSHR(), reg));
				if (pointer.Offset != 0) {
					tr.Instructions.Add(new ILInstruction(ILOpCode.PUSHI_DWORD, ILImmediate.Create(pointer.Offset, ASTType.I4)));
					if (pointer.Register.Type == ASTType.I4)
						tr.Instructions.Add(new ILInstruction(ILOpCode.ADD_DWORD));
					else
						tr.Instructions.Add(new ILInstruction(ILOpCode.ADD_QWORD));
				}
				tr.Instructions.Add(new ILInstruction(pointer.GetSIND()));
			}
			else
				throw new NotSupportedException();
		}
	}
}