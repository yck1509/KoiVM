using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using dnlib.DotNet.Emit;
using KoiVM.AST;
using KoiVM.AST.IR;
using KoiVM.CFG;

namespace KoiVM.VMIR.Transforms {
	public class EHTransform : ITransform {
		public void Initialize(IRTransformer tr) {
		}

		ScopeBlock[] thisScopes;

		public void Transform(IRTransformer tr) {
			thisScopes = tr.RootScope.SearchBlock(tr.Block);
			AddTryStart(tr);
			if (thisScopes[thisScopes.Length - 1].Type == ScopeType.Handler) {
				var tryScope = SearchForTry(tr.RootScope, thisScopes[thisScopes.Length - 1].ExceptionHandler);
				var scopes = tr.RootScope.SearchBlock(tryScope.GetBasicBlocks().First());
				thisScopes = scopes.TakeWhile(s => s != tryScope).ToArray();
			}
			tr.Instructions.VisitInstrs(VisitInstr, tr);
		}

		void SearchForHandlers(ScopeBlock scope, ExceptionHandler eh, ref IBasicBlock handler, ref IBasicBlock filter) {
			if (scope.ExceptionHandler == eh) {
				if (scope.Type == ScopeType.Handler)
					handler = scope.GetBasicBlocks().First();
				else if (scope.Type == ScopeType.Filter)
					filter = scope.GetBasicBlocks().First();
			}
			foreach (var child in scope.Children)
				SearchForHandlers(child, eh, ref handler, ref filter);
		}

		void AddTryStart(IRTransformer tr) {
			var tryStartInstrs = new List<IRInstruction>();
			for (int i = 0; i < thisScopes.Length; i++) {
				var scope = thisScopes[i];
				if (scope.Type != ScopeType.Try)
					continue;
				if (scope.GetBasicBlocks().First() != tr.Block)
					continue;

				// Search for handler/filter
				IBasicBlock handler = null, filter = null;
				SearchForHandlers(tr.RootScope, scope.ExceptionHandler, ref handler, ref filter);
				Debug.Assert(handler != null &&
				             (scope.ExceptionHandler.HandlerType != ExceptionHandlerType.Filter || filter != null));

				// Add instructions
				tryStartInstrs.Add(new IRInstruction(IROpCode.PUSH, new IRBlockTarget(handler)));

				IIROperand tryOperand = null;
				int ehType;
				if (scope.ExceptionHandler.HandlerType == ExceptionHandlerType.Catch) {
					tryOperand = IRConstant.FromI4((int)tr.VM.Data.GetId(scope.ExceptionHandler.CatchType));
					ehType = tr.VM.Runtime.RTFlags.EH_CATCH;
				}
				else if (scope.ExceptionHandler.HandlerType == ExceptionHandlerType.Filter) {
					tryOperand = new IRBlockTarget(filter);
					ehType = tr.VM.Runtime.RTFlags.EH_FILTER;
				}
				else if (scope.ExceptionHandler.HandlerType == ExceptionHandlerType.Fault) {
					ehType = tr.VM.Runtime.RTFlags.EH_FAULT;
				}
				else if (scope.ExceptionHandler.HandlerType == ExceptionHandlerType.Finally) {
					ehType = tr.VM.Runtime.RTFlags.EH_FINALLY;
				}
				else {
					throw new InvalidProgramException();
				}

				tryStartInstrs.Add(new IRInstruction(IROpCode.TRY, IRConstant.FromI4(ehType), tryOperand) {
					Annotation = new EHInfo(scope.ExceptionHandler)
				});
			}
			tr.Instructions.InsertRange(0, tryStartInstrs);
		}

		ScopeBlock SearchForTry(ScopeBlock scope, ExceptionHandler eh) {
			if (scope.ExceptionHandler == eh && scope.Type == ScopeType.Try)
				return scope;
			foreach (var child in scope.Children) {
				var s = SearchForTry(child, eh);
				if (s != null)
					return s;
			}
			return null;
		}


		static ScopeBlock FindCommonAncestor(ScopeBlock[] a, ScopeBlock[] b) {
			ScopeBlock ret = null;
			for (int i = 0; i < a.Length && i < b.Length; i++) {
				if (a[i] == b[i])
					ret = a[i];
				else
					break;
			}
			return ret;
		}

		void VisitInstr(IRInstrList instrs, IRInstruction instr, ref int index, IRTransformer tr) {
			if (instr.OpCode != IROpCode.__LEAVE)
				return;

			var targetScopes = tr.RootScope.SearchBlock(((IRBlockTarget)instr.Operand1).Target);

			var escapeTarget = FindCommonAncestor(thisScopes, targetScopes);
			var leaveInstrs = new List<IRInstruction>();
			for (int i = thisScopes.Length - 1; i >= 0; i--) {
				if (thisScopes[i] == escapeTarget)
					break;
				if (thisScopes[i].Type != ScopeType.Try)
					continue;

				IBasicBlock handler = null, filter = null;
				SearchForHandlers(tr.RootScope, thisScopes[i].ExceptionHandler, ref handler, ref filter);
				if (handler == null)
					throw new InvalidProgramException();

				leaveInstrs.Add(new IRInstruction(IROpCode.LEAVE, new IRBlockTarget(handler)) {
					Annotation = new EHInfo(thisScopes[i].ExceptionHandler)
				});
			}
			instr.OpCode = IROpCode.JMP;
			leaveInstrs.Add(instr);
			instrs.Replace(index, leaveInstrs);
		}
	}
}