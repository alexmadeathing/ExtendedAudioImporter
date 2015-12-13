// This is the main DLL file.

#include "stdafx.h"
#include <msclr\marshal_cppstd.h>
#include <fstream>
#include "SoXWrapper.h"
#include "sox.h"

class SoXContext
{
public:
	SoXContext()
	{
		if (sox_init() != SOX_SUCCESS)
		{
			throw gcnew SoXWrapper::SoXContextException();
		}

		auto globals = sox_get_globals();
		globals->output_message_handler = OnSoXMessage;

	}
	inline ~SoXContext() { sox_quit(); }

private:
	static void OnSoXMessage(
		unsigned int level,   /**< 1 = FAIL, 2 = WARN, 3 = INFO, 4 = DEBUG, 5 = DEBUG_MORE, 6 = DEBUG_MOST. */
		const char* filename, /**< Source code __FILENAME__ from which message originates. */
		const char* format,   /**< Message format string. */
		va_list args          /**< Message format parameters. */
		)
	{
//		if (level == 1)
		{
			std::vector<char> buffer;
			auto numChars = vsnprintf_s(nullptr, 0, 0, format, args);
			if (numChars > 0)
			{
				buffer.resize(numChars + 1);
				vsnprintf_s(&buffer[0], buffer.size(), numChars, format, args);
				throw gcnew SoXWrapper::SoXIOException(gcnew String(buffer.data()));
			}
		}
	}
};

class SoXFormat
{
public:
	inline explicit SoXFormat(sox_format_t* format) : mFormat(format) { }
	inline ~SoXFormat() { if (mFormat != nullptr) sox_close(mFormat); }

	inline sox_format_t* operator ->() const { return mFormat; }
	inline operator sox_format_t*() const { return mFormat; }

private:
	sox_format_t* mFormat;
};

array<Byte>^ SoXWrapper::SoXWrapper::LoadAudioAsVorbisStream(String^ path)
{
	try
	{
		SoXContext context;

		// SoX does not naturally support writing to a buffer in Windows (because it relies on open_memstream)
		// We'll save to a temporary file instead
		auto tempFilePath = GetTempFilePath();
		WriteIntoTempFile(msclr::interop::marshal_as<std::string>(path), tempFilePath);
		return LoadTempFile(tempFilePath);
	}
	catch (SoXContextException^)
	{
		// Failed to open SoX - rethrow
		throw;
	}
	catch (SoXIOException^)
	{
		// Failed to read/write audio file - rethrow
		throw;
	}
	catch (...)
	{
		// Unhandled exception
		throw gcnew SoXUnhandledException();
	}
	return nullptr;
}

std::string SoXWrapper::SoXWrapper::GetTempFilePath()
{
	char tempString[L_tmpnam_s];
	tmpnam_s(tempString, L_tmpnam_s);
	return std::string(tempString);
}

void SoXWrapper::SoXWrapper::WriteIntoTempFile(std::string path, std::string tempPath)
{
	// SoX format detection seems a bit flaky (failing to load mp3)
	// We'll give it a bit of a helping hand
	std::string extension = path.substr(path.find_last_of(".") + 1);

	// Open file for reading
	SoXFormat inFile(sox_open_read(path.c_str(), nullptr, nullptr, extension.c_str()));
	if (inFile == nullptr)
	{
		throw gcnew SoXIOException(gcnew String((std::string("Failed to open file for reading: ") + path.c_str()).c_str()));
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

	// Open file for writing
	SoXFormat outFile(sox_open_write(tempPath.c_str(), &inFile->signal, &vorbisEncoding, "ogg", nullptr, nullptr));
	if (outFile == nullptr)
	{
		throw gcnew SoXIOException(gcnew String((std::string("Failed to open file for writing: ") + tempPath.c_str()).c_str()));
	}

	// Read all samples
	const size_t maxSamples = 2048;
	sox_sample_t samples[maxSamples];
	size_t numRead;
	while ((numRead = sox_read(inFile, samples, maxSamples)))
	{
		if (sox_write(outFile, samples, numRead) != numRead)
		{
			throw gcnew SoXIOException(gcnew String((std::string("Failed to copy samples: ") + path.c_str()).c_str()));
		}
	}
}

array<Byte>^ SoXWrapper::SoXWrapper::LoadTempFile(std::string path)
{
	// Open temp file
	std::ifstream inFile;
	inFile.open(path.c_str(), std::ios::binary | std::ios::in);
	if (!inFile.is_open())
	{
		throw gcnew SoXIOException(gcnew String((std::string("Failed to open file for reading: ") + path.c_str()).c_str()));
	}

	// Get file size
	inFile.seekg(0, inFile.end);
	auto fileSize = inFile.tellg();
	inFile.seekg(0, inFile.beg);

	// Allocate managed array
	array<Byte>^ result = gcnew array<Byte>(static_cast<int>(fileSize));
	pin_ptr<Byte> managedPtr = &result[0];
	Byte* rawManagedData = managedPtr;

	// Copy into managed array
	inFile.read(reinterpret_cast<char*>(rawManagedData), fileSize);
	inFile.close();
	return result;
}
