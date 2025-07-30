/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.DavidWhittaker.Containers
{
	internal class Sample
	{
		public short SampleNumber { get; set; }
		public sbyte[] SampleData { get; set; }
		public uint Length { get; set; }
		public int LoopStart { get; set; }
		public ushort Volume { get; set; }
		public ushort FineTunePeriod { get; set; }
		public sbyte Transpose { get; set; }
	}
}
