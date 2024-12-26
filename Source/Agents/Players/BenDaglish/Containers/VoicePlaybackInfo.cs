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

		public Sample PlayingSample;
		public byte SamplePlayTicksCounter;

		public ushort NotePeriod;
		public short FinalVolume;

		public ushort FinalVolumeSlideSpeed;
		public ushort FinalVolumeSlideSpeedCounter;
		public short FinalVolumeSlideAddValue;

		public ushort LoopDelayCounter;

		public short PortamentoAddValue;

		public short SamplePortamentoDuration;
		public short SamplePortamentoAddValue;

		public ushort SampleVibratoDepth;
		public short SampleVibratoAddValue;

		public short SamplePeriodAddValue;

		public HandleSample HandleSampleCallback;

		// These are used to hold the Amiga hardware registers.
		// It is needed, because the original player sets the hardware
		// registers but will first enable the DMA on the next frame/tick
		public bool DmaEnabled;
		public short SampleNumber;
		public sbyte[] SampleData;
		public ushort SampleLength;

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
