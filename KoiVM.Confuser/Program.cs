using System;
using System.Windows.Forms;

namespace KoiVM.Confuser {
	internal class Program {
		[STAThread]
		static void Main(string[] args) {
			KoiInfo.Init();

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new ConfigWindow());
		}
	}
}