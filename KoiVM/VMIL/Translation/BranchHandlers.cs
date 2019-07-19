using System;
using System.Diagnostics;
using KoiVM.AST;
using KoiVM.AST.IL;
using KoiVM.AST.IR;
using KoiVM.VMIR;

namespace KoiVM.VMIL.Translation {
	public class CmpHandler : ITranslationHandler {
		public IROpCode IRCode {
			get { return IROpCode.CMP; }
		}

		public void Translate(IRInstruction instr, ILTranslator tr) {
			tr.PushOperand(instr.Operand1);
			tr.PushOperand(instr.Operand2);

			if (instr.Operand1.Type == ASTType.O || instr.Operand2.Type == ASTType.O)
				tr.Instructions.Add(new ILInstruction(ILOpCode.CMP));

			else if (instr.Operand1.Type == ASTType.I8 || instr.Operand2.Type == ASTType.I8 ||
			         instr.Operand1.Type == ASTType.Ptr || instr.Operand2.Type == ASTType.Ptr)
				tr.Instructions.Add(new ILInstruction(ILOpCode.CMP_QWORD));

			else if (instr.Operand1.Type == ASTType.R8 || instr.Operand2.Type == ASTType.R8)
				tr.Instructions.Add(new ILInstruction(ILOpCode.CMP_R64));

			else if (instr.Operand1.Type == ASTType.R4 || instr.Operand2.Type == ASTType.R4)
				tr.Instructions.Add(new ILInstruction(ILOpCode.CMP_R32));

			else
				tr.Instructions.Add(new ILInstruction(ILOpCode.CMP_DWORD));
		}
	}

	public class JmpHandler : ITranslationHandler {
		public IROpCode IRCode {
			get { return IROpCode.JMP; }
		}

		public void Translate(IRInstruction instr, ILTranslator tr) {
			tr.PushOperand(instr.Operand1);
			tr.Instructions.Add(new ILInstruction(ILOpCode.JMP) {
				Annotation = InstrAnnotation.JUMP
			});
		}
	}

	public class JzHandler : ITranslationHandler {
		public IROpCode IRCode {
			get { return IROpCode.JZ; }
		}

		public void Translate(IRInstruction instr, ILTranslator tr) {
			tr.PushOperand(instr.Operand2);
			tr.PushOperand(instr.Operand1);
			tr.Instructions.Add(new ILInstruction(ILOpCode.JZ) {
				Annotation = InstrAnnotation.JUMP
			});
		}
	}

	public class JnzHandler : ITranslationHandler {
		public IROpCode IRCode {
			get { return IROpCode.JNZ; }
		}

		public void Translate(IRInstruction instr, ILTranslator tr) {
			tr.PushOperand(instr.Operand2);
			tr.PushOperand(instr.Operand1);
			tr.Instructions.Add(new ILInstruction(ILOpCode.JNZ) {
				Annotation = InstrAnnotation.JUMP
			});
		}
	}

	public class SwtHandler : ITranslationHandler {
		public IROpCode IRCode {
			get { return IROpCode.SWT; }
		}

		public void Translate(IRInstruction instr, ILTranslator tr) {
			tr.PushOperand(instr.Operand2);
			tr.PushOperand(instr.Operand1);

			var lastInstr = tr.Instructions[tr.Instructions.Count - 1];
			Debug.Assert(lastInstr.OpCode == ILOpCode.PUSHI_DWORD && lastInstr.Operand is ILJumpTable);

			var switchInstr = new ILInstruction(ILOpCode.SWT) {
				Annotation = InstrAnnotation.JUMP
			};
			tr.Instructions.Add(switchInstr);

			var jmpTable = (ILJumpTable)lastInstr.Operand;
			jmpTable.Chunk.runtime = tr.Runtime;
			jmpTable.RelativeBase = switchInstr;
			tr.Runtime.AddChunk(jmpTable.Chunk);
		}
	}
}