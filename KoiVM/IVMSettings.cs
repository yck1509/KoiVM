using System;
using dnlib.DotNet;

namespace KoiVM {
	public interface IVMSettings {
		int Seed { get; }
		bool IsDebug { get; }
		bool ExportDbgInfo { get; }
		bool DoStackWalk { get; }
		bool IsVirtualized(MethodDef method);
		bool IsExported(MethodDef method);
	}
}