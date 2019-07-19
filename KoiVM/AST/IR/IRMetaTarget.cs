using System;

namespace KoiVM.AST.IR {
	public class IRMetaTarget : IIROperand {
		public IRMetaTarget(object mdItem) {
			MetadataItem = mdItem;
		}

		public object MetadataItem { get; set; }
		public bool LateResolve { get; set; }

		public ASTType Type {
			get { return ASTType.Ptr; }
		}

		public override string ToString() {
			return MetadataItem.ToString();
		}
	}
}