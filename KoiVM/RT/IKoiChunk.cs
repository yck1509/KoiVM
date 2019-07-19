using System;

namespace KoiVM.RT {
	public interface IKoiChunk {
		uint Length { get; }

		void OnOffsetComputed(uint offset);
		byte[] GetData();
	}
}