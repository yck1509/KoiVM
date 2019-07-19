using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using KoiVM.VM;
using KoiVM.VMIL;

namespace KoiVM.RT.Mutation {
	public class RTConstants {
		Dictionary<string, int> constants = new Dictionary<string, int>();

		void AddField(string fieldName, int fieldValue) {
			constants[fieldName] = fieldValue;
		}

		void Conclude(Random random, IList<Instruction> instrs, TypeDef constType) {
			var constValues = constants.ToList();
			random.Shuffle(constValues);
			foreach (var c in constValues) {
				instrs.Add(new Instruction(OpCodes.Ldnull));
				instrs.Add(new Instruction(OpCodes.Ldc_I4, c.Value));
				instrs.Add(new Instruction(OpCodes.Stfld, constType.FindField(RTMap.VMConstMap[c.Key])));
			}
		}

		public int? GetConstant(string name) {
			int ret;
			if (!constants.TryGetValue(name, out ret))
				return null;
			return ret;
		}

		public void InjectConstants(ModuleDef rtModule, VMDescriptor desc, RuntimeHelpers helpers) {
			var constants = rtModule.Find(RTMap.VMConstants, true);
			var cctor = constants.FindOrCreateStaticConstructor();
			var instrs = cctor.Body.Instructions;
			instrs.Clear();

			for (int i = 0; i < (int)VMRegisters.Max; i++) {
				var reg = (VMRegisters)i;
				var regId = desc.Architecture.Registers[reg];
				var regField = reg.ToString();
				AddField(regField, regId);
			}

			for (int i = 0; i < (int)VMFlags.Max; i++) {
				var fl = (VMFlags)i;
				var flId = desc.Architecture.Flags[fl];
				var flField = fl.ToString();
				AddField(flField, 1 << flId);
			}

			for (int i = 0; i < (int)ILOpCode.Max; i++) {
				var op = (ILOpCode)i;
				var opId = desc.Architecture.OpCodes[op];
				var opField = op.ToString();
				AddField(opField, opId);
			}

			for (int i = 0; i < (int)VMCalls.Max; i++) {
				var vc = (VMCalls)i;
				var vcId = desc.Runtime.VMCall[vc];
				var vcField = vc.ToString();
				AddField(vcField, vcId);
			}

			AddField(ConstantFields.E_CALL.ToString(), (int)desc.Runtime.VCallOps.ECALL_CALL);
			AddField(ConstantFields.E_CALLVIRT.ToString(), (int)desc.Runtime.VCallOps.ECALL_CALLVIRT);
			AddField(ConstantFields.E_NEWOBJ.ToString(), (int)desc.Runtime.VCallOps.ECALL_NEWOBJ);
			AddField(ConstantFields.E_CALLVIRT_CONSTRAINED.ToString(), (int)desc.Runtime.VCallOps.ECALL_CALLVIRT_CONSTRAINED);

			AddField(ConstantFields.INIT.ToString(), (int)helpers.INIT);

			AddField(ConstantFields.INSTANCE.ToString(), desc.Runtime.RTFlags.INSTANCE);

			AddField(ConstantFields.CATCH.ToString(), desc.Runtime.RTFlags.EH_CATCH);
			AddField(ConstantFields.FILTER.ToString(), desc.Runtime.RTFlags.EH_FILTER);
			AddField(ConstantFields.FAULT.ToString(), desc.Runtime.RTFlags.EH_FAULT);
			AddField(ConstantFields.FINALLY.ToString(), desc.Runtime.RTFlags.EH_FINALLY);

			Conclude(desc.Random, instrs, constants);
			instrs.Add(Instruction.Create(OpCodes.Ret));
			cctor.Body.OptimizeMacros();
		}
	}

	[Obfuscation(Exclude = false, ApplyToMembers = false, Feature = "+rename(forceRen=true);")]
	internal enum ConstantFields {
		E_CALL,
		E_CALLVIRT,
		E_NEWOBJ,
		E_CALLVIRT_CONSTRAINED,

		INIT,

		INSTANCE,

		CATCH,
		FILTER,
		FAULT,
		FINALLY,
	}
}