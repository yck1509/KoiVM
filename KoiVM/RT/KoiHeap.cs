using System;
using System.Collections.Generic;
using System.IO;
using dnlib.DotNet.Writer;

namespace KoiVM.RT {
	internal class KoiHeap : HeapBase {
		List<byte[]> chunks = new List<byte[]>();
		uint currentLen;

		public uint AddChunk(byte[] chunk) {
			uint offset = currentLen;
			chunks.Add(chunk);
			currentLen += (uint)chunk.Length;
			return offset;
		}

		public override string Name {
			get { return "#Koi"; }
		}

		public override uint GetRawLength() {
			return currentLen;
		}

		protected override void WriteToImpl(BinaryWriter writer) {
			foreach (var chunk in chunks)
				writer.Write(chunk);
		}
	}
}