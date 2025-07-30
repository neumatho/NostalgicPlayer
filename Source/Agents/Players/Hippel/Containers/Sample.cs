/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.Hippel.Containers
{
	/// <summary>
	/// Holds information about a single sample
	/// </summary>
	internal class Sample
	{
		public string Name { get; set; }
		public sbyte[] SampleData { get; set; }
		public uint Length { get; set; }
		public ushort Volume { get; set; }
		public uint LoopStart { get; set; }
		public uint LoopLength { get; set; }
	}
}
