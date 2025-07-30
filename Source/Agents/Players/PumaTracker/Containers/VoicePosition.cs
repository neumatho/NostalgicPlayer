/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.PumaTracker.Containers
{
	/// <summary>
	/// Holds information about a position for a single voice
	/// </summary>
	internal class VoicePosition
	{
		public byte TrackNumber { get; set; }
		public sbyte InstrumentTranspose { get; set; }
		public sbyte NoteTranspose { get; set; }
	}
}
