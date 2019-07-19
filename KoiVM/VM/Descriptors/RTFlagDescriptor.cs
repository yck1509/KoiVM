using System;
using System.Linq;

namespace KoiVM.VM {
	public class RTFlagDescriptor {
		byte[] flagOrder = Enumerable.Range(1, 7).Select(x => (byte)x).ToArray();
		byte[] ehOrder = Enumerable.Range(0, 4).Select(x => (byte)x).ToArray();

		public RTFlagDescriptor(Random random) {
			random.Shuffle(flagOrder);
			random.Shuffle(ehOrder);
		}

		public byte INSTANCE {
			get { return flagOrder[0]; }
		}

		public byte EH_CATCH {
			get { return ehOrder[0]; }
		}

		public byte EH_FILTER {
			get { return ehOrder[1]; }
		}

		public byte EH_FAULT {
			get { return ehOrder[2]; }
		}

		public byte EH_FINALLY {
			get { return ehOrder[3]; }
		}
	}
}