using System;
using System.Text;

namespace KoiVM.AST.ILAST {
	public class ILASTPhi : ASTNode, IILASTStatement {
		public ILASTVariable Variable { get; set; }
		public ILASTVariable[] SourceVariables { get; set; }

		public override string ToString() {
			var ret = new StringBuilder();
			ret.AppendFormat("{0} = [", Variable);
			for (int i = 0; i < SourceVariables.Length; i++) {
				if (i != 0)
					ret.Append(", ");
				ret.Append(SourceVariables[i]);
			}
			ret.Append("]");
			return ret.ToString();
		}
	}
}