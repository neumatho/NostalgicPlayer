/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Agent.Player.MusiclineEditor.Containers;

namespace Polycode.NostalgicPlayer.Agent.Player.MusiclineEditor
{
	/// <summary>
	/// Handle all the part effect commands
	/// </summary>
	internal partial class MusiclineEditorWorker
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PlayPartFx(VoiceInfo voiceInfo)
		{
			voiceInfo.Play |= PlayFlag.FirstRowTick;
			voiceInfo.Effects1.Effect &= ~InstrumentEffect1.HoldSustain;

			voiceInfo.Vibrato = PartEffect.None;
			voiceInfo.VibratoNote = 0;
			voiceInfo.PtVibratoNote = 0;
			voiceInfo.Tremolo = PartEffect.None;
			voiceInfo.Volume = PartEffect.None;
			voiceInfo.VolumeAdd = PartEffect.None;
			voiceInfo.VolumeSlide = PartEffect.None;
			voiceInfo.ChannelVolumeSlide = PartEffect.None;
			voiceInfo.MasterVolumeSlide = PartEffect.None;
			voiceInfo.PitchSlide = PartEffect.None;
			voiceInfo.PtPitchSlide = PartEffect.None;
			voiceInfo.SampleOffset = 0;
			voiceInfo.Restart = RestartFlag.None;

			voiceInfo.Arpeggio &= ~(ArpeggioFlag.WillSetNote | ArpeggioFlag.UseWaveSample);

			if (voiceInfo.TestAndClearArpFlag(ArpeggioFlag.OneStepPending))
			{
				voiceInfo.Arpeggio = ArpeggioFlag.None;
				voiceInfo.ArpeggioVolumeSlide = ArpeggioEffect.None;
				voiceInfo.ArpeggioPitchSlide = ArpeggioEffect.None;
				voiceInfo.ArpeggioPitchSlideNote = 0;
			}

			if (voiceInfo.PartNote != 0)
			{
				voiceInfo.PitchSlideToNote = -1;
				voiceInfo.PtPitchSlideToNote = -1;
			}

			for (int i = 0; i < 5; i++)
			{
				PartEffectEntry effect = voiceInfo.PartEffects[i];

				switch (effect.Effect)
				{
					case PartEffect.SlideUp:
					{
						PFx_SlideUp(voiceInfo, effect);
						break;
					}

					case PartEffect.SlideDown:
					{
						PFx_SlideDown(voiceInfo, effect);
						break;
					}

					case PartEffect.Portamento:
					{
						PFx_Portamento(voiceInfo, effect);
						break;
					}

					case PartEffect.InitInstrumentPortamento:
					{
						PFx_InitInstrumentPortamento(voiceInfo);
						break;
					}

					case PartEffect.PitchUp:
					{
						PFx_PitchUp(voiceInfo, effect);
						break;
					}

					case PartEffect.PitchDown:
					{
						PFx_PitchDown(voiceInfo, effect);
						break;
					}

					case PartEffect.VibratoSpeed:
					{
						PFx_VibratoSpeed(voiceInfo, effect);
						break;
					}

					case PartEffect.VibratoUp:
					{
						PFx_VibratoUp(voiceInfo, effect);
						break;
					}

					case PartEffect.VibratoDown:
					{
						PFx_VibratoDown(voiceInfo, effect);
						break;
					}

					case PartEffect.VibratoWave:
					{
						PFx_VibratoWave(voiceInfo, effect);
						break;
					}

					case PartEffect.SetFineTune:
					{
						PFx_SetFineTune(voiceInfo, effect);
						break;
					}

					case PartEffect.SetInstrumentVolume:
					{
						PFx_SetInstrumentVolume(voiceInfo, effect);
						break;
					}

					case PartEffect.InstrumentVolumeSlideUp:
					{
						PFx_InstrumentVolumeSlideUp(voiceInfo, effect);
						break;
					}

					case PartEffect.InstrumentVolumeSlideDown:
					{
						PFx_InstrumentVolumeSlideDown(voiceInfo, effect);
						break;
					}

					case PartEffect.SetSlideInstrumentVolume:
					{
						PFx_SetSlideInstrumentVolume(voiceInfo, effect);
						break;
					}

					case PartEffect.SlideToInstrumentVolume:
					{
						PFx_SlideToInstrumentVolume(voiceInfo, effect);
						break;
					}

					case PartEffect.InstrumentVolumeAdd:
					{
						PFx_InstrumentVolumeAdd(voiceInfo, effect);
						break;
					}

					case PartEffect.InstrumentVolumeSub:
					{
						PFx_InstrumentVolumeSub(voiceInfo, effect);
						break;
					}

					case PartEffect.TremoloSpeed:
					{
						PFx_TremoloSpeed(voiceInfo, effect);
						break;
					}

					case PartEffect.TremoloUp:
					{
						PFx_TremoloUp(voiceInfo, effect);
						break;
					}

					case PartEffect.TremoloDown:
					{
						PFx_TremoloDown(voiceInfo, effect);
						break;
					}

					case PartEffect.TremoloWave:
					{
						PFx_TremoloWave(voiceInfo, effect);
						break;
					}

					case PartEffect.SetChannelVolume:
					{
						PFx_SetChannelVolume(voiceInfo, effect);
						break;
					}

					case PartEffect.ChannelVolumeSlideUp:
					{
						PFx_ChannelVolumeSlideUp(voiceInfo, effect);
						break;
					}

					case PartEffect.ChannelVolumeSlideDown:
					{
						PFx_ChannelVolumeSlideDown(voiceInfo, effect);
						break;
					}

					case PartEffect.SetSlideChannelVolume:
					{
						PFx_SetSlideChannelVolume(voiceInfo, effect);
						break;
					}

					case PartEffect.SlideToChannelVolume:
					{
						PFx_SlideToChannelVolume(voiceInfo, effect);
						break;
					}

					case PartEffect.ChannelVolumeAdd:
					{
						PFx_ChannelVolumeAdd(voiceInfo, effect);
						break;
					}

					case PartEffect.ChannelVolumeSub:
					{
						PFx_ChannelVolumeSub(voiceInfo, effect);
						break;
					}

					case PartEffect.SetVolumeAllChannels:
					{
						PFx_SetVolumeAllChannels(effect);
						break;
					}

					case PartEffect.SetMasterVolume:
					{
						PFx_SetMasterVolume(effect);
						break;
					}

					case PartEffect.MasterVolumeSlideUp:
					{
						PFx_MasterVolumeSlideUp(voiceInfo, effect);
						break;
					}

					case PartEffect.MasterVolumeSlideDown:
					{
						PFx_MasterVolumeSlideDown(voiceInfo, effect);
						break;
					}

					case PartEffect.SetSlideMasterVolume:
					{
						PFx_SetSlideMasterVolume(voiceInfo, effect);
						break;
					}

					case PartEffect.SlideToMasterVolume:
					{
						PFx_SlideToMasterVolume(voiceInfo, effect);
						break;
					}

					case PartEffect.MasterVolumeAdd:
					{
						PFx_MasterVolumeAdd(voiceInfo, effect);
						break;
					}

					case PartEffect.MasterVolumeSub:
					{
						PFx_MasterVolumeSub(voiceInfo, effect);
						break;
					}

					case PartEffect.SpeedOneChannel:
					{
						PFx_SpeedOneChannel(voiceInfo, effect);
						break;
					}

					case PartEffect.GrooveOneChannel:
					{
						PFx_GrooveOneChannel(voiceInfo, effect);
						break;
					}

					case PartEffect.SpeedAllChannels:
					{
						PFx_SpeedAllChannels(voiceInfo, effect);
						break;
					}

					case PartEffect.GrooveAllChannels:
					{
						PFx_GrooveAllChannels(voiceInfo, effect);
						break;
					}

					case PartEffect.ArpeggioTable:
					{
						PFx_ArpeggioTable(voiceInfo, effect);
						break;
					}

					case PartEffect.ArpeggioTableOneStep:
					{
						PFx_ArpeggioTableOneStep(voiceInfo, effect);
						break;
					}

					case PartEffect.HoldSustain:
					{
						PFx_HoldSustain(voiceInfo, effect);
						break;
					}

					case PartEffect.Filter:
					{
						PFx_Filter(effect);
						break;
					}

					case PartEffect.SampleOffset:
					{
						PFx_SampleOffset(voiceInfo, effect);
						break;
					}

					case PartEffect.RestartNoVolume:
					{
						PFx_RestartNoVolume(voiceInfo);
						break;
					}

					case PartEffect.WaveSample:
					{
						PFx_WaveSample(voiceInfo, effect);
						break;
					}

					case PartEffect.InitInstrument:
					{
						PFx_InitInstrument(voiceInfo);
						break;
					}

					case PartEffect.PtSlideUp:
					{
						PFx_PtSlideUp(voiceInfo, effect);
						break;
					}

					case PartEffect.PtSlideDown:
					{
						PFx_PtSlideDown(voiceInfo, effect);
						break;
					}

					case PartEffect.PtPortamento:
					{
						PFx_PtPortamento(voiceInfo, effect);
						break;
					}

					case PartEffect.PtFineSlideUp:
					{
						PFx_PtFineSlideUp(voiceInfo, effect);
						break;
					}

					case PartEffect.PtFineSlideDown:
					{
						PFx_PtFineSlideDown(voiceInfo, effect);
						break;
					}

					case PartEffect.PtVolumeSlideUp:
					{
						PFx_PtVolumeSlideUp(voiceInfo, effect);
						break;
					}

					case PartEffect.PtVolumeSlideDown:
					{
						PFx_PtVolumeSlideDown(voiceInfo, effect);
						break;
					}

					case PartEffect.PtTremolo:
					{
						PFx_PtTremolo(voiceInfo, effect);
						break;
					}

					case PartEffect.PtTremoloWave:
					{
						PFx_PtTremoloWave(voiceInfo, effect);
						break;
					}

					case PartEffect.PtVibrato:
					{
						PFx_PtVibrato(voiceInfo, effect);
						break;
					}

					case PartEffect.PtVibratoWave:
					{
						PFx_PtVibratoWave(voiceInfo, effect);
						break;
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PFx_SlideUp(VoiceInfo voiceInfo, PartEffectEntry effect)
		{
			voiceInfo.PitchSlide = effect.Effect;
			voiceInfo.PitchSlideType = Direction.Upward;

			if (effect.Argument != 0)
				voiceInfo.PitchSlideSpeed = effect.Argument;

			voiceInfo.PitchSlideToNote = (59 * 32) + 32;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PFx_SlideDown(VoiceInfo voiceInfo, PartEffectEntry effect)
		{
			voiceInfo.PitchSlide = effect.Effect;
			voiceInfo.PitchSlideType = Direction.Downward;

			if (effect.Argument != 0)
				voiceInfo.PitchSlideSpeed = effect.Argument;

			voiceInfo.PitchSlideToNote = 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PFx_Portamento(VoiceInfo voiceInfo, PartEffectEntry effect)
		{
			voiceInfo.PitchSlide = effect.Effect;

			if (effect.Argument != 0)
				voiceInfo.PitchSlideSpeed = effect.Argument;

			ushort note = (ushort)(voiceInfo.Note + voiceInfo.PitchSlideNote);
			ushort slideToNote = voiceInfo.PartNote;

			if (slideToNote != 0)
			{
				slideToNote <<= 5;

				voiceInfo.PartNote = 0;
				voiceInfo.PitchSlideToNote = (short)slideToNote;

				if (slideToNote != note)
				{
					voiceInfo.PitchSlideType = slideToNote < note ? Direction.Downward : Direction.Upward;
					return;
				}
				else
					voiceInfo.PitchSlideToNote = -1;
			}

			if (voiceInfo.PitchSlideToNote == -1)
				voiceInfo.PitchSlide = PartEffect.None;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PFx_InitInstrumentPortamento(VoiceInfo voiceInfo)
		{
			voiceInfo.InstrumentPitchSlide = PartEffect.None;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PFx_PitchUp(VoiceInfo voiceInfo, PartEffectEntry effect)
		{
			if (effect.Argument != 0)
				voiceInfo.PitchAdd += effect.Argument;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PFx_PitchDown(VoiceInfo voiceInfo, PartEffectEntry effect)
		{
			if (effect.Argument != 0)
				voiceInfo.PitchAdd -= effect.Argument;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PFx_VibratoSpeed(VoiceInfo voiceInfo, PartEffectEntry effect)
		{
			if (effect.Argument != 0)
				voiceInfo.VibratoCommandSpeed = effect.Argument;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PFx_VibratoUp(VoiceInfo voiceInfo, PartEffectEntry effect)
		{
			PFx_Vibrato(voiceInfo, effect, Direction.Upward);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PFx_VibratoDown(VoiceInfo voiceInfo, PartEffectEntry effect)
		{
			PFx_Vibrato(voiceInfo, effect, Direction.Downward);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PFx_Vibrato(VoiceInfo voiceInfo, PartEffectEntry effect, Direction direction)
		{
			voiceInfo.Vibrato = effect.Effect;

			if (voiceInfo.PartNote != 0)
			{
				voiceInfo.VibratoDirection = direction;
				voiceInfo.VibratoCounter = 0;
				voiceInfo.VibratoCommandDepth = 0;
				voiceInfo.VibratoCommandDelay = 0;
				voiceInfo.VibratoAttackSpeed = 0;
				voiceInfo.VibratoAttackLength = 0;
			}

			if (effect.Argument != 0)
				voiceInfo.VibratoDepth = (ushort)(effect.Argument << 8);

			voiceInfo.VibratoWaveType = voiceInfo.PartVibratoWaveType;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PFx_VibratoWave(VoiceInfo voiceInfo, PartEffectEntry effect)
		{
			if (effect.Argument <= 3)
				voiceInfo.VibratoWaveType = voiceInfo.PartVibratoWaveType = (WaveType)effect.Argument;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PFx_SetFineTune(VoiceInfo voiceInfo, PartEffectEntry effect)
		{
			sbyte fineTune = (sbyte)effect.Argument;

			if (fineTune < 0)
			{
				if (fineTune < -31)
					fineTune = -31;
			}
			else
			{
				if (fineTune > 31)
					fineTune = 31;
			}

			voiceInfo.FineTune = fineTune;
			voiceInfo.Play |= PlayFlag.KeepEffectFineTune;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PFx_SetInstrumentVolume(VoiceInfo voiceInfo, PartEffectEntry effect)
		{
			voiceInfo.Volume = effect.Effect;
			voiceInfo.VolumeSet = (ushort)(effect.Argument << 4);

			if (voiceInfo.VolumeSet > 1024)
				voiceInfo.VolumeSet = 1024;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PFx_InstrumentVolumeSlideUp(VoiceInfo voiceInfo, PartEffectEntry effect)
		{
			voiceInfo.VolumeSlide = effect.Effect;

			if (effect.Argument != 0)
				voiceInfo.VolumeSlideSpeed = effect.Argument;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PFx_InstrumentVolumeSlideDown(VoiceInfo voiceInfo, PartEffectEntry effect)
		{
			voiceInfo.VolumeSlide = effect.Effect;

			if (effect.Argument != 0)
				voiceInfo.VolumeSlideSpeed = effect.Argument;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PFx_SetSlideInstrumentVolume(VoiceInfo voiceInfo, PartEffectEntry effect)
		{
			voiceInfo.VolumeSlideToVolume = (ushort)(effect.Argument << 4);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PFx_SlideToInstrumentVolume(VoiceInfo voiceInfo, PartEffectEntry effect)
		{
			voiceInfo.VolumeSlide = effect.Effect;

			if (effect.Argument != 0)
				voiceInfo.VolumeSlideSpeed = effect.Argument;

			ushort volume = voiceInfo.Volume1;
			voiceInfo.VolumeSlideVolume = volume;
			ushort slideToVolume = voiceInfo.VolumeSlideToVolume;

			if (slideToVolume != volume)
			{
				voiceInfo.VolumeSlideType = slideToVolume < volume ? Direction.Downward : Direction.Upward;
				voiceInfo.VolumeSlideToVolumeOff = false;
			}
			else
				voiceInfo.VolumeSlideToVolumeOff = true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PFx_InstrumentVolumeAdd(VoiceInfo voiceInfo, PartEffectEntry effect)
		{
			voiceInfo.VolumeAdd = effect.Effect;

			if (effect.Argument != 0)
				voiceInfo.VolumeAddNumber = (short)(effect.Argument << 4);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PFx_InstrumentVolumeSub(VoiceInfo voiceInfo, PartEffectEntry effect)
		{
			voiceInfo.VolumeAdd = effect.Effect;

			if (effect.Argument != 0)
				voiceInfo.VolumeAddNumber = (short)(-(effect.Argument << 4));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PFx_TremoloSpeed(VoiceInfo voiceInfo, PartEffectEntry effect)
		{
			if (effect.Argument != 0)
				voiceInfo.TremoloCommandSpeed = effect.Argument;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PFx_TremoloUp(VoiceInfo voiceInfo, PartEffectEntry effect)
		{
			PFx_Tremolo(voiceInfo, effect, Direction.Upward);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PFx_TremoloDown(VoiceInfo voiceInfo, PartEffectEntry effect)
		{
			PFx_Tremolo(voiceInfo, effect, Direction.Downward);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PFx_Tremolo(VoiceInfo voiceInfo, PartEffectEntry effect, Direction direction)
		{
			voiceInfo.Tremolo = effect.Effect;

			if (voiceInfo.PartNote != 0)
			{
				voiceInfo.TremoloDirection = direction;
				voiceInfo.TremoloCounter = 0;
				voiceInfo.TremoloCommandDepth = 0;
				voiceInfo.TremoloCommandDelay = 0;
				voiceInfo.TremoloAttackSpeed = 0;
				voiceInfo.TremoloAttackLength = 0;
			}

			if (effect.Argument != 0)
				voiceInfo.TremoloDepth = (ushort)(effect.Argument << 8);

			voiceInfo.TremoloWaveType = voiceInfo.PartTremoloWaveType;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PFx_TremoloWave(VoiceInfo voiceInfo, PartEffectEntry effect)
		{
			if (effect.Argument <= 3)
				voiceInfo.TremoloWaveType = voiceInfo.PartTremoloWaveType = (WaveType)effect.Argument;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PFx_SetChannelVolume(VoiceInfo voiceInfo, PartEffectEntry effect)
		{
			voiceInfo.ChannelVolume = (ushort)(effect.Argument << 4);

			if (voiceInfo.ChannelVolume > 64 * 16)
				voiceInfo.ChannelVolume = 64 * 16;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PFx_ChannelVolumeSlideUp(VoiceInfo voiceInfo, PartEffectEntry effect)
		{
			voiceInfo.ChannelVolumeSlide = effect.Effect;

			if (effect.Argument != 0)
				voiceInfo.ChannelVolumeSlideSpeed = effect.Argument;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PFx_ChannelVolumeSlideDown(VoiceInfo voiceInfo, PartEffectEntry effect)
		{
			voiceInfo.ChannelVolumeSlide = effect.Effect;

			if (effect.Argument != 0)
				voiceInfo.ChannelVolumeSlideSpeed = effect.Argument;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PFx_SetSlideChannelVolume(VoiceInfo voiceInfo, PartEffectEntry effect)
		{
			voiceInfo.ChannelVolumeSlideToVolume = (ushort)(effect.Argument << 4);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PFx_SlideToChannelVolume(VoiceInfo voiceInfo, PartEffectEntry effect)
		{
			voiceInfo.ChannelVolumeSlide = effect.Effect;

			if (effect.Argument != 0)
				voiceInfo.ChannelVolumeSlideSpeed = effect.Argument;

			ushort volume = voiceInfo.ChannelVolume;
			voiceInfo.ChannelVolumeSlideVolume = volume;
			ushort slideToVolume = voiceInfo.ChannelVolumeSlideToVolume;

			if (slideToVolume != volume)
			{
				voiceInfo.ChannelVolumeSlideType = slideToVolume < volume ? Direction.Downward : Direction.Upward;
				voiceInfo.ChannelVolumeSlideToVolumeOff = false;
			}
			else
				voiceInfo.ChannelVolumeSlideToVolumeOff = true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PFx_ChannelVolumeAdd(VoiceInfo voiceInfo, PartEffectEntry effect)
		{
			if (effect.Argument != 0)
				voiceInfo.ChannelVolumeAddNumber = (short)(effect.Argument << 4);

			short newVolume = (short)(voiceInfo.ChannelVolume + voiceInfo.ChannelVolumeAddNumber);
			if (newVolume > 64 * 16)
				newVolume = 64 * 16;

			voiceInfo.ChannelVolume = (ushort)newVolume;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PFx_ChannelVolumeSub(VoiceInfo voiceInfo, PartEffectEntry effect)
		{
			if (effect.Argument != 0)
				voiceInfo.ChannelVolumeAddNumber = (short)(effect.Argument << 4);

			short newVolume = (short)(voiceInfo.ChannelVolume - voiceInfo.ChannelVolumeAddNumber);
			if (newVolume < 0)
				newVolume = 0;

			voiceInfo.ChannelVolume = (ushort)newVolume;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PFx_SetVolumeAllChannels(PartEffectEntry effect)
		{
			ushort newVolume = (ushort)(effect.Argument << 4);

			for (int i = 0; i < numberOfChannels; i++)
				voices[i].ChannelVolume = newVolume;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PFx_SetMasterVolume(PartEffectEntry effect)
		{
			playingInfo.MasterVolume = (ushort)(effect.Argument << 4);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PFx_MasterVolumeSlideUp(VoiceInfo voiceInfo, PartEffectEntry effect)
		{
			voiceInfo.MasterVolumeSlide = effect.Effect;

			if (effect.Argument != 0)
				voiceInfo.MasterVolumeSlideSpeed = effect.Argument;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PFx_MasterVolumeSlideDown(VoiceInfo voiceInfo, PartEffectEntry effect)
		{
			voiceInfo.MasterVolumeSlide = effect.Effect;

			if (effect.Argument != 0)
				voiceInfo.MasterVolumeSlideSpeed = effect.Argument;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PFx_SetSlideMasterVolume(VoiceInfo voiceInfo, PartEffectEntry effect)
		{
			voiceInfo.MasterVolumeSlideToVolume = (ushort)(effect.Argument << 4);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PFx_SlideToMasterVolume(VoiceInfo voiceInfo, PartEffectEntry effect)
		{
			voiceInfo.MasterVolumeSlide = effect.Effect;

			if (effect.Argument != 0)
				voiceInfo.MasterVolumeSlideSpeed = effect.Argument;

			ushort volume = playingInfo.MasterVolume;
			voiceInfo.MasterVolumeSlideVolume = volume;
			ushort slideToVolume = voiceInfo.MasterVolumeSlideToVolume;

			if (slideToVolume != volume)
			{
				voiceInfo.MasterVolumeSlideType = slideToVolume < volume ? Direction.Downward : Direction.Upward;
				voiceInfo.MasterVolumeSlideToVolumeOff = false;
			}
			else
				voiceInfo.MasterVolumeSlideToVolumeOff = true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PFx_MasterVolumeAdd(VoiceInfo voiceInfo, PartEffectEntry effect)
		{
			if (effect.Argument != 0)
				voiceInfo.MasterVolumeAddNumber = (short)(effect.Argument << 4);

			short newVolume = (short)(playingInfo.MasterVolume + voiceInfo.MasterVolumeAddNumber);
			if (newVolume > 64 * 16)
				newVolume = 64 * 16;

			playingInfo.MasterVolume = (ushort)newVolume;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PFx_MasterVolumeSub(VoiceInfo voiceInfo, PartEffectEntry effect)
		{
			if (effect.Argument != 0)
				voiceInfo.MasterVolumeAddNumber = (short)(effect.Argument << 4);

			short newVolume = (short)(playingInfo.MasterVolume - voiceInfo.MasterVolumeAddNumber);
			if (newVolume < 0)
				newVolume = 0;

			playingInfo.MasterVolume = (ushort)newVolume;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PFx_SpeedOneChannel(VoiceInfo voiceInfo, PartEffectEntry effect)
		{
			byte newSpeed = effect.Argument;

			if (newSpeed != 0)
			{
				if (newSpeed > 31)
					newSpeed = 31;

				voiceInfo.SpeedPart = 1;
				voiceInfo.Speed = newSpeed;

				if ((voiceInfo.Groove == 0) || !voiceInfo.PartGroove)
					voiceInfo.SpeedCounter = newSpeed;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PFx_GrooveOneChannel(VoiceInfo voiceInfo, PartEffectEntry effect)
		{
			byte newGroove = effect.Argument;

			if (newGroove != 0)
			{
				if (newGroove > 31)
					newGroove = 31;

				voiceInfo.GroovePart = 1;
				voiceInfo.Groove = newGroove;

				if ((voiceInfo.Groove == 0) || voiceInfo.PartGroove)
					voiceInfo.SpeedCounter = newGroove;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PFx_SpeedAllChannels(VoiceInfo voiceInfo, PartEffectEntry effect)
		{
			byte newSpeed = effect.Argument;

			if (newSpeed != 0)
			{
				if (newSpeed < 32)
				{
					playingInfo.TuneSpeed = newSpeed;

					for (int i = 0; i < numberOfChannels; i++)
					{
						VoiceInfo workVoiceInfo = voices[i];

						if (voiceInfo.ChannelNumber >= workVoiceInfo.ChannelNumber)
						{
							if (workVoiceInfo.SpeedPart == 0)
							{
								workVoiceInfo.Speed = newSpeed;

								if ((workVoiceInfo.Groove == 0) || !workVoiceInfo.PartGroove)
									workVoiceInfo.SpeedCounter = newSpeed;
							}
						}
						else
							workVoiceInfo.Speed = newSpeed;
					}
				}
				else
				{
					playingInfo.TuneTempo = newSpeed;

					SetBpmTempo(newSpeed);
					ShowTempo();
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PFx_GrooveAllChannels(VoiceInfo voiceInfo, PartEffectEntry effect)
		{
			byte newGroove = effect.Argument;

			if (newGroove != 0)
			{
				newGroove &= 0x1f;
				playingInfo.TuneGroove = newGroove;

				for (int i = 0; i < numberOfChannels; i++)
				{
					VoiceInfo workVoiceInfo = voices[i];

					if (voiceInfo.ChannelNumber >= workVoiceInfo.ChannelNumber)
					{
						if (workVoiceInfo.GroovePart == 0)
						{
							workVoiceInfo.Groove = newGroove;

							if ((newGroove != 0) && !workVoiceInfo.PartGroove)
								workVoiceInfo.SpeedCounter = newGroove;
						}
					}
					else
						workVoiceInfo.Groove = newGroove;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PFx_ArpeggioTable(VoiceInfo voiceInfo, PartEffectEntry effect)
		{
			voiceInfo.Arpeggio |= ArpeggioFlag.UseTable;
			voiceInfo.ArpeggioTable = effect.Argument;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PFx_ArpeggioTableOneStep(VoiceInfo voiceInfo, PartEffectEntry effect)
		{
			voiceInfo.Arpeggio |= (ArpeggioFlag.OneStepPending | ArpeggioFlag.UseTable);
			voiceInfo.ArpeggioTable = effect.Argument;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PFx_HoldSustain(VoiceInfo voiceInfo, PartEffectEntry effect)
		{
			voiceInfo.Effects1.Effect |= InstrumentEffect1.HoldSustain;
			voiceInfo.Effects1.Argument &= ~InstrumentFlag.EnvelopeHoldSustain;

			if (effect.Argument != 0)
				voiceInfo.Effects1.Argument |= InstrumentFlag.EnvelopeHoldSustain;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PFx_Filter(PartEffectEntry effect)
		{
			AmigaFilter = effect.Argument != 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PFx_SampleOffset(VoiceInfo voiceInfo, PartEffectEntry effect)
		{
			voiceInfo.SampleOffsetActive = true;

			if (effect.Argument != 0)
				voiceInfo.SampleOffset = effect.Argument;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PFx_RestartNoVolume(VoiceInfo voiceInfo)
		{
			if (voiceInfo.PartInstrument == 0)
				voiceInfo.Restart = RestartFlag.RestartFromPartFx;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PFx_WaveSample(VoiceInfo voiceInfo, PartEffectEntry effect)
		{
			if (effect.Argument != 0)
			{
				Sample sample = samples[effect.Argument];

				if (sample != null)
				{
					voiceInfo.WaveSample = sample;
					voiceInfo.Arpeggio |= ArpeggioFlag.UseWaveSample;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PFx_InitInstrument(VoiceInfo voiceInfo)
		{
			voiceInfo.PhaseInit = 0;
			voiceInfo.ResonanceInit = 0;
			voiceInfo.FilterInit = 0;
			voiceInfo.TransformInit = 0;
			voiceInfo.MixInit = 0;
			voiceInfo.LoopInit = 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PFx_PtSlideUp(VoiceInfo voiceInfo, PartEffectEntry effect)
		{
			voiceInfo.PtPitchSlide = effect.Effect;
			voiceInfo.PtPitchSlideType = Direction.Upward;

			if (effect.Argument != 0)
				voiceInfo.PtPitchSlideSpeed = effect.Argument;

			voiceInfo.PtPitchSlideToNote = 106;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PFx_PtSlideDown(VoiceInfo voiceInfo, PartEffectEntry effect)
		{
			voiceInfo.PtPitchSlide = effect.Effect;
			voiceInfo.PtPitchSlideType = Direction.Downward;

			if (effect.Argument != 0)
				voiceInfo.PtPitchSlideSpeed = effect.Argument;

			voiceInfo.PtPitchSlideToNote = 3591;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PFx_PtPortamento(VoiceInfo voiceInfo, PartEffectEntry effect)
		{
			voiceInfo.PitchSlide = PartEffect.Portamento;
			voiceInfo.PtPitchSlide = effect.Effect;

			if (effect.Argument != 0)
				voiceInfo.PtPitchSlideSpeed2 = effect.Argument;

			ushort period = (ushort)(GetPeriod(voiceInfo) + voiceInfo.PtPitchSlideNote);

			if (voiceInfo.PartNote != 0)
			{
				ushort period2 = GetPeriod2(voiceInfo, (ushort)(voiceInfo.PartNote << 5));
				voiceInfo.PartNote = 0;
				voiceInfo.PtPitchSlideToNote = (short)period2;

				if (period != period2)
				{
					voiceInfo.PtPitchSlideType = period < period2 ? Direction.Downward : Direction.Upward;
					return;
				}

				voiceInfo.PtPitchSlideToNote = -1;
			}

			if (voiceInfo.PtPitchSlideToNote == -1)
			{
				voiceInfo.PtPitchSlide = PartEffect.None;
				voiceInfo.PitchSlide = PartEffect.None;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PFx_PtFineSlideUp(VoiceInfo voiceInfo, PartEffectEntry effect)
		{
			if (effect.Argument != 0)
				voiceInfo.PtPitchAdd -= (short)(effect.Argument & 0x0f);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PFx_PtFineSlideDown(VoiceInfo voiceInfo, PartEffectEntry effect)
		{
			if (effect.Argument != 0)
				voiceInfo.PtPitchAdd += (short)(effect.Argument & 0x0f);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PFx_PtTremolo(VoiceInfo voiceInfo, PartEffectEntry effect)
		{
			voiceInfo.Tremolo = effect.Effect;

			if (effect.Argument != 0)
			{
				byte tremoloCommand = voiceInfo.PtTremoloCommand;
				byte argument = (byte)(effect.Argument & 0x0f);

				if (argument != 0)
					tremoloCommand = (byte)((tremoloCommand & 0xf0) | argument);

				argument = (byte)(effect.Argument & 0xf0);

				if (argument != 0)
					tremoloCommand = (byte)((tremoloCommand & 0x0f) | argument);

				voiceInfo.PtTremoloCommand = tremoloCommand;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PFx_PtTremoloWave(VoiceInfo voiceInfo, PartEffectEntry effect)
		{
			voiceInfo.PtTremoloWaveType = (PtWaveType)(effect.Argument & 0x0f);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PFx_PtVibrato(VoiceInfo voiceInfo, PartEffectEntry effect)
		{
			voiceInfo.Vibrato = effect.Effect;

			if (effect.Argument != 0)
			{
				byte vibratoCommand = voiceInfo.PtVibratoCommand;
				byte argument = (byte)(effect.Argument & 0x0f);

				if (argument != 0)
					vibratoCommand = (byte)((vibratoCommand & 0xf0) | argument);

				argument = (byte)(effect.Argument & 0xf0);

				if (argument != 0)
					vibratoCommand = (byte)((vibratoCommand & 0x0f) | argument);

				voiceInfo.PtVibratoCommand = vibratoCommand;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PFx_PtVibratoWave(VoiceInfo voiceInfo, PartEffectEntry effect)
		{
			voiceInfo.PtVibratoWaveType = (PtWaveType)(effect.Argument & 0x0f);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PFx_PtVolumeSlideUp(VoiceInfo voiceInfo, PartEffectEntry effect)
		{
			PFx_InstrumentVolumeSlideUp(voiceInfo, effect);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PFx_PtVolumeSlideDown(VoiceInfo voiceInfo, PartEffectEntry effect)
		{
			PFx_InstrumentVolumeSlideDown(voiceInfo, effect);
		}
	}
}
