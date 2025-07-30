/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.DigiBoosterPro.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class EnvelopeInterpolator : IDeepCloneable<EnvelopeInterpolator>
	{
		public uint16_t Index { get; set; }				// Index to envelopes table, set in MSynth_Instrument(), -1 if no envelope
		public uint16_t TickCounter { get; set; }		// Ticks left in section
		public uint16_t Section { get; set; }			// Current section
		public int16_t XDelta { get; set; }				// Current section length in ticks
		public int32_t YDelta { get; set; }				// Current section value delta
		public int32_t YStart { get; set; }				// Value at section start
		public int16_t PreviousValue { get; set; }		// Previous returned value (used for sustains)
		public uint16_t SustainA { get; set; }			// Set by trigger, cleared to 0xffff with keyoff
		public uint16_t SustainB { get; set; }			// Set by trigger, cleared to 0xffff with keyoff
		public uint16_t LoopEnd { get; set; }			// Set by trigger, cleared to 0xffff with keyoff

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public EnvelopeInterpolator MakeDeepClone()
		{
			return (EnvelopeInterpolator)MemberwiseClone();
		}
	}
}
