using System;

namespace KoiVM.VMIL {
	public interface ITransform {
		void Initialize(ILTransformer tr);
		void Transform(ILTransformer tr);
	}
}