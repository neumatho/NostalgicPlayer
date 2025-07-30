/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.SidMon20.Containers
{
	/// <summary>
	/// Holds information about a single sequence
	/// </summary>
	internal class Sequence
	{
		public byte TrackNumber { get; set; }
		public sbyte NoteTranspose { get; set; }
		public sbyte InstrumentTranspose { get; set; }
	}
}
