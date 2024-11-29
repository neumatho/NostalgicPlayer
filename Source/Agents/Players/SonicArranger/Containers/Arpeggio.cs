/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.SonicArranger.Containers
{
	/// <summary>
	/// Single arpeggio information
	/// </summary>
	internal class Arpeggio
	{
		public byte Length;
		public byte Repeat;
		public byte[] Values = new byte[14];
	}
}
