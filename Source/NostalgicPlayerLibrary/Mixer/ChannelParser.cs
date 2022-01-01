/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
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
		private ChannelFlags privateFlags;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ChannelParser()
		{
			privateFlags = ChannelFlags.None;
		}



		/********************************************************************/
		/// <summary>
		/// Will parse the channel info and store the result in the VoiceInfo
		/// given
		/// </summary>
		/********************************************************************/
		public ChannelFlags ParseInfo(ref VoiceInfo voiceInfo, int clickBuffer, bool bufferMode)
		{
			// Get the channel flags
			ChannelFlags newFlags = flags;
			ChannelFlags retFlags = flags;
			ChannelFlags privFlags = privateFlags;
			SampleFlag infoFlags = SampleFlag.None;

			if ((newFlags & ChannelFlags.TrigIt) != 0)
			{
				privFlags |= ChannelFlags.TrigIt;
				privFlags &= ~ChannelFlags.Loop;
			}

			if (!bufferMode)
			{
				if ((newFlags & ChannelFlags.Loop) != 0)
					privFlags |= ChannelFlags.Loop;

				if ((newFlags & ChannelFlags.TrigLoop) != 0)
				{
					newFlags &= ~ChannelFlags.TrigLoop;
					retFlags &= ~ChannelFlags.TrigLoop;

					if ((newFlags & ChannelFlags.Active) == 0)     // Only trigger if the channel is not already playing
					{
						newFlags |= (ChannelFlags.TrigIt | ChannelFlags.Loop);
						retFlags |= (ChannelFlags.TrigIt | ChannelFlags.Loop);

						// Did we trigger the sound with a normal "play" command?
						if ((flags & ChannelFlags.TrigIt) == 0)
						{
							// No, then trigger it
							privFlags |= ChannelFlags.TrigIt;

							sampleAddress = loopAddress;
							sampleStart = loopStart;
							sampleLength = loopLength;
						}
					}
				}
			}

			// Change the volume?
			if ((newFlags & ChannelFlags.Volume) != 0)
			{
				// Protect against clicks if volume variation is too high
				if (Math.Abs(voiceInfo.Volume - volume) > 32)
					voiceInfo.RampVolume = clickBuffer;

				voiceInfo.Volume = volume;

				newFlags &= ~ChannelFlags.Volume;
			}

			// Change the panning?
			if ((newFlags & ChannelFlags.Panning) != 0)
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

				newFlags &= ~ChannelFlags.Panning;
			}

			// Change the frequency?
			if ((newFlags & ChannelFlags.Frequency) != 0)
			{
				voiceInfo.Frequency = frequency;
				newFlags &= ~ChannelFlags.Frequency;
			}

			// Mute the channel?
			if ((newFlags & ChannelFlags.MuteIt) != 0)
			{
				voiceInfo.Active = false;
				voiceInfo.Kick = false;
			}
			else
			{
				if (sampleAddress != null)
				{
					// Trigger the sample to play from the start?
					if ((newFlags & ChannelFlags.TrigIt) != 0)
					{
						voiceInfo.Address = sampleAddress;
						voiceInfo.Start = sampleStart;
						voiceInfo.Size = sampleLength;
						voiceInfo.RepeatPosition = 0;
						voiceInfo.RepeatEnd = 0;
						voiceInfo.ReleaseEnd = 0;
						voiceInfo.NewRepeatPosition = 0;
						voiceInfo.NewRepeatEnd = 0;
						voiceInfo.Kick = true;
					}

					if (!bufferMode)
					{
						// Does the sample loop?
						if (((newFlags & ChannelFlags.Loop) != 0) && (loopLength > 2))
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

							if ((newFlags & ChannelFlags.PingPong) != 0)
								infoFlags |= SampleFlag.Bidi;
						}

						// Special release command. Used in Oktalyzer player
						if ((newFlags & ChannelFlags.Release) != 0)
						{
							voiceInfo.NewLoopAddress = loopAddress;
							voiceInfo.NewRepeatPosition = voiceInfo.RepeatPosition = loopStart;
							voiceInfo.ReleaseEnd = loopStart + releaseLength;
							newFlags &= ~ChannelFlags.Release;
						}
					}

					if ((newFlags & ChannelFlags._16Bit) != 0)
						infoFlags |= SampleFlag._16Bits;
				}
			}

			// Store the flags back
			if ((newFlags & ~ChannelFlags.Active) != 0)
				voiceInfo.Flags = infoFlags;

			privateFlags = privFlags;
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
