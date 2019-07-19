using System;
using System.Collections.Generic;

namespace KoiVM.CFG {
	public interface IBasicBlock {
		int Id { get; }
		object Content { get; }
		BlockFlags Flags { get; set; }
		IEnumerable<IBasicBlock> Sources { get; }
		IEnumerable<IBasicBlock> Targets { get; }
	}
}