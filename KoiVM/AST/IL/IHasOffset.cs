using System;

namespace KoiVM.AST.IL {
	public interface IHasOffset {
		uint Offset { get; }
	}
}