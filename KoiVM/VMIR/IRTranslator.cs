using System;
using System.Collections.Generic;
using System.Diagnostics;
using dnlib.DotNet.Emit;
using KoiVM.AST.ILAST;
using KoiVM.AST.IR;
using KoiVM.CFG;
using KoiVM.RT;
using KoiVM.VM;

namespace KoiVM.VMIR {
	public class IRTranslator {
		static IRTranslator() {
			handlers = new Dictionary<Code, ITranslationHandler>();
			foreach (var type in typeof(IRTranslator).Assembly.GetExportedTypes()) {
				if (typeof(ITranslationHandler).IsAssignableFrom(type) && !type.IsAbstract) {
					var handler = (ITranslationHandler)Activator.CreateInstance(type);
					handlers.Add(handler.ILCode, handler);
				}
			}
		}

		static readonly Dictionary<Code, ITranslationHandler> handlers;

		public IRTranslator(IRContext ctx, VMRuntime runtime) {
			Context = ctx;
			Runtime = runtime;
		}

		public ScopeBlock RootScope { get; private set; }
		public IRContext Context { get; private set; }
		public VMRuntime Runtime { get; private set; }

		public VMDescriptor VM {
			get { return Runtime.Descriptor; }
		}

		public ArchDescriptor Arch {
			get { return VM.Architecture; }
		}

		internal BasicBlock<ILASTTree> Block { get; private set; }
		internal IRInstrList Instructions { get; private set; }

		internal IIROperand Translate(IILASTNode node) {
			if (node is ILASTExpression) {
				var expr = (ILASTExpression)node;
				try {
					ITranslationHandler handler;
					if (!handlers.TryGetValue(expr.ILCode, out handler))
						throw new NotSupportedException(expr.ILCode.ToString());

					int i = Instructions.Count;
					var operand = handler.Translate(expr, this);
					while (i < Instructions.Count) {
						Instructions[i].ILAST = expr;
						i++;
					}
					return operand;
				}
				catch (Exception ex) {
					throw new Exception(string.Format("Failed to translate expr {0} @ {1:x4}.",
						expr.CILInstr, expr.CILInstr.GetOffset()), ex);
				}
			}
			if (node is ILASTVariable) {
				return Context.ResolveVRegister((ILASTVariable)node);
			}
			throw new NotSupportedException();
		}

		IRInstrList Translate(BasicBlock<ILASTTree> block) {
			Block = block;
			Instructions = new IRInstrList();

			bool seenJump = false;
			foreach (var st in block.Content) {
				if (st is ILASTPhi) {
					var variable = ((ILASTPhi)st).Variable;
					Instructions.Add(new IRInstruction(IROpCode.POP) {
						Operand1 = Context.ResolveVRegister(variable),
						ILAST = st
					});
				}
				else if (st is ILASTAssignment) {
					var assignment = (ILASTAssignment)st;
					var valueVar = Translate(assignment.Value);
					Instructions.Add(new IRInstruction(IROpCode.MOV) {
						Operand1 = Context.ResolveVRegister(assignment.Variable),
						Operand2 = valueVar,
						ILAST = st
					});
				}
				else if (st is ILASTExpression) {
					var expr = (ILASTExpression)st;
					var opCode = expr.ILCode.ToOpCode();
					if (!seenJump && (opCode.FlowControl == FlowControl.Cond_Branch ||
					                  opCode.FlowControl == FlowControl.Branch ||
					                  opCode.FlowControl == FlowControl.Return ||
					                  opCode.FlowControl == FlowControl.Throw)) {
						// Add stack remain before jumps
						foreach (var remain in block.Content.StackRemains) {
							Instructions.Add(new IRInstruction(IROpCode.PUSH) {
								Operand1 = Context.ResolveVRegister(remain),
								ILAST = st
							});
						}
						seenJump = true;
					}
					Translate((ILASTExpression)st);
				}
				else
					throw new NotSupportedException();
			}
			Debug.Assert(seenJump);

			var ret = Instructions;
			Instructions = null;
			return ret;
		}

		public void Translate(ScopeBlock rootScope) {
			RootScope = rootScope;
			var blockMap = rootScope.UpdateBasicBlocks<ILASTTree, IRInstrList>(block => { return Translate(block); });
			rootScope.ProcessBasicBlocks<IRInstrList>(block => {
				foreach (var instr in block.Content) {
					if (instr.Operand1 is IRBlockTarget) {
						var op = (IRBlockTarget)instr.Operand1;
						op.Target = blockMap[(BasicBlock<ILASTTree>)op.Target];
					}
					else if (instr.Operand1 is IRJumpTable) {
						var op = (IRJumpTable)instr.Operand1;
						for (int i = 0; i < op.Targets.Length; i++)
							op.Targets[i] = blockMap[(BasicBlock<ILASTTree>)op.Targets[i]];
					}

					if (instr.Operand2 is IRBlockTarget) {
						var op = (IRBlockTarget)instr.Operand2;
						op.Target = blockMap[(BasicBlock<ILASTTree>)op.Target];
					}
					else if (instr.Operand2 is IRJumpTable) {
						var op = (IRJumpTable)instr.Operand2;
						for (int i = 0; i < op.Targets.Length; i++)
							op.Targets[i] = blockMap[(BasicBlock<ILASTTree>)op.Targets[i]];
					}
				}
			});
		}
	}
}