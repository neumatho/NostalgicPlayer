/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Ports.LibOpus.Containers;
using Polycode.NostalgicPlayer.Ports.LibOpus.Internal.Celt;

namespace Polycode.NostalgicPlayer.Ports.LibOpus.Internal.Silk
{
	/// <summary>
	/// 
	/// </summary>
	internal class Init_Decoder
	{
		/********************************************************************/
		/// <summary>
		/// Reset decoder state
		/// </summary>
		/********************************************************************/
		public static SilkError Silk_Reset_Decoder(Silk_Decoder_State psDec)
		{
			// Clear the entire encoder state, except anything copied
			psDec.Reset();

			// Used to deactivate LSF interpolation
			psDec.first_frame_after_reset = true;
			psDec.prev_gain_Q16 = 65536;
			psDec.arch = Cpu_Support.Opus_Select_Arch();

			// Reset CNG state
			Cng.Silk_CNG_Reset(psDec);

			// Reset PLC state
			Plc.Silk_PLC_Reset(psDec);

			return SilkError.No_Error;
		}



		/********************************************************************/
		/// <summary>
		/// Init decoder state
		/// </summary>
		/********************************************************************/
		public static SilkError Silk_Init_Decoder(Silk_Decoder_State psDec)
		{
			// Clear the entire encoder state, except anything copied
			psDec.Clear();

			Silk_Reset_Decoder(psDec);

			return SilkError.No_Error;
		}
	}
}
