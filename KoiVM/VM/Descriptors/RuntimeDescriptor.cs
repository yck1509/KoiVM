using System;

namespace KoiVM.VM {
	public class RuntimeDescriptor {
		public RuntimeDescriptor(Random random) {
			VMCall = new VMCallDescriptor(random);
			VCallOps = new VCallOpsDescriptor(random);
			RTFlags = new RTFlagDescriptor(random);
		}

		public VMCallDescriptor VMCall { get; private set; }
		public VCallOpsDescriptor VCallOps { get; private set; }
		public RTFlagDescriptor RTFlags { get; private set; }
	}
}