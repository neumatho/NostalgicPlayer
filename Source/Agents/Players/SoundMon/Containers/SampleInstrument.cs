/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.SoundMon.Containers
{
	/// <summary>
	/// Holds information about sample instruments
	/// </summary>
	internal class SampleInstrument : Instrument
	{
		public string Name { get; set; }				// Sample name
		public ushort Length { get; set; }				// Length of sample
		public ushort LoopStart { get; set; }			// Offset to loop start
		public ushort LoopLength { get; set; }			// Loop length
		public sbyte[] Adr { get; set; }				// Sample data
	}
}
