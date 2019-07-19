using System;
using System.Collections.Generic;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace KoiVM {
	public class GenericInstantiation {
		readonly Dictionary<MethodSpec, MethodDef> instantiations =
			new Dictionary<MethodSpec, MethodDef>(MethodEqualityComparer.CompareDeclaringTypes);

		public event Func<MethodSpec, bool> ShouldInstantiate;

		public void EnsureInstantiation(MethodDef method, Action<MethodSpec, MethodDef> onInstantiated) {
			foreach (var instr in method.Body.Instructions) {
				if (instr.Operand is MethodSpec) {
					var spec = (MethodSpec)instr.Operand;
					if (ShouldInstantiate != null && !ShouldInstantiate(spec))
						continue;

					MethodDef instantiation;
					if (!Instantiate(spec, out instantiation))
						onInstantiated(spec, instantiation);
					instr.Operand = instantiation;
				}
			}
		}

		public bool Instantiate(MethodSpec methodSpec, out MethodDef def) {
			if (instantiations.TryGetValue(methodSpec, out def))
				return true;

			var genericArguments = new GenericArguments();
			genericArguments.PushMethodArgs(methodSpec.GenericInstMethodSig.GenericArguments);
			var originDef = methodSpec.Method.ResolveMethodDefThrow();

			var newSig = ResolveMethod(originDef.MethodSig, genericArguments);
			newSig.Generic = false;
			newSig.GenParamCount = 0;

			string newName = originDef.Name;
			foreach (var typeArg in methodSpec.GenericInstMethodSig.GenericArguments)
				newName += ";" + typeArg.TypeName;

			def = new MethodDefUser(newName, newSig, originDef.ImplAttributes, originDef.Attributes);
			var thisParam = originDef.HasThis ? originDef.Parameters[0].Type : null;
			def.DeclaringType2 = originDef.DeclaringType2;
			if (thisParam != null) {
				def.Parameters[0].Type = thisParam;
			}

			foreach (var declSec in originDef.DeclSecurities)
				def.DeclSecurities.Add(declSec);
			def.ImplMap = originDef.ImplMap;
			foreach (var ov in originDef.Overrides)
				def.Overrides.Add(ov);

			def.Body = new CilBody();
			def.Body.InitLocals = originDef.Body.InitLocals;
			def.Body.MaxStack = originDef.Body.MaxStack;
			foreach (var variable in originDef.Body.Variables) {
				var newVar = new Local(variable.Type);
				def.Body.Variables.Add(newVar);
			}

			var instrMap = new Dictionary<Instruction, Instruction>();
			foreach (var instr in originDef.Body.Instructions) {
				var newInstr = new Instruction(instr.OpCode, ResolveOperand(instr.Operand, genericArguments));
				def.Body.Instructions.Add(newInstr);
				instrMap[instr] = newInstr;
			}
			foreach (var instr in def.Body.Instructions) {
				if (instr.Operand is Instruction)
					instr.Operand = instrMap[(Instruction)instr.Operand];
				else if (instr.Operand is Instruction[]) {
					var targets = (Instruction[])((Instruction[])instr.Operand).Clone();
					for (int i = 0; i < targets.Length; i++)
						targets[i] = instrMap[targets[i]];
					instr.Operand = targets;
				}
			}
			def.Body.UpdateInstructionOffsets();

			foreach (var eh in originDef.Body.ExceptionHandlers) {
				var newEH = new ExceptionHandler(eh.HandlerType);
				newEH.TryStart = instrMap[eh.TryStart];
				newEH.HandlerStart = instrMap[eh.HandlerStart];
				if (eh.TryEnd != null)
					newEH.TryEnd = instrMap[eh.TryEnd];
				if (eh.HandlerEnd != null)
					newEH.HandlerEnd = instrMap[eh.HandlerEnd];
				if (eh.CatchType != null)
					newEH.CatchType = genericArguments.Resolve(newEH.CatchType.ToTypeSig()).ToTypeDefOrRef();
				else if (eh.FilterStart != null)
					newEH.FilterStart = instrMap[eh.FilterStart];

				def.Body.ExceptionHandlers.Add(newEH);
			}

			instantiations[methodSpec] = def;
			return false;
		}

		FieldSig ResolveField(FieldSig sig, GenericArguments genericArgs) {
			var newSig = sig.Clone();
			newSig.Type = genericArgs.ResolveType(newSig.Type);
			return newSig;
		}

		GenericInstMethodSig ResolveInst(GenericInstMethodSig sig, GenericArguments genericArgs) {
			var newSig = sig.Clone();
			for (int i = 0; i < newSig.GenericArguments.Count; i++)
				newSig.GenericArguments[i] = genericArgs.ResolveType(newSig.GenericArguments[i]);
			return newSig;
		}

		MethodSig ResolveMethod(MethodSig sig, GenericArguments genericArgs) {
			var newSig = sig.Clone();

			for (int i = 0; i < newSig.Params.Count; i++)
				newSig.Params[i] = genericArgs.ResolveType(newSig.Params[i]);

			if (newSig.ParamsAfterSentinel != null) {
				for (int i = 0; i < newSig.ParamsAfterSentinel.Count; i++)
					newSig.ParamsAfterSentinel[i] = genericArgs.ResolveType(newSig.ParamsAfterSentinel[i]);
			}

			newSig.RetType = genericArgs.ResolveType(newSig.RetType);
			return newSig;
		}

		object ResolveOperand(object operand, GenericArguments genericArgs) {
			if (operand is MemberRef) {
				var memberRef = (MemberRef)operand;
				if (memberRef.IsFieldRef) {
					var field = ResolveField(memberRef.FieldSig, genericArgs);
					memberRef = new MemberRefUser(memberRef.Module, memberRef.Name, field, memberRef.Class);
				}
				else {
					var method = ResolveMethod(memberRef.MethodSig, genericArgs);
					memberRef = new MemberRefUser(memberRef.Module, memberRef.Name, method, memberRef.Class);
				}
				return memberRef;
			}
			if (operand is TypeSpec) {
				var sig = ((TypeSpec)operand).TypeSig;
				return genericArgs.ResolveType(sig).ToTypeDefOrRef();
			}
			if (operand is MethodSpec) {
				var spec = (MethodSpec)operand;
				spec = new MethodSpecUser(spec.Method, ResolveInst(spec.GenericInstMethodSig, genericArgs));
				return spec;
			}
			return operand;
		}
	}
}