using System;
using System.Reflection;

namespace KoiVM.Runtime.Data {
	internal class VMFuncSig {
		public unsafe VMFuncSig(ref byte* ptr, Module module) {
			this.module = module;

			Flags = *ptr++;
			paramToks = new int[Utils.ReadCompressedUInt(ref ptr)];
			for (int i = 0; i < paramToks.Length; i++) {
				paramToks[i] = (int)Utils.FromCodedToken(Utils.ReadCompressedUInt(ref ptr));
			}
			retTok = (int)Utils.FromCodedToken(Utils.ReadCompressedUInt(ref ptr));
		}

		Module module;
		readonly int[] paramToks;
		readonly int retTok;
		Type[] paramTypes;
		Type retType;

		public byte Flags;

		public Type[] ParamTypes {
			get {
				if (paramTypes != null)
					return paramTypes;

				var p = new Type[paramToks.Length];
				for (int i = 0; i < p.Length; i++) {
					p[i] = module.ResolveType(paramToks[i]);
				}
				paramTypes = p;
				return p;
			}
		}

		public Type RetType {
			get { return retType ?? (retType = module.ResolveType(retTok)); }
		}
	}
}