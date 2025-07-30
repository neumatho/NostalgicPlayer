/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.BenDaglish.Containers
{
	/// <summary>
	/// Holds information about a single sample
	/// </summary>
	internal class Sample
	{
		public short SampleNumber { get; set; }
		public sbyte[] SampleData { get; set; }
		public ushort Length { get; set; }
		public uint LoopOffset { get; set; }
		public ushort LoopLength { get; set; }

		public ushort Volume { get; set; }
		public short VolumeFadeSpeed { get; set; }

		public short PortamentoDuration { get; set; }
		public short PortamentoAddValue { get; set; }

		public ushort VibratoDepth { get; set; }
		public ushort VibratoAddValue { get; set; }

		public short NoteTranspose { get; set; }
		public ushort FineTunePeriod { get; set; }
	}
}
