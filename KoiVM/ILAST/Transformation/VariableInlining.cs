using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using dnlib.DotNet.Emit;
using KoiVM.AST.ILAST;

namespace KoiVM.ILAST.Transformation {
	public class VariableInlining : ITransformationHandler {
		public void Initialize(ILASTTransformer tr) {
		}

		public static ILASTExpression GetExpression(IILASTStatement node) {
			if (node is ILASTExpression) {
				var expr = (ILASTExpression)node;
				if (expr.ILCode == Code.Pop && expr.Arguments[0] is ILASTExpression)
					expr = (ILASTExpression)expr.Arguments[0];
				return expr;
			}
			else if (node is ILASTAssignment)
				return ((ILASTAssignment)node).Value;
			else
				return null;
		}

		public void Transform(ILASTTransformer tr) {
			var varUsage = new Dictionary<ILASTVariable, int>();

			for (int i = 0; i < tr.Tree.Count; i++) {
				var st = tr.Tree[i];
				var expr = GetExpression(st);
				if (expr == null)
					continue;

				if (st is ILASTExpression && expr.ILCode == Code.Nop) {
					tr.Tree.RemoveAt(i);
					i--;
					continue;
				}
				else if (st is ILASTAssignment) {
					var assignment = (ILASTAssignment)st;
					if (Array.IndexOf(tr.Tree.StackRemains, assignment.Variable) != -1)
						continue;
					Debug.Assert(assignment.Variable.VariableType == ILASTVariableType.StackVar);
				}

				foreach (var arg in expr.Arguments) {
					Debug.Assert(arg is ILASTVariable);
					var argVar = (ILASTVariable)arg;
					if (argVar.VariableType == ILASTVariableType.StackVar)
						varUsage.Increment(argVar);
				}
			}

			// If a variable is remained on stack, it cannot be inlined since it would be pushed on the stack.
			foreach (var remain in tr.Tree.StackRemains)
				varUsage.Remove(remain);

			var simpleVars = new HashSet<ILASTVariable>(varUsage.Where(usage => usage.Value == 1).Select(pair => pair.Key));
			bool modified;
			do {
				modified = false;

				for (int i = 0; i < tr.Tree.Count - 1; i++) {
					var assignment = tr.Tree[i] as ILASTAssignment;
					if (assignment == null)
						continue;

					if (!simpleVars.Contains(assignment.Variable))
						continue;

					var expr = GetExpression(tr.Tree[i + 1]);
					if (expr == null || expr.ILCode.ToOpCode().Name.StartsWith("stelem"))
						continue;

					for (int argIndex = 0; argIndex < expr.Arguments.Length; argIndex++) {
						var argVar = expr.Arguments[argIndex] as ILASTVariable;
						// If previous arguments are not variables (ie. expression),
						// there might be side-effect inlining succeeding arguments.
						if (argVar == null)
							break;

						if (argVar == assignment.Variable) {
							expr.Arguments[argIndex] = assignment.Value;
							tr.Tree.RemoveAt(i);
							i--;
							modified = true;
							break;
						}
					}

					// Ensure the block is processed sequentially.
					if (modified)
						break;
				}
			} while (modified);
		}
	}
}