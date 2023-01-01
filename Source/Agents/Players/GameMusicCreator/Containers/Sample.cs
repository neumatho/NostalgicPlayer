/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.GameMusicCreator.Containers
{
	/// <summary>
	/// Sample structure
	/// </summary>
	internal class Sample
	{
		public sbyte[] Data;		// Sample data
		public ushort Length;		// Length
		public ushort LoopStart;	// Loop start offset
		public ushort LoopLength;	// Loop length
		public ushort Volume;		// The volume (0-64)
	}
}
