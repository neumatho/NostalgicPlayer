/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.Actionamics.Containers
{
	/// <summary>
	/// Holds information for a single voice position
	/// </summary>
	internal class SinglePositionInfo
	{
		public byte TrackNumber { get; set; }
		public sbyte NoteTranspose { get; set; }
		public sbyte InstrumentTranspose { get; set; }
	}
}
