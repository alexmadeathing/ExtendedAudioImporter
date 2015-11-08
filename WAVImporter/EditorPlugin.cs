using Duality.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WAVImporter
{
    public class WAVImporterPlugin : EditorPlugin
    {
		public override string Id
		{
			get { return "WAVAssetImporter"; }
		}
	}
}
