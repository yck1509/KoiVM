using System;
using KoiVM.CFG;
using KoiVM.RT;

namespace KoiVM.AST.IL {
	public class ILJumpTable : IILOperand, IHasOffset {
		public ILJumpTable(IBasicBlock[] targets) {
			Targets = targets;
			Chunk = new JumpTableChunk(this);
		}

		public JumpTableChunk Chunk { get; private set; }
		public ILInstruction RelativeBase { get; set; }
		public IBasicBlock[] Targets { get; set; }

		public uint Offset {
			get { return Chunk.Offset; }
		}

		public override string ToString() {
			return string.Format("[..{0}..]", Targets.Length);
		}
	}
}