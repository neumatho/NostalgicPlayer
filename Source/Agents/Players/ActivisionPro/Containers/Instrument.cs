/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.ActivisionPro.Containers
{
	/// <summary>
	/// Holds information about a single instrument
	/// </summary>
	internal class Instrument
	{
		public byte SampleNumber { get; set; }
		public byte EnvelopeNumber { get; set; }
		public byte Volume { get; set; }
		public byte EnabledEffectFlags { get; set; }
		public byte PortamentoAdd { get; set; }
		public short FineTune { get; set; }
		public byte StopResetEffectDelay { get; set; }
		public byte SampleNumber2 { get; set; }
		public ushort SampleStartOffset { get; set; }
		public sbyte[] ArpeggioTable { get; } = new sbyte[4];
		public byte FixedOrTransposedNote { get; set; }
		public sbyte Transpose { get; set; }
		public byte VibratoNumber { get; set; }
		public byte VibratoDelay { get; set; }
	}
}
