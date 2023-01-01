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
		public uint8_t Note;					// 0 to 11
		public uint8_t Octave;					// 0 to 7
		public uint8_t Instrument;
		public Effect Command1;
		public uint8_t Parameter1;
		public Effect Command2;
		public uint8_t Parameter2;
	}
}
