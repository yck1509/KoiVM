using System;
using dnlib.DotNet;
using KoiVM.AST.IR;

namespace KoiVM.VMIR.Transforms {
	public class MetadataTransform : ITransform {
		public void Initialize(IRTransformer tr) {
		}

		public void Transform(IRTransformer tr) {
			tr.Instructions.VisitInstrs(VisitInstr, tr);
		}

		void VisitInstr(IRInstrList instrs, IRInstruction instr, ref int index, IRTransformer tr) {
			instr.Operand1 = TransformMD(instr.Operand1, tr);
			instr.Operand2 = TransformMD(instr.Operand2, tr);
		}

		IIROperand TransformMD(IIROperand operand, IRTransformer tr) {
			if (operand is IRMetaTarget) {
				var target = (IRMetaTarget)operand;
				if (!target.LateResolve) {
					if (!(target.MetadataItem is IMemberRef))
						throw new NotSupportedException();
					return IRConstant.FromI4((int)tr.VM.Data.GetId((IMemberRef)target.MetadataItem));
				}
			}
			return operand;
		}
	}
}