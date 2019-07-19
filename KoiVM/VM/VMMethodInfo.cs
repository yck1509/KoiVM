using System;
using System.Collections.Generic;
using KoiVM.CFG;

namespace KoiVM.VM {
	public class VMMethodInfo {
		public VMMethodInfo() {
			BlockKeys = new Dictionary<IBasicBlock, VMBlockKey>();
			UsedRegister = new HashSet<VMRegisters>();
		}

		public ScopeBlock RootScope;
		public readonly Dictionary<IBasicBlock, VMBlockKey> BlockKeys;
		public readonly HashSet<VMRegisters> UsedRegister;
		public byte EntryKey;
		public byte ExitKey;
	}

	public struct VMBlockKey {
		public byte EntryKey;
		public byte ExitKey;
	}
}