using System;
using KoiVM.RT;

namespace KoiVM.AST.IL {
	public class ILRelReference : IILOperand {
		public ILRelReference(IHasOffset target, IHasOffset relBase) {
			Target = target;
			Base = relBase;
		}

		public IHasOffset Target { get; set; }
		public IHasOffset Base { get; set; }

		public virtual uint Resolve(VMRuntime runtime) {
			var relBase = Base.Offset;
			if (Base is ILInstruction)
				relBase += runtime.serializer.ComputeLength((ILInstruction)Base);
			return Target.Offset - relBase;
		}

		public override string ToString() {
			return string.Format("[{0:x8}:{1:x8}]", Base.Offset, Target.Offset);
		}
	}
}