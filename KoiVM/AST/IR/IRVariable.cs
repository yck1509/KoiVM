using System;
using dnlib.DotNet;

namespace KoiVM.AST.IR {
	public class IRVariable : ASTVariable, IIROperand {
		public IRVariableType VariableType { get; set; }
		public TypeSig RawType { get; set; }
		public int Id { get; set; }
		public object Annotation { get; set; }

		public override string ToString() {
			return string.Format("{0}:{1}", Name, Type);
		}
	}
}