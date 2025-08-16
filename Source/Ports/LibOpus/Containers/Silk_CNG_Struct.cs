/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.LibOpus.Containers
{
	/// <summary>
	/// Struct for CNG
	/// </summary>
	internal class Silk_CNG_Struct : IDeepCloneable<Silk_CNG_Struct>
	{
		public CPointer<opus_int32> CNG_exc_buf_Q14 = new CPointer<opus_int32>(Constants.Max_Frame_Length);
		public readonly opus_int16[] CNG_smth_NLSF_Q15 = new opus_int16[Constants.Max_Lpc_Order];
		public readonly opus_int32[] CNG_synth_state = new opus_int32[Constants.Max_Lpc_Order];
		public opus_int32 CNG_smth_Gain_Q16;
		public opus_int32 rand_seed;
		public opus_int fs_kHz;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Clear()
		{
			CNG_exc_buf_Q14.Clear();
			Array.Clear(CNG_smth_NLSF_Q15);
			Array.Clear(CNG_synth_state);

			CNG_smth_Gain_Q16 = 0;
			rand_seed = 0;
			fs_kHz = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Clone the current object into a new one
		/// </summary>
		/********************************************************************/
		public Silk_CNG_Struct MakeDeepClone()
		{
			Silk_CNG_Struct clone = new Silk_CNG_Struct
			{
				CNG_exc_buf_Q14 = CNG_exc_buf_Q14.MakeDeepClone(),
				CNG_smth_Gain_Q16 = CNG_smth_Gain_Q16,
				rand_seed = rand_seed,
				fs_kHz = fs_kHz
			};

			Array.Copy(CNG_smth_NLSF_Q15, clone.CNG_smth_NLSF_Q15, CNG_smth_NLSF_Q15.Length);
			Array.Copy(CNG_synth_state, clone.CNG_synth_state, CNG_synth_state.Length);

			return clone;
		}
	}
}
