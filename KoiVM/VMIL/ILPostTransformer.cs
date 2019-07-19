using System;
using System.Collections.Generic;
using dnlib.DotNet;
using KoiVM.AST.IL;
using KoiVM.CFG;
using KoiVM.RT;
using KoiVM.VMIL.Transforms;

namespace KoiVM.VMIL {
	public class ILPostTransformer {
		IPostTransform[] pipeline;

		public ILPostTransformer(MethodDef method, ScopeBlock rootScope, VMRuntime runtime) {
			RootScope = rootScope;
			Method = method;
			Runtime = runtime;

			Annotations = new Dictionary<object, object>();
			pipeline = InitPipeline();
		}

		IPostTransform[] InitPipeline() {
			return new IPostTransform[] {
				new SaveRegistersTransform(),
				new FixMethodRefTransform(),
				new BlockKeyTransform()
			};
		}

		public VMRuntime Runtime { get; private set; }
		public MethodDef Method { get; private set; }
		public ScopeBlock RootScope { get; private set; }

		internal Dictionary<object, object> Annotations { get; private set; }
		internal ILBlock Block { get; private set; }

		internal ILInstrList Instructions {
			get { return Block.Content; }
		}

		public void Transform() {
			if (pipeline == null)
				throw new InvalidOperationException("Transformer already used.");

			foreach (var handler in pipeline) {
				handler.Initialize(this);

				RootScope.ProcessBasicBlocks<ILInstrList>(block => {
					Block = (ILBlock)block;
					handler.Transform(this);
				});
			}

			pipeline = null;
		}
	}
}