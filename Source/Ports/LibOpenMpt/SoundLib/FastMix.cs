/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Base;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib
{
	/// <summary>
	/// Mixer core for rendering samples, mixing plugins, etc...
	/// </summary>
	internal partial class CSoundFile
	{
		#region ChannelOffsets structure
		/// <summary>
		/// Holds the references to the end-of-sample pop reduction tail
		/// levels a single channel has to mix into. It is the equivalent of
		/// the two mixsample_t pointers used in the original code, so
		/// writing to OfsL/OfsR will update the member variable pointed to
		/// </summary>
		public readonly ref struct ChannelOffsets
		{
			public readonly ref mixsample_t OfsL;
			public readonly ref mixsample_t OfsR;

			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public ChannelOffsets(ref mixsample_t ofsL, ref mixsample_t ofsR)
			{
				OfsL = ref ofsL;
				OfsR = ref ofsR;
			}
		}
		#endregion

		#region MixLoopState structure
		private struct MixLoopState
		{
			public CPointer<int8> SamplePointer = null;
			public CPointer<int8> LookaheadPointer = null;
			public SmpLength LookaheadStart = 0;
			public uint32 MaxSamples = 0;
			public uint8 ItPingPongDiff;
			public bool PrecisePingPongLoops;

			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public MixLoopState(CSoundFile sndFile, ModChannel chn)
			{
				ItPingPongDiff = (uint8)(sndFile.m_PlayBehaviour[PlayBehaviour.ItPingPongMode] ? 1 : 0);
				PrecisePingPongLoops = !sndFile.m_PlayBehaviour[PlayBehaviour.ImprecisePingPongLoops];

				if (chn.pCurrentSample == null)
					return;

				UpdateLookaheadPointers(chn);

				// For platforms that have no fast 64-bit division, precompute this constant
				// as it won't change during the invocation of CreateStereoMix
				SamplePosition increment = chn.Increment;

				if (increment.IsNegative())
					increment.Negate();

				MaxSamples = 16384U / (increment.GetUInt() + 1U);

				if (MaxSamples < 2)
					MaxSamples = 2;
			}



			/********************************************************************/
			/// <summary>
			/// Calculate offset of loop wrap-around buffer for this sample
			/// </summary>
			/********************************************************************/
			public void UpdateLookaheadPointers(ModChannel chn)//XX 62
			{
				SamplePointer = chn.pCurrentSample != null ? chn.pCurrentSample.Cast<int8>() : null;
				LookaheadPointer.SetToNull();

				if (SamplePointer.IsNull)
					return;

				if (chn.nLoopEnd < Mixer.InterpolationLookaheadBufferSize)
					LookaheadStart = chn.nLoopStart;
				else
					LookaheadStart = Math.Max(chn.nLoopStart, chn.nLoopEnd - Mixer.InterpolationLookaheadBufferSize);

				// We only need to apply the loop wrap-around logic if the sample is actually looping.
				// As we round rather than truncate with No Interpolation, we also need the wrap-around logic for samples that are not interpolated
				if (chn.dwFlags.Test(ChannelFlags.Chn_Loop))
				{
					bool inSustainLoop = chn.InSustainLoop() && (chn.nLoopStart == chn.pModSample.nSustainStart) && (chn.nLoopEnd == chn.pModSample.nSustainEnd);

					// Do not enable wraparound magic if we're previewing a custom loop
					if (inSustainLoop || ((chn.nLoopStart == chn.pModSample.nLoopStart) && (chn.nLoopEnd == chn.pModSample.nLoopEnd)))
					{
						SmpLength lookaheadOffset = (3 * Mixer.InterpolationLookaheadBufferSize) + chn.pModSample.nLength - chn.nLoopEnd;

						if (inSustainLoop)
							lookaheadOffset += 4 * Mixer.InterpolationLookaheadBufferSize;

						LookaheadPointer = SamplePointer + (lookaheadOffset * chn.pModSample.GetBytesPerSample());
					}
				}
			}



			/********************************************************************/
			/// <summary>
			/// Returns the buffer length required to render a certain amount of
			/// samples, based on the channel's playback speed
			/// </summary>
			/********************************************************************/
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static uint32 DistanceToBufferLength(SamplePosition from, SamplePosition to, SamplePosition inc)//XX 92
			{
				if (from < to)
					return (uint32)((to - from - new SamplePosition(1)) / inc) + 1;
				else
					return 1;
			}



			/********************************************************************/
			/// <summary>
			/// Check how many samples can be rendered without encountering loop
			/// or sample end, and also update loop position / direction
			/// </summary>
			/********************************************************************/
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public uint32 GetSampleCount(ModChannel chn, uint32 nSamples)//XX 101
			{
				int32 nLoopStart = chn.dwFlags.Test(ChannelFlags.Chn_Loop) ? (int32)chn.nLoopStart : 0;
				SamplePosition nInc = chn.Increment;

				if ((nSamples <= 0) || nInc.IsZero() || (chn.nLength == 0) || SamplePointer.IsNull)
					return 0;

				// Part 1: Making sure the play position is valid, and if necessary, invert the play direction in case we reached a loop boundary of a ping-pong loop
				chn.pCurrentSample = SamplePointer;

				// Under zero?
				if (chn.Position.GetInt() < nLoopStart)
				{
					if (nInc.IsNegative())
					{
						// Invert loop direction for bidi loops
						chn.Position = new SamplePosition(nLoopStart + nLoopStart, 0) - chn.Position;

						if ((chn.Position.GetInt() < nLoopStart) || (chn.Position.GetUInt() >= ((nLoopStart + chn.nLength) / 2)))
							chn.Position.Set(nLoopStart, 0);

						if (chn.dwFlags.Test(ChannelFlags.Chn_PingPongLoop))
						{
							chn.dwFlags.Reset(ChannelFlags.Chn_PingPongFlag);	// Go forward
							nInc.Negate();
							chn.Increment = nInc;
						}
						else
							chn.Position.SetInt((int32)chn.nLength - 1);

						if (!chn.dwFlags.Test(ChannelFlags.Chn_Loop) || (chn.Position.GetUInt() >= chn.nLength))
						{
							chn.Position.Set((int32)chn.nLength);
							return 0;
						}
					}
					else
					{
						// We probably didn't hit the loop end yet (first loop), so we do nothing
						if (chn.Position.GetInt() < 0)
							chn.Position.SetInt(0);
					}
				} else if (chn.Position.GetUInt() >= chn.nLength)
				{
					// Past the end
					if (!chn.dwFlags.Test(ChannelFlags.Chn_Loop))
						return 0;	// Not looping -> stop this channel

					if (chn.dwFlags.Test(ChannelFlags.Chn_PingPongLoop))
					{
						// Invert loop
						if (nInc.IsPositive())
						{
							nInc.Negate();
							chn.Increment = nInc;
						}

						chn.dwFlags.Set(ChannelFlags.Chn_PingPongFlag);

						// Adjust loop position
						if (PrecisePingPongLoops)
						{
							// More accurate loop end overshoot calculation.
							// Test cases: BidiPrecision.it, BidiPrecision.xm
							SamplePosition overshoot = chn.Position - new SamplePosition((int32)chn.nLength, 0);
							uint32 loopLength = chn.nLoopEnd - chn.nLoopStart - ItPingPongDiff;

							if (overshoot.GetUInt() < loopLength)
								chn.Position = new SamplePosition((int32)chn.nLength - ItPingPongDiff, 0) - overshoot;
							else
								chn.Position = new SamplePosition((int32)chn.nLoopStart, 0);
						}
						else
						{
							SamplePosition invFract = chn.Position.GetInvertedFract();
							chn.Position = new SamplePosition((int32)(chn.nLength - (chn.Position.GetInt() - chn.nLength) - invFract.GetInt()), invFract.GetFract());

							if ((chn.Position.GetUInt() <= chn.nLoopStart) || (chn.Position.GetUInt() >= chn.nLength))
							{
								// Impulse Tracker's software mixer would put a -2 (instead of -1) in the following line (doesn't happen on a GUS)
								chn.Position.SetInt((int32)(chn.nLength - Math.Min(chn.nLength, (SmpLength)(ItPingPongDiff + 1))));
							}
						}
					}
					else
					{
						if (nInc.IsNegative())	// This is a bug
						{
							nInc.Negate();
							chn.Increment = nInc;
						}

						// Restart at loop start
						chn.Position += new SamplePosition((int32)(nLoopStart - chn.nLength), 0);

						// Interpolate correctly after wrapping around
						chn.dwFlags.Set(ChannelFlags.Chn_Wrapped_Loop);
					}
				}

				// Part 2: Compute how many samples we can render until we reach the end of sample / loop boundary / etc.

				SamplePosition nPos = chn.Position;
				SmpLength nPosInt = nPos.GetUInt();

				if (nPos.GetInt() < nLoopStart)
				{
					// Too big increment, and/or too small loop length
					if (nPos.IsNegative() || nInc.IsNegative())
						return 0;
				}
				else
				{
					// Not testing for equality since we might be going backwards from the very end of the sample
					if (nPosInt > chn.nLength)
						return 0;

					// If going forwards and we're precisely at the end, there's no point in going further
					if ((nPosInt == chn.nLength) && nInc.IsPositive())
						return 0;
				}

				uint32 nSmpCount = nSamples;
				SamplePosition nInv = nInc;

				if (nInc.IsNegative())
					nInv.Negate();

				OpenMpt.LimitMax(ref nSamples, MaxSamples);
				SamplePosition incSamples = nInc * (nSamples - 1);
				int32 nPosDest = (nPos + incSamples).GetInt();

				bool isAtLoopStart = (nPosInt >= chn.nLoopStart) && (nPosInt < (chn.nLoopStart + Mixer.InterpolationLookaheadBufferSize));

				if (!isAtLoopStart)
					chn.dwFlags.Reset(ChannelFlags.Chn_Wrapped_Loop);

				// Loop wrap-around magic
				bool checkDest = true;

				if (LookaheadPointer.IsNotNull)
				{
					if (nPosInt >= LookaheadStart)
					{
						// We are close to the loop end
						if (nInc.IsNegative())
						{
							// Going backward -> only go up to start of lookahead area
							nSmpCount = DistanceToBufferLength(new SamplePosition((int32)LookaheadStart, 0), nPos, nInv);
							chn.pCurrentSample = LookaheadPointer;
						}
						else if (nPosInt <= chn.nLoopEnd)
						{
							// Going forward, approaching loop end -> only go up to end of loop
							nSmpCount = DistanceToBufferLength(nPos, new SamplePosition((int32)chn.nLoopEnd, 0), nInv);
							chn.pCurrentSample = LookaheadPointer;
						}
						else
						{
							// We are already past the end of the loop
							nSmpCount = DistanceToBufferLength(nPos, new SamplePosition((int32)chn.nLength, 0), nInv);
						}

						checkDest = false;
					}
					else if (chn.dwFlags.Test(ChannelFlags.Chn_Wrapped_Loop) && isAtLoopStart)
					{
						// We just restarted the loop, so interpolate correctly after wrapping around
						nSmpCount = DistanceToBufferLength(nPos, new SamplePosition(nLoopStart + Mixer.InterpolationLookaheadBufferSize, 0), nInv);
						chn.pCurrentSample = LookaheadPointer + ((chn.nLoopEnd - nLoopStart) * chn.pModSample.GetBytesPerSample());
						checkDest = false;
					}
					else if (nInc.IsPositive() && ((SmpLength)nPosDest >= LookaheadStart) && (nSmpCount > 1))
					{
						// We shouldn't read that far if we're not using the pre-computed wrap-around buffer
						nSmpCount = DistanceToBufferLength(nPos, new SamplePosition((int32)LookaheadStart, 0), nInv);
						checkDest = false;
					}
				}

				if (checkDest)
				{
					// Fix up sample count if target position is invalid
					if (nInc.IsNegative())
					{
						if (nPosDest < nLoopStart)
							nSmpCount = DistanceToBufferLength(new SamplePosition(nLoopStart, 0), nPos, nInv);
					}
					else
					{
						if (nPosDest >= (int32)chn.nLength)
							nSmpCount = DistanceToBufferLength(nPos, new SamplePosition((int32)chn.nLength, 0), nInv);
					}
				}

				OpenMpt.Limit(ref nSmpCount, 1U, nSamples);

				return nSmpCount;
			}
		}
		#endregion

		/********************************************************************/
		/// <summary>
		/// Render count * number of channels samples
		/// </summary>
		/********************************************************************/
		public void CreateStereoMix(c_int count)//XX 302
		{
			if (count == 0)
				return;

			// Resetting sound buffer
			MixerLoops.StereoFill(MixSoundBuffer, (uint32)count, ref m_DryROfsVol, ref m_DryLOfsVol);

			if (m_MixerSettings.gnChannels > 2)
				MixerLoops.StereoFill(MixRearBuffer, (uint32)count, ref m_SurroundROfsVol, ref m_SurroundLOfsVol);

			// Channels that are actually mixed and not skipped (because they are paused or muted)
			ChannelIndex numChannelsMixed = 0;

			for (uint32 nChn = 0; nChn < m_nMixChannels; nChn++)
			{
				if (MixChannel(count, m_PlayState.Chn[m_PlayState.ChnMix[nChn]], m_PlayState.ChnMix[nChn], numChannelsMixed < m_MixerSettings.m_nMaxMixChannels))
					numChannelsMixed++;
			}

			m_nMixStat = Math.Max(m_nMixStat, numChannelsMixed);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public ChannelOffsets GetChannelOffsets(ModChannel chn, ChannelIndex channel)//XX 324
		{
			ref mixsample_t pOfsR = ref m_DryROfsVol;
			ref mixsample_t pOfsL = ref m_DryLOfsVol;

			if ((((m_MixerSettings.DSPMask & DspFlags.Reverb) != 0) && !chn.dwFlags.Test(ChannelFlags.Chn_NoReverb)) || chn.dwFlags.Test(ChannelFlags.Chn_Reverb))
			{
				pOfsR = ref m_RvbROfsVol;
				pOfsL = ref m_RvbLOfsVol;
			}

			if (chn.dwFlags.Test(ChannelFlags.Chn_Surround) && (m_MixerSettings.gnChannels > 2))
			{
				pOfsR = ref m_SurroundROfsVol;
				pOfsL = ref m_SurroundLOfsVol;
			}

			return new ChannelOffsets(ref pOfsL, ref pOfsR);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public bool MixChannel(c_int count, ModChannel chn, ChannelIndex channel, bool doMix)//XX 360
		{
			if ((chn.pCurrentSample != null) || (chn.nLOfs != 0) || (chn.nROfs != 0))
			{
				ChannelOffsets ofs = GetChannelOffsets(chn, channel);

				MixFuncTable.FunctionIndex functionNdx = (MixFuncTable.FunctionIndex)(uint32)MixFuncTable.ResamplingModeToMixFlags(chn.ResamplingMode);

				if (chn.dwFlags.Test(ChannelFlags.Chn_16Bit))
					functionNdx |= MixFuncTable.FunctionIndex._16Bit;

				if (chn.dwFlags.Test(ChannelFlags.Chn_Stereo))
					functionNdx |= MixFuncTable.FunctionIndex.Stereo;

				if (chn.dwFlags.Test(ChannelFlags.Chn_Filter))
					functionNdx |= MixFuncTable.FunctionIndex.Filter;

				CPointer<mixsample_t> pBuffer = MixSoundBuffer;

				if ((((m_MixerSettings.DSPMask & DspFlags.Reverb) != 0) && !chn.dwFlags.Test(ChannelFlags.Chn_NoReverb)) || (chn.dwFlags.Test(ChannelFlags.Chn_Reverb)))
				{
					m_Reverb.TouchReverbSendBuffer(ReverbSendBuffer, ref m_RvbROfsVol, ref m_RvbLOfsVol, (uint32)count);
					pBuffer = ReverbSendBuffer;
				}

				if (chn.dwFlags.Test(ChannelFlags.Chn_Surround) && (m_MixerSettings.gnChannels > 2))
					pBuffer = MixRearBuffer;

				if (chn.IsPaused)
				{
					MixerLoops.EndChannelOfs(chn, pBuffer, (uint32)count);
					ofs.OfsR += chn.nROfs;
					ofs.OfsL += chn.nLOfs;
					chn.nROfs = chn.nLOfs = 0;

					return false;
				}

				MixLoopState mixLoopState = new MixLoopState(this, chn);

				////////////////////////////////////////////////////
				bool addToMix = false;
				c_int nSamples = count;

				// Keep mixing this sample until the buffer is filled
				do
				{
					uint32 nRampSamples = (uint32)nSamples;

					if (chn.nRampLength > 0)
					{
						if (nRampSamples > chn.nRampLength)
							nRampSamples = chn.nRampLength;
					}

					int32 nSmpCount = (int32)mixLoopState.GetSampleCount(chn, nRampSamples);

					if (nSmpCount <= 0)
					{
						// Stopping the channel
						chn.pCurrentSample = null;
						chn.nLength = 0;
						chn.Position.Set(0);
						chn.nRampLength = 0;

						MixerLoops.EndChannelOfs(chn, pBuffer, (uint32)nSamples);

						ofs.OfsR += chn.nROfs;
						ofs.OfsL += chn.nLOfs;
						chn.nROfs = chn.nLOfs = 0;
						chn.dwFlags.Reset(ChannelFlags.Chn_PingPongFlag);
						break;
					}

					// Should we mix this channel?
					if (!doMix ||	// Too many channels
					    ((chn.nRampLength == 0) && ((chn.LeftVol | chn.RightVol) == 0)))	// Channel is completely silent
					{
						chn.Position += chn.Increment * (uint32)nSmpCount;
						chn.nROfs = chn.nLOfs = 0;
						pBuffer += nSmpCount * 2;
						addToMix = false;
					}
					else
					{
						// Do mixing
						CPointer<mixsample_t> pBufMax = pBuffer + (nSmpCount * 2);
						chn.nROfs = -(pBufMax[-2]);
						chn.nLOfs = -(pBufMax[-1]);

						MixFuncTable.Functions[(c_int)(functionNdx | (chn.nRampLength != 0 ? MixFuncTable.FunctionIndex.Ramp : 0))](chn, m_Resampler, pBuffer, (c_uint)nSmpCount);

						chn.nROfs += pBufMax[-2];
						chn.nLOfs += pBufMax[-1];

						pBuffer = pBufMax;
						addToMix = true;
					}

					nSamples -= nSmpCount;

					if (chn.nRampLength != 0)
					{
						if (chn.nRampLength <= (uint32)nSmpCount)
						{
							// Ramping is done
							chn.nRampLength = 0;
							chn.LeftVol = chn.NewLeftVol;
							chn.RightVol = chn.NewRightVol;
							chn.RightRamp = chn.LeftRamp = 0;

							if (chn.dwFlags.Test(ChannelFlags.Chn_NoteFade) && (chn.nFadeOutVol == 0))
							{
								chn.nLength = 0;
								chn.pCurrentSample = null;
							}
						}
						else
							chn.nRampLength -= (uint32)nSmpCount;
					}

					bool pastLoopEnd = (chn.Position.GetUInt() >= chn.nLoopEnd) && chn.dwFlags.Test(ChannelFlags.Chn_Loop);
					bool pastSampleEnd = (chn.Position.GetUInt() >= chn.nLength) && !chn.dwFlags.Test(ChannelFlags.Chn_Loop) && (chn.nLength != 0) && (chn.nMasterChn == 0);
					bool doSampleSwap = m_PlayBehaviour[PlayBehaviour.ModSampleSwap] && (chn.SwapSampleIndex != 0) && (chn.SwapSampleIndex <= GetNumSamples()) && (chn.pModSample != Samples[chn.SwapSampleIndex]);

					if ((pastLoopEnd || pastSampleEnd) && doSampleSwap)
					{
						// ProTracker compatibility: Instrument changes without a note do not happen instantly, but rather when the sample loop has finished playing.
						// Test case: PTInstrSwap.mod, PTSwapNoLoop.mod
						ModSample smp = Samples[chn.SwapSampleIndex];

						chn.pModSample = smp;
						chn.pCurrentSample = smp.SampleEv();
						chn.dwFlags = (chn.dwFlags & Snd_Def.Chn_ChannelFlags) | (smp.uFlags & Snd_Def.Smp_FlagsMask);

						if (smp.uFlags.Test(ChannelFlags.Chn_Loop))
							chn.nLength = smp.nLoopEnd;
						else if (!m_PlayBehaviour[PlayBehaviour.ModOneShotLoops])
							chn.nLength = smp.nLength;
						else
							chn.nLength = 0;	// Non-looping sample continue in oneshot mode (i.e. they will most probably just play silence)

						chn.nLoopStart = smp.nLoopStart;
						chn.nLoopEnd = smp.nLoopEnd;
						chn.Position.SetInt((int32)chn.nLoopStart);
						chn.SwapSampleIndex = 0;
						mixLoopState.UpdateLookaheadPointers(chn);

						if (chn.pCurrentSample == null)
							break;
					}
					else if (pastLoopEnd && !doSampleSwap && m_PlayBehaviour[PlayBehaviour.ModOneShotLoops] && (chn.nLoopStart == 0))
					{
						// ProTracker "oneshot" loops (if loop start is 0, play the whole sample once and then repeat until loop end)
						chn.Position.SetInt(0);
						chn.nLoopEnd = chn.nLength = chn.pModSample.nLoopEnd;
					}
				}
				while (nSamples > 0);

				// Restore sample pointer in case it got changed through loop wrap-around
				chn.pCurrentSample = mixLoopState.SamplePointer;

				return addToMix;
			}

			return false;
		}
	}
}
