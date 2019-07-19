using System;
using System.Reflection;
using Confuser.Core;
using KoiVM.Confuser.Internal;

namespace KoiVM.Confuser {
	[Obfuscation(Exclude = false, Feature = "-rename", ApplyToMembers = false)]
	[BeforeProtection("Ki.ControlFlow", "Ki.AntiTamper"), AfterProtection("Ki.Constants")]
	public class KoiProtection : Protection {
		public const string _Id = "koi";
		public const string _FullId = "Ki.Koi";

		public override string Name {
			get { return "Koi Virtualizer"; }
		}

		public override string Description {
			get { return "A majestic Koi fish (or Magikarp, if you prefer) will virtualize your code!"; }
		}

		public override string Id {
			get { return _Id; }
		}

		public override string FullId {
			get { return _FullId; }
		}

		public override ProtectionPreset Preset {
			get { return ProtectionPreset.Maximum; }
		}

		protected override void Initialize(ConfuserContext context) {
			KoiInfo.Init(context);
		}

		protected override void PopulatePipeline(ProtectionPipeline pipeline) {
			pipeline.InsertPostStage(PipelineStage.Inspection,
				new InitializePhase(this, KoiInfo.KoiDirectory));
			pipeline.InsertPreStage(PipelineStage.EndModule, new MarkPhase(this));
			pipeline.InsertPreStage(PipelineStage.Debug, new FinalizePhase(this));
			pipeline.InsertPreStage(PipelineStage.Pack, new SavePhase(this));
		}
	}
}