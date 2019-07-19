using System;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using KoiVM.AST.ILAST;

namespace KoiVM.ILAST.Transformation {
	public class IndirectTransform : ITransformationHandler {
		public void Initialize(ILASTTransformer tr) {
		}

		public void Transform(ILASTTransformer tr) {
			tr.Tree.TraverseTree(Transform, tr.Method.Module);
		}

		static void Transform(ILASTExpression expr, ModuleDef module) {
			switch (expr.ILCode) {
				case Code.Ldind_I1:
					expr.ILCode = Code.Ldobj;
					expr.Operand = module.CorLibTypes.SByte.ToTypeDefOrRef();

					expr.Arguments = new IILASTNode[] { expr.Clone() };
					expr.ILCode = Code.Conv_I1;
					break;
				case Code.Ldind_U1:
					expr.ILCode = Code.Ldobj;
					expr.Operand = module.CorLibTypes.Byte.ToTypeDefOrRef();
					break;
				case Code.Ldind_I2:
					expr.ILCode = Code.Ldobj;
					expr.Operand = module.CorLibTypes.Int16.ToTypeDefOrRef();

					expr.Arguments = new IILASTNode[] { expr.Clone() };
					expr.ILCode = Code.Conv_I2;
					break;
				case Code.Ldind_U2:
					expr.ILCode = Code.Ldobj;
					expr.Operand = module.CorLibTypes.UInt16.ToTypeDefOrRef();
					break;
				case Code.Ldind_I4:
					expr.ILCode = Code.Ldobj;
					expr.Operand = module.CorLibTypes.Int32.ToTypeDefOrRef();
					break;
				case Code.Ldind_U4:
					expr.ILCode = Code.Ldobj;
					expr.Operand = module.CorLibTypes.UInt32.ToTypeDefOrRef();
					break;
				case Code.Ldind_I8:
					expr.ILCode = Code.Ldobj;
					expr.Operand = module.CorLibTypes.UInt64.ToTypeDefOrRef();
					break;
				case Code.Ldind_R4:
					expr.ILCode = Code.Ldobj;
					expr.Operand = module.CorLibTypes.Single.ToTypeDefOrRef();
					break;
				case Code.Ldind_R8:
					expr.ILCode = Code.Ldobj;
					expr.Operand = module.CorLibTypes.Double.ToTypeDefOrRef();
					break;
				case Code.Ldind_I:
					expr.ILCode = Code.Ldobj;
					expr.Operand = module.CorLibTypes.IntPtr.ToTypeDefOrRef();
					break;
				case Code.Ldind_Ref:
					expr.ILCode = Code.Ldobj;
					expr.Operand = module.CorLibTypes.Object.ToTypeDefOrRef();
					break;

				case Code.Stind_I1:
					expr.ILCode = Code.Stobj;
					expr.Operand = module.CorLibTypes.SByte.ToTypeDefOrRef();
					break;
				case Code.Stind_I2:
					expr.ILCode = Code.Stobj;
					expr.Operand = module.CorLibTypes.Int16.ToTypeDefOrRef();
					break;
				case Code.Stind_I4:
					expr.ILCode = Code.Stobj;
					expr.Operand = module.CorLibTypes.Int32.ToTypeDefOrRef();
					break;
				case Code.Stind_I8:
					expr.ILCode = Code.Stobj;
					expr.Operand = module.CorLibTypes.UInt64.ToTypeDefOrRef();
					break;
				case Code.Stind_R4:
					expr.ILCode = Code.Stobj;
					expr.Operand = module.CorLibTypes.Single.ToTypeDefOrRef();
					break;
				case Code.Stind_R8:
					expr.ILCode = Code.Stobj;
					expr.Operand = module.CorLibTypes.Double.ToTypeDefOrRef();
					break;
				case Code.Stind_I:
					expr.ILCode = Code.Stobj;
					expr.Operand = module.CorLibTypes.IntPtr.ToTypeDefOrRef();
					break;
				case Code.Stind_Ref:
					expr.ILCode = Code.Stobj;
					expr.Operand = module.CorLibTypes.Object.ToTypeDefOrRef();
					break;
			}
		}
	}
}