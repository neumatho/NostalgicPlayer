/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
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
		private Flags privateFlags;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ChannelParser()
		{
			privateFlags = Flags.None;
		}



		/********************************************************************/
		/// <summary>
		/// Will parse the channel info and store the result in the VoiceInfo
		/// given
		/// </summary>
		/********************************************************************/
		public Flags ParseInfo(ref VoiceInfo voiceInfo, int clickBuffer)
		{
			// Get the channel flags
			Flags newFlags = flags;
			Flags retFlags = flags;
			Flags privFlags = privateFlags;
			SampleFlags infoFlags = SampleFlags.None;

			if ((newFlags & Flags.TrigIt) != 0)
			{
				privFlags |= Flags.TrigIt;
				privFlags &= ~Flags.Loop;
			}

			if ((newFlags & Flags.Loop) != 0)
				privFlags |= Flags.Loop;

			if ((newFlags & Flags.TrigLoop) != 0)
			{
				newFlags &= ~Flags.TrigLoop;
				retFlags &= ~Flags.TrigLoop;

				if ((newFlags & Flags.Active) == 0)     // Only trigger if the channel is not already playing
				{
					newFlags |= (Flags.TrigIt | Flags.Loop);
					retFlags |= (Flags.TrigIt | Flags.Loop);

					// Did we trigger the sound with a normal "play" command?
					if ((flags & Flags.TrigIt) == 0)
					{
						// No, then trigger it
						privFlags |= Flags.TrigIt;

						sampAddress = loopAddress;
						sampStart = loopStart;
						sampLength = loopLength;
					}
				}
			}

			// Speaker volume set?
			if ((newFlags & Flags.SpeakerVolume) != 0)
			{
				// Protect against clicks if volume variation is too high
				if (Math.Abs(voiceInfo.LeftVolume - leftVolume) > 32)
					voiceInfo.RampVolume = clickBuffer;

				voiceInfo.LeftVolume = leftVolume;
				voiceInfo.RightVolume = rightVolume;

				newFlags &= ~(Flags.SpeakerVolume | Flags.Volume | Flags.Panning);
				retFlags &= ~(Flags.SpeakerVolume | Flags.Volume | Flags.Panning);
				infoFlags |= SampleFlags.Speaker;
			}

			// Change the volume?
			if ((newFlags & Flags.Volume) != 0)
			{
				// Protect against clicks if volume variation is too high
				if (Math.Abs(voiceInfo.LeftVolume - volume) > 32)
					voiceInfo.RampVolume = clickBuffer;

				voiceInfo.LeftVolume = volume;
				voiceInfo.RightVolume = volume;

				newFlags &= ~Flags.Volume;
			}

			// Change the panning?
			if ((newFlags & Flags.Panning) != 0)
			{
				if (panning == (int)Panning.Surround)
					voiceInfo.Panning = (int)Panning.Surround;
				else
				{
					// Protect against clicks if panning variation is too high
					if (Math.Abs(voiceInfo.Panning - panning) > 48)
						voiceInfo.RampVolume = clickBuffer;

					voiceInfo.Panning = (int)panning;
				}

				newFlags &= ~Flags.Panning;
			}

			// Change the frequency?
			if ((newFlags & Flags.Frequency) != 0)
			{
				voiceInfo.Frequency = frequency;
				newFlags &= ~Flags.Frequency;
			}

			// Mute the channel?
			if ((newFlags & Flags.MuteIt) != 0)
			{
				voiceInfo.Active = false;
				voiceInfo.Kick = false;
			}
			else
			{
				if (sampAddress != null)
				{
					// Trigger the sample to play from the start?
					if ((newFlags & Flags.TrigIt) != 0)
					{
						voiceInfo.Address = sampAddress;
						voiceInfo.Start = sampStart;
						voiceInfo.Size = sampLength;
						voiceInfo.RepeatPosition = 0;
						voiceInfo.RepeatEnd = 0;
						voiceInfo.ReleaseLength = 0;
						voiceInfo.Kick = true;
					}

					// Does the sample loop?
					if (((newFlags & Flags.Loop) != 0) && (loopLength > 4))
					{
						voiceInfo.LoopAddress = loopAddress;
						voiceInfo.RepeatPosition = loopStart;
						voiceInfo.RepeatEnd = loopStart + loopLength;
						infoFlags |= SampleFlags.Loop;

						if ((newFlags & Flags.PingPong) != 0)
							infoFlags |= SampleFlags.Bidi;
					}

					// Special release command. Used in Octalyzer player
					if ((newFlags & Flags.Release) != 0)
					{
						voiceInfo.LoopAddress = loopAddress;
						voiceInfo.ReleaseLength = releaseLength;
						newFlags &= ~Flags.Release;
					}

					if ((newFlags & Flags._16Bit) != 0)
						infoFlags |= SampleFlags._16Bits;
				}
			}

			// Store the flags back
			if ((newFlags & ~Flags.Active) != 0)
				voiceInfo.Flags = infoFlags;

			privateFlags = privFlags;
			flags = Flags.None;

			return retFlags & ~Flags.Active;
		}
	}
}
