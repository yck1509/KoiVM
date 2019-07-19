using System;
using System.Linq;
using KoiVM.AST.IL;
using KoiVM.VMIL;

namespace KoiVM.Protections.SMC {
	internal class SMCILTransform : ITransform {
		ILBlock trampoline;
		SMCBlock newTrampoline;
		int adrKey;

		public void Initialize(ILTransformer tr) {
			trampoline = null;
			tr.RootScope.ProcessBasicBlocks<ILInstrList>(b => {
				if (b.Content.Any(instr => instr.IR != null && instr.IR.Annotation == SMCBlock.AddressPart2))
					trampoline = (ILBlock)b;
			});
			if (trampoline == null)
				return;

			var scope = tr.RootScope.SearchBlock(trampoline).Last();
			newTrampoline = new SMCBlock(trampoline.Id, trampoline.Content);
			scope.Content[scope.Content.IndexOf(trampoline)] = newTrampoline;

			adrKey = tr.VM.Random.Next();
			newTrampoline.Key = (byte)tr.VM.Random.Next();
		}

		public void Transform(ILTransformer tr) {
			if (tr.Block.Targets.Contains(trampoline))
				tr.Block.Targets[tr.Block.Targets.IndexOf(trampoline)] = newTrampoline;

			if (tr.Block.Sources.Contains(trampoline))
				tr.Block.Sources[tr.Block.Sources.IndexOf(trampoline)] = newTrampoline;

			tr.Instructions.VisitInstrs(VisitInstr, tr);
		}

		void VisitInstr(ILInstrList instrs, ILInstruction instr, ref int index, ILTransformer tr) {
			if (instr.Operand is ILBlockTarget) {
				var target = (ILBlockTarget)instr.Operand;
				if (target.Target == trampoline)
					target.Target = newTrampoline;
			}
			else if (instr.IR == null)
				return;

			if (instr.IR.Annotation == SMCBlock.CounterInit && instr.OpCode == ILOpCode.PUSHI_DWORD) {
				var imm = (ILImmediate)instr.Operand;
				if ((int)imm.Value == 0x0f000001) {
					newTrampoline.CounterOperand = imm;
				}
			}
			else if (instr.IR.Annotation == SMCBlock.EncryptionKey && instr.OpCode == ILOpCode.PUSHI_DWORD) {
				var imm = (ILImmediate)instr.Operand;
				if ((int)imm.Value == 0x0f000002) {
					imm.Value = (int)newTrampoline.Key;
				}
			}
			else if (instr.IR.Annotation == SMCBlock.AddressPart1 && instr.OpCode == ILOpCode.PUSHI_DWORD &&
			         instr.Operand is ILBlockTarget) {
				var target = (ILBlockTarget)instr.Operand;

				var relBase = new ILInstruction(ILOpCode.PUSHR_QWORD, ILRegister.IP, instr);
				instr.OpCode = ILOpCode.PUSHI_DWORD;
				instr.Operand = new SMCBlockRef(target, relBase, (uint)adrKey);

				instrs.Replace(index, new[] {
					relBase,
					instr,
					new ILInstruction(ILOpCode.ADD_QWORD, null, instr)
				});
			}
			else if (instr.IR.Annotation == SMCBlock.AddressPart2 && instr.OpCode == ILOpCode.PUSHI_DWORD) {
				var imm = (ILImmediate)instr.Operand;
				if ((int)imm.Value == 0x0f000003) {
					imm.Value = adrKey;
				}
			}
		}
	}
}