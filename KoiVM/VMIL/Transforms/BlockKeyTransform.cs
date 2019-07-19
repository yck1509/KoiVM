using System;
using System.Collections.Generic;
using System.Linq;
using dnlib.DotNet.Emit;
using KoiVM.AST.IL;
using KoiVM.CFG;
using KoiVM.RT;
using KoiVM.VM;

namespace KoiVM.VMIL.Transforms {
	public class BlockKeyTransform : IPostTransform {
		struct BlockKey {
			public uint Entry;
			public uint Exit;
		}

		class FinallyInfo {
			public HashSet<ILBlock> FinallyEnds = new HashSet<ILBlock>();
			public HashSet<ILBlock> TryEndNexts = new HashSet<ILBlock>();
		}

		class EHMap {
			public HashSet<ILBlock> Starts = new HashSet<ILBlock>();
			public Dictionary<ExceptionHandler, FinallyInfo> Finally = new Dictionary<ExceptionHandler, FinallyInfo>();
		}

		VMRuntime runtime;
		VMMethodInfo methodInfo;
		Dictionary<ILBlock, BlockKey> Keys;

		public void Initialize(ILPostTransformer tr) {
			runtime = tr.Runtime;
			methodInfo = tr.Runtime.Descriptor.Data.LookupInfo(tr.Method);
			ComputeBlockKeys(tr.RootScope);
		}

		void ComputeBlockKeys(ScopeBlock rootScope) {
			var blocks = rootScope.GetBasicBlocks().OfType<ILBlock>().ToList();
			uint id = 1;
			Keys = blocks.ToDictionary(
				block => block,
				block => new BlockKey { Entry = id++, Exit = id++ });
			var ehMap = MapEHs(rootScope);

			bool updated;
			do {
				updated = false;

				BlockKey key;

				key = Keys[blocks[0]];
				key.Entry = 0xfffffffe;
				Keys[blocks[0]] = key;

				key = Keys[blocks[blocks.Count - 1]];
				key.Exit = 0xfffffffd;
				Keys[blocks[blocks.Count - 1]] = key;

				// Update the state ids with the maximum id
				foreach (var block in blocks) {
					key = Keys[block];
					if (block.Sources.Count > 0) {
						uint newEntry = block.Sources.Select(b => Keys[(ILBlock)b].Exit).Max();
						if (key.Entry != newEntry) {
							key.Entry = newEntry;
							updated = true;
						}
					}
					if (block.Targets.Count > 0) {
						uint newExit = block.Targets.Select(b => Keys[(ILBlock)b].Entry).Max();
						if (key.Exit != newExit) {
							key.Exit = newExit;
							updated = true;
						}
					}
					Keys[block] = key;
				}

				// Match finally enter = finally exit = try end exit
				// Match filter start = 0xffffffff
				MatchHandlers(ehMap, ref updated);
			} while (updated);

			// Replace id with actual values
			var idMap = new Dictionary<uint, uint>();
			idMap[0xffffffff] = 0;
			idMap[0xfffffffe] = methodInfo.EntryKey;
			idMap[0xfffffffd] = methodInfo.ExitKey;
			foreach (var block in blocks) {
				var key = Keys[block];

				uint entryId = key.Entry;
				if (!idMap.TryGetValue(entryId, out key.Entry))
					key.Entry = idMap[entryId] = (byte)runtime.Descriptor.Random.Next();

				uint exitId = key.Exit;
				if (!idMap.TryGetValue(exitId, out key.Exit))
					key.Exit = idMap[exitId] = (byte)runtime.Descriptor.Random.Next();

				Keys[block] = key;
			}
		}

		EHMap MapEHs(ScopeBlock rootScope) {
			EHMap map = new EHMap();
			MapEHsInternal(rootScope, map);
			return map;
		}

		void MapEHsInternal(ScopeBlock scope, EHMap map) {
			if (scope.Type == ScopeType.Filter) {
				map.Starts.Add((ILBlock)scope.GetBasicBlocks().First());
			}
			else if (scope.Type != ScopeType.None) {
				if (scope.ExceptionHandler.HandlerType == ExceptionHandlerType.Finally) {
					FinallyInfo info;
					if (!map.Finally.TryGetValue(scope.ExceptionHandler, out info))
						map.Finally[scope.ExceptionHandler] = info = new FinallyInfo();

					if (scope.Type == ScopeType.Try) {
						// Try End Next
						var scopeBlocks = new HashSet<IBasicBlock>(scope.GetBasicBlocks());
						foreach (ILBlock block in scopeBlocks) {
							if ((block.Flags & BlockFlags.ExitEHLeave) != 0 &&
							    (block.Targets.Count == 0 ||
							     block.Targets.Any(target => !scopeBlocks.Contains(target)))) {
								foreach (var target in block.Targets)
									info.TryEndNexts.Add((ILBlock)target);
							}
						}
					}
					else if (scope.Type == ScopeType.Handler) {
						// Finally End
						IEnumerable<IBasicBlock> candidates;
						if (scope.Children.Count > 0) {
							// Only ScopeType None can endfinally
							candidates = scope.Children
								.Where(s => s.Type == ScopeType.None)
								.SelectMany(s => s.GetBasicBlocks());
						}
						else {
							candidates = scope.Content;
						}
						foreach (ILBlock block in candidates) {
							if ((block.Flags & BlockFlags.ExitEHReturn) != 0 &&
							    block.Targets.Count == 0) {
								info.FinallyEnds.Add(block);
							}
						}
					}
				}
				if (scope.Type == ScopeType.Handler)
					map.Starts.Add((ILBlock)scope.GetBasicBlocks().First());
			}
			foreach (var child in scope.Children)
				MapEHsInternal(child, map);
		}

		void MatchHandlers(EHMap map, ref bool updated) {
			// handler start = 0xffffffff
			// finally end = next block of try end
			foreach (var start in map.Starts) {
				var key = Keys[start];
				if (key.Entry != 0xffffffff) {
					key.Entry = 0xffffffff;
					Keys[start] = key;
					updated = true;
				}
			}
			foreach (var info in map.Finally.Values) {
				uint maxEnd = info.FinallyEnds.Max(block => Keys[block].Exit);
				uint maxEntry = info.TryEndNexts.Max(block => Keys[block].Entry);
				uint maxId = Math.Max(maxEnd, maxEntry);

				foreach (var block in info.FinallyEnds) {
					var key = Keys[block];
					if (key.Exit != maxId) {
						key.Exit = maxId;
						Keys[block] = key;
						updated = true;
					}
				}

				foreach (var block in info.TryEndNexts) {
					var key = Keys[block];
					if (key.Entry != maxId) {
						key.Entry = maxId;
						Keys[block] = key;
						updated = true;
					}
				}
			}
		}

		public void Transform(ILPostTransformer tr) {
			var key = Keys[tr.Block];
			methodInfo.BlockKeys[tr.Block] = new VMBlockKey {
				EntryKey = (byte)key.Entry,
				ExitKey = (byte)key.Exit
			};
		}
	}
}