using System;
using System.Collections.Generic;
using dnlib.DotNet;
using KoiVM.AST;
using KoiVM.AST.IR;
using KoiVM.VM;

namespace KoiVM.VMIR.Transforms {
	public class InvokeTransform : ITransform {
		public void Initialize(IRTransformer tr) {
		}

		public void Transform(IRTransformer tr) {
			tr.Instructions.VisitInstrs(VisitInstr, tr);
		}

		void VisitInstr(IRInstrList instrs, IRInstruction instr, ref int index, IRTransformer tr) {
			if (instr.OpCode != IROpCode.__CALL && instr.OpCode != IROpCode.__CALLVIRT &&
			    instr.OpCode != IROpCode.__NEWOBJ)
				return;

			var method = ((IMethod)((IRMetaTarget)instr.Operand1).MetadataItem).ResolveMethodDef();
			var callInfo = (InstrCallInfo)instr.Annotation;

			if (method == null ||
			    method.Module != tr.Context.Method.Module || // TODO: cross-module direct call
			    !tr.VM.Settings.IsVirtualized(method) ||
			    instr.OpCode != IROpCode.__CALL) {
				callInfo.IsECall = true;
				ProcessECall(instrs, instr, index, tr);
			}
			else {
				callInfo.IsECall = false;
				ProcessDCall(instrs, instr, index, tr, method);
			}
		}

		// External call
		void ProcessECall(IRInstrList instrs, IRInstruction instr, int index, IRTransformer tr) {
			var method = (IMethod)((IRMetaTarget)instr.Operand1).MetadataItem;
			var retVar = (IRVariable)instr.Operand2;

			uint opCode = 0;
			ITypeDefOrRef constrainType = ((InstrCallInfo)instr.Annotation).ConstrainType;
			if (instr.OpCode == IROpCode.__CALL) {
				opCode = tr.VM.Runtime.VCallOps.ECALL_CALL;
			}
			else if (instr.OpCode == IROpCode.__CALLVIRT) {
				if (constrainType != null)
					opCode = tr.VM.Runtime.VCallOps.ECALL_CALLVIRT_CONSTRAINED;
				else
					opCode = tr.VM.Runtime.VCallOps.ECALL_CALLVIRT;
			}
			else if (instr.OpCode == IROpCode.__NEWOBJ) {
				opCode = tr.VM.Runtime.VCallOps.ECALL_NEWOBJ;
			}

			var methodId = (int)(tr.VM.Data.GetId(method) | opCode << 30);
			var ecallId = tr.VM.Runtime.VMCall.ECALL;
			var callInstrs = new List<IRInstruction>();

			if (constrainType != null) {
				callInstrs.Add(new IRInstruction(IROpCode.PUSH) {
					Operand1 = IRConstant.FromI4((int)tr.VM.Data.GetId(constrainType)),
					Annotation = instr.Annotation,
					ILAST = instr.ILAST
				});
			}
			callInstrs.Add(new IRInstruction(IROpCode.VCALL) {
				Operand1 = IRConstant.FromI4(ecallId),
				Operand2 = IRConstant.FromI4(methodId),
				Annotation = instr.Annotation,
				ILAST = instr.ILAST
			});
			if (retVar != null) {
				callInstrs.Add(new IRInstruction(IROpCode.POP, retVar) {
					Annotation = instr.Annotation,
					ILAST = instr.ILAST
				});
			}
			instrs.Replace(index, callInstrs);
		}

		// Direct call
		void ProcessDCall(IRInstrList instrs, IRInstruction instr, int index, IRTransformer tr, MethodDef method) {
			var retVar = (IRVariable)instr.Operand2;
			var callinfo = (InstrCallInfo)instr.Annotation;
			callinfo.Method = method; // Ensure it's resolved

			var callInstrs = new List<IRInstruction>();
			callInstrs.Add(new IRInstruction(IROpCode.CALL, new IRMetaTarget(method) { LateResolve = true }) {
				Annotation = instr.Annotation,
				ILAST = instr.ILAST
			});
			if (retVar != null) {
				callInstrs.Add(new IRInstruction(IROpCode.MOV, retVar, new IRRegister(VMRegisters.R0, retVar.Type)) {
					Annotation = instr.Annotation,
					ILAST = instr.ILAST
				});
			}
			var stackAdjust = -callinfo.Arguments.Length;
			callInstrs.Add(new IRInstruction(IROpCode.ADD, IRRegister.SP, IRConstant.FromI4(stackAdjust)) {
				Annotation = instr.Annotation,
				ILAST = instr.ILAST
			});

			instrs.Replace(index, callInstrs);
		}
	}
}