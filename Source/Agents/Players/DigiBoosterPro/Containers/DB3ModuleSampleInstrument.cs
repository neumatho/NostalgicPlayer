/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.DigiBoosterPro.Containers
{
	/// <summary>
	/// Sample based instrument
	/// </summary>
	internal class DB3ModuleSampleInstrument : DB3ModuleInstrument
	{
		public uint32_t C3Frequency;
		public uint16_t SampleNumber;
		public int32_t LoopStart;
		public int32_t LoopLength;
		public InstrumentFlag Flags;
	}
}
