/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Agent.ModuleConverter.Mo3Converter.Containers;
using Polycode.NostalgicPlayer.Agent.ModuleConverter.Mo3Converter.Decoders;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.Mo3Converter
{
	/// <summary>
	/// Helper class to decompress and write all samples
	/// </summary>
	internal static class Mo3SampleWriter
	{
		/********************************************************************/
		/// <summary>
		/// Will unpack and save the sample data
		/// </summary>
		/********************************************************************/
		public static bool PrepareAndWriteSamples(Mo3Module module, ModuleStream moduleStream, ConverterStream converterStream, bool unsigned = false)
		{
			DecodeSampleInfo[] decodeSampleInfo = PrepareSamples(module, moduleStream, unsigned);
			if (decodeSampleInfo == null)
				return false;

			WriteSamples(decodeSampleInfo, converterStream);

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Will unpack and prepare all the samples
		/// </summary>
		/********************************************************************/
		public static DecodeSampleInfo[] PrepareSamples(Mo3Module module, ModuleStream moduleStream, bool unsigned = false)
		{
			DecodeSampleInfo[] decodeSampleInfo = new DecodeSampleInfo[module.Header.NumSamples];

			// Phase 1: Read all the compressed samples into memory.
			// They will then be decompressed in second phase. This is
			// needed because Ogg samples with shared headers may reference
			// a later sample's header
			for (int i = 0; i < module.Header.NumSamples; i++)
			{
				Sample sample = module.Samples[i];

				DecodeSampleInfo sampleInfo = new DecodeSampleInfo
				{
					SampleHeader = sample,
					SharedHeader = sample.SharedOggHeader,
				};

				bool compression = (sample.Flags & SampleInfoFlag.CompressionMask) != 0;

				if (!compression && (sample.CompressedSize == 0))
				{
					// Uncompressed sample, just load it
					uint length = sample.Length;

					if ((sample.Flags & SampleInfoFlag._16Bit) != 0)
						length *= 2;

					if ((sample.Flags & SampleInfoFlag.Stereo) != 0)
						length *= 2;

					sampleInfo.SampleData = ReadSampleData(moduleStream, (int)length);
					if (sampleInfo.SampleData == null)
						return null;
				}
				else if (sample.CompressedSize > 0)
				{
					// Compressed sample. Read the data
					sampleInfo.Chunk = ReadSampleData(moduleStream, sample.CompressedSize);
					if (sampleInfo.Chunk == null)
						return null;
				}

				decodeSampleInfo[i] = sampleInfo;
			}

			// Phase 2: Decompress all the samples
			for (int i = 1; i <= decodeSampleInfo.Length; i++)
			{
				DecodeSampleInfo sampleInfo = decodeSampleInfo[i - 1];
				Sample sample = sampleInfo.SampleHeader;

				if ((sample.CompressedSize < 0) && ((i + sample.CompressedSize) > 0))
				{
					// Duplicate sample
					byte[] sourceSample = decodeSampleInfo[i + sample.CompressedSize].SampleData;

					sampleInfo.SampleData = new byte[sourceSample.Length];
					Array.Copy(sourceSample, sampleInfo.SampleData, sourceSample.Length);
				}
				else
				{
					// Is the sample compressed?
					if ((sample.Length != 0) && (sampleInfo.Chunk != null))
					{
						byte[] sampleData = sampleInfo.Chunk;
						byte numChannels = (byte)((sample.Flags & SampleInfoFlag.Stereo) != 0 ? 2 : 1);
						SampleInfoFlag compression = sample.Flags & SampleInfoFlag.CompressionMask;
						bool unsupportedSample = false;

						void AllocateSample()
						{
							uint length = sample.Length;

							if ((sample.Flags & SampleInfoFlag._16Bit) != 0)
								length *= 2;

							if ((sample.Flags & SampleInfoFlag.Stereo) != 0)
								length *= 2;

							sampleInfo.SampleData = new byte[length];
						}

						if ((compression == SampleInfoFlag.DeltaCompression) || (compression == SampleInfoFlag.DeltaPrediction))
						{
							// In the best case, MO3 compression represents each sample point as two bits.
							// As a result, if we have a file length of n, we know that the sample can be
							// at most n*4 sample points long
							int maxLength = sampleData.Length;
							int maxSamplesPerByte = 4 / numChannels;

							if ((int.MaxValue / maxSamplesPerByte) >= maxLength)
								maxLength *= maxSamplesPerByte;
							else
								maxLength = int.MaxValue;

							if (sample.Length > maxLength)
								sample.Length = (uint)maxLength;
						}

						if ((compression == SampleInfoFlag.DeltaCompression))
						{
							AllocateSample();

							if ((sample.Flags & SampleInfoFlag._16Bit) != 0)
								new DeltaDecoder(new Delta16BitParams()).Decode(sampleData, sampleInfo.SampleData, sample.Length, numChannels);
							else
								new DeltaDecoder(new Delta8BitParams()).Decode(sampleData, sampleInfo.SampleData, sample.Length, numChannels);

							SplitStereoSample(sampleInfo);
						}
						else if (compression == SampleInfoFlag.DeltaPrediction)
						{
							AllocateSample();

							if ((sample.Flags & SampleInfoFlag._16Bit) != 0)
								new DeltaPredictionDecoder(new Delta16BitParams()).Decode(sampleData, sampleInfo.SampleData, sample.Length, numChannels);
							else
								new DeltaPredictionDecoder(new Delta8BitParams()).Decode(sampleData, sampleInfo.SampleData, sample.Length, numChannels);

							SplitStereoSample(sampleInfo);
						}
						else if ((compression == SampleInfoFlag.CompressionOgg) || (compression == SampleInfoFlag.SharedOgg))
						{
							unsupportedSample = !new OggDecoder().Decode(i, sampleInfo, decodeSampleInfo, module.Header);

							SplitStereoSample(sampleInfo);
						}
						else if (compression == SampleInfoFlag.CompressionMpeg)
						{
							unsupportedSample = !new MpegDecoder().Decode(sampleInfo);

							SplitStereoSample(sampleInfo);
						}
						else if (compression == SampleInfoFlag.OplInstrument)
						{
							// The chunk array holds the OPL data uncompressed, so just
							// set the OPL array to this
							sampleInfo.OplData = sampleInfo.Chunk;
						}
						else
							unsupportedSample = true;

						if (unsupportedSample)
							return null;

						if (unsigned)
							ConvertSample(sampleInfo.SampleData, (sampleInfo.SampleHeader.Flags & SampleInfoFlag._16Bit) != 0);
					}
				}
			}

			return decodeSampleInfo;
		}



		/********************************************************************/
		/// <summary>
		/// Will write all the samples
		/// </summary>
		/********************************************************************/
		public static void WriteSamples(DecodeSampleInfo[] decodeSampleInfo, ConverterStream converterStream)
		{
			foreach (DecodeSampleInfo sampleInfo in decodeSampleInfo)
			{
				if (sampleInfo.SampleData != null)
					converterStream.Write(sampleInfo.SampleData, 0, sampleInfo.SampleData.Length);
			}
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Read sample data
		/// </summary>
		/********************************************************************/
		private static byte[] ReadSampleData(ModuleStream moduleStream, int length)
		{
			byte[] sampleData = new byte[length];
			int bytesRead = moduleStream.Read(sampleData, 0, length);

			return bytesRead == length ? sampleData : null;
		}



		/********************************************************************/
		/// <summary>
		/// Convert signed to unsigned sample data
		/// </summary>
		/********************************************************************/
		private static void ConvertSample(byte[] sampleData, bool _16Bit)
		{
			int offset = 0;

			if (_16Bit)
			{
				Span<ushort> span = MemoryMarshal.Cast<byte, ushort>(sampleData);
				int length = span.Length;

				for (; length-- != 0;)
					span[offset++] += 0x8000;
			}
			else
			{
				int length = sampleData.Length;

				for (; length-- != 0;)
					sampleData[offset++] += 0x80;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will split stereo samples into 2 channels
		/// </summary>
		/********************************************************************/
		private static void SplitStereoSample(DecodeSampleInfo sampleInfo)
		{
			if ((sampleInfo.SampleHeader.Flags & SampleInfoFlag.Stereo) != 0)
			{
				// Stereo samples are interleaves, but in IT files, they are stored separately
				byte[] leftChannel = new byte[sampleInfo.SampleData.Length / 2];
				byte[] rightChannel = new byte[sampleInfo.SampleData.Length / 2];

				if ((sampleInfo.SampleHeader.Flags & SampleInfoFlag._16Bit) != 0)
				{
					Span<short> sampleData = MemoryMarshal.Cast<byte, short>(sampleInfo.SampleData);
					Span<short> leftSpan = MemoryMarshal.Cast<byte, short>(leftChannel);
					Span<short> rightSpan = MemoryMarshal.Cast<byte, short>(rightChannel);

					for (int i = 0, j = 0; i < leftSpan.Length; i++, j += 2)
					{
						leftSpan[i] = sampleData[j];
						rightSpan[i] = sampleData[j + 1];
					}
				}
				else
				{
					for (int i = 0, j = 0; i < leftChannel.Length; i++, j += 2)
					{
						leftChannel[i] = sampleInfo.SampleData[j];
						rightChannel[i] = sampleInfo.SampleData[j + 1];
					}
				}

				Array.Copy(leftChannel, 0, sampleInfo.SampleData, 0, leftChannel.Length);
				Array.Copy(rightChannel, 0, sampleInfo.SampleData, leftChannel.Length, rightChannel.Length);
			}
		}
		#endregion
	}
}
