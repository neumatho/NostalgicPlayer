/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.IO;
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Agent.ModuleConverter.Mo3Converter.Containers;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.LibVorbis;
using Polycode.NostalgicPlayer.Ports.LibVorbis.Containers;
using Polycode.NostalgicPlayer.Ports.LibVorbisFile;

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

				ms.Seek(0, SeekOrigin.Begin);
			}
			else
				ms = new MemoryStream(sampleInfo.Chunk);

			if (!VorbisDecode(ms, out short[] decodedBuffer))
				return false;

			if ((sampleInfo.SampleHeader.Flags & SampleInfoFlag._16Bit) == 0)
			{
				sampleInfo.SampleData = new byte[decodedBuffer.Length];
				Span<sbyte> pcm8 = MemoryMarshal.Cast<byte, sbyte>(sampleInfo.SampleData);

				for (int i = 0; i < decodedBuffer.Length; i++)
					pcm8[i] = (sbyte)(decodedBuffer[i] >> 8);
			}
			else
			{
				sampleInfo.SampleData = new byte[decodedBuffer.Length * 2];
				decodedBuffer.CopyTo(MemoryMarshal.Cast<byte, short>(sampleInfo.SampleData));
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private bool VorbisDecode(MemoryStream ms, out short[] decodedBuffer)
		{
			VorbisError result = VorbisFile.Ov_Open(ms, false, out VorbisFile vorbisFile, null, 0);
			if (result != VorbisError.Ok)
			{
				decodedBuffer = null;

				return false;
			}

			long todo = vorbisFile.Ov_Pcm_Total(-1);

			VorbisInfo info = vorbisFile.Ov_Info(-1);

			decodedBuffer = new short[todo * info.channels];

			int offset = 0;

			while (todo > 0)
			{
				int done = vorbisFile.Ov_Read_Float(out CPointer<float>[] buffer, (int)todo, out _);
				if (done == (int)VorbisError.Hole)
					continue;

				if (done <= 0)
					break;

				// Copy the samples into one buffer
				for (int i = 0; i < done; i++)
				{
					for (int j = 0; j < info.channels; j++)
						decodedBuffer[offset++] = (short)Math.Clamp(buffer[j][i] * 32767, -32768, 32767);
				}

				todo -= done;
			}

			vorbisFile.Ov_Clear();

			return true;
		}
	}
}
