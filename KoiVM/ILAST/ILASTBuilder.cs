using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using KoiVM.AST;
using KoiVM.AST.ILAST;
using KoiVM.CFG;

namespace KoiVM.ILAST {
	using CILBlock = BasicBlock<CILInstrList>;

	public class ILASTBuilder {
		MethodDef method;
		CilBody body;
		ScopeBlock scope;
		IList<CILBlock> basicBlocks;
		Dictionary<Instruction, CILBlock> blockHeaders;
		Dictionary<CILBlock, BlockState> blockStates;
		List<ILASTExpression> instrReferences;

		ILASTBuilder(MethodDef method, CilBody body, ScopeBlock scope) {
			this.method = method;
			this.body = body;
			this.scope = scope;

			basicBlocks = scope.GetBasicBlocks().Cast<CILBlock>().ToList();
			blockHeaders = basicBlocks.ToDictionary(block => block.Content[0], block => block);
			blockStates = new Dictionary<CILBlock, BlockState>();
			instrReferences = new List<ILASTExpression>();
			Debug.Assert(basicBlocks.Count > 0);
		}

		struct BlockState {
			public ILASTVariable[] BeginStack;
			public ILASTTree ASTTree;
		}

		public static void BuildAST(MethodDef method, CilBody body, ScopeBlock scope) {
			var builder = new ILASTBuilder(method, body, scope);
			var basicBlocks = scope.GetBasicBlocks().Cast<CILBlock>().ToList();
			builder.BuildAST();
		}

		void BuildAST() {
			BuildASTInternal();
			BuildPhiNodes();
			var blockMap = scope.UpdateBasicBlocks((CILBlock block) => blockStates[block].ASTTree);
			var newBlockMap = blockHeaders.ToDictionary(pair => pair.Key, pair => blockMap[pair.Value]);
			foreach (var expr in instrReferences) {
				if (expr.Operand is Instruction)
					expr.Operand = newBlockMap[(Instruction)expr.Operand];
				else
					expr.Operand = ((Instruction[])expr.Operand).Select(instr => (IBasicBlock)newBlockMap[instr]).ToArray();
			}
		}

		void BuildASTInternal() {
			var workList = new Stack<CILBlock>();
			PopulateBeginStates(workList);

			var visited = new HashSet<CILBlock>();
			while (workList.Count > 0) {
				var block = workList.Pop();
				if (visited.Contains(block))
					continue;
				visited.Add(block);

				Debug.Assert(blockStates.ContainsKey(block));
				var state = blockStates[block];
				Debug.Assert(state.ASTTree == null);

				var tree = BuildAST(block.Content, state.BeginStack);
				var remains = tree.StackRemains;
				state.ASTTree = tree;
				blockStates[block] = state;

				// Propagate stack states
				foreach (var successor in block.Targets) {
					BlockState successorState;
					if (!blockStates.TryGetValue(successor, out successorState)) {
						var blockVars = new ILASTVariable[remains.Length];
						for (int i = 0; i < blockVars.Length; i++) {
							blockVars[i] = new ILASTVariable {
								Name = string.Format("ph_{0:x2}_{1:x2}", successor.Id, i),
								Type = remains[i].Type,
								VariableType = ILASTVariableType.PhiVar
							};
						}
						successorState = new BlockState {
							BeginStack = blockVars
						};
						blockStates[successor] = successorState;
					}
					else {
						if (successorState.BeginStack.Length != remains.Length)
							throw new InvalidProgramException("Inconsistent stack depth.");
					}
					workList.Push(successor);
				}
			}
		}

		void PopulateBeginStates(Stack<CILBlock> workList) {
			for (int i = 0; i < body.ExceptionHandlers.Count; i++) {
				var eh = body.ExceptionHandlers[i];
				blockStates[blockHeaders[eh.TryStart]] = new BlockState {
					BeginStack = new ILASTVariable[0]
				};

				var handlerBlock = blockHeaders[eh.HandlerStart];
				workList.Push(handlerBlock);
				if (eh.HandlerType == ExceptionHandlerType.Fault ||
				    eh.HandlerType == ExceptionHandlerType.Finally) {
					blockStates[handlerBlock] = new BlockState {
						BeginStack = new ILASTVariable[0]
					};
				}
				else {
					var type = TypeInference.ToASTType(eh.CatchType.ToTypeSig());
					// Do not process overlapped handler blocks twice
					if (!blockStates.ContainsKey(handlerBlock)) {
						var exVar = new ILASTVariable {
							Name = string.Format("ex_{0:x2}", i),
							Type = type,
							VariableType = ILASTVariableType.ExceptionVar,
							Annotation = eh
						};
						blockStates[handlerBlock] = new BlockState {
							BeginStack = new[] { exVar }
						};
					}
					else
						Debug.Assert(blockStates[handlerBlock].BeginStack.Length == 1);

					if (eh.FilterStart != null) {
						var filterVar = new ILASTVariable {
							Name = string.Format("ef_{0:x2}", i),
							Type = type,
							VariableType = ILASTVariableType.FilterVar,
							Annotation = eh
						};
						var filterBlock = blockHeaders[eh.FilterStart];
						workList.Push(filterBlock);
						blockStates[filterBlock] = new BlockState {
							BeginStack = new[] { filterVar }
						};
					}
				}
			}
			blockStates[basicBlocks[0]] = new BlockState {
				BeginStack = new ILASTVariable[0]
			};
			workList.Push(basicBlocks[0]);
			foreach (var block in basicBlocks) {
				if (block.Sources.Count > 0)
					continue;
				if (workList.Contains(block))
					continue;
				blockStates[block] = new BlockState {
					BeginStack = new ILASTVariable[0]
				};
				workList.Push(block);
			}
		}

		void BuildPhiNodes() {
			foreach (var statePair in blockStates) {
				var block = statePair.Key;
				var state = statePair.Value;
				// source count = 0 => eh handlers begin state, having ex object
				if (block.Sources.Count == 0 && state.BeginStack.Length > 0) {
					Debug.Assert(state.BeginStack.Length == 1);
					var phi = new ILASTPhi {
						Variable = state.BeginStack[0],
						SourceVariables = new[] { state.BeginStack[0] }
					};
					state.ASTTree.Insert(0, phi);
				}
				else if (state.BeginStack.Length > 0) {
					for (int varIndex = 0; varIndex < state.BeginStack.Length; varIndex++) {
						var phi = new ILASTPhi { Variable = state.BeginStack[varIndex] };
						phi.SourceVariables = new ILASTVariable[block.Sources.Count];
						for (int i = 0; i < phi.SourceVariables.Length; i++) {
							phi.SourceVariables[i] = blockStates[block.Sources[i]].ASTTree.StackRemains[varIndex];
						}
						// reverse phi nodes => pop in correct order
						state.ASTTree.Insert(0, phi);
					}
				}
			}
		}

		ILASTTree BuildAST(CILInstrList instrs, ILASTVariable[] beginStack) {
			var tree = new ILASTTree();
			var evalStack = new Stack<ILASTVariable>(beginStack);
			Func<int, IILASTNode[]> popArgs = numArgs => {
				var args = new IILASTNode[numArgs];
				for (int i = numArgs - 1; i >= 0; i--)
					args[i] = evalStack.Pop();
				return args;
			};

			List<Instruction> prefixes = new List<Instruction>();
			foreach (var instr in instrs) {
				if (instr.OpCode.OpCodeType == OpCodeType.Prefix) {
					prefixes.Add(instr);
					continue;
				}

				int pushes, pops;
				ILASTExpression expr;
				if (instr.OpCode.Code == Code.Dup) {
					pushes = pops = 1;

					var arg = evalStack.Peek();
					expr = new ILASTExpression {
						ILCode = Code.Dup,
						Operand = null,
						Arguments = new IILASTNode[] { arg }
					};
				}
				else {
					instr.CalculateStackUsage(method.ReturnType.ElementType != ElementType.Void, out pushes, out pops);
					Debug.Assert(pushes == 0 || pushes == 1);

					if (pops == -1) {
						evalStack.Clear();
						pops = 0;
					}

					expr = new ILASTExpression {
						ILCode = instr.OpCode.Code,
						Operand = instr.Operand,
						Arguments = popArgs(pops)
					};
					if (expr.Operand is Instruction || expr.Operand is Instruction[])
						instrReferences.Add(expr);
				}
				expr.CILInstr = instr;
				if (prefixes.Count > 0) {
					expr.Prefixes = prefixes.ToArray();
					prefixes.Clear();
				}

				if (pushes == 1) {
					var variable = new ILASTVariable {
						Name = string.Format("s_{0:x4}", instr.Offset),
						VariableType = ILASTVariableType.StackVar
					};
					evalStack.Push(variable);

					tree.Add(new ILASTAssignment {
						Variable = variable,
						Value = expr
					});
				}
				else {
					tree.Add(expr);
				}
			}
			tree.StackRemains = evalStack.Reverse().ToArray();
			return tree;
		}
	}
}