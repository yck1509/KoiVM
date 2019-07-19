using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using KoiVM.AST;
using KoiVM.AST.ILAST;

namespace KoiVM.ILAST.Transformation {
	public class BranchTransform : ITransformationHandler {
		public void Initialize(ILASTTransformer tr) {
		}

		public void Transform(ILASTTransformer tr) {
			tr.Tree.TraverseTree(Transform, tr.Method.Module);
		}

		/* 
		 * Transform according to spec, identical effects
		 * beq -> ceq; brtrue
		 * bne.un -> ceq; brfalse
		 * bge -> clt/clt.un; brfalse
		 * bge.un -> clt.un/clt; brfalse (spec does not say?)
		 * bgt -> cgt; brtrue
		 * bgt.un -> cgt.un; brtrue
		 * ble -> cgt/cgt.un; brfalse
		 * ble.un -> cgt.un/cgt; brfalse
		 * blt -> clt; brtrue
		 * blt.un -> clt.un; brtrue
		 */

		static readonly Dictionary<Code, Tuple<Code, Code, Code>> transformMap =
			new Dictionary<Code, Tuple<Code, Code, Code>> {
				{ Code.Beq, Tuple.Create(Code.Ceq, Code.Ceq, Code.Brtrue) },
				{ Code.Bne_Un, Tuple.Create(Code.Ceq, Code.Ceq, Code.Brfalse) },
				{ Code.Bge, Tuple.Create(Code.Clt, Code.Clt_Un, Code.Brfalse) },
				{ Code.Bge_Un, Tuple.Create(Code.Clt_Un, Code.Clt, Code.Brfalse) },
				{ Code.Ble, Tuple.Create(Code.Cgt, Code.Cgt_Un, Code.Brfalse) },
				{ Code.Ble_Un, Tuple.Create(Code.Cgt_Un, Code.Cgt, Code.Brfalse) },
				{ Code.Bgt, Tuple.Create(Code.Cgt, Code.Cgt, Code.Brtrue) },
				{ Code.Bgt_Un, Tuple.Create(Code.Cgt_Un, Code.Cgt_Un, Code.Brtrue) },
				{ Code.Blt, Tuple.Create(Code.Clt, Code.Clt, Code.Brtrue) },
				{ Code.Blt_Un, Tuple.Create(Code.Clt_Un, Code.Clt_Un, Code.Brtrue) }
			};

		static void Transform(ILASTExpression expr, ModuleDef module) {
			switch (expr.ILCode) {
				case Code.Beq:
				case Code.Bne_Un:
				case Code.Bge:
				case Code.Bge_Un:
				case Code.Bgt:
				case Code.Bgt_Un:
				case Code.Ble:
				case Code.Ble_Un:
				case Code.Blt:
				case Code.Blt_Un:
					break;
				default:
					return;
			}
			Debug.Assert(expr.Arguments.Length == 2);
			var mapInfo = transformMap[expr.ILCode];
			var isFloat = expr.Arguments.Any(arg => arg.Type.Value == ASTType.R4 || arg.Type.Value == ASTType.R8);
			var compCode = isFloat ? mapInfo.Item2 : mapInfo.Item1;

			expr.ILCode = mapInfo.Item3;
			expr.Arguments = new IILASTNode[] {
				new ILASTExpression {
					ILCode = compCode,
					Arguments = expr.Arguments,
					Type = ASTType.I4
				}
			};
		}
	}
}