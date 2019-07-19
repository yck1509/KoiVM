using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using KoiVM.AST.IR;
using KoiVM.CFG;
using KoiVM.VM;

namespace KoiVM.VMIR.RegAlloc {
	public class RegisterAllocator {
		// TODO: Cross basic block allocation
		IRTransformer transformer;
		Dictionary<BasicBlock<IRInstrList>, BlockLiveness> liveness;
		Dictionary<IRVariable, StackSlot> globalVars;
		Dictionary<IRVariable, object> allocation;
		int baseOffset;

		public int LocalSize { get; set; }

		struct StackSlot {
			public readonly int Offset;
			public readonly IRVariable Variable;

			public StackSlot(int offset, IRVariable var) {
				Offset = offset;
				Variable = var;
			}
		}

		class RegisterPool {
			const int NumRegisters = 8;

			static VMRegisters ToRegister(int regId) {
				return (VMRegisters)regId;
			}

			static int FromRegister(VMRegisters reg) {
				return (int)reg;
			}

			IRVariable[] regAlloc;
			Dictionary<IRVariable, StackSlot> spillVars;

			public int SpillOffset { get; set; }

			public static RegisterPool Create(int baseOffset, Dictionary<IRVariable, StackSlot> globalVars) {
				var pool = new RegisterPool();
				pool.regAlloc = new IRVariable[NumRegisters];
				pool.spillVars = new Dictionary<IRVariable, StackSlot>(globalVars);
				pool.SpillOffset = baseOffset;
				return pool;
			}

			public VMRegisters? Allocate(IRVariable var) {
				for (int i = 0; i < regAlloc.Length; i++) {
					if (regAlloc[i] == null) {
						regAlloc[i] = var;
						return ToRegister(i);
					}
				}
				return null;
			}

			public void Deallocate(IRVariable var, VMRegisters reg) {
				Debug.Assert(regAlloc[FromRegister(reg)] == var);
				regAlloc[FromRegister(reg)] = null;
			}

			public void CheckLiveness(HashSet<IRVariable> live) {
				for (int i = 0; i < regAlloc.Length; i++) {
					if (regAlloc[i] != null && !live.Contains(regAlloc[i])) {
						regAlloc[i].Annotation = null;
						regAlloc[i] = null;
					}
				}
			}

			public StackSlot SpillVariable(IRVariable var) {
				var slot = new StackSlot(SpillOffset++, var);
				spillVars[var] = slot;
				return slot;
			}

			public StackSlot? CheckSpill(IRVariable var) {
				StackSlot ret;
				if (!spillVars.TryGetValue(var, out ret))
					return null;
				return ret;
			}
		}

		public RegisterAllocator(IRTransformer transformer) {
			this.transformer = transformer;
		}

		public void Initialize() {
			var blocks = transformer.RootScope.GetBasicBlocks().Cast<BasicBlock<IRInstrList>>().ToList();
			liveness = LivenessAnalysis.ComputeLiveness(blocks);

			var stackVars = new HashSet<IRVariable>();
			foreach (var blockLiveness in liveness) {
				foreach (var instr in blockLiveness.Key.Content) {
					if (instr.OpCode != IROpCode.__LEA)
						continue;

					var variable = (IRVariable)instr.Operand2;
					if (variable.VariableType != IRVariableType.Argument)
						stackVars.Add(variable);
				}
				stackVars.UnionWith(blockLiveness.Value.OutLive);
			}

			// [BP - 2] = last argument
			// [BP - 1] = return address
			// [BP    ] = old BP
			// [BP + 1] = first local

			int offset = 1;
			globalVars = stackVars.ToDictionary(var => var, var => new StackSlot(offset++, var));
			baseOffset = offset;
			LocalSize = baseOffset - 1;

			offset = -2;
			var parameters = transformer.Context.GetParameters();
			for (int i = parameters.Length - 1; i >= 0; i--) {
				var paramVar = parameters[i];
				globalVars[paramVar] = new StackSlot(offset--, paramVar);
			}

			allocation = globalVars.ToDictionary(pair => pair.Key, pair => (object)pair.Value);
		}

		public void Allocate(BasicBlock<IRInstrList> block) {
			var blockLiveness = liveness[block];
			var instrLiveness = LivenessAnalysis.ComputeLiveness(block, blockLiveness);
			var pool = RegisterPool.Create(baseOffset, globalVars);

			for (int i = 0; i < block.Content.Count; i++) {
				var instr = block.Content[i];
				pool.CheckLiveness(instrLiveness[instr]);

				// Allocates
				if (instr.Operand1 != null)
					instr.Operand1 = AllocateOperand(instr.Operand1, pool);
				if (instr.Operand2 != null)
					instr.Operand2 = AllocateOperand(instr.Operand2, pool);
			}
			if (pool.SpillOffset - 1 > LocalSize)
				LocalSize = pool.SpillOffset - 1;
			baseOffset = pool.SpillOffset;
		}

		IIROperand AllocateOperand(IIROperand operand, RegisterPool pool) {
			if (operand is IRVariable) {
				var variable = (IRVariable)operand;

				StackSlot? slot;
				var reg = AllocateVariable(pool, variable, out slot);
				if (reg != null)
					return new IRRegister(reg.Value) {
						SourceVariable = variable,
						Type = variable.Type
					};
				variable.Annotation = slot.Value;
				return new IRPointer {
					Register = IRRegister.BP,
					Offset = slot.Value.Offset,
					SourceVariable = variable,
					Type = variable.Type
				};
			}
			return operand;
		}

		VMRegisters? AllocateVariable(RegisterPool pool, IRVariable var, out StackSlot? stackSlot) {
			stackSlot = pool.CheckSpill(var);
			if (stackSlot == null) {
				var allocReg = var.Annotation == null ? (VMRegisters?)null : (VMRegisters)var.Annotation;
				if (allocReg == null)
					allocReg = pool.Allocate(var);
				if (allocReg != null) {
					if (var.Annotation == null)
						var.Annotation = allocReg.Value;
					return allocReg;
				}
				// Spill variable
				stackSlot = pool.SpillVariable(var);
			}
			return null;
		}
	}
}