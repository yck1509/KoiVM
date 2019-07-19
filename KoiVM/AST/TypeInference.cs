using System;
using dnlib.DotNet;

namespace KoiVM.AST {
	public static class TypeInference {
		public static ASTType ToASTType(TypeSig type) {
			switch (type.ElementType) {
				case ElementType.I1:
				case ElementType.I2:
				case ElementType.I4:
				case ElementType.U1:
				case ElementType.U2:
				case ElementType.U4:
				case ElementType.Boolean:
				case ElementType.Char:
					return ASTType.I4;

				case ElementType.I8:
				case ElementType.U8:
					return ASTType.I8;

				case ElementType.R4:
					return ASTType.R4;

				case ElementType.R8:
					return ASTType.R8;

				case ElementType.I:
				case ElementType.U:
				case ElementType.FnPtr:
				case ElementType.Ptr:
					return ASTType.Ptr;

				case ElementType.ByRef:
					return ASTType.ByRef;

				case ElementType.ValueType:
					var typeDef = type.ScopeType.ResolveTypeDef();
					if (typeDef != null && typeDef.IsEnum)
						return ToASTType(typeDef.GetEnumUnderlyingType());
					return ASTType.O;

				default:
					return ASTType.O;
			}
		}

		public static ASTType InferBinaryOp(ASTType a, ASTType b) {
			if (a == b && (a == ASTType.I4 || a == ASTType.I8 || a == ASTType.R4 || a == ASTType.R8))
				return a;
			// Here we sometimes uses I8 for Ptr
			if ((a == ASTType.Ptr && (b == ASTType.I4 || b == ASTType.I8 || b == ASTType.Ptr)) ||
			    (b == ASTType.Ptr && (a == ASTType.I4 || b == ASTType.I4 || a == ASTType.Ptr)))
				return ASTType.Ptr;
			if ((a == ASTType.ByRef && (b == ASTType.I4 || b == ASTType.Ptr)) ||
			    (b == ASTType.ByRef && (a == ASTType.I4 || a == ASTType.Ptr)))
				return ASTType.ByRef;
			if (a == ASTType.ByRef && b == ASTType.ByRef)
				return ASTType.Ptr;
			throw new ArgumentException("Invalid Binary Op Operand Types.");
		}

		public static ASTType InferIntegerOp(ASTType a, ASTType b) {
			if (a == b && (a == ASTType.I4 || a == ASTType.I8 || a == ASTType.R4 || a == ASTType.R8))
				return a;
			// Here we sometimes uses I8 for Ptr
			if ((a == ASTType.Ptr && (b == ASTType.I4 || b == ASTType.I8 || b == ASTType.Ptr)) ||
			    (b == ASTType.Ptr && (a == ASTType.I4 || b == ASTType.I8 || a == ASTType.Ptr)))
				return ASTType.Ptr;
			throw new ArgumentException("Invalid Integer Op Operand Types.");
		}

		public static ASTType InferShiftOp(ASTType a, ASTType b) {
			if ((b == ASTType.Ptr || b == ASTType.I4) &&
			    (a == ASTType.I4 || b == ASTType.I4 || a == ASTType.Ptr))
				return a;
			throw new ArgumentException("Invalid Shift Op Operand Types.");
		}
	}
}