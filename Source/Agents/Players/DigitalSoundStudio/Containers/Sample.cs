/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.DigitalSoundStudio.Containers
{
	/// <summary>
	/// Sample structure
	/// </summary>
	internal class Sample
	{
		public string Name { get; set; }		// The name of the sample
		public uint StartOffset { get; set; }	// Offset to start playing at
		public ushort Length { get; set; }		// Length (oneshot)
		public uint LoopStart { get; set; }		// Loop start offset
		public ushort LoopLength { get; set; }	// Loop length
		public byte FineTune { get; set; }		// Fine tune (-8 - +7, but used as 0-15)
		public byte Volume { get; set; }		// The volume
		public ushort Frequency { get; set; }	// Sample frequency
		public sbyte[] Data { get; set; }		// Sample data
	}
}
