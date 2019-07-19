using System;

namespace KoiVM.Runtime.Execution {
	internal class EHState {
		public enum EHProcess {
			Searching, // Search for handler, filter are executed
			Unwinding // Unwind the stack, fault/finally are executed
		}

		public EHProcess CurrentProcess;
		public object ExceptionObj;
		public VMSlot OldBP;
		public VMSlot OldSP;
		public int? CurrentFrame;
		public int? HandlerFrame;
	}
}