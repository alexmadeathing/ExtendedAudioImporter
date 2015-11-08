using Duality;
using Duality.Resources;
using Duality.Editor.AssetManagement;
using System;
using System.IO;
using System.Linq;

namespace WAVImporter
{
	public class WAVAssetImporter : IAssetImporter
	{
		public static readonly string SourceFileExtPrimary = ".wav";
		private static readonly string[] SourceFileExts = new[] { SourceFileExtPrimary };

		public string Id
		{
			get { return "WAVAudioDataAssetImporter"; }
		}
		public string Name
		{
			get { return "WAV AudioData Importer"; }
		}
		public int Priority
		{
			get { return 0; }
		}

		public void PrepareImport(IAssetImportEnvironment env)
		{
			// Ask to handle all input that matches the conditions in AcceptsInput
			foreach (AssetImportInput input in env.HandleAllInput(this.AcceptsInput))
			{
				// For all handled input items, specify which Resource the importer intends to create / modify
				env.AddOutput<AudioData>(input.AssetName, input.Path);
			}
		}
		public void Import(IAssetImportEnvironment env)
		{
			// Handle all available input. No need to filter or ask for this anymore, as
			// the preparation step already made a selection with AcceptsInput. We won't
			// get any input here that didn't match.
			foreach (AssetImportInput input in env.Input)
			{
				// Request a target Resource with a name matching the input
				ContentRef<AudioData> targetRef = env.GetOutput<AudioData>(input.AssetName);

				// If we successfully acquired one, proceed with the import
				if (targetRef.IsAvailable)
				{
					AudioData target = targetRef.Res;

					// Load as WAV
					var data = (new WAVVorbisLoader(input.Path)).ConvertToDualityFormat();

					// Push into Duality
					target.Native.LoadData(data.SampleRate, data.Data, data.NumSamples, data.DataLayout, data.DataElementType);

					// Add the requested output to signal that we've done something with it
					env.AddOutput(targetRef, input.Path);
				}
			}
		}

		public void PrepareExport(IAssetExportEnvironment env)
		{
			// We can export any Resource that is an AudioData
			if (env.Input is AudioData)
			{
				// Add the file path of the exported output we'll produce.
				env.AddOutputPath(env.Input.Name + SourceFileExtPrimary);
			}
		}
		public void Export(IAssetExportEnvironment env)
		{
			// Determine input and output path
			AudioData input = env.Input as AudioData;
			string outputPath = env.AddOutputPath(input.Name + SourceFileExtPrimary);

			// Convert to WAV
			var data = new WAVVorbisLoader(input.OggVorbisData);

			// Write to hard drive
			data.SaveToFile(outputPath);
		}

		private bool AcceptsInput(AssetImportInput input)
		{
			string inputFileExt = Path.GetExtension(input.Path);
			bool matchingFileExt = SourceFileExts.Any(acceptedExt => string.Equals(inputFileExt, acceptedExt, StringComparison.InvariantCultureIgnoreCase));
			return matchingFileExt;
		}
	}
}
