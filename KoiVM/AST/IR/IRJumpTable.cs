using System;
using KoiVM.CFG;

namespace KoiVM.AST.IR {
	public class IRJumpTable : IIROperand {
		public IRJumpTable(IBasicBlock[] targets) {
			Targets = targets;
		}

		public IBasicBlock[] Targets { get; set; }

		public ASTType Type {
			get { return ASTType.Ptr; }
		}

		public override string ToString() {
			return string.Format("[..{0}..]", Targets.Length);
		}
	}
}