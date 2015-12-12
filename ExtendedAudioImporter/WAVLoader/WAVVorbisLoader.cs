using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NVorbis;
using System.IO;

namespace WAVImporter
{
	internal class WAVVorbisLoader : WAVLoader
	{
		public WAVVorbisLoader(string filePath):
			base(filePath)
		{
		}

		public WAVVorbisLoader(byte[] vorbisData)
		{
			Statics.AssertArgumentNotNull(vorbisData, "vorbisData");

			try
			{
				using (var stream = new MemoryStream(vorbisData, false))
				{
					using (var reader = new VorbisReader(stream, false))
					{
						ReadVorbisData(reader);
					}
				}
			}
			catch (IOException)
			{
			}
			// TODO Correct asset importer error reporting
		}

#region Vorbis Reading
		private void ReadVorbisData(VorbisReader reader)
		{
			const int numChunkBytes = 1024 * 1024; // 1 MiB
			const int numChunkSamples = numChunkBytes / sizeof(float);

			short bitsPerSample = 32;

			this.dataFormat = WAVE_FORMAT_IEEE_FLOAT;
			this.numChannels = (short)reader.Channels;
			this.sampleRate = reader.SampleRate;
			this.bitDepth = bitsPerSample;
			this.numSamples = 0;

			// TODO Handle exceptions
			using (var stream = new MemoryStream())
			{
				using (var writer = new BinaryWriter(stream))
				{
					var buffer = new float[numChunkSamples];
					var byteBuffer = new byte[numChunkBytes];
					int numRead = reader.ReadSamples(buffer, 0, buffer.Length);
					while (numRead > 0)
					{
						this.numSamples += numRead;
						// TODO Icky - Can we definitely not just cast the buffer?
						Buffer.BlockCopy(buffer, 0, byteBuffer, 0, numRead * sizeof(float));
						writer.Write(byteBuffer, 0, numRead * sizeof(float));
						numRead = reader.ReadSamples(buffer, 0, buffer.Length);
					}
				}
				this.audioData = stream.ToArray();
			}
		}
#endregion
	}
}
