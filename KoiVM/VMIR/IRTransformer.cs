using System;
using System.Collections.Generic;
using KoiVM.AST.IR;
using KoiVM.CFG;
using KoiVM.RT;
using KoiVM.VM;
using KoiVM.VMIR.Transforms;

namespace KoiVM.VMIR {
	public class IRTransformer {
		ITransform[] pipeline;

		public IRTransformer(ScopeBlock rootScope, IRContext ctx, VMRuntime runtime) {
			RootScope = rootScope;
			Context = ctx;
			Runtime = runtime;

			Annotations = new Dictionary<object, object>();
			InitPipeline();
		}

		void InitPipeline() {
			pipeline = new ITransform[] {
				// new SMCIRTransform(),
				Context.IsRuntime ? null : new GuardBlockTransform(),
				Context.IsRuntime ? null : new EHTransform(),
				new InitLocalTransform(),
				new ConstantTypePromotionTransform(),
				new GetSetFlagTransform(),
				new LogicTransform(),
				new InvokeTransform(),
				new MetadataTransform(),
				Context.IsRuntime ? null : new RegisterAllocationTransform(),
				Context.IsRuntime ? null : new StackFrameTransform(),
				new LeaTransform(),
				Context.IsRuntime ? null : new MarkReturnRegTransform()
			};
		}

		public IRContext Context { get; private set; }
		public VMRuntime Runtime { get; private set; }

		public VMDescriptor VM {
			get { return Runtime.Descriptor; }
		}

		public ScopeBlock RootScope { get; private set; }

		internal Dictionary<object, object> Annotations { get; private set; }
		internal BasicBlock<IRInstrList> Block { get; private set; }

		internal IRInstrList Instructions {
			get { return Block.Content; }
		}

		public void Transform() {
			if (pipeline == null)
				throw new InvalidOperationException("Transformer already used.");

			foreach (var handler in pipeline) {
				if (handler == null)
					continue;
				handler.Initialize(this);

				RootScope.ProcessBasicBlocks<IRInstrList>(block => {
					Block = block;
					handler.Transform(this);
				});
			}

			pipeline = null;
		}
	}
}