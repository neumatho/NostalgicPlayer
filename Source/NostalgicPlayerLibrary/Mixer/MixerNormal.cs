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
	/// Normal mixer implementation
	/// </summary>
	internal class MixerNormal : MixerBase
	{
		private long idxSize;			// The current size of the playing sample in fixed point
		private long idxLoopPos;		// The loop start position in fixed point
		private long idxLoopEnd;		// The loop end position in fixed point
		private long idxReleaseEnd;		// The release end position in fixed point

		/********************************************************************/
		/// <summary>
		/// Will initialize mixer stuff
		/// </summary>
		/********************************************************************/
		protected override void InitMixer()
		{
		}



		/********************************************************************/
		/// <summary>
		/// Will cleanup mixer stuff
		/// </summary>
		/********************************************************************/
		protected override void CleanupMixer()
		{
		}



		/********************************************************************/
		/// <summary>
		/// Returns the click constant value
		/// </summary>
		/********************************************************************/
		public override int GetClickConstant()
		{
			return Native.CLICK_BUFFER;
		}



		/********************************************************************/
		/// <summary>
		/// This is the main mixer method
		/// </summary>
		/********************************************************************/
		public override void Mixing(int[] dest, int offset, int todo, MixerMode mode)
		{
			// Loop through all the channels and mix the samples into the buffer
			for (int t = 0; t < channelNumber; t++)
			{
				ref VoiceInfo vnf = ref voiceInfo[t];

				if (vnf.Kick)
				{
					vnf.Current = ((long)vnf.Start) << Native.FRACBITS;
					vnf.Kick = false;
					vnf.Active = true;
				}

				if (vnf.Frequency == 0)
					vnf.Active = false;

				if (vnf.Active)
				{
					vnf.Increment = ((long)vnf.Frequency << Native.FRACBITS) / mixerFrequency;

					if ((vnf.Flags & SampleFlag.Reverse) != 0)
						vnf.Increment = -vnf.Increment;

					int lVol, rVol;

					if (vnf.Enabled)
					{
						lVol = vnf.LeftVolume * masterVolume / 256;
						rVol = vnf.RightVolume * masterVolume / 256;
					}
					else
					{
						lVol = 0;
						rVol = 0;
					}

					vnf.OldLeftVolume = vnf.LeftVolumeSelected;
					vnf.OldRightVolume = vnf.RightVolumeSelected;

					if ((mode & MixerMode.Stereo) != 0)
					{
						if ((vnf.Flags & SampleFlag.Speaker) != 0)
						{
							vnf.LeftVolumeSelected = lVol;
							vnf.RightVolumeSelected = rVol;
						}
						else
						{
							if (vnf.Panning != (int)ChannelPanning.Surround)
							{
								// Stereo, calculate the volume with panning
								int pan = (((vnf.Panning - 128) * stereoSeparation) / 128) + 128;

								vnf.LeftVolumeSelected = (lVol * ((int)ChannelPanning.Right - pan)) >> 8;
								vnf.RightVolumeSelected = (lVol * pan) >> 8;
							}
							else
							{
								// Dolby Surround
								vnf.LeftVolumeSelected = vnf.RightVolumeSelected = lVol / 2;
							}
						}
					}
					else
					{
						// Well, just mono
						vnf.LeftVolumeSelected = lVol;
					}

					idxSize = vnf.Size != 0 ? ((long)vnf.Size << Native.FRACBITS) - 1 : 0;
					idxLoopEnd = vnf.RepeatEnd != 0 ? ((long)vnf.RepeatEnd << Native.FRACBITS) - 1 : 0;
					idxLoopPos = (long)vnf.RepeatPosition << Native.FRACBITS;
					idxReleaseEnd = vnf.ReleaseLength != 0 ? ((long)vnf.ReleaseLength << Native.FRACBITS) - 1 : 0;

					AddChannel(ref vnf, dest, offset, todo, mode);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Mix a channel into the buffer
		/// </summary>
		/********************************************************************/
		private void AddChannel(ref VoiceInfo vnf, int[] buf, int offset, int todo, MixerMode mode)
		{
			sbyte[] s;

			if ((s = vnf.Address) == null)
			{
				vnf.Current = 0;
				vnf.Active = false;
				return;
			}

			// Update the 'current' index so the sample loops, or
			// stops playing if it reached the end of the sample
			while (todo > 0)
			{
				if ((vnf.Flags & SampleFlag.Reverse) != 0)
				{
					// The sampling is playing in reverse
					if (((vnf.Flags & SampleFlag.Loop) != 0) && (vnf.Current < idxLoopPos))
					{
						// The sample is looping, and has reached the loop start index
						if ((vnf.Flags & SampleFlag.Bidi) != 0)
						{
							// Sample is doing bidirectional loops, so 'bounce'
							// the current index against the idxLoopPos
							vnf.Current = idxLoopPos + (idxLoopPos - vnf.Current);
							vnf.Increment = -vnf.Increment;
							vnf.Flags &= ~SampleFlag.Reverse;
						}
						else
						{
							// Normal backwards looping, so set the
							// current position to loopEnd index
							vnf.Current = idxLoopEnd - (idxLoopPos - vnf.Current);
						}
					}
					else
					{
						// The sample is not looping, so check if it reached index 0
						if (vnf.Current < 0)
						{
							// Playing index reached 0, so stop playing this sample
							vnf.Current = 0;
							vnf.Active = false;
							break;
						}
					}
				}
				else
				{
					// The sample is playing forward
					if ((vnf.Flags & SampleFlag.Loop) != 0)
					{
						if (vnf.Current >= idxLoopEnd)
						{
							// Do we have a loop address?
							if (vnf.LoopAddress == null)
							{
								vnf.Current = 0;
								vnf.Active = false;
								break;
							}

							// Copy the loop address
							s = vnf.Address = vnf.LoopAddress;

							// Should we release the sample?
							if (vnf.ReleaseLength != 0)
							{
								// Yes, so set the current position
								vnf.Current = vnf.Current - idxLoopEnd;
								vnf.Flags |= SampleFlag.Release;
								vnf.Flags &= ~SampleFlag.Loop;
							}
							else
							{
								// The sample is looping, so check if it reached the loopEnd index
								if ((vnf.Flags & SampleFlag.Bidi) != 0)
								{
									// Sample is doing bidirectional loops, so 'bounce'
									// the current index against the idxLoopEnd
									vnf.Current = idxLoopEnd - (vnf.Current - idxLoopEnd);
									vnf.Increment = -vnf.Increment;
									vnf.Flags |= SampleFlag.Reverse;
								}
								else
								{
									// Normal looping, so set the
									// current position to loopEnd index
									vnf.Current = idxLoopPos + (vnf.Current - idxLoopEnd);
								}
							}
						}
					}
					else
					{
						// Sample is not looping, so check if it reached the last position
						if ((vnf.Flags & SampleFlag.Release) != 0)
						{
							// We play the release part
							if (vnf.Current >= idxReleaseEnd)
							{
								// Stop playing this sample
								vnf.Current = 0;
								vnf.Active = false;
								break;
							}
						}
						else
						{
							if (vnf.Current >= idxSize)
							{
								// Stop playing this sample
								vnf.Current = 0;
								vnf.Active = false;
								break;
							}
						}
					}
				}

				long end = (vnf.Flags & SampleFlag.Reverse) != 0 ?
							(vnf.Flags & SampleFlag.Loop) != 0 ? idxLoopPos : 0 :
							(vnf.Flags & SampleFlag.Loop) != 0 ? idxLoopEnd :
							(vnf.Flags & SampleFlag.Release) != 0 ? idxReleaseEnd : idxSize;

				// If the sample is not blocked
				int done;

				if ((end == vnf.Current) || (vnf.Increment == 0))
					done = 0;
				else
				{
					done = Math.Min((int)((end - vnf.Current) / vnf.Increment + 1), todo);
					if (done < 0)
						done = 0;
				}

				if (done == 0)
				{
					vnf.Active = false;
					break;
				}

				long endPos = vnf.Current + done * vnf.Increment;

				if ((vnf.LeftVolume != 0) || (vnf.RightVolume != 0))
				{
					GCHandle pinnedBuf = GCHandle.Alloc(buf, GCHandleType.Pinned);

					try
					{
						IntPtr bufAddr = pinnedBuf.AddrOfPinnedObject();

						bool use64Mixers = true;
						if (!Environment.Is64BitProcess)
						{
							// Use 32 bit mixers as often as we can (they're much faster)
							if ((vnf.Current < 0x7fffffff) && (endPos < 0x7fffffff))
							{
								// Use 32 bit mixers
								use64Mixers = false;

								// Check to see if we need to make interpolation on the mixing
								if ((mode & MixerMode.Interpolation) != 0)
								{
									if ((vnf.Flags & SampleFlag._16Bits) != 0)
									{
										// 16 bit input sample to be mixed
										if ((mode & MixerMode.Stereo) != 0)
										{
											if ((vnf.Panning == (int)ChannelPanning.Surround) && ((mode & MixerMode.Surround) != 0))
												vnf.Current = Native.Mix16SurroundInterp32(s, bufAddr, offset, (int)vnf.Current, (int)vnf.Increment, done, vnf.LeftVolumeSelected, vnf.RightVolumeSelected, vnf.OldLeftVolume, vnf.OldRightVolume, ref vnf.RampVolume);
											else
												vnf.Current = Native.Mix16StereoInterp32(s, bufAddr, offset, (int)vnf.Current, (int)vnf.Increment, done, vnf.LeftVolumeSelected, vnf.RightVolumeSelected, vnf.OldLeftVolume, vnf.OldRightVolume, ref vnf.RampVolume);
										}
										else
											vnf.Current = Native.Mix16MonoInterp32(s, bufAddr, offset, (int)vnf.Current, (int)vnf.Increment, done, vnf.LeftVolumeSelected, vnf.OldLeftVolume, ref vnf.RampVolume);
									}
									else
									{
										// 8 bit input sample to be mixed
										if ((mode & MixerMode.Stereo) != 0)
										{
											if ((vnf.Panning == (int)ChannelPanning.Surround) && ((mode & MixerMode.Surround) != 0))
												vnf.Current = Native.Mix8SurroundInterp32(s, bufAddr, offset, (int)vnf.Current, (int)vnf.Increment, done, vnf.LeftVolumeSelected, vnf.RightVolumeSelected, vnf.OldLeftVolume, vnf.OldRightVolume, ref vnf.RampVolume);
											else
												vnf.Current = Native.Mix8StereoInterp32(s, bufAddr, offset, (int)vnf.Current, (int)vnf.Increment, done, vnf.LeftVolumeSelected, vnf.RightVolumeSelected, vnf.OldLeftVolume, vnf.OldRightVolume, ref vnf.RampVolume);
										}
										else
											vnf.Current = Native.Mix8MonoInterp32(s, bufAddr, offset, (int)vnf.Current, (int)vnf.Increment, done, vnf.LeftVolumeSelected, vnf.OldLeftVolume, ref vnf.RampVolume);
									}
								}
								else
								{
									// No interpolation
									if ((vnf.Flags & SampleFlag._16Bits) != 0)
									{
										// 16 bit input sample to be mixed
										if ((mode & MixerMode.Stereo) != 0)
										{
											if ((vnf.Panning == (int)ChannelPanning.Surround) && ((mode & MixerMode.Surround) != 0))
												vnf.Current = Native.Mix16SurroundNormal32(s, bufAddr, offset, (int)vnf.Current, (int)vnf.Increment, done, vnf.LeftVolumeSelected, vnf.RightVolumeSelected);
											else
												vnf.Current = Native.Mix16StereoNormal32(s, bufAddr, offset, (int)vnf.Current, (int)vnf.Increment, done, vnf.LeftVolumeSelected, vnf.RightVolumeSelected);
										}
										else
											vnf.Current = Native.Mix16MonoNormal32(s, bufAddr, offset, (int)vnf.Current, (int)vnf.Increment, done, vnf.LeftVolumeSelected);
									}
									else
									{
										// 8 bit input sample to be mixed
										if ((mode & MixerMode.Stereo) != 0)
										{
											if ((vnf.Panning == (int)ChannelPanning.Surround) && ((mode & MixerMode.Surround) != 0))
												vnf.Current = Native.Mix8SurroundNormal32(s, bufAddr, offset, (int)vnf.Current, (int)vnf.Increment, done, vnf.LeftVolumeSelected, vnf.RightVolumeSelected);
											else
												vnf.Current = Native.Mix8StereoNormal32(s, bufAddr, offset, (int)vnf.Current, (int)vnf.Increment, done, vnf.LeftVolumeSelected, vnf.RightVolumeSelected);
										}
										else
											vnf.Current = Native.Mix8MonoNormal32(s, bufAddr, offset, (int)vnf.Current, (int)vnf.Increment, done, vnf.LeftVolumeSelected);
									}
								}
							}
						}

						if (use64Mixers)
						{
							// Use 64 bit mixers
							//
							// Check to see if we need to make interpolation on the mixing
							if ((mode & MixerMode.Interpolation) != 0)
							{
								if ((vnf.Flags & SampleFlag._16Bits) != 0)
								{
									// 16 bit input sample to be mixed
									if ((mode & MixerMode.Stereo) != 0)
									{
										if ((vnf.Panning == (int)ChannelPanning.Surround) && ((mode & MixerMode.Surround) != 0))
											vnf.Current = Native.Mix16SurroundInterp64(s, bufAddr, offset, vnf.Current, vnf.Increment, done, vnf.LeftVolumeSelected, vnf.RightVolumeSelected, vnf.OldLeftVolume, vnf.OldRightVolume, ref vnf.RampVolume);
										else
											vnf.Current = Native.Mix16StereoInterp64(s, bufAddr, offset, vnf.Current, vnf.Increment, done, vnf.LeftVolumeSelected, vnf.RightVolumeSelected, vnf.OldLeftVolume, vnf.OldRightVolume, ref vnf.RampVolume);
									}
									else
										vnf.Current = Native.Mix16MonoInterp64(s, bufAddr, offset, vnf.Current, vnf.Increment, done, vnf.LeftVolumeSelected, vnf.OldLeftVolume, ref vnf.RampVolume);
								}
								else
								{
									// 8 bit input sample to be mixed
									if ((mode & MixerMode.Stereo) != 0)
									{
										if ((vnf.Panning == (int)ChannelPanning.Surround) && ((mode & MixerMode.Surround) != 0))
											vnf.Current = Native.Mix8SurroundInterp64(s, bufAddr, offset, vnf.Current, vnf.Increment, done, vnf.LeftVolumeSelected, vnf.RightVolumeSelected, vnf.OldLeftVolume, vnf.OldRightVolume, ref vnf.RampVolume);
										else
											vnf.Current = Native.Mix8StereoInterp64(s, bufAddr, offset, vnf.Current, vnf.Increment, done, vnf.LeftVolumeSelected, vnf.RightVolumeSelected, vnf.OldLeftVolume, vnf.OldRightVolume, ref vnf.RampVolume);
									}
									else
										vnf.Current = Native.Mix8MonoInterp64(s, bufAddr, offset, vnf.Current, vnf.Increment, done, vnf.LeftVolumeSelected, vnf.OldLeftVolume, ref vnf.RampVolume);
								}
							}
							else
							{
								// No interpolation
								if ((vnf.Flags & SampleFlag._16Bits) != 0)
								{
									// 16 bit input sample to be mixed
									if ((mode & MixerMode.Stereo) != 0)
									{
										if ((vnf.Panning == (int)ChannelPanning.Surround) && ((mode & MixerMode.Surround) != 0))
											vnf.Current = Native.Mix16SurroundNormal64(s, bufAddr, offset, vnf.Current, vnf.Increment, done, vnf.LeftVolumeSelected, vnf.RightVolumeSelected);
										else
											vnf.Current = Native.Mix16StereoNormal64(s, bufAddr, offset, vnf.Current, vnf.Increment, done, vnf.LeftVolumeSelected, vnf.RightVolumeSelected);
									}
									else
										vnf.Current = Native.Mix16MonoNormal64(s, bufAddr, offset, vnf.Current, vnf.Increment, done, vnf.LeftVolumeSelected);
								}
								else
								{
									// 8 bit input sample to be mixed
									if ((mode & MixerMode.Stereo) != 0)
									{
										if ((vnf.Panning == (int)ChannelPanning.Surround) && ((mode & MixerMode.Surround) != 0))
											vnf.Current = Native.Mix8SurroundNormal64(s, bufAddr, offset, vnf.Current, vnf.Increment, done, vnf.LeftVolumeSelected, vnf.RightVolumeSelected);
										else
											vnf.Current = Native.Mix8StereoNormal64(s, bufAddr, offset, vnf.Current, vnf.Increment, done, vnf.LeftVolumeSelected, vnf.RightVolumeSelected);
									}
									else
										vnf.Current = Native.Mix8MonoNormal64(s, bufAddr, offset, vnf.Current, vnf.Increment, done, vnf.LeftVolumeSelected);
								}
							}
						}
					}
					finally
					{
						pinnedBuf.Free();
					}
				}
				else
				{
					// Update the sample position
					vnf.Current = endPos;
				}

				todo -= done;
				offset += (mode & MixerMode.Stereo) != 0 ? done << 1 : done;
			}
		}
	}
}
