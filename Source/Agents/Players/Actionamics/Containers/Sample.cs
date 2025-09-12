/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.Actionamics.Containers
{
	/// <summary>
	/// Holds information about a single sample
	/// </summary>
	internal class Sample
	{
		public string Name { get; set; }
		public ushort Length { get; set; }
		public ushort LoopStart { get; set; }
		public ushort LoopLength { get; set; }
		public sbyte[] SampleData { get; set; }

		public byte ArpeggioListNumber { get; set; }
		public ushort EffectStartPosition { get; set; }
		public ushort EffectLength { get; set; }
		public ushort EffectSpeed { get; set; }
		public ushort EffectMode { get; set; }

		public ushort CounterInitValue { get; set; }
	}
}
