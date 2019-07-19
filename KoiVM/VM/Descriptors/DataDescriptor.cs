using System;
using System.Collections.Generic;
using dnlib.DotNet;

namespace KoiVM.VM {
	public class DataDescriptor {
		internal class FuncSigDesc {
			public readonly uint Id;
			public readonly MethodDef Method;
			public readonly ITypeDefOrRef DeclaringType;
			public readonly MethodSig Signature;
			public readonly FuncSig FuncSig;

			public FuncSigDesc(uint id, MethodDef method) {
				Id = id;
				Method = method;
				DeclaringType = method.DeclaringType;
				Signature = method.MethodSig;
				FuncSig = new FuncSig();
			}

			public FuncSigDesc(uint id, ITypeDefOrRef declType, MethodSig sig) {
				Id = id;
				Method = null;
				DeclaringType = declType;
				Signature = sig;
				FuncSig = new FuncSig();
			}
		}

		internal Dictionary<IMemberRef, uint> refMap = new Dictionary<IMemberRef, uint>();
		internal Dictionary<string, uint> strMap = new Dictionary<string, uint>(StringComparer.Ordinal);
		Dictionary<MethodDef, uint> exportMap = new Dictionary<MethodDef, uint>();
		Dictionary<MethodSig, uint> sigMap = new Dictionary<MethodSig, uint>(SignatureEqualityComparer.Instance);
		internal List<FuncSigDesc> sigs = new List<FuncSigDesc>();
		Dictionary<MethodDef, VMMethodInfo> methodInfos = new Dictionary<MethodDef, VMMethodInfo>();

		uint nextRefId;
		uint nextStrId;
		uint nextSigId;
		Random random;

		public DataDescriptor(Random random) {
			// 0 = null, 1 = ""
			strMap[""] = 1;
			nextStrId = 2;

			nextRefId = 1;
			nextSigId = 1;

			this.random = random;
		}

		public uint GetId(IMemberRef memberRef) {
			uint ret;
			if (!refMap.TryGetValue(memberRef, out ret))
				refMap[memberRef] = ret = nextRefId++;
			return ret;
		}

		public void ReplaceReference(IMemberRef old, IMemberRef @new) {
			uint id;
			if (!refMap.TryGetValue(old, out id))
				return;
			refMap.Remove(old);
			refMap[@new] = id;
		}

		public uint GetId(string str) {
			uint ret;
			if (!strMap.TryGetValue(str, out ret))
				strMap[str] = ret = nextStrId++;
			return ret;
		}

		public uint GetId(ITypeDefOrRef declType, MethodSig methodSig) {
			uint ret;
			if (!sigMap.TryGetValue(methodSig, out ret)) {
				var id = nextSigId++;
				sigMap[methodSig] = ret = id;
				sigs.Add(new FuncSigDesc(id, declType, methodSig));
			}
			return ret;
		}

		public uint GetExportId(MethodDef method) {
			uint ret;
			if (!exportMap.TryGetValue(method, out ret)) {
				var id = nextSigId++;
				exportMap[method] = ret = id;
				sigs.Add(new FuncSigDesc(id, method));
			}
			return ret;
		}

		public VMMethodInfo LookupInfo(MethodDef method) {
			VMMethodInfo ret;
			if (!methodInfos.TryGetValue(method, out ret)) {
				var k = random.Next();
				ret = new VMMethodInfo {
					EntryKey = (byte)k,
					ExitKey = (byte)(k >> 8)
				};
				methodInfos[method] = ret;
			}
			return ret;
		}

		public void SetInfo(MethodDef method, VMMethodInfo info) {
			methodInfos[method] = info;
		}
	}
}