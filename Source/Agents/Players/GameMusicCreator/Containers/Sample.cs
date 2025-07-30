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
		public sbyte[] Data { get; set; }		// Sample data
		public ushort Length { get; set; }		// Length
		public ushort LoopStart { get; set; }	// Loop start offset
		public ushort LoopLength { get; set; }	// Loop length
		public ushort Volume { get; set; }		// The volume (0-64)
	}
}
