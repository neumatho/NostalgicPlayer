/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Kit.Containers.Flags;
using Polycode.NostalgicPlayer.Kit.Containers.Types;
using Polycode.NostalgicPlayer.PlayerLibrary.Mixer.Containers;

namespace Polycode.NostalgicPlayer.PlayerLibrary.Mixer
{
	/// <summary>
	/// Helper class for the mixer to parse the channel information given
	/// by the player
	/// </summary>
	internal class ChannelParser : Channel
	{
		/********************************************************************/
		/// <summary>
		/// Will parse the channel info and store the result in the VoiceInfo
		/// given
		/// </summary>
		/********************************************************************/
		public ChannelFlag ParseInfo(ref VoiceInfo voiceInfo, int clickBuffer, bool bufferMode)
		{
			SampleFlag infoFlags = voiceInfo.Flags;

			// Change the volume?
			if ((flags & ChannelFlag.Volume) != 0)
			{
				// Protect against clicks if volume variation is too high
				if (Math.Abs(voiceInfo.Volume - volume) > 32)
					voiceInfo.RampVolume = clickBuffer;

				voiceInfo.Volume = volume;
			}

			// Change the panning?
			if ((flags & ChannelFlag.Panning) != 0)
			{
				if (panning == (int)ChannelPanningType.Surround)
					voiceInfo.Panning = (int)ChannelPanningType.Surround;
				else
				{
					// Protect against clicks if panning variation is too high
					if (Math.Abs(voiceInfo.Panning - panning) > 48)
						voiceInfo.RampVolume = clickBuffer;

					voiceInfo.Panning = (int)panning;
				}
			}

			// Change the frequency?
			if ((flags & ChannelFlag.Frequency) != 0)
				voiceInfo.Frequency = frequency;

			// Mute the channel?
			if ((flags & ChannelFlag.MuteIt) != 0)
			{
				voiceInfo.Active = false;
				voiceInfo.Kick = false;
				infoFlags = SampleFlag.None;
			}
			else
			{
				if (sampleAddresses[0] != null)
				{
					// Trigger the sample to play from the start?
					if ((flags & ChannelFlag.TrigIt) != 0)
					{
						infoFlags = SampleFlag.None;

						bool backwards = false;
						if ((flags & ChannelFlag.Backwards) != 0)
						{
							backwards = true;
							infoFlags |= SampleFlag.Reverse;
						}

						voiceInfo.Addresses[0] = sampleAddresses[0];
						voiceInfo.Addresses[1] = sampleAddresses[1];
						voiceInfo.Start = backwards ? sampleLength - sampleStart - 1 : sampleStart;
						voiceInfo.Size = sampleLength;
						voiceInfo.RepeatPosition = 0;
						voiceInfo.RepeatEnd = 0;
						voiceInfo.ReleaseEnd = 0;
						voiceInfo.NewRepeatPosition = 0;
						voiceInfo.NewRepeatEnd = 0;
						voiceInfo.Kick = true;
					}

					if ((flags & ChannelFlag.ChangePosition) != 0)
					{
						voiceInfo.NewPosition = samplePosition;
						voiceInfo.RelativePosition = (flags & ChannelFlag.Relative) != 0;

						infoFlags |= SampleFlag.ChangePosition;
					}

					if (!bufferMode)
					{
						// Does the sample loop?
						if (((flags & ChannelFlag.Loop) != 0) && (loopLength > 2))
						{
							voiceInfo.NewLoopAddresses[0] = loopAddresses[0];
							voiceInfo.NewLoopAddresses[1] = loopAddresses[1];
							voiceInfo.NewRepeatPosition = loopStart;
							voiceInfo.NewRepeatEnd = loopStart + loopLength;

							if (voiceInfo.Kick)
							{
								voiceInfo.RepeatPosition = voiceInfo.NewRepeatPosition;
								voiceInfo.RepeatEnd = voiceInfo.NewRepeatEnd;
							}

							infoFlags |= SampleFlag.Loop;

							if ((flags & ChannelFlag.PingPong) != 0)
								infoFlags |= SampleFlag.Bidi;
						}

						// Special release command. Used in Oktalyzer player
						if ((flags & ChannelFlag.Release) != 0)
						{
							voiceInfo.NewLoopAddresses[0] = loopAddresses[0];
							voiceInfo.NewLoopAddresses[1] = loopAddresses[1];
							voiceInfo.NewRepeatPosition = voiceInfo.RepeatPosition = loopStart;
							voiceInfo.ReleaseEnd = loopStart + releaseLength;
						}
					}

					if ((flags & ChannelFlag._16Bit) != 0)
						infoFlags |= SampleFlag._16Bits;
				}
			}

			// Store the flags back
			voiceInfo.Flags = infoFlags;

			ChannelFlag retFlags = flags;
			flags = ChannelFlag.None;

			return retFlags & ~ChannelFlag.Active;
		}



		/********************************************************************/
		/// <summary>
		/// Will set the channel to active or inactive
		/// </summary>
		/********************************************************************/
		public void Active(bool active)
		{
			if (active)
				flags |= ChannelFlag.Active;
			else
				flags &= ~ChannelFlag.Active;
		}
	}
}
