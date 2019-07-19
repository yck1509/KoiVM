using System;
using System.Collections.Generic;
using KoiVM.Runtime.OpCodes;

namespace KoiVM.Runtime.Data {
	internal static class OpCodeMap {
		static readonly Dictionary<byte, IOpCode> opCodes;

		static OpCodeMap() {
			opCodes = new Dictionary<byte, IOpCode>();
			foreach (var type in typeof(OpCodeMap).Assembly.GetTypes()) {
				if (typeof(IOpCode).IsAssignableFrom(type) && !type.IsAbstract) {
					var opCode = (IOpCode)Activator.CreateInstance(type);
					opCodes[opCode.Code] = opCode;
				}
			}
		}

		public static IOpCode Lookup(byte code) {
			return opCodes[code];
		}
	}
}