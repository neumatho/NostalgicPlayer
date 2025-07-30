/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.Oktalyzer.Containers
{
	/// <summary>
	/// Sample structure
	/// </summary>
	internal class Sample
	{
		public string Name { get; set; }
		public uint Length { get; set; }
		public ushort RepeatStart { get; set; }
		public ushort RepeatLength { get; set; }
		public ushort Volume { get; set; }
		public ushort Mode { get; set; }			// 0 = 8, 1 = 4, 2 = B
		public sbyte[] SampleData { get; set; }
	}
}
