using System;
using System.Diagnostics;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using KoiVM.AST;
using KoiVM.AST.ILAST;
using KoiVM.AST.IR;
using KoiVM.VM;

namespace KoiVM.VMIR.Translation {
	public class CallHandler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Call; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			var callInfo = new InstrCallInfo("CALL") { Method = (IMethod)expr.Operand };

			tr.Instructions.Add(new IRInstruction(IROpCode.__BEGINCALL) {
				Annotation = callInfo
			});

			var args = new IIROperand[expr.Arguments.Length];
			for (int i = 0; i < args.Length; i++) {
				args[i] = tr.Translate(expr.Arguments[i]);
				tr.Instructions.Add(new IRInstruction(IROpCode.PUSH) {
					Operand1 = args[i],
					Annotation = callInfo
				});
			}
			callInfo.Arguments = args;


			IIROperand retVal = null;
			if (expr.Type != null) {
				retVal = tr.Context.AllocateVRegister(expr.Type.Value);
				tr.Instructions.Add(new IRInstruction(IROpCode.__CALL) {
					Operand1 = new IRMetaTarget(callInfo.Method),
					Operand2 = retVal,
					Annotation = callInfo
				});
			}
			else {
				tr.Instructions.Add(new IRInstruction(IROpCode.__CALL) {
					Operand1 = new IRMetaTarget(callInfo.Method),
					Annotation = callInfo
				});
			}
			callInfo.ReturnValue = retVal;

			tr.Instructions.Add(new IRInstruction(IROpCode.__ENDCALL) {
				Annotation = callInfo
			});

			return retVal;
		}
	}

	public class CallvirtHandler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Callvirt; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			var callInfo = new InstrCallInfo("CALLVIRT") { Method = (IMethod)expr.Operand };
			if (expr.Prefixes != null && expr.Prefixes[0].OpCode == OpCodes.Constrained)
				callInfo.ConstrainType = (ITypeDefOrRef)expr.Prefixes[0].Operand;

			tr.Instructions.Add(new IRInstruction(IROpCode.__BEGINCALL) {
				Annotation = callInfo
			});

			var args = new IIROperand[expr.Arguments.Length];
			for (int i = 0; i < args.Length; i++) {
				args[i] = tr.Translate(expr.Arguments[i]);
				tr.Instructions.Add(new IRInstruction(IROpCode.PUSH) {
					Operand1 = args[i],
					Annotation = callInfo
				});
			}
			callInfo.Arguments = args;


			IIROperand retVal = null;
			if (expr.Type != null) {
				retVal = tr.Context.AllocateVRegister(expr.Type.Value);
				tr.Instructions.Add(new IRInstruction(IROpCode.__CALLVIRT) {
					Operand1 = new IRMetaTarget(callInfo.Method),
					Operand2 = retVal,
					Annotation = callInfo
				});
			}
			else {
				tr.Instructions.Add(new IRInstruction(IROpCode.__CALLVIRT) {
					Operand1 = new IRMetaTarget(callInfo.Method),
					Annotation = callInfo
				});
			}
			callInfo.ReturnValue = retVal;

			tr.Instructions.Add(new IRInstruction(IROpCode.__ENDCALL) {
				Annotation = callInfo
			});

			return retVal;
		}
	}

	public class NewobjHandler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Newobj; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			var callInfo = new InstrCallInfo("NEWOBJ") { Method = (IMethod)expr.Operand };

			tr.Instructions.Add(new IRInstruction(IROpCode.__BEGINCALL) {
				Annotation = callInfo
			});

			var args = new IIROperand[expr.Arguments.Length];
			for (int i = 0; i < args.Length; i++) {
				args[i] = tr.Translate(expr.Arguments[i]);
				tr.Instructions.Add(new IRInstruction(IROpCode.PUSH) {
					Operand1 = args[i],
					Annotation = callInfo
				});
			}
			callInfo.Arguments = args;


			var retVal = tr.Context.AllocateVRegister(expr.Type.Value);
			tr.Instructions.Add(new IRInstruction(IROpCode.__NEWOBJ) {
				Operand1 = new IRMetaTarget(callInfo.Method),
				Operand2 = retVal,
				Annotation = callInfo
			});
			callInfo.ReturnValue = retVal;

			tr.Instructions.Add(new IRInstruction(IROpCode.__ENDCALL) {
				Annotation = callInfo
			});

			return retVal;
		}
	}

	public class RetHandler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Ret; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			if (expr.Arguments.Length == 1) {
				var value = tr.Translate(expr.Arguments[0]);
				tr.Instructions.Add(new IRInstruction(IROpCode.MOV) {
					Operand1 = new IRRegister(VMRegisters.R0, value.Type),
					Operand2 = value
				});
			}
			else
				Debug.Assert(expr.Arguments.Length == 0);
			tr.Instructions.Add(new IRInstruction(IROpCode.RET));

			return null;
		}
	}

	public class CalliHandler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Calli; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			throw new NotSupportedException();
		}
	}
}