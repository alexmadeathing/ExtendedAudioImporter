// SoXWrapper.h

#pragma once

using namespace System;

namespace SoXWrapper
{
	public ref struct SoXUnhandledException : public Exception { SoXUnhandledException(){ } SoXUnhandledException(String^ message) : Exception(message) { } };
	public ref struct SoXContextException   : public Exception { SoXContextException  (){ } SoXContextException  (String^ message) : Exception(message) { } };
	public ref struct SoXIOException        : public Exception { SoXIOException       (){ } SoXIOException       (String^ message) : Exception(message) { } };

	public ref class SoXWrapper abstract sealed
	{
	public:
		static array<Byte>^ LoadAudioAsVorbisStream(String^ path);

	private:
		static std::string GetTempFilePath();
		static void WriteIntoTempFile(std::string path, std::string tempPath);
		static array<Byte>^ LoadTempFile(std::string path);
	};
}
