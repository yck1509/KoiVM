using System;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using KoiVM.AST.ILAST;

namespace KoiVM.ILAST.Transformation {
	public class StringTransform : ITransformationHandler {
		public void Initialize(ILASTTransformer tr) {
		}

		public void Transform(ILASTTransformer tr) {
			tr.Tree.TraverseTree(Transform, tr);
		}

		static void Transform(ILASTExpression expr, ILASTTransformer tr) {
			if (expr.ILCode != Code.Ldstr)
				return;

			var operand = (string)expr.Operand;
			expr.ILCode = Code.Box;
			expr.Operand = tr.Method.Module.CorLibTypes.String.ToTypeDefOrRef();
			expr.Arguments = new IILASTNode[] {
				new ILASTExpression {
					ILCode = Code.Ldc_I4,
					Operand = (int)tr.VM.Data.GetId(operand),
					Arguments = new IILASTNode[0]
				}
			};
		}
	}
}