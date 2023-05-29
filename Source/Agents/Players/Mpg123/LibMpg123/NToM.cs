/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Agent.Player.Mpg123.LibMpg123.Containers;

namespace Polycode.NostalgicPlayer.Agent.Player.Mpg123.LibMpg123
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
		public c_int Synth_NToM_Set_Step(Mpg123_Handle fr)
		{
			c_long m = lib.parse.Frame_Freq(fr);
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

			fr.NToM_Val[0] = fr.NToM_Val[1] = NToM_Val(fr, fr.Num);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Set the ntom value for next expected frame to be decoded.
		/// This is for keeping output consistent across seeks
		/// </summary>
		/********************************************************************/
		public void NToM_Set_NToM(Mpg123_Handle fr, off_t num)
		{
			fr.NToM_Val[1] = fr.NToM_Val[0] = NToM_Val(fr, num);
		}



		/********************************************************************/
		/// <summary>
		/// Carry out the ntom sample count operation for this one frame.
		/// No fear of integer overflow here
		/// </summary>
		/********************************************************************/
		public off_t NToM_Frame_OutSamples(Mpg123_Handle fr)
		{
			// The do this before decoding the separate channels, so there is
			// only one common ntom value
			c_int ntm = (c_int)fr.NToM_Val[0];
			ntm += (c_int)(fr.Spf * fr.NToM_Step);

			return ntm / Constant.NToM_Mul;
		}



		/********************************************************************/
		/// <summary>
		/// Convert frame offset to unadjusted output sample offset
		/// </summary>
		/********************************************************************/
		public off_t NToM_FrmOuts(Mpg123_Handle fr, off_t frame)
		{
			off_t sOff = 0;
			off_t ntm = (off_t)NToM_Val(fr, 0);

			if (frame <= 0)
				return 0;

			for (off_t f = 0; f < frame; ++f)
			{
				ntm += fr.Spf * (off_t)fr.NToM_Step;
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
		public off_t NToM_Ins2Outs(Mpg123_Handle fr, off_t ins)
		{
			off_t sOff = 0;
			off_t ntm = (off_t)NToM_Val(fr, 0);

			{
				off_t block = fr.Spf;

				if (ins <= 0)
					return 0;

				do
				{
					off_t nowBlock = ins > block ? block : ins;
					ntm += nowBlock * (off_t)fr.NToM_Step;
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
		public off_t NToM_FrameOff(Mpg123_Handle fr, off_t sOff)
		{
			off_t iOff = 0;	// Frames or samples
			off_t ntm = (off_t)NToM_Val(fr, 0);

			if (sOff <= 0)
				return 0;

			for (iOff = 0; true; ++iOff)
			{
				ntm += fr.Spf * (off_t)fr.NToM_Step;

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
		private c_ulong NToM_Val(Mpg123_Handle fr, off_t frame)
		{
			off_t ntm = Constant.NToM_Mul >> 1;	// For frame 0

			for (off_t f = 0; f < frame; ++f)	// For frame > 0
			{
				ntm += fr.Spf * (off_t)fr.NToM_Step;
				ntm -= (ntm / Constant.NToM_Mul) * Constant.NToM_Mul;
			}

			return (c_ulong)ntm;
		}
		#endregion
	}
}
