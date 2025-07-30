/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.Mo3Converter.Containers
{
	/// <summary>
	/// Hold a single envelope
	/// </summary>
	internal class Envelope
	{
		public EnvelopeFlag Flags { get; set; }
		public byte NumNodes { get; set; }
		public byte SustainStart { get; set; }
		public byte SustainEnd { get; set; }
		public byte LoopStart { get; set; }
		public byte LoopEnd { get; set; }
		public short[] Points { get; } = new short[25 * 2];
	}
}
