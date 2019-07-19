using System;

namespace KoiVM.AST.ILAST {
	public class ILASTAssignment : ASTNode, IILASTStatement {
		public ILASTVariable Variable { get; set; }
		public ILASTExpression Value { get; set; }

		public override string ToString() {
			return string.Format("{0} = {1}", Variable, Value);
		}
	}
}