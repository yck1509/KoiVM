using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Security.Cryptography;
using KoiVM.AST.IL;

namespace KoiVM.RT {
	[Obfuscation(Exclude = false, Feature = "+koi;-ref proxy")]
	internal class DbgWriter {
		struct DbgEntry {
			public uint offset;
			public uint len;

			public string document;
			public uint lineNum;
		}

		Dictionary<ILBlock, List<DbgEntry>> entries = new Dictionary<ILBlock, List<DbgEntry>>();
		HashSet<string> documents = new HashSet<string>();
		byte[] dbgInfo;

		public void AddSequencePoint(ILBlock block, uint offset, uint len, string document, uint lineNum) {
			List<DbgEntry> entryList;
			if (!entries.TryGetValue(block, out entryList))
				entryList = entries[block] = new List<DbgEntry>();

			entryList.Add(new DbgEntry {
				offset = offset,
				len = len,
				document = document,
				lineNum = lineNum
			});
			documents.Add(document);
		}

		public DbgSerializer GetSerializer() {
			return new DbgSerializer(this);
		}

		public byte[] GetDbgInfo() {
			return dbgInfo;
		}

		internal class DbgSerializer : IDisposable {
			DbgWriter dbg;
			BinaryWriter writer;
			MemoryStream stream;
			Dictionary<string, uint> docMap;

			internal DbgSerializer(DbgWriter dbg) {
				this.dbg = dbg;
				stream = new MemoryStream();
				var aes = new AesManaged();
				aes.IV = aes.Key = Convert.FromBase64String("UkVwAyrARLAy4GmQLL860w==");
				writer = new BinaryWriter(
					new DeflateStream(
						new CryptoStream(stream, aes.CreateEncryptor(), CryptoStreamMode.Write),
						CompressionMode.Compress
						)
					);

				InitStream();
			}

			void InitStream() {
				docMap = new Dictionary<string, uint>();
				writer.Write(dbg.documents.Count);
				uint docId = 0;
				foreach (var doc in dbg.documents) {
					writer.Write(doc);
					docMap[doc] = docId++;
				}
			}

			public void WriteBlock(BasicBlockChunk chunk) {
				List<DbgEntry> entryList;
				if (chunk == null || !dbg.entries.TryGetValue(chunk.Block, out entryList) ||
				    chunk.Block.Content.Count == 0)
					return;

				var offset = chunk.Block.Content[0].Offset;
				foreach (var entry in entryList) {
					writer.Write(entry.offset + chunk.Block.Content[0].Offset);
					writer.Write(entry.len);
					writer.Write(docMap[entry.document]);
					writer.Write(entry.lineNum);
				}
			}

			public void Dispose() {
				writer.Dispose();
				dbg.dbgInfo = stream.ToArray();
			}
		}
	}
}