using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using KoiVM.AST.IR;
using KoiVM.CFG;

namespace KoiVM.VMIR.RegAlloc {
	public class LivenessAnalysis {
		#region OpCode Liveness Info

		enum LiveFlags {
			GEN1 = 1,
			GEN2 = 2,
			KILL1 = 4,
			KILL2 = 8
		}

		static readonly Dictionary<IROpCode, LiveFlags> opCodeLiveness = new Dictionary<IROpCode, LiveFlags> {
			{ IROpCode.MOV, LiveFlags.KILL1 | LiveFlags.GEN2 },
			{ IROpCode.POP, LiveFlags.KILL1 },
			{ IROpCode.PUSH, LiveFlags.GEN1 },
			{ IROpCode.CALL, LiveFlags.GEN1 | LiveFlags.KILL2 },
			{ IROpCode.NOR, LiveFlags.GEN1 | LiveFlags.GEN2 | LiveFlags.KILL1 },
			{ IROpCode.CMP, LiveFlags.GEN1 | LiveFlags.GEN2 },
			{ IROpCode.JZ, LiveFlags.GEN2 },
			{ IROpCode.JNZ, LiveFlags.GEN2 },
			{ IROpCode.SWT, LiveFlags.GEN2 },
			{ IROpCode.ADD, LiveFlags.GEN1 | LiveFlags.GEN2 | LiveFlags.KILL1 },
			{ IROpCode.SUB, LiveFlags.GEN1 | LiveFlags.GEN2 | LiveFlags.KILL1 },
			{ IROpCode.MUL, LiveFlags.GEN1 | LiveFlags.GEN2 | LiveFlags.KILL1 },
			{ IROpCode.DIV, LiveFlags.GEN1 | LiveFlags.GEN2 | LiveFlags.KILL1 },
			{ IROpCode.REM, LiveFlags.GEN1 | LiveFlags.GEN2 | LiveFlags.KILL1 },
			{ IROpCode.SHR, LiveFlags.GEN1 | LiveFlags.GEN2 | LiveFlags.KILL1 },
			{ IROpCode.SHL, LiveFlags.GEN1 | LiveFlags.GEN2 | LiveFlags.KILL1 },
			{ IROpCode.FCONV, LiveFlags.KILL1 | LiveFlags.GEN2 },
			{ IROpCode.ICONV, LiveFlags.KILL1 | LiveFlags.GEN2 },
			{ IROpCode.SX, LiveFlags.KILL1 | LiveFlags.GEN2 },
			{ IROpCode.VCALL, LiveFlags.GEN1 | LiveFlags.GEN1 },
			{ IROpCode.TRY, LiveFlags.GEN1 | LiveFlags.GEN2 },
			{ IROpCode.LEAVE, LiveFlags.GEN1 },
			{ IROpCode.__EHRET, LiveFlags.GEN1 },
			{ IROpCode.__LEA, LiveFlags.KILL1 | LiveFlags.GEN2 },
			{ IROpCode.__LDOBJ, LiveFlags.GEN1 | LiveFlags.KILL2 },
			{ IROpCode.__STOBJ, LiveFlags.GEN1 | LiveFlags.GEN2 },
			{ IROpCode.__GEN, LiveFlags.GEN1 },
			{ IROpCode.__KILL, LiveFlags.KILL1 }
		};

		#endregion

		public static Dictionary<BasicBlock<IRInstrList>, BlockLiveness> ComputeLiveness(
			IList<BasicBlock<IRInstrList>> blocks) {
			var liveness = new Dictionary<BasicBlock<IRInstrList>, BlockLiveness>();
			var entryBlocks = blocks.Where(block => block.Sources.Count == 0).ToList();
			var order = new List<BasicBlock<IRInstrList>>();
			var visited = new HashSet<BasicBlock<IRInstrList>>();
			foreach (var entry in entryBlocks)
				PostorderTraversal(entry, visited, block => order.Add(block));

			bool worked = false;
			do {
				foreach (var currentBlock in order) {
					var blockLiveness = BlockLiveness.Empty();

					foreach (var successor in currentBlock.Targets) {
						BlockLiveness successorLiveness;
						if (!liveness.TryGetValue(successor, out successorLiveness)) {
							continue;
						}
						blockLiveness.OutLive.UnionWith(successorLiveness.InLive);
					}

					var live = new HashSet<IRVariable>(blockLiveness.OutLive);
					for (int i = currentBlock.Content.Count - 1; i >= 0; i--) {
						var instr = currentBlock.Content[i];
						ComputeInstrLiveness(instr, live);
					}
					blockLiveness.InLive.UnionWith(live);

					BlockLiveness prevLiveness;
					if (!worked && liveness.TryGetValue(currentBlock, out prevLiveness)) {
						worked = !prevLiveness.InLive.SetEquals(blockLiveness.InLive) ||
						         !prevLiveness.OutLive.SetEquals(blockLiveness.OutLive);
					}
					liveness[currentBlock] = blockLiveness;
				}
			} while (worked);

			return liveness;
		}

		public static Dictionary<IRInstruction, HashSet<IRVariable>> ComputeLiveness(
			BasicBlock<IRInstrList> block, BlockLiveness liveness) {
			var ret = new Dictionary<IRInstruction, HashSet<IRVariable>>();
			var live = new HashSet<IRVariable>(liveness.OutLive);
			for (int i = block.Content.Count - 1; i >= 0; i--) {
				var instr = block.Content[i];
				ComputeInstrLiveness(instr, live);
				ret[instr] = new HashSet<IRVariable>(live);
			}
			Debug.Assert(live.SetEquals(liveness.InLive));
			return ret;
		}

		static void PostorderTraversal(
			BasicBlock<IRInstrList> block,
			HashSet<BasicBlock<IRInstrList>> visited,
			Action<BasicBlock<IRInstrList>> visitFunc) {
			visited.Add(block);
			foreach (var successor in block.Targets) {
				if (!visited.Contains(successor))
					PostorderTraversal(successor, visited, visitFunc);
			}
			visitFunc(block);
		}

		static void ComputeInstrLiveness(IRInstruction instr, HashSet<IRVariable> live) {
			LiveFlags flags;
			if (!opCodeLiveness.TryGetValue(instr.OpCode, out flags))
				flags = 0;

			var op1 = instr.Operand1 as IRVariable;
			var op2 = instr.Operand2 as IRVariable;
			if ((flags & LiveFlags.KILL1) != 0 && op1 != null)
				live.Remove(op1);
			if ((flags & LiveFlags.KILL2) != 0 && op2 != null)
				live.Remove(op2);
			if ((flags & LiveFlags.GEN1) != 0 && op1 != null)
				live.Add(op1);
			if ((flags & LiveFlags.GEN2) != 0 && op2 != null)
				live.Add(op2);
		}
	}
}