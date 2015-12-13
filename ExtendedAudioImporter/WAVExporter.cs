using NVorbis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtendedAudioImporter
{
	internal static class WAVExporter
	{
		private struct PCMData
		{
			public byte[] Data;
			public short ChannelCount;
			public int SampleRate;
		}

		private const int FormatChunkSize = 16;

		private const short BitDepth = 32;
		private const short WAVE_FORMAT_IEEE_FLOAT = 0x3;
		private const int SampleSizeBytes = 4;

		private const int DefaultBufferSize = 1024 * 16;

		public static void SaveVorbisDataToWAV(byte[] vorbisData, string filePath)
		{
			using (var outStream = new BinaryWriter(new FileStream(filePath, FileMode.Create)))
			{
				using (var vorbisStream = new VorbisReader(new MemoryStream(vorbisData), true))
				{
					var pcm = ReadAll(vorbisStream);

					WriteRIFFFileHeaderChunk(outStream, pcm);
					WriteRIFFFormatChunk(outStream, pcm);
					WriteRIFFDataChunk(outStream, pcm);
				}
			}
		}

		private static PCMData ReadAll(VorbisReader vorbisStream)
		{
			PCMData pcm;
			pcm.ChannelCount = (short)vorbisStream.Channels;
			pcm.SampleRate = vorbisStream.SampleRate;

			var samples = new List<float>();

			var buffer = new float[DefaultBufferSize];
			int samplesRead = 0;
			while ((samplesRead = vorbisStream.ReadSamples(buffer, 0, buffer.Length)) > 0)
			{
				if (samplesRead != buffer.Length)
				{
					Array.Resize(ref buffer, samplesRead);
				}
				samples.AddRange(buffer);
				buffer = new float[DefaultBufferSize];
			}

			var samplesArray = samples.ToArray();
			pcm.Data = new byte[samplesArray.Length * SampleSizeBytes];
			Buffer.BlockCopy(samplesArray, 0, pcm.Data, 0, pcm.Data.Length);

			return pcm;
		}

		private static void WriteRIFFFileHeaderChunk(BinaryWriter outStream, PCMData pcm)
		{
			int fileHeaderSize = 12;
			int numChunks = 2;
			int chunkPreambleSize = 8;
			int fileSize = fileHeaderSize + numChunks * chunkPreambleSize + pcm.Data.Length + FormatChunkSize;

			outStream.Write("RIFF".ToCharArray());
			outStream.Write(fileSize);
			outStream.Write("WAVE".ToCharArray());
		}

		private static void WriteRIFFFormatChunk(BinaryWriter outStream, PCMData pcm)
		{
			int avgBPS = (pcm.SampleRate * BitDepth * pcm.ChannelCount) / 8;
			short blockAlign = (short)((BitDepth * pcm.ChannelCount) / 8);

			// Chunk preamble
			outStream.Write("fmt ".ToCharArray());
			outStream.Write(FormatChunkSize);

			// Chunk data
			outStream.Write(WAVE_FORMAT_IEEE_FLOAT);
			outStream.Write(pcm.ChannelCount);
			outStream.Write(pcm.SampleRate);
			outStream.Write(avgBPS);
			outStream.Write(blockAlign);
			outStream.Write(BitDepth);

			// NOTE - Don't need to write chunk extension if FormatChunkSize is 16
		}

		private static void WriteRIFFDataChunk(BinaryWriter outStream, PCMData pcm)
		{
			// Chunk preamble
			outStream.Write("data".ToCharArray());
			outStream.Write(pcm.Data.Length);

			// Chunk data
			outStream.Write(pcm.Data);
		}
	}
}
