using System;

namespace KoiVM.AST.IR {
	public enum IRVariableType {
		VirtualRegister,
		Local,
		Argument,
		ExceptionObj
	}
}