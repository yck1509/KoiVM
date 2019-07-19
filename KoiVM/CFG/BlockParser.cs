using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.DotNet.Pdb;

namespace KoiVM.CFG {
	public class BlockParser {
		public static ScopeBlock Parse(MethodDef method, CilBody body) {
			body.SimplifyMacros(method.Parameters);
			ExpandSequencePoints(body);
			HashSet<Instruction> headers, entries;
			FindHeaders(body, out headers, out entries);
			var blocks = SplitBlocks(body, headers, entries);
			LinkBlocks(blocks);
			return AssignScopes(body, blocks);
		}

		static void ExpandSequencePoints(CilBody body) {
			SequencePoint current = null;
			foreach (var instr in body.Instructions) {
				if (instr.SequencePoint != null)
					current = instr.SequencePoint;
				else
					instr.SequencePoint = current;
			}
		}

		static void FindHeaders(CilBody body, out HashSet<Instruction> headers, out HashSet<Instruction> entries) {
			headers = new HashSet<Instruction>();
			entries = new HashSet<Instruction>();

			foreach (var eh in body.ExceptionHandlers) {
				headers.Add(eh.TryStart);
				if (eh.TryEnd != null)
					headers.Add(eh.TryEnd);

				headers.Add(eh.HandlerStart);
				entries.Add(eh.HandlerStart);
				if (eh.HandlerEnd != null)
					headers.Add(eh.HandlerEnd);

				if (eh.FilterStart != null) {
					headers.Add(eh.FilterStart);
					entries.Add(eh.FilterStart);
				}
			}

			var instrs = body.Instructions;
			for (int i = 0; i < instrs.Count; i++) {
				var instr = instrs[i];

				if (instr.Operand is Instruction) {
					headers.Add((Instruction)instr.Operand);
					if (i + 1 < body.Instructions.Count)
						headers.Add(body.Instructions[i + 1]);
				}
				else if (instr.Operand is Instruction[]) {
					foreach (Instruction target in (Instruction[])instr.Operand)
						headers.Add(target);
					if (i + 1 < body.Instructions.Count)
						headers.Add(body.Instructions[i + 1]);
				}
				else if ((instr.OpCode.FlowControl == FlowControl.Throw || instr.OpCode.FlowControl == FlowControl.Return) &&
				         i + 1 < body.Instructions.Count) {
					headers.Add(body.Instructions[i + 1]);
				}
			}
			if (instrs.Count > 0) {
				headers.Add(instrs[0]);
				entries.Add(instrs[0]);
			}
		}

		static List<BasicBlock<CILInstrList>> SplitBlocks(CilBody body, HashSet<Instruction> headers,
			HashSet<Instruction> entries) {
			int nextBlockId = 0;
			int currentBlockId = -1;
			Instruction currentBlockHdr = null;
			var blocks = new List<BasicBlock<CILInstrList>>();

			var instrList = new CILInstrList();
			for (int i = 0; i < body.Instructions.Count; i++) {
				Instruction instr = body.Instructions[i];
				if (headers.Contains(instr)) {
					if (currentBlockHdr != null) {
						Instruction footer = body.Instructions[i - 1];

						Debug.Assert(instrList.Count > 0);
						blocks.Add(new BasicBlock<CILInstrList>(currentBlockId, instrList));
						instrList = new CILInstrList();
					}

					currentBlockId = nextBlockId++;
					currentBlockHdr = instr;
				}

				instrList.Add(instr);
			}
			if (blocks.Count == 0 || blocks[blocks.Count - 1].Id != currentBlockId) {
				Instruction footer = body.Instructions[body.Instructions.Count - 1];

				Debug.Assert(instrList.Count > 0);
				blocks.Add(new BasicBlock<CILInstrList>(currentBlockId, instrList));
			}
			return blocks;
		}

		static void LinkBlocks(List<BasicBlock<CILInstrList>> blocks) {
			var instrMap = blocks
				.SelectMany(block => block.Content.Select(instr => new { Instr = instr, Block = block }))
				.ToDictionary(instr => instr.Instr, instr => instr.Block);

			foreach (var block in blocks)
				foreach (var instr in block.Content) {
					if (instr.Operand is Instruction) {
						var dstBlock = instrMap[(Instruction)instr.Operand];
						dstBlock.Sources.Add(block);
						block.Targets.Add(dstBlock);
					}
					else if (instr.Operand is Instruction[]) {
						foreach (Instruction target in (Instruction[])instr.Operand) {
							var dstBlock = instrMap[target];
							dstBlock.Sources.Add(block);
							block.Targets.Add(dstBlock);
						}
					}
				}
			for (int i = 0; i < blocks.Count; i++) {
				var footer = blocks[i].Content.Last();
				if (footer.OpCode.FlowControl != FlowControl.Branch &&
				    footer.OpCode.FlowControl != FlowControl.Return &&
				    footer.OpCode.FlowControl != FlowControl.Throw &&
				    i + 1 < blocks.Count) {
					var src = blocks[i];
					var dst = blocks[i + 1];
					if (!src.Targets.Contains(dst)) {
						src.Targets.Add(dst);
						dst.Sources.Add(src);
						src.Content.Add(Instruction.Create(OpCodes.Br, dst.Content[0]));
					}
				}
			}
		}

		static ScopeBlock AssignScopes(CilBody body, List<BasicBlock<CILInstrList>> blocks) {
			var ehScopes = new Dictionary<ExceptionHandler, Tuple<ScopeBlock, ScopeBlock, ScopeBlock>>();
			foreach (var eh in body.ExceptionHandlers) {
				var tryBlock = new ScopeBlock(ScopeType.Try, eh);
				var handlerBlock = new ScopeBlock(ScopeType.Handler, eh);

				if (eh.FilterStart != null) {
					var filterBlock = new ScopeBlock(ScopeType.Filter, eh);
					ehScopes[eh] = Tuple.Create(tryBlock, handlerBlock, filterBlock);
				}
				else
					ehScopes[eh] = Tuple.Create(tryBlock, handlerBlock, (ScopeBlock)null);
			}

			var root = new ScopeBlock();
			var scopeStack = new Stack<ScopeBlock>();
			scopeStack.Push(root);

			foreach (var block in blocks) {
				var header = block.Content[0];

				foreach (ExceptionHandler eh in body.ExceptionHandlers) {
					Tuple<ScopeBlock, ScopeBlock, ScopeBlock> ehScope = ehScopes[eh];

					if (header == eh.TryEnd) {
						var pop = scopeStack.Pop();
						Debug.Assert(pop == ehScope.Item1);
					}

					if (header == eh.HandlerEnd) {
						var pop = scopeStack.Pop();
						Debug.Assert(pop == ehScope.Item2);
					}

					if (eh.FilterStart != null && header == eh.HandlerStart) {
						// Filter must precede handler immediately
						Debug.Assert(scopeStack.Peek().Type == ScopeType.Filter);
						var pop = scopeStack.Pop();
						Debug.Assert(pop == ehScope.Item3);
					}
				}
				foreach (ExceptionHandler eh in body.ExceptionHandlers.Reverse()) {
					Tuple<ScopeBlock, ScopeBlock, ScopeBlock> ehScope = ehScopes[eh];
					ScopeBlock parent = scopeStack.Count > 0 ? scopeStack.Peek() : null;

					if (header == eh.TryStart) {
						if (parent != null)
							AddScopeBlock(parent, ehScope.Item1);
						scopeStack.Push(ehScope.Item1);
					}

					if (header == eh.HandlerStart) {
						if (parent != null)
							AddScopeBlock(parent, ehScope.Item2);
						scopeStack.Push(ehScope.Item2);
					}

					if (header == eh.FilterStart) {
						if (parent != null)
							AddScopeBlock(parent, ehScope.Item3);
						scopeStack.Push(ehScope.Item3);
					}
				}

				ScopeBlock scope = scopeStack.Peek();
				AddBasicBlock(scope, block);
			}
			foreach (ExceptionHandler eh in body.ExceptionHandlers) {
				if (eh.TryEnd == null) {
					var pop = scopeStack.Pop();
					Debug.Assert(pop == ehScopes[eh].Item1);
				}
				if (eh.HandlerEnd == null) {
					var pop = scopeStack.Pop();
					Debug.Assert(pop == ehScopes[eh].Item2);
				}
			}
			Debug.Assert(scopeStack.Count == 1);
			Validate(root);

			return root;
		}

		static void Validate(ScopeBlock scope) {
			scope.Validate();
			foreach (var child in scope.Children)
				Validate(child);
		}

		static void AddScopeBlock(ScopeBlock block, ScopeBlock child) {
			if (block.Content.Count > 0) {
				var newScope = new ScopeBlock();
				foreach (var instrBlock in block.Content)
					newScope.Content.Add(instrBlock);
				block.Content.Clear();
				block.Children.Add(newScope);
			}
			block.Children.Add(child);
		}

		static void AddBasicBlock(ScopeBlock block, BasicBlock<CILInstrList> child) {
			if (block.Children.Count > 0) {
				var last = block.Children.Last();
				if (last.Type != ScopeType.None) {
					last = new ScopeBlock();
					block.Children.Add(last);
				}
				block = last;
			}
			Debug.Assert(block.Children.Count == 0);
			block.Content.Add(child);
		}
	}
}