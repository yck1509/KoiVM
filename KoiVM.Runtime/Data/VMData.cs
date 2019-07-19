using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

namespace KoiVM.Runtime.Data {
	internal unsafe class VMData {
		[StructLayout(LayoutKind.Sequential)]
		struct VMDAT_HEADER {
			public uint MAGIC;
			public uint MD_COUNT;
			public uint STR_COUNT;
			public uint EXP_COUNT;
		}

		class RefInfo {
			public Module module;
			public int token;
			public MemberInfo resolved;

			public MemberInfo Member {
				get { return resolved ?? (resolved = module.ResolveMember(token)); }
			}
		}

		Dictionary<uint, RefInfo> references;
		Dictionary<uint, string> strings;
		Dictionary<uint, VMExportInfo> exports;

		static Dictionary<Module, VMData> moduleVMData = new Dictionary<Module, VMData>();

		public Module Module { get; private set; }

		public VMData(Module module, void* data) {
			var header = (VMDAT_HEADER*)data;
			if (header->MAGIC != 0x68736966)
				throw new InvalidProgramException();

			references = new Dictionary<uint, RefInfo>();
			strings = new Dictionary<uint, string>();
			exports = new Dictionary<uint, VMExportInfo>();

			var ptr = (byte*)(header + 1);
			for (int i = 0; i < header->MD_COUNT; i++) {
				var id = Utils.ReadCompressedUInt(ref ptr);
				var token = (int)Utils.FromCodedToken(Utils.ReadCompressedUInt(ref ptr));
				references[id] = new RefInfo {
					module = module,
					token = token
				};
			}
			for (int i = 0; i < header->STR_COUNT; i++) {
				var id = Utils.ReadCompressedUInt(ref ptr);
				var len = Utils.ReadCompressedUInt(ref ptr);
				strings[id] = new string((char*)ptr, 0, (int)len);
				ptr += len << 1;
			}
			for (int i = 0; i < header->EXP_COUNT; i++) {
				exports[Utils.ReadCompressedUInt(ref ptr)] = new VMExportInfo(ref ptr, module);
			}

			KoiSection = (byte*)data;

			Module = module;
			moduleVMData[module] = this;
		}

		public static VMData Instance(Module module) {
			VMData data;
			lock (moduleVMData) {
				if (!moduleVMData.TryGetValue(module, out data))
					data = moduleVMData[module] = VMDataInitializer.GetData(module);
			}
			return data;
		}

		public byte* KoiSection { get; set; }

		public MemberInfo LookupReference(uint id) {
			return references[id].Member;
		}

		public string LookupString(uint id) {
			if (id == 0)
				return null;
			return strings[id];
		}

		public VMExportInfo LookupExport(uint id) {
			return exports[id];
		}
	}
}