// SoXWrapper.h

#pragma once

using namespace System;

namespace SoXWrapper
{
	public ref class SoXWrapper abstract sealed
	{
	public:
		static array<char>^ LoadAudioAsVorbisStream(String^ path);

	private:
		static array<char>^ LoadAudioAsVorbisStreamInternal(std::string path);
	};
}
