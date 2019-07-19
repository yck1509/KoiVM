using System;

namespace KoiVM.RT {
	public class BinaryChunk : IKoiChunk {
		public BinaryChunk(byte[] data) {
			Data = data;
		}

		public byte[] Data { get; private set; }
		public uint Offset { get; private set; }

		public EventHandler<OffsetComputeEventArgs> OffsetComputed;

		uint IKoiChunk.Length {
			get { return (uint)Data.Length; }
		}

		void IKoiChunk.OnOffsetComputed(uint offset) {
			if (OffsetComputed != null)
				OffsetComputed(this, new OffsetComputeEventArgs(offset));
			Offset = offset;
		}

		byte[] IKoiChunk.GetData() {
			return Data;
		}
	}

	public class OffsetComputeEventArgs : EventArgs {
		internal OffsetComputeEventArgs(uint offset) {
			Offset = offset;
		}

		public uint Offset { get; private set; }
	}
}