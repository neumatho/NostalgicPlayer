/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.SoundFx.Containers
{
	/// <summary>
	/// Sample information
	/// </summary>
	internal class Sample
	{
		public string Name { get; set; }
		public sbyte[] SampleAddr { get; set; }
		public uint Length { get; set; }
		public ushort Volume { get; set; }
		public uint LoopStart { get; set; }
		public uint LoopLength { get; set; }
	}
}
