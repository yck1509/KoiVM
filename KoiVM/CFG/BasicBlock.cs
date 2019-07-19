using System;
using System.Collections.Generic;

namespace KoiVM.CFG {
	public class BasicBlock<TContent> : IBasicBlock {
		public BasicBlock(int id, TContent content) {
			Id = id;
			Content = content;
			Sources = new List<BasicBlock<TContent>>();
			Targets = new List<BasicBlock<TContent>>();
		}

		public int Id { get; set; }
		public TContent Content { get; set; }
		public BlockFlags Flags { get; set; }
		public IList<BasicBlock<TContent>> Sources { get; private set; }
		public IList<BasicBlock<TContent>> Targets { get; private set; }

		object IBasicBlock.Content {
			get { return Content; }
		}

		IEnumerable<IBasicBlock> IBasicBlock.Sources {
			get { return Sources; }
		}

		IEnumerable<IBasicBlock> IBasicBlock.Targets {
			get { return Targets; }
		}

		public void LinkTo(BasicBlock<TContent> target) {
			Targets.Add(target);
			target.Sources.Add(this);
		}

		public override string ToString() {
			return string.Format("Block_{0:x2}:{1}{2}", Id, Environment.NewLine, Content);
		}
	}
}