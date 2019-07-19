using System;
using dnlib.DotNet;

namespace KoiVM.VMIR.Compiler {
	public class IRCompilerAssemblyFinder : IAssemblyRefFinder {
		readonly ModuleDef module;
		readonly AssemblyDef corlib;

		public IRCompilerAssemblyFinder(ModuleDef module) {
			this.module = module;
			corlib = module.Context.AssemblyResolver.Resolve(module.CorLibTypes.AssemblyRef, module);
		}

		public AssemblyRef FindAssemblyRef(TypeRef nonNestedTypeRef) {
			if (corlib.Find(nonNestedTypeRef) != null) {
				return module.CorLibTypes.AssemblyRef;
			}
			return AssemblyRef.CurrentAssembly;
		}
	}
}