using System;

namespace KoiVM.VM {
	public class VMDescriptor {
		public VMDescriptor(IVMSettings settings) {
			Random = new Random(settings.Seed);
			Settings = settings;
			Architecture = new ArchDescriptor(Random);
			Runtime = new RuntimeDescriptor(Random);
			Data = new DataDescriptor(Random);
		}

		public Random Random { get; private set; }
		public IVMSettings Settings { get; private set; }
		public ArchDescriptor Architecture { get; private set; }
		public RuntimeDescriptor Runtime { get; private set; }
		public DataDescriptor Data { get; private set; }

		public void ResetData() {
			Data = new DataDescriptor(Random);
		}
	}
}