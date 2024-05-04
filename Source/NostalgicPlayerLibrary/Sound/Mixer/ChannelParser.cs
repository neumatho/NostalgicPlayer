/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Containers.Flags;
using Polycode.NostalgicPlayer.Kit.Containers.Types;
using Polycode.NostalgicPlayer.PlayerLibrary.Sound.Mixer.Containers;

namespace Polycode.NostalgicPlayer.PlayerLibrary.Sound.Mixer
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
		public ChannelChanged ParseInfo(VoiceInfo voiceInfo, int clickBuffer, bool channelEnabled, bool bufferMode)
		{
			// Initialize ChannelChanged properties
			bool ccMuted = false;
			bool ccNoteKicked = false;
			bool ccLooping = false;
			bool ccSamplePositionRelative = false;
			int? ccSamplePosition = null;
			ushort? ccVolume = null;
			uint? ccFrequency = null;

			VoiceFlag voiceInfoFlags = voiceInfo.Flags;
			SampleFlag sampleInfoFlags = voiceInfo.SampleInfo.Flags;

			// Change the volume?
			if ((flags & ChannelFlag.Volume) != 0)
			{
				// Protect against clicks if volume variation is too high
				if (Math.Abs(voiceInfo.Volume - volume) > 32)
					voiceInfo.RampVolume = clickBuffer;

				voiceInfo.Volume = volume;
				ccVolume = volume;
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
			{
				voiceInfo.Frequency = frequency;
				ccFrequency = frequency;
			}

			// Mute the channel?
			if ((flags & ChannelFlag.MuteIt) != 0)
			{
				voiceInfo.Active = false;
				voiceInfo.Kick = false;
				voiceInfoFlags = VoiceFlag.None;
				sampleInfoFlags = SampleFlag.None;

				ccMuted = true;
			}
			else
			{
				if (sampleInfo.Sample.Left != null)
				{
					// Trigger the sample to play from the start?
					if ((flags & ChannelFlag.TrigIt) != 0)
					{
						Sample sample = sampleInfo.Sample;
						VoiceSampleInfo voiceSampleInfo = voiceInfo.SampleInfo;
						VoiceSample voiceSample = voiceSampleInfo.Sample;

						voiceInfoFlags = VoiceFlag.None;
						sampleInfoFlags = SampleFlag.None;

						voiceSample.Left = sample.Left;
						voiceSample.Right = sample.Right;
						voiceSample.Start = sample.Start;
						voiceSample.Size = sample.Length;

						if ((sampleInfo.Flags & ChannelSampleFlag.Backwards) != 0)
						{
							voiceSample.Start = sample.Length - sample.Start - 1;
							sampleInfoFlags |= SampleFlag.Reverse;
						}

						voiceInfo.ReleaseEnd = 0;
						voiceInfo.NewSampleInfo = null;
						voiceInfo.Kick = true;

						if ((sampleInfo.Flags & ChannelSampleFlag._16Bit) != 0)
							sampleInfoFlags |= SampleFlag._16Bits;

						ccNoteKicked = true;

						// Does sample loops?
						if (sampleInfo.Loop != null)
						{
							Sample loopSample = sampleInfo.Loop;

							if (loopSample.Length > 2)
							{
								VoiceSample loopVoiceSample = voiceSampleInfo.Loop = new VoiceSample();

								loopVoiceSample.Left = loopSample.Left;
								loopVoiceSample.Right = loopSample.Right;
								loopVoiceSample.Start = loopSample.Start;
								loopVoiceSample.Size = loopSample.Length;

								if ((sampleInfo.Flags & ChannelSampleFlag.Backwards) != 0)
									loopVoiceSample.Start = loopSample.Length - loopSample.Start - 1;

								if ((sampleInfo.Flags & ChannelSampleFlag.PingPong) != 0)
									sampleInfoFlags |= SampleFlag.Bidi;

								ccLooping = true;
							}

							sampleInfo.Loop = null;
						}
						else
							voiceSampleInfo.Loop = null;
					}

					if ((flags & ChannelFlag.ChangePosition) != 0)
					{
						voiceInfo.NewPosition = samplePosition;
						voiceInfo.RelativePosition = (flags & ChannelFlag.Relative) != 0;

						voiceInfoFlags |= VoiceFlag.ChangePosition;

						ccSamplePositionRelative = voiceInfo.RelativePosition;
						ccSamplePosition = samplePosition;
					}

					if (!bufferMode)
					{
						// Use new sample after current sample has played or looping?
						if (newSampleInfo != null)
						{
							Sample newSample = newSampleInfo.Sample;

							if (newSample.Length > 2)
							{
								VoiceSampleInfo newVoiceSampleInfo = voiceInfo.NewSampleInfo = new VoiceSampleInfo();
								VoiceSample newVoiceSample = newVoiceSampleInfo.Sample;

								newVoiceSample.Left = newSample.Left;
								newVoiceSample.Right = newSample.Right;
								newVoiceSample.Start = newSample.Start;
								newVoiceSample.Size = newSample.Length - newSample.Start;	// Because the new sample is used as loop points, the size need to be the length of the loop and not the sample

								if ((newSampleInfo.Flags & ChannelSampleFlag.Backwards) != 0)
								{
									newVoiceSample.Start = newSample.Length - newSample.Start - 1;
									sampleInfoFlags |= SampleFlag.Reverse;
								}

								if ((newSampleInfo.Flags & ChannelSampleFlag._16Bit) != 0)
									sampleInfoFlags |= SampleFlag._16Bits;

								// Does sample loops?
								if ((newSampleInfo.Loop != null) && (newSampleInfo.Loop.Length > 2))
								{
									Sample loopSample = newSampleInfo.Loop;
									VoiceSample loopVoiceSample = newVoiceSampleInfo.Loop = new VoiceSample();

									loopVoiceSample.Left = loopSample.Left;
									loopVoiceSample.Right = loopSample.Right;
									loopVoiceSample.Start = loopSample.Start;
									loopVoiceSample.Size = loopSample.Length;

									if ((sampleInfo.Flags & ChannelSampleFlag.Backwards) != 0)
										loopVoiceSample.Start = loopSample.Length - loopSample.Start - 1;

									if ((sampleInfo.Flags & ChannelSampleFlag.PingPong) != 0)
										sampleInfoFlags |= SampleFlag.Bidi;
								}

								ccLooping = true;
							}

							newSampleInfo = null;
						}

						//XX To be backward compatible, an extra check has been added here. Some players
						// still use SetLoop() to play a new sample after current one has played. Until
						// the players has been rewritten to use SetSample(), we need this part
						if (sampleInfo.Loop != null)
						{
							if ((voiceInfo.NewSampleInfo == null) && (sampleInfo.Loop.Length > 2))
							{
								Sample loopSample = sampleInfo.Loop;
								VoiceSampleInfo newVoiceSampleInfo = voiceInfo.NewSampleInfo = new VoiceSampleInfo();
								VoiceSample newVoiceSample = newVoiceSampleInfo.Sample;

								newVoiceSample.Left = loopSample.Left;
								newVoiceSample.Right = loopSample.Right;
								newVoiceSample.Start = loopSample.Start;
								newVoiceSample.Size = loopSample.Length;

								VoiceSample loopVoiceSample = newVoiceSampleInfo.Loop = new VoiceSample();

								loopVoiceSample.Left = loopSample.Left;
								loopVoiceSample.Right = loopSample.Right;
								loopVoiceSample.Start = loopSample.Start;
								loopVoiceSample.Size = loopSample.Length;

								if ((sampleInfo.Flags & ChannelSampleFlag.PingPong) != 0)
									sampleInfoFlags |= SampleFlag.Bidi;

								ccLooping = true;
							}

							sampleInfo.Loop = null;
						}

						// Special release command. Used in Oktalyzer player
						if ((flags & ChannelFlag.Release) != 0)//XX Need to look at this later on
						{
/*							voiceInfo.NewLoopAddresses[0] = loopAddresses[0];
							voiceInfo.NewLoopAddresses[1] = loopAddresses[1];
							voiceInfo.NewRepeatPosition = voiceInfo.RepeatPosition = loopStart;
							voiceInfo.ReleaseEnd = loopStart + releaseLength;
*/						}
					}
				}

				if ((flags & ChannelFlag.VirtualTrig) != 0)
					ccNoteKicked = true;
			}

			// Store the flags back
			voiceInfo.Flags = voiceInfoFlags;
			voiceInfo.SampleInfo.Flags = sampleInfoFlags;

			ChannelFlag retFlags = flags;
			flags = ChannelFlag.None;

			return bufferMode || ((retFlags & ~ChannelFlag.Active) == ChannelFlag.None) ? null : new ChannelChanged(channelEnabled, ccMuted, ccNoteKicked, currentSampleNumber, voiceInfo.SampleInfo.Sample.Size, ccLooping, ccSamplePositionRelative, ccSamplePosition, ccVolume, ccFrequency);
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
