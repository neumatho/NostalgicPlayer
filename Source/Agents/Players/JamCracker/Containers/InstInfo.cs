/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.JamCracker.Containers
{
	/// <summary>
	/// Instrument info structure
	/// </summary>
	internal class InstInfo
	{
		public byte[] Name { get; } = new byte[32];
		public byte Flags { get; set; }
		public uint Size { get; set; }
		public sbyte[] Address { get; set; }
	}
}
