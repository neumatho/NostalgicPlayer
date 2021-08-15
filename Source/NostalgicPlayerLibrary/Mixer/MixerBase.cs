/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/

using System;
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.PlayerLibrary.Mixer.Containers;

namespace Polycode.NostalgicPlayer.PlayerLibrary.Mixer
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
				info.Flags = SampleFlag.None;
				info.Address = null;
				info.LoopAddress = null;
				info.Start = 0;
				info.Size = 0;
				info.RepeatPosition = 0;
				info.RepeatEnd = 0;
				info.ReleaseEnd = 0;
				info.Frequency = 10000;
				info.Volume = 0;
				info.Panning = (int)((((i & 3) == 0) || ((i & 3) == 3)) ? ChannelPanning.Left : ChannelPanning.Right);
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
		/// Convert the mix buffer to the output format and store the result
		/// in the supplied buffer
		/// </summary>
		/********************************************************************/
		public void ConvertMixedData(byte[] dest, int offset, int[] source, int todo, bool swapSpeakers)
		{
			if (bytesPerSample == 2)
				MixConvertTo16(MemoryMarshal.Cast<byte, short>(dest), offset / 2, source, todo, swapSpeakers);
			else if (bytesPerSample == 4)
				MixConvertTo32(MemoryMarshal.Cast<byte, int>(dest), offset / 4, source, todo, swapSpeakers);
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

		#region Conversion methods
		private const int MixBitShift16 = 9;
		private const int MixBitShift32 = 7;

		/********************************************************************/
		/// <summary>
		/// Converts the mixed data to a 16 bit sample buffer
		/// </summary>
		/********************************************************************/
		private void MixConvertTo16(Span<short> dest, int offset, int[] source, int count, bool swapSpeakers)
		{
			int x1, x2, x3, x4;

			int remain = count & 3;
			int sourceOffset = 0;

			if (swapSpeakers)
			{
				for (count >>= 2; count != 0; count--)
				{
					x1 = source[sourceOffset++] >> MixBitShift16;
					x2 = source[sourceOffset++] >> MixBitShift16;
					x3 = source[sourceOffset++] >> MixBitShift16;
					x4 = source[sourceOffset++] >> MixBitShift16;

					x1 = (x1 >= 32767) ? 32767 - 1 : (x1 < -32767) ? -32767 : x1;
					x2 = (x2 >= 32767) ? 32767 - 1 : (x2 < -32767) ? -32767 : x2;
					x3 = (x3 >= 32767) ? 32767 - 1 : (x3 < -32767) ? -32767 : x3;
					x4 = (x4 >= 32767) ? 32767 - 1 : (x4 < -32767) ? -32767 : x4;

					dest[offset++] = (short)x2;
					dest[offset++] = (short)x1;
					dest[offset++] = (short)x4;
					dest[offset++] = (short)x3;
				}

				// We know it is always stereo samples when coming here
				while (remain > 0)
				{
					x1 = source[sourceOffset++] >> MixBitShift16;
					x2 = source[sourceOffset++] >> MixBitShift16;

					x1 = (x1 >= 32767) ? 32767 - 1 : (x1 < -32767) ? -32767 : x1;
					x2 = (x2 >= 32767) ? 32767 - 1 : (x2 < -32767) ? -32767 : x2;

					dest[offset++] = (short)x2;
					dest[offset++] = (short)x1;

					remain -= 2;
				}
			}
			else
			{
				for (count >>= 2; count != 0; count--)
				{
					x1 = source[sourceOffset++] >> MixBitShift16;
					x2 = source[sourceOffset++] >> MixBitShift16;
					x3 = source[sourceOffset++] >> MixBitShift16;
					x4 = source[sourceOffset++] >> MixBitShift16;

					x1 = (x1 >= 32767) ? 32767 - 1 : (x1 < -32767) ? -32767 : x1;
					x2 = (x2 >= 32767) ? 32767 - 1 : (x2 < -32767) ? -32767 : x2;
					x3 = (x3 >= 32767) ? 32767 - 1 : (x3 < -32767) ? -32767 : x3;
					x4 = (x4 >= 32767) ? 32767 - 1 : (x4 < -32767) ? -32767 : x4;

					dest[offset++] = (short)x1;
					dest[offset++] = (short)x2;
					dest[offset++] = (short)x3;
					dest[offset++] = (short)x4;
				}

				while (remain-- != 0)
				{
					x1 = source[sourceOffset++] >> MixBitShift16;
					x1 = (x1 >= 32767) ? 32767 - 1 : (x1 < -32767) ? -32767 : x1;
					dest[offset++] = (short)x1;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Converts the mixed data to a 32 bit sample buffer
		/// </summary>
		/********************************************************************/
		private void MixConvertTo32(Span<int> dest, int offset, int[] source, int count, bool swapSpeakers)
		{
			long x1, x2, x3, x4;

			int remain = count & 3;
			int sourceOffset = 0;

			if (swapSpeakers)
			{
				for (count >>= 2; count != 0; count--)
				{
					x1 = (long)source[sourceOffset++] << MixBitShift32;
					x2 = (long)source[sourceOffset++] << MixBitShift32;
					x3 = (long)source[sourceOffset++] << MixBitShift32;
					x4 = (long)source[sourceOffset++] << MixBitShift32;

					x1 = (x1 >= 2147483647) ? 2147483647 - 1 : (x1 < -2147483647) ? -2147483647 : x1;
					x2 = (x2 >= 2147483647) ? 2147483647 - 1 : (x2 < -2147483647) ? -2147483647 : x2;
					x3 = (x3 >= 2147483647) ? 2147483647 - 1 : (x3 < -2147483647) ? -2147483647 : x3;
					x4 = (x4 >= 2147483647) ? 2147483647 - 1 : (x4 < -2147483647) ? -2147483647 : x4;

					dest[offset++] = (int)x2;
					dest[offset++] = (int)x1;
					dest[offset++] = (int)x4;
					dest[offset++] = (int)x3;
				}

				// We know it is always stereo samples when coming here
				while (remain > 0)
				{
					x1 = (long)source[sourceOffset++] << MixBitShift32;
					x2 = (long)source[sourceOffset++] << MixBitShift32;

					x1 = (x1 >= 2147483647) ? 2147483647 - 1 : (x1 < -2147483647) ? -2147483647 : x1;
					x2 = (x2 >= 2147483647) ? 2147483647 - 1 : (x2 < -2147483647) ? -2147483647 : x2;

					dest[offset++] = (int)x2;
					dest[offset++] = (int)x1;

					remain -= 2;
				}
			}
			else
			{
				for (count >>= 2; count != 0; count--)
				{
					x1 = (long)source[sourceOffset++] << MixBitShift32;
					x2 = (long)source[sourceOffset++] << MixBitShift32;
					x3 = (long)source[sourceOffset++] << MixBitShift32;
					x4 = (long)source[sourceOffset++] << MixBitShift32;

					x1 = (x1 >= 2147483647) ? 2147483647 - 1 : (x1 < -2147483647) ? -2147483647 : x1;
					x2 = (x2 >= 2147483647) ? 2147483647 - 1 : (x2 < -2147483647) ? -2147483647 : x2;
					x3 = (x3 >= 2147483647) ? 2147483647 - 1 : (x3 < -2147483647) ? -2147483647 : x3;
					x4 = (x4 >= 2147483647) ? 2147483647 - 1 : (x4 < -2147483647) ? -2147483647 : x4;

					dest[offset++] = (int)x1;
					dest[offset++] = (int)x2;
					dest[offset++] = (int)x3;
					dest[offset++] = (int)x4;
				}

				while (remain-- != 0)
				{
					x1 = (long)source[sourceOffset++] << MixBitShift32;
					x1 = (x1 >= 2147483647) ? 2147483647 - 1 : (x1 < -2147483647) ? -2147483647 : x1;
					dest[offset++] = (int)x1;
				}
			}
		}
		#endregion
	}
}
