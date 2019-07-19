using System;
using System.Reflection;

namespace KoiVM.VMIR {
	[Obfuscation(Exclude = false, ApplyToMembers = false, Feature = "+rename(forceRen=true);")]
	public enum IROpCode {
		NOP,

		MOV,
		POP,
		PUSH,
		CALL,
		RET,

		NOR,

		CMP,
		JZ,
		JNZ,
		JMP,
		SWT,

		ADD,
		SUB, // Only for floats
		MUL,
		DIV,
		REM,
		SHR,
		SHL,

		FCONV,
		ICONV,
		SX,

		VCALL,

		TRY,
		LEAVE,

		Max,

		// Pseudo-Opcodes, will be eliminate by transforms
		__NOT,
		__AND,
		__OR,
		__XOR,

		__GETF,
		__SETF,

		__CALL,
		__CALLVIRT,
		__NEWOBJ,
		__BEGINCALL,
		__ENDCALL,

		__ENTRY,
		__EXIT,

		__LEAVE,
		__EHRET,

		__LDOBJ,
		__STOBJ,

		__GEN,
		__KILL,

		__LEA
	}
}