/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.DigiBoosterPro.Containers
{
	/// <summary>
	/// Single pattern entry
	/// </summary>
	internal class DB3ModuleEntry
	{
		public uint8_t Note { get; set; }				// 0 to 11
		public uint8_t Octave { get; set; }				// 0 to 7
		public uint8_t Instrument { get; set; }
		public Effect Command1 { get; set; }
		public uint8_t Parameter1 { get; set; }
		public Effect Command2 { get; set; }
		public uint8_t Parameter2 { get; set; }
	}
}
