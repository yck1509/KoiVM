using System;

namespace KoiVM.AST.IR {
	public interface IIROperand {
		ASTType Type { get; }
	}
}