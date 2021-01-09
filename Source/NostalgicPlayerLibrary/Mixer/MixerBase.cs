/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using Polycode.NostalgicPlayer.NostalgicPlayerKit.Containers;
using Polycode.NostalgicPlayer.NostalgicPlayerLibrary.Mixer.Containers;

namespace Polycode.NostalgicPlayer.NostalgicPlayerLibrary.Mixer
{
	/// <summary>
	/// Base class for different mixers
	/// </summary>
	internal abstract class MixerBase
	{
		protected int mixerFrequency;		// The mixer frequency
		protected int masterVolume;			// This is the master volume (0-256)
		protected int channelNumber;		// Number of channels this mixer use
		protected int stereoSeparation;		// This is the stereo separation (0-128)

		private int bytesPerSample;			// How many bytes each sample uses in the output buffer

		protected VoiceInfo[] voiceInfo;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		protected MixerBase()
		{
			// Initialize mixer variables
			voiceInfo = null;
			masterVolume = 256;
			stereoSeparation = 128;
		}



		/********************************************************************/
		/// <summary>
		/// Will initialize global mixer stuff as well as local mixer stuff
		/// </summary>
		/********************************************************************/
		public void Initialize(int channels)
		{
			// Start to remember the arguments
			channelNumber = channels;

			// Allocate and initialize the VoiceInfo structures
			voiceInfo = new VoiceInfo[channels];

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
			for (int i = 0; i < channelNumber; i++)
			{
				ref VoiceInfo info = ref voiceInfo[i];

				info.Enabled = true;
				info.Kick = false;
				info.Active = false;
				info.Flags = SampleFlags.None;
				info.Address = null;
				info.LoopAddress = null;
				info.Start = 0;
				info.Size = 0;
				info.RepeatPosition = 0;
				info.RepeatEnd = 0;
				info.ReleaseLength = 0;
				info.Frequency = 10000;
				info.LeftVolume = 0;
				info.RightVolume = 0;
				info.Panning = (int)((((i & 3) == 0) || ((i & 3) == 3)) ? Panning.Left : Panning.Right);
				info.RampVolume = 0;
				info.LeftVolumeSelected = 0;
				info.RightVolumeSelected = 0;
				info.OldLeftVolume = 0;
				info.OldRightVolume = 0;
				info.Current = 0;
				info.Increment = 0;
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
			bytesPerSample = outputInformation.BytesPerSample;
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
		/// Will set the master volume
		/// </summary>
		/********************************************************************/
		public void SetMasterVolume(int volume)
		{
			masterVolume = volume;
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
		/// Convert the mix buffer to the output format and store the result
		/// in the supplied buffer
		/// </summary>
		/********************************************************************/
		public void ConvertMixedData(byte[] dest, int offset, int[] source, int todo)
		{
			if (bytesPerSample == 2)
				ConvertTo16(dest, offset / 2, source, todo);
			else if (bytesPerSample == 4)
				ConvertTo32(dest, offset / 4, source, todo);
		}



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
		public abstract void Mixing(int[] dest, int offset, int todo, MixerMode mode);



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
		/// Converts the mixed data to a 16 bit sample buffer
		/// </summary>
		/********************************************************************/
		protected abstract void ConvertTo16(byte[] dest, int offset, int[] source, int todo);



		/********************************************************************/
		/// <summary>
		/// Converts the mixed data to a 32 bit sample buffer
		/// </summary>
		/********************************************************************/
		protected abstract void ConvertTo32(byte[] dest, int offset, int[] source, int todo);
	}
}
