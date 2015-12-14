# Dependencies

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