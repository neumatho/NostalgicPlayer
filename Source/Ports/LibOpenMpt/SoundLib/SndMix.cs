/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Audio;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Base;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers;
using Algorithm = Polycode.NostalgicPlayer.Kit.C.Std.Algorithm;
using Random = Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Random.Random;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib
{
	/// <summary>
	/// Pattern playback, effect processing
	/// </summary>
	internal partial class CSoundFile
	{
		/// <summary>
		/// Log tables for pre-amp
		/// Pre-amp (or more precisely: Pre-attenuation) depends on the number of channels,
		/// which this table takes care of
		/// </summary>
		private static readonly uint8[] preAmpTable =
		[
			0x60, 0x60, 0x60, 0x70,	// 0-7
			0x80, 0x88, 0x90, 0x98,	// 8-15
			0xa0, 0xa4, 0xa8, 0xac,	// 16-23
			0xb0, 0xb4, 0xb8, 0xbc	// 24-31
		];

		private static readonly int8[] dbmSinus =
		[
			33, 52, 69, 84, 96, 107, 116, 122,  125, 127,  125, 122, 116, 107, 96, 84,
			69, 52, 33, 13, -8, -31, -54, -79, -104,-128, -104, -79, -54, -31, -8, 13
		];

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void SetMixerSettings(ref MixerSettings mixerSettings)//XX 50
		{
			SetPreAmp(mixerSettings.m_nPreAmp);	// Adjust agc
			bool reset = false;

			if ((mixerSettings.gdwMixingFreq != m_MixerSettings.gdwMixingFreq) || (mixerSettings.gnChannels != m_MixerSettings.gnChannels) || (mixerSettings.MixerFlags != m_MixerSettings.MixerFlags))
				reset = true;

			m_MixerSettings = mixerSettings;

			InitPlayer(reset);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void SetResamplerSettings(ref CResamplerSettings resamplerSettings)//XX 66
		{
			m_Resampler.m_Settings = resamplerSettings;
			m_Resampler.UpdateTables();

			InitAmigaResampler();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void InitPlayer(bool bReset = false)//XX 74
		{
			if (bReset)
			{
				ResetMixStat();
				m_DryLOfsVol = m_DryROfsVol = 0;
				m_SurroundLOfsVol = m_SurroundROfsVol = 0;
				InitAmigaResampler();
			}

			m_Reverb.Initialize(bReset, ref m_RvbROfsVol, ref m_RvbLOfsVol, m_MixerSettings.gdwMixingFreq);

			m_Resampler.UpdateTables();

//XX			if (m_opl != null)
//				m_opl.Initialize(m_MixerSettings.gdwMixingFreq);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public bool FadeSong(uint32 msec)//XX 112
		{
			samplecount_t nSamples = (samplecount_t)Util.MulDiv((int32)msec, (int32)m_MixerSettings.gdwMixingFreq, 1000);

			if (nSamples <= 0)
				return false;

			if (nSamples > 0x100000)
				nSamples = 0x100000;

			m_PlayState.m_nBufferCount = nSamples;

			int32 nRampLength = (int32)m_PlayState.m_nBufferCount;

			// Ramp everything down
			for (uint32 nOff = 0; nOff < m_nMixChannels; nOff++)
			{
				ModChannel pramp = m_PlayState.Chn[m_PlayState.ChnMix[nOff]];

				pramp.NewRightVol = pramp.NewLeftVol = 0;
				pramp.LeftRamp = -pramp.LeftVol * (1 << Mixer.VolumeRampPrecision) / nRampLength;
				pramp.RightRamp = -pramp.RightVol * (1 << Mixer.VolumeRampPrecision) / nRampLength;
				pramp.RampLeftVol = pramp.LeftVol * (1 << Mixer.VolumeRampPrecision);
				pramp.RampRightVol = pramp.RightVol * (1 << Mixer.VolumeRampPrecision);
				pramp.nRampLength = (uint32)nRampLength;
				pramp.dwFlags.Set(ChannelFlags.Chn_VolumeRamp);
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Apply stereo separation factor on an interleaved stereo/quad
		/// stream.
		/// count = Number of stereo sample pairs to process
		/// separation = -256...256 (negative values = swap L/R, 0 = mono,
		/// 128 = normal)
		/// </summary>
		/********************************************************************/
		public void ApplyStereoSeparation(CPointer<mixsample_t> mixBuf, size_t count, int32 separation)//XX 138
		{
			mixsample_t factor_Num = separation;	// 128 =^= 1.0f
			mixsample_t factor_Den = MixerSettings.StereoSeparationScale;	// 128
			mixsample_t normalize_Den = 2;	// mid/side pre/post normalization
			mixsample_t mid_Den = normalize_Den;
			mixsample_t side_Num = factor_Num;
			mixsample_t side_Den = factor_Den * normalize_Den;

			for (size_t i = 0; i < count; i++)
			{
				mixsample_t l = mixBuf[0];
				mixsample_t r = mixBuf[1];
				mixsample_t m = l + r;
				mixsample_t s = l - r;

				m /= mid_Den;
				s = Util.MulDiv(s, side_Num, side_Den);
				l = m + s;
				r = m - s;

				mixBuf[0] = l;
				mixBuf[1] = r;

				mixBuf += 2;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void ApplyStereoSeparation(CPointer<mixsample_t> soundFrontBuffer, CPointer<mixsample_t> soundRearBuffer, size_t channels, size_t countChunk, int32 separation)//XX 175
		{
			if (separation == MixerSettings.StereoSeparationScale)
			{
				// Identity
				return;
			}

			if (channels >= 2)
				ApplyStereoSeparation(soundFrontBuffer, countChunk, separation);

			if (channels >= 4)
				ApplyStereoSeparation(soundRearBuffer, countChunk, separation);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void ProcessInputChannels(IAudioSource source, size_t countChunk)//XX 186
		{
			for (size_t channel = 0; channel < Mixer.NumMixInputBuffers; ++channel)
				Algorithm.fill(MixInputBuffer[channel], new CPointer<mixsample_t>(MixInputBuffer[channel], (c_int)countChunk), 0);

			CPointer<mixsample_t>[] buffers = new CPointer<mixsample_t>[Mixer.NumMixInputBuffers];

			for (size_t channel = 0; channel < Mixer.NumMixInputBuffers; ++channel)
				buffers[channel] = MixInputBuffer[channel];

			source.Process(new Audio_Span_Planar<MixSampleInt>(buffers, m_MixerSettings.NumInputChannels, countChunk));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public samplecount_t Read(samplecount_t count, IAudioTarget target, IAudioSource source, IMonitorOutput outputMonitor = null, IMonitorInput inputMonitor = null)//XX 222
		{
			samplecount_t countRendered = 0;
			samplecount_t countToRender = count;

			while (!m_PlayState.m_Flags.Test(PlayFlags.Song_EndReached) && (countToRender > 0))
			{
				// Update Channel Data
				if (m_PlayState.m_nBufferCount == 0)
				{
					// Last tick or fade completely processed, find out what to do next

					if (m_PlayState.m_Flags.Test(PlayFlags.Song_FadingSong))
					{
						// Song was faded out
						m_PlayState.m_Flags.Set(PlayFlags.Song_EndReached);
					}
					else if (ReadNote())
					{
						// Render next tick (normal progress)
					}
					else
					{
						// No new pattern data
						if (IsRenderingToDisc())
						{
							// Disable song fade when rendering or when requested in libopenmpt
							m_PlayState.m_Flags.Set(PlayFlags.Song_EndReached);
						}
						else	// End of song reached, fade it out
						{
							if (FadeSong(FadeSongDelay))	// sets m_nBufferCount xor returns false
							{	// FadeSong sets m_nBufferCount here
								m_PlayState.m_Flags.Set(PlayFlags.Song_FadingSong);
							}
							else
								m_PlayState.m_Flags.Set(PlayFlags.Song_EndReached);
						}
					}
				}

				if (m_PlayState.m_Flags.Test(PlayFlags.Song_EndReached))
				{
					// Mix done.
					//
					// If we decide to continue the mix (possible in libopenmpt), the tick count
					// is valid right now (0), meaning that no new row data will be processed.
					// This would effectively prolong the last played row
					m_PlayState.m_nTickCount = m_PlayState.TicksOnRow();
					break;
				}

				samplecount_t countChunk = Math.Min(Mixer.MixBufferSize, Math.Min(m_PlayState.m_nBufferCount, countToRender));

				if (m_MixerSettings.NumInputChannels > 0)
					ProcessInputChannels(source, countChunk);

				if (inputMonitor != null)
				{
					CPointer<mixsample_t>[] buffers = new CPointer<mixsample_t>[Mixer.NumMixInputBuffers];

					for (size_t channel = 0; channel < Mixer.NumMixInputBuffers; ++channel)
						buffers[channel] = MixInputBuffer[channel];

					inputMonitor.Process(new Audio_Span_Planar<mixsample_t>(buffers, m_MixerSettings.NumInputChannels, countChunk));
				}

				CreateStereoMix((c_int)countChunk);

/*//XX				if (m_Opl != null)
					m_Opl.Mix(MixSoundBuffer, countChunk, m_OplVolumeFactor * m_nVstiVolume / 48);*/

				m_Reverb.Process(MixSoundBuffer, ReverbSendBuffer, ref m_RvbROfsVol, ref m_RvbLOfsVol, countChunk);

				if (m_MixerSettings.gnChannels == 1)
					MixerLoops.MonoFromStereo(MixSoundBuffer, countChunk);

				if (m_PlayConfig.GetGlobalVolumeAppliesToMaster())
					ProcessGlobalVolume(countChunk);

				if (m_MixerSettings.m_nStereoSeparation != MixerSettings.StereoSeparationScale)
					ProcessStereoSeparation(countChunk);

				if (m_MixerSettings.DSPMask != DspFlags.None)
					ProcessDsp(countChunk);

				if (m_MixerSettings.gnChannels == 4)
					MixerLoops.InterleaveFrontRear(MixSoundBuffer, MixRearBuffer, countChunk);

				if (outputMonitor != null)
					outputMonitor.Process(new Audio_Span_Interleaved<mixsample_t>(MixSoundBuffer, m_MixerSettings.gnChannels, countChunk));

				target.Process(new Audio_Span_Interleaved<mixsample_t>(MixSoundBuffer, m_MixerSettings.gnChannels, countChunk));

				// Buffer ready
				countRendered += countChunk;
				countToRender -= countChunk;
				m_PlayState.m_nBufferCount -= countChunk;
				m_PlayState.m_lTotalSampleCount += countChunk;

				RowIndex rowsPerBeat = m_PlayState.m_nCurrentRowsPerBeat != 0 ? m_PlayState.m_nCurrentRowsPerBeat : Snd_Def.Default_Rows_Per_Beat;

				if ((m_PlayState.m_nBufferCount == 0) && !m_PlayState.m_Flags.Test(PlayFlags.Song_Paused))
					m_PlayState.m_ppqPosFract += 1.0 / (rowsPerBeat * m_PlayState.TicksOnRow());
			}

			// Mix done

			return countRendered;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void ProcessDsp(uint32 countChunk)//XX 399
		{
		}

		//XX 443
		#region Handles navigation/effects
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public bool ProcessRow()//XX 445
		{
			while (++m_PlayState.m_nTickCount >= m_PlayState.TicksOnRow())
			{
				(bool ignoreRow, bool patternTransition) = NextRow(m_PlayState, m_PlayState.m_Flags.Test(PlayFlags.Song_BreakToRow));

				m_PlayState.UpdatePpq(patternTransition);

				// Check if pattern is valid
				if (!m_PlayState.m_Flags.Test(PlayFlags.Song_PatternLoop))
				{
					size_t songEnd = m_MaxOrderPosition != 0 ? m_MaxOrderPosition : Order.Current.size();
					m_PlayState.m_nPattern = (m_PlayState.m_nCurrentOrder < songEnd) ? Order.Current[m_PlayState.m_nCurrentOrder] : Snd_Def.PatternIndex_Invalid;

					if ((m_PlayState.m_nPattern < Patterns.Size()) && !Patterns[m_PlayState.m_nPattern].IsValid())
						m_PlayState.m_nPattern = Snd_Def.PatternIndex_Skip;

					while (m_PlayState.m_nPattern >= Patterns.Size())
					{
						// End of song?
						if ((m_PlayState.m_nPattern == Snd_Def.PatternIndex_Invalid) || (m_PlayState.m_nCurrentOrder >= songEnd))
						{
							OrderIndex restartPosOverride = m_MaxOrderPosition != 0 ? m_RestartOverridePos : Order.Current.GetRestartPos();

							if ((restartPosOverride == 0) && (m_PlayState.m_nCurrentOrder <= songEnd) && (m_PlayState.m_nCurrentOrder > 0))
							{
								// Subtune detection. Subtunes are separated by "---" order items, so if we're in a
								// subtune and there's no restart position, we go to the first order of the subtune
								// (i.e. the first order after the previous "---" item)
								for (OrderIndex ord = (OrderIndex)(m_PlayState.m_nCurrentOrder - 1); ord > 0; ord--)
								{
									if (Order.Current[ord] == Snd_Def.PatternIndex_Invalid)
									{
										// Jump back to first order of this subtune
										restartPosOverride = (OrderIndex)(ord + 1);
										break;
									}
								}
							}

							// If channel resetting is disabled in MPT, we will emulate a pattern break (and we always do it if we're not in MPT)
							{
								m_PlayState.m_Flags.Set(PlayFlags.Song_BreakToRow);
							}

							if ((restartPosOverride == 0) && !m_PlayState.m_Flags.Test(PlayFlags.Song_BreakToRow))
							{
								// rewbs.instroVSTi: stop all VSTi at end of song, if looping
								StopAllVsti();

								m_PlayState.m_nMusicSpeed = Order.Current.GetDefaultSpeed();
								m_PlayState.m_nMusicTempo = Order.Current.GetDefaultTempo();
								m_PlayState.m_nGlobalVolume = (int32)m_nDefaultGlobalVolume;

								for (ChannelIndex i = 0; i < m_PlayState.Chn.size(); i++)
								{
									ModChannel chn = m_PlayState.Chn[i];

//XX									if (chn.dwFlags.Test(ChannelFlags.Chn_Adlib) && (m_Opl != null))
//										m_Opl.NoteCut(i);

									chn.dwFlags.Set(ChannelFlags.Chn_NoteFade | ChannelFlags.Chn_KeyOff);
									chn.nFadeOutVol = 0;

									if (i < GetNumChannels())
									{
										chn.nGlobalVol = ChnSettings[i].nVolume;
										chn.nVolume = ChnSettings[i].nVolume;
										chn.nPan = ChnSettings[i].nPan;
										chn.nPanSwing = chn.nVolSwing = 0;
										chn.nCutSwing = chn.nResSwing = 0;
										chn.nOldVolParam = 0;
										chn.OldOffset = 0;
										chn.nOldHiOffset = 0;
										chn.nPortamentoDest = 0;

										if (chn.nLength == 0)
										{
											chn.dwFlags = ChnSettings[i].dwFlags;
											chn.nLoopStart = 0;
											chn.nLoopEnd = 0;
											chn.pModInstrument = null;
											chn.pModSample = null;
										}
									}
								}
							}

							// Handle Repeat position
							m_PlayState.m_nCurrentOrder = restartPosOverride;
							m_PlayState.m_Flags.Reset(PlayFlags.Song_BreakToRow);

							// If restart pos points to +++, move along
							while ((m_PlayState.m_nCurrentOrder < Order.Current.size()) && (Order.Current[m_PlayState.m_nCurrentOrder] == Snd_Def.PatternIndex_Skip))
								m_PlayState.m_nCurrentOrder++;

							// Check for end of song or bad pattern
							if ((m_PlayState.m_nCurrentOrder >= Order.Current.size()) || !Order.Current.IsValidPat(m_PlayState.m_nCurrentOrder))
							{
								m_VisitedRows.Initialize(true);
								return false;
							}
						}
						else
							m_PlayState.m_nCurrentOrder++;

						if (m_PlayState.m_nCurrentOrder < Order.Current.size())
							m_PlayState.m_nPattern = Order.Current[m_PlayState.m_nCurrentOrder];
						else
							m_PlayState.m_nPattern = Snd_Def.PatternIndex_Invalid;

						if ((m_PlayState.m_nPattern < Patterns.Size()) && !Patterns[m_PlayState.m_nPattern].IsValid())
							m_PlayState.m_nPattern = Snd_Def.PatternIndex_Skip;
					}

					m_PlayState.m_nNextOrder = m_PlayState.m_nCurrentOrder;
				}

				// Weird stuff?
				if (!Patterns.IsValidPat(m_PlayState.m_nPattern))
					return false;

				// Did we jump to an invalid row?
				if (m_PlayState.m_nRow >= Patterns[m_PlayState.m_nPattern].GetNumRows())
					m_PlayState.m_nRow = 0;

				// Has this row been visited before? We might want to stop playback now.
				// But: We will not mark the row as modified if the song is not in loop mode but
				// the pattern loop (editor flag, not to be confused with the pattern loop effect)
				// flag is set - because in that case, the module would stop after the first pattern loop...
				bool overrideLoopCheck = (m_nRepeatCount != -1) && m_PlayState.m_Flags.Test(PlayFlags.Song_PatternLoop);

				if (!overrideLoopCheck && m_VisitedRows.Visit(m_PlayState.m_nCurrentOrder, m_PlayState.m_nRow, m_PlayState.Chn, ignoreRow))
				{
					if (m_nRepeatCount != 0)
					{
						// repeat count == -1 means repeat infinitely
						if (m_nRepeatCount > 0)
							m_nRepeatCount--;

						// Forget all but the current row
						m_VisitedRows.Initialize(true);
						m_VisitedRows.Visit(m_PlayState.m_nCurrentOrder, m_PlayState.m_nRow, m_PlayState.Chn, ignoreRow);
					}
					else
					{
						if (m_SongFlags.Test(SongFlags.PlayAllSongs))
						{
							// When playing all subsongs consecutively, first search for any hidden subsongs...
							if (!m_VisitedRows.GetFirstUnvisitedRow(out m_PlayState.m_nCurrentOrder, out m_PlayState.m_nRow, true))
							{
								// ...and then try the next sequence
								m_PlayState.m_nNextOrder = m_PlayState.m_nCurrentOrder = 0;
								m_PlayState.m_nNextRow = m_PlayState.m_nRow = 0;

								if (Order.GetCurrentSequenceIndex() >= (Order.GetNumSequences() - 1))
								{
									Order.SetSequence(0);
									m_VisitedRows.Initialize(true);

									return false;
								}

								Order.SetSequence((SequenceIndex)(Order.GetCurrentSequenceIndex() + 1));
								m_VisitedRows.Initialize(true);
							}

							// When jumping to the next subsong, stop all playing notes from the previous song...
							ChannelFlags muteFlag = GetChannelMuteFlag();

							for (ChannelIndex i = 0; i < m_PlayState.Chn.size(); i++)
								m_PlayState.Chn[i].Reset(ModChannel.ResetFlags.SetPosFull, this, i, muteFlag);

							StopAllVsti();

							// ...and the global playback information
							m_PlayState.m_nMusicSpeed = Order.Current.GetDefaultSpeed();
							m_PlayState.m_nMusicTempo = Order.Current.GetDefaultTempo();
							m_PlayState.m_nGlobalVolume = (int32)m_nDefaultGlobalVolume;

							m_PlayState.m_nNextOrder = m_PlayState.m_nCurrentOrder;
							m_PlayState.m_nNextRow = m_PlayState.m_nRow;

							if (Order.Current.size() > m_PlayState.m_nCurrentOrder)
								m_PlayState.m_nPattern = Order.Current[m_PlayState.m_nCurrentOrder];

							m_VisitedRows.Visit(m_PlayState.m_nCurrentOrder, m_PlayState.m_nRow, m_PlayState.Chn, ignoreRow);

							if (!Patterns.IsValidPat(m_PlayState.m_nPattern))
								return false;
						}
						else
						{
							m_VisitedRows.Initialize(true);
							return false;
						}
					}
				}

				SetupNextRow(m_PlayState, m_PlayState.m_Flags.Test(PlayFlags.Song_PatternLoop));

				// Reset channel values
				CPointer<ModCommand> mp = Patterns[m_PlayState.m_nPattern].GetpModCommand(m_PlayState.m_nRow, 0);

				foreach (ModChannel chn in m_PlayState.PatternChannels(this))
				{
					ModCommand m = mp[0];

					// First, handle some quirks that happen after the last tick of the previous row...
					if (m_PlayBehaviour[PlayBehaviour.St3PortaAfterArpeggio] && (chn.nCommand == EffectCommand.Arpeggio) && ((m.Command == EffectCommand.PortamentoUp) || (m.Command == EffectCommand.PortamentoDown)))
					{
						// In ST3, a portamento immediately following an arpeggio continues where the arpeggio left off.
						// Test case: PortaAfterArp.s3m
						chn.nPeriod = (int32)GetPeriodFromNote(chn.nArpeggioLastNote, chn.nFineTune, (uint32)chn.nC5Speed);
					}

					if (m_PlayBehaviour[PlayBehaviour.ModOutOfRangeNoteDelay] && !m.IsNote() && chn.RowCommand.IsNote() && (chn.RowCommand.Command == EffectCommand.ModCmdEx) && ((chn.RowCommand.Param & 0xf0) == 0xd0) && ((chn.RowCommand.Param & 0x0f) >= m_PlayState.m_nMusicSpeed))
					{
						// In ProTracker, a note triggered by an out-of-range note delay can be heard on the next row
						// if there is no new note on that row.
						// Test case: NoteDelay-NextRow.mod
						chn.nPeriod = (int32)GetPeriodFromNote(chn.RowCommand.Note, chn.nFineTune, 0);
					}

					if (m_PlayBehaviour[PlayBehaviour.St3TonePortaWithAdlibNote] && !m.IsNote() && chn.dwFlags.Test(ChannelFlags.Chn_Adlib) && (chn.nPortamentoDest != 0) && chn.RowCommand.IsNote() && chn.RowCommand.IsTonePortamento())
					{
						// ST3: Adlib Note + Tone Portamento does not execute the slide, but changes to the target note instantly on the next row (unless there is another note with tone portamento)
						// Test case: TonePortamentoWithAdlibNote.s3m
						chn.nPeriod = chn.nPortamentoDest;
					}

					if (m_PlayBehaviour[PlayBehaviour.ModTempoOnSecondTick] && !m_PlayBehaviour[PlayBehaviour.ModVBlankTiming] && (m_PlayState.m_nMusicSpeed == 1) && (chn.RowCommand.Command == EffectCommand.Tempo))
					{
						// ProTracker sets the tempo after the first tick. This block handles the case of one tick per row.
						// Test case: TempoChange.mod
						m_PlayState.m_nMusicTempo = new Tempo(Math.Max((ModCommandParam)1, chn.RowCommand.Param), 0);
					}

					chn.RightVol = chn.NewRightVol;
					chn.LeftVol = chn.NewLeftVol;
					chn.dwFlags.Reset(ChannelFlags.Chn_Vibrato | ChannelFlags.Chn_Tremolo);

					if (!m_PlayBehaviour[PlayBehaviour.ItVibratoTremoloPanbrello])
						chn.nPanbrelloOffset = 0;

					chn.nCommand = EffectCommand.None;
					chn.m_PlugParamValueStep = 0;
					m.CopyTo(chn.RowCommand);
					mp++;
				}

				// Now that we know which pattern we're on, we can update time signatures (global or pattern-specific)
				m_PlayState.UpdateTimeSignature(this);

				if (ignoreRow)
				{
					m_PlayState.m_nTickCount = m_PlayState.m_nMusicSpeed;
					continue;
				}

				break;
			}

			// Should we process tick0 effects?
			if (m_PlayState.m_nMusicSpeed == 0)
				m_PlayState.m_nMusicSpeed = 1;

			// End of row? stop pattern step (aka "play row")
			if (m_PlayState.m_nTickCount != 0)
			{
				m_PlayState.m_Flags.Reset(PlayFlags.Song_FirstTick);

				if (((GetType_() & (ModType.Xm | ModType.Mt2)) == 0) && ((GetType_() != ModType.Mod) || m_SongFlags.Test(SongFlags.Pt_Mode)) &&	// Fix infinite loop in "GamerMan " by MrGamer, which was made with FT2
					(m_PlayState.m_nTickCount < m_PlayState.TicksOnRow()))
				{
					// Emulate first tick behaviour if Row Delay is set.
					// Test cases: PatternDelaysRetrig.it, PatternDelaysRetrig.s3m, PatternDelaysRetrig.xm, PatternDelaysRetrig.mod
					if ((m_PlayState.m_nTickCount % (m_PlayState.m_nMusicSpeed + m_PlayState.m_nFrameDelay)) == 0)
						m_PlayState.m_Flags.Set(PlayFlags.Song_FirstTick);
				}
			}
			else
			{
				m_PlayState.m_Flags.Set(PlayFlags.Song_FirstTick);
				m_PlayState.m_Flags.Reset(PlayFlags.Song_BreakToRow);
			}

			// Update Effects
			return ProcessEffects();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public (bool first, bool second) NextRow(PlayState playState, bool breakRow)//XX 778
		{
			// When having an EEx effect on the same row as a Dxx jump, the target row is not played in ProTracker.
			// Test case: DelayBreak.mod (based on condom_corruption by Travolta)
			bool ignoreRow = (playState.m_nPatternDelay > 1) && breakRow && (GetType_() == ModType.Mod);

			// Done with the last row of the pattern or jumping somewhere else
			// (could also be a result of pattern loop to row 0, but that doesn't matter here)
			bool patternTransition = (playState.m_nNextRow == 0) || breakRow;

			if (patternTransition && (GetType_() == ModType.S3M))
			{
				// Reset pattern loop start
				// Test case: LoopReset.s3m
				for (ChannelIndex i = 0; i < GetNumChannels(); i++)
					playState.Chn[i].nPatternLoop = 0;
			}

			playState.m_nPatternDelay = 0;
			playState.m_nFrameDelay = 0;
			playState.m_nTickCount = 0;
			playState.m_nRow = playState.m_nNextRow;
			playState.m_nCurrentOrder = playState.m_nNextOrder;

			return (ignoreRow, patternTransition);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void SetupNextRow(PlayState playState, bool patternLoop)//XX 806
		{
			playState.m_nNextRow = playState.m_nRow + 1;

			if (playState.m_nNextRow >= Patterns[playState.m_nPattern].GetNumRows())
			{
				if (!patternLoop)
					playState.m_nNextOrder = (OrderIndex)(playState.m_nCurrentOrder + 1);

				playState.m_nNextRow = 0;

				// FT2 idiosyncrasy: When E60 is used on a pattern row x, the following pattern also starts from row x
				// instead of the beginning of the pattern, unless there was a Bxx or Dxx effect
				if (m_PlayBehaviour[PlayBehaviour.Ft2LoopE60Restart])
				{
					playState.m_nNextRow = playState.m_NextPatStartRow;
					playState.m_NextPatStartRow = 0;
				}
			}
		}
		#endregion

		//XX 826
		#region Channel effect processing
		/********************************************************************/
		/// <summary>
		/// Calculate delta for Vibrato / Tremolo / Panbrello effect
		/// </summary>
		/********************************************************************/
		public c_int GetVibratoDelta(c_int type, c_int position)//XX 831
		{
			// IT compatibility: IT has its own, more precise tables
			if (m_PlayBehaviour[PlayBehaviour.ItVibratoTremoloPanbrello])
			{
				position &= 0xff;

				switch (type & 0x03)
				{
					// Sine
					case 0:
					default:
						return Tables.ItSinusTable[position];

					// Ramp down
					case 1:
						return 64 - ((position + 1) / 2);

					// Square
					case 2:
						return position < 128 ? 64 : 0;

					// Random
					case 3:
						return Random.Random_<c_int, uint16>(AccessPrng(), 7) - 0x40;
				}
			}
			else if ((GetType_() & (ModType.Digi | ModType.Dbm)) != 0)
			{
				// Other waveforms are not supported
				return dbmSinus[((uint32)position / 2U) & 0x1f];
			}
			else
			{
				position &= 0x3f;

				switch (type & 0x03)
				{
					// Sine
					case 0:
					default:
						return Tables.ModSinusTable[position];

					// Ramp down
					case 1:
						return (position < 32 ? 0 : 255) - (position * 4);

					// Square
					case 2:
						return position < 32 ? 127 : -127;

					// Random
					case 3:
						return Tables.ModRandomTable[position];
				}
			}
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public void ProcessVolumeSwing(ModChannel chn, ref c_int vol)//XX 877
		{
			if (m_PlayBehaviour[PlayBehaviour.ItSwingBehaviour])
			{
				vol += chn.nVolSwing;
				OpenMpt.Limit(ref vol, 0, 64);
			}
			else if (m_PlayBehaviour[PlayBehaviour.MptOldSwingBehaviour])
			{
				vol += chn.nVolSwing;
				OpenMpt.Limit(ref vol, 0, 256);
			}
			else
			{
				chn.nVolume += chn.nVolSwing;
				OpenMpt.Limit(ref chn.nVolume, 0, 256);

				vol = chn.nVolume;
				chn.nVolSwing = 0;
			}
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public void ProcessPanningSwing(ModChannel chn)//XX 897
		{
			if (m_PlayBehaviour[PlayBehaviour.ItSwingBehaviour] || m_PlayBehaviour[PlayBehaviour.MptOldSwingBehaviour])
			{
				chn.nRealPan = chn.nPan + chn.nPanSwing;
				OpenMpt.Limit(ref chn.nRealPan, 0, 256);
			}
			else
			{
				chn.nPan += chn.nPanSwing;
				OpenMpt.Limit(ref chn.nPan, 0, 256);

				chn.nPanSwing = 0;
				chn.nRealPan = chn.nPan;
			}
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public void ProcessTremolo(ModChannel chn, ref c_int vol)//XX 913
		{
			if (chn.dwFlags.Test(ChannelFlags.Chn_Tremolo))
			{
				if (m_SongFlags.Test(SongFlags.Pt_Mode) && m_PlayState.m_Flags.Test(PlayFlags.Song_FirstTick))
				{
					// ProTracker doesn't apply tremolo nor advance on the first tick.
					// Test case: VibratoReset.mod
					return;
				}

				// IT compatibility: Why would you not want to execute tremolo at volume 0?
				if ((vol > 0) || m_PlayBehaviour[PlayBehaviour.ItVibratoTremoloPanbrello])
				{
					// IT compatibility: We don't need a different attenuation here because of the different tables we're going to use
					uint8 attenuation = (uint8)(((GetType_() & (ModType.Xm | ModType.Mod)) != 0) || m_PlayBehaviour[PlayBehaviour.ItVibratoTremoloPanbrello] ? 5 : 6);

					c_int delta = GetVibratoDelta(chn.nTremoloType, chn.nTremoloPos);

					if (((chn.nTremoloType & 0x03) == 1) && m_PlayBehaviour[PlayBehaviour.Ft2ModTremoloRampWaveform])
					{
						// FT2 compatibility: Tremolo ramp down / triangle implementation is weird and affected by vibrato position (copy-paste bug)
						// Test case: TremoloWaveforms.xm, TremoloVibrato.xm
						uint8 ramp = (uint8)((chn.nTremoloPos * 4U) & 0x7f);

						// Volume-column vibrato gets executed first in FT2, so we may need to advance the vibrato position first
						uint32 vibPos = chn.nVibratoPos;

						if (!m_PlayState.m_Flags.Test(PlayFlags.Song_FirstTick) && chn.dwFlags.Test(ChannelFlags.Chn_Vibrato))
							vibPos += chn.nVibratoSpeed;

						if ((vibPos & 0x3f) >= 32)
							ramp ^= 0x7f;

						if ((chn.nTremoloPos & 0x3f) >= 32)
							delta = -ramp;
						else
							delta = ramp;
					}

					if (GetType_() != ModType.Dmf)
						vol += (delta * chn.nTremoloDepth) / (1 << attenuation);
					else
					{
						// Tremolo in DMF always attenuates by a percentage of the current note volume
						vol -= (vol * chn.nTremoloDepth * (64 - delta)) / (128 * 64);
					}
				}

				if (!m_PlayState.m_Flags.Test(PlayFlags.Song_FirstTick) || (((GetType_() & (ModType.It | ModType.Mpt)) != 0) && !m_SongFlags.Test(SongFlags.ItOldEffects)))
				{
					// IT compatibility: IT has its own, more precise tables
					if (m_PlayBehaviour[PlayBehaviour.ItVibratoTremoloPanbrello])
						chn.nTremoloPos += (uint8)(4U * chn.nTremoloSpeed);
					else
						chn.nTremoloPos += chn.nTremoloSpeed;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public void ProcessTremor(ChannelIndex nChn, ref c_int vol)//XX 968
		{
			ModChannel chn = m_PlayState.Chn[nChn];

			if (m_PlayBehaviour[PlayBehaviour.Ft2Tremor])
			{
				// FT2 Compatibility: Weird XM tremor.
				// Test case: Tremor.xm
				if ((chn.nTremorCount & 0x80) != 0)
				{
					if (!m_PlayState.m_Flags.Test(PlayFlags.Song_FirstTick) && (chn.nCommand == EffectCommand.Tremor))
					{
						chn.nTremorCount &= unchecked((uint8)(~0x20));

						if (chn.nTremorCount == 0x80)
						{
							// Reached end of off-time
							chn.nTremorCount = (uint8)((chn.nTremorParam >> 4) | 0xc0);
						}
						else if (chn.nTremorCount == 0xc0)
						{
							// Reached end of on-time
							chn.nTremorCount = (uint8)((chn.nTremorParam & 0x0f) | 0x80);
						}
						else
							chn.nTremorCount--;

						chn.dwFlags.Set(ChannelFlags.Chn_FastVolRamp);
					}

					if ((chn.nTremorCount & 0xe0) == 0x80)
						vol = 0;
				}
			}
			else if (chn.nCommand == EffectCommand.Tremor)
			{
				// IT compatibility 12. / 13.: Tremor
				if (m_PlayBehaviour[PlayBehaviour.ItTremor])
				{
					if (((chn.nTremorCount & 0x80) != 0) && (chn.nLength != 0))
					{
						if (chn.nTremorCount == 0x80)
							chn.nTremorCount = (uint8)((chn.nTremorParam >> 4) | 0xc0);
						else if (chn.nTremorCount == 0xc0)
							chn.nTremorCount = (uint8)((chn.nTremorParam & 0x0f) | 0x80);
						else
							chn.nTremorCount--;
					}

					if ((chn.nTremorCount & 0xc0) == 0x80)
						vol = 0;
				}
				else
				{
					uint8 onTime = (uint8)(chn.nTremorParam >> 4);
					uint8 n = (uint8)(onTime + (chn.nTremorParam & 0x0f));	// Total tremor cycle time (On + Off)

					if (((GetType_() & (ModType.It | ModType.Mpt)) == 0) || m_SongFlags.Test(SongFlags.ItOldEffects))
					{
						n += 2;
						onTime++;
					}

					uint8 tremCount = chn.nTremorCount;

					if ((GetType_() & ModType.Xm) == 0)
					{
						if (tremCount >= n)
							tremCount = 0;

						if (tremCount >= onTime)
							vol = 0;

						chn.nTremorCount = (uint8)(tremCount + 1);
					}
					else
					{
						if (m_PlayState.m_Flags.Test(PlayFlags.Song_FirstTick))
						{
							// tremcount is only 0 on the first tremor tick after triggering a note
							if (tremCount > 0)
								tremCount--;
						}
						else
							chn.nTremorCount = (uint8)(tremCount + 1);

						if ((tremCount % n) >= onTime)
							vol = 0;
					}
				}

				chn.dwFlags.Set(ChannelFlags.Chn_FastVolRamp);
			}
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public bool IsEnvelopeProcessed(ModChannel chn, EnvelopeType env)//XX 1075
		{
			if (chn.pModInstrument == null)
				return false;

			InstrumentEnvelope insEnv = chn.pModInstrument.GetEnvelope(env);

			// IT Compatibility: S77/S79/S7B do not disable the envelope, they just pause the counter
			// Test cases: s77.it, EnvLoops.xm, PanSustainRelease.xm
			bool playIfPaused = m_PlayBehaviour[PlayBehaviour.ItEnvelopePositionHandling] || m_PlayBehaviour[PlayBehaviour.Ft2PanSustainRelease];

			return (chn.GetEnvelope(env).Flags.Test(EnvelopeFlags.Enabled) || (insEnv.dwFlags.Test(EnvelopeFlags.Enabled) && playIfPaused)) && !insEnv.empty();
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public void ProcessVolumeEnvelope(ModChannel chn, ref c_int vol)//XX 1091
		{
			if (IsEnvelopeProcessed(chn, EnvelopeType.Volume))
			{
				ModInstrument pIns = chn.pModInstrument;

				if (m_PlayBehaviour[PlayBehaviour.ItEnvelopePositionHandling] && (chn.VolEnv.nEnvPosition == 0))
				{
					// If the envelope is disabled at the very same moment as it is triggered, we do not process anything
					return;
				}

				c_int envPos = (c_int)(chn.VolEnv.nEnvPosition - (m_PlayBehaviour[PlayBehaviour.ItEnvelopePositionHandling] ? 1 : 0));

				// Get values in [0, 256]
				c_int envVal = pIns.VolEnv.GetValueFromPosition(envPos, 256);

				// If we are in the release portion of the envelope,
				// rescale envelope factor so that it is proportional to the release point
				// and release envelope beginning
				if ((pIns.VolEnv.nReleaseNode != Snd_Def.Env_Release_Node_Unset) && (chn.VolEnv.nEnvValueAtReleaseJump != Snd_Def.Not_Yet_Released))
				{
					c_int envValueAtReleaseJump = chn.VolEnv.nEnvValueAtReleaseJump;
					c_int envValueAtReleaseNode = pIns.VolEnv[pIns.VolEnv.nReleaseNode].Value * 4;

					// If we have just hit the release node, force the current env value
					// to be that of the release node. This works around the case where
					// we have another node at the same position as the release node
					if (envPos == pIns.VolEnv[pIns.VolEnv.nReleaseNode].Tick)
						envVal = envValueAtReleaseNode;

					if (m_PlayBehaviour[PlayBehaviour.LegacyReleaseNode])
					{
						// Old, hard to grasp release node behaviour (additive)
						c_int relativeVolumeChange = (envVal - envValueAtReleaseNode) * 2;
						envVal = envValueAtReleaseJump + relativeVolumeChange;
					}
					else
					{
						// New behaviour, truly relative to release node
						if (envValueAtReleaseNode > 0)
							envVal = envValueAtReleaseJump * envVal / envValueAtReleaseNode;
						else
							envVal = 0;
					}
				}

				vol = (vol * OpenMpt.Clamp(envVal, 0, 512)) / 256;
			}
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public void ProcessPanningEnvelope(ModChannel chn)//XX 1141
		{
			if (IsEnvelopeProcessed(chn, EnvelopeType.Panning))
			{
				ModInstrument pIns = chn.pModInstrument;

				if (m_PlayBehaviour[PlayBehaviour.ItEnvelopePositionHandling] && (chn.PanEnv.nEnvPosition == 0))
				{
					// If the envelope is disabled at the very same moment as it is triggered, we do not process anything
					return;
				}

				c_int envPos = (c_int)(chn.PanEnv.nEnvPosition - (m_PlayBehaviour[PlayBehaviour.ItEnvelopePositionHandling] ? 1 : 0));

				// Get values in [-32, 32]
				c_int envVal = pIns.PanEnv.GetValueFromPosition(envPos, 64) - 32;

				c_int pan = chn.nRealPan;

				if (pan >= 128)
					pan += (envVal * (256 - pan)) / 32;
				else
					pan += (envVal * pan) / 32;

				chn.nRealPan = OpenMpt.Clamp(pan, 0, 256);
			}
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public c_int ProcessPitchFilterEnvelope(ModChannel chn, ref int32 period)//XX 1170
		{
			if (IsEnvelopeProcessed(chn, EnvelopeType.Pitch))
			{
				ModInstrument pIns = chn.pModInstrument;

				if (m_PlayBehaviour[PlayBehaviour.ItEnvelopePositionHandling] && (chn.PitchEnv.nEnvPosition == 0))
				{
					// If the envelope is disabled at the very same moment as it is triggered, we do not process anything.
					// However, Impulse Tracker still applies the filter settings as if the envelope was at its midway.
					// Test case: S7B_StillAppliesFilter.it
					if (m_PlayBehaviour[PlayBehaviour.ItStoppedFilterEnvAtStart] && chn.PitchEnv.Flags.Test(EnvelopeFlags.Filter))
						return SetupChannelFilter(chn, !chn.dwFlags.Test(ChannelFlags.Chn_Filter), 0);
					else
						return -1;
				}

				c_int envPos = (c_int)(chn.PitchEnv.nEnvPosition - (m_PlayBehaviour[PlayBehaviour.ItEnvelopePositionHandling] ? 1 : 0));

				// Get values in [-256, 256]
				//
				// TODO: AMS2 envelopes behave differently when linear slides are off - emulate with 15 * (-128...127) >> 6
				// Copy over vibrato behaviour for that?
				int32 range = GetType_() == ModType.Ams ? uint8.MaxValue : Snd_Def.Envelope_Max;
				int32 amp;

				switch (GetType_())
				{
					case ModType.Ams:
					{
						amp = 64;
						break;
					}

					case ModType.Mdl:
					{
						amp = 192;
						break;
					}

					default:
					{
						amp = 512;
						break;
					}
				}

				c_int envVal = pIns.PitchEnv.GetValueFromPosition(envPos, amp, range) - (amp / 2);

				if (chn.PitchEnv.Flags.Test(EnvelopeFlags.Filter))
				{
					// Filter Envelope: controls cutoff frequency
					return SetupChannelFilter(chn, !chn.dwFlags.Test(ChannelFlags.Chn_Filter), envVal);
				}
				else
				{
					// Pitch Envelope
					if (chn.HasCustomTuning())
					{
						if (chn.nFineTune != envVal)
						{
							chn.nFineTune = int16.CreateSaturating(envVal);
							chn.m_CalculateFreq = true;

							// Preliminary tests indicated that this behavior
							// is very close to original(with 12TET) when finestep count
							// is 15
						}
					}
					else	// Original behaviour
					{
						bool useFreq = PeriodsAreFrequencies();
						uint32[] upTable = useFreq ? Tables.LinearSlideUpTable : Tables.LinearSlideDownTable;
						uint32[] downTable = useFreq ? Tables.LinearSlideDownTable : Tables.LinearSlideUpTable;

						c_int l = envVal;

						if (l < 0)
						{
							l = -l;
							OpenMpt.LimitMax(ref l, 255);
							period = Util.MulDiv(period, (int32)downTable[l], 65536);
						}
						else
						{
							OpenMpt.LimitMax(ref l, 255);
							period = Util.MulDiv(period, (int32)upTable[l], 65536);
						}
					}
				}
			}

			return -1;
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public void IncrementEnvelopePosition(ModChannel chn, EnvelopeType envType)//XX 1247
		{
			ModChannel.EnvInfo chnEnv = chn.GetEnvelope(envType);

			if ((chn.pModInstrument == null) || !chnEnv.Flags.Test(EnvelopeFlags.Enabled))
				return;

			// Increase position
			uint32 position = (uint32)(chnEnv.nEnvPosition + (m_PlayBehaviour[PlayBehaviour.ItEnvelopePositionHandling] ? 0 : 1));

			InstrumentEnvelope insEnv = chn.pModInstrument.GetEnvelope(envType);

			if (insEnv.empty())
				return;

			bool endReached = false;

			if (!m_PlayBehaviour[PlayBehaviour.ItEnvelopePositionHandling])
			{
				// FT2-style envelope processing
				if (insEnv.dwFlags.Test(EnvelopeFlags.Loop))
				{
					// Normal loop active
					uint32 end = insEnv[insEnv.nLoopEnd].Tick;

					if ((GetType_() & (ModType.Xm | ModType.Mt2)) == 0)
						end++;

					// FT2 compatibility: If the sustain point is at the loop end and the sustain loop has been released, don't loop anymore.
					// Test case: EnvLoops.xm
					bool escapeLoop = (insEnv.nLoopEnd == insEnv.nSustainEnd) && insEnv.dwFlags.Test(EnvelopeFlags.Sustain) && chn.dwFlags.Test(ChannelFlags.Chn_KeyOff) && m_PlayBehaviour[PlayBehaviour.Ft2EnvelopeEscape];

					if ((position == end) && !escapeLoop)
						position = insEnv[insEnv.nLoopStart].Tick;
				}

				if (insEnv.dwFlags.Test(EnvelopeFlags.Sustain) && !chn.dwFlags.Test(ChannelFlags.Chn_KeyOff))
				{
					// Envelope sustained
					if (position == (insEnv[insEnv.nSustainEnd].Tick + 1U))
					{
						position = insEnv[insEnv.nSustainStart].Tick;

						// FT2 compatibility: If the panning envelope reaches its sustain point before key-off, it stays there forever.
						// Test case: PanSustainRelease.xm
						if (m_PlayBehaviour[PlayBehaviour.Ft2PanSustainRelease] && (envType == EnvelopeType.Panning) && !chn.dwFlags.Test(ChannelFlags.Chn_KeyOff))
							chnEnv.Flags.Reset(EnvelopeFlags.Enabled);
					}
				}
				else
				{
					// Limit to last envelope point
					if (position > insEnv.back().Tick)
					{
						// Env of envelope
						position = insEnv.back().Tick;
						endReached = true;
					}
				}
			}
			else
			{
				// IT envelope processing.
				// Test case: EnvLoops.it
				uint32 start, end;

				// IT compatiblity: OpenMPT processes the key-off flag earlier than IT. Grab the flag from the previous tick instead.
				// Test case: EnvOffLength.it
				if (insEnv.dwFlags.Test(EnvelopeFlags.Sustain) && !chn.dwOldFlags.Test(ChannelFlags.Chn_KeyOff) && ((chnEnv.nEnvValueAtReleaseJump == Snd_Def.Not_Yet_Released) || m_PlayBehaviour[PlayBehaviour.ReleaseNodePastSustainBug]))
				{
					// Envelope sustained
					start = insEnv[insEnv.nSustainStart].Tick;
					end = insEnv[insEnv.nSustainEnd].Tick + 1U;
				}
				else if (insEnv.dwFlags.Test(EnvelopeFlags.Loop))
				{
					// Normal loop active
					start = insEnv[insEnv.nLoopStart].Tick;
					end = insEnv[insEnv.nLoopEnd].Tick + 1U;
				}
				else
				{
					// Limit to last envelope point
					start = end = insEnv.back().Tick;

					if (position > end)
					{
						// Env of envelope
						endReached = true;
					}
				}

				if (position >= end)
					position = start;
			}

			if ((envType == EnvelopeType.Volume) && endReached)
			{
				// Special handling for volume envelopes at end of envelope
				if (((GetType_() & (ModType.It | ModType.Mpt)) != 0) || (chn.dwFlags.Test(ChannelFlags.Chn_KeyOff) && (GetType_() != ModType.Mdl)))
					chn.dwFlags.Set(ChannelFlags.Chn_NoteFade);

				if ((insEnv.back().Value == 0) && ((chn.nMasterChn > 0) || ((GetType_() & (ModType.It | ModType.Mpt)) != 0)))
				{
					// Stop channel if the last envelope node is silent anyway
					chn.dwFlags.Set(ChannelFlags.Chn_NoteFade);
					chn.nFadeOutVol = 0;
					chn.nRealVolume = 0;
					chn.nCalcVolume = 0;
				}
			}

			chnEnv.nEnvPosition = (uint32)(position + (m_PlayBehaviour[PlayBehaviour.ItEnvelopePositionHandling] ? 1 : 0));
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public void IncrementEnvelopePositions(ModChannel chn)//XX 1367
		{
			if (chn.IsFirstTick && (GetType_() == ModType.Med))
				return;

			IncrementEnvelopePosition(chn, EnvelopeType.Volume);
			IncrementEnvelopePosition(chn, EnvelopeType.Panning);
			IncrementEnvelopePosition(chn, EnvelopeType.Pitch);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public void ProcessInstrumentFade(ModChannel chn, ref c_int vol)//XX 1377
		{
			// FadeOut volume
			if (chn.dwFlags.Test(ChannelFlags.Chn_NoteFade) && (chn.pModInstrument != null))
			{
				ModInstrument pIns = chn.pModInstrument;

				uint32 fadeOut = pIns.nFadeOut;

				if (fadeOut != 0)
				{
					chn.nFadeOutVol -= (int32)(fadeOut * 2);

					if (chn.nFadeOutVol <= 0)
						chn.nFadeOutVol = 0;

					vol = (vol * chn.nFadeOutVol) / 65536;
				}
				else if (chn.nFadeOutVol == 0)
					vol = 0;
			}
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public void ProcessPitchPanSeparation(ref int32 pan, c_int note, ModInstrument instr)//XX 1399
		{
			if ((instr.nPps == 0) || (note == ModCommand.Note_None))
				return;

			// with PPS = 16 / PPC = C-5, E-6 will pan hard right (and D#6 will not)
			int32 delta = (note - instr.nPpc - ModCommand.Note_Min) * instr.nPps / 2;
			pan = OpenMpt.Clamp(pan + delta, 0, 256);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public void ProcessPanbrello(ModChannel chn)//XX 1409
		{
			c_int pDelta = chn.nPanbrelloOffset;

			if (chn.RowCommand.Command == EffectCommand.Panbrello)
			{
				uint32 panPos;

				// IT compatibility: IT has its own, more precise tables
				if (m_PlayBehaviour[PlayBehaviour.ItVibratoTremoloPanbrello])
					panPos = chn.nPanbrelloPos;
				else
					panPos = (uint32)((chn.nPanbrelloPos + 0x10) >> 2);

				pDelta = GetVibratoDelta(chn.nPanbrelloType, (c_int)panPos);

				// IT compatibility: Sample-and-hold style random panbrello (tremolo and vibrato don't use this mechanism in IT)
				// Test case: RandomWaveform.it
				if (m_PlayBehaviour[PlayBehaviour.ItSampleAndHoldPanbrello] && (chn.nPanbrelloType == 3))
				{
					if ((chn.nPanbrelloPos == 0) || (chn.nPanbrelloPos >= chn.nPanbrelloSpeed))
					{
						chn.nPanbrelloPos = 0;
						chn.nPanbrelloRandomMemory = (int8)pDelta;
					}

					chn.nPanbrelloPos++;
					pDelta = chn.nPanbrelloRandomMemory;
				}
				else
					chn.nPanbrelloPos += chn.nPanbrelloSpeed;

				// IT compatibility: Panbrello effect is active until next note or panning command.
				// Test case: PanbrelloHold.it
				if (m_PlayBehaviour[PlayBehaviour.ItPanbrelloHold])
					chn.nPanbrelloOffset = (int8)pDelta;
			}

			if (pDelta != 0)
			{
				pDelta = ((pDelta * chn.nPanbrelloDepth) + 2) / 8;
				pDelta += chn.nRealPan;
				chn.nRealPan = OpenMpt.Clamp(pDelta, 0, 256);
			}
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public void ProcessArpeggio(ChannelIndex nChn, ref int32 period, ref TuningNoteIndexType arpeggioSteps)//XX 1454
		{
			ModChannel chn = m_PlayState.Chn[nChn];

			if (chn.nCommand == EffectCommand.Arpeggio)
			{
				if (chn.HasCustomTuning())
				{
					switch (m_PlayState.m_nTickCount % 3)
					{
						case 0:
						{
							arpeggioSteps = 0;
							break;
						}

						case 1:
						{
							arpeggioSteps = (TuningNoteIndexType)(chn.nArpeggio >> 4);
							break;
						}

						case 2:
						{
							arpeggioSteps = (TuningNoteIndexType)(chn.nArpeggio & 0x0f);
							break;
						}
					}

					chn.m_CalculateFreq = true;
					chn.m_RecalculateFreqOnFirstTick = true;
				}
				else
				{
					if ((GetType_() == ModType.Mt2) && m_PlayState.m_Flags.Test(PlayFlags.Song_FirstTick))
					{
						// MT2 resets any previous portamento when an arpeggio occurs
						chn.nPeriod = period = (int32)GetPeriodFromNote(chn.nNote, chn.nFineTune, (uint32)chn.nC5Speed);
					}

					if (m_PlayBehaviour[PlayBehaviour.ItArpeggio])
					{
						// IT playback compatibility 01 & 02
						//
						// Pattern delay restarts tick counting.
						// Test case: JxxTicks.it
						uint32 tick = m_PlayState.m_nTickCount % (m_PlayState.m_nMusicSpeed + m_PlayState.m_nFrameDelay);

						if (chn.nArpeggio != 0)
						{
							uint32 arpRatio = 65536;

							switch (tick % 3)
							{
								case 1:
								{
									arpRatio = Tables.LinearSlideUpTable[(chn.nArpeggio >> 4) * 16];
									break;
								}

								case 2:
								{
									arpRatio = Tables.LinearSlideUpTable[(chn.nArpeggio & 0x0f) * 16];
									break;
								}
							}

							if (PeriodsAreFrequencies())
								period = Util.MulDivR(period, (int32)arpRatio, 65536);
							else
								period = Util.MulDivR(period, 65536, (int32)arpRatio);
						}
					}
					else if (m_PlayBehaviour[PlayBehaviour.Ft2Arpeggio])
					{
						// FastTracker 2: Swedish tracker logic (TM) arpeggio
						if (!m_PlayState.m_Flags.Test(PlayFlags.Song_FirstTick))
						{
							// Arpeggio is added on top of current note, but cannot do it the IT way because of
							// the behaviour in ArpeggioClamp.xm.
							// Test case: ArpSlide.xm
							uint32 note = 0;

							// The fact that arpeggio behaves in a totally fucked up way at 16 ticks/row or more is that the arpeggio offset LUT only has 16 entries in FT2.
							// At more than 16 ticks/row, FT2 reads into the vibrato table, which is placed right after the arpeggio table.
							// Test case: Arpeggio.xm
							c_int arpPos = (c_int)(m_PlayState.m_nMusicSpeed - (m_PlayState.m_nTickCount % m_PlayState.m_nMusicSpeed));

							if (arpPos > 16)
								arpPos = 2;
							else if (arpPos == 16)
								arpPos = 0;
							else
								arpPos %= 3;

							switch (arpPos)
							{
								case 1:
								{
									note = (uint32)(chn.nArpeggio >> 4);
									break;
								}

								case 2:
								{
									note = (uint32)(chn.nArpeggio & 0x0f);
									break;
								}
							}

							if (arpPos != 0)
							{
								// Arpeggio is added on top of current note, but cannot do it the IT way because of
								// the behaviour in ArpeggioClamp.xm.
								// Test case: ArpSlide.xm
								note += GetNoteFromPeriod((uint32)period, chn.nFineTune, (uint32)chn.nC5Speed);

								period = (int32)GetPeriodFromNote(note, chn.nFineTune, (uint32)chn.nC5Speed);

								// FT2 compatibility: FT2 has a different note limit for Arpeggio.
								// Test case: ArpeggioClamp.xm
								if (note >= (108 + ModCommand.Note_Min))
									period = (int32)Math.Max((uint32)period, GetPeriodFromNote(108 + ModCommand.Note_Min, 0, (uint32)chn.nC5Speed));
							}
						}
					}
					else	// Other trackers
					{
						uint32 tick = m_PlayState.m_nTickCount;

						// TODO other likely formats for MOD case: MED, OKT, etc
						uint8 note = (GetType_() != ModType.Mod) ? chn.nNote : (uint8)(GetNoteFromPeriod((uint32)period, chn.nFineTune, (uint32)chn.nC5Speed));

						if ((GetType_() & (ModType.Dbm | ModType.Digi)) != 0)
							tick += 2;

						// SFX uses a 0-1-2-0-2-1 pattern (fixed at 6 ticks per row)
						if ((GetType_() == ModType.Sfx) && (tick > 3))
							tick ^= 3;

						switch (tick % 3)
						{
							case 1:
							{
								note += (uint8)(chn.nArpeggio >> 4);
								break;
							}

							case 2:
							{
								note += (uint8)(chn.nArpeggio & 0x0f);
								break;
							}
						}

						if ((note != chn.nNote) || ((GetType_() & (ModType.Dbm | ModType.Digi | ModType.Stm)) != 0) || m_PlayBehaviour[PlayBehaviour.St3PortaAfterArpeggio])
						{
							if (m_SongFlags.Test(SongFlags.Pt_Mode))
							{
								// Weird arpeggio wrap-around in ProTracker.
								// Test case: ArpWraparound.mod, and the snare sound in "Jim is dead" by doh
								if (note == (ModCommand.Note_MiddleC + 24))
								{
									period = 65536;		// Period 0 is treated as period 65536 by the Paula chip
									return;
								}
								else if (note > (ModCommand.Note_MiddleC + 24))
									note -= 37;
							}

							period = (int32)GetPeriodFromNote(note, chn.nFineTune, (uint32)chn.nC5Speed);

							if ((GetType_() & (ModType.Dbm | ModType.Digi | ModType.Psm | ModType.Stm | ModType.Okt | ModType.Sfx)) != 0)
							{
								// The arpeggio note offset remains effective after the end of the current row in ScreamTracker 2.
								// This fixes the flute lead in MORPH.STM by Skaven, pattern 27.
								// Note that ScreamTracker 2.24 handles arpeggio slightly differently: It only considers the lower
								// nibble, and switches to that note halfway through the row
								chn.nPeriod = period;
							}
							else if (m_PlayBehaviour[PlayBehaviour.St3PortaAfterArpeggio])
								chn.nArpeggioLastNote = note;
						}
					}
				}
			}
			else if (chn.RowCommand.Command == EffectCommand.Hmn_Mega_Arp)
			{
				uint8 note = (uint8)(GetNoteFromPeriod((uint32)period, chn.nFineTune, (uint32)chn.nC5Speed));
				note += Tables.HisMastersNoiseMegaArp[chn.RowCommand.Param & 0x0f][chn.nArpeggio & 0x0f];
				chn.nArpeggio++;
				period = (int32)GetPeriodFromNote(note, chn.nFineTune, (uint32)chn.nC5Speed);
			}
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public void ProcessVibrato(ChannelIndex nChn, ref int32 period, ref TuningRatioType vibratoFactor)//XX 1655
		{
			ModChannel chn = m_PlayState.Chn[nChn];

			if (chn.dwFlags.Test(ChannelFlags.Chn_Vibrato))
			{
				bool advancePosition = !m_PlayState.m_Flags.Test(PlayFlags.Song_FirstTick) || (((GetType_() & (ModType.It | ModType.Mpt | ModType.Med)) != 0) && !m_SongFlags.Test(SongFlags.ItOldEffects));

				if (GetType_() == ModType._669)
				{
					if ((chn.nVibratoPos % 2U) != 0)
						period += chn.nVibratoDepth * 167;	// Already multiplied by 4, and it seems like the real factor here is 669... how original =)

					chn.nVibratoPos++;
					return;
				}

				// IT compatibility: IT has its own, more precise tables and pre-increments the vibrato position
				if (advancePosition && m_PlayBehaviour[PlayBehaviour.ItVibratoTremoloPanbrello])
					chn.nVibratoPos += (uint8)(4U * chn.nVibratoSpeed);

				c_int vDelta = GetVibratoDelta(chn.nVibratoType, chn.nVibratoPos);

				if (chn.HasCustomTuning())
				{
					// Hack implementation: Scaling vibratofactor to [0.95; 1.05]
					// using figure from above tables and vibratodepth parameter
					vibratoFactor += 0.05f * (vDelta * chn.nVibratoDepth) / (128.0f * 60.0f);

					chn.m_CalculateFreq = true;
					chn.m_RecalculateFreqOnFirstTick = false;

					if ((m_PlayState.m_nTickCount + 1) == m_PlayState.m_nMusicSpeed)
						chn.m_RecalculateFreqOnFirstTick = true;
				}
				else
				{
					// Original behaviour
					if ((m_SongFlags.Test(SongFlags.Pt_Mode) || ((GetType_() & (ModType.Digi | ModType.Dbm)) != 0)) && m_PlayState.m_Flags.Test(PlayFlags.Song_FirstTick))
					{
						// ProTracker doesn't apply vibrato nor advance on the first tick.
						// Test case: VibratoReset.mod
						return;
					}
					else if (((GetType_() & (ModType.Xm | ModType.Mod)) != 0) && ((chn.nVibratoType & 0x03) == 1))
					{
						// FT2 compatibility: Vibrato ramp down table is upside down.
						// Test case: VibratoWaveforms.xm
						vDelta = -vDelta;
					}

					uint32 vDepth;

					// IT compatibility: correct vibrato depth
					if (m_PlayBehaviour[PlayBehaviour.ItVibratoTremoloPanbrello])
					{
						// Yes, vibrato goes backwards with old effects enabled!
						if (m_SongFlags.Test(SongFlags.ItOldEffects))
						{
							// Test case: vibrato-oldfx.it
							vDepth = 5;
						}
						else
						{
							// Test case: vibrato.it
							vDepth = 6;
							vDelta = -vDelta;
						}
					}
					else
					{
						if (m_SongFlags.Test(SongFlags.S3MOldVibrato))
							vDepth = 5;
						else if (GetType_() == ModType.Dtm)
							vDepth = 8;
						else if ((GetType_() & (ModType.Dbm | ModType.Mtm)) != 0)
							vDepth = 7;
						else if (((GetType_() & (ModType.It | ModType.Mpt)) != 0) && !m_SongFlags.Test(SongFlags.ItOldEffects))
							vDepth = 7;
						else
							vDepth = 6;

						// ST3 compatibility: Do not distinguish between vibrato types in effect memory
						// Test case: VibratoTypeChange.s3m
						if (m_PlayBehaviour[PlayBehaviour.St3VibratoMemory] && (chn.RowCommand.Command == EffectCommand.FineVibrato))
							vDepth += 2;
					}

					vDelta = (-vDelta * chn.nVibratoDepth) / (1 << (c_int)vDepth);

					DoFreqSlide(chn, ref period, vDelta);

					// Process MIDI vibrato for plugins:
				}

				// Advance vibrato position - IT updates on every tick, unless "old effects" are enabled (in this case it only updates on non-first ticks like other trackers)
				// IT compatibility: IT has its own, more precise tables and pre-increments the vibrato position
				if (advancePosition && !m_PlayBehaviour[PlayBehaviour.ItVibratoTremoloPanbrello])
					chn.nVibratoPos += chn.nVibratoSpeed;
			}
			else if (chn.dwOldFlags.Test(ChannelFlags.Chn_Vibrato))
			{
				// Stop MIDI vibrato for plugins:
			}
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public void ProcessSampleAutoVibrato(ModChannel chn, ref int32 period, ref TuningRatioType vibratoFactor, ref c_int nPeriodFrac)//XX 1777
		{
			// Sample Auto-Vibrato
			if ((chn.pModSample != null) && (chn.pModSample.nVibDepth != 0))
			{
				ModSample pSmp = chn.pModSample;
				bool hasTuning = chn.HasCustomTuning();

				// In IT compatible mode, we use always frequencies, otherwise we use periods, which are upside down.
				// In this context, the "up" tables refer to the tables that increase frequency, and the down tables are the ones that decrease frequency
				bool useFreq = PeriodsAreFrequencies();
				uint32[] upTable = useFreq ? Tables.LinearSlideUpTable : Tables.LinearSlideDownTable;
				uint32[] downTable = useFreq ? Tables.LinearSlideDownTable : Tables.LinearSlideUpTable;
				uint32[] fineUpTable = useFreq ? Tables.FineLinearSlideUpTable : Tables.FineLinearSlideDownTable;
				uint32[] fineDownTable = useFreq ? Tables.FineLinearSlideDownTable : Tables.FineLinearSlideUpTable;

				// IT compatibility: Autovibrato is so much different in IT that I just put this in a separate code block, to get rid of a dozen IsCompatibilityMode() calls
				if (m_PlayBehaviour[PlayBehaviour.ItVibratoTremoloPanbrello] && !hasTuning && (GetType_() != ModType.Mt2))
				{
					if (pSmp.nVibRate == 0)
						return;

					// Schism's autovibrato code
					//
					// X86 Assembler from ITTECH.TXT:
					// 1) Mov AX, [SomeVariableNameRelatingToVibrato]
					// 2) Add AL, Rate
					// 3) AdC AH, 0
					// 4) AH contains the depth of the vibrato as a fine-linear slide.
					// 5) Mov [SomeVariableNameRelatingToVibrato], AX  ; For the next cycle
					c_int vibPos = chn.nAutoVibPos & 0xff;
					c_int aDepth = chn.nAutoVibDepth;	// (1)
					aDepth += pSmp.nVibSweep;			// (2 & 3)
					OpenMpt.LimitMax(ref aDepth, (c_int)(pSmp.nVibDepth * 256U));
					chn.nAutoVibDepth = aDepth;			// (5)
					aDepth /= 256;						// (4)

					chn.nAutoVibPos += pSmp.nVibRate;

					c_int vDelta;

					switch (pSmp.nVibType)
					{
						case VibratoType.Random:
						{
							vDelta = Random.Random_<c_int, uint16>(AccessPrng(), 7) - 0x40;
							break;
						}

						case VibratoType.Ramp_Down:
						{
							vDelta = 64 - ((vibPos + 1) / 2);
							break;
						}

						case VibratoType.Ramp_Up:
						{
							vDelta = ((vibPos + 1) / 2) - 64;
							break;
						}

						case VibratoType.Square:
						{
							vDelta = vibPos < 128 ? 64 : 0;
							break;
						}

						case VibratoType.Sine:
						default:
						{
							vDelta = Tables.ItSinusTable[vibPos];
							break;
						}
					}

					vDelta = (vDelta * aDepth) / 64;
					uint32 l = (uint32)CMath.abs(vDelta);
					OpenMpt.LimitMax(ref period, c_int.MaxValue / 256);
					period *= 256;

					if (vDelta < 0)
					{
						vDelta = Util.MulDiv(period, (int32)downTable[l / 4U], 0x10000) - period;

						if ((l & 0x03) != 0)
							vDelta += Util.MulDiv(period, (int32)fineDownTable[l & 0x03], 0x10000) - period;
					}
					else
					{
						vDelta = Util.MulDiv(period, (int32)upTable[l / 4U], 0x10000) - period;

						if ((l & 0x03) != 0)
							vDelta += Util.MulDiv(period, (int32)fineUpTable[l & 0x03], 0x10000) - period;
					}

					if ((c_int.MaxValue - period) >= vDelta)
					{
						period = (period + vDelta) / 256;
						nPeriodFrac = vDelta & 0xff;
					}
					else
					{
						period = c_int.MaxValue / 256;
						nPeriodFrac = 0;
					}
				}
				else
				{
					// MPT's autovibrato code
					int32 autoVibDepth = chn.nAutoVibDepth;
					int32 fullDepth = pSmp.nVibDepth * 256;

					if ((pSmp.nVibSweep == 0) && ((GetType_() & (ModType.It | ModType.Mpt)) == 0))
						autoVibDepth = fullDepth;
					else
					{
						// Calculate current autovibrato depth using vibsweep
						if ((GetType_() & (ModType.It | ModType.Mpt)) != 0)
						{
							autoVibDepth += pSmp.nVibSweep * 2;
							OpenMpt.LimitMax(ref autoVibDepth, fullDepth);
							chn.nAutoVibDepth = autoVibDepth;
						}
						else
						{
							if (!chn.dwFlags.Test(ChannelFlags.Chn_KeyOff) && (autoVibDepth <= fullDepth))
							{
								autoVibDepth += fullDepth / pSmp.nVibSweep;
								chn.nAutoVibDepth = autoVibDepth;
							}

							// FT2 compatibility: Key-off before auto-vibrato sweep-in is complete resets auto-vibrato depth
							// Test case: AutoVibratoSweepKeyOff.xm
							if (autoVibDepth > fullDepth)
								autoVibDepth = fullDepth;
							else if (chn.dwFlags.Test(ChannelFlags.Chn_KeyOff) && m_PlayBehaviour[PlayBehaviour.Ft2AutoVibratoAbortSweep])
								autoVibDepth = fullDepth / pSmp.nVibSweep;
						}
					}

					chn.nAutoVibPos += pSmp.nVibRate;
					c_int vDelta;

					switch (pSmp.nVibType)
					{
						case VibratoType.Random:
						{
							vDelta = Tables.ModRandomTable[chn.nAutoVibPos & 0x3f];
							chn.nAutoVibPos++;
							break;
						}

						case VibratoType.Ramp_Down:
						{
							vDelta = ((0x40 - (chn.nAutoVibPos / 2)) & 0x7f) - 0x40;
							break;
						}

						case VibratoType.Ramp_Up:
						{
							vDelta = ((0x40 + (chn.nAutoVibPos / 2)) & 0x7f) - 0x40;
							break;
						}

						case VibratoType.Square:
						{
							vDelta = (chn.nAutoVibPos & 128) != 0 ? +64 : -64;
							break;
						}

						case VibratoType.Sine:
						default:
						{
							if (GetType_() != ModType.Mt2)
								vDelta = -Tables.ItSinusTable[chn.nAutoVibPos & 0xff];
							else
							{
								// Fix flat-sounding pads in "another worlds" by Eternal Engine.
								// Vibrato starts at the maximum amplitude of the sine wave
								// and the vibrato frequency never decreases below the original note's frequency
								vDelta = (-Tables.ItSinusTable[(chn.nAutoVibPos + 192) & 0xff] + 64) / 2;
							}

							break;
						}
					}

					c_int n = (vDelta * autoVibDepth) / 256;

					if (hasTuning)
					{
						// Vib sweep is not taken into account here
						vibratoFactor += 0.05f * (pSmp.nVibDepth * vDelta) / 4096.0f;	// 4096 == 64^2

						// See vibrato for explanation
						chn.m_CalculateFreq = true;
					}
					else
					{
						// Original behavior
						if (GetType_() != ModType.Xm)
						{
							c_int df1, df2;

							if (n < 0)
							{
								n = -n;
								uint32 n1 = (uint32)(n / 256);
								df1 = (c_int)downTable[n1];
								df2 = (c_int)downTable[n1 + 1];
							}
							else
							{
								uint32 n1 = (uint32)(n / 256);
								df1 = (c_int)upTable[n1];
								df2 = (c_int)upTable[n1 + 1];
							}

							n /= 4;
							period = Util.MulDiv(period, df1 + ((df2 - df1) * (n & 0x3f) / 64), 256);
							nPeriodFrac = period & 0xff;
							period /= 256;
						}
						else
							period += (n / 64);
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public void ProcessRamping(ModChannel chn)//XX 1974
		{
			chn.LeftRamp = chn.RightRamp = 0;

			OpenMpt.LimitMax(ref chn.NewLeftVol, int32.MaxValue >> Mixer.VolumeRampPrecision);
			OpenMpt.LimitMax(ref chn.NewRightVol, int32.MaxValue >> Mixer.VolumeRampPrecision);

			if (chn.dwFlags.Test(ChannelFlags.Chn_VolumeRamp) && ((chn.LeftVol != chn.NewLeftVol) || (chn.RightVol != chn.NewRightVol)))
			{
				bool rampUp = (chn.NewLeftVol > chn.LeftVol) || (chn.NewRightVol > chn.RightVol);
				int32 rampLength, globalRampLength, instrRampLength = 0;
				rampLength = globalRampLength = rampUp ? m_MixerSettings.GetVolumeRampUpSamples() : m_MixerSettings.GetVolumeRampDownSamples();

				if (m_PlayBehaviour[PlayBehaviour.Ft2VolumeRamping] && ((GetType_() & ModType.Xm) != 0))
				{
					// Apply FT2-style super-soft volume ramping (5ms), overriding openmpt settings
					rampLength = globalRampLength = Util.MulDivR(5, (int32)m_MixerSettings.gdwMixingFreq, 1000);
				}

				if ((chn.pModInstrument != null) && rampUp)
				{
					instrRampLength = chn.pModInstrument.nVolRampUp;
					rampLength = (int32)(instrRampLength != 0 ? (m_MixerSettings.gdwMixingFreq * instrRampLength / 100000) : globalRampLength);
				}

				bool enableCustomRamp = instrRampLength > 0;

				if (rampLength == 0)
					rampLength = 1;

				int32 leftDelta = ((chn.NewLeftVol - chn.LeftVol) * (1 << Mixer.VolumeRampPrecision));
				int32 rightDelta = ((chn.NewRightVol - chn.RightVol) * (1 << Mixer.VolumeRampPrecision));

				if (!enableCustomRamp)
				{
					// Extra-smooth ramping, unless we're forced to use the default values
					if (((chn.LeftVol | chn.RightVol) != 0) && ((chn.NewLeftVol | chn.NewRightVol) != 0) && !chn.dwFlags.Test(ChannelFlags.Chn_FastVolRamp))
					{
						rampLength = (int32)m_PlayState.m_nBufferCount;
						OpenMpt.Limit(ref rampLength, globalRampLength, 1 << (Mixer.VolumeRampPrecision - 1));
					}
				}

				chn.LeftRamp = leftDelta / rampLength;
				chn.RightRamp = rightDelta / rampLength;
				chn.LeftVol = chn.NewLeftVol - ((chn.LeftRamp * rampLength) / (1 << Mixer.VolumeRampPrecision));
				chn.RightVol = chn.NewRightVol - ((chn.RightRamp * rampLength) / (1 << Mixer.VolumeRampPrecision));

				if ((chn.LeftRamp | chn.RightRamp) != 0)
					chn.nRampLength = (uint32)rampLength;
				else
				{
					chn.dwFlags.Reset(ChannelFlags.Chn_VolumeRamp);
					chn.LeftVol = chn.NewLeftVol;
					chn.RightVol = chn.NewRightVol;
				}
			}
			else
			{
				chn.dwFlags.Reset(ChannelFlags.Chn_VolumeRamp);
				chn.LeftVol = chn.NewLeftVol;
				chn.RightVol = chn.NewRightVol;
			}

			chn.RampLeftVol = chn.LeftVol * (1 << Mixer.VolumeRampPrecision);
			chn.RampRightVol = chn.RightVol * (1 << Mixer.VolumeRampPrecision);
			chn.dwFlags.Reset(ChannelFlags.Chn_FastVolRamp);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public c_int HandleNoteChangeFilter(ModChannel chn)//XX 2042
		{
			c_int cutOff = -1;

			if (!chn.TriggerNote)
				return cutOff;

			bool useFilter = !m_PlayState.m_Flags.Test(PlayFlags.Song_MptFilterMode);
			ModInstrument pIns = chn.pModInstrument;

			if (pIns != null)
			{
				if (pIns.IsResonanceEnabled())
				{
					chn.nResonance = pIns.GetResonance();
					useFilter = true;
				}

				if (pIns.IsCutOffEnabled())
				{
					chn.nCutOff = pIns.GetCutOff();
					useFilter = true;
				}

				if (useFilter && (pIns.FilterMode != FilterMode.Unchanged))
					chn.nFilterMode = pIns.FilterMode;
			}
			else
			{
				chn.nVolSwing = chn.nPanSwing = 0;
				chn.nCutSwing = chn.nResSwing = 0;
			}

			if (((chn.nCutOff < 0x7f) || m_PlayBehaviour[PlayBehaviour.ItFilterBehaviour]) && useFilter)
			{
				cutOff = SetupChannelFilter(chn, true);

				if (cutOff >= 0)
					cutOff = chn.nCutOff / 2;
			}

			return cutOff;
		}



		/********************************************************************/
		/// <summary>
		/// Returns channel increment and frequency with FREQ_FRACBITS
		/// fractional bits
		/// </summary>
		/********************************************************************/
		public (SamplePosition first, uint32 second) GetChannelIncrement(ModChannel chn, uint32 period, c_int periodFrac)//XX 2081
		{
			uint32 freq;

			if (!chn.HasCustomTuning())
				freq = GetFreqFromPeriod(period, (uint32)chn.nC5Speed, periodFrac);
			else
				freq = (uint32)chn.nPeriod;

			ModInstrument ins = chn.pModInstrument;
			int32 fineTune = chn.MicroTuning;

			if (fineTune != 0)
			{
				if (ins != null)
					fineTune *= ins.MidiPwd;

				if (fineTune != 0)
					freq = SaturateRound.Saturate_Round<uint32, c_double>(freq * CMath.pow(2.0, fineTune / (12.0 * 256.0 * 128.0)));
			}

			// Applying Pitch/Tempo lock
			if ((ins != null) && (ins.PitchToTempoLock.GetRaw() != 0))
				freq = (uint32)Util.MulDivR((int32)freq, (int32)m_PlayState.m_nMusicTempo.GetRaw(), (int32)ins.PitchToTempoLock.GetRaw());

			// Avoid increment to overflow and become negative with unrealisticly high frequencies
			OpenMpt.LimitMax(ref freq, (uint32)int32.MaxValue);

			return (SamplePosition.Ratio(freq, m_MixerSettings.gdwMixingFreq << Snd_Def.Freq_FracBits), freq);
		}
		#endregion

		//XX 2112
		#region Handles envelopes & mixer setup
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public bool ReadNote()
		{
			if (!ProcessRow())
				return false;

			////////////////////////////////////////////////////////////////////////////////////

			if (m_PlayState.m_nMusicTempo.GetRaw() == 0)
				return false;

			m_PlayState.m_GlobalScriptState.NextTick(m_PlayState, this);
			m_PlayState.m_nSamplesPerTick = GetTickDuration(m_PlayState);
			m_PlayState.m_nBufferCount = m_PlayState.m_nSamplesPerTick;

			// Master Volume + Pre-Amplification / Attenuation setup
			uint32 nMasterVol;
			{
				ChannelIndex nChn32 = OpenMpt.Clamp(GetNumChannels(), (ChannelIndex)1, (ChannelIndex)31);
				uint32 masterVol;

				if (m_PlayConfig.GetUseGlobalPreAmp())
				{
					c_int realMasterVol = (c_int)m_MixerSettings.m_nPreAmp;

					if (realMasterVol > 0x80)
					{
						// Attenuate global pre-amp depending on num channels
						realMasterVol = 0x80 + (((realMasterVol - 0x80) * (nChn32 + 4)) / 16);
					}

					masterVol = (uint32)((realMasterVol * m_nSamplePreAmp) / 64);
				}
				else
				{
					// Preferred option: don't use global pre-amp at all
					masterVol = m_nSamplePreAmp;
				}

				if (m_PlayConfig.GetUseGlobalPreAmp())
				{
					uint32 attenuation = preAmpTable[nChn32 / 2];

					if (attenuation < 1)
						attenuation = 1;

					nMasterVol = (masterVol << 7) / attenuation;
				}
				else
					nMasterVol = masterVol;
			}

			////////////////////////////////////////////////////////////////////////////////////
			// Update channels data
			m_nMixChannels = 0;

			for (ChannelIndex nChn = 0; nChn < m_PlayState.Chn.size(); nChn++)
			{
				ModChannel chn = m_PlayState.Chn[nChn];

				// FT2 Compatibility: Prevent notes to be stopped after a fadeout. This way, a portamento effect can pick up a faded instrument which is long enough.
				// This occurs for example in the bassline (channel 11) of jt_burn.xm. I hope this won't break anything else...
				// I also suppose this could decrease mixing performance a bit, but hey, which CPU can't handle 32 muted channels these days... :-)
				if (chn.dwFlags.Test(ChannelFlags.Chn_NoteFade) && ((chn.nFadeOutVol | chn.LeftVol | chn.RightVol) == 0) && !m_PlayBehaviour[PlayBehaviour.Ft2ProcessSilentChannels])
				{
					chn.nLength = 0;
					chn.nROfs = chn.nLOfs = 0;
				}

				// Increment age of NNA channels
				if ((chn.nMasterChn != 0) && (nChn < GetNumChannels()) && (chn.NnaChannelAge < uint16.MaxValue))
					chn.NnaChannelAge++;

				// Check for unused channel
				if (chn.dwFlags.Test(ChannelFlags.Chn_Mute) || ((nChn >= GetNumChannels()) && (chn.nLength == 0)))
				{
					if (nChn < GetNumChannels())
					{
						// Process MIDI macros on channels that are currently muted
						ProcessMacroOnChannel(nChn);
					}

					continue;
				}

				// Reset channel data
				chn.Increment = new SamplePosition(0);
				chn.nRealVolume = 0;
				chn.nCalcVolume = 0;

				chn.nRampLength = 0;

				// Aux variables
				TuningRatioType vibratoFactor = 1;
				TuningNoteIndexType arpeggioSteps = 0;

				ModInstrument pIns = chn.pModInstrument;

				// Calc frequency
				int32 period = 0;

				chn.SynthState.NextTick(m_PlayState, nChn, this);

				// Also process envelopes etc. when there's a plugin on this channel, for possible fake automation using volume and pan data.
				// We only care about master channels, though, since automation only "happens" on them
				bool samplePlaying = (chn.nPeriod != 0) && (chn.nLength != 0);
				bool plugAssigned = (nChn < GetNumChannels()) && ((ChnSettings[nChn].nMixPlugin != 0) || ((chn.pModInstrument != null) && (chn.pModInstrument.nMixPlug != 0)));

				if (samplePlaying || plugAssigned)
				{
					c_int vol = chn.nVolume;
					c_int insVol = chn.nInsVol;		// This is the "SV * IV" value in ITTECH.TXT

					ProcessVolumeSwing(chn, ref m_PlayBehaviour[PlayBehaviour.ItSwingBehaviour] ? ref insVol : ref vol);
					ProcessPanningSwing(chn);
					ProcessTremolo(chn, ref vol);
					ProcessTremor(nChn, ref vol);

					// Clip volume and multiply (extend to 14 bits)
					OpenMpt.Limit(ref vol, 0, 256);
					vol <<= 6;

					// Process Envelopes
					if (pIns != null)
					{
						if (m_PlayBehaviour[PlayBehaviour.ItEnvelopePositionHandling])
						{
							// In IT compatible mode, envelope position indices are shifted by one for proper envelope pausing,
							// so we have to update the position before we actually process the envelopes.
							// When using MPT behaviour, we get the envelope position for the next tick while we are still calculating the current tick,
							// which then results in wrong position information when the envelope is paused on the next row.
							// Test cases: s77.it
							IncrementEnvelopePositions(chn);
						}

						ProcessVolumeEnvelope(chn, ref vol);
						ProcessInstrumentFade(chn, ref vol);
						ProcessPanningEnvelope(chn);

						if (!m_PlayBehaviour[PlayBehaviour.ItPitchPanSeparation] && (chn.nNote != ModCommand.Note_None) && (chn.pModInstrument != null) && (chn.pModInstrument.nPps != 0))
							ProcessPitchPanSeparation(ref chn.nRealPan, chn.nNote, chn.pModInstrument);
					}
					else
					{
						// No Envelope: key off => note cut
						if (chn.dwFlags.Test(ChannelFlags.Chn_NoteFade))	// 1.41-: CHN_KEYOFF|CHN_NOTEFADE
						{
							chn.nFadeOutVol = 0;
							vol = 0;
						}
					}

					if (chn.IsPaused)
						vol = 0;

					// Vol is 14-bits
					if (vol != 0)
					{
						// IMPORTANT: chn.nRealVolume is 14 bits !!!
						// -> Util::muldiv( 14+8, 6+6, 18); => RealVolume: 14-bit result (22+12-20)
						if (chn.dwFlags.Test(ChannelFlags.Chn_SyncMute))
							chn.nRealVolume = 0;
						else if (m_PlayConfig.GetGlobalVolumeAppliesToMaster())
						{
							// Don't let global volume affect level of sample if
							// Global volume is going to be applied to master output anyway
							chn.nRealVolume = Util.MulDiv((int32)(vol * Snd_Def.Max_Global_Volume), chn.nGlobalVol * insVol, 1 << 20);
						}
						else
							chn.nRealVolume = Util.MulDiv(vol * m_PlayState.m_nGlobalVolume, chn.nGlobalVol * insVol, 1 << 20);
					}

					chn.nCalcVolume = vol;	// Update calculated volume for MIDI macros

					// ST3 only clamps the final output period, but never the channel's internal period.
					// Test case: PeriodLimit.s3m
					if ((chn.nPeriod < m_nMinPeriod) && (GetType_() != ModType.S3M) && !PeriodsAreFrequencies())
						chn.nPeriod = m_nMinPeriod;
					else if ((chn.nPeriod >= m_nMaxPeriod) && m_PlayBehaviour[PlayBehaviour.ApplyUpperPeriodLimit] && !PeriodsAreFrequencies())
					{
						// ...but on the other hand, ST3's SoundBlaster driver clamps the maximum channel period.
						// Test case: PeriodLimitUpper.s3m
						chn.nPeriod = m_nMaxPeriod;
					}

					if (m_PlayBehaviour[PlayBehaviour.Ft2Periods])
						OpenMpt.Clamp(chn.nPeriod, 1, 31999);

					period = chn.nPeriod;

					// When glissando mode is set to semitones, clamp to the next halftone
					if (((chn.dwFlags & (ChannelFlags.Chn_Glissando | ChannelFlags.Chn_Portamento)) == (ChannelFlags.Chn_Glissando | ChannelFlags.Chn_Portamento)) && ((!m_SongFlags.Test(SongFlags.Pt_Mode) || (chn.RowCommand.IsTonePortamento() && !m_PlayState.m_Flags.Test(PlayFlags.Song_FirstTick)))))
					{
						if (period != chn.CachedPeriod)
						{
							// Only recompute this whole thing in case the base period has changed
							chn.CachedPeriod = period;
							chn.GlissandoPeriod = (int32)GetPeriodFromNote(GetNoteFromPeriod((uint32)period, chn.nFineTune, (uint32)chn.nC5Speed), chn.nFineTune, (uint32)chn.nC5Speed);
						}

						period = chn.GlissandoPeriod;
					}

					ProcessArpeggio(nChn, ref period, ref arpeggioSteps);

					// Preserve Amiga freq limits.
					// In ST3, the frequency is always clamped to periods 113 to 856, while in ProTracker,
					// the limit is variable, depending on the finetune of the sample.
					// The int32_max test is for the arpeggio wrap-around in ProcessArpeggio().
					// Test case: AmigaLimits.s3m, AmigaLimitsFinetune.mod
					if (m_SongFlags.Test(SongFlags.AmigaLimits | SongFlags.Pt_Mode) && (period != int32.MaxValue))
					{
						c_int limitLow = 113 * 4, limitHigh = 856 * 4;

						if (GetType_() != ModType.S3M)
						{
							c_int tableOffset = Xm2ModFineTune(chn.nFineTune) * 12;
							limitLow = Tables.ProTrackerTunedPeriods[tableOffset + 11] / 2;
							limitHigh = Tables.ProTrackerTunedPeriods[tableOffset] * 2;

							// Amiga cannot actually keep up with lower periods
							if (limitLow < 113 * 4)
								limitLow = 113 * 4;
						}

						OpenMpt.Limit(ref period, limitLow, limitHigh);
						OpenMpt.Limit(ref chn.nPeriod, limitLow, limitHigh);
					}

					ProcessPanbrello(chn);
				}

				// IT Compatibility: Ensure that there is no pan swing, panbrello, panning envelopes, etc. applied on surround channels.
				// Test case: surround-pan.it
				if (chn.dwFlags.Test(ChannelFlags.Chn_Surround) && !m_PlayState.m_Flags.Test(PlayFlags.Song_SurroundPan) && m_PlayBehaviour[PlayBehaviour.ItNoSurroundPan])
					chn.nRealPan = 128;

				// Setup Initial Filter for this note
				c_int cutOff = HandleNoteChangeFilter(chn);

//XX				if ((cutOff >= 0) && chn.dwFlags.Test(ChannelFlags.Chn_Adlib) && (m_Opl != null))
//					m_Opl.Volume(nChn, (uint8)cutOff, true);

				// Now that all relevant envelopes etc. have been processed, we can parse the MIDI macro data
				ProcessMacroOnChannel(nChn);

				// After MIDI macros have been processed, we can also process the pitch / filter envelope and other pitch-related things
				if (samplePlaying)
				{
					c_int envCutOff = ProcessPitchFilterEnvelope(chn, ref period);

					// Cutoff doubles as modulator intensity for FM instruments
//XX					if ((envCutOff >= 0) && chn.dwFlags.Test(ChannelFlags.Chn_Adlib) && (m_Opl != null))
//						m_Opl.Volume(nChn, (uint8)(envCutOff / 4), true);
				}

				if ((chn.RowCommand.VolCmd == VolumeCommand.VibratoDepth) && ((chn.RowCommand.Command == EffectCommand.Vibrato) || (chn.RowCommand.Command == EffectCommand.VibratoVol) || (chn.RowCommand.Command == EffectCommand.FineVibrato)))
				{
					if (GetType_() == ModType.Xm)
					{
						// XM Compatibility: Vibrato should be advanced twice (but not added up) if both volume-column and effect column vibrato is present.
						// Effect column vibrato parameter has precedence if non-zero.
						// Test case: VibratoDouble.xm
						if (!m_PlayState.m_Flags.Test(PlayFlags.Song_FirstTick))
							chn.nVibratoPos += chn.nVibratoSpeed;
					}
					else if ((GetType_() & (ModType.It | ModType.Mpt)) != 0)
					{
						// IT Compatibility: Vibrato should be applied twice if both volume-colum and effect column vibrato is present.
						// Volume column vibrato parameter has precedence if non-zero.
						// Test case: VibratoDouble.it
						Vibrato(chn, chn.RowCommand.Vol);
						ProcessVibrato(nChn, ref period, ref vibratoFactor);
					}
				}

				// Plugins may also receive vibrato
				ProcessVibrato(nChn, ref period, ref vibratoFactor);

				if (samplePlaying)
				{
					chn.SynthState.ApplyChannelState(chn, ref period, this);
					m_PlayState.m_GlobalScriptState.ApplyChannelState(m_PlayState, nChn, ref period, this);

					c_int nPeriodFrac = 0;
					ProcessSampleAutoVibrato(chn, ref period, ref vibratoFactor, ref nPeriodFrac);

					// Final Period
					// ST3 only clamps the final output period, but never the channel's internal period.
					// Test case: PeriodLimit.s3m
					if (period <= m_nMinPeriod)
					{
						if (m_PlayBehaviour[PlayBehaviour.St3LimitPeriod])	// Pattern 15 in watcha.s3m
							chn.nLength = 0;

						period = m_nMinPeriod;
					}

					bool hasTuning = chn.HasCustomTuning();

					if (hasTuning)
					{
						if (chn.m_CalculateFreq || (chn.m_RecalculateFreqOnFirstTick && (m_PlayState.m_nTickCount == 0)))
						{
							chn.RecalcTuningFreq(vibratoFactor, arpeggioSteps, this);

							if (!chn.m_CalculateFreq)
								chn.m_RecalculateFreqOnFirstTick = false;
							else
								chn.m_CalculateFreq = false;
						}
					}

					(SamplePosition nInc, uint32 freq) = GetChannelIncrement(chn, (uint32)period, nPeriodFrac);

					nInc.MulDiv(m_nFreqFactor, 65536);

					if (nInc.IsZero())
						nInc.Set(0, 1);

					chn.Increment = nInc;

//XX					if (((chn.dwFlags & (ChannelFlags.Chn_Adlib | ChannelFlags.Chn_Mute | ChannelFlags.Chn_SyncMute)) == ChannelFlags.Chn_Adlib) && (m_Opl != null))
/*					{
						//XX A lot is missing here
					}*/
				}

				// Increment envelope positions
				if ((pIns != null) && !m_PlayBehaviour[PlayBehaviour.ItEnvelopePositionHandling])
				{
					// In IT and FT2 compatible mode, envelope positions are updated above.
					// Test cases: s77.it, EnvLoops.xm
					IncrementEnvelopePositions(chn);
				}

				// Volume ramping
				chn.dwFlags.Set(ChannelFlags.Chn_VolumeRamp, ((chn.nRealVolume | chn.RightVol | chn.LeftVol) != 0) && !chn.dwFlags.Test(ChannelFlags.Chn_Adlib));

				chn.NewLeftVol = chn.NewRightVol = 0;
				chn.pCurrentSample = (chn.pModSample != null) && chn.pModSample.HasSampleData() && (chn.nLength != 0) && chn.IsSamplePlaying() ? chn.pModSample.SampleEv() : null;

				if (chn.pCurrentSample != null)
				{
					uint32 kChnMasterVol = nMasterVol;

					// Adjusting volumes
					{
						int32 pan = m_MixerSettings.gnChannels >= 2 ? OpenMpt.Clamp(chn.nRealPan, 0, 256) : 128;

						int32 realVol = (int32)((chn.nRealVolume * kChnMasterVol) / 128);

						// Extra attenuation required here if we're bypassing pre-amp
						if (!m_PlayConfig.GetUseGlobalPreAmp())
							realVol /= 2;

						PanningMode panningMode = m_PlayConfig.GetPanningMode();

						if ((panningMode == PanningMode.SoftPanning) || ((panningMode == PanningMode.Undetermined) && ((m_MixerSettings.MixerFlags & MixerFlags.SoftPanning) != 0)))
						{
							if (pan < 128)
							{
								chn.NewLeftVol = (realVol * 128) / 256;
								chn.NewRightVol = (realVol * pan) / 256;
							}
							else
							{
								chn.NewLeftVol = (realVol * (256 - pan)) / 256;
								chn.NewRightVol = (realVol * 128) / 256;
							}
						}
						else if (panningMode == PanningMode.Ft2Panning)
						{
							// FT2 uses square root panning. There is a 257-entry LUT for this,
							// but FT2's internal panning ranges from 0 to 255 only, meaning that
							// you can never truly achieve 100% right panning in FT2, only 100% left.
							// Test case: FT2PanLaw.xm
							OpenMpt.LimitMax(ref pan, 255);

							c_int panL = pan > 0 ? Tables.XmPanningTable[256 - pan] : 65536;
							c_int panR = Tables.XmPanningTable[pan];
							chn.NewLeftVol = (realVol * panL) / 65536;
							chn.NewRightVol = (realVol * panR) / 65536;
						}
						else
						{
							chn.NewLeftVol = (realVol * (256 - pan)) / 256;
							chn.NewRightVol = (realVol * pan) / 256;
						}
					}

					if ((chn.pModInstrument != null) && Resampling.IsKnownMode(chn.pModInstrument.Resampling))
					{
						// For defined resampling modes, use per-instrument resampling mode if set
						chn.ResamplingMode = chn.pModInstrument.Resampling;
					}
					else if (Resampling.IsKnownMode(m_nResampling))
						chn.ResamplingMode = m_nResampling;
					else if (m_SongFlags.Test(SongFlags.IsAmiga) && (m_Resampler.m_Settings.EmulateAmiga != Resampling.AmigaFilter.Off))
					{
						// Enforce Amiga resampler for Amiga modules
						chn.ResamplingMode = ResamplingMode.Amiga;
					}
					else
					{
						// Default to global mixer settings
						chn.ResamplingMode = m_Resampler.m_Settings.SrcMode;
					}

					if (chn.Increment.IsUnity() && !(chn.dwFlags.Test(ChannelFlags.Chn_Vibrato) || (chn.nAutoVibDepth != 0) || (chn.ResamplingMode == ResamplingMode.Amiga)))
					{
						// Exact sample rate match, do not interpolate at all
						// - unless vibrato is applied, because in this case the constant enabling and disabling
						// of resampling can introduce clicks (this is easily observable with a sine sample
						// played at the mix rate)
						chn.ResamplingMode = ResamplingMode.Nearest;
					}

					c_int extraAttenuation = m_PlayConfig.GetExtraSampleAttenuation();
					chn.NewLeftVol /= (1 << extraAttenuation);
					chn.NewRightVol /= (1 << extraAttenuation);

					// Dolby Pro-Logic Surround
					if (chn.dwFlags.Test(ChannelFlags.Chn_Surround) && (m_MixerSettings.gnChannels == 2) && m_PlayState.m_SurroundEnabled)
						chn.NewRightVol = -chn.NewRightVol;

					// Checking Ping-Pong Loops
					if (chn.dwFlags.Test(ChannelFlags.Chn_PingPongFlag))
						chn.Increment.Negate();

					// Setting up volume ramp
					ProcessRamping(chn);

					// Adding the channel in the channel list
					if (!chn.dwFlags.Test(ChannelFlags.Chn_Adlib))
						m_PlayState.ChnMix[m_nMixChannels++] = nChn;
				}
				else
				{
					chn.RightVol = chn.LeftVol = 0;
					chn.nLength = 0;

					// Put the channel back into the mixer for end-of-sample pop reduction
					if ((chn.nLOfs != 0) || (chn.nROfs != 0))
						m_PlayState.ChnMix[m_nMixChannels++] = nChn;
				}

				chn.dwOldFlags = chn.dwFlags;

				m_VisualNoteKicked[nChn] |= chn.TriggerNote;	// NostalgicPlayer specific
				chn.TriggerNote = false;		// For SONG_PAUSED mode
			}

			// If there are more channels being mixed than allowed, order them by volume and discard the most quiet ones
			if (m_nMixChannels >= m_MixerSettings.m_nMaxMixChannels)
				Algorithm.partial_sort(m_PlayState.ChnMix.begin(), m_PlayState.ChnMix.begin() + m_MixerSettings.m_nMaxMixChannels, m_PlayState.ChnMix.begin() + m_nMixChannels, (ChannelIndex i, ChannelIndex j) => { return m_PlayState.Chn[i].nRealVolume > m_PlayState.Chn[j].nRealVolume; });

			return true;
		}
		#endregion

		//XX 2174
		#region Update channels data
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void ProcessMacroOnChannel(ChannelIndex nChn)//XX 2627
		{
			ModChannel chn = m_PlayState.Chn[nChn];

			if (nChn < GetNumChannels())
			{
				if (((chn.RowCommand.Command == EffectCommand.Midi) && m_PlayState.m_Flags.Test(PlayFlags.Song_FirstTick)) || (chn.RowCommand.Command == EffectCommand.SmoothMidi))
				{
					if (chn.RowCommand.Param < 0x80)
						ProcessMidiMacro(m_PlayState, nChn, chn.RowCommand.Command == EffectCommand.SmoothMidi, m_MidiCfg.SFx[chn.nActiveMacro], chn.RowCommand.Param);
					else
						ProcessMidiMacro(m_PlayState, nChn, chn.RowCommand.Command == EffectCommand.SmoothMidi, m_MidiCfg.Zxx[chn.RowCommand.Param & 0x7f], chn.RowCommand.Param);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void ApplyGlobalVolumeWithRamping(c_int channels, CPointer<int32> soundBuffer, CPointer<int32> rearBuffer, uint32 lCount, int32 m_nGlobalVolume, int32 step, ref int32 m_nSamplesToGlobalVolRampDest, ref int32 m_lHighResRampingGlobalVolume)//XX 2758
		{
			bool isStereo = channels >= 2;
			bool hasRear = channels >= 4;

			for (uint32 pos = 0; pos < lCount; ++pos)
			{
				if (m_nSamplesToGlobalVolRampDest > 0)
				{
					// Ramping required
					m_lHighResRampingGlobalVolume += step;

					soundBuffer[0] = Util.MulDiv(soundBuffer[0], m_lHighResRampingGlobalVolume, (int32)(Snd_Def.Max_Global_Volume << Mixer.VolumeRampPrecision));

					if (isStereo)
						soundBuffer[1] = Util.MulDiv(soundBuffer[1], m_lHighResRampingGlobalVolume, (int32)(Snd_Def.Max_Global_Volume << Mixer.VolumeRampPrecision));

					if (hasRear)
					{
						rearBuffer[0] = Util.MulDiv(rearBuffer[0], m_lHighResRampingGlobalVolume, (int32)(Snd_Def.Max_Global_Volume << Mixer.VolumeRampPrecision));
						rearBuffer[1] = Util.MulDiv(rearBuffer[1], m_lHighResRampingGlobalVolume, (int32)(Snd_Def.Max_Global_Volume << Mixer.VolumeRampPrecision));
					}

					m_nSamplesToGlobalVolRampDest--;
				}
				else
				{
					soundBuffer[0] = Util.MulDiv(soundBuffer[0], m_nGlobalVolume, (int32)Snd_Def.Max_Global_Volume);

					if (isStereo)
						soundBuffer[1] = Util.MulDiv(soundBuffer[1], m_nGlobalVolume, (int32)Snd_Def.Max_Global_Volume);

					if (hasRear)
					{
						rearBuffer[0] = Util.MulDiv(rearBuffer[0], m_nGlobalVolume, (int32)Snd_Def.Max_Global_Volume);
						rearBuffer[1] = Util.MulDiv(rearBuffer[1], m_nGlobalVolume, (int32)Snd_Def.Max_Global_Volume);
					}

					m_lHighResRampingGlobalVolume = m_nGlobalVolume << Mixer.VolumeRampPrecision;
				}

				soundBuffer += isStereo ? 2 : 1;

				if (hasRear)
					rearBuffer += 2;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void ProcessGlobalVolume(samplecount_t lCount)//XX 2787
		{
			// Should we ramp?
			if (IsGlobalVolumeUnset())
			{
				// do not ramp if no global volume was set before (which is the case at song start),
				// to prevent audible glitches when default volume is > 0 and it is set to 0 in the first row
				m_PlayState.m_nGlobalVolumeDestination = m_PlayState.m_nGlobalVolume;
				m_PlayState.m_nSamplesToGlobalVolRampDest = 0;
				m_PlayState.m_nGlobalVolumeRampAmount = 0;
			}
			else if (m_PlayState.m_nGlobalVolumeDestination != m_PlayState.m_nGlobalVolume)
			{
				// User has provided new global volume
				//
				// m_nGlobalVolume: the last global volume which got set e.g. by a pattern command
				// m_nGlobalVolumeDestination: the current target of the ramping algorithm
				bool rampUp = m_PlayState.m_nGlobalVolume > m_PlayState.m_nGlobalVolumeDestination;

				m_PlayState.m_nGlobalVolumeDestination = m_PlayState.m_nGlobalVolume;
				m_PlayState.m_nSamplesToGlobalVolRampDest = m_PlayState.m_nGlobalVolumeRampAmount = rampUp ? m_MixerSettings.GetVolumeRampUpSamples() : m_MixerSettings.GetVolumeRampDownSamples();
			}

			// Calculate ramping step
			int32 step = 0;

			if (m_PlayState.m_nSamplesToGlobalVolRampDest > 0)
			{
				// Still some ramping left to do
				int32 highResGlobalVolumeDestination = m_PlayState.m_nGlobalVolumeDestination << Mixer.VolumeRampPrecision;

				int32 delta = highResGlobalVolumeDestination - m_PlayState.m_lHighResRampingGlobalVolume;
				step = delta / m_PlayState.m_nSamplesToGlobalVolRampDest;

				if (m_nMixLevels == MixLevels.v1_17RC2)
				{
					// Define max step size as some factor of user defined ramping value: the lower the value, the more likely the click.
					// If step is too big (might cause click), extend ramp length.
					// Warning: This increases the volume ramp length by EXTREME amounts (factors of 100 are easily reachable)
					// compared to the user-defined setting, so this really should not be used!
					int32 maxStep = Math.Max(50, 10000 / (m_PlayState.m_nGlobalVolumeRampAmount + 1));

					while (CMath.abs(step) > maxStep)
					{
						m_PlayState.m_nSamplesToGlobalVolRampDest += m_PlayState.m_nGlobalVolumeRampAmount;
						step = delta / m_PlayState.m_nSamplesToGlobalVolRampDest;
					}
				}
			}

			// Apply volume and ramping
			if (m_MixerSettings.gnChannels == 1)
				ApplyGlobalVolumeWithRamping(1, MixSoundBuffer, MixRearBuffer, lCount, m_PlayState.m_nGlobalVolume, step, ref m_PlayState.m_nSamplesToGlobalVolRampDest, ref m_PlayState.m_lHighResRampingGlobalVolume);
			else if (m_MixerSettings.gnChannels == 2)
				ApplyGlobalVolumeWithRamping(2, MixSoundBuffer, MixRearBuffer, lCount, m_PlayState.m_nGlobalVolume, step, ref m_PlayState.m_nSamplesToGlobalVolRampDest, ref m_PlayState.m_lHighResRampingGlobalVolume);
			else if (m_MixerSettings.gnChannels == 4)
				ApplyGlobalVolumeWithRamping(4, MixSoundBuffer, MixRearBuffer, lCount, m_PlayState.m_nGlobalVolume, step, ref m_PlayState.m_nSamplesToGlobalVolRampDest, ref m_PlayState.m_lHighResRampingGlobalVolume);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void ProcessStereoSeparation(samplecount_t countChunk)//XX 2850
		{
			ApplyStereoSeparation(MixSoundBuffer, MixRearBuffer, m_MixerSettings.gnChannels, countChunk, m_MixerSettings.m_nStereoSeparation);
		}
		#endregion
	}
}
