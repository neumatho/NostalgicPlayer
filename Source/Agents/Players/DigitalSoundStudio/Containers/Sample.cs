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
		public string Name;			// The name of the sample
		public uint StartOffset;	// Offset to start playing at
		public ushort Length;		// Length (oneshot)
		public uint LoopStart;		// Loop start offset
		public ushort LoopLength;	// Loop length
		public byte FineTune;		// Fine tune (-8 - +7, but used as 0-15)
		public byte Volume;			// The volume
		public ushort Frequency;	// Sample frequency
		public sbyte[] Data;		// Sample data
	}
}
