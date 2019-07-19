using System;
using System.Linq;

namespace KoiVM.VM {
	public class VMCallDescriptor {
		int[] callOrder = Enumerable.Range(0, 256).ToArray();

		public VMCallDescriptor(Random random) {
			random.Shuffle(callOrder);
		}

		public int this[VMCalls call] {
			get { return callOrder[(int)call]; }
		}

		public int EXIT {
			get { return callOrder[0]; }
		}

		public int BREAK {
			get { return callOrder[1]; }
		}

		public int ECALL {
			get { return callOrder[2]; }
		}

		public int CAST {
			get { return callOrder[3]; }
		}

		public int CKFINITE {
			get { return callOrder[4]; }
		}

		public int CKOVERFLOW {
			get { return callOrder[5]; }
		}

		public int RANGECHK {
			get { return callOrder[6]; }
		}

		public int INITOBJ {
			get { return callOrder[7]; }
		}

		public int LDFLD {
			get { return callOrder[8]; }
		}

		public int LDFTN {
			get { return callOrder[9]; }
		}

		public int TOKEN {
			get { return callOrder[10]; }
		}

		public int THROW {
			get { return callOrder[11]; }
		}

		public int SIZEOF {
			get { return callOrder[12]; }
		}

		public int STFLD {
			get { return callOrder[13]; }
		}

		public int BOX {
			get { return callOrder[14]; }
		}

		public int UNBOX {
			get { return callOrder[15]; }
		}

		public int LOCALLOC {
			get { return callOrder[16]; }
		}
	}
}