/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Mixer;
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
		public ChannelFlags ParseInfo(ref VoiceInfo voiceInfo, int clickBuffer, bool bufferMode)
		{
			SampleFlag infoFlags = voiceInfo.Flags;

			// Change the volume?
			if ((flags & ChannelFlags.Volume) != 0)
			{
				// Protect against clicks if volume variation is too high
				if (Math.Abs(voiceInfo.Volume - volume) > 32)
					voiceInfo.RampVolume = clickBuffer;

				voiceInfo.Volume = volume;
			}

			// Change the panning?
			if ((flags & ChannelFlags.Panning) != 0)
			{
				if (panning == (int)ChannelPanning.Surround)
					voiceInfo.Panning = (int)ChannelPanning.Surround;
				else
				{
					// Protect against clicks if panning variation is too high
					if (Math.Abs(voiceInfo.Panning - panning) > 48)
						voiceInfo.RampVolume = clickBuffer;

					voiceInfo.Panning = (int)panning;
				}
			}

			// Change the frequency?
			if ((flags & ChannelFlags.Frequency) != 0)
				voiceInfo.Frequency = frequency;

			// Mute the channel?
			if ((flags & ChannelFlags.MuteIt) != 0)
			{
				voiceInfo.Active = false;
				voiceInfo.Kick = false;
				infoFlags = SampleFlag.None;
			}
			else
			{
				if (sampleAddress != null)
				{
					// Trigger the sample to play from the start?
					if ((flags & ChannelFlags.TrigIt) != 0)
					{
						infoFlags = SampleFlag.None;

						bool backwards = false;
						if ((flags & ChannelFlags.Backwards) != 0)
						{
							backwards = true;
							infoFlags |= SampleFlag.Reverse;
						}

						voiceInfo.Address = sampleAddress;
						voiceInfo.Start = backwards ? sampleLength - sampleStart - 1 : sampleStart;
						voiceInfo.Size = sampleLength;
						voiceInfo.RepeatPosition = 0;
						voiceInfo.RepeatEnd = 0;
						voiceInfo.ReleaseEnd = 0;
						voiceInfo.NewRepeatPosition = 0;
						voiceInfo.NewRepeatEnd = 0;
						voiceInfo.Kick = true;
					}

					if ((flags & ChannelFlags.ChangePosition) != 0)
					{
						voiceInfo.NewPosition = samplePosition;
						voiceInfo.RelativePosition = (flags & ChannelFlags.Relative) != 0;

						infoFlags |= SampleFlag.ChangePosition;
					}

					if (!bufferMode)
					{
						// Does the sample loop?
						if (((flags & ChannelFlags.Loop) != 0) && (loopLength > 2))
						{
							voiceInfo.NewLoopAddress = loopAddress;
							voiceInfo.NewRepeatPosition = loopStart;
							voiceInfo.NewRepeatEnd = loopStart + loopLength;

							if (voiceInfo.Kick)
							{
								voiceInfo.RepeatPosition = voiceInfo.NewRepeatPosition;
								voiceInfo.RepeatEnd = voiceInfo.NewRepeatEnd;
							}

							infoFlags |= SampleFlag.Loop;

							if ((flags & ChannelFlags.PingPong) != 0)
								infoFlags |= SampleFlag.Bidi;
						}

						// Special release command. Used in Oktalyzer player
						if ((flags & ChannelFlags.Release) != 0)
						{
							voiceInfo.NewLoopAddress = loopAddress;
							voiceInfo.NewRepeatPosition = voiceInfo.RepeatPosition = loopStart;
							voiceInfo.ReleaseEnd = loopStart + releaseLength;
						}
					}

					if ((flags & ChannelFlags._16Bit) != 0)
						infoFlags |= SampleFlag._16Bits;
				}
			}

			// Store the flags back
			voiceInfo.Flags = infoFlags;

			ChannelFlags retFlags = flags;
			flags = ChannelFlags.None;

			return retFlags & ~ChannelFlags.Active;
		}



		/********************************************************************/
		/// <summary>
		/// Will set the channel to active or inactive
		/// </summary>
		/********************************************************************/
		public void Active(bool active)
		{
			if (active)
				flags |= ChannelFlags.Active;
			else
				flags &= ~ChannelFlags.Active;
		}
	}
}
