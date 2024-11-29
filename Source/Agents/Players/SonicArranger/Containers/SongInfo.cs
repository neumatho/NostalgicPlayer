/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.SonicArranger.Containers
{
	/// <summary>
	/// Information about a single sub-song
	/// </summary>
	internal class SongInfo
	{
		public ushort StartSpeed;
		public ushort RowsPerTrack;
		public ushort FirstPosition;
		public ushort LastPosition;
		public ushort RestartPosition;
		public ushort Tempo;
	}
}
