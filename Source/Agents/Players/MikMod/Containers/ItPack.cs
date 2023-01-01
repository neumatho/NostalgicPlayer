/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.MikMod.Containers
{
	/// <summary>
	/// IT pack structure
	/// </summary>
	internal class ItPack
	{
		public ushort Bits;						// Current number of bits
		public ushort BufBits;					// Bits in buffer
		public short Last;						// Last output
		public byte Buf;						// Bit buffer
	}
}
