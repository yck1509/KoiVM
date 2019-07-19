using System;
using System.Collections.Generic;
using KoiVM.AST.IL;
using KoiVM.AST.IR;
using KoiVM.CFG;
using KoiVM.RT;
using KoiVM.VM;
using KoiVM.VMIR;

namespace KoiVM.VMIL {
	public class ILTranslator {
		static ILTranslator() {
			handlers = new Dictionary<IROpCode, ITranslationHandler>();
			foreach (var type in typeof(ILTranslator).Assembly.GetExportedTypes()) {
				if (typeof(ITranslationHandler).IsAssignableFrom(type) && !type.IsAbstract) {
					var handler = (ITranslationHandler)Activator.CreateInstance(type);
					handlers.Add(handler.IRCode, handler);
				}
			}
		}

		static readonly Dictionary<IROpCode, ITranslationHandler> handlers;

		public ILTranslator(VMRuntime runtime) {
			Runtime = runtime;
		}

		public VMRuntime Runtime { get; private set; }

		public VMDescriptor VM {
			get { return Runtime.Descriptor; }
		}

		internal ILInstrList Instructions { get; private set; }

		public ILInstrList Translate(IRInstrList instrs) {
			Instructions = new ILInstrList();

			int i = 0;
			foreach (var instr in instrs) {
				ITranslationHandler handler;
				if (!handlers.TryGetValue(instr.OpCode, out handler))
					throw new NotSupportedException(instr.OpCode.ToString());
				try {
					handler.Translate(instr, this);
				}
				catch (Exception ex) {
					throw new Exception(string.Format("Failed to translate ir {0}.", instr.ILAST), ex);
				}
				while (i < Instructions.Count) {
					Instructions[i].IR = instr;
					i++;
				}
			}

			var ret = Instructions;
			Instructions = null;
			return ret;
		}

		public void Translate(ScopeBlock rootScope) {
			var blockMap = rootScope.UpdateBasicBlocks<IRInstrList, ILInstrList>(
				block => { return Translate(block.Content); },
				(id, content) => new ILBlock(id, content));

			rootScope.ProcessBasicBlocks<ILInstrList>(block => {
				foreach (var instr in block.Content) {
					if (instr.Operand is ILBlockTarget) {
						var op = (ILBlockTarget)instr.Operand;
						op.Target = blockMap[(BasicBlock<IRInstrList>)op.Target];
					}
					else if (instr.Operand is ILJumpTable) {
						var op = (ILJumpTable)instr.Operand;
						for (int i = 0; i < op.Targets.Length; i++)
							op.Targets[i] = blockMap[(BasicBlock<IRInstrList>)op.Targets[i]];
					}
				}
			});
		}
	}
}