using System;
using KoiVM.VM;

namespace KoiVM.AST.IR {
	public class IRRegister : IIROperand {
		public IRRegister(VMRegisters reg) {
			Register = reg;
			Type = ASTType.Ptr;
		}

		public IRRegister(VMRegisters reg, ASTType type) {
			Register = reg;
			Type = type;
		}

		public VMRegisters Register { get; set; }
		public IRVariable SourceVariable { get; set; }
		public ASTType Type { get; set; }

		public override string ToString() {
			return Register.ToString();
		}

		public static readonly IRRegister BP = new IRRegister(VMRegisters.BP, ASTType.I4);
		public static readonly IRRegister SP = new IRRegister(VMRegisters.SP, ASTType.I4);
		public static readonly IRRegister IP = new IRRegister(VMRegisters.IP);
		public static readonly IRRegister FL = new IRRegister(VMRegisters.FL, ASTType.I4);
		public static readonly IRRegister K1 = new IRRegister(VMRegisters.K1, ASTType.I4);
		public static readonly IRRegister K2 = new IRRegister(VMRegisters.K2, ASTType.I4);
		public static readonly IRRegister M1 = new IRRegister(VMRegisters.M1, ASTType.I4);
		public static readonly IRRegister M2 = new IRRegister(VMRegisters.M2, ASTType.I4);
	}
}