/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.FutureComposer.Containers
{
	/// <summary>
	/// Volume sequence structure
	/// </summary>
	internal class VolSequence
	{
		public byte Speed { get; set; }
		public byte FrqNumber { get; set; }
		public sbyte VibSpeed { get; set; }
		public sbyte VibDepth { get; set; }
		public byte VibDelay { get; set; }
		public byte[] Values { get; } = new byte[59];
	}
}
