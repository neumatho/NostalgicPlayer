/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.InStereo20.Containers
{
	/// <summary>
	/// Information about a single sub-song
	/// </summary>
	internal class SongInfo
	{
		public byte StartSpeed { get; set; }
		public byte RowsPerTrack { get; set; }
		public ushort FirstPosition { get; set; }
		public ushort LastPosition { get; set; }
		public ushort RestartPosition { get; set; }
		public ushort Tempo { get; set; }
	}
}
