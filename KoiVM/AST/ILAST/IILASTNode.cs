using System;

namespace KoiVM.AST.ILAST {
	public interface IILASTNode {
		ASTType? Type { get; }
	}
}