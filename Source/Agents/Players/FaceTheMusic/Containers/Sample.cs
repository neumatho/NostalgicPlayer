/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.FaceTheMusic.Containers
{
	/// <summary>
	/// Holds information about a single sample
	/// </summary>
	internal class Sample
	{
		public string Name { get; set; }
		public sbyte[] SampleData { get; set; }
		public ushort OneshotLength { get; set; }	// In words
		public uint LoopStart { get; set; }			// In bytes
		public ushort LoopLength { get; set; }		// In words
		public uint TotalLength { get; set; }		// In bytes
	}
}
