using System;
using System.Collections.Generic;
using KoiVM.VM;

namespace KoiVM.AST.IL {
	public class ILRegister : IILOperand {
		static readonly Dictionary<VMRegisters, ILRegister> regMap = new Dictionary<VMRegisters, ILRegister>();

		ILRegister(VMRegisters reg) {
			Register = reg;
			regMap.Add(reg, this);
		}

		public VMRegisters Register { get; set; }

		public override string ToString() {
			return Register.ToString();
		}

		public static readonly ILRegister R0 = new ILRegister(VMRegisters.R0);
		public static readonly ILRegister R1 = new ILRegister(VMRegisters.R1);
		public static readonly ILRegister R2 = new ILRegister(VMRegisters.R2);
		public static readonly ILRegister R3 = new ILRegister(VMRegisters.R3);
		public static readonly ILRegister R4 = new ILRegister(VMRegisters.R4);
		public static readonly ILRegister R5 = new ILRegister(VMRegisters.R5);
		public static readonly ILRegister R6 = new ILRegister(VMRegisters.R6);
		public static readonly ILRegister R7 = new ILRegister(VMRegisters.R7);

		public static readonly ILRegister BP = new ILRegister(VMRegisters.BP);
		public static readonly ILRegister SP = new ILRegister(VMRegisters.SP);
		public static readonly ILRegister IP = new ILRegister(VMRegisters.IP);
		public static readonly ILRegister FL = new ILRegister(VMRegisters.FL);
		public static readonly ILRegister K1 = new ILRegister(VMRegisters.K1);
		public static readonly ILRegister K2 = new ILRegister(VMRegisters.K2);
		public static readonly ILRegister M1 = new ILRegister(VMRegisters.M1);
		public static readonly ILRegister M2 = new ILRegister(VMRegisters.M2);

		public static ILRegister LookupRegister(VMRegisters reg) {
			return regMap[reg];
		}
	}
}