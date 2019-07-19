using System;
using System.Reflection;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace KoiVM.RT.Mutation {
	[Obfuscation(Exclude = false, Feature = "+koi;-ref proxy")]
	internal static class RuntimePatcher {
		public static void Patch(ModuleDef runtime, bool debug, bool stackwalk) {
			PatchDispatcher(runtime, debug, stackwalk);
		}

		static void PatchDispatcher(ModuleDef runtime, bool debug, bool stackwalk) {
			var dispatcher = runtime.Find(RTMap.VMDispatcher, true);
			var dispatcherRun = dispatcher.FindMethod(RTMap.VMRun);
			foreach (var eh in dispatcherRun.Body.ExceptionHandlers) {
				if (eh.HandlerType == ExceptionHandlerType.Catch)
					eh.CatchType = runtime.CorLibTypes.Object.ToTypeDefOrRef();
			}
			PatchDoThrow(dispatcher.FindMethod(RTMap.VMDispatcherDothrow).Body, debug, stackwalk);
			dispatcher.Methods.Remove(dispatcher.FindMethod(RTMap.VMDispatcherThrow));
		}

		static void PatchDoThrow(CilBody body, bool debug, bool stackwalk) {
			for (int i = 0; i < body.Instructions.Count; i++) {
				var method = body.Instructions[i].Operand as IMethod;
				if (method != null && method.Name == RTMap.VMDispatcherThrow) {
					body.Instructions.RemoveAt(i);
				}
				else if (method != null && method.Name == RTMap.VMDispatcherGetIP) {
					if (!debug) {
						body.Instructions.RemoveAt(i);
						body.Instructions[i - 1].OpCode = OpCodes.Ldnull;
						var def = method.ResolveMethodDefThrow();
						def.DeclaringType.Methods.Remove(def);
					}
					else if (stackwalk) {
						var def = method.ResolveMethodDefThrow();
						body.Instructions[i].Operand = def.DeclaringType.FindMethod(RTMap.VMDispatcherStackwalk);
						def.DeclaringType.Methods.Remove(def);
					}
					else {
						var def = method.ResolveMethodDefThrow();
						def = def.DeclaringType.FindMethod(RTMap.VMDispatcherStackwalk);
						def.DeclaringType.Methods.Remove(def);
					}
				}
			}
		}
	}
}