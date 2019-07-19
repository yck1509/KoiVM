using System;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using KoiVM.AST.IR;

namespace KoiVM.AST {
	public class InstrAnnotation {
		public InstrAnnotation(string name) {
			Name = name;
		}

		public string Name { get; private set; }

		public override string ToString() {
			return Name;
		}

		public static readonly InstrAnnotation JUMP = new InstrAnnotation("JUMP");
	}

	public class InstrCallInfo : InstrAnnotation {
		public InstrCallInfo(string name)
			: base(name) {
		}

		public ITypeDefOrRef ConstrainType { get; set; }
		public IMethod Method { get; set; }
		public IIROperand[] Arguments { get; set; }
		public IIROperand ReturnValue { get; set; }
		public IRRegister ReturnRegister { get; set; }
		public IRPointer ReturnSlot { get; set; }
		public bool IsECall { get; set; }

		public override string ToString() {
			return base.ToString() + " " + Method;
		}
	}

	public class PointerInfo : InstrAnnotation {
		public PointerInfo(string name, ITypeDefOrRef ptrType)
			: base(name) {
			PointerType = ptrType;
		}

		public ITypeDefOrRef PointerType { get; set; }
	}

	public class EHInfo : InstrAnnotation {
		public EHInfo(ExceptionHandler eh)
			: base("EH_" + eh.GetHashCode()) {
			ExceptionHandler = eh;
		}

		public ExceptionHandler ExceptionHandler { get; set; }
	}
}