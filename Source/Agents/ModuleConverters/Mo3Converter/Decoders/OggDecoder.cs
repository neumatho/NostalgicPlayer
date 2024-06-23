/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.IO;
using System.Runtime.InteropServices;
using NVorbis;
using Polycode.NostalgicPlayer.Agent.ModuleConverter.Mo3Converter.Containers;

namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.Mo3Converter.Decoders
{
	/// <summary>
	/// Decode an Ogg sample back to normal PCM
	/// </summary>
	internal class OggDecoder
	{
		/********************************************************************/
		/// <summary>
		/// Will decode the given sample
		/// </summary>
		/********************************************************************/
		public bool Decode(int smp, DecodeSampleInfo sampleInfo, DecodeSampleInfo[] allSamples, FileHeader header)
		{
			ushort sharedHeaderSize = sampleInfo.SampleHeader.EncoderDelay;
			int sharedOggHeader = ((smp + sampleInfo.SharedHeader) > 0) ? (smp + sampleInfo.SharedHeader) : smp;

			// Which chunk are we going to read the header from?
			bool sharedHeader = (sharedOggHeader != smp) && (sharedOggHeader > 0) && (sharedOggHeader <= header.NumSamples) && (sharedHeaderSize > 0);

			MemoryStream ms;

			if (sharedHeader)
			{
				// Prepend the shared header to the actual sample data
				byte[] sharedChunk = allSamples[sharedOggHeader - 1].Chunk;

				ms = new MemoryStream(sharedHeaderSize + sampleInfo.Chunk.Length);
				ms.Write(sharedChunk, 0, sharedHeaderSize);
				ms.Write(sampleInfo.Chunk);
			}
			else
				ms = new MemoryStream(sampleInfo.Chunk);

			float[] decodedBuffer;

			try
			{
				using (VorbisReader vorbis = new VorbisReader(ms))
				{
					decodedBuffer = new float[vorbis.TotalSamples * vorbis.Channels];

					if (vorbis.ReadSamples(decodedBuffer) < (vorbis.TotalSamples - 1))
						return false;
				}
			}
			catch (Exception)
			{
				return false;
			}

			if ((sampleInfo.SampleHeader.Flags & SampleInfoFlag._16Bit) == 0)
			{
				sampleInfo.SampleData = new byte[decodedBuffer.Length];
				Span<sbyte> pcm8 = MemoryMarshal.Cast<byte, sbyte>(sampleInfo.SampleData);

				for (int i = 0; i < decodedBuffer.Length; i++)
					pcm8[i] = (sbyte)((short)(decodedBuffer[i] * 32767.0f) >> 8);
			}
			else
			{
				sampleInfo.SampleData = new byte[decodedBuffer.Length * 2];
				Span<short> pcm16 = MemoryMarshal.Cast<byte, short>(sampleInfo.SampleData);

				for (int i = 0; i < decodedBuffer.Length; i++)
					pcm16[i] = (short)(decodedBuffer[i] * 32767.0f);
			}

			return true;
		}
	}
}
