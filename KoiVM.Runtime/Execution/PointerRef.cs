using System;
using KoiVM.Runtime.Execution.Internal;

namespace KoiVM.Runtime.Execution {
	internal unsafe class PointerRef : IReference {
		// Only for typed reference use

		void* ptr;

		public PointerRef(void* ptr) {
			this.ptr = ptr;
		}

		public VMSlot GetValue(VMContext ctx, PointerType type) {
			throw new NotSupportedException();
		}

		public void SetValue(VMContext ctx, VMSlot slot, PointerType type) {
			throw new NotSupportedException();
		}

		public IReference Add(uint value) {
			throw new NotSupportedException();
		}

		public IReference Add(ulong value) {
			throw new NotSupportedException();
		}

		public void ToTypedReference(VMContext ctx, TypedRefPtr typedRef, Type type) {
			TypedReferenceHelpers.MakeTypedRef(ptr, typedRef, type);
		}
	}
}