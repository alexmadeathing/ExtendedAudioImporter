using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WAVImporter
{
	/// <summary>
	/// WAV file format loader with limited Vorbis read/write capability
	/// </summary>
	internal class WAVLoader
	{
		protected const short WAVE_FORMAT_PCM = 0x1;
		protected const short WAVE_FORMAT_IEEE_FLOAT = 0x3;

		protected short dataFormat = WAVE_FORMAT_PCM;
		protected short numChannels = 0;
		protected int sampleRate = 0;
		protected short bitDepth = 0;
		protected byte[] audioData = null;

		protected WAVLoader()
		{

		}

		public WAVLoader(string filePath)
		{
			Statics.AssertArgumentNotNull(filePath, "filePath");

			try
			{
				using (var stream = new FileStream(filePath, FileMode.Open))
				{
					using (var reader = new BinaryReader(stream))
					{
						ReadRIFFData(reader);
					}
				}
			}
			catch (FileNotFoundException)
			{
			}
			catch (IOException)
			{
			}
			catch (WAVDataIntegrityException)
			{
			}
			// TODO Correct asset importer error reporting
		}

		public void SaveToFile(string outputPath)
		{
			try
			{
				using (var writer = new BinaryWriter(new FileStream(outputPath, FileMode.Create)))
				{
					WriteRIFFFileHeaderChunk(writer);
					WriteRIFFFormatChunk(writer);
					WriteRIFFDataChunk(writer);
				}
			}
			catch (IOException)
			{
			}
			// TODO Correct asset importer error reporting
		}

#region WAV Reading
		private void ReadRIFFData(BinaryReader reader)
		{
			ReadRIFFHeaderChunk(reader);

			// NOTE - Despite this being done in a loop, we currently only allow
			//        one format chunk and one data chunk. All other chunks will
			//        be skipped. I don't think WAV files ever contain more than
			//        one chunk of each type; there's no linkage between chunks.
			while(ReadRIFFChunk(reader));
		}

		private void ReadRIFFHeaderChunk(BinaryReader reader)
		{
			int loadedRiffID = reader.ReadInt32();
			Statics.Assert(loadedRiffID == LabelToInt32("RIFF"), new WAVDataIntegrityException("Bad RIFF data"));

			// Ignore file size
			reader.ReadInt32();

			int loadedRiffTypeID = reader.ReadInt32();
			Statics.Assert(loadedRiffTypeID == LabelToInt32("WAVE"), new WAVDataIntegrityException("Bad RIFF data"));
		}

		private bool ReadRIFFChunk(BinaryReader reader)
		{
			int chunkID = reader.ReadInt32();
			if (chunkID == 0)
			{
				// If we fail to read at this point, it probably means we've read the whole file
				return false;
			}
			int chunkSize = reader.ReadInt32();

			if (chunkID == LabelToInt32("fmt "))
			{
				ReadRIFFFormatChunk(reader, chunkSize);
			}
			else if (chunkID == LabelToInt32("data"))
			{
				ReadRIFFDataChunk(reader, chunkSize);
			}
			else
			{
				// Skip unrecognised chunk
				reader.ReadBytes(chunkSize);
			}

			return true;
		}

		private void ReadRIFFFormatChunk(BinaryReader reader, int chunkSize)
		{
			// Get data format
			this.dataFormat = reader.ReadInt16();
			Statics.Assert(this.dataFormat == WAVE_FORMAT_PCM || this.dataFormat == WAVE_FORMAT_IEEE_FLOAT,
				new WAVUnsupportedException("WAV importer currently supports pcm or IEEE float uncompressed audio data"));

			// Get num channels
			this.numChannels = reader.ReadInt16();
			// NOTE - At the time of writing, Duality also only supports mono or stereo
			Statics.Assert(this.numChannels == 1 || this.numChannels == 2, new WAVUnsupportedException("WAV importer currently supports mono or stereo audio data"));

			// Get sample rate
			this.sampleRate = reader.ReadInt32();
			Statics.Assert(this.sampleRate > 0, new WAVDataIntegrityException("Invalid sample rate: " + this.sampleRate.ToString()));
			Statics.Assert(this.sampleRate <= 48000, new WAVUnsupportedException("WAV importer currently supports sample rates up to 48KHz"));

			// Get some pointless stuff (pointless for uncompressed RIFFs)
			int avgBPS = reader.ReadInt32();
			short blockAlign = reader.ReadInt16();

			// Get bit depth
			this.bitDepth = reader.ReadInt16();
			Statics.Assert(this.bitDepth == 8 || this.bitDepth == 16 || this.bitDepth == 24 || this.bitDepth == 32, new WAVUnsupportedException("WAV importer currently supports bit depths: 8, 16, 24, 32"));

			// Final checks
			int expectedAverageBytesPerSecond = (this.sampleRate * this.bitDepth * this.numChannels) / 8;
			short expectedBlockAlign = (short)((this.bitDepth * this.numChannels) / 8);
			Statics.Assert(avgBPS == expectedAverageBytesPerSecond, new WAVDataIntegrityException("RIFF format chunk appears to contain corrupted data"));
			Statics.Assert(blockAlign == expectedBlockAlign, new WAVDataIntegrityException("RIFF format chunk appears to contain corrupted data"));

			// Skip chunk extensions - pretty sure we're just going to ignore this for now
			if (chunkSize > 16)
			{
				int fmtExtraSize = reader.ReadInt16();
				reader.ReadBytes(fmtExtraSize);
			}
		}

		private void ReadRIFFDataChunk(BinaryReader reader, int chunkSize)
		{
			this.audioData = reader.ReadBytes(chunkSize);

			// Pad if chunk size is odd
			if (chunkSize % 2 == 1)
			{
				reader.ReadByte();
			}
		}
#endregion

#region WAV Writing
		private void WriteRIFFFileHeaderChunk(BinaryWriter writer)
		{
			writer.Write(LabelToInt32("RIFF"));
			writer.Write(GetFileSize());
			writer.Write(LabelToInt32("WAVE"));
		}

		private void WriteRIFFFormatChunk(BinaryWriter writer)
		{
			// Chunk preamble
			writer.Write(LabelToInt32("fmt "));
			writer.Write(GetFormatChunkSize());

			int avgBPS = (this.sampleRate * this.bitDepth * this.numChannels) / 8;
			short blockAlign = (short)((this.bitDepth * this.numChannels) / 8);

			// Chunk data
			writer.Write(this.dataFormat);
			writer.Write(this.numChannels);
			writer.Write(this.sampleRate);
			writer.Write(avgBPS);
			writer.Write(blockAlign);
			writer.Write(this.bitDepth);

			// NOTE - Don't need to write chunk extension if LPCMData.FormatChunkSize is 16
		}

		private void WriteRIFFDataChunk(BinaryWriter writer)
		{
			// Chunk preamble
			writer.Write(LabelToInt32("data"));
			writer.Write(GetDataChunkSize());

			// Chunk data
			writer.Write(this.audioData);
		}

		private int GetFormatChunkSize()
		{
			return 16;
		}

		private int GetDataChunkSize()
		{
			return audioData.Length;
		}

		private int GetFileSize()
		{
			int fileHeaderSize = 12;
			int numChunks = 2;
			int chunkPreambleSize = 8;
			return fileHeaderSize + numChunks * chunkPreambleSize + GetDataChunkSize() + GetFormatChunkSize();
		}
#endregion

		private static int LabelToInt32(string label)
		{
			return BitConverter.ToInt32(new byte[] { Convert.ToByte(label[0]), Convert.ToByte(label[1]), Convert.ToByte(label[2]), Convert.ToByte(label[3]) }, 0);
		}
	}
}
