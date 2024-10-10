/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Ports.LibOpus.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibOpus.Internal.Silk
{
	/// <summary>
	/// 
	/// </summary>
	internal static class Decode_Pitch
	{
		/********************************************************************/
		/// <summary>
		/// Pitch analyser function
		/// </summary>
		/********************************************************************/
		public static void Silk_Decode_Pitch(opus_int16 lagIndex, opus_int8 contourIndex, Pointer<opus_int> pitch_lags, opus_int Fs_kHz, opus_int nb_subfr)
		{
			opus_int cbk_size;
			Pointer<opus_int8> Lag_CB_ptr;

			if (Fs_kHz == 8)
			{
				if (nb_subfr == Constants.Pe_Max_Nb_Subfr)
				{
					Lag_CB_ptr = Pitch_Est_Tables.Silk_CB_Lags_Stage2;
					cbk_size = Constants.Pe_Nb_Cbks_Stage2_Ext;
				}
				else
				{
					Lag_CB_ptr = Pitch_Est_Tables.Silk_CB_Lags_Stage2_10_ms;
					cbk_size = Constants.Pe_Nb_Cbks_Stage2_10ms;
				}
			}
			else
			{
				if (nb_subfr == Constants.Pe_Max_Nb_Subfr)
				{
					Lag_CB_ptr = Pitch_Est_Tables.Silk_CB_Lags_Stage3;
					cbk_size = Constants.Pe_Nb_Cbks_Stage3_Max;
				}
				else
				{
					Lag_CB_ptr = Pitch_Est_Tables.Silk_CB_Lags_Stage3_10_ms;
					cbk_size = Constants.Pe_Nb_Cbks_Stage3_10ms;
				}
			}

			opus_int min_lag = Macros.Silk_SMULBB(Constants.Pe_Min_Lag_Ms, Fs_kHz);
			opus_int max_lag = Macros.Silk_SMULBB(Constants.Pe_Max_Lag_Ms, Fs_kHz);
			opus_int lag = min_lag + lagIndex;

			for (opus_int k = 0; k < nb_subfr; k++)
			{
				pitch_lags[k] = lag + Macros.Matrix_Ptr(Lag_CB_ptr, k, contourIndex, cbk_size);
				pitch_lags[k] = SigProc_Fix.Silk_LIMIT(pitch_lags[k], min_lag, max_lag);
			}
		}
	}
}
