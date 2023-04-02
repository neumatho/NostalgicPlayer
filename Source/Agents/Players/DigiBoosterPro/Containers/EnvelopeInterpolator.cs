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
		public uint16_t Index;					// Index to envelopes table, set in MSynth_Instrument(), -1 if no envelope
		public uint16_t TickCounter;			// Ticks left in section
		public uint16_t Section;				// Current section
		public int16_t XDelta;					// Current section length in ticks
		public int32_t YDelta;					// Current section value delta
		public int32_t YStart;					// Value at section start
		public int16_t PreviousValue;			// Previous returned value (used for sustains)
		public uint16_t SustainA;				// Set by trigger, cleared to 0xffff with keyoff
		public uint16_t SustainB;				// Set by trigger, cleared to 0xffff with keyoff
		public uint16_t LoopEnd;				// Set by trigger, cleared to 0xffff with keyoff

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
