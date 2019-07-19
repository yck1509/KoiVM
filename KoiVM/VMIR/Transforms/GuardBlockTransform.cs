using System;
using KoiVM.AST.IR;
using KoiVM.CFG;

namespace KoiVM.VMIR.Transforms {
	public class GuardBlockTransform : ITransform {
		BasicBlock<IRInstrList> prolog;
		BasicBlock<IRInstrList> epilog;

		public void Initialize(IRTransformer tr) {
			int maxId = 0;
			BasicBlock<IRInstrList> entry = null;
			tr.RootScope.ProcessBasicBlocks<IRInstrList>(block => {
				block.Id++;
				if (block.Id > maxId)
					maxId = block.Id;
				if (entry == null)
					entry = block;
			});

			prolog = new BasicBlock<IRInstrList>(0, new IRInstrList {
				new IRInstruction(IROpCode.__ENTRY),
				new IRInstruction(IROpCode.JMP, new IRBlockTarget(entry))
			});
			prolog.Targets.Add(entry);
			entry.Sources.Add(prolog);

			epilog = new BasicBlock<IRInstrList>(maxId + 1, new IRInstrList {
				new IRInstruction(IROpCode.__EXIT)
			});
			InsertProlog(tr.RootScope);
			InsertEpilog(tr.RootScope);
		}

		void InsertProlog(ScopeBlock block) {
			if (block.Children.Count > 0) {
				if (block.Children[0].Type == ScopeType.None)
					InsertProlog(block.Children[0]);
				else {
					var prologScope = new ScopeBlock();
					prologScope.Content.Add(prolog);
					block.Children.Insert(0, prologScope);
				}
			}
			else {
				block.Content.Insert(0, prolog);
			}
		}

		void InsertEpilog(ScopeBlock block) {
			if (block.Children.Count > 0) {
				if (block.Children[block.Children.Count - 1].Type == ScopeType.None)
					InsertEpilog(block.Children[block.Children.Count - 1]);
				else {
					var epilogScope = new ScopeBlock();
					epilogScope.Content.Add(epilog);
					block.Children.Insert(block.Children.Count, epilogScope);
				}
			}
			else {
				block.Content.Insert(block.Content.Count, epilog);
			}
		}

		public void Transform(IRTransformer tr) {
			tr.Instructions.VisitInstrs(VisitInstr, tr);
		}

		void VisitInstr(IRInstrList instrs, IRInstruction instr, ref int index, IRTransformer tr) {
			if (instr.OpCode == IROpCode.RET) {
				instrs.Replace(index, new[] {
					new IRInstruction(IROpCode.JMP, new IRBlockTarget(epilog))
				});
				if (!tr.Block.Targets.Contains(epilog)) {
					tr.Block.Targets.Add(epilog);
					epilog.Sources.Add(tr.Block);
				}
			}
		}
	}
}