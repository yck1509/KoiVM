using System;

namespace KoiVM.CFG {
	[Flags]
	public enum BlockFlags {
		Normal = 0,
		ExitEHLeave = 1,
		ExitEHReturn = 2
	}
}