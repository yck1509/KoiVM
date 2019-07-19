using System;
using System.Linq;

namespace KoiVM.VM {
	public class RegisterDescriptor {
		byte[] regOrder = Enumerable.Range(0, (int)VMRegisters.Max).Select(x => (byte)x).ToArray();

		public RegisterDescriptor(Random random) {
			random.Shuffle(regOrder);
		}

		public byte this[VMRegisters reg] {
			get { return regOrder[(int)reg]; }
		}
	}
}