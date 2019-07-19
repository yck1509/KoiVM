using System;
using System.Collections.Generic;
using dnlib.DotNet;
using KoiVM.AST;
using KoiVM.AST.IR;

namespace KoiVM.VMIR.Transforms {
	public class InitLocalTransform : ITransform {
		bool done;

		public void Initialize(IRTransformer tr) {
		}

		public void Transform(IRTransformer tr) {
			if (!tr.Context.Method.Body.InitLocals)
				return;
			tr.Instructions.VisitInstrs(VisitInstr, tr);
		}

		void VisitInstr(IRInstrList instrs, IRInstruction instr, ref int index, IRTransformer tr) {
			if (instr.OpCode == IROpCode.__ENTRY && !done) {
				var init = new List<IRInstruction>();
				init.Add(instr);
				foreach (var local in tr.Context.Method.Body.Variables) {
					if (local.Type.IsValueType && !local.Type.IsPrimitive) {
						var adr = tr.Context.AllocateVRegister(ASTType.ByRef);
						init.Add(new IRInstruction(IROpCode.__LEA, adr, tr.Context.ResolveLocal(local)));

						var typeId = (int)tr.VM.Data.GetId(local.Type.RemovePinnedAndModifiers().ToTypeDefOrRef());
						var ecallId = tr.VM.Runtime.VMCall.INITOBJ;
						init.Add(new IRInstruction(IROpCode.PUSH, adr));
						init.Add(new IRInstruction(IROpCode.VCALL, IRConstant.FromI4(ecallId), IRConstant.FromI4(typeId)));
					}
				}
				instrs.Replace(index, init);
				done = true;
			}
		}
	}
}