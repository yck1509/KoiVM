using System;
using System.Reflection;

namespace KoiVM.AST {
	[Obfuscation(Exclude = false, ApplyToMembers = false, Feature = "+rename(forceRen=true);")]
	public enum ASTType {
		I4,
		I8,
		R4,
		R8,
		O,
		Ptr,
		ByRef
	}
}