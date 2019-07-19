using System;
using System.Collections.Generic;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace KoiVM {
	public class Scanner {
		ModuleDef module;
		HashSet<MethodDef> methods;
		HashSet<MethodDef> exclude = new HashSet<MethodDef>();
		HashSet<MethodDef> export = new HashSet<MethodDef>();
		List<Tuple<MethodDef, bool>> results = new List<Tuple<MethodDef, bool>>();

		public Scanner(ModuleDef module)
			: this(module, null) {
		}

		public Scanner(ModuleDef module, HashSet<MethodDef> methods) {
			this.module = module;
			this.methods = methods;
		}

		public IEnumerable<Tuple<MethodDef, bool>> Scan() {
			ScanMethods(FindExclusion);
			ScanMethods(ScanExport);
			ScanMethods(PopulateResult);
			return results;
		}

		void ScanMethods(Action<MethodDef> scanFunc) {
			foreach (var type in module.GetTypes()) {
				foreach (var method in type.Methods)
					scanFunc(method);
			}
		}

		void FindExclusion(MethodDef method) {
			if (!method.HasBody || (methods != null && !methods.Contains(method)))
				exclude.Add(method);
			else if (method.HasGenericParameters) {
				foreach (var instr in method.Body.Instructions) {
					var target = instr.Operand as IMethod;
					if (target != null && target.IsMethod &&
					    (target = target.ResolveMethodDef()) != null &&
					    (methods == null || methods.Contains((MethodDef)target)))
						export.Add((MethodDef)target);
				}
			}
		}

		void ScanExport(MethodDef method) {
			if (!method.HasBody)
				return;

			bool shouldExport = false;
			shouldExport |= method.IsPublic;
			shouldExport |= method.SemanticsAttributes != 0;
			shouldExport |= method.IsConstructor;
			shouldExport |= method.IsVirtual;
			shouldExport |= (method.Module.EntryPoint == method);
			if (shouldExport)
				export.Add(method);

			bool excluded = exclude.Contains(method) || method.DeclaringType.HasGenericParameters;
			foreach (var instr in method.Body.Instructions) {
				if (instr.OpCode == OpCodes.Callvirt ||
				    (instr.Operand is IMethod && excluded)) {
					var target = ((IMethod)instr.Operand).ResolveMethodDef();
					if (target != null && (methods == null || methods.Contains((MethodDef)target)))
						export.Add(target);
				}
			}
		}

		void PopulateResult(MethodDef method) {
			if (exclude.Contains(method) || method.DeclaringType.HasGenericParameters)
				return;
			results.Add(Tuple.Create(method, export.Contains(method)));
		}
	}
}