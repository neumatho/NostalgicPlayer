/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.DigitalMugician.Containers
{
	/// <summary>
	/// Holds information about a single sub-song
	/// </summary>
	internal class SubSong
	{
		public bool LoopSong;
		public byte LoopPosition;
		public byte SongSpeed;
		public byte NumberOfSequences;
		public string Name;
	}
}
