using System;
using System.Linq;
using KoiVM.VMIL;

namespace KoiVM.VM {
	public class OpCodeDescriptor {
		byte[] opCodeOrder = Enumerable.Range(0, 256).Select(x => (byte)x).ToArray();

		public OpCodeDescriptor(Random random) {
			random.Shuffle(opCodeOrder);
		}

		public byte this[ILOpCode opCode] {
			get { return opCodeOrder[(int)opCode]; }
		}
	}
}