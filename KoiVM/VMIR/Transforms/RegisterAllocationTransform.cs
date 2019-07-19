using System;
using KoiVM.VMIR.RegAlloc;

namespace KoiVM.VMIR.Transforms {
	public class RegisterAllocationTransform : ITransform {
		RegisterAllocator allocator;

		public static readonly object RegAllocatorKey = new object();

		public void Initialize(IRTransformer tr) {
			allocator = new RegisterAllocator(tr);
			allocator.Initialize();
			tr.Annotations[RegAllocatorKey] = allocator;
		}

		public void Transform(IRTransformer tr) {
			allocator.Allocate(tr.Block);
		}
	}
}