using System;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using KoiVM.AST;
using KoiVM.AST.ILAST;

namespace KoiVM.ILAST.Transformation {
	public class ArrayTransform : ITransformationHandler {
		public void Initialize(ILASTTransformer tr) {
		}

		public void Transform(ILASTTransformer tr) {
			var module = tr.Method.Module;
			tr.Tree.TraverseTree(Transform, module);

			for (int i = 0; i < tr.Tree.Count; i++) {
				var st = tr.Tree[i];
				var expr = VariableInlining.GetExpression(st);
				if (expr == null)
					continue;

				switch (expr.ILCode) {
					case Code.Stelem:
						TransformSTELEM(expr, module, (ITypeDefOrRef)expr.Operand, tr.Tree, ref i);
						break;
					case Code.Stelem_I1:
						TransformSTELEM(expr, module, module.CorLibTypes.SByte.ToTypeDefOrRef(), tr.Tree, ref i);
						break;
					case Code.Stelem_I2:
						TransformSTELEM(expr, module, module.CorLibTypes.Int16.ToTypeDefOrRef(), tr.Tree, ref i);
						break;
					case Code.Stelem_I4:
						TransformSTELEM(expr, module, module.CorLibTypes.Int32.ToTypeDefOrRef(), tr.Tree, ref i);
						break;
					case Code.Stelem_I8:
						TransformSTELEM(expr, module, module.CorLibTypes.Int64.ToTypeDefOrRef(), tr.Tree, ref i);
						break;
					case Code.Stelem_R4:
						TransformSTELEM(expr, module, module.CorLibTypes.Single.ToTypeDefOrRef(), tr.Tree, ref i);
						break;
					case Code.Stelem_R8:
						TransformSTELEM(expr, module, module.CorLibTypes.Double.ToTypeDefOrRef(), tr.Tree, ref i);
						break;
					case Code.Stelem_I:
						TransformSTELEM(expr, module, module.CorLibTypes.IntPtr.ToTypeDefOrRef(), tr.Tree, ref i);
						break;
					case Code.Stelem_Ref:
						TransformSTELEM(expr, module, module.CorLibTypes.Object.ToTypeDefOrRef(), tr.Tree, ref i);
						break;
				}
			}
		}

		static void Transform(ILASTExpression expr, ModuleDef module) {
			switch (expr.ILCode) {
				case Code.Ldlen: {
					expr.ILCode = Code.Call;
					var array = module.CorLibTypes.GetTypeRef("System", "Array");
					var lenSig = MethodSig.CreateInstance(module.CorLibTypes.Int32);
					var methodRef = new MemberRefUser(module, "get_Length", lenSig, array);
					expr.Operand = methodRef;
					break;
				}
				case Code.Newarr: {
					expr.ILCode = Code.Newobj;
					var array = new SZArraySig(((ITypeDefOrRef)expr.Operand).ToTypeSig()).ToTypeDefOrRef();
					var ctorSig = MethodSig.CreateInstance(module.CorLibTypes.Void, module.CorLibTypes.Int32);
					var ctorRef = new MemberRefUser(module, ".ctor", ctorSig, array);
					expr.Operand = ctorRef;
					break;
				}
				case Code.Ldelema: {
					expr.ILCode = Code.Call;
					var elemType = ((ITypeDefOrRef)expr.Operand).ToTypeSig();
					var array = new SZArraySig(elemType).ToTypeDefOrRef();
					var addrSig = MethodSig.CreateInstance(new ByRefSig(elemType), module.CorLibTypes.Int32);
					var addrRef = new MemberRefUser(module, "Address", addrSig, array);
					expr.Operand = addrRef;
					break;
				}
				case Code.Ldelem:
					TransformLDELEM(expr, module, (ITypeDefOrRef)expr.Operand);
					break;
				case Code.Ldelem_I1:
					TransformLDELEM(expr, module, module.CorLibTypes.SByte.ToTypeDefOrRef());
					break;
				case Code.Ldelem_U1:
					TransformLDELEM(expr, module, module.CorLibTypes.Byte.ToTypeDefOrRef());
					break;
				case Code.Ldelem_I2:
					TransformLDELEM(expr, module, module.CorLibTypes.Int16.ToTypeDefOrRef());
					break;
				case Code.Ldelem_U2:
					TransformLDELEM(expr, module, module.CorLibTypes.UInt16.ToTypeDefOrRef());
					break;
				case Code.Ldelem_I4:
					TransformLDELEM(expr, module, module.CorLibTypes.Int32.ToTypeDefOrRef());
					break;
				case Code.Ldelem_U4:
					TransformLDELEM(expr, module, module.CorLibTypes.UInt32.ToTypeDefOrRef());
					break;
				case Code.Ldelem_I8:
					TransformLDELEM(expr, module, module.CorLibTypes.Int64.ToTypeDefOrRef());
					break;
				case Code.Ldelem_R4:
					TransformLDELEM(expr, module, module.CorLibTypes.Single.ToTypeDefOrRef());
					break;
				case Code.Ldelem_R8:
					TransformLDELEM(expr, module, module.CorLibTypes.Double.ToTypeDefOrRef());
					break;
				case Code.Ldelem_I:
					TransformLDELEM(expr, module, module.CorLibTypes.IntPtr.ToTypeDefOrRef());
					break;
				case Code.Ldelem_Ref:
					TransformLDELEM(expr, module, module.CorLibTypes.Object.ToTypeDefOrRef());
					break;
			}
		}

		static void TransformLDELEM(ILASTExpression expr, ModuleDef module, ITypeDefOrRef type) {
			var array = module.CorLibTypes.GetTypeRef("System", "Array");
			var getValSig = MethodSig.CreateInstance(module.CorLibTypes.Object, module.CorLibTypes.Int32);
			var getValRef = new MemberRefUser(module, "GetValue", getValSig, array);

			var getValue = new ILASTExpression {
				ILCode = Code.Call,
				Operand = getValRef,
				Arguments = expr.Arguments
			};
			expr.ILCode = Code.Unbox_Any;
			expr.Operand = type.IsValueType ? module.CorLibTypes.Object.ToTypeDefOrRef() : type;
			expr.Type = TypeInference.ToASTType(type.ToTypeSig());
			expr.Arguments = new IILASTNode[] { getValue };
		}

		static void TransformSTELEM(ILASTExpression expr, ModuleDef module, ITypeDefOrRef type, ILASTTree tree, ref int index) {
			var array = module.CorLibTypes.GetTypeRef("System", "Array");
			var setValSig = MethodSig.CreateInstance(module.CorLibTypes.Void, module.CorLibTypes.Object, module.CorLibTypes.Int32);
			var setValRef = new MemberRefUser(module, "SetValue", setValSig, array);

			ILASTVariable tmpVar1, tmpVar2;
			if (expr.Arguments[1] is ILASTVariable) {
				tmpVar1 = (ILASTVariable)expr.Arguments[1];
			}
			else {
				tmpVar1 = new ILASTVariable {
					Name = string.Format("arr_{0:x4}_1", expr.CILInstr.Offset),
					VariableType = ILASTVariableType.StackVar
				};
				tree.Insert(index++, new ILASTAssignment {
					Variable = tmpVar1,
					Value = (ILASTExpression)expr.Arguments[1]
				});
			}
			if (expr.Arguments[2] is ILASTVariable) {
				tmpVar2 = (ILASTVariable)expr.Arguments[2];
			}
			else {
				tmpVar2 = new ILASTVariable {
					Name = string.Format("arr_{0:x4}_2", expr.CILInstr.Offset),
					VariableType = ILASTVariableType.StackVar
				};
				tree.Insert(index++, new ILASTAssignment {
					Variable = tmpVar2,
					Value = (ILASTExpression)expr.Arguments[2]
				});
			}

			if (type.IsPrimitive) {
				var elem = new ILASTExpression {
					ILCode = Code.Box,
					Operand = type,
					Arguments = new[] { tmpVar2 }
				};
				expr.Arguments[2] = tmpVar1;
				expr.Arguments[1] = elem;
			}
			else {
				expr.Arguments[2] = tmpVar1;
				expr.Arguments[1] = tmpVar2;
			}
			expr.ILCode = Code.Call;
			expr.Operand = setValRef;
		}
	}
}