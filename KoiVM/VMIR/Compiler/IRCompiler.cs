using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using dnlib.DotNet;
using KoiVM.AST;
using KoiVM.AST.IR;
using KoiVM.CFG;
using KoiVM.RT;
using KoiVM.RT.Mutation;
using KoiVM.VM;
using PerCederberg.Grammatica.Runtime;

namespace KoiVM.VMIR.Compiler {
	public class IRCompiler {
		VMRuntime runtime;
		ModuleDef module;
		RTConstants constants;
		Dictionary<string, BinaryChunk> chunks = new Dictionary<string, BinaryChunk>();
		Dictionary<string, BasicBlock<IRInstrList>> codes = new Dictionary<string, BasicBlock<IRInstrList>>();
		Dictionary<string, IMemberRef> memberRefs = new Dictionary<string, IMemberRef>();
		IRCompilerAssemblyFinder assemblyFinder;

		Dictionary<TypeSig, IRVariable> pseudoVars = new Dictionary<TypeSig, IRVariable>();
		List<IRInstruction> references = new List<IRInstruction>();

		public IRCompiler(VMRuntime runtime, ModuleDef rtModule, RTConstants constants) {
			this.runtime = runtime;
			module = rtModule;
			this.constants = constants;
			assemblyFinder = new IRCompilerAssemblyFinder(rtModule);
		}

		public BinaryChunk GetDataChunk(string name) {
			return chunks[name];
		}

		public BasicBlock<IRInstrList> GetCodeBlock(string name) {
			return codes[name];
		}

		public IEnumerable<BasicBlock<IRInstrList>> GetCodeBlocks() {
			return codes.Values;
		}

		public void Compile(TextReader reader) {
			var parser = new IRParser(reader);
			var rootNode = parser.Parse();
			for (int i = 0; i < rootNode.Count; i++) {
				var block = rootNode[i][0];
				if (block.Id == (int)IRConstants.DATA)
					CompileData(block);
				else if (block.Id == (int)IRConstants.CODE)
					CompileCode(block);
				else if (block.Id == (int)IRConstants.TYPE_REF_DECL)
					CompileTypeDecl(block);
				else if (block.Id == (int)IRConstants.METHOD_REF_DECL)
					CompileMethodDecl(block);
			}
			ResolveReferences();
		}

		IIROperand ResolveReference(string name) {
			var constant = constants.GetConstant(name);
			if (constant != null)
				return IRConstant.FromI4(constant.Value);

			BinaryChunk chunk;
			if (chunks.TryGetValue(name, out chunk))
				return new IRDataTarget(chunk) { Name = name };

			BasicBlock<IRInstrList> block;
			if (codes.TryGetValue(name, out block))
				return new IRJumpTarget(block);

			IMemberRef memberRef;
			if (memberRefs.TryGetValue(name, out memberRef))
				return new IRMetaTarget(memberRef);

			throw new InvalidOperationException("Unresolved reference: " + name);
		}

		void ResolveReferences() {
			foreach (var instr in references) {
				if (instr.Operand1 is UnresolvedReference)
					instr.Operand1 = ResolveReference(((UnresolvedReference)instr.Operand1).Name);
				if (instr.Operand2 is UnresolvedReference)
					instr.Operand2 = ResolveReference(((UnresolvedReference)instr.Operand2).Name);
			}
		}

		static readonly ByteConverter byteConverter = new ByteConverter();
		static readonly UInt32Converter uint32Converter = new UInt32Converter();
		static readonly UInt64Converter uint64Converter = new UInt64Converter();

		static byte ParseByte(Token token) {
			return (byte)byteConverter.ConvertFromString(token.Image);
		}

		static uint ParseUInt32(Token token) {
			return (uint)uint32Converter.ConvertFromString(token.Image);
		}

		static ulong ParseUInt64(Token token) {
			return (ulong)uint64Converter.ConvertFromString(token.Image);
		}

		static float ParseR4(Token token) {
			return float.Parse(token.Image);
		}

		static double ParseR8(Token token) {
			return double.Parse(token.Image);
		}

		void CompileData(Node node) {
			string name = ((Token)node[1]).Image;
			byte[] chunk = null;
			for (int i = 0; i < node.Count; i++) {
				var child = node[i];
				if (child.Id == (int)IRConstants.DATA_SIZE) {
					chunk = new byte[ParseUInt32((Token)child[0])];
					break;
				}
				if (child.Id == (int)IRConstants.DATA_CONTENT) {
					chunk = ReadDataContent(child);
					break;
				}
			}
			Debug.Assert(chunk != null);
			var bin = new BinaryChunk(chunk);
			;
			chunks[name] = bin;
			runtime.AddChunk(bin);
		}

		byte[] ReadDataContent(Node node) {
			var content = node[1];
			if (content.Id == (int)IRConstants.DATA_STRING) {
				var str = ((Token)content[0]).Image;
				return Encoding.UTF8.GetBytes(str.Substring(1, str.Length - 2));
			}
			if (content.Id == (int)IRConstants.DATA_BUFFER) {
				var bytes = new List<byte>();
				for (int i = 0; i < content.Count; i++) {
					if (content[i].Id == (int)IRConstants.NUM)
						bytes.Add(ParseByte((Token)content[i]));
				}
				return bytes.ToArray();
			}
			Debug.Assert(false);
			return null;
		}

		void CompileCode(Node node) {
			string name = ((Token)node[1]).Image;
			var instrs = new IRInstrList();
			for (int i = 0; i < node.Count; i++) {
				var child = node[i];
				if (child.Id == (int)IRConstants.INSTR) {
					instrs.Add(ReadInstr(child));
				}
			}
			codes[name] = new BasicBlock<IRInstrList>(0, instrs);
		}

		IRInstruction ReadInstr(Node node) {
			var instr = new IRInstruction(IROpCode.NOP);
			for (int i = 0; i < node.Count; i++) {
				var child = node[i];
				if (child.Id == (int)IRConstants.OP_CODE) {
					instr.OpCode = (IROpCode)Enum.Parse(typeof(IROpCode), ((Token)child[0]).Image);
				}
				else if (child.Id == (int)IRConstants.OPERAND) {
					if (instr.Operand1 == null)
						instr.Operand1 = ReadOperand(child);
					else
						instr.Operand2 = ReadOperand(child);
				}
			}
			if (instr.Operand1 is UnresolvedReference || instr.Operand2 is UnresolvedReference)
				references.Add(instr);
			return instr;
		}

		IIROperand ReadOperand(Node node) {
			for (int i = 0; i < node.Count; i++) {
				var child = node[i];
				if (child.Id == (int)IRConstants.NUMBER) {
					return ReadNumber(child);
				}
				if (child.Id == (int)IRConstants.REGISTER) {
					return ReadRegister(child);
				}
				if (child.Id == (int)IRConstants.POINTER) {
					return ReadPointer(child);
				}
				if (child.Id == (int)IRConstants.REFERENCE) {
					return ReadReference(child);
				}
			}
			Debug.Assert(false);
			return null;
		}

		IRVariable GetTypedVariable(ASTType type, TypeSig rawType) {
			Debug.Assert(rawType != null);
			IRVariable ret;
			if (!pseudoVars.TryGetValue(rawType, out ret)) {
				ret = new IRVariable {
					Name = "pesudo_" + rawType.ReflectionName,
					Id = -1,
					Type = type,
					RawType = rawType,
					VariableType = IRVariableType.VirtualRegister
				};
				pseudoVars[rawType] = ret;
			}
			return ret;
		}

		void ReadType(Node node, ref ASTType type, ref TypeSig rawType) {
			for (int i = 0; i < node.Count; i++) {
				var child = node[i];
				if (child.Id == (int)IRConstants.ASTTYPE) {
					type = (ASTType)Enum.Parse(typeof(ASTType), ((Token)child).Image);
				}
				else if (child.Id == (int)IRConstants.RAW_TYPE) {
					var propertyName = ((Token)child[0]).Image;
					var property = typeof(ICorLibTypes).GetProperty(propertyName);
					rawType = (TypeSig)property.GetValue(module.CorLibTypes, null);
				}
			}
		}

		IRConstant ReadNumber(Node node) {
			var type = ASTType.I4;
			TypeSig rawType = null;
			object value = null;

			for (int i = 0; i < node.Count; i++) {
				var child = node[i];
				if (child.Id == (int)IRConstants.TYPE) {
					ReadType(child, ref type, ref rawType);
				}
				else if (child.Id == (int)IRConstants.NUM) {
					var token = (Token)child;
					switch (type) {
						case ASTType.I4:
							value = (int)ParseUInt32(token);
							break;
						case ASTType.I8:
							value = (long)ParseUInt64(token);
							break;
						case ASTType.R4:
							value = ParseR4(token);
							break;
						case ASTType.R8:
							value = ParseR8(token);
							break;
					}
				}
			}
			return new IRConstant {
				Type = type,
				Value = value
			};
		}

		IRRegister ReadRegister(Node node) {
			var type = ASTType.I4;
			TypeSig rawType = null;
			VMRegisters? reg = null;

			for (int i = 0; i < node.Count; i++) {
				var child = node[i];
				if (child.Id == (int)IRConstants.TYPE) {
					ReadType(child, ref type, ref rawType);
				}
				else if (child.Id == (int)IRConstants.REG) {
					var token = (Token)child;
					reg = (VMRegisters)Enum.Parse(typeof(VMRegisters), token.Image);
				}
			}
			Debug.Assert(reg != null);
			return new IRRegister(reg.Value, type) {
				SourceVariable = rawType == null ? null : GetTypedVariable(type, rawType)
			};
		}

		IRPointer ReadPointer(Node node) {
			var type = ASTType.I4;
			TypeSig rawType = null;
			IRRegister reg = null;
			int offset = 0;

			for (int i = 0; i < node.Count; i++) {
				var child = node[i];
				if (child.Id == (int)IRConstants.TYPE) {
					ReadType(child, ref type, ref rawType);
				}
				else if (child.Id == (int)IRConstants.REGISTER) {
					reg = ReadRegister(child);
				}
				else if (child.Id == (int)IRConstants.POINTER_OFFSET) {
					offset = (int)ParseUInt32((Token)child[1]);
					if (child[0].Id == (int)IRConstants.MINUS)
						offset = -offset;
				}
			}
			return new IRPointer {
				Register = reg,
				Type = type,
				Offset = offset,
				SourceVariable = rawType == null ? null : GetTypedVariable(type, rawType)
			};
		}

		UnresolvedReference ReadReference(Node node) {
			return new UnresolvedReference(((Token)node[0]).Image);
		}

		class UnresolvedReference : IIROperand {
			public UnresolvedReference(string name) {
				Name = name;
			}

			public string Name { get; private set; }

			public ASTType Type {
				get { return ASTType.Ptr; }
			}

			public override string ToString() {
				return Name;
			}
		}

		TypeSig ReadTypeRef(Node node) {
			var sig = ((Token)node[0]).Image;
			sig = sig.Substring(1, sig.Length - 2);
			return TypeNameParser.ParseAsTypeSigReflectionThrow(module, sig, assemblyFinder);
		}

		MemberRef ReadMethodRef(Node node) {
			string name = "";
			TypeSig declType = null;
			TypeSig retType = null;
			var paramSigs = new List<TypeSig>();
			bool instance = false;

			for (int i = 0; i < node.Count; i++) {
				var child = node[i];
				if (child.Id == (int)IRConstants.STR) {
					name = ((Token)child).Image;
					name = name.Substring(1, name.Length - 2);
				}
				else if (child.Id == (int)IRConstants.INSTANCE) {
					instance = true;
				}
				else if (child.Id == (int)IRConstants.METHOD_RET_TYPE) {
					retType = ReadTypeRef(child[0]);
				}
				else if (child.Id == (int)IRConstants.TYPE_REF) {
					declType = ReadTypeRef(child);
				}
				else if (child.Id == (int)IRConstants.METHOD_PARAMS) {
					for (int j = 0; j < child.Count; j++) {
						if (child[j].Id == (int)IRConstants.TYPE_REF)
							paramSigs.Add(ReadTypeRef(child[j]));
					}
				}
			}

			if (instance)
				return new MemberRefUser(module, name, MethodSig.CreateInstance(retType, paramSigs.ToArray()),
					declType.ToTypeDefOrRef());
			return new MemberRefUser(module, name, MethodSig.CreateStatic(retType, paramSigs.ToArray()),
				declType.ToTypeDefOrRef());
		}

		void CompileTypeDecl(Node node) {
			string name = ((Token)node[1]).Image;
			var typeRef = ReadTypeRef(node[2]);
			memberRefs[name] = typeRef.ToTypeDefOrRef();
		}

		void CompileMethodDecl(Node node) {
			string name = ((Token)node[1]).Image;
			var methodRef = ReadMethodRef(node[2]);
			memberRefs[name] = methodRef;
		}
	}
}