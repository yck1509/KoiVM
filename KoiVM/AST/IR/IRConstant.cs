using System;

namespace KoiVM.AST.IR {
	public class IRConstant : ASTConstant, IIROperand {
		ASTType IIROperand.Type {
			get { return base.Type.Value; }
		}

		public static IRConstant FromI4(int value) {
			return new IRConstant {
				Value = value,
				Type = ASTType.I4
			};
		}

		public static IRConstant FromI8(long value) {
			return new IRConstant {
				Value = value,
				Type = ASTType.I8
			};
		}

		public static IRConstant FromR4(float value) {
			return new IRConstant {
				Value = value,
				Type = ASTType.R4
			};
		}

		public static IRConstant FromR8(double value) {
			return new IRConstant {
				Value = value,
				Type = ASTType.R8
			};
		}

		public static IRConstant FromString(string value) {
			return new IRConstant {
				Value = value,
				Type = ASTType.O
			};
		}

		public static IRConstant Null() {
			return new IRConstant {
				Value = null,
				Type = ASTType.O
			};
		}
	}
}