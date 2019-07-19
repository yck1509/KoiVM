using System;

namespace KoiVM.VM {
	public class ArchDescriptor {
		public ArchDescriptor(Random random) {
			OpCodes = new OpCodeDescriptor(random);
			Flags = new FlagDescriptor(random);
			Registers = new RegisterDescriptor(random);
		}

		public OpCodeDescriptor OpCodes { get; private set; }
		public FlagDescriptor Flags { get; private set; }
		public RegisterDescriptor Registers { get; private set; }
	}
}