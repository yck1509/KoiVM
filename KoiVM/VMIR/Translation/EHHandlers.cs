using System;
using System.Diagnostics;
using dnlib.DotNet.Emit;
using KoiVM.AST.ILAST;
using KoiVM.AST.IR;
using KoiVM.CFG;

namespace KoiVM.VMIR.Translation {
	public class LeaveHandler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Leave; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			tr.Instructions.Add(new IRInstruction(IROpCode.__LEAVE) {
				Operand1 = new IRBlockTarget((IBasicBlock)expr.Operand)
			});
			tr.Block.Flags |= BlockFlags.ExitEHLeave;
			return null;
		}
	}

	public class EndfilterHandler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Endfilter; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			Debug.Assert(expr.Arguments.Length == 1);
			tr.Instructions.Add(new IRInstruction(IROpCode.__EHRET, tr.Translate(expr.Arguments[0])));
			tr.Block.Flags |= BlockFlags.ExitEHReturn;
			return null;
		}
	}

	public class EndfinallyHandler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Endfinally; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			tr.Instructions.Add(new IRInstruction(IROpCode.__EHRET));
			tr.Block.Flags |= BlockFlags.ExitEHReturn;
			return null;
		}
	}

	public class ThrowHandler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Throw; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			Debug.Assert(expr.Arguments.Length == 1);

			var ecallId = tr.VM.Runtime.VMCall.THROW;
			tr.Instructions.Add(new IRInstruction(IROpCode.PUSH, tr.Translate(expr.Arguments[0])));
			tr.Instructions.Add(new IRInstruction(IROpCode.VCALL) {
				Operand1 = IRConstant.FromI4(ecallId),
				Operand2 = IRConstant.FromI4(0)
			});
			return null;
		}
	}

	public class RethrowHandler : ITranslationHandler {
		public Code ILCode {
			get { return Code.Rethrow; }
		}

		public IIROperand Translate(ILASTExpression expr, IRTranslator tr) {
			Debug.Assert(expr.Arguments.Length == 0);

			var parentScopes = tr.RootScope.SearchBlock(tr.Block);
			var catchScope = parentScopes[parentScopes.Length - 1];
			if (catchScope.Type != ScopeType.Handler ||
			    catchScope.ExceptionHandler.HandlerType != ExceptionHandlerType.Catch)
				throw new InvalidProgramException();

			var exVar = tr.Context.ResolveExceptionVar(catchScope.ExceptionHandler);
			Debug.Assert(exVar != null);

			var ecallId = tr.VM.Runtime.VMCall.THROW;
			tr.Instructions.Add(new IRInstruction(IROpCode.PUSH, exVar));
			tr.Instructions.Add(new IRInstruction(IROpCode.VCALL) {
				Operand1 = IRConstant.FromI4(ecallId),
				Operand2 = IRConstant.FromI4(1)
			});
			return null;
		}
	}
}