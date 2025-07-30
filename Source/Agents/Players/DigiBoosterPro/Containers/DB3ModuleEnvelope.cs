/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.DigiBoosterPro.Containers
{
	/// <summary>
	/// Envelope (used for both panning and volume envelopes)
	/// </summary>
	internal class DB3ModuleEnvelope
	{
		public uint16_t InstrumentNumber { get; set; }	// Number of instrument (from 0)
		public uint16_t NumberOfSections { get; set; }
		public uint16_t LoopFirst { get; set; }			// Point number
		public uint16_t LoopLast { get; set; }			// Point number, loop disabled if 0xffff
		public uint16_t SustainA { get; set; }			// Point number, disabled if 0xffff
		public uint16_t SustainB { get; set; }			// Point number, disabled if 0xffff
		public DB3ModuleEnvelopePoint[] Points { get; } = new DB3ModuleEnvelopePoint[Constants.Envelope_Max_Points];
	}
}
