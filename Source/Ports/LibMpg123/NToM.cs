/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Ports.LibMpg123.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibMpg123
{
	/// <summary>
	/// N->M down/up sampling; the setup code
	/// </summary>
	internal class NToM
	{
		private readonly LibMpg123 lib;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public NToM(LibMpg123 libMpg123)
		{
			lib = libMpg123;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Int123_Synth_NToM_Set_Step(Mpg123_Handle fr)
		{
			c_long m = lib.parse.Int123_Frame_Freq(fr);
			c_long n = fr.Af.Rate;

			if ((n > Constant.NToM_Max_Freq) || (m > Constant.NToM_Max_Freq) || (m <= 0) || (n <= 0))
			{
				fr.Err = Mpg123_Errors.Bad_Rate;
				return -1;
			}

			n *= Constant.NToM_Mul;
			fr.NToM_Step = (c_ulong)(n / m);

			if (fr.NToM_Step > Constant.NToM_Max * Constant.NToM_Mul)
			{
				fr.Err = Mpg123_Errors.Bad_Rate;
				return -1;
			}

			fr.Int123_NToM_Val[0] = fr.Int123_NToM_Val[1] = Int123_NToM_Val(fr, fr.Num);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Set the ntom value for next expected frame to be decoded.
		/// This is for keeping output consistent across seeks
		/// </summary>
		/********************************************************************/
		public void Int123_NToM_Set_NToM(Mpg123_Handle fr, int64_t num)
		{
			fr.Int123_NToM_Val[1] = fr.Int123_NToM_Val[0] = Int123_NToM_Val(fr, num);
		}



		/********************************************************************/
		/// <summary>
		/// Carry out the ntom sample count operation for this one frame.
		/// No fear of integer overflow here
		/// </summary>
		/********************************************************************/
		public int64_t Int123_NToM_Frame_OutSamples(Mpg123_Handle fr)
		{
			// The do this before decoding the separate channels, so there is
			// only one common ntom value
			c_int ntm = (c_int)fr.Int123_NToM_Val[0];
			ntm += (c_int)((c_ulong)fr.Spf * fr.NToM_Step);

			return ntm / Constant.NToM_Mul;
		}



		/********************************************************************/
		/// <summary>
		/// Convert frame offset to unadjusted output sample offset
		/// </summary>
		/********************************************************************/
		public int64_t Int123_NToM_FrmOuts(Mpg123_Handle fr, int64_t frame)
		{
			int64_t sOff = 0;
			int64_t ntm = (int64_t)Int123_NToM_Val(fr, 0);

			if (frame <= 0)
				return 0;

			for (int64_t f = 0; f < frame; ++f)
			{
				ntm += (int64_t)((c_ulong)fr.Spf * fr.NToM_Step);
				sOff += ntm / Constant.NToM_Mul;
				ntm -= (ntm / Constant.NToM_Mul) * Constant.NToM_Mul;
			}

			return sOff;
		}



		/********************************************************************/
		/// <summary>
		/// Convert input samples to unadjusted output samples
		/// </summary>
		/********************************************************************/
		public int64_t Int123_NToM_Ins2Outs(Mpg123_Handle fr, int64_t ins)
		{
			int64_t sOff = 0;
			int64_t ntm = (int64_t)Int123_NToM_Val(fr, 0);

			{
				int64_t block = fr.Spf;

				if (ins <= 0)
					return 0;

				do
				{
					int64_t nowBlock = ins > block ? block : ins;
					ntm += (int64_t)((c_ulong)nowBlock * fr.NToM_Step);
					sOff += ntm / Constant.NToM_Mul;
					ntm -= (ntm / Constant.NToM_Mul) * Constant.NToM_Mul;
					ins -= nowBlock;
				}
				while (ins > 0);
			}

			return sOff;
		}



		/********************************************************************/
		/// <summary>
		/// Determine frame offset from unadjusted output sample offset
		/// </summary>
		/********************************************************************/
		public int64_t Int123_NToM_FrameOff(Mpg123_Handle fr, int64_t sOff)
		{
			int64_t iOff = 0;	// Frames or samples
			int64_t ntm = (int64_t)Int123_NToM_Val(fr, 0);

			if (sOff <= 0)
				return 0;

			for (iOff = 0; true; ++iOff)
			{
				ntm += (int64_t)((c_ulong)fr.Spf * fr.NToM_Step);

				if ((ntm / Constant.NToM_Mul) > sOff)
					break;

				sOff -= ntm / Constant.NToM_Mul;
				ntm -= (ntm / Constant.NToM_Mul) * Constant.NToM_Mul;
			}

			return iOff;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_ulong Int123_NToM_Val(Mpg123_Handle fr, int64_t frame)
		{
			int64_t ntm = Constant.NToM_Mul >> 1;	// For frame 0

			for (int64_t f = 0; f < frame; ++f)	// For frame > 0
			{
				ntm += fr.Spf * (off_t)fr.NToM_Step;
				ntm -= (ntm / Constant.NToM_Mul) * Constant.NToM_Mul;
			}

			return (c_ulong)ntm;
		}
		#endregion
	}
}
