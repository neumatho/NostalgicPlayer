/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Containers.Types;
using Polycode.NostalgicPlayer.PlayerLibrary.Sound.Mixer.Containers;
using Polycode.NostalgicPlayer.PlayerLibrary.Utility;

namespace Polycode.NostalgicPlayer.PlayerLibrary.Sound.Mixer
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
		public override void Mixing(MixerInfo currentMixerInfo, int[][][] channelMap, int offsetInFrames, int todoInFrames)
		{
			// Loop through all the channels and mix the samples into the buffer
			for (int t = 0; t < virtualChannelCount; t++)
			{
				VoiceInfo vnf = voiceInfo[t];
				VoiceSampleInfo vsi = vnf.SampleInfo;

				if (vnf.Kick)
				{
					vnf.Current = ((long)vsi.Sample.Start) << FracBits;
					vnf.Kick = false;
					vnf.Active = true;
				}

				if (vnf.Frequency == 0)
					vnf.Active = false;

				if (vnf.Active)
				{
					vnf.Increment = ((long)vnf.Frequency << FracBits) / mixerFrequency;

					if ((vsi.Flags & SampleFlag.Reverse) != 0)
						vnf.Increment = -vnf.Increment;

					if ((vnf.Flags & VoiceFlag.ChangePosition) != 0)
					{
						long newPosition;

						if (vnf.RelativePosition)
							newPosition = (vnf.Current >> FracBits) + vnf.NewPosition;
						else
							newPosition = vnf.NewPosition;

						if ((newPosition >= 0) && (newPosition < (vsi.Loop != null ? (vsi.Loop.Start + vsi.Loop.Length) : (vsi.Sample.Start + vsi.Sample.Length))))
							vnf.Current = newPosition << FracBits;

						vnf.Flags &= ~VoiceFlag.ChangePosition;
					}

					int vol = vnf.Enabled ? vnf.Volume : 0;

					Array.Copy(vnf.PanningVolume, vnf.OldPanningVolume, vnf.PanningVolume.Length);

					if (vnf.Panning != (int)ChannelPanningType.Surround)
					{
						// Stereo, calculate the volume with panning
						int pan = (((vnf.Panning - 128) * stereoSeparation) / 128) + 128;

						vnf.PanningVolume[0] = (vol * ((int)ChannelPanningType.Right - pan)) >> 8;
						vnf.PanningVolume[1] = (vol * pan) >> 8;
					}
					else
					{
						// Dolby Surround
						vnf.PanningVolume[0] = vnf.PanningVolume[1] = vol / 2;
					}

					AddChannel(currentMixerInfo, vnf, channelMap[t], offsetInFrames, todoInFrames);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Convert the mixing buffers to 32 bit
		/// </summary>
		/********************************************************************/
		public override void ConvertMixedData(int[] buffer, int todoInFrames)
		{
			MixConvertTo32(buffer, todoInFrames);
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Mix a channel into the buffer
		/// </summary>
		/********************************************************************/
		private void AddChannel(MixerInfo currentMixerInfo, VoiceInfo vnf, int[][] mixingBuffers, int offsetInFrames, int todoInFrames)
		{
			// todoInFrames at this point is actually the same as todoInSamples, since it works on the
			// sample to be mixed into the buf[] and the sample is in mono

			VoiceSampleInfo vsi = vnf.SampleInfo;

			Array sampleData = vsi.Sample.SampleData;

			if (sampleData == null)
			{
				vnf.Current = 0;
				vnf.Active = false;
				return;
			}

			// The current size of the playing sample in fixed point
			long idxEnd = vsi.Sample.Length != 0 ? ((long)(vsi.Sample.Start + vsi.Sample.Length) << FracBits) - 1 : 0;

			long idxLoopStart = 0, idxLoopEnd = 0;

			if (vsi.Loop != null)
			{
				// The loop start position in fixed point
				idxLoopStart = (long)vsi.Loop.Start << FracBits;

				// The loop end position in fixed point
				idxLoopEnd = vsi.Loop.Length != 0 ? ((long)(vsi.Loop.Start + vsi.Loop.Length) << FracBits) - 1 : 0;
			}

			// Update the 'current' index so the sample loops, or
			// stops playing if it reached the end of the sample
			while (todoInFrames > 0)
			{
				if ((vsi.Flags & SampleFlag.Reverse) != 0)
				{
					// The sampling is playing in reverse
					if ((vsi.Loop != null) && (vnf.Current < idxLoopStart))
					{
						// The sample is looping, and has reached the loop start index
						if ((vsi.Flags & SampleFlag.Bidi) != 0)
						{
							// Sample is doing bidirectional loops, so 'bounce'
							// the current index against the idxLoopStart
							vnf.Current = idxLoopStart + (idxLoopStart - vnf.Current);
							vnf.Increment = -vnf.Increment;
							vsi.Flags &= ~SampleFlag.Reverse;
						}
						else
						{
							// Normal backwards looping, so set the
							// current position to loopEnd index
							vnf.Current = idxLoopEnd - (idxLoopStart - vnf.Current);
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
					void SetNewSample()
					{
						vnf.SampleInfo = vnf.NewSampleInfo;
						vnf.NewSampleInfo = null;

						vsi = vnf.SampleInfo;
						vnf.Current = (long)vsi.Sample.Start << FracBits;
						idxEnd = vsi.Sample.Length != 0 ? ((long)(vsi.Sample.Start + vsi.Sample.Length) << FracBits) - 1 : 0;

						if (vsi.Loop != null)
						{
							idxLoopStart = (long)vsi.Loop.Start << FracBits;
							idxLoopEnd = vsi.Loop.Length != 0 ? ((long)(vsi.Loop.Start + vsi.Loop.Length) << FracBits) - 1 : 0;
						}
						else
						{
							idxLoopStart = 0;
							idxLoopEnd = 0;
						}

						sampleData = vsi.Sample.SampleData;
					}

					// The sample is playing forward
					if (vsi.Loop != null)
					{
						if (vnf.Current >= idxLoopEnd)
						{
							if (vnf.NewSampleInfo != null)
								SetNewSample();
							else
							{
								// Loop sample
								//
								// Copy the loop address
								sampleData = vsi.Loop.SampleData;

								// Recalculate loop indexes
								long idxNewLoopStart = (long)vsi.Loop.Start << FracBits;
								long idxNewLoopEnd = vsi.Loop.Length != 0 ? ((long)(vsi.Loop.Start + vsi.Loop.Length) << FracBits) - 1 : 0;

								// The sample is looping, so check if it reached the loopEnd index
								if ((vnf.SampleInfo.Flags & SampleFlag.Bidi) != 0)
								{
									// Sample is doing bidirectional loops, so 'bounce'
									// the current index against the idxLoopEnd
									vnf.Current = idxNewLoopEnd - (vnf.Current - idxLoopEnd);
									vnf.Increment = -vnf.Increment;
									vsi.Flags |= SampleFlag.Reverse;
								}
								else
								{
									// Normal looping, so set the
									// current position to loopEnd index
									vnf.Current = idxNewLoopStart + (vnf.Current - idxLoopEnd);
								}

								idxLoopStart = idxNewLoopStart;
								idxLoopEnd = idxNewLoopEnd;
							}
						}
					}
					else
					{
						// Sample is not looping, so check if it reached the last position
						if (vnf.Current >= idxEnd)
						{
							if (vnf.NewSampleInfo != null)
								SetNewSample();
							else
							{
								// Stop playing this sample
								vnf.Current = 0;
								vnf.Active = false;
								break;
							}
						}
					}
				}

				long end = (vsi.Flags & SampleFlag.Reverse) != 0 ? vsi.Loop != null ? idxLoopStart : 0 :
							vsi.Loop != null ? idxLoopEnd : idxEnd;

				// If the sample is not blocked
				int done;

				if (((vnf.Increment > 0) && (vnf.Current >= end)) || ((vnf.Increment < 0) && (vnf.Current <= end)) || (vnf.Increment == 0))
					done = 0;
				else
				{
					done = Math.Min((int)((end - vnf.Current) / vnf.Increment + 1), todoInFrames);
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
					long newCurrent = endPos;
					int rampVolume = vnf.RampVolume;

					if ((vsi.Flags & SampleFlag.Stereo) != 0)
					{
						// Left channel
						if (vnf.PanningVolume[0] != 0)
							newCurrent = MixSample(currentMixerInfo, vnf, sampleData, 0, 2, vnf.PanningVolume[0], vnf.OldPanningVolume[0], ref rampVolume, mixingBuffers[0], offsetInFrames, done);

						// Right channel
						if (vnf.PanningVolume[1] != 0)
						{
							rampVolume = vnf.RampVolume;
							newCurrent = MixSample(currentMixerInfo, vnf, sampleData, 1, 2, vnf.PanningVolume[1], vnf.OldPanningVolume[1], ref rampVolume, mixingBuffers[1], offsetInFrames, done);	
						}
					}
					else
					{
						if ((vnf.Panning == (int)ChannelPanningType.Surround) && (currentMixerInfo.SurroundMode != SurroundMode.None))//XX
						{
							// Mix the same sample into both front channels, but with negative volume on right channel
							// to encode it as Dolby Pro Logic surround
							if (vnf.PanningVolume[0] != 0)
							{
								MixSample(currentMixerInfo, vnf, sampleData, 0, 1, vnf.PanningVolume[0], vnf.OldPanningVolume[0], ref rampVolume, mixingBuffers[0], offsetInFrames, done);

								rampVolume = vnf.RampVolume;
								newCurrent = MixSample(currentMixerInfo, vnf, sampleData, 0, 1, -vnf.PanningVolume[0], vnf.OldPanningVolume[0], ref rampVolume, mixingBuffers[1], offsetInFrames, done);
							}

							// Take the rest of the channels
							for (int i = 2; i < mixingBuffers.Length; i++)
							{
								if (vnf.PanningVolume[i] != 0)
								{
									rampVolume = vnf.RampVolume;
									newCurrent = MixSample(currentMixerInfo, vnf, sampleData, 0, 1, vnf.PanningVolume[i], vnf.OldPanningVolume[i], ref rampVolume, mixingBuffers[i], offsetInFrames, done);
								}
							}
						}
						else
						{
							for (int i = 0; i < mixingBuffers.Length; i++)
							{
								if (vnf.PanningVolume[i] != 0)
								{
									rampVolume = vnf.RampVolume;
									newCurrent = MixSample(currentMixerInfo, vnf, sampleData, 0, 1, vnf.PanningVolume[i], vnf.OldPanningVolume[i], ref rampVolume, mixingBuffers[i], offsetInFrames, done);
								}
							}
						}
					}

					vnf.RampVolume = rampVolume;
					vnf.Current = newCurrent;
				}
				else
				{
					// Update the sample position
					vnf.Current = endPos;
				}

				todoInFrames -= done;
				offsetInFrames += done;
			}
		}
		#endregion

		#region Real mixing methods
		/********************************************************************/
		/// <summary>
		/// Mix the given sample into the output buffers
		/// </summary>
		/********************************************************************/
		private long MixSample(MixerInfo currentMixerInfo, VoiceInfo vnf, Array sampleData, uint sampleStartOffset, int sampleStep, int volume, int oldVolume, ref int rampVolume, int[] mixingBuffer, int offsetInFrames, int todoInFrames)
		{
			// Check to see if we need to make interpolation on the mixing
			if (currentMixerInfo.EnableInterpolation)
			{
				if ((vnf.SampleInfo.Flags & SampleFlag._16Bits) != 0)
				{
					Span<short> source = SampleHelper.ConvertSampleTypeTo16Bit(sampleData, sampleStartOffset);

					return Mix16Interpolation(source, sampleStep, mixingBuffer, offsetInFrames, vnf.Current, vnf.Increment, todoInFrames, volume, oldVolume, ref rampVolume);
				}
				else
				{
					// 8 bit input sample to be mixed
					Span<sbyte> source = SampleHelper.ConvertSampleTypeTo8Bit(sampleData, sampleStartOffset);

					return Mix8Interpolation(source, sampleStep, mixingBuffer, offsetInFrames, vnf.Current, vnf.Increment, todoInFrames, volume, oldVolume, ref rampVolume);
				}
			}

			// No interpolation
			if ((vnf.SampleInfo.Flags & SampleFlag._16Bits) != 0)
			{
				// 16 bit input sample to be mixed
				Span<short> source = SampleHelper.ConvertSampleTypeTo16Bit(sampleData, sampleStartOffset);

				return Mix16Normal(source, sampleStep, mixingBuffer, offsetInFrames, vnf.Current, vnf.Increment, todoInFrames, volume);
			}
			else
			{
				// 8 bit input sample to be mixed
				Span<sbyte> source = SampleHelper.ConvertSampleTypeTo8Bit(sampleData, sampleStartOffset);

				return Mix8Normal(source, sampleStep, mixingBuffer, offsetInFrames, vnf.Current, vnf.Increment, todoInFrames, volume);
			}
		}

		#region 8 bit sample
		/********************************************************************/
		/// <summary>
		/// Mixes an 8 bit sample into the output buffer
		/// </summary>
		/********************************************************************/
		private long Mix8Normal(Span<sbyte> source, int sampleStep, int[] dest, int offsetInFrames, long index, long increment, int todoInFrames, int volume)
		{
			int len = source.Length;

			while (todoInFrames-- != 0)
			{
				long idx = (index >> FracBits) * sampleStep;
				if (idx >= len)
					break;

				int sample = source[(int)idx] << 8;
				index += increment;

				dest[offsetInFrames++] += volume * sample;
			}

			return index;
		}



		/********************************************************************/
		/// <summary>
		/// Mixes an 8 bit sample into the output buffer with interpolation
		/// </summary>
		/********************************************************************/
		private long Mix8Interpolation(Span<sbyte> source, int sampleStep, int[] dest, int offsetInFrames, long index, long increment, int todoInFrames, int volume, int oldVolume, ref int rampVolume)
		{
			int len = source.Length;

			if (rampVolume != 0)
			{
				oldVolume -= volume;

				while (todoInFrames-- != 0)
				{
					int idx = (int)((index >> FracBits) * sampleStep);
					if (idx >= len)
						break;

					long a = (long)source[idx] << 8;
					long b = idx + sampleStep >= source.Length ? a : (long)source[idx + sampleStep] << 8;

					int sample = (int)(a + ((b - a) * (index & FracMask) >> FracBits));
					index += increment;

					dest[offsetInFrames++] += ((volume << ClickShift) + oldVolume * rampVolume) * sample >> ClickShift;

					if (--rampVolume == 0)
						break;
				}

				if (todoInFrames < 0)
					return index;
			}

			while (todoInFrames-- != 0)
			{
				int idx = (int)((index >> FracBits) * sampleStep);
				if (idx >= len)
					break;

				long a = (long)source[idx] << 8;
				long b = idx + sampleStep >= source.Length ? a : (long)source[idx + sampleStep] << 8;

				int sample = (int)(a + ((b - a) * (index & FracMask) >> FracBits));
				index += increment;

				dest[offsetInFrames++] += volume * sample;
			}

			return index;
		}
		#endregion

		#region 16 bit sample
		/********************************************************************/
		/// <summary>
		/// Mixes a 16 bit sample into the output buffer
		/// </summary>
		/********************************************************************/
		private long Mix16Normal(Span<short> source, int sampleStep, int[] dest, int offsetInFrames, long index, long increment, int todoInFrames, int volume)
		{
			int len = source.Length;

			while (todoInFrames-- != 0)
			{
				long idx = (index >> FracBits) * sampleStep;
				if (idx >= len)
					break;

				int sample = source[(int)idx];
				index += increment;

				dest[offsetInFrames++] += volume * sample;
			}

			return index;
		}



		/********************************************************************/
		/// <summary>
		/// Mixes a 16 bit sample into the output buffer with interpolation
		/// </summary>
		/********************************************************************/
		private long Mix16Interpolation(Span<short> source, int sampleStep, int[] dest, int offsetInSamples, long index, long increment, int todoInFrames, int volume, int oldVolume, ref int rampVolume)
		{
			int len = source.Length;

			if (rampVolume != 0)
			{
				oldVolume -= volume;

				while (todoInFrames-- != 0)
				{
					int idx = (int)((index >> FracBits) * sampleStep);
					if (idx >= len)
						break;

					long a = source[idx];
					long b = idx + sampleStep >= len ? a : source[idx + sampleStep];

					int sample = (int)(a + ((b - a) * (index & FracMask) >> FracBits));
					index += increment;

					dest[offsetInSamples++] += ((volume << ClickShift) + oldVolume * rampVolume) * sample >> ClickShift;

					if (--rampVolume == 0)
						break;
				}

				if (todoInFrames < 0)
					return index;
			}

			while (todoInFrames-- != 0)
			{
				int idx = (int)((index >> FracBits) * sampleStep);
				if (idx >= len)
					break;

				long a = source[idx];
				long b = idx + sampleStep >= len ? a : source[idx + sampleStep];

				int sample = (int)(a + ((b - a) * (index & FracMask) >> FracBits));
				index += increment;

				dest[offsetInSamples++] += volume * sample;
			}

			return index;
		}
		#endregion

		#endregion

		#region Conversion methods
		private const int MixBitShift = 7;

		/********************************************************************/
		/// <summary>
		/// Converts the mixed data to 32 bit in same buffer
		/// </summary>
		/********************************************************************/
		private void MixConvertTo32(int[] buffer, int todoInFrames)
		{
			long x1, x2, x3, x4;

			int offset = 0;
			int remain = todoInFrames & 3;

			for (todoInFrames >>= 2; todoInFrames != 0; todoInFrames--)
			{
				x1 = (long)buffer[offset] << MixBitShift;
				x2 = (long)buffer[offset + 1] << MixBitShift;
				x3 = (long)buffer[offset + 2] << MixBitShift;
				x4 = (long)buffer[offset + 3] << MixBitShift;

				x1 = (x1 >= 2147483647) ? 2147483647 - 1 : (x1 < -2147483647) ? -2147483647 : x1;
				x2 = (x2 >= 2147483647) ? 2147483647 - 1 : (x2 < -2147483647) ? -2147483647 : x2;
				x3 = (x3 >= 2147483647) ? 2147483647 - 1 : (x3 < -2147483647) ? -2147483647 : x3;
				x4 = (x4 >= 2147483647) ? 2147483647 - 1 : (x4 < -2147483647) ? -2147483647 : x4;

				buffer[offset] = (int)x1;
				buffer[offset + 1] = (int)x2;
				buffer[offset + 2] = (int)x3;
				buffer[offset + 3] = (int)x4;

				offset += 4;
			}

			while (remain-- != 0)
			{
				x1 = (long)buffer[offset] << MixBitShift;
				x1 = (x1 >= 2147483647) ? 2147483647 - 1 : (x1 < -2147483647) ? -2147483647 : x1;
				buffer[offset++] = (int)x1;
			}
		}
		#endregion
	}
}
