using System;

namespace KoiVM.AST {
	public class ASTVariable {
		public ASTType Type { get; set; }
		public string Name { get; set; }

		public override string ToString() {
			return Name;
		}
	}
}