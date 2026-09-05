/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.IO;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Base;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Io_Read;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundBase.SampleConversion;
using FileReader = Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common.FileReader;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib
{
	/// <summary>
	/// Central code for reading and writing samples. Create your SampleIO object and have a go at the ReadSample and WriteSample functions!
	/// Notes  : Not all combinations of possible sample format combinations are implemented, especially for WriteSample.
	///          Using the existing generic functions, it should be quite easy to extend the code, though.
	/// </summary>
	internal class SampleIO : IEquatable<SampleIO>
	{
		#region Bitdepth
		/// <summary>
		/// Bits per sample
		/// </summary>
		public enum BitDepth : uint8
		{
			_8Bit = 8,
			_16Bit = 16,
			_24Bit = 24,
			_32Bit = 32,
			_64Bit = 64
		}
		#endregion

		#region Channels
		/// <summary>
		/// Number of channels + channel format
		/// </summary>
		public enum Channels : uint8
		{
			/// <summary>
			/// 
			/// </summary>
			Mono = 1,

			/// <summary>
			/// LRLRLR...
			/// </summary>
			StereoInterleaved,

			/// <summary>
			/// LLL...RRR...
			/// </summary>
			StereoSplit
		}
		#endregion

		#region Endianness
		/// <summary>
		/// Sample byte order
		/// </summary>
		public enum Endianness : uint8
		{
			LittleEndian = 0,
			BigEndian = 1
		}
		#endregion

		#region Encoding
		/// <summary>
		/// Sample encoding
		/// </summary>
		public enum Encoding : uint8
		{
			/// <summary>
			/// Integer PCM, signed
			/// </summary>
			SignedPcm = 0,

			/// <summary>
			/// Integer PCM, unsigned
			/// </summary>
			UnsignedPcm,

			/// <summary>
			/// Integer PCM, delta-encoded
			/// </summary>
			DeltaPcm,

			/// <summary>
			/// Floating point PCM
			/// </summary>
			FloatPcm,

			/// <summary>
			/// Impulse Tracker 2.14 compressed
			/// </summary>
			It214,

			/// <summary>
			/// Impulse Tracker 2.15 compressed
			/// </summary>
			It215,

			/// <summary>
			/// AMS / Velvet Studio packed
			/// </summary>
			Ams,

			/// <summary>
			/// DMF Huffman compression
			/// </summary>
			Dmf,

			/// <summary>
			/// MDL Huffman compression
			/// </summary>
			Mdl,

			/// <summary>
			/// PTM 8-Bit delta value -> 16-Bit sample
			/// </summary>
			Ptm8Dto16,

			/// <summary>
			/// 4-Bit ADPCM-packed
			/// </summary>
			Adpcm,

			/// <summary>
			/// MadTracker 2 stereo delta encoding
			/// </summary>
			Mt2,

			/// <summary>
			/// Floating point PCM with 2^15 full scale
			/// </summary>
			FloatPcm15,

			/// <summary>
			/// Floating point PCM with 2^23 full scale
			/// </summary>
			FloatPcm23,

			/// <summary>
			/// Floating point PCM and data will be normalized while reading
			/// </summary>
			FloatPcmNormalize,

			/// <summary>
			/// Integer PCM and data will be normalized while reading
			/// </summary>
			SignedPcmNormalize,

			/// <summary>
			/// 8-to-16 bit G.711 u-law compression
			/// </summary>
			uLaw,

			/// <summary>
			/// 8-to-16 bit G.711 a-law compression
			/// </summary>
			aLaw
		}
		#endregion

		private BitDepth m_BitDepth;
		private Channels m_Channels;
		private Endianness m_Endianness;
		private Encoding m_Encoding;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SampleIO(BitDepth bits = BitDepth._8Bit, Channels channels = Channels.Mono, Endianness endianness = Endianness.LittleEndian, Encoding encoding = Encoding.SignedPcm)
		{
			m_BitDepth = bits;
			m_Channels = channels;
			m_Endianness = endianness;
			m_Encoding = encoding;
		}



		/********************************************************************/
		/// <summary>
		/// Return 0 in case of variable-length encoded samples
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public uint8 GetEncodedBitsPerSample()
		{
			switch (GetEncoding())
			{
				case Encoding.SignedPcm:
				case Encoding.UnsignedPcm:
				case Encoding.DeltaPcm:
				case Encoding.FloatPcm:
				case Encoding.Mt2:
				case Encoding.FloatPcm15:
				case Encoding.FloatPcm23:
				case Encoding.FloatPcmNormalize:
				case Encoding.SignedPcmNormalize:
					return GetBitDepth();

				case Encoding.It214:
				case Encoding.It215:
				case Encoding.Ams:
				case Encoding.Dmf:
				case Encoding.Mdl:
					return 0;

				case Encoding.Ptm8Dto16:
					return 16;

				case Encoding.Adpcm:
					return 4;

				case Encoding.uLaw:
					return 8;

				case Encoding.aLaw:
					return 8;

				default:
					return 0;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Return the static header size additional to the raw encoded
		/// sample data
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public size_t GetEncodedHeaderSize()
		{
			switch (GetEncoding())
			{
				case Encoding.Adpcm:
					return 16;

				default:
					return 0;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Returns true if the encoded size cannot be calculated apriori
		/// from the encoding format and the sample length
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsVariableLengthEncoded()
		{
			return GetEncodedBitsPerSample() == 0;
		}



		/********************************************************************/
		/// <summary>
		/// Returns true if the decoder for a given format uses FileReader
		/// interface and thus do not need to call GetPinnedView()
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool UsesFileReaderForDecoding()
		{
			switch (GetEncoding())
			{
				case Encoding.It214:
				case Encoding.It215:
				case Encoding.Ams:
				case Encoding.Dmf:
				case Encoding.Mdl:
					return true;

				default:
					return false;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Get bits per sample
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public uint8 GetBitDepth()
		{
			return (uint8)m_BitDepth;
		}



		/********************************************************************/
		/// <summary>
		/// Get channel layout
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Channels GetChannelFormat()
		{
			return m_Channels;
		}



		/********************************************************************/
		/// <summary>
		/// Returns number of channels
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public uint8 GetNumChannels()
		{
			return (uint8)(GetChannelFormat() == Channels.Mono ? 1 : 2);
		}



		/********************************************************************/
		/// <summary>
		/// Get sample byte order
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Endianness GetEndianness()
		{
			return m_Endianness;
		}



		/********************************************************************/
		/// <summary>
		/// Get sample format / encoding
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Encoding GetEncoding()
		{
			return m_Encoding;
		}



		/********************************************************************/
		/// <summary>
		/// Returns the encoded size of the sample. In case of
		/// variable-length encoding returns 0
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public size_t CalculateEncodedSize(SmpLength length)
		{
			if (IsVariableLengthEncoded())
				return 0;

			uint8 bps = GetEncodedBitsPerSample();

			if ((bps % 8U) != 0)
				return GetEncodedHeaderSize() + (((length + 1) / 2) * GetNumChannels());	// Round up

			return GetEncodedHeaderSize() + (length * (bps / 8U) * GetNumChannels());
		}



		/********************************************************************/
		/// <summary>
		/// Read sample from memory
		/// </summary>
		/********************************************************************/
		public size_t ReadSample(ModSample sample, FileReader file, SampleIndex smp, SmpLength sampleLength)
		{
//XX			using (Stream sampleStream = file.DataContainer().GetSampleStream(smp, sampleLength))
//			{
//				FileReader sampleFile = new FileReader(FileCursor_StdStream.Make_FileCursor<PathString>(sampleStream));
				return ReadSample(sample, file);
//			}
		}



		/********************************************************************/
		/// <summary>
		/// Read sample from memory
		/// </summary>
		/********************************************************************/
		public size_t ReadSample(ModSample sample, FileReader file)//XX 35
		{
			if (!file.IsValid())
				return 0;

			OpenMpt.LimitMax(ref sample.nLength, Snd_Def.Max_Sample_Length);

			size_t bytesRead = 0;	// Amount of memory that has been read from file

			size_t filePosition = file.GetPosition();
			CPointer<byte> sourceBuf = null;
			FileReader.PinnedView restrictedSampleDataView = null;
			size_t fileSize = 0;

			if (UsesFileReaderForDecoding())
			{
				sourceBuf = null;
				fileSize = file.BytesLeft();
			}
			else if (!IsVariableLengthEncoded())
			{
				restrictedSampleDataView = file.GetPinnedView(CalculateEncodedSize(sample.nLength));
				sourceBuf = restrictedSampleDataView.Data();
				fileSize = restrictedSampleDataView.Size();

				if (fileSize < 1)
					return 0;
			}

			if (!IsVariableLengthEncoded() && (sample.nLength > 0x40000))
			{
				// Limit sample length to available bytes in file to avoid excessive memory allocation.
				// However, for ProTracker MODs we need to support samples exceeding the end of file
				// (see the comment about MOD.shorttune2 in Load_mod.cpp), so as a semi-arbitrary threshold,
				// we do not apply this limit to samples shorter than 256K
				size_t maxLength = fileSize - Math.Min(GetEncodedHeaderSize(), fileSize);
				uint8 bps = GetEncodedBitsPerSample();

				if ((bps % 8U) != 0)
				{
					if ((size_t.MaxValue / 2U) >= maxLength)
						maxLength *= 2;
					else
						maxLength = size_t.MaxValue;
				}
				else
				{
					size_t encodedBytesPerSample = (size_t)(GetNumChannels() * GetEncodedBitsPerSample() / 8U);

					// Check if we can round up without overflowing
					if ((size_t.MaxValue - maxLength) >= (encodedBytesPerSample - 1U))
						maxLength += encodedBytesPerSample - 1U;
					else
						maxLength = size_t.MaxValue;

					maxLength /= encodedBytesPerSample;
				}

				OpenMpt.LimitMax(ref sample.nLength, SmpLength.CreateSaturating(maxLength));
			}
			else if ((GetEncoding() == Encoding.It214) || (GetEncoding() == Encoding.It215) || (GetEncoding() == Encoding.Mdl) || (GetEncoding() == Encoding.Dmf))
			{
				// In the best case, IT compression represents each sample point as a single bit.
				// In practice, there is of course the two-byte header per compressed block and the initial bit width change.
				// As a result, if we have a file length of n, we know that the sample can be at most n*8 sample points long.
				// For DMF, there are at least two bits per sample, and for MDL at least 5 (so both are worse than IT)
				size_t maxLength = fileSize;
				uint8 maxSamplesPerByte = (uint8)(8 / GetNumChannels());

				if ((size_t.MaxValue / maxSamplesPerByte) >= maxLength)
					maxLength *= maxSamplesPerByte;
				else
					maxLength = size_t.MaxValue;

				OpenMpt.LimitMax(ref sample.nLength, SmpLength.CreateSaturating(maxLength));
			}
			else if (GetEncoding() == Encoding.Ams)
			{
				if (fileSize <= 9)
					return 0;

				file.Skip(4);	// Target sample size (we already know this)
				SmpLength maxLength = Math.Min(file.ReadUInt32LE(), uint32.CreateSaturating(fileSize));
				file.SkipBack(8);

				// In the best case, every byte triplet can decode to 255 bytes, which is a ratio of exactly 1:85
				if ((uint32.MaxValue / 85U) >= maxLength)
					maxLength *= 85;
				else
					maxLength = uint32.MaxValue;

				OpenMpt.LimitMax(ref sample.nLength, maxLength / ((uint32)m_BitDepth / 8U));
			}

			if (sample.nLength < 1)
				return 0;

			sample.uFlags.Set(SampleFlags.Chn_16Bit, GetBitDepth() >= 16);
			sample.uFlags.Set(SampleFlags.Chn_Stereo, GetChannelFormat() != Channels.Mono);
			size_t sampleSize = sample.AllocateSample();	// Target sample size in bytes

			if (sampleSize == 0)
			{
				sample.nLength = 0;

				return 0;
			}

			//////////////////////////////////////////////////////
			// Compressed samples

			if (this == new SampleIO(BitDepth._8Bit, Channels.Mono, Endianness.LittleEndian, Encoding.Adpcm))
			{
				// 4-Bit ADPCM data
				int8[] compressionTable = new int8[16];

				if (file.ReadArray(compressionTable))
				{
					size_t readLength = (sample.nLength + 1) / 2;
					OpenMpt.LimitMax(ref readLength, size_t.CreateSaturating(file.BytesLeft()));

					CPointer<uint8> inBuf = sourceBuf.Cast<uint8>() + compressionTable.Length;
					CPointer<int8> outBuf = sample.Sample8();
					int8 delta = 0;

					for (size_t i = readLength; i != 0; i--)
					{
						delta += compressionTable[inBuf[0] & 0x0f];
						outBuf[0, 1] = delta;

						delta += compressionTable[(inBuf[0] >> 4) & 0x0f];
						outBuf[0, 1] = delta;

						inBuf++;
					}

					bytesRead = compressionTable.Size() + readLength;
				}
			}
			else if ((GetEncoding() == Encoding.It214) || (GetEncoding() == Encoding.It215))
			{
				throw new NotImplementedException("IT");
			}
			else if ((GetEncoding() == Encoding.Ams) && (GetChannelFormat() == Channels.Mono))
			{
				throw new NotImplementedException("AMS");
			}
			else if ((GetEncoding() == Encoding.Ptm8Dto16) && (GetChannelFormat() == Channels.Mono) && (GetBitDepth() == 16))
			{
				throw new NotImplementedException("Ptm8Dto16");
			}
			else if ((GetEncoding() == Encoding.Mdl) && (GetChannelFormat() == Channels.Mono) && (GetBitDepth() <= 16))
			{
				throw new NotImplementedException("MDL");
			}
			else if ((GetEncoding() == Encoding.Dmf) && (GetChannelFormat() == Channels.Mono) && (GetBitDepth() <= 16))
			{
				throw new NotImplementedException("DMF");
			}
			else if (((GetEncoding() == Encoding.uLaw) || (GetEncoding() == Encoding.aLaw)) && (GetBitDepth() == 16) && ((GetChannelFormat() == Channels.Mono) || (GetChannelFormat() == Channels.StereoInterleaved)))
			{
				throw new NotImplementedException("uLaw/aLaw");
			}

			/////////////////////////
			// Uncompressed samples

			//////////////////////////////////////////////////////
			// 8-Bit / Mono / PCM
			else if ((GetBitDepth() == 8) && (GetChannelFormat() == Channels.Mono))
			{
				switch (GetEncoding())
				{
					// 8-Bit / Mono / Signed / PCM
					case Encoding.SignedPcm:
					{
						bytesRead = ModSampleCopy.CopyMonoSample<DecodeInt8, int8>(sample, sourceBuf, fileSize);
						break;
					}

					// 8-Bit / Mono / Unsigned / PCM
					case Encoding.UnsignedPcm:
					{
						bytesRead = ModSampleCopy.CopyMonoSample<DecodeUInt8, int8>(sample, sourceBuf, fileSize);
						break;
					}

					// 8-Bit / Mono / Delta / PCM
					case Encoding.DeltaPcm:
					case Encoding.Mt2:
					{
						bytesRead = ModSampleCopy.CopyMonoSample<DecodeInt8Delta, int8>(sample, sourceBuf, fileSize);
						break;
					}
				}
			}

			//////////////////////////////////////////////////////
			// 8-Bit / Stereo Split / PCM
			else if ((GetBitDepth() == 8) && (GetChannelFormat() == Channels.StereoSplit))
			{
				switch (GetEncoding())
				{
					// 8-Bit / Stereo Split / Signed / PCM
					case Encoding.SignedPcm:
					{
						bytesRead = ModSampleCopy.CopyStereoSplitSample<DecodeInt8, int8>(sample, sourceBuf, fileSize);
						break;
					}

					// 8-Bit / Stereo Split / Unsigned / PCM
					case Encoding.UnsignedPcm:
					{
						bytesRead = ModSampleCopy.CopyStereoSplitSample<DecodeUInt8, int8>(sample, sourceBuf, fileSize);
						break;
					}

					// 8-Bit / Stereo Split / Delta / PCM
					case Encoding.DeltaPcm:
					// Same as deltaPCM, but right channel is stored as a difference from the left channel
					case Encoding.Mt2:
					{
						bytesRead = ModSampleCopy.CopyStereoSplitSample<DecodeInt8Delta, int8>(sample, sourceBuf, fileSize);

						if (GetEncoding() == Encoding.Mt2)
							throw new NotImplementedException("MT2");

						break;
					}
				}
			}

			//////////////////////////////////////////////////////
			// 8-Bit / Stereo Interleaved / PCM
			else if ((GetBitDepth() == 8) && (GetChannelFormat() == Channels.StereoInterleaved))
			{
				switch (GetEncoding())
				{
					// 8-Bit / Stereo Interleaved / Signed / PCM
					case Encoding.SignedPcm:
					{
						bytesRead = ModSampleCopy.CopyStereoInterleavedSample<DecodeInt8, int8>(sample, sourceBuf, fileSize);
						break;
					}

					// 8-Bit / Stereo Interleaved / Unsigned / PCM
					case Encoding.UnsignedPcm:
					{
						bytesRead = ModSampleCopy.CopyStereoInterleavedSample<DecodeUInt8, int8>(sample, sourceBuf, fileSize);
						break;
					}

					// 8-Bit / Stereo Interleaved / Delta / PCM
					case Encoding.DeltaPcm:
					{
						bytesRead = ModSampleCopy.CopyStereoInterleavedSample<DecodeInt8Delta, int8>(sample, sourceBuf, fileSize);
						break;
					}
				}
			}

			//////////////////////////////////////////////////////
			// 16-Bit / Mono / Little Endian / PCM
			else if ((GetBitDepth() == 16) && (GetChannelFormat() == Channels.Mono) && (GetEndianness() == Endianness.LittleEndian))
			{
				switch (GetEncoding())
				{
					// 16-Bit / Mono / Signed / PCM
					case Encoding.SignedPcm:
					{
						bytesRead = ModSampleCopy.CopyMonoSample<DecodeInt16<Offset_0, LittleEndian16>, int16>(sample, sourceBuf, fileSize);
						break;
					}

					// 16-Bit / Mono / Unsigned / PCM
					case Encoding.UnsignedPcm:
					{
						bytesRead = ModSampleCopy.CopyMonoSample<DecodeInt16<Offset_0x8000, LittleEndian16>, int16>(sample, sourceBuf, fileSize);
						break;
					}

					// 16-Bit / Mono / Delta / PCM
					case Encoding.DeltaPcm:
					case Encoding.Mt2:
					{
						bytesRead = ModSampleCopy.CopyMonoSample<DecodeInt16Delta<LittleEndian16>, int16>(sample, sourceBuf, fileSize);
						break;
					}
				}
			}

			//////////////////////////////////////////////////////
			// 16-Bit / Mono / Big Endian / PCM
			else if ((GetBitDepth() == 16) && (GetChannelFormat() == Channels.Mono) && (GetEndianness() == Endianness.BigEndian))
			{
				switch (GetEncoding())
				{
					// 16-Bit / Mono / Signed / PCM
					case Encoding.SignedPcm:
					{
						bytesRead = ModSampleCopy.CopyMonoSample<DecodeInt16<Offset_0, BigEndian16>, int16>(sample, sourceBuf, fileSize);
						break;
					}

					// 16-Bit / Mono / Unsigned / PCM
					case Encoding.UnsignedPcm:
					{
						bytesRead = ModSampleCopy.CopyMonoSample<DecodeInt16<Offset_0x8000, BigEndian16>, int16>(sample, sourceBuf, fileSize);
						break;
					}

					// 16-Bit / Mono / Delta / PCM
					case Encoding.DeltaPcm:
					{
						bytesRead = ModSampleCopy.CopyMonoSample<DecodeInt16Delta<BigEndian16>, int16>(sample, sourceBuf, fileSize);
						break;
					}
				}
			}

			//////////////////////////////////////////////////////
			// 16-Bit / Stereo Split / Little Endian / PCM
			else if ((GetBitDepth() == 16) && (GetChannelFormat() == Channels.StereoSplit) && (GetEndianness() == Endianness.LittleEndian))
			{
				switch (GetEncoding())
				{
					// 16-Bit / Stereo Split / Signed / PCM
					case Encoding.SignedPcm:
					{
						bytesRead = ModSampleCopy.CopyStereoSplitSample<DecodeInt16<Offset_0, LittleEndian16>, int16>(sample, sourceBuf, fileSize);
						break;
					}

					// 16-Bit / Stereo Split / Unsigned / PCM
					case Encoding.UnsignedPcm:
					{
						bytesRead = ModSampleCopy.CopyStereoSplitSample<DecodeInt16<Offset_0x8000, LittleEndian16>, int16>(sample, sourceBuf, fileSize);
						break;
					}

					// 16-Bit / Stereo Split / Delta / PCM
					case Encoding.DeltaPcm:
					// Same as deltaPCM, but right channel is stored as a difference from the left channel
					case Encoding.Mt2:
					{
						bytesRead = ModSampleCopy.CopyStereoSplitSample<DecodeInt16Delta<LittleEndian16>, int16>(sample, sourceBuf, fileSize);

						if (GetEncoding() == Encoding.Mt2)
							throw new NotImplementedException("MT2");

						break;
					}
				}
			}

			//////////////////////////////////////////////////////
			// 16-Bit / Stereo Split / Big Endian / PCM
			else if ((GetBitDepth() == 16) && (GetChannelFormat() == Channels.StereoSplit) && (GetEndianness() == Endianness.BigEndian))
			{
				switch (GetEncoding())
				{
					// 16-Bit / Stereo Split / Signed / PCM
					case Encoding.SignedPcm:
					{
						bytesRead = ModSampleCopy.CopyStereoSplitSample<DecodeInt16<Offset_0, BigEndian16>, int16>(sample, sourceBuf, fileSize);
						break;
					}

					// 16-Bit / Stereo Split / Unsigned / PCM
					case Encoding.UnsignedPcm:
					{
						bytesRead = ModSampleCopy.CopyStereoSplitSample<DecodeInt16<Offset_0x8000, BigEndian16>, int16>(sample, sourceBuf, fileSize);
						break;
					}

					// 16-Bit / Stereo Split / Delta / PCM
					case Encoding.DeltaPcm:
					{
						bytesRead = ModSampleCopy.CopyStereoSplitSample<DecodeInt16Delta<BigEndian16>, int16>(sample, sourceBuf, fileSize);
						break;
					}
				}
			}

			//////////////////////////////////////////////////////
			// 16-Bit / Stereo Interleave / Little Endian / PCM
			else if ((GetBitDepth() == 16) && (GetChannelFormat() == Channels.StereoInterleaved) && (GetEndianness() == Endianness.LittleEndian))
			{
				switch (GetEncoding())
				{
					// 16-Bit / Stereo Interleaved / Signed / PCM
					case Encoding.SignedPcm:
					{
						bytesRead = ModSampleCopy.CopyStereoInterleavedSample<DecodeInt16<Offset_0, LittleEndian16>, int16>(sample, sourceBuf, fileSize);
						break;
					}

					// 16-Bit / Stereo Interleaved / Unsigned / PCM
					case Encoding.UnsignedPcm:
					{
						bytesRead = ModSampleCopy.CopyStereoInterleavedSample<DecodeInt16<Offset_0x8000, LittleEndian16>, int16>(sample, sourceBuf, fileSize);
						break;
					}

					// 16-Bit / Stereo Interleaved / Delta / PCM
					case Encoding.DeltaPcm:
					{
						bytesRead = ModSampleCopy.CopyStereoInterleavedSample<DecodeInt16Delta<LittleEndian16>, int16>(sample, sourceBuf, fileSize);
						break;
					}
				}
			}

			//////////////////////////////////////////////////////
			// 16-Bit / Stereo Interleave / Big Endian / PCM
			else if ((GetBitDepth() == 16) && (GetChannelFormat() == Channels.StereoInterleaved) && (GetEndianness() == Endianness.BigEndian))
			{
				switch (GetEncoding())
				{
					// 16-Bit / Stereo Interleaved / Signed / PCM
					case Encoding.SignedPcm:
					{
						bytesRead = ModSampleCopy.CopyStereoInterleavedSample<DecodeInt16<Offset_0, BigEndian16>, int16>(sample, sourceBuf, fileSize);
						break;
					}

					// 16-Bit / Stereo Interleaved / Unsigned / PCM
					case Encoding.UnsignedPcm:
					{
						bytesRead = ModSampleCopy.CopyStereoInterleavedSample<DecodeInt16<Offset_0x8000, BigEndian16>, int16>(sample, sourceBuf, fileSize);
						break;
					}

					// 16-Bit / Stereo Interleaved / Delta / PCM
					case Encoding.DeltaPcm:
					{
						bytesRead = ModSampleCopy.CopyStereoInterleavedSample<DecodeInt16Delta<BigEndian16>, int16>(sample, sourceBuf, fileSize);
						break;
					}
				}
			}

			//////////////////////////////////////////////////////
			// 24-Bit / Signed / Mono / PCM
			else if ((GetBitDepth() == 24) && (GetChannelFormat() == Channels.Mono) && (GetEncoding() == Encoding.SignedPcm))
			{
				throw new NotImplementedException("24-bit mono");
			}

			//////////////////////////////////////////////////////
			// 24-Bit / Signed / Stereo Interleaved / PCM
			else if ((GetBitDepth() == 24) && (GetChannelFormat() == Channels.StereoInterleaved) && (GetEncoding() == Encoding.SignedPcm))
			{
				throw new NotImplementedException("24-bit stereo");
			}

			//////////////////////////////////////////////////////
			// 32-Bit / Signed / Mono / PCM
			else if ((GetBitDepth() == 32) && (GetChannelFormat() == Channels.Mono) && (GetEncoding() == Encoding.SignedPcm))
			{
				throw new NotImplementedException("32-bit mono");
			}

			//////////////////////////////////////////////////////
			// 32-Bit / Signed / Stereo Interleaved / PCM
			else if ((GetBitDepth() == 32) && (GetChannelFormat() == Channels.StereoInterleaved) && (GetEncoding() == Encoding.SignedPcm))
			{
				throw new NotImplementedException("32-bit stereo");
			}

			//////////////////////////////////////////////////////
			// 64-Bit / Signed / Mono / PCM
			else if ((GetBitDepth() == 64) && (GetChannelFormat() == Channels.Mono) && (GetEncoding() == Encoding.SignedPcm))
			{
				throw new NotImplementedException("64-bit mono");
			}

			//////////////////////////////////////////////////////
			// 64-Bit / Signed / Stereo Interleaved / PCM
			else if ((GetBitDepth() == 64) && (GetChannelFormat() == Channels.StereoInterleaved) && (GetEncoding() == Encoding.SignedPcm))
			{
				throw new NotImplementedException("64-bit stereo");
			}

			//////////////////////////////////////////////////////
			// 32-Bit / Float / Mono / PCM
			else if ((GetBitDepth() == 32) && (GetChannelFormat() == Channels.Mono) && (GetEncoding() == Encoding.FloatPcm))
			{
				throw new NotImplementedException("32-bit float mono");
			}

			//////////////////////////////////////////////////////
			// 32-Bit / Float / Stereo Interleaved / PCM
			else if ((GetBitDepth() == 32) && (GetChannelFormat() == Channels.StereoInterleaved) && (GetEncoding() == Encoding.FloatPcm))
			{
				throw new NotImplementedException("32-bit float stereo");
			}

			//////////////////////////////////////////////////////
			// 64-Bit / Float / Mono / PCM
			else if ((GetBitDepth() == 64) && (GetChannelFormat() == Channels.Mono) && (GetEncoding() == Encoding.FloatPcm))
			{
				throw new NotImplementedException("64-bit float mono");
			}

			//////////////////////////////////////////////////////
			// 64-Bit / Float / Stereo Interleaved / PCM
			else if ((GetBitDepth() == 64) && (GetChannelFormat() == Channels.StereoInterleaved) && (GetEncoding() == Encoding.FloatPcm))
			{
				throw new NotImplementedException("64-bit float stereo");
			}

			//////////////////////////////////////////////////////
			// 24-Bit / Signed / Mono, Stereo Interleaved / PCM
			else if ((GetBitDepth() == 24) && ((GetChannelFormat() == Channels.Mono) || (GetChannelFormat() == Channels.StereoInterleaved)) && (GetEncoding() == Encoding.SignedPcmNormalize))
			{
				throw new NotImplementedException("24-bit normalize");
			}

			//////////////////////////////////////////////////////
			// 32-Bit / Signed / Mono, Stereo Interleaved / PCM
			else if ((GetBitDepth() == 32) && ((GetChannelFormat() == Channels.Mono) || (GetChannelFormat() == Channels.StereoInterleaved)) && (GetEncoding() == Encoding.SignedPcmNormalize))
			{
				throw new NotImplementedException("32-bit normalize");
			}

			//////////////////////////////////////////////////////
			// 32-Bit / Float / Mono, Stereo Interleaved / PCM
			else if ((GetBitDepth() == 32) && ((GetChannelFormat() == Channels.Mono) || (GetChannelFormat() == Channels.StereoInterleaved)) && (GetEncoding() == Encoding.FloatPcmNormalize))
			{
				throw new NotImplementedException("32-bit float normalize");
			}

			//////////////////////////////////////////////////////
			// 32-Bit / Float / Mono, Stereo Interleaved / PCM
			else if ((GetBitDepth() == 32) && ((GetChannelFormat() == Channels.Mono) || (GetChannelFormat() == Channels.StereoInterleaved)) && (GetEncoding() == Encoding.FloatPcmNormalize))
			{
				throw new NotImplementedException("32-bit float normalize");
			}

			//////////////////////////////////////////////////////
			// 64-Bit / Float / Mono, Stereo Interleaved / PCM
			else if ((GetBitDepth() == 64) && ((GetChannelFormat() == Channels.Mono) || (GetChannelFormat() == Channels.StereoInterleaved)) && (GetEncoding() == Encoding.FloatPcmNormalize))
			{
				throw new NotImplementedException("64-bit float normalize");
			}

			//////////////////////////////////////////////////////
			// 32-Bit / Float / Mono / PCM / full scale 2^15
			else if ((GetBitDepth() == 32) && (GetChannelFormat() == Channels.Mono) && (GetEncoding() == Encoding.FloatPcm15))
			{
				throw new NotImplementedException("32-bit mono float 2^15 normalize");
			}

			//////////////////////////////////////////////////////
			// 32-Bit / Float / Stereo Interleaved / PCM / full scale 2^15
			else if ((GetBitDepth() == 32) && (GetChannelFormat() == Channels.StereoInterleaved) && (GetEncoding() == Encoding.FloatPcm15))
			{
				throw new NotImplementedException("32-bit stereo float 2^15 normalize");
			}

			//////////////////////////////////////////////////////
			// 32-Bit / Float / Mono / PCM / full scale 2^23
			else if ((GetBitDepth() == 32) && (GetChannelFormat() == Channels.Mono) && (GetEncoding() == Encoding.FloatPcm23))
			{
				throw new NotImplementedException("32-bit mono float 2^23 normalize");
			}

			//////////////////////////////////////////////////////
			// 32-Bit / Float / Stereo Interleaved / PCM / full scale 2^23
			else if ((GetBitDepth() == 32) && (GetChannelFormat() == Channels.StereoInterleaved) && (GetEncoding() == Encoding.FloatPcm23))
			{
				throw new NotImplementedException("32-bit stereo float 2^23 normalize");
			}

			file.Seek(filePosition + bytesRead);

			return bytesRead;
		}




		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator == (SampleIO me, SampleIO other)
		{
			if (ReferenceEquals(me, other))
				return true;

			if ((me is null) || (other is null))
				return false;

			return (me.m_BitDepth == other.m_BitDepth) && (me.m_Channels == other.m_Channels) && (me.m_Endianness == other.m_Endianness) && (me.m_Encoding == other.m_Encoding);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator != (SampleIO me, SampleIO other)
		{
			return !(me == other);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public override bool Equals(object obj)
		{
			return (obj is SampleIO other) && (this == other);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(SampleIO other)
		{
			return this == other;
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public override int GetHashCode()
		{
			HashCode hash = new HashCode();

			hash.Add(m_BitDepth);
			hash.Add(m_Channels);
			hash.Add(m_Endianness);
			hash.Add(m_Encoding);

			return hash.ToHashCode();
		}
	}
}
