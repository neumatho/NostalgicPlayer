/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Ports.LibMpg123.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibMpg123
{
	/// <summary>
	/// Generic mono related synth functions
	/// </summary>
	internal class Synth_Mono
	{
		/********************************************************************/
		/// <summary>
		/// Mono synth
		/// </summary>
		/********************************************************************/
		public int DoMonoSynth<SAMPLE_T>(Memory<Real> bandPtr, Mpg123_Handle fr, c_int block, Synth_S.Func_Synth sample_Name) where SAMPLE_T : struct
		{
			c_int sample_T_Size = Marshal.SizeOf(default(SAMPLE_T));

			c_uchar[] samples_Byte = new c_uchar[block * sample_T_Size];
			Span<SAMPLE_T> samples_Tmp = MemoryMarshal.Cast<c_uchar, SAMPLE_T>(samples_Byte);
			Span<SAMPLE_T> tmp1 = samples_Tmp;
			int tmp1Offset = 0;

			// Save buffer stuff, trick samples tmp into there, decode, restore
			c_uchar[] samples = fr.Buffer.Data;
			c_int pnt = (int)fr.Buffer.Fill;
			fr.Buffer.Data = samples_Byte;
			fr.Buffer.Fill = 0;
			c_int ret = sample_Name(bandPtr, 0, fr, false);	// Decode into samples_tmp
			fr.Buffer.Data = samples;		// Restore original value

			// Now append samples from samples_tmp
			Span<SAMPLE_T> samplesSpan = MemoryMarshal.Cast<c_uchar, SAMPLE_T>(samples);
			int samplesOffset = pnt / sample_T_Size;	// Just the next mem in frame buffer

			for (c_int i = 0; i < (block / 2); i++)
			{
				samplesSpan[samplesOffset] = tmp1[tmp1Offset];
				samplesOffset++;
				tmp1Offset += 2;
			}

			fr.Buffer.Fill = (size_t)(pnt + (block / 2) * sample_T_Size);

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// Mono to stereo synth
		/// </summary>
		/********************************************************************/
		public c_int DoMono2StereoSynth<SAMPLE_T>(Memory<Real> bandPtr, Mpg123_Handle fr, c_int block, Synth_S.Func_Synth sample_Name) where SAMPLE_T : struct
		{
			c_int sample_T_Size = Marshal.SizeOf(default(SAMPLE_T));

			c_uchar[] samples = fr.Buffer.Data;

			c_int ret = sample_Name(bandPtr, 0, fr, false);

			int samplesOffset = (int)fr.Buffer.Fill - block * sample_T_Size;

			for (c_int i = 0; i < (block / 2); i++)
			{
				samples[samplesOffset + 1] = samples[samplesOffset];
				samplesOffset += 2;
			}

			return ret;
		}
	}
}
