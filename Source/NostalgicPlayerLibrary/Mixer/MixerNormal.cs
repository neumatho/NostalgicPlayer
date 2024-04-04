/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Kit.Containers.Types;
using Polycode.NostalgicPlayer.PlayerLibrary.Mixer.Containers;
using Polycode.NostalgicPlayer.PlayerLibrary.Utility;

namespace Polycode.NostalgicPlayer.PlayerLibrary.Mixer
{
	/// <summary>
	/// Normal mixer implementation
	/// </summary>
	internal class MixerNormal : MixerBase
	{
		private const int FracBits = 11;
		private const int FracMask = ((1 << FracBits) - 1);

		private const int ClickShift = 6;
		private const int ClickBuffer = 1 << ClickShift;

		#region Overrides
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
			return ClickBuffer;
		}



		/********************************************************************/
		/// <summary>
		/// This is the main mixer method
		/// </summary>
		/********************************************************************/
		public override void Mixing(int[][] channelMap, int offsetInSamples, int todoInSamples, MixerMode mode)
		{
			// Loop through all the channels and mix the samples into the buffer
			for (int t = 0; t < channelNumber; t++)
			{
				ref VoiceInfo vnf = ref voiceInfo[t];

				if (vnf.Kick)
				{
					vnf.Current = ((long)vnf.Start) << FracBits;
					vnf.Kick = false;
					vnf.Active = true;
				}

				if (vnf.Frequency == 0)
					vnf.Active = false;

				if (vnf.Active)
				{
					vnf.Increment = ((long)vnf.Frequency << FracBits) / mixerFrequency;

					if ((vnf.Flags & SampleFlag.Reverse) != 0)
						vnf.Increment = -vnf.Increment;

					if ((vnf.Flags & SampleFlag.ChangePosition) != 0)
					{
						long newPosition;

						if (vnf.RelativePosition)
							newPosition = (vnf.Current >> FracBits) + vnf.NewPosition;
						else
							newPosition = vnf.NewPosition;

						if ((newPosition >= 0) && (newPosition < (((vnf.Flags & SampleFlag.Loop) != 0) ? vnf.RepeatEnd : vnf.Size)))
							vnf.Current = newPosition << FracBits;

						vnf.Flags &= ~SampleFlag.ChangePosition;
					}

					int vol;

					if (vnf.Enabled)
						vol = vnf.Volume;
					else
						vol = 0;

					vnf.OldLeftVolume = vnf.LeftVolumeSelected;
					vnf.OldRightVolume = vnf.RightVolumeSelected;

					if ((mode & MixerMode.Stereo) != 0)
					{
						if (vnf.Panning != (int)ChannelPanningType.Surround)
						{
							// Stereo, calculate the volume with panning
							int pan = (((vnf.Panning - 128) * stereoSeparation) / 128) + 128;

							vnf.LeftVolumeSelected = (vol * ((int)ChannelPanningType.Right - pan)) >> 8;
							vnf.RightVolumeSelected = (vol * pan) >> 8;
						}
						else
						{
							// Dolby Surround
							vnf.LeftVolumeSelected = vnf.RightVolumeSelected = vol / 2;
						}
					}
					else
					{
						// Well, just mono
						vnf.LeftVolumeSelected = vol;
					}

					AddChannel(ref vnf, channelMap[t], offsetInSamples, todoInSamples, mode);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Convert the mix buffer to the output format and store the result
		/// in the supplied buffer
		/// </summary>
		/********************************************************************/
		public override void ConvertMixedData(byte[] dest, int offsetInBytes, int[] source, int todoInSamples, int samplesToSkip, bool isStereo, bool swapSpeakers)
		{
			MixConvertTo32(MemoryMarshal.Cast<byte, int>(dest), offsetInBytes / 4, source, todoInSamples, samplesToSkip, isStereo, swapSpeakers);
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Mix a channel into the buffer
		/// </summary>
		/********************************************************************/
		private void AddChannel(ref VoiceInfo vnf, int[] buf, int offsetInSamples, int todoInSamples, MixerMode mode)
		{
			Array left = vnf.Addresses[0];
			Array right = vnf.Addresses[1];

			if (left == null)
			{
				vnf.Current = 0;
				vnf.Active = false;
				return;
			}

			// The current size of the playing sample in fixed point
			long idxSize = vnf.Size != 0 ? ((long)vnf.Size << FracBits) - 1 : 0;

			// The loop start position in fixed point
			long idxLoopPos = (long)vnf.RepeatPosition << FracBits;

			// The loop end position in fixed point
			long idxLoopEnd = vnf.RepeatEnd != 0 ? ((long)vnf.RepeatEnd << FracBits) - 1 : 0;

			// The release end position in fixed point
			long idxReleaseEnd = vnf.ReleaseEnd != 0 ? ((long)vnf.ReleaseEnd << FracBits) - 1 : 0;

			// Update the 'current' index so the sample loops, or
			// stops playing if it reached the end of the sample
			while (todoInSamples > 0)
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
							if (vnf.NewLoopAddresses[0] == null)
							{
								vnf.Current = 0;
								vnf.Active = false;
								break;
							}

							// Copy the loop address
							left = vnf.Addresses[0] = vnf.NewLoopAddresses[0];
							right = vnf.Addresses[1] = vnf.NewLoopAddresses[1];

							// Recalculate loop indexes
							long idxNewLoopPos = (long)vnf.NewRepeatPosition << FracBits;
							long idxNewLoopEnd = vnf.NewRepeatEnd != 0 ? ((long)vnf.NewRepeatEnd << FracBits) - 1 : 0;

							// Should we release the sample?
							if (vnf.ReleaseEnd != 0)
							{
								// Yes, so set the current position
								vnf.Current = idxLoopPos + (vnf.Current - idxLoopEnd);
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
									vnf.Current = idxNewLoopEnd - (vnf.Current - idxLoopEnd);
									vnf.Increment = -vnf.Increment;
									vnf.Flags |= SampleFlag.Reverse;
								}
								else
								{
									// Normal looping, so set the
									// current position to loopEnd index
									vnf.Current = idxNewLoopPos + (vnf.Current - idxLoopEnd);
								}

								// Copy new loop points
								vnf.RepeatPosition = vnf.NewRepeatPosition;
								vnf.RepeatEnd = vnf.NewRepeatEnd;

								idxLoopPos = idxNewLoopPos;
								idxLoopEnd = idxNewLoopEnd;
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

				if (((vnf.Increment > 0) && (vnf.Current >= end)) || ((vnf.Increment < 0) && (vnf.Current <= end)) || (vnf.Increment == 0))
					done = 0;
				else
				{
					done = Math.Min((int)((end - vnf.Current) / vnf.Increment + 1), todoInSamples);
					if (done < 0)
						done = 0;
				}

				if (done == 0)
				{
					vnf.Active = false;
					break;
				}

				long endPos = vnf.Current + done * vnf.Increment;

				if (vnf.Volume != 0)
				{
					if (right == null)
						vnf.Current = MixSample(ref vnf, left, buf, offsetInSamples, done, mode);
					else
					{
						if (((mode & MixerMode.Stereo) != 0) && (vnf.Panning != (int)ChannelPanningType.Surround))
						{
							int oldVolume = vnf.RightVolumeSelected;
							vnf.RightVolumeSelected = 0;
							MixSample(ref vnf, left, buf, offsetInSamples, done, mode);
							vnf.RightVolumeSelected = oldVolume;

							oldVolume = vnf.LeftVolumeSelected;
							vnf.LeftVolumeSelected = 0;
							vnf.Current = MixSample(ref vnf, right, buf, offsetInSamples, done, mode);
							vnf.LeftVolumeSelected = oldVolume;
						}
						else
						{
							MixSample(ref vnf, left, buf, offsetInSamples, done, mode);
							vnf.Current = MixSample(ref vnf, right, buf, offsetInSamples, done, mode);
						}
					}
				}
				else
				{
					// Update the sample position
					vnf.Current = endPos;
				}

				todoInSamples -= done;
				offsetInSamples += (mode & MixerMode.Stereo) != 0 ? done << 1 : done;
			}
		}
		#endregion

		#region Real mixing methods

		/********************************************************************/
		/// <summary>
		/// Mix the given sample into the output buffers
		/// </summary>
		/********************************************************************/
		private long MixSample(ref VoiceInfo vnf, Array s, int[] buf, int offsetInSamples, int todoInSamples, MixerMode mode)
		{
			// Check to see if we need to make interpolation on the mixing
			if ((mode & MixerMode.Interpolation) != 0)
			{
				if ((vnf.Flags & SampleFlag._16Bits) != 0)
				{
					Span<short> source = SampleHelper.ConvertSampleTo16Bit(s, 0);

					// 16 bit input sample to be mixed
					if ((mode & MixerMode.Stereo) != 0)
					{
						if ((vnf.Panning == (int)ChannelPanningType.Surround) && ((mode & MixerMode.Surround) != 0))
							return Mix16SurroundInterpolation(source, buf, offsetInSamples, vnf.Current, vnf.Increment, todoInSamples, vnf.LeftVolumeSelected, vnf.RightVolumeSelected, vnf.OldLeftVolume, vnf.OldRightVolume, ref vnf.RampVolume);

						return Mix16StereoInterpolation(source, buf, offsetInSamples, vnf.Current, vnf.Increment, todoInSamples, vnf.LeftVolumeSelected, vnf.RightVolumeSelected, vnf.OldLeftVolume, vnf.OldRightVolume, ref vnf.RampVolume);
					}

					return Mix16MonoInterpolation(source, buf, offsetInSamples, vnf.Current, vnf.Increment, todoInSamples, vnf.LeftVolumeSelected, vnf.OldLeftVolume, ref vnf.RampVolume);
				}
				else
				{
					Span<sbyte> source = SampleHelper.ConvertSampleTo8Bit(s, 0);

					// 8 bit input sample to be mixed
					if ((mode & MixerMode.Stereo) != 0)
					{
						if ((vnf.Panning == (int)ChannelPanningType.Surround) && ((mode & MixerMode.Surround) != 0))
							return Mix8SurroundInterpolation(source, buf, offsetInSamples, vnf.Current, vnf.Increment, todoInSamples, vnf.LeftVolumeSelected, vnf.RightVolumeSelected, vnf.OldLeftVolume, vnf.OldRightVolume, ref vnf.RampVolume);

						return Mix8StereoInterpolation(source, buf, offsetInSamples, vnf.Current, vnf.Increment, todoInSamples, vnf.LeftVolumeSelected, vnf.RightVolumeSelected, vnf.OldLeftVolume, vnf.OldRightVolume, ref vnf.RampVolume);
					}

					return Mix8MonoInterpolation(source, buf, offsetInSamples, vnf.Current, vnf.Increment, todoInSamples, vnf.LeftVolumeSelected, vnf.OldLeftVolume, ref vnf.RampVolume);
				}
			}

			// No interpolation
			if ((vnf.Flags & SampleFlag._16Bits) != 0)
			{
				Span<short> source = SampleHelper.ConvertSampleTo16Bit(s, 0);

				// 16 bit input sample to be mixed
				if ((mode & MixerMode.Stereo) != 0)
				{
					if ((vnf.Panning == (int)ChannelPanningType.Surround) && ((mode & MixerMode.Surround) != 0))
						return Mix16SurroundNormal(source, buf, offsetInSamples, vnf.Current, vnf.Increment, todoInSamples, vnf.LeftVolumeSelected, vnf.RightVolumeSelected);

					return Mix16StereoNormal(source, buf, offsetInSamples, vnf.Current, vnf.Increment, todoInSamples, vnf.LeftVolumeSelected, vnf.RightVolumeSelected);
				}

				return Mix16MonoNormal(source, buf, offsetInSamples, vnf.Current, vnf.Increment, todoInSamples, vnf.LeftVolumeSelected);
			}
			else
			{
				Span<sbyte> source = SampleHelper.ConvertSampleTo8Bit(s, 0);

				// 8 bit input sample to be mixed
				if ((mode & MixerMode.Stereo) != 0)
				{
					if ((vnf.Panning == (int)ChannelPanningType.Surround) && ((mode & MixerMode.Surround) != 0))
						return Mix8SurroundNormal(source, buf, offsetInSamples, vnf.Current, vnf.Increment, todoInSamples, vnf.LeftVolumeSelected, vnf.RightVolumeSelected);

					return Mix8StereoNormal(source, buf, offsetInSamples, vnf.Current, vnf.Increment, todoInSamples, vnf.LeftVolumeSelected, vnf.RightVolumeSelected);
				}

				return Mix8MonoNormal(source, buf, offsetInSamples, vnf.Current, vnf.Increment, todoInSamples, vnf.LeftVolumeSelected);
			}
		}

		#region 8 bit sample

		#region Normal
		/********************************************************************/
		/// <summary>
		/// Mixes a 8 bit sample into a mono output buffer
		/// </summary>
		/********************************************************************/
		private long Mix8MonoNormal(Span<sbyte> source, int[] dest, int offsetInSamples, long index, long increment, int todoInSamples, int volSel)
		{
			int len = source.Length;

			while (todoInSamples-- != 0)
			{
				long idx = index >> FracBits;
				if (idx >= len)
					break;

				int sample = source[(int)idx] << 7;
				index += increment;

				dest[offsetInSamples++] += volSel * sample;
			}

			return index;
		}



		/********************************************************************/
		/// <summary>
		/// Mixes a 8 bit sample into a stereo output buffer
		/// </summary>
		/********************************************************************/
		private long Mix8StereoNormal(Span<sbyte> source, int[] dest, int offsetInSamples, long index, long increment, int todoInSamples, int lVolSel, int rVolSel)
		{
			int len = source.Length;

			while (todoInSamples-- != 0)
			{
				long idx = index >> FracBits;
				if (idx >= len)
					break;

				int sample = source[(int)idx] << 8;
				index += increment;

				dest[offsetInSamples++] += lVolSel * sample;
				dest[offsetInSamples++] += rVolSel * sample;
			}

			return index;
		}



		/********************************************************************/
		/// <summary>
		/// Mixes a 8 bit surround sample into a stereo output buffer
		/// </summary>
		/********************************************************************/
		private long Mix8SurroundNormal(Span<sbyte> source, int[] dest, int offsetInSamples, long index, long increment, int todoInSamples, int lVolSel, int rVolSel)
		{
			int len = source.Length;

			if (lVolSel >= rVolSel)
			{
				while (todoInSamples-- != 0)
				{
					long idx = index >> FracBits;
					if (idx >= len)
						break;

					int sample = source[(int)idx] << 8;
					index += increment;

					dest[offsetInSamples++] += lVolSel * sample;
					dest[offsetInSamples++] -= lVolSel * sample;
				}
			}
			else
			{
				while (todoInSamples-- != 0)
				{
					long idx = index >> FracBits;
					if (idx >= len)
						break;

					int sample = source[(int)idx] << 8;
					index += increment;

					dest[offsetInSamples++] -= rVolSel * sample;
					dest[offsetInSamples++] += rVolSel * sample;
				}
			}

			return index;
		}
		#endregion

		#region Interpolation
		/********************************************************************/
		/// <summary>
		/// Mixes a 8 bit sample into a mono output buffer with interpolation
		/// </summary>
		/********************************************************************/
		private long Mix8MonoInterpolation(Span<sbyte> source, int[] dest, int offsetInSamples, long index, long increment, int todoInSamples, int volSel, int oldVol, ref int rampVol)
		{
			int len = source.Length;

			if (rampVol != 0)
			{
				oldVol -= volSel;

				while (todoInSamples-- != 0)
				{
					int idx = (int)(index >> FracBits);
					if (idx >= len)
						break;

					long a = (long)source[idx] << 7;
					long b = idx + 1 >= source.Length ? a : (long)source[idx + 1] << 7;

					int sample = (int)(a + ((b - a) * (index & FracMask) >> FracBits));
					index += increment;

					dest[offsetInSamples++] += ((volSel << ClickShift) + oldVol * rampVol) * sample >> ClickShift;

					if (--rampVol == 0)
						break;
				}

				if (todoInSamples < 0)
					return index;
			}

			while (todoInSamples-- != 0)
			{
				int idx = (int)(index >> FracBits);
				if (idx >= len)
					break;

				long a = (long)source[idx] << 7;
				long b = idx + 1 >= source.Length ? a : (long)source[idx + 1] << 7;

				int sample = (int)(a + ((b - a) * (index & FracMask) >> FracBits));
				index += increment;

				dest[offsetInSamples++] += volSel * sample;
			}

			return index;
		}



		/********************************************************************/
		/// <summary>
		/// Mixes a 8 bit sample into a stereo output buffer with
		/// interpolation
		/// </summary>
		/********************************************************************/
		private long Mix8StereoInterpolation(Span<sbyte> source, int[] dest, int offsetInSamples, long index, long increment, int todoInSamples, int lVolSel, int rVolSel, int oldLVol, int oldRVol, ref int rampVol)
		{
			int len = source.Length;

			if (rampVol != 0)
			{
				oldLVol -= lVolSel;
				oldRVol -= rVolSel;

				while (todoInSamples-- != 0)
				{
					int idx = (int)(index >> FracBits);
					if (idx >= len)
						break;

					long a = (long)source[idx] << 8;
					long b = idx + 1 >= source.Length ? a : (long)source[idx + 1] << 8;

					int sample = (int)(a + ((b - a) * (index & FracMask) >> FracBits));
					index += increment;

					dest[offsetInSamples++] += ((lVolSel << ClickShift) + oldLVol * rampVol) * sample >> ClickShift;
					dest[offsetInSamples++] += ((rVolSel << ClickShift) + oldRVol * rampVol) * sample >> ClickShift;

					if (--rampVol == 0)
						break;
				}

				if (todoInSamples < 0)
					return index;
			}

			while (todoInSamples-- != 0)
			{
				int idx = (int)(index >> FracBits);
				if (idx >= len)
					break;

				long a = (long)source[idx] << 8;
				long b = idx + 1 >= source.Length ? a : (long)source[idx + 1] << 8;

				int sample = (int)(a + ((b - a) * (index & FracMask) >> FracBits));
				index += increment;

				dest[offsetInSamples++] += lVolSel * sample;
				dest[offsetInSamples++] += rVolSel * sample;
			}

			return index;
		}



		/********************************************************************/
		/// <summary>
		/// Mixes a 8 bit surround sample into a stereo output buffer with
		/// interpolation
		/// </summary>
		/********************************************************************/
		private long Mix8SurroundInterpolation(Span<sbyte> source, int[] dest, int offsetInSamples, long index, long increment, int todoInSamples, int lVolSel, int rVolSel, int oldLVol, int oldRVol, ref int rampVol)
		{
			int oldVol, vol;
			int len = source.Length;

			if (lVolSel >= rVolSel)
			{
				vol = lVolSel;
				oldVol = oldLVol;
			}
			else
			{
				vol = rVolSel;
				oldVol = oldRVol;
			}

			if (rampVol != 0)
			{
				oldVol -= vol;

				while (todoInSamples-- != 0)
				{
					int idx = (int)(index >> FracBits);
					if (idx >= len)
						break;

					long a = (long)source[idx] << 8;
					long b = idx + 1 >= source.Length ? a : (long)source[idx + 1] << 8;

					int sample = (int)(a + ((b - a) * (index & FracMask) >> FracBits));
					index += increment;

					sample = ((vol << ClickShift) + oldVol * rampVol) * sample >> ClickShift;
					dest[offsetInSamples++] += sample;
					dest[offsetInSamples++] -= sample;

					if (--rampVol == 0)
						break;
				}

				if (todoInSamples < 0)
					return index;
			}

			while (todoInSamples-- != 0)
			{
				int idx = (int)(index >> FracBits);
				if (idx >= len)
					break;

				long a = (long)source[idx] << 8;
				long b = idx + 1 >= source.Length ? a : (long)source[idx + 1] << 8;

				int sample = (int)(a + ((b - a) * (index & FracMask) >> FracBits));
				index += increment;

				dest[offsetInSamples++] += vol * sample;
				dest[offsetInSamples++] -= vol * sample;
			}

			return index;
		}
		#endregion

		#endregion

		#region 16 bit sample

		#region Normal
		/********************************************************************/
		/// <summary>
		/// Mixes a 16 bit sample into a mono output buffer
		/// </summary>
		/********************************************************************/
		private long Mix16MonoNormal(Span<short> source, int[] dest, int offsetInSamples, long index, long increment, int todoInSamples, int volSel)
		{
			int len = source.Length;

			while (todoInSamples-- != 0)
			{
				long idx = index >> FracBits;
				if (idx >= len)
					break;

				int sample = source[(int)idx];
				index += increment;

				dest[offsetInSamples++] += volSel * sample;
			}

			return index;
		}



		/********************************************************************/
		/// <summary>
		/// Mixes a 16 bit sample into a stereo output buffer
		/// </summary>
		/********************************************************************/
		private long Mix16StereoNormal(Span<short> source, int[] dest, int offsetInSamples, long index, long increment, int todoInSamples, int lVolSel, int rVolSel)
		{
			int len = source.Length;

			while (todoInSamples-- != 0)
			{
				long idx = index >> FracBits;
				if (idx >= len)
					break;

				int sample = source[(int)idx];
				index += increment;

				dest[offsetInSamples++] += lVolSel * sample;
				dest[offsetInSamples++] += rVolSel * sample;
			}

			return index;
		}



		/********************************************************************/
		/// <summary>
		/// Mixes a 16 bit surround sample into a stereo output buffer
		/// </summary>
		/********************************************************************/
		private long Mix16SurroundNormal(Span<short> source, int[] dest, int offsetInSamples, long index, long increment, int todoInSamples, int lVolSel, int rVolSel)
		{
			int len = source.Length;

			if (lVolSel >= rVolSel)
			{
				while (todoInSamples-- != 0)
				{
					long idx = index >> FracBits;
					if (idx >= len)
						break;

					int sample = source[(int)idx];
					index += increment;

					dest[offsetInSamples++] += lVolSel * sample;
					dest[offsetInSamples++] -= lVolSel * sample;
				}
			}
			else
			{
				while (todoInSamples-- != 0)
				{
					long idx = index >> FracBits;
					if (idx >= len)
						break;

					int sample = source[(int)idx];
					index += increment;

					dest[offsetInSamples++] -= rVolSel * sample;
					dest[offsetInSamples++] += rVolSel * sample;
				}
			}

			return index;
		}
		#endregion

		#region Interpolation
		/********************************************************************/
		/// <summary>
		/// Mixes a 16 bit sample into a mono output buffer with interpolation
		/// </summary>
		/********************************************************************/
		private long Mix16MonoInterpolation(Span<short> source, int[] dest, int offsetInSamples, long index, long increment, int todoInSamples, int volSel, int oldVol, ref int rampVol)
		{
			int len = source.Length;

			if (rampVol != 0)
			{
				oldVol -= volSel;

				while (todoInSamples-- != 0)
				{
					int idx = (int)(index >> FracBits);
					if (idx >= len)
						break;

					long a = source[idx];
					long b = idx + 1 >= len ? a : source[idx + 1];

					int sample = (int)(a + ((b - a) * (index & FracMask) >> FracBits));
					index += increment;

					dest[offsetInSamples++] += ((volSel << ClickShift) + oldVol * rampVol) * sample >> ClickShift;

					if (--rampVol == 0)
						break;
				}

				if (todoInSamples < 0)
					return index;
			}

			while (todoInSamples-- != 0)
			{
				int idx = (int)(index >> FracBits);
				if (idx >= len)
					break;

				long a = source[idx];
				long b = idx + 1 >= len ? a : source[idx + 1];

				int sample = (int)(a + ((b - a) * (index & FracMask) >> FracBits));
				index += increment;

				dest[offsetInSamples++] += volSel * sample;
			}

			return index;
		}



		/********************************************************************/
		/// <summary>
		/// Mixes a 16 bit sample into a stereo output buffer with
		/// interpolation
		/// </summary>
		/********************************************************************/
		private long Mix16StereoInterpolation(Span<short> source, int[] dest, int offsetInSamples, long index, long increment, int todoInSamples, int lVolSel, int rVolSel, int oldLVol, int oldRVol, ref int rampVol)
		{
			int len = source.Length;

			if (rampVol != 0)
			{
				oldLVol -= lVolSel;
				oldRVol -= rVolSel;

				while (todoInSamples-- != 0)
				{
					int idx = (int)(index >> FracBits);
					if (idx >= len)
						break;

					long a = source[idx];
					long b = idx + 1 >= len ? a : source[idx + 1];

					int sample = (int)(a + ((b - a) * (index & FracMask) >> FracBits));
					index += increment;

					dest[offsetInSamples++] += ((lVolSel << ClickShift) + oldLVol * rampVol) * sample >> ClickShift;
					dest[offsetInSamples++] += ((rVolSel << ClickShift) + oldRVol * rampVol) * sample >> ClickShift;

					if (--rampVol == 0)
						break;
				}

				if (todoInSamples < 0)
					return index;
			}

			while (todoInSamples-- != 0)
			{
				int idx = (int)(index >> FracBits);
				if (idx >= len)
					break;

				long a = source[idx];
				long b = idx + 1 >= len ? a : source[idx + 1];

				int sample = (int)(a + ((b - a) * (index & FracMask) >> FracBits));
				index += increment;

				dest[offsetInSamples++] += lVolSel * sample;
				dest[offsetInSamples++] += rVolSel * sample;
			}

			return index;
		}



		/********************************************************************/
		/// <summary>
		/// Mixes a 16 bit surround sample into a stereo output buffer with
		/// interpolation
		/// </summary>
		/********************************************************************/
		private long Mix16SurroundInterpolation(Span<short> source, int[] dest, int offsetInSamples, long index, long increment, int todoInSamples, int lVolSel, int rVolSel, int oldLVol, int oldRVol, ref int rampVol)
		{
			int oldVol, vol;
			int len = source.Length;

			if (lVolSel >= rVolSel)
			{
				vol = lVolSel;
				oldVol = oldLVol;
			}
			else
			{
				vol = rVolSel;
				oldVol = oldRVol;
			}

			if (rampVol != 0)
			{
				oldVol -= vol;

				while (todoInSamples-- != 0)
				{
					int idx = (int)(index >> FracBits);
					if (idx >= len)
						break;

					long a = source[idx];
					long b = idx + 1 >= len ? a : source[idx + 1];

					int sample = (int)(a + ((b - a) * (index & FracMask) >> FracBits));
					index += increment;

					sample = ((vol << ClickShift) + oldVol * rampVol) * sample >> ClickShift;
					dest[offsetInSamples++] += sample;
					dest[offsetInSamples++] -= sample;

					if (--rampVol == 0)
						break;
				}

				if (todoInSamples < 0)
					return index;
			}

			while (todoInSamples-- != 0)
			{
				int idx = (int)(index >> FracBits);
				if (idx >= len)
					break;

				long a = source[idx];
				long b = idx + 1 >= len ? a : source[idx + 1];

				int sample = (int)(a + ((b - a) * (index & FracMask) >> FracBits));
				index += increment;

				dest[offsetInSamples++] += vol * sample;
				dest[offsetInSamples++] -= vol * sample;
			}

			return index;
		}
		#endregion

		#endregion

		#endregion

		#region Conversion methods
		private const int MixBitShift = 7;

		/********************************************************************/
		/// <summary>
		/// Converts the mixed data to a 32 bit sample buffer
		/// </summary>
		/********************************************************************/
		private void MixConvertTo32(Span<int> dest, int offsetInSamples, int[] source, int countInSamples, int samplesToSkip, bool isStereo, bool swapSpeakers)
		{
			long x1, x2, x3, x4;
			int remain;

			int sourceOffset = 0;

			if (swapSpeakers)
			{
				if (samplesToSkip == 0)
				{
					remain = countInSamples & 3;

					for (countInSamples >>= 2; countInSamples != 0; countInSamples--)
					{
						x1 = (long)source[sourceOffset++] << MixBitShift;
						x2 = (long)source[sourceOffset++] << MixBitShift;
						x3 = (long)source[sourceOffset++] << MixBitShift;
						x4 = (long)source[sourceOffset++] << MixBitShift;

						x1 = (x1 >= 2147483647) ? 2147483647 - 1 : (x1 < -2147483647) ? -2147483647 : x1;
						x2 = (x2 >= 2147483647) ? 2147483647 - 1 : (x2 < -2147483647) ? -2147483647 : x2;
						x3 = (x3 >= 2147483647) ? 2147483647 - 1 : (x3 < -2147483647) ? -2147483647 : x3;
						x4 = (x4 >= 2147483647) ? 2147483647 - 1 : (x4 < -2147483647) ? -2147483647 : x4;

						dest[offsetInSamples++] = (int)x2;
						dest[offsetInSamples++] = (int)x1;
						dest[offsetInSamples++] = (int)x4;
						dest[offsetInSamples++] = (int)x3;
					}
				}
				else
				{
					remain = countInSamples & 1;

					for (countInSamples >>= 1; countInSamples != 0; countInSamples--)
					{
						x1 = (long)source[sourceOffset++] << MixBitShift;
						x2 = (long)source[sourceOffset++] << MixBitShift;

						x1 = (x1 >= 2147483647) ? 2147483647 - 1 : (x1 < -2147483647) ? -2147483647 : x1;
						x2 = (x2 >= 2147483647) ? 2147483647 - 1 : (x2 < -2147483647) ? -2147483647 : x2;

						dest[offsetInSamples++] = (int)x2;
						dest[offsetInSamples++] = (int)x1;

						for (int i = 0; i < samplesToSkip; i++)
							dest[offsetInSamples++] = 0;
					}
				}
			}
			else
			{
				if (isStereo)
				{
					if (samplesToSkip == 0)
					{
						remain = countInSamples & 3;

						for (countInSamples >>= 2; countInSamples != 0; countInSamples--)
						{
							x1 = (long)source[sourceOffset++] << MixBitShift;
							x2 = (long)source[sourceOffset++] << MixBitShift;
							x3 = (long)source[sourceOffset++] << MixBitShift;
							x4 = (long)source[sourceOffset++] << MixBitShift;

							x1 = (x1 >= 2147483647) ? 2147483647 - 1 : (x1 < -2147483647) ? -2147483647 : x1;
							x2 = (x2 >= 2147483647) ? 2147483647 - 1 : (x2 < -2147483647) ? -2147483647 : x2;
							x3 = (x3 >= 2147483647) ? 2147483647 - 1 : (x3 < -2147483647) ? -2147483647 : x3;
							x4 = (x4 >= 2147483647) ? 2147483647 - 1 : (x4 < -2147483647) ? -2147483647 : x4;

							dest[offsetInSamples++] = (int)x1;
							dest[offsetInSamples++] = (int)x2;
							dest[offsetInSamples++] = (int)x3;
							dest[offsetInSamples++] = (int)x4;
						}
					}
					else
					{
						remain = countInSamples & 1;

						for (countInSamples >>= 1; countInSamples != 0; countInSamples--)
						{
							x1 = (long)source[sourceOffset++] << MixBitShift;
							x2 = (long)source[sourceOffset++] << MixBitShift;

							x1 = (x1 >= 2147483647) ? 2147483647 - 1 : (x1 < -2147483647) ? -2147483647 : x1;
							x2 = (x2 >= 2147483647) ? 2147483647 - 1 : (x2 < -2147483647) ? -2147483647 : x2;

							dest[offsetInSamples++] = (int)x1;
							dest[offsetInSamples++] = (int)x2;

							for (int i = 0; i < samplesToSkip; i++)
								dest[offsetInSamples++] = 0;
						}
					}
				}
				else
				{
					remain = 0;

					for (; countInSamples != 0; countInSamples--)
					{
						x1 = (long)source[sourceOffset++] << MixBitShift;
						x1 = (x1 >= 2147483647) ? 2147483647 - 1 : (x1 < -2147483647) ? -2147483647 : x1;
						dest[offsetInSamples++] = (int)x1;
					}
				}
			}

			while (remain-- != 0)
			{
				x1 = (long)source[sourceOffset++] << MixBitShift;
				x1 = (x1 >= 2147483647) ? 2147483647 - 1 : (x1 < -2147483647) ? -2147483647 : x1;
				dest[offsetInSamples++] = (int)x1;

				for (int i = 0; i < samplesToSkip; i++)
					dest[offsetInSamples++] = 0;
			}
		}
		#endregion
	}
}
