using System;
using System.Linq;

namespace KoiVM.VM {
	public class FlagDescriptor {
		int[] flagOrder = Enumerable.Range(0, (int)VMFlags.Max).ToArray();

		public FlagDescriptor(Random random) {
			random.Shuffle(flagOrder);
		}

		public int this[VMFlags flag] {
			get { return flagOrder[(int)flag]; }
		}

		public int OVERFLOW {
			get { return flagOrder[0]; }
		}

		public int CARRY {
			get { return flagOrder[1]; }
		}

		public int ZERO {
			get { return flagOrder[2]; }
		}

		public int SIGN {
			get { return flagOrder[3]; }
		}

		public int UNSIGNED {
			get { return flagOrder[4]; }
		}

		public int BEHAV1 {
			get { return flagOrder[5]; }
		}

		public int BEHAV2 {
			get { return flagOrder[6]; }
		}

		public int BEHAV3 {
			get { return flagOrder[7]; }
		}
	}
}