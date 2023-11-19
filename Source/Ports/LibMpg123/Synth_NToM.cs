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
	/// N-to-M resampling synth functions
	/// </summary>
	internal class Synth_NToM
	{
		private readonly LibMpg123 lib;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Synth_NToM(LibMpg123 libMpg123)
		{
			lib = libMpg123;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int DoMono<SAMPLE_T>(Memory<Real> bandPtr, Mpg123_Handle fr, Synth.Write_Sample<SAMPLE_T> write_Sample) where SAMPLE_T : struct
		{
			c_int sample_T_Size = Marshal.SizeOf(default(SAMPLE_T));

			c_uchar[] samples_Byte = new c_uchar[8 * 64 * sample_T_Size];
			Span<SAMPLE_T> samples_Tmp = MemoryMarshal.Cast<c_uchar, SAMPLE_T>(samples_Byte);
			Span<SAMPLE_T> tmp1 = samples_Tmp;
			int tmp1Offset = 0;

			size_t pnt = fr.Buffer.Fill;
			c_uchar[] samples = fr.Buffer.Data;
			fr.Buffer.Data = samples_Byte;
			fr.Buffer.Fill = 0;
			c_int ret = DoSynth(bandPtr, 0, fr, true, write_Sample);
			fr.Buffer.Data = samples;

			Span<SAMPLE_T> samplesSpan = MemoryMarshal.Cast<c_uchar, SAMPLE_T>(samples);
			int samplesOffset = (int)pnt / sample_T_Size;

			for (size_t i = 0; i < (fr.Buffer.Fill / (size_t)(2 * sample_T_Size)); i++)
			{
				samplesSpan[samplesOffset] = tmp1[tmp1Offset];
				samplesOffset++;
				tmp1Offset += 2;
			}

			fr.Buffer.Fill = pnt + (fr.Buffer.Fill / 2);

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int DoMono2Stereo<SAMPLE_T>(Memory<Real> bandPtr, Mpg123_Handle fr, Synth.Write_Sample<SAMPLE_T> write_Sample) where SAMPLE_T : struct
		{
			c_int sample_T_Size = Marshal.SizeOf(default(SAMPLE_T));

			size_t pnt1 = fr.Buffer.Fill;
			Span<SAMPLE_T> samples = MemoryMarshal.Cast<c_uchar, SAMPLE_T>(fr.Buffer.Data);
			int samplesOffset = (int)pnt1 / sample_T_Size;

			c_int ret = DoSynth(bandPtr, 0, fr, true, write_Sample);

			for (size_t i = 0; i < ((fr.Buffer.Fill - pnt1) / (size_t)(2 * sample_T_Size)); i++)
			{
				samples[samplesOffset + 1] = samples[samplesOffset];
				samplesOffset += 2;
			}

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// Main synth function
		/// </summary>
		/********************************************************************/
		public c_int DoSynth<SAMPLE_T>(Memory<Real> bandPtr, c_int channel, Mpg123_Handle fr, bool final, Synth.Write_Sample<SAMPLE_T> write_Sample) where SAMPLE_T : struct
		{
			const c_int Step = 2;

			Span<SAMPLE_T> samples = MemoryMarshal.Cast<byte, SAMPLE_T>(fr.Buffer.Data.AsSpan((int)fr.Buffer.Fill));
			int samplesOffset = 0;

			Memory<Real>[] buf;
			c_int clip = 0;
			Span<Real> b0;
			c_int bo1;
			c_ulong ntom;

			if (channel == 0)
			{
				fr.Bo--;
				fr.Bo &= 0xf;
				buf = fr.Real_Buffs[0];
				ntom = fr.Int123_NToM_Val[1] = fr.Int123_NToM_Val[0];
			}
			else
			{
				samplesOffset = 1;
				buf = fr.Real_Buffs[1];
				ntom = fr.Int123_NToM_Val[1];
			}

			if ((fr.Bo & 0x1) != 0)
			{
				b0 = buf[0].Span;
				bo1 = fr.Bo;
				lib.dct64.Int123_Dct64(buf[1].Slice((fr.Bo + 1) & 0xf), buf[0].Slice(fr.Bo), bandPtr);
			}
			else
			{
				b0 = buf[1].Span;
				bo1 = fr.Bo + 1;
				lib.dct64.Int123_Dct64(buf[0].Slice(fr.Bo), buf[1].Slice(fr.Bo + 1), bandPtr);
			}

			{
				Span<Real> window = fr.DecWin.Slice(16 - bo1).Span;
				int windowOffset = 0;
				int b0Offset = 0;

				for (c_int j = 16; j != 0; j--, windowOffset += 0x10)
				{
					ntom += fr.NToM_Step;

					if (ntom < Constant.NToM_Mul)
					{
						windowOffset += 16;
						b0Offset += 16;
						continue;
					}

					Real sum = Helpers.Real_Mul_Synth(window[windowOffset++], b0[b0Offset++]);
					sum -= Helpers.Real_Mul_Synth(window[windowOffset++], b0[b0Offset++]);
					sum += Helpers.Real_Mul_Synth(window[windowOffset++], b0[b0Offset++]);
					sum -= Helpers.Real_Mul_Synth(window[windowOffset++], b0[b0Offset++]);
					sum += Helpers.Real_Mul_Synth(window[windowOffset++], b0[b0Offset++]);
					sum -= Helpers.Real_Mul_Synth(window[windowOffset++], b0[b0Offset++]);
					sum += Helpers.Real_Mul_Synth(window[windowOffset++], b0[b0Offset++]);
					sum -= Helpers.Real_Mul_Synth(window[windowOffset++], b0[b0Offset++]);
					sum += Helpers.Real_Mul_Synth(window[windowOffset++], b0[b0Offset++]);
					sum -= Helpers.Real_Mul_Synth(window[windowOffset++], b0[b0Offset++]);
					sum += Helpers.Real_Mul_Synth(window[windowOffset++], b0[b0Offset++]);
					sum -= Helpers.Real_Mul_Synth(window[windowOffset++], b0[b0Offset++]);
					sum += Helpers.Real_Mul_Synth(window[windowOffset++], b0[b0Offset++]);
					sum -= Helpers.Real_Mul_Synth(window[windowOffset++], b0[b0Offset++]);
					sum += Helpers.Real_Mul_Synth(window[windowOffset++], b0[b0Offset++]);
					sum -= Helpers.Real_Mul_Synth(window[windowOffset++], b0[b0Offset++]);

					while (ntom >= Constant.NToM_Mul)
					{
						write_Sample(ref samples[samplesOffset], sum, ref clip);
						samplesOffset += Step;
						ntom -= Constant.NToM_Mul;
					}
				}

				ntom += fr.NToM_Step;

				if (ntom >= Constant.NToM_Mul)
				{
					Real sum = Helpers.Real_Mul_Synth(window[0x0], b0[0x0]);
					sum += Helpers.Real_Mul_Synth(window[0x2], b0[0x2]);
					sum += Helpers.Real_Mul_Synth(window[0x4], b0[0x4]);
					sum += Helpers.Real_Mul_Synth(window[0x6], b0[0x6]);
					sum += Helpers.Real_Mul_Synth(window[0x8], b0[0x8]);
					sum += Helpers.Real_Mul_Synth(window[0xa], b0[0xa]);
					sum += Helpers.Real_Mul_Synth(window[0xc], b0[0xc]);
					sum += Helpers.Real_Mul_Synth(window[0xe], b0[0xe]);

					while (ntom >= Constant.NToM_Mul)
					{
						write_Sample(ref samples[samplesOffset], sum, ref clip);
						samplesOffset += Step;
						ntom -= Constant.NToM_Mul;
					}
				}

				b0Offset -= 0x10;
				windowOffset -= 0x20;
				windowOffset += bo1 << 1;

				for (c_int j = 15; j != 0; j--, b0Offset -= 0x20, windowOffset -= 0x10)
				{
					ntom += fr.NToM_Step;

					if (ntom < Constant.NToM_Mul)
					{
						windowOffset -= 16;
						b0Offset += 16;
						continue;
					}

					Real sum = Helpers.Real_Mul_Synth(window[--windowOffset], b0[b0Offset++]);
					sum -= Helpers.Real_Mul_Synth(window[--windowOffset], b0[b0Offset++]);
					sum -= Helpers.Real_Mul_Synth(window[--windowOffset], b0[b0Offset++]);
					sum -= Helpers.Real_Mul_Synth(window[--windowOffset], b0[b0Offset++]);
					sum -= Helpers.Real_Mul_Synth(window[--windowOffset], b0[b0Offset++]);
					sum -= Helpers.Real_Mul_Synth(window[--windowOffset], b0[b0Offset++]);
					sum -= Helpers.Real_Mul_Synth(window[--windowOffset], b0[b0Offset++]);
					sum -= Helpers.Real_Mul_Synth(window[--windowOffset], b0[b0Offset++]);
					sum -= Helpers.Real_Mul_Synth(window[--windowOffset], b0[b0Offset++]);
					sum -= Helpers.Real_Mul_Synth(window[--windowOffset], b0[b0Offset++]);
					sum -= Helpers.Real_Mul_Synth(window[--windowOffset], b0[b0Offset++]);
					sum -= Helpers.Real_Mul_Synth(window[--windowOffset], b0[b0Offset++]);
					sum -= Helpers.Real_Mul_Synth(window[--windowOffset], b0[b0Offset++]);
					sum -= Helpers.Real_Mul_Synth(window[--windowOffset], b0[b0Offset++]);
					sum -= Helpers.Real_Mul_Synth(window[--windowOffset], b0[b0Offset++]);
					sum -= Helpers.Real_Mul_Synth(window[--windowOffset], b0[b0Offset++]);

					while (ntom >= Constant.NToM_Mul)
					{
						write_Sample(ref samples[samplesOffset], sum, ref clip);
						samplesOffset += Step;
						ntom -= Constant.NToM_Mul;
					}
				}
			}

			fr.Int123_NToM_Val[channel] = ntom;

			if (final)
				fr.Buffer.Fill = (size_t)(samplesOffset - (channel != 0 ? Marshal.SizeOf(default(SAMPLE_T)) : 0));

			return clip;
		}
	}
}
