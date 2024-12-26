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
		public short SampleNumber;
		public sbyte[] SampleData;
		public ushort Length;
		public uint LoopOffset;
		public ushort LoopLength;

		public ushort Volume;
		public short VolumeFadeSpeed;

		public short PortamentoDuration;
		public short PortamentoAddValue;

		public ushort VibratoDepth;
		public ushort VibratoAddValue;

		public short NoteTranspose;
		public ushort FineTunePeriod;
	}
}
