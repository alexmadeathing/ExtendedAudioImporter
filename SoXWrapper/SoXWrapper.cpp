// This is the main DLL file.

#include "stdafx.h"
#include <msclr\marshal_cppstd.h>
#include "SoXWrapper.h"
#include "sox.h"

struct SoXContextException { };
struct SoXOpenReadFailed { };
struct SoXOpenWriteFailed { };
struct SoXWriteException { };

class SoXContext
{
public:
	SoXContext()
	{
		if (sox_init() != SOX_SUCCESS)
		{
			throw SoXContextException();
		}
	}
	inline ~SoXContext() { sox_quit(); }
};

class SoXFormat
{
public:
	inline explicit SoXFormat(sox_format_t* format) : mFormat(format) { }
	inline ~SoXFormat() { sox_close(mFormat); }

	inline sox_format_t* operator ->() const { return mFormat; }
	inline operator sox_format_t*() const { return mFormat; }

private:
	sox_format_t* mFormat;
};

class SoXBuffer
{
public:
	SoXBuffer() : mBuffer(nullptr), mBufferSize(0) { }
	~SoXBuffer() { free(mBuffer); }

	inline char*& GetBuffer() { return mBuffer; }
	inline size_t& GetBufferSize() { return mBufferSize; }

private:
	char* mBuffer;
	size_t mBufferSize;
};

array<char>^ SoXWrapper::SoXWrapper::LoadAudioAsVorbisStream(String^ path)
{
	try
	{
		SoXContext context;
		return LoadAudioAsVorbisStreamInternal(msclr::interop::marshal_as<std::string>(path));
	}
	catch (SoXContextException)
	{
		// Failed to open SoX
	}
	catch (SoXOpenReadFailed)
	{
		// Failed to load audio file
	}
	catch (SoXOpenWriteFailed)
	{
		// Failed to load audio file
	}
	catch (...)
	{
		// Unhandled exception
	}
	return nullptr;
}

array<char>^ SoXWrapper::SoXWrapper::LoadAudioAsVorbisStreamInternal(std::string path)
{
	// Open file for reading
	SoXFormat inFile(sox_open_read(path.c_str(), nullptr, nullptr, nullptr));
	if (inFile == nullptr)
	{
		throw SoXOpenReadFailed();
	}

	// Setup Vorbis encoding info
	sox_encodinginfo_t vorbisEncoding;
	vorbisEncoding.encoding = SOX_ENCODING_VORBIS;
	vorbisEncoding.bits_per_sample = SOX_UNSPEC;
	vorbisEncoding.compression = 5.0; // 0.0 = fast but bad quality ... 10.0 = slow but high quality
	vorbisEncoding.reverse_bytes = sox_option_default;
	vorbisEncoding.reverse_nibbles = sox_option_default;
	vorbisEncoding.reverse_bits = sox_option_default;
	vorbisEncoding.opposite_endian = sox_false;

	// Open memory stream for writing
	SoXBuffer buffer;
	SoXFormat outFile(sox_open_memstream_write(&buffer.GetBuffer(), &buffer.GetBufferSize(), &inFile->signal, &vorbisEncoding, nullptr, nullptr));
	if (outFile == nullptr)
	{
		throw SoXOpenWriteFailed();
	}

	// Read all samples
	const size_t maxSamples = 2048;
	sox_sample_t samples[maxSamples];
	size_t numRead;
	while ((numRead = sox_read(inFile, samples, maxSamples)))
	{
		if (sox_write(outFile, samples, numRead) != numRead)
		{
			throw SoXWriteException();
		}
	}

	// Copy into managed array
	array<char>^ result = gcnew array<char>(static_cast<int>(buffer.GetBufferSize()));
	pin_ptr<char> managedPtr = &result[0];
	char* rawManagedData = managedPtr;
	std::copy(buffer.GetBuffer(), buffer.GetBuffer() + buffer.GetBufferSize(), rawManagedData);
	return result;
}
