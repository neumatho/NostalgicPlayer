/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.InStereo10.Containers
{
	/// <summary>
	/// Holds information for a single voice position
	/// </summary>
	internal class SinglePositionInfo
	{
		public ushort StartTrackRow { get; set; }
		public sbyte SoundTranspose { get; set; }
		public sbyte NoteTranspose { get; set; }
	}
}
