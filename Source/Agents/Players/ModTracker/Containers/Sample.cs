/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.ModTracker.Containers
{
	/// <summary>
	/// Sample structure
	/// </summary>
	internal class Sample
	{
		public string SampleName;	// Name of sample
		public sbyte[] Data;		// Sample data
		public ushort Length;		// Length in words
		public ushort LoopStart;	// Loop start offset in words
		public ushort LoopLength;	// Loop length in words
		public byte FineTune;		// Fine tune (-8 - +7, but used as 0-15)
		public sbyte FineTuneHmn;	// Fine tune used instead of above for His Master's Noise modules (using a different algorithm to calculate the period)
		public byte Volume;			// The volume (0-64)
	}
}
