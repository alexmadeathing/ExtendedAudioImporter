using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WAVImporter
{
	internal class Statics
	{
		public static void AssertArgumentNotNull(object arg, string argName)
		{
			if (arg == null)
			{
				throw new ArgumentException(argName + " argument cannot be null", argName);
			}
		}

		public static void Assert(bool condition, Exception e)
		{
			if (!condition)
			{
				throw e;
			}
		}
	}
}
