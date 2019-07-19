using System;
using System.Diagnostics;
using System.IO;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using KoiVM.AST;
using KoiVM.AST.IL;
using KoiVM.VMIL;

namespace KoiVM.RT {
	internal class BasicBlockChunk : IKoiChunk {
		VMRuntime rt;
		MethodDef method;

		public BasicBlockChunk(VMRuntime rt, MethodDef method, ILBlock block) {
			this.rt = rt;
			this.method = method;
			Block = block;
			Length = rt.serializer.ComputeLength(block);
		}

		public ILBlock Block { get; set; }

		public uint Length { get; set; }

		public void OnOffsetComputed(uint offset) {
			var len = rt.serializer.ComputeOffset(Block, offset);
			Debug.Assert(len - offset == Length);
		}

		public byte[] GetData() {
			var stream = new MemoryStream();
			rt.serializer.WriteData(Block, new BinaryWriter(stream));
			return Encrypt(stream.ToArray());
		}

		byte[] Encrypt(byte[] data) {
			var blockKey = rt.Descriptor.Data.LookupInfo(method).BlockKeys[Block];
			byte currentKey = blockKey.EntryKey;

			var firstInstr = Block.Content[0];
			var lastInstr = Block.Content[Block.Content.Count - 1];
			foreach (var instr in Block.Content) {
				var instrStart = instr.Offset - firstInstr.Offset;
				var instrEnd = instrStart + rt.serializer.ComputeLength(instr);

				// Encrypt OpCode
				{
					byte b = data[instrStart];
					data[instrStart] ^= currentKey;
					currentKey = (byte)(currentKey * 7 + b);
				}

				byte? fixupTarget = null;
				if (instr.Annotation == InstrAnnotation.JUMP ||
				    instr == lastInstr) {
					fixupTarget = blockKey.ExitKey;
				}
				else if (instr.OpCode == ILOpCode.LEAVE) {
					var eh = ((EHInfo)instr.Annotation).ExceptionHandler;
					if (eh.HandlerType == ExceptionHandlerType.Finally) {
						fixupTarget = blockKey.ExitKey;
					}
				}
				else if (instr.OpCode == ILOpCode.CALL) {
					var callInfo = (InstrCallInfo)instr.Annotation;
					var info = rt.Descriptor.Data.LookupInfo((MethodDef)callInfo.Method);
					fixupTarget = info.EntryKey;
				}

				if (fixupTarget != null) {
					var fixup = CalculateFixupByte(fixupTarget.Value, data, currentKey, instrStart + 1, instrEnd);
					data[instrStart + 1] = fixup;
				}

				// Encrypt rest of instruction
				for (uint i = instrStart + 1; i < instrEnd; i++) {
					byte b = data[i];
					data[i] ^= currentKey;
					currentKey = (byte)(currentKey * 7 + b);
				}
				if (fixupTarget != null)
					Debug.Assert(currentKey == fixupTarget.Value);

				if (instr.OpCode == ILOpCode.CALL) {
					var callInfo = (InstrCallInfo)instr.Annotation;
					var info = rt.Descriptor.Data.LookupInfo((MethodDef)callInfo.Method);
					currentKey = info.ExitKey;
				}
			}

			return data;
		}

		static byte CalculateFixupByte(byte target, byte[] data, uint currentKey, uint rangeStart, uint rangeEnd) {
			// Calculate fixup byte
			// f = k3 * 7 + d3
			// f = (k2 * 7 + d2) * 7 + d3
			// f = ((k1 * 7 + d1) * 7 + d2) * 7 + d3
			// f = (((k0 * 7 + d0) * 7 + d1) * 7 + d2) * 7 + d3
			// 7 ^ -1 (mod 256) = 183
			byte fixupByte = target;
			for (uint i = rangeEnd - 1; i > rangeStart; i--) {
				fixupByte = (byte)((fixupByte - data[i]) * 183);
			}
			fixupByte -= (byte)(currentKey * 7);
			return fixupByte;
		}
	}
}