using System;

namespace KoiVM.AST.ILAST {
	public class ILASTVariable : ASTVariable, IILASTNode {
		ASTType? IILASTNode.Type {
			get { return base.Type; }
		}

		public ILASTVariableType VariableType { get; set; }
		public object Annotation { get; set; }
	}
}