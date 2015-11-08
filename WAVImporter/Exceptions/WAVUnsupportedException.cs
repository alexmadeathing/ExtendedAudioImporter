using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WAVImporter
{
	internal class WAVUnsupportedException : Exception
	{
		public WAVUnsupportedException()
		{
		}

		public WAVUnsupportedException(string message)
			: base(message)
		{
		}

		public WAVUnsupportedException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}
