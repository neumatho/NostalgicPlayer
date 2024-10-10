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
	public enum OpusControlGetRequest
	{
		// Opus
		/// <summary></summary>
		Opus_Get_Final_Range = 4031,
		/// <summary></summary>
		Opus_Get_Last_Packet_Duration = 4039,

		// Celt
		/// <summary></summary>
		Celt_Get_Mode = 10015
	}
}
