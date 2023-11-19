/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Ports.LibMpg123.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibMpg123
{
	/// <summary>
	/// 
	/// </summary>
	internal class Synth_S32
	{
		private const Real Real_Plus_S32 = 2147483647.0f;
		private const Real Real_Minus_S32 = -2147483648.0f;

		private readonly LibMpg123 lib;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Synth_S32(LibMpg123 libMpg123)
		{
			lib = libMpg123;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Int123_Synth_1To1_S32(Memory<Real> bandPtr, c_int channel, Mpg123_Handle fr, bool final)
		{
			return lib.synth.DoSynth<int32_t>(bandPtr, channel, fr, final, 0x40, Write_S32_Sample);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Int123_Synth_1To1_S32_Mono(Memory<Real> bandPtr, Mpg123_Handle fr)
		{
			return lib.synth_Mono.DoMonoSynth<int32_t>(bandPtr, fr, 0x40, fr.Synths.Plain[(int)Synth_Resample.OneToOne, (int)Synth_Format.ThirtyTwo]);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Int123_Synth_1To1_S32_M2S(Memory<Real> bandPtr, Mpg123_Handle fr)
		{
			return lib.synth_Mono.DoMono2StereoSynth<int32_t>(bandPtr, fr, 0x40, fr.Synths.Plain[(int)Synth_Resample.OneToOne, (int)Synth_Format.ThirtyTwo]);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Int123_Synth_2To1_S32(Memory<Real> bandPtr, int channel, Mpg123_Handle fr, bool final)
		{
			return lib.synth.DoSynth<int32_t>(bandPtr, channel, fr, final, 0x20, Write_S32_Sample);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Int123_Synth_2To1_S32_Mono(Memory<Real> bandPtr, Mpg123_Handle fr)
		{
			return lib.synth_Mono.DoMonoSynth<int32_t>(bandPtr, fr, 0x20, fr.Synths.Plain[(int)Synth_Resample.TwoToOne, (int)Synth_Format.ThirtyTwo]);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Int123_Synth_2To1_S32_M2S(Memory<Real> bandPtr, Mpg123_Handle fr)
		{
			return lib.synth_Mono.DoMono2StereoSynth<int32_t>(bandPtr, fr, 0x20, fr.Synths.Plain[(int)Synth_Resample.TwoToOne, (int)Synth_Format.ThirtyTwo]);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Int123_Synth_4To1_S32(Memory<Real> bandPtr, int channel, Mpg123_Handle fr, bool final)
		{
			return lib.synth.DoSynth<int32_t>(bandPtr, channel, fr, final, 0x10, Write_S32_Sample);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Int123_Synth_4To1_S32_Mono(Memory<Real> bandPtr, Mpg123_Handle fr)
		{
			return lib.synth_Mono.DoMonoSynth<int32_t>(bandPtr, fr, 0x10, fr.Synths.Plain[(int)Synth_Resample.FourToOne, (int)Synth_Format.ThirtyTwo]);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Int123_Synth_4To1_S32_M2S(Memory<Real> bandPtr, Mpg123_Handle fr)
		{
			return lib.synth_Mono.DoMono2StereoSynth<int32_t>(bandPtr, fr, 0x10, fr.Synths.Plain[(int)Synth_Resample.FourToOne, (int)Synth_Format.ThirtyTwo]);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Int123_Synth_NToM_S32_Mono(Memory<Real> bandPtr, Mpg123_Handle fr)
		{
			return lib.synth_NToM.DoMono<int32_t>(bandPtr, fr, Write_S32_Sample);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Int123_Synth_NToM_S32_M2S(Memory<Real> bandPtr, Mpg123_Handle fr)
		{
			return lib.synth_NToM.DoMono2Stereo<int32_t>(bandPtr, fr, Write_S32_Sample);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Int123_Synth_NToM_S32(Memory<Real> bandPtr, c_int channel, Mpg123_Handle fr, bool final)
		{
			return lib.synth_NToM.DoSynth<int32_t>(bandPtr, channel, fr, final, Write_S32_Sample);
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// We do clipping with the same old borders... but different
		/// conversion.
		/// We see here that we need extra work for non-16bit output... we
		/// optimized for 16bit.
		/// -0x7fffffff-1 is the minimum 32 bit signed integer value
		/// expressed so that MSVC does not give a compile time warning
		/// </summary>
		/********************************************************************/
		private void Write_S32_Sample(ref int32_t sample, Real sum, ref c_int clip)
		{
			Real tmpSum = Helpers.Real_Mul(sum, Constant.S32_Rescale);

			if (tmpSum > Real_Plus_S32)
			{
				sample = 0x7fffffff;
				clip++;
			}
			else if (tmpSum < Real_Minus_S32)
			{
				sample = -0x7fffffff - 1;
				clip++;
			}
			else
				sample = Real_To_S32(tmpSum);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private int32_t Real_To_S32(Real x)
		{
			return (int32_t)x;
		}
		#endregion
	}
}
