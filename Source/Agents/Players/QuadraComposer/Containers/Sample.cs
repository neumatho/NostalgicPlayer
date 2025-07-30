/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.QuadraComposer.Containers
{
	/// <summary>
	/// Sample structure
	/// </summary>
	internal class Sample
	{
		public string Name { get; set; }			// The name of the sample
		public uint Length { get; set; }			// Length
		public uint LoopStart { get; set; }			// Loop start offset
		public uint LoopLength { get; set; }		// Loop length
		public byte Volume { get; set; }			// The volume (0-64)
		public SampleControlFlag ControlByte { get; set; }
		public byte FineTune { get; set; }			// Fine tune (0-15)
		public sbyte[] Data { get; set; }			// Sample data
	}
}
