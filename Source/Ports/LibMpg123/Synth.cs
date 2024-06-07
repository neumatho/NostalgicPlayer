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
	/// Generic synth functions
	/// </summary>
	internal class Synth
	{
		private const Real Real_Plus_32767 = 32767.0f;
		private const Real Real_Minus_32768 = -32768.0f;

		public delegate void Write_Sample<SAMPLE_T>(ref SAMPLE_T sample, Real sum, ref c_int clip);

		private readonly LibMpg123 lib;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Synth(LibMpg123 libMpg123)
		{
			lib = libMpg123;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Int123_Synth_1To1(Memory<Real> bandPtr, c_int channel, Mpg123_Handle fr, bool final)
		{
			return lib.synth.DoSynth<c_short>(bandPtr, channel, fr, final, 0x40, Write_Short_Sample);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Int123_Synth_1To1_Mono(Memory<Real> bandPtr, Mpg123_Handle fr)
		{
			return lib.synth_Mono.DoMonoSynth<c_short>(bandPtr, fr, 0x40, fr.Synths.Plain[(int)Synth_Resample.OneToOne, (int)Synth_Format.Sixteen]);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Int123_Synth_1To1_M2S(Memory<Real> bandPtr, Mpg123_Handle fr)
		{
			return lib.synth_Mono.DoMono2StereoSynth<c_short>(bandPtr, fr, 0x40, fr.Synths.Plain[(int)Synth_Resample.OneToOne, (int)Synth_Format.Sixteen]);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Int123_Synth_2To1(Memory<Real> bandPtr, int channel, Mpg123_Handle fr, bool final)
		{
			return lib.synth.DoSynth<c_short>(bandPtr, channel, fr, final, 0x20, Write_Short_Sample);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Int123_Synth_2To1_Mono(Memory<Real> bandPtr, Mpg123_Handle fr)
		{
			return lib.synth_Mono.DoMonoSynth<c_short>(bandPtr, fr, 0x20, fr.Synths.Plain[(int)Synth_Resample.TwoToOne, (int)Synth_Format.Sixteen]);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Int123_Synth_2To1_M2S(Memory<Real> bandPtr, Mpg123_Handle fr)
		{
			return lib.synth_Mono.DoMono2StereoSynth<c_short>(bandPtr, fr, 0x20, fr.Synths.Plain[(int)Synth_Resample.TwoToOne, (int)Synth_Format.Sixteen]);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Int123_Synth_4To1(Memory<Real> bandPtr, int channel, Mpg123_Handle fr, bool final)
		{
			return lib.synth.DoSynth<c_short>(bandPtr, channel, fr, final, 0x10, Write_Short_Sample);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Int123_Synth_4To1_Mono(Memory<Real> bandPtr, Mpg123_Handle fr)
		{
			return lib.synth_Mono.DoMonoSynth<c_short>(bandPtr, fr, 0x10, fr.Synths.Plain[(int)Synth_Resample.FourToOne, (int)Synth_Format.Sixteen]);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Int123_Synth_4To1_M2S(Memory<Real> bandPtr, Mpg123_Handle fr)
		{
			return lib.synth_Mono.DoMono2StereoSynth<c_short>(bandPtr, fr, 0x10, fr.Synths.Plain[(int)Synth_Resample.FourToOne, (int)Synth_Format.Sixteen]);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Int123_Synth_NToM_Mono(Memory<Real> bandPtr, Mpg123_Handle fr)
		{
			return lib.synth_NToM.DoMono<c_short>(bandPtr, fr, Write_Short_Sample);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Int123_Synth_NToM_M2S(Memory<Real> bandPtr, Mpg123_Handle fr)
		{
			return lib.synth_NToM.DoMono2Stereo<c_short>(bandPtr, fr, Write_Short_Sample);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Int123_Synth_NToM(Memory<Real> bandPtr, c_int channel, Mpg123_Handle fr, bool final)
		{
			return lib.synth_NToM.DoSynth<c_short>(bandPtr, channel, fr, final, Write_Short_Sample);
		}



		/********************************************************************/
		/// <summary>
		/// Main synth function
		/// </summary>
		/********************************************************************/
		public c_int DoSynth<SAMPLE_T>(Memory<Real> bandPtr, c_int channel, Mpg123_Handle fr, bool final, c_int block, Write_Sample<SAMPLE_T> write_Sample) where SAMPLE_T : struct
		{
			const c_int BackPedal = 0x10;     // We use autoincrement and thus need this re-adjustment for window/b0
			Action<Memory<Real>, Memory<Real>, Memory<Real>> myDct64 = lib.dct64.Int123_Dct64;
			const c_int Step = 2;

			Span<SAMPLE_T> samples = MemoryMarshal.Cast<byte, SAMPLE_T>(fr.Buffer.Data.AsSpan((int)fr.Buffer.Fill));
			int samplesOffset = 0;

			Memory<Real>[] buf;
			Span<Real> b0;
			c_int bo1;
			c_int clip = 0;

			if (channel == 0)
			{
				fr.Bo--;
				fr.Bo &= 0xf;
				buf = fr.Real_Buffs[0];
			}
			else
			{
				samplesOffset++;
				buf = fr.Real_Buffs[1];
			}

			if ((fr.Bo & 0x1) != 0)
			{
				b0 = buf[0].Span;
				bo1 = fr.Bo;
				myDct64(buf[1].Slice((fr.Bo + 1) & 0xf), buf[0].Slice(fr.Bo), bandPtr);
			}
			else
			{
				b0 = buf[1].Span;
				bo1 = fr.Bo + 1;
				myDct64(buf[0].Slice(fr.Bo), buf[1].Slice(fr.Bo + 1), bandPtr);
			}

			{
				Span<Real> window = fr.DecWin.Slice(16 - bo1).Span;
				int windowOffset = 0;
				int b0Offset = 0;

				for (c_int j = block / 4; j != 0; j--, b0Offset += 0x400 / block - BackPedal, windowOffset += 0x800 / block - BackPedal, samplesOffset += Step)
				{
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

					write_Sample(ref samples[samplesOffset], sum, ref clip);
				}

				{
					Real sum = Helpers.Real_Mul_Synth(window[windowOffset + 0x0], b0[b0Offset + 0x0]);
					sum += Helpers.Real_Mul_Synth(window[windowOffset + 0x2], b0[b0Offset + 0x2]);
					sum += Helpers.Real_Mul_Synth(window[windowOffset + 0x4], b0[b0Offset + 0x4]);
					sum += Helpers.Real_Mul_Synth(window[windowOffset + 0x6], b0[b0Offset + 0x6]);
					sum += Helpers.Real_Mul_Synth(window[windowOffset + 0x8], b0[b0Offset + 0x8]);
					sum += Helpers.Real_Mul_Synth(window[windowOffset + 0xa], b0[b0Offset + 0xa]);
					sum += Helpers.Real_Mul_Synth(window[windowOffset + 0xc], b0[b0Offset + 0xc]);
					sum += Helpers.Real_Mul_Synth(window[windowOffset + 0xe], b0[b0Offset + 0xe]);

					write_Sample(ref samples[samplesOffset], sum, ref clip);
					samplesOffset += Step;
					b0Offset -= 0x400 / block;
					windowOffset -= 0x800 / block;
				}

				windowOffset += bo1 << 1;

				for (c_int j = block / 4 - 1; j != 0; j--, b0Offset -= 0x400 / block + BackPedal, windowOffset -= 0x800 / block - BackPedal, samplesOffset += Step)
				{
					Real sum = -Helpers.Real_Mul_Synth(window[--windowOffset], b0[b0Offset++]);
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

					write_Sample(ref samples[samplesOffset], sum, ref clip);
				}
			}

			if (final)
				fr.Buffer.Fill += (size_t)(block * Marshal.SizeOf(default(SAMPLE_T)));

			return clip;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Method to produce a short (signed 16bit) output sample from
		/// internal representation, which may be float, double or indeed
		/// some integer for fixed point handling
		/// </summary>
		/********************************************************************/
		private void Write_Short_Sample(ref int16_t sample, Real sum, ref c_int clip)
		{
			if (sum > Real_Plus_32767)
			{
				sample = 0x7fff;
				clip++;
			}
			else if (sum < Real_Minus_32768)
			{
				sample = -0x8000;
				clip++;
			}
			else
				sample = Real_To_Short(sum);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private int16_t Real_To_Short(Real x)
		{
			return (int16_t)x;
		}
		#endregion
	}
}
