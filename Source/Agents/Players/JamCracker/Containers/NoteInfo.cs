/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.JamCracker.Containers
{
	/// <summary>
	/// Note info structure
	/// </summary>
	internal class NoteInfo
	{
		public byte Period { get; set; }
		public sbyte Instr { get; set; }
		public byte Speed { get; set; }
		public byte Arpeggio { get; set; }
		public byte Vibrato { get; set; }
		public byte Phase { get; set; }
		public byte Volume { get; set; }
		public byte Porta { get; set; }
	}
}
