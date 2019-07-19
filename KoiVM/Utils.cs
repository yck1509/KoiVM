using System;
using System.Collections.Generic;
using System.IO;
using dnlib.DotNet;
using KoiVM.AST.IR;
using KoiVM.VM;

namespace KoiVM {
	public static class Utils {
		public static void Increment<T>(this Dictionary<T, int> self, T key) {
			int count;
			if (!self.TryGetValue(key, out count))
				count = 0;
			self[key] = ++count;
		}

		public static void Shuffle<T>(this Random random, IList<T> list) {
			int n = list.Count;
			while (n > 1) {
				n--;
				int k = random.Next(n + 1);
				T value = list[k];
				list[k] = list[n];
				list[n] = value;
			}
		}

		public static void Replace<T>(this List<T> list, int index, IEnumerable<T> newItems) {
			list.RemoveAt(index);
			list.InsertRange(index, newItems);
		}

		public static void Replace(this List<IRInstruction> list, int index, IEnumerable<IRInstruction> newItems) {
			var instr = list[index];
			list.RemoveAt(index);
			foreach (var i in newItems)
				i.ILAST = instr.ILAST;
			list.InsertRange(index, newItems);
		}

		public static bool IsGPR(this VMRegisters reg) {
			if (reg >= VMRegisters.R0 && reg <= VMRegisters.R7)
				return true;
			return false;
		}

		public static uint GetCompressedUIntLength(uint value) {
			uint len = 0;
			do {
				value >>= 7;
				len++;
			} while (value != 0);
			return len;
		}

		public static void WriteCompressedUInt(this BinaryWriter writer, uint value) {
			do {
				byte b = (byte)(value & 0x7f);
				value >>= 7;
				if (value != 0)
					b |= 0x80;
				writer.Write(b);
			} while (value != 0);
		}

		public static TypeSig ResolveType(this GenericArguments genericArgs, TypeSig typeSig) {
			switch (typeSig.ElementType) {
				case ElementType.Ptr:
					return new PtrSig(genericArgs.ResolveType(typeSig.Next));

				case ElementType.ByRef:
					return new ByRefSig(genericArgs.ResolveType(typeSig.Next));

				case ElementType.SZArray:
					return new SZArraySig(genericArgs.ResolveType(typeSig.Next));

				case ElementType.Array:
					var arraySig = (ArraySig)typeSig;
					return new ArraySig(genericArgs.ResolveType(typeSig.Next), arraySig.Rank, arraySig.Sizes, arraySig.LowerBounds);

				case ElementType.Pinned:
					return new PinnedSig(genericArgs.ResolveType(typeSig.Next));

				case ElementType.Var:
				case ElementType.MVar:
					return genericArgs.Resolve(typeSig);

				case ElementType.GenericInst:
					var genInst = (GenericInstSig)typeSig;
					var typeArgs = new List<TypeSig>();
					foreach (var arg in genInst.GenericArguments)
						typeArgs.Add(genericArgs.ResolveType(arg));
					return new GenericInstSig(genInst.GenericType, typeArgs);

				case ElementType.CModReqd:
					return new CModReqdSig(((CModReqdSig)typeSig).Modifier, genericArgs.ResolveType(typeSig.Next));

				case ElementType.CModOpt:
					return new CModOptSig(((CModOptSig)typeSig).Modifier, genericArgs.ResolveType(typeSig.Next));

				case ElementType.ValueArray:
					return new ValueArraySig(genericArgs.ResolveType(typeSig.Next), ((ValueArraySig)typeSig).Size);

				case ElementType.Module:
					return new ModuleSig(((ModuleSig)typeSig).Index, genericArgs.ResolveType(typeSig.Next));
			}
			if (typeSig.IsTypeDefOrRef) {
				var s = (TypeDefOrRefSig)typeSig;
				if (s.TypeDefOrRef is TypeSpec)
					throw new NotSupportedException(); // TODO: ?
			}
			return typeSig;
		}
	}
}