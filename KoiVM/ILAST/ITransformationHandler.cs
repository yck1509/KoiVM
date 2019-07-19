using System;

namespace KoiVM.ILAST {
	public interface ITransformationHandler {
		void Initialize(ILASTTransformer tr);
		void Transform(ILASTTransformer tr);
	}
}