using Duality;
using Duality.Resources;
using Duality.Editor.AssetManagement;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Duality.IO;

namespace ExtendedAudioImporter
{
	public class ExtendedAudioImporter : IAssetImporter
	{
		public static readonly string SourceFileExtPrimary = ".wav";
		private static readonly string[] SourceFileExts = new[]
		{
			// #NOTE - A select few formats are chosen here, only the most popular ones
			// This can be extended at a later date to support more SoX loaders, but for now
			// this will cover most bases. Note that ".ogg" could be included here since it is
			// supported by SoX, but is currently handled by the default AudioData importer.
			SourceFileExtPrimary, // ".wav"
			".aif",
			".aiff",
			".aifc",
			".au",
			".mp2",
			".mp3",
			".flac"
			// ".ogg"
		};

		public string Id
		{
			get { return "ExtendedAudioImporter"; }
		}
		public string Name
		{
			get { return "Extended Audio Importer"; }
		}
		public int Priority
		{
			get { return 0; }
		}

		private static MethodInfo LoadMethod;

		static ExtendedAudioImporter()
		{
			// Load assembly
			string nativeAssemblyName = Environment.Is64BitProcess ? "SoXWrapper_x64.dll" : "SoXWrapper_x86.dll";
			string nativeAssemblyPath = PathOp.GetFullPath(PathOp.Combine(DualityApp.PluginDirectory, nativeAssemblyName));
			Assembly nativeAssembly = Assembly.LoadFile(nativeAssemblyPath);
			// #NOTE: Allow any "file missing" exceptions to propagate to Duality

			// Find SoXWrapper class in assembly
			Type wrapperClass = nativeAssembly.GetType("SoXWrapper.SoXWrapper");
			if (wrapperClass == null)
			{
				throw new InvalidOperationException("Failed to find required class in assembly: " + nativeAssemblyPath);
			}

			// Find Load method in SoXWrapper class
			LoadMethod = wrapperClass.GetMethod("LoadAudioAsVorbisStream");
			if (LoadMethod == null)
			{
				throw new InvalidOperationException("Failed to find required method in assembly: " + nativeAssemblyPath);
			}
		}

		private static byte[] InvokeLoadMethod(string path)
		{
 			try
 			{
				return LoadMethod.Invoke(null, new object[] { path }) as byte[];
 			}
 			catch (TargetInvocationException e)
			{
				throw e.InnerException;
 			}
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

					// Load audio via SoX
					var data = InvokeLoadMethod(input.Path);
					if (data != null)
					{
						// Pass to AudioData target
						target.OggVorbisData = data;

						// Add the requested output to signal that we've done something with it
						env.AddOutput(targetRef, input.Path);
					}
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
			//			var data = new WAVVorbisLoader(input.OggVorbisData);

			// Write to hard drive
			//			data.SaveToFile(outputPath);
		}

		private bool AcceptsInput(AssetImportInput input)
		{
			string inputFileExt = Path.GetExtension(input.Path);
			bool matchingFileExt = SourceFileExts.Any(acceptedExt => string.Equals(inputFileExt, acceptedExt, StringComparison.InvariantCultureIgnoreCase));
			return matchingFileExt;
		}
	}
}
