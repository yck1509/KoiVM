using System;
using KoiVM.AST.IL;

namespace KoiVM.RT {
	public class JumpTableChunk : IKoiChunk {
		internal VMRuntime runtime;

		public JumpTableChunk(ILJumpTable table) {
			Table = table;
			if (table.Targets.Length > ushort.MaxValue)
				throw new NotSupportedException("Jump table too large.");
		}

		public ILJumpTable Table { get; private set; }
		public uint Offset { get; private set; }

		uint IKoiChunk.Length {
			get { return (uint)Table.Targets.Length * 4 + 2; }
		}

		void IKoiChunk.OnOffsetComputed(uint offset) {
			Offset = offset + 2;
		}

		byte[] IKoiChunk.GetData() {
			byte[] data = new byte[Table.Targets.Length * 4 + 2];
			ushort len = (ushort)Table.Targets.Length;
			int ptr = 0;
			data[ptr++] = (byte)Table.Targets.Length;
			data[ptr++] = (byte)(Table.Targets.Length >> 8);

			var relBase = Table.RelativeBase.Offset;
			relBase += runtime.serializer.ComputeLength(Table.RelativeBase);
			for (int i = 0; i < Table.Targets.Length; i++) {
				var offset = ((ILBlock)Table.Targets[i]).Content[0].Offset;
				offset -= relBase;
				data[ptr++] = (byte)(offset >> 0);
				data[ptr++] = (byte)(offset >> 8);
				data[ptr++] = (byte)(offset >> 16);
				data[ptr++] = (byte)(offset >> 24);
			}
			return data;
		}
	}
}