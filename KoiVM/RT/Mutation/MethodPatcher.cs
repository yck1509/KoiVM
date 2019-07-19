using System;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace KoiVM.RT.Mutation {
	internal class MethodPatcher {
		MethodDef vmEntryNormal;
		MethodDef vmEntryTyped;

		public MethodPatcher(ModuleDef rtModule) {
			foreach (var entry in rtModule.Find(RTMap.VMEntry, true).FindMethods(RTMap.VMRun)) {
				if (entry.Parameters.Count == 3)
					vmEntryNormal = entry;
				else
					vmEntryTyped = entry;
			}
		}

		static bool ShouldBeTyped(MethodDef method) {
			if (!method.IsStatic && method.DeclaringType.IsValueType)
				return true;
			foreach (var param in method.Parameters) {
				if (param.Type.IsByRef)
					return true;
			}
			if (method.ReturnType.IsByRef)
				return true;
			return false;
		}

		public void PatchMethodStub(MethodDef method, uint id) {
			if (ShouldBeTyped(method))
				PatchTyped(method.Module, method, (int)id);
			else
				PatchNormal(method.Module, method, (int)id);
		}

		void PatchNormal(ModuleDef module, MethodDef method, int id) {
			var body = new CilBody();
			method.Body = body;

			body.Instructions.Add(Instruction.Create(OpCodes.Ldtoken, method.DeclaringType));
			body.Instructions.Add(Instruction.Create(OpCodes.Ldc_I4, id));
			body.Instructions.Add(Instruction.Create(OpCodes.Ldc_I4, method.Parameters.Count));
			body.Instructions.Add(Instruction.Create(OpCodes.Newarr, method.Module.CorLibTypes.Object.ToTypeDefOrRef()));

			foreach (var param in method.Parameters) {
				body.Instructions.Add(Instruction.Create(OpCodes.Dup));
				body.Instructions.Add(Instruction.Create(OpCodes.Ldc_I4, param.Index));
				body.Instructions.Add(Instruction.Create(OpCodes.Ldarg, param));
				if (param.Type.IsValueType)
					body.Instructions.Add(Instruction.Create(OpCodes.Box, param.Type.ToTypeDefOrRef()));
				else if (param.Type.IsPointer) {
					body.Instructions.Add(Instruction.Create(OpCodes.Conv_U));
					body.Instructions.Add(Instruction.Create(OpCodes.Box, method.Module.CorLibTypes.UIntPtr.ToTypeDefOrRef()));
				}
				body.Instructions.Add(Instruction.Create(OpCodes.Stelem_Ref));
			}
			body.Instructions.Add(Instruction.Create(OpCodes.Call, method.Module.Import(vmEntryNormal)));
			if (method.ReturnType.ElementType == ElementType.Void)
				body.Instructions.Add(Instruction.Create(OpCodes.Pop));
			else if (method.ReturnType.IsValueType)
				body.Instructions.Add(Instruction.Create(OpCodes.Unbox_Any, method.ReturnType.ToTypeDefOrRef()));
			else
				body.Instructions.Add(Instruction.Create(OpCodes.Castclass, method.ReturnType.ToTypeDefOrRef()));
			body.Instructions.Add(Instruction.Create(OpCodes.Ret));

			body.OptimizeMacros();
		}

		void PatchTyped(ModuleDef module, MethodDef method, int id) {
			var body = new CilBody();
			method.Body = body;

			body.Instructions.Add(Instruction.Create(OpCodes.Ldtoken, method.DeclaringType));
			body.Instructions.Add(Instruction.Create(OpCodes.Ldc_I4, id));
			body.Instructions.Add(Instruction.Create(OpCodes.Ldc_I4, method.Parameters.Count));
			body.Instructions.Add(Instruction.Create(OpCodes.Newarr, new PtrSig(method.Module.CorLibTypes.Void).ToTypeDefOrRef()));

			foreach (var param in method.Parameters) {
				body.Instructions.Add(Instruction.Create(OpCodes.Dup));
				body.Instructions.Add(Instruction.Create(OpCodes.Ldc_I4, param.Index));
				if (param.Type.IsByRef) {
					body.Instructions.Add(Instruction.Create(OpCodes.Ldarg, param));
					body.Instructions.Add(Instruction.Create(OpCodes.Mkrefany, param.Type.Next.ToTypeDefOrRef()));
				}
				else {
					body.Instructions.Add(Instruction.Create(OpCodes.Ldarga, param));
					body.Instructions.Add(Instruction.Create(OpCodes.Mkrefany, param.Type.ToTypeDefOrRef()));
				}
				var local = new Local(method.Module.CorLibTypes.TypedReference);
				body.Variables.Add(local);
				body.Instructions.Add(Instruction.Create(OpCodes.Stloc, local));
				body.Instructions.Add(Instruction.Create(OpCodes.Ldloca, local));
				body.Instructions.Add(Instruction.Create(OpCodes.Conv_I));
				body.Instructions.Add(Instruction.Create(OpCodes.Stelem_I));
			}

			if (method.ReturnType.GetElementType() != ElementType.Void) {
				var retVar = new Local(method.ReturnType);
				var retRef = new Local(method.Module.CorLibTypes.TypedReference);
				body.Variables.Add(retVar);
				body.Variables.Add(retRef);
				body.Instructions.Add(Instruction.Create(OpCodes.Ldloca, retVar));
				body.Instructions.Add(Instruction.Create(OpCodes.Mkrefany, method.ReturnType.ToTypeDefOrRef()));
				body.Instructions.Add(Instruction.Create(OpCodes.Stloc, retRef));
				body.Instructions.Add(Instruction.Create(OpCodes.Ldloca, retRef));
				body.Instructions.Add(Instruction.Create(OpCodes.Call, method.Module.Import(vmEntryTyped)));

				body.Instructions.Add(Instruction.Create(OpCodes.Ldloc, retVar));
			}
			else {
				body.Instructions.Add(Instruction.Create(OpCodes.Ldnull));
				body.Instructions.Add(Instruction.Create(OpCodes.Call, method.Module.Import(vmEntryTyped)));
			}
			body.Instructions.Add(Instruction.Create(OpCodes.Ret));

			body.OptimizeMacros();
		}
	}
}