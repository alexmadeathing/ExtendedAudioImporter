# DualityExtendedAudioImporter
![Duality Extended Audio Importer Icon](https://cloud.githubusercontent.com/assets/12872500/11780392/866b5e2c-a259-11e5-9b4b-2e9acbf88c61.png)

Extended audio format support for Duality 2D Game Development Framework http://duality.adamslair.net


## Introduction

Duality is a plugin based games development framework designed to be a minimal
interface into which more specific or complex packages are "plugged in". Since
Vorbis is used as Duality's primary audio compression mechanism, it has built
in support for importing OGG Vorbis files. It does not provide any mechanism
to load or save any other audio file formats.

This project aims to extend the number of audio formats supported by implementing
an editor plugin which fits into Duality's asset importing workflow. Simply
drag new audio files into Duality's Project View.


## Formats Supported

* .aiff (including files with extensions: .aif, .aifc)
* .mp2
* .mp3
* .flac
* .wav

DualityExtendedAudioImporter uses [libsox](http://sox.sourceforge.net/) to import
audio files of various flavours. The formats exposed here have been confirmed
working to some degree. Since libsox actually allows many more formats to be
loaded, new formats will be exposed as and when they have been reasonably tested.


## Installation

DualityExtendedAudioImporter can be installed via Duality Editor's package management
dialog, or by cloning this GitHub project and building in Visual Studio.

### Via Duality Editor

![Duality Package Manager](https://cloud.githubusercontent.com/assets/12872500/11785448/d703c840-a279-11e5-9c67-0d180145c020.png)

DualityExtendedAudioImporter can be installed via Duality Editor's built in package manager. Simply search for Duality Extended Audio Importer and press the Install/Update button.

### Manual Installation

* Clone this repository: E.g. *git clone git@github.com:importjingles/DualityExtendedAudioImporter.git*
* [Build the project](#building-dualityextendedaudioimporter-from-source)
* Copy the following built files into *YourProject/Plugins/*:
  * ExtendedAudioImporter.editor.dll
  * SoXWrapper_x86.dll
  * SoXWrapper_x64.dll
* *(optional)* Copy associated *.pdb* files
* Run *YourProject/DualityEditor.exe* - You should see the log line: [Core] Info: Assembly loaded: ExtendedAudioImporter.editor


## Usage

Simply drag audio files into Duality:

![Drag and Drop into Duality](https://cloud.githubusercontent.com/assets/12872500/11782535/73115326-a269-11e5-9204-1f344868f07f.png)


## Building DualityExtendedAudioImporter From Source

### Structure

DualityExtentededAudioImporter is made up of the following parts:

* ExtendedAudioImporter
  * Core DualityExtendedAudioImporter project
  * Contains Editor plugin interface
  * Contains new asset importer for AudioData objects
  * Also contains a very basic WAVExporter class for asset exporting
* SoXWrapper
  * CLR compatible wrapper for libsox
  * Split into separate x86 and x64 binaries to make it easier to build under "Any CPU" configuration
* Dependencies
  * libsox and its main dependencies

ExtendedAudioImporter will pick the correct SoXWrapper for your platform at runtime.

### Dependencies

The following dependencies must be gathered prior to building DualityExtendedAudioImporter manually:

**libflac, libogg and libvorbis** - https://www.xiph.org/ (project tested with libflac-1.3.1, libogg-1.3.2, libvorbis-1.3.5)

**libsox** - http://sox.sourceforge.net/ (project tested with libsox-14.4.2)

**libmad** - http://www.underbit.com/products/mad/ (project tested with libmad-0.15.1b)

Dependency folders should be renamed as follows (strip off version numbers):
* libflac
* libmad
* libogg
* libsox
* libvorbis

Basic Visual Studio 2013 project files are included to compile the essentials. These were borrowed and adapted from libsox. Each project file has an x86 and x64 version to make compiling using a single configuration in C# easier (Any CPU).

All projects produce static libraries and are linked into the SoXWrapper project.

### Building in Visual Studio

* Ensure you have acquired all dependencies and they are in the correct folders (see above)
* Open *DualityExtendedAudioImporter.sln*
* Choose either Debug or Release configuration
* From the *Build* menu, click *Build Solution*
* The following plugin files will end up in *bin/Release/* (or *bin/Debug/*):
  * ExtendedAudioImporter.editor.dll
  * SoXWrapper_x86.dll
  * SoXWrapper_x64.dll

### Other build environments...
...are not currently supported. Contributions most welcome!
