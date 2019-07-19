using System;
using dnlib.DotNet;
using KoiVM.AST;
using KoiVM.AST.IL;
using KoiVM.RT;

namespace KoiVM.Protections.SMC {
	internal class SMCBlock : ILBlock {
		public SMCBlock(int id, ILInstrList content)
			: base(id, content) {
		}

		internal static readonly InstrAnnotation CounterInit = new InstrAnnotation("SMC_COUNTER");
		internal static readonly InstrAnnotation EncryptionKey = new InstrAnnotation("SMC_KEY");
		internal static readonly InstrAnnotation AddressPart1 = new InstrAnnotation("SMC_PART1");
		internal static readonly InstrAnnotation AddressPart2 = new InstrAnnotation("SMC_PART2");

		public byte Key { get; set; }
		public ILImmediate CounterOperand { get; set; }

		public override IKoiChunk CreateChunk(VMRuntime rt, MethodDef method) {
			return new SMCBlockChunk(rt, method, this);
		}
	}

	internal class SMCBlockChunk : BasicBlockChunk, IKoiChunk {
		public SMCBlockChunk(VMRuntime rt, MethodDef method, SMCBlock block)
			: base(rt, method, block) {
			block.CounterOperand.Value = Length + 1;
		}

		uint IKoiChunk.Length {
			get { return base.Length + 1; }
		}

		void IKoiChunk.OnOffsetComputed(uint offset) {
			base.OnOffsetComputed(offset + 1);
		}

		byte[] IKoiChunk.GetData() {
			byte[] data = GetData();
			byte[] newData = new byte[data.Length + 1];
			byte key = ((SMCBlock)Block).Key;

			for (int i = 0; i < data.Length; i++)
				newData[i + 1] = (byte)(data[i] ^ key);
			newData[0] = key;
			return newData;
		}
	}

	internal class SMCBlockRef : ILRelReference {
		public SMCBlockRef(IHasOffset target, IHasOffset relBase, uint key)
			: base(target, relBase) {
			Key = key;
		}

		public uint Key { get; set; }

		public override uint Resolve(VMRuntime runtime) {
			return base.Resolve(runtime) ^ Key;
		}
	}
}