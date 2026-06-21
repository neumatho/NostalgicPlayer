/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.LibTfmxAudioDecoder.Chris.Dns.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class VoiceVars : IDeepCloneable<VoiceVars>
	{
		public udword SampleHeader;
		public uword Period;
		public ubyte PipelineState;

		public
		(
			uword Count,
			uword Speed,
			uword DecaySpeed,
			uword ReleaseSpeed,
			sword Volume,
			uword TargetVolume,
			uword SustainVolume,
			sword Strength,
			sword DecayStrength,
			sword ReleaseStrength,
			EnvPhase Phase,
			bool KeyUp,
			bool SetSustain,
			uword Duration				// Unsigned comparison
		) Envelope;

		public
		(
			udword Offset,
			uword Length
		) PaulaOrig;

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public VoiceVars MakeDeepClone()
		{
			return (VoiceVars)MemberwiseClone();
		}
	}
}
