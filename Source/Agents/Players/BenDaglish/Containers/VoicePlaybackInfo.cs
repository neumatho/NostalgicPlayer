/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.BenDaglish.Containers
{
	/// <summary>
	/// Holds playback information for a single voice
	/// </summary>
	internal class VoicePlaybackInfo : IDeepCloneable<VoicePlaybackInfo>
	{
		public delegate void HandleSample(VoicePlaybackInfo playbackInfo, IChannel channel, Sample sample);

		public Sample PlayingSample { get; set; }
		public byte SamplePlayTicksCounter { get; set; }

		public ushort NotePeriod { get; set; }
		public short FinalVolume { get; set; }

		public ushort FinalVolumeSlideSpeed { get; set; }
		public ushort FinalVolumeSlideSpeedCounter { get; set; }
		public short FinalVolumeSlideAddValue { get; set; }

		public ushort LoopDelayCounter { get; set; }

		public short PortamentoAddValue { get; set; }

		public short SamplePortamentoDuration { get; set; }
		public short SamplePortamentoAddValue { get; set; }

		public ushort SampleVibratoDepth { get; set; }
		public short SampleVibratoAddValue { get; set; }

		public short SamplePeriodAddValue { get; set; }

		public HandleSample HandleSampleCallback { get; set; }

		// These are used to hold the Amiga hardware registers.
		// It is needed, because the original player sets the hardware
		// registers but will first enable the DMA on the next frame/tick
		public bool DmaEnabled { get; set; }
		public short SampleNumber { get; set; }
		public sbyte[] SampleData { get; set; }
		public ushort SampleLength { get; set; }

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public VoicePlaybackInfo MakeDeepClone()
		{
			return (VoicePlaybackInfo)MemberwiseClone();
		}
	}
}
