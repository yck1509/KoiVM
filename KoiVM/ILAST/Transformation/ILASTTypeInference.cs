using System;
using System.Diagnostics;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using KoiVM.AST;
using KoiVM.AST.ILAST;

namespace KoiVM.ILAST.Transformation {
	public class ILASTTypeInference : ITransformationHandler {
		public void Initialize(ILASTTransformer tr) {
		}

		public void Transform(ILASTTransformer tr) {
			foreach (var st in tr.Tree) {
				if (st is ILASTExpression)
					ProcessExpression((ILASTExpression)st);
				else if (st is ILASTAssignment) {
					var assignment = (ILASTAssignment)st;
					assignment.Variable.Type = ProcessExpression(assignment.Value).Value;
				}
				else if (st is ILASTPhi) {
					ProcessPhiNode((ILASTPhi)st);
				}
			}
		}

		void ProcessPhiNode(ILASTPhi phi) {
			// TODO: Check all source variables having same type?
			phi.Variable.Type = phi.SourceVariables[0].Type;
		}

		ASTType? ProcessExpression(ILASTExpression expr) {
			foreach (var arg in expr.Arguments) {
				if (arg is ILASTExpression) {
					var argExpr = (ILASTExpression)arg;
					argExpr.Type = ProcessExpression(argExpr).Value;
				}
			}
			var exprType = InferType(expr);
			if (exprType != null)
				expr.Type = exprType.Value;
			return exprType;
		}

		static ASTType? InferType(ILASTExpression expr) {
			if (expr.Type != null)
				return expr.Type;

			var opCode = expr.ILCode.ToOpCode();
			switch (opCode.StackBehaviourPush) {
				case StackBehaviour.Push1:
					return InferPush1(expr);
				case StackBehaviour.Pushi:
					return InferPushI(expr);
				case StackBehaviour.Pushi8:
					return InferPushI8(expr);
				case StackBehaviour.Pushr4:
					return InferPushR4(expr);
				case StackBehaviour.Pushr8:
					return InferPushR8(expr);
				case StackBehaviour.Pushref:
					return InferPushRef(expr);
				case StackBehaviour.Varpush:
					return InferVarPush(expr);

				case StackBehaviour.Push1_push1:
					Debug.Assert(expr.Arguments.Length == 1);
					return expr.Arguments[0].Type;

				case StackBehaviour.Push0:
				default:
					return null;
			}
		}

		static ASTType? InferPush1(ILASTExpression expr) {
			switch (expr.ILCode) {
				case Code.Add:
				case Code.Add_Ovf:
				case Code.Add_Ovf_Un:
				case Code.Sub:
				case Code.Sub_Ovf:
				case Code.Sub_Ovf_Un:
				case Code.Mul:
				case Code.Mul_Ovf:
				case Code.Mul_Ovf_Un:
				case Code.Div:
				case Code.Div_Un:
				case Code.Rem:
				case Code.Rem_Un:
					Debug.Assert(expr.Arguments.Length == 2);
					Debug.Assert(expr.Arguments[0].Type != null && expr.Arguments[1].Type != null);
					return TypeInference.InferBinaryOp(expr.Arguments[0].Type.Value, expr.Arguments[1].Type.Value);

				case Code.Xor:
				case Code.And:
				case Code.Or:
					Debug.Assert(expr.Arguments.Length == 2);
					Debug.Assert(expr.Arguments[0].Type != null && expr.Arguments[1].Type != null);
					return TypeInference.InferIntegerOp(expr.Arguments[0].Type.Value, expr.Arguments[1].Type.Value);

				case Code.Not:
					Debug.Assert(expr.Arguments.Length == 1 && expr.Arguments[0].Type != null);
					if (expr.Arguments[0].Type != ASTType.I4 &&
					    expr.Arguments[0].Type != ASTType.I8 &&
					    expr.Arguments[0].Type != ASTType.Ptr)
						throw new ArgumentException("Invalid Not Operand Types.");
					return expr.Arguments[0].Type;

				case Code.Neg:
					Debug.Assert(expr.Arguments.Length == 1 && expr.Arguments[0].Type != null);
					if (expr.Arguments[0].Type != ASTType.I4 &&
					    expr.Arguments[0].Type != ASTType.I8 &&
					    expr.Arguments[0].Type != ASTType.R4 &&
					    expr.Arguments[0].Type != ASTType.R8 &&
					    expr.Arguments[0].Type != ASTType.Ptr)
						throw new ArgumentException("Invalid Not Operand Types.");
					return expr.Arguments[0].Type;

				case Code.Shr:
				case Code.Shl:
				case Code.Shr_Un:
					Debug.Assert(expr.Arguments.Length == 2);
					Debug.Assert(expr.Arguments[0].Type != null && expr.Arguments[1].Type != null);
					return TypeInference.InferShiftOp(expr.Arguments[0].Type.Value, expr.Arguments[1].Type.Value);

				case Code.Mkrefany:
					return ASTType.O;

				case Code.Ldarg:
					return TypeInference.ToASTType(((Parameter)expr.Operand).Type);
				case Code.Ldloc:
					return TypeInference.ToASTType(((Local)expr.Operand).Type);

				case Code.Unbox_Any:
				case Code.Ldelem:
				case Code.Ldobj:
					return TypeInference.ToASTType(((ITypeDefOrRef)expr.Operand).ToTypeSig());

				case Code.Ldfld:
				case Code.Ldsfld:
					return TypeInference.ToASTType(((IField)expr.Operand).FieldSig.Type);

				default:
					throw new NotSupportedException(expr.ILCode.ToString());
			}
		}

		static ASTType? InferPushI(ILASTExpression expr) {
			switch (expr.ILCode) {
				case Code.Ldftn:
				case Code.Ldind_I:
				case Code.Ldelem_I:
				case Code.Ldvirtftn:
				case Code.Localloc:
				case Code.Conv_U:
				case Code.Conv_Ovf_U:
				case Code.Conv_Ovf_U_Un:
				case Code.Conv_I:
				case Code.Conv_Ovf_I:
				case Code.Conv_Ovf_I_Un:
					return ASTType.Ptr;

				case Code.Ldarga:
				case Code.Ldelema:
				case Code.Ldflda:
				case Code.Ldloca:
				case Code.Ldsflda:
					return ASTType.ByRef;

				case Code.Ldtoken:
				case Code.Arglist:
				case Code.Unbox:
				case Code.Refanytype:
				case Code.Refanyval:
				case Code.Isinst:
					return ASTType.O;

				default:
					return ASTType.I4;
			}
		}

		static ASTType? InferPushI8(ILASTExpression expr) {
			return ASTType.I8;
		}

		static ASTType? InferPushR4(ILASTExpression expr) {
			return ASTType.R4;
		}

		static ASTType? InferPushR8(ILASTExpression expr) {
			return ASTType.R8;
		}

		static ASTType? InferPushRef(ILASTExpression expr) {
			return ASTType.O;
		}

		static ASTType? InferVarPush(ILASTExpression expr) {
			var method = (IMethod)expr.Operand;
			if (method.MethodSig.RetType.ElementType == ElementType.Void)
				return null;

			var genArgs = new GenericArguments();
			if (method is MethodSpec)
				genArgs.PushMethodArgs(((MethodSpec)method).GenericInstMethodSig.GenericArguments);
			if (method.DeclaringType.TryGetGenericInstSig() != null)
				genArgs.PushTypeArgs(method.DeclaringType.TryGetGenericInstSig().GenericArguments);


			return TypeInference.ToASTType(genArgs.ResolveType(method.MethodSig.RetType));
		}
	}
}