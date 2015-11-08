using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WAVImporter
{
	internal class WAVDataIntegrityException : Exception
	{
		public WAVDataIntegrityException()
		{
		}

		public WAVDataIntegrityException(string message)
			: base(message)
		{
		}

		public WAVDataIntegrityException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}
