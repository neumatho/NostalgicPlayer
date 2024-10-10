/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibOpus.Containers
{
	/// <summary>
	/// These are the actual Encoder CTL ID numbers.
	/// They should not be used directly by applications.
	/// In general, SETs should be even and GETs should be odd
	/// </summary>
	public enum OpusControlSetRequest
	{
		// Opus
		/// <summary></summary>
		Opus_Reset_State = 4028,

		// Celt
		/// <summary></summary>
		Celt_Set_Channels = 10008,
		/// <summary></summary>
		Celt_Set_Start_Band = 10010,
		/// <summary></summary>
		Celt_Set_End_Band = 10012,
		/// <summary></summary>
		Celt_Set_Signalling = 10016
	}
}
