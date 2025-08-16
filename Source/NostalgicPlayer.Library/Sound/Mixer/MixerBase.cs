/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.ObjectModel;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Containers.Flags;
using Polycode.NostalgicPlayer.Kit.Containers.Types;
using Polycode.NostalgicPlayer.Library.Sound.Mixer.Containers;

namespace Polycode.NostalgicPlayer.Library.Sound.Mixer
{
	/// <summary>
	/// Base class for different mixers
	/// </summary>
	internal abstract class MixerBase
	{
		protected int mixerFrequency;			// The mixer frequency
		protected int virtualChannelCount;		// The number of channels the module use
		protected int mixerChannelCount;		// The number of channels to mix in
		protected int stereoSeparation;			// This is the stereo separation (0-128)

		protected VoiceInfo[] voiceInfo;

		protected const int MasterVolume = 256;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		protected MixerBase()
		{
			// Initialize mixer variables
			voiceInfo = null;
			stereoSeparation = 128;
		}



		/********************************************************************/
		/// <summary>
		/// Will initialize global mixer stuff as well as local mixer stuff
		/// </summary>
		/********************************************************************/
		public void Initialize(int virtualChannels, int mixerChannels)
		{
			// Start to remember the arguments
			virtualChannelCount = virtualChannels;
			mixerChannelCount = mixerChannels;

			// Allocate and initialize the VoiceInfo structures
			voiceInfo = new VoiceInfo[virtualChannelCount];

			// Clear the voices
			ClearVoices();

			InitMixer();
		}



		/********************************************************************/
		/// <summary>
		/// Will cleanup all the mixer stuff
		/// </summary>
		/********************************************************************/
		public void Cleanup()
		{
			// Start to call the mixer cleanup routine
			CleanupMixer();

			// Deallocate the VoiceInfo buffer
			voiceInfo = null;
		}



		/********************************************************************/
		/// <summary>
		/// Will stop any playing samples in the voices
		/// </summary>
		/********************************************************************/
		public void ClearVoices()
		{
			for (int i = 0; i < virtualChannelCount; i++)
			{
				voiceInfo[i] = new VoiceInfo
				{
					Enabled = true,
					Kick = false,
					Active = false,
					Flags = VoiceFlag.None,
					SampleInfo = new VoiceSampleInfo(),
					Frequency = 10000,
					Volume = 0,
					Panning = (int)((((i & 3) == 0) || ((i & 3) == 3)) ? ChannelPanningType.Left : ChannelPanningType.Right),
					RampVolume = 0,
					PanningVolume = new int[mixerChannelCount],
					OldPanningVolume = new int[mixerChannelCount],
					Current = 0,
					Increment = 0
				};
			}
		}



		/********************************************************************/
		/// <summary>
		/// Set the output format
		/// </summary>
		/********************************************************************/
		public void SetOutputFormat(OutputInfo outputInformation)
		{
			mixerFrequency = outputInformation.Frequency;
		}



		/********************************************************************/
		/// <summary>
		/// Sets a new stereo separation value in percent
		/// </summary>
		/********************************************************************/
		public void SetStereoSeparation(int separation)
		{
			if (separation > 100)
				separation = 100;

			stereoSeparation = (separation * 128) / 100;
		}



		/********************************************************************/
		/// <summary>
		/// Return an array to the channels used for mixing
		/// </summary>
		/********************************************************************/
		public VoiceInfo[] GetMixerChannels()
		{
			return voiceInfo;
		}



		/********************************************************************/
		/// <summary>
		/// Check to see if the given channel is active or not
		/// </summary>
		/********************************************************************/
		public bool IsActive(int channel)
		{
			return voiceInfo[channel].Active;
		}



		/********************************************************************/
		/// <summary>
		/// Enable or disable a channel
		/// </summary>
		/********************************************************************/
		public void EnableChannel(int channel, bool enable)
		{
			voiceInfo[channel].Enabled = enable;
		}



		/********************************************************************/
		/// <summary>
		/// Will initialize mixer stuff
		/// </summary>
		/********************************************************************/
		protected abstract void InitMixer();



		/********************************************************************/
		/// <summary>
		/// Will cleanup mixer stuff
		/// </summary>
		/********************************************************************/
		protected abstract void CleanupMixer();



		/********************************************************************/
		/// <summary>
		/// Returns the click constant value
		/// </summary>
		/********************************************************************/
		public abstract int GetClickConstant();



		/********************************************************************/
		/// <summary>
		/// This is the main mixer method
		/// </summary>
		/********************************************************************/
		public abstract void Mixing(MixerInfo currentMixerInfo, int[][][] channelMap, int offsetInFrames, int todoInFrames, ReadOnlyDictionary<SpeakerFlag, int> playerSpeakerToChannelMap);



		/********************************************************************/
		/// <summary>
		/// Convert the mixing buffer to 32 bit
		/// </summary>
		/********************************************************************/
		public abstract void ConvertMixedData(int[] buffer, int todoInFrames);
	}
}
