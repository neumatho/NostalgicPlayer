/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibOpus.Containers
{
	/// <summary>
	/// Decoder API flag
	/// </summary>
	internal enum LostFlag
	{
		Decode_Normal = 0,
		Packet_Lost,
		Decode_LBRR
	}
}
