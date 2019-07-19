using System;

namespace KoiVM.VM {
	public class VCallOpsDescriptor {
		uint[] ecallOrder = { 0, 1, 2, 3 };

		public VCallOpsDescriptor(Random random) {
			random.Shuffle(ecallOrder);
		}

		public uint ECALL_CALL {
			get { return ecallOrder[0]; }
		}

		public uint ECALL_CALLVIRT {
			get { return ecallOrder[1]; }
		}

		public uint ECALL_NEWOBJ {
			get { return ecallOrder[2]; }
		}

		public uint ECALL_CALLVIRT_CONSTRAINED {
			get { return ecallOrder[3]; }
		}
	}
}