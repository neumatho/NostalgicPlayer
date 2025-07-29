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
		public byte SampleNumber;
		public byte EnvelopeNumber;
		public byte Volume;
		public byte EnabledEffectFlags;
		public byte PortamentoAdd;
		public short FineTune;
		public byte StopResetEffectDelay;
		public byte SampleNumber2;
		public ushort SampleStartOffset;
		public readonly sbyte[] ArpeggioTable = new sbyte[4];
		public byte FixedOrTransposedNote;
		public sbyte Transpose;
		public byte VibratoNumber;
		public byte VibratoDelay;
	}
}
