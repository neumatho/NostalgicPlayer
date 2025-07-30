/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.FutureComposer.Containers
{
	/// <summary>
	/// Sample information structure
	/// </summary>
	internal class Sample
	{
		public short SampleNumber { get; set; }
		public sbyte[] Address { get; set; }
		public ushort Length { get; set; }
		public ushort LoopStart { get; set; }
		public ushort LoopLength { get; set; }
		public MultiSample Multi { get; set; }
	}
}
