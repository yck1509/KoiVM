using System;
using dnlib.DotNet;
using KoiVM.RT;

namespace KoiVM.AST.IL {
	public class ILMethodTarget : IILOperand, IHasOffset {
		ILBlock methodEntry;

		public ILMethodTarget(MethodDef target) {
			Target = target;
		}

		public MethodDef Target { get; set; }

		public void Resolve(VMRuntime runtime) {
			runtime.LookupMethod(Target, out methodEntry);
		}

		public uint Offset {
			get { return methodEntry == null ? 0 : methodEntry.Content[0].Offset; }
		}

		public override string ToString() {
			return Target.ToString();
		}
	}
}