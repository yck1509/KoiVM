using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace KoiVM.RT.Mutation {
	[Obfuscation(Exclude = false, Feature = "+koi;-ref proxy")]
	internal class RTMap {
		public static readonly string VMEntry = "KoiVM.Runtime.VMEntry";
		public static readonly string VMRun = "Run";
		public static readonly string VMDispatcher = "KoiVM.Runtime.Execution.VMDispatcher";
		public static readonly string VMDispatcherDothrow = "DoThrow";
		public static readonly string VMDispatcherThrow = "Throw";
		public static readonly string VMDispatcherGetIP = "GetIP";
		public static readonly string VMDispatcherStackwalk = "StackWalk";

		public static readonly string VMConstants = "KoiVM.Runtime.Dynamic.Constants";

		public static readonly Dictionary<string, string> VMConstMap;

		static RTMap() {
			const string map = @"
REG_R0								R0
REG_R1								R1
REG_R2								R2
REG_R3								R3
REG_R4								R4
REG_R5								R5
REG_R6								R6
REG_R7								R7
REG_BP								BP
REG_SP								SP
REG_IP								IP
REG_FL								FL
REG_K1								K1
REG_K2								K2
REG_M1								M1
REG_M2								M2
FL_OVERFLOW							OVERFLOW
FL_CARRY							CARRY
FL_ZERO								ZERO
FL_SIGN								SIGN
FL_UNSIGNED							UNSIGNED
FL_BEHAV1							BEHAV1
FL_BEHAV2							BEHAV2
FL_BEHAV3							BEHAV3
OP_NOP								NOP
OP_LIND_PTR							LIND_PTR
OP_LIND_OBJECT						LIND_OBJECT
OP_LIND_BYTE						LIND_BYTE
OP_LIND_WORD						LIND_WORD
OP_LIND_DWORD						LIND_DWORD
OP_LIND_QWORD						LIND_QWORD
OP_SIND_PTR							SIND_PTR
OP_SIND_OBJECT						SIND_OBJECT
OP_SIND_BYTE						SIND_BYTE
OP_SIND_WORD						SIND_WORD
OP_SIND_DWORD						SIND_DWORD
OP_SIND_QWORD						SIND_QWORD
OP_POP								POP
OP_PUSHR_OBJECT						PUSHR_OBJECT
OP_PUSHR_BYTE						PUSHR_BYTE
OP_PUSHR_WORD						PUSHR_WORD
OP_PUSHR_DWORD						PUSHR_DWORD
OP_PUSHR_QWORD						PUSHR_QWORD
OP_PUSHI_DWORD						PUSHI_DWORD
OP_PUSHI_QWORD						PUSHI_QWORD
OP_SX_BYTE							SX_BYTE
OP_SX_WORD							SX_WORD
OP_SX_DWORD							SX_DWORD
OP_CALL								CALL
OP_RET								RET
OP_NOR_DWORD						NOR_DWORD
OP_NOR_QWORD						NOR_QWORD
OP_CMP								CMP
OP_CMP_DWORD						CMP_DWORD
OP_CMP_QWORD						CMP_QWORD
OP_CMP_R32							CMP_R32
OP_CMP_R64							CMP_R64
OP_JZ								JZ
OP_JNZ								JNZ
OP_JMP								JMP
OP_SWT								SWT
OP_ADD_DWORD						ADD_DWORD
OP_ADD_QWORD						ADD_QWORD
OP_ADD_R32							ADD_R32
OP_ADD_R64							ADD_R64
OP_SUB_R32							SUB_R32
OP_SUB_R64							SUB_R64
OP_MUL_DWORD						MUL_DWORD
OP_MUL_QWORD						MUL_QWORD
OP_MUL_R32							MUL_R32
OP_MUL_R64							MUL_R64
OP_DIV_DWORD						DIV_DWORD
OP_DIV_QWORD						DIV_QWORD
OP_DIV_R32							DIV_R32
OP_DIV_R64							DIV_R64
OP_REM_DWORD						REM_DWORD
OP_REM_QWORD						REM_QWORD
OP_REM_R32							REM_R32
OP_REM_R64							REM_R64
OP_SHR_DWORD						SHR_DWORD
OP_SHR_QWORD						SHR_QWORD
OP_SHL_DWORD						SHL_DWORD
OP_SHL_QWORD						SHL_QWORD
OP_FCONV_R32_R64					FCONV_R32_R64
OP_FCONV_R64_R32					FCONV_R64_R32
OP_FCONV_R32						FCONV_R32
OP_FCONV_R64						FCONV_R64
OP_ICONV_PTR						ICONV_PTR
OP_ICONV_R64						ICONV_R64
OP_VCALL							VCALL
OP_TRY								TRY
OP_LEAVE							LEAVE
VCALL_EXIT							EXIT
VCALL_BREAK							BREAK
VCALL_ECALL							ECALL
VCALL_CAST							CAST
VCALL_CKFINITE						CKFINITE
VCALL_CKOVERFLOW					CKOVERFLOW
VCALL_RANGECHK						RANGECHK
VCALL_INITOBJ						INITOBJ
VCALL_LDFLD							LDFLD
VCALL_LDFTN							LDFTN
VCALL_TOKEN							TOKEN
VCALL_THROW							THROW
VCALL_SIZEOF						SIZEOF
VCALL_STFLD							STFLD
VCALL_BOX							BOX
VCALL_UNBOX							UNBOX
VCALL_LOCALLOC						LOCALLOC
HELPER_INIT							INIT
ECALL_CALL							E_CALL
ECALL_CALLVIRT						E_CALLVIRT
ECALL_NEWOBJ						E_NEWOBJ
ECALL_CALLVIRT_CONSTRAINED			E_CALLVIRT_CONSTRAINED
FLAG_INSTANCE						INSTANCE
EH_CATCH							CATCH
EH_FILTER							FILTER
EH_FAULT							FAULT
EH_FINALLY							FINALLY
";

			VMConstMap = new Dictionary<string, string>();
			using (var reader = new StringReader(map)) {
				while (reader.Peek() > 0) {
					var line = reader.ReadLine().Trim();
					if (string.IsNullOrEmpty(line))
						continue;
					var entry = line.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
					VMConstMap[entry[1]] = entry[0];
				}
			}
		}
	}
}