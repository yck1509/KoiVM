using System;
using dnlib.DotNet;

namespace KoiVM.VM {
	public class FuncSig {
		public byte Flags;
		public ITypeDefOrRef[] ParamSigs;
		public ITypeDefOrRef RetType;

		public override int GetHashCode() {
			var comparer = new SigComparer();
			int hashCode = Flags;
			foreach (var param in ParamSigs)
				hashCode = (hashCode * 7) + comparer.GetHashCode(param);
			return (hashCode * 7) + comparer.GetHashCode(RetType);
		}

		public override bool Equals(object obj) {
			var other = obj as FuncSig;
			if (other == null || other.Flags != Flags)
				return false;

			if (other.ParamSigs.Length != ParamSigs.Length)
				return false;
			var comparer = new SigComparer();
			for (int i = 0; i < ParamSigs.Length; i++) {
				if (!comparer.Equals(ParamSigs[i], other.ParamSigs[i]))
					return false;
			}
			if (!comparer.Equals(RetType, other.RetType))
				return false;
			return true;
		}
	}
}