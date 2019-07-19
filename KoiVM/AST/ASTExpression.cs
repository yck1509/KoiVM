using System;

namespace KoiVM.AST {
	public abstract class ASTExpression : ASTNode {
		public ASTType? Type { get; set; }
	}
}