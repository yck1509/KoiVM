using System;
using System.Reflection;
using dnlib.DotNet;
using KoiVM.CFG;
using KoiVM.ILAST;
using KoiVM.RT;
using KoiVM.VMIL;
using KoiVM.VMIR;

namespace KoiVM {
	[Obfuscation(Exclude = false, Feature = "+koi;-ref proxy")]
	public class MethodVirtualizer {
		public MethodVirtualizer(VMRuntime runtime) {
			Runtime = runtime;
		}

		protected VMRuntime Runtime { get; private set; }
		protected MethodDef Method { get; private set; }
		protected ScopeBlock RootScope { get; private set; }
		protected IRContext IRContext { get; private set; }
		protected bool IsExport { get; private set; }

		public ScopeBlock Run(MethodDef method, bool isExport) {
			try {
				Method = method;
				IsExport = isExport;

				Init();
				BuildILAST();
				TransformILAST();
				BuildVMIR();
				TransformVMIR();
				BuildVMIL();
				TransformVMIL();
				Deinitialize();

				var scope = RootScope;
				RootScope = null;
				Method = null;
				return scope;
			}
			catch (Exception ex) {
				throw new Exception(string.Format("Failed to translate method {0}.", method), ex);
			}
		}

		protected virtual void Init() {
			RootScope = BlockParser.Parse(Method, Method.Body);
			IRContext = new IRContext(Method, Method.Body);
		}

		protected virtual void BuildILAST() {
			ILASTBuilder.BuildAST(Method, Method.Body, RootScope);
		}

		protected virtual void TransformILAST() {
			var transformer = new ILASTTransformer(Method, RootScope, Runtime);
			transformer.Transform();
		}

		protected virtual void BuildVMIR() {
			var translator = new IRTranslator(IRContext, Runtime);
			translator.Translate(RootScope);
		}

		protected virtual void TransformVMIR() {
			var transformer = new IRTransformer(RootScope, IRContext, Runtime);
			transformer.Transform();
		}

		protected virtual void BuildVMIL() {
			var translator = new ILTranslator(Runtime);
			translator.Translate(RootScope);
		}

		protected virtual void TransformVMIL() {
			var transformer = new ILTransformer(Method, RootScope, Runtime);
			transformer.Transform();
		}

		protected virtual void Deinitialize() {
			IRContext = null;
			Runtime.AddMethod(Method, RootScope);
			if (IsExport)
				Runtime.ExportMethod(Method);
		}
	}
}