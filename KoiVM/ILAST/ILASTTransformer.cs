using System;
using System.Collections.Generic;
using dnlib.DotNet;
using KoiVM.AST.ILAST;
using KoiVM.CFG;
using KoiVM.ILAST.Transformation;
using KoiVM.RT;
using KoiVM.VM;

namespace KoiVM.ILAST {
	public class ILASTTransformer {
		ITransformationHandler[] pipeline;

		public ILASTTransformer(MethodDef method, ScopeBlock rootScope, VMRuntime runtime) {
			RootScope = rootScope;
			Method = method;
			Runtime = runtime;

			Annotations = new Dictionary<object, object>();
			InitPipeline();
		}

		void InitPipeline() {
			pipeline = new ITransformationHandler[] {
				new VariableInlining(),
				new StringTransform(),
				new ArrayTransform(),
				new IndirectTransform(),
				new ILASTTypeInference(),
				new NullTransform(),
				new BranchTransform()
			};
		}

		public MethodDef Method { get; private set; }
		public ScopeBlock RootScope { get; private set; }
		public VMRuntime Runtime { get; private set; }

		public VMDescriptor VM {
			get { return Runtime.Descriptor; }
		}

		internal Dictionary<object, object> Annotations { get; private set; }
		internal BasicBlock<ILASTTree> Block { get; private set; }

		internal ILASTTree Tree {
			get { return Block.Content; }
		}

		public void Transform() {
			if (pipeline == null)
				throw new InvalidOperationException("Transformer already used.");

			foreach (var handler in pipeline) {
				handler.Initialize(this);

				RootScope.ProcessBasicBlocks<ILASTTree>(block => {
					Block = block;
					handler.Transform(this);
				});
			}

			pipeline = null;
		}
	}
}