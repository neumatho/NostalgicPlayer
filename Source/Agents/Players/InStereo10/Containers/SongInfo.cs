/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.InStereo10.Containers
{
	/// <summary>
	/// Information about a single sub-song
	/// </summary>
	internal class SongInfo
	{
		public byte StartSpeed;
		public byte RowsPerTrack;
		public ushort FirstPosition;
		public ushort LastPosition;
		public ushort RestartPosition;
	}
}
