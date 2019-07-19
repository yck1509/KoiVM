using System;
using dnlib.DotNet;
using KoiVM.CFG;
using KoiVM.RT;

namespace KoiVM.AST.IL {
	public class ILBlock : BasicBlock<ILInstrList> {
		public ILBlock(int id, ILInstrList content)
			: base(id, content) {
		}

		public virtual IKoiChunk CreateChunk(VMRuntime rt, MethodDef method) {
			return new BasicBlockChunk(rt, method, this);
		}
	}
}