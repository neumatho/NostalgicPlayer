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
		public EnvelopeFlag Flags;
		public byte NumNodes;
		public byte SustainStart;
		public byte SustainEnd;
		public byte LoopStart;
		public byte LoopEnd;
		public short[] Points = new short[25 * 2];
	}
}
