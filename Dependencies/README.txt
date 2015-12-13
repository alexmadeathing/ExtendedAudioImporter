Several dependencies are required:

Please download libflac, libogg and libvorbis from:
	https://www.xiph.org/
	(tested with libflac-1.3.1, libogg-1.3.2, libvorbis-1.3.5)

Please download libsox from:
	http://sox.sourceforge.net/
	(tested with libsox-14.4.2)

Please download libmad from:
	http://www.underbit.com/products/mad/
	(tested with libmad-0.15.1b)

Dependency folders should be renamed as follows:
	libflac
	libmad
	libogg
	libsox
	libvorbis
	(strip off version numbers)

Basic Visual Studio 2013 project files are included to compile the essentials. These were borrowed and adapted from libsox. Each project file has an x86 and x64 version to make compiling using a single configuration in C# easier (Any CPU).

All projects produce static libraries and are linked into the SoXWrapper project.

The correct version of SoXWrapper is picked at runtime.