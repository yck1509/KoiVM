using System;

namespace KoiVM.VMIR {
	public interface ITransform {
		void Initialize(IRTransformer tr);
		void Transform(IRTransformer tr);
	}
}