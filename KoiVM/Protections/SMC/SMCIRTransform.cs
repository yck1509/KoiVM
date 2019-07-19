using System;
using System.Linq;
using dnlib.DotNet;
using KoiVM.AST;
using KoiVM.AST.IR;
using KoiVM.CFG;
using KoiVM.Protections.SMC;
using KoiVM.VMIR;

namespace KoiVM.Protections {
	public class SMCIRTransform : ITransform {
		bool doWork = true;

		public void Initialize(IRTransformer tr) {
			if (tr.Context.Method.Module == null) {
				doWork = false;
				return;
			}

			var counter = tr.Context.AllocateVRegister(ASTType.I4);
			var pointer = tr.Context.AllocateVRegister(ASTType.Ptr);
			var t1 = tr.Context.AllocateVRegister(ASTType.I4);
			var t2 = tr.Context.AllocateVRegister(ASTType.I4);
			var t3 = tr.Context.AllocateVRegister(ASTType.I4);

			var entry = (BasicBlock<IRInstrList>)tr.RootScope.GetBasicBlocks().First();

			var entryStub = new BasicBlock<IRInstrList>(-3, new IRInstrList());
			var loopBody = new BasicBlock<IRInstrList>(-2, new IRInstrList());
			var trampoline = new BasicBlock<IRInstrList>(-1, new IRInstrList());

			var byteType = tr.Context.Method.Module.CorLibTypes.Byte.ToTypeDefOrRef();

			entryStub.Content.AddRange(new[] {
				new IRInstruction(IROpCode.MOV, counter, IRConstant.FromI4(0x0f000001), SMCBlock.CounterInit),
				new IRInstruction(IROpCode.MOV, pointer, new IRBlockTarget(trampoline)),
				new IRInstruction(IROpCode.ADD, pointer, IRConstant.FromI4(-1)),
				new IRInstruction(IROpCode.__LDOBJ, pointer, t1, new PointerInfo("LDOBJ", byteType)),
				new IRInstruction(IROpCode.CMP, t1, IRConstant.FromI4(0)),
				new IRInstruction(IROpCode.__GETF, t1, IRConstant.FromI4(1 << tr.VM.Architecture.Flags.ZERO)),
				new IRInstruction(IROpCode.JNZ, new IRBlockTarget(trampoline), t1),
				new IRInstruction(IROpCode.CMP, counter, IRConstant.FromI4(0)),
				new IRInstruction(IROpCode.__GETF, t1, IRConstant.FromI4(1 << tr.VM.Architecture.Flags.ZERO)),
				new IRInstruction(IROpCode.JZ, new IRBlockTarget(loopBody), t1),
				new IRInstruction(IROpCode.MOV, t3, new IRBlockTarget(entry), SMCBlock.AddressPart1),
				new IRInstruction(IROpCode.JMP, new IRBlockTarget(trampoline)),
			});
			entryStub.LinkTo(loopBody);
			entryStub.LinkTo(trampoline);

			loopBody.Content.AddRange(new[] {
				new IRInstruction(IROpCode.__LDOBJ, pointer, t2, new PointerInfo("LDOBJ", byteType)),
				new IRInstruction(IROpCode.__XOR, t2, IRConstant.FromI4(0x0f000002), SMCBlock.EncryptionKey),
				new IRInstruction(IROpCode.__STOBJ, pointer, t2, new PointerInfo("STOBJ", byteType)),
				new IRInstruction(IROpCode.ADD, counter, IRConstant.FromI4(-1)),
				new IRInstruction(IROpCode.ADD, pointer, IRConstant.FromI4(1)),
				new IRInstruction(IROpCode.CMP, counter, IRConstant.FromI4(0)),
				new IRInstruction(IROpCode.__GETF, t2, IRConstant.FromI4(1 << tr.VM.Architecture.Flags.ZERO)),
				new IRInstruction(IROpCode.JZ, new IRBlockTarget(loopBody), t2),
				new IRInstruction(IROpCode.MOV, t3, new IRBlockTarget(entry), SMCBlock.AddressPart1),
				new IRInstruction(IROpCode.NOP),
				new IRInstruction(IROpCode.JMP, new IRBlockTarget(trampoline)),
			});
			loopBody.LinkTo(loopBody);
			loopBody.LinkTo(trampoline);

			trampoline.Content.AddRange(new[] {
				new IRInstruction(IROpCode.NOP),
				new IRInstruction(IROpCode.NOP),
				new IRInstruction(IROpCode.__XOR, t3, IRConstant.FromI4(0x0f000003), SMCBlock.AddressPart2),
				new IRInstruction(IROpCode.JMP, new IRBlockTarget(entry)),
			});
			trampoline.LinkTo(entry);

			var scope = tr.RootScope.SearchBlock(entry).Last();
			scope.Content.Insert(0, entryStub);
			scope.Content.Insert(1, loopBody);
			scope.Content.Insert(2, trampoline);
		}

		public void Transform(IRTransformer tr) {
			if (doWork)
				tr.Block.Id += 3;
		}
	}
}