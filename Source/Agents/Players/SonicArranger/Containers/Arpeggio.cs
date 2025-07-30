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
		public byte Length { get; set; }
		public byte Repeat { get; set; }
		public sbyte[] Values { get; } = new sbyte[14];
	}
}
