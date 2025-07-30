/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.Med.Containers
{
	/// <summary>
	/// Sample information
	/// </summary>
	internal class Sample
	{
		public string Name { get; set; }
		public SampleType Type { get; set; }
		public sbyte[] SampleData { get; set; }
		public uint LoopStart { get; set; }
		public uint LoopLength { get; set; }
		public byte Volume { get; set; }
	}
}
