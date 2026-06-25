/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Ports.LibTfmxAudioDecoder.Chris.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibTfmxAudioDecoder.Chris
{
	/// <summary>
	/// 
	/// </summary>
	public partial class TfmxDecoder
	{
		/********************************************************************/
		/// <summary>
		/// For the publicly available TFMX modules, the array of offsets to
		/// each macro script is located either at the end of the file (for
		/// compressed MDAT) or before the track table (for uncompressed
		/// MDAT) and, apparently, always _not before_ the array of pattern
		/// offsets.
		///
		/// Thus, for truncated (!) MDAT one or more macro offsets may be
		/// undefined. For example, if the memory following the MDAT were
		/// cleared to 0 on Amiga, it would point at the beginning of the
		/// MDAT which would be wrong. Or it may point anywhere, if other
		/// data or the SMPL data are stored directly after the MDAT.
		/// Furthermore, macro script data may be missing, too.
		///
		/// With smart pointer access, an OOB offset is no threat. The
		/// underlying implementation would read value 0. Since we store
		/// MDAT+SMPL data within the same buffer, for truncated MDAT we may
		/// read from SMPL space by mistake. But, and that is an important
		/// BUT, we cannot fix truncated data. Imagine cases like a Goto/Cont
		/// macro command jumping to a bad offset. Rejecting that would be
		/// wrong. And it could also be damaged data, not just truncated
		/// data.
		///
		/// So, nothing is won if trying to reject obviously bad offsets here
		/// by applying range checks (e.g. to see whether an offset is
		/// ‹ input.mdatSize) and handling the return value in the calling
		/// function, too
		/// </summary>
		/********************************************************************/
		private udword GetMacroOffset(ubyte macro)
		{
			realMacrosUsed.Add(macro);

			return offsets.Header + MyEndian.ReadBEUdword(pBuf, (udword)(offsets.Macros + ((macro & 0x7f) << 2)));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void InitMacro(VoiceVars voice)
		{
			voice.Macro.Step = 0;
			voice.Macro.Wait = 0;
			voice.Macro.Loop = 0xff;
			voice.Macro.State = -1;
			voice.WaitOnDmaCount = 0;

			if (variant.ExecOrder == ExecOrder.Mac_Mod_Seq)
				voice.EffectsMode = 0;
			else
				voice.EffectsMode = 1;

			voice.Macro.ExtraWait = true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void ProcessMacroMain(VoiceVars voice)
		{
			if (voice.Macro.State == 0)
				return;
			else if (voice.Macro.State == 1)
				InitMacro(voice);
			else
			{
				if (voice.Macro.Wait > 0)
				{
					voice.Macro.Wait--;
					return;
				}
			}

			c_int macroLen = 0;

			do
			{
				playerInfo.MacroEvalAgain = false;

				udword p = voice.Macro.Offset + (voice.Macro.Step << 2);
				c_int command = pBuf[p];

				playerInfo.Cmd.Aa = 0;
				playerInfo.Cmd.Bb = pBuf[p + 1];
				playerInfo.Cmd.Cd = pBuf[p + 2];
				playerInfo.Cmd.Ee = pBuf[p + 3];

				macroCmdUsed[command & 0x3f] = true;
				macroCmdFuncs[command & 0x3f](voice);
			}
			while (playerInfo.MacroEvalAgain && (++macroLen < 32));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void MacroFunc_ExtraWait(VoiceVars voice)
		{
			voice.Macro.Step++;

			if (variant.ExtraWaitV1)
				return;

			// TBD
			if (!voice.Macro.ExtraWait)
			{
				voice.Macro.ExtraWait = true;
				playerInfo.MacroEvalAgain = true;
			}

			// Resetting the flag via DMAoff macro is extremely rare
			// and potentially not needed with no hardware Paula
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void MacroFunc_Nop(VoiceVars voice)
		{
			voice.Macro.Step++;
			playerInfo.MacroEvalAgain = true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void MacroFunc_StopSound(VoiceVars voice)
		{
			voice.Envelope.Flag = 0;
			voice.Vibrato.Time = 0;
			voice.Portamento.Speed = 0;
			voice.Sid.TargetLength = 0;
			voice.Rnd.Flag = 0;

			// The variant that also does AddVolume/SetVolume
			MacroFunc_StopSample_Sub(voice);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void MacroFunc_StartSample(VoiceVars voice)
		{
			PaulaVoice paulaVoice = paulaVoices[voice.VoiceNum];

			// There are variants of the "DMAon" macro command, which
			// are not needed because we don't emulate access to Amiga
			// custom chip registers like DMACON, INTENA and INTREQ
			if (variant.NoDelayedDmaOn)
				paulaVoice.On();
			else
				voice.Macro.DelayedOn = true;

			voice.Macro.Step++;

			// Variants of the player can set the macro wait value here.
			// The high byte (in cmd.aa) is set to zero early, so only the
			// low byte (in cmd.bb) would matter. However, as the low byte
			// is 0 for all but a very few TFMX files, the resulting wait
			// value would stay at 0, which would be pointless.
			//
			// Furthermore, of the existing files that run a subsequent Wait
			// command, that wait value would take precedence. It can be
			// assumed that setting the wait value here is not the real goal.
			//
			// Instead, of the few remaining files that set the first parameter
			// to 1, they want the player to run sound synthesis via the effects
			// processor a first time before turning on the audio channel
			if (playerInfo.Cmd.Bb != 0)
			{
				voice.EffectsMode = 1;

				if (variant.ExecOrder == ExecOrder.Mod_Mac_Seq)
					ProcessModulation(voice);
			}

			playerInfo.MacroEvalAgain = true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void MacroFunc_SetBegin(VoiceVars voice)
		{
			voice.AddBeginCount = 0;

			udword start = offsets.SampleData + MyEndian.MakeDword(0, playerInfo.Cmd.Bb, playerInfo.Cmd.Cd, playerInfo.Cmd.Ee);
			MacroFunc_SetBegin_Sub(voice, start);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void MacroFunc_SetBegin_Sub(VoiceVars voice, udword start)
		{
			voice.Sample.Start = start;
			ToPaulaStart(voice, start);

			voice.Macro.Step++;
			playerInfo.MacroEvalAgain = true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void MacroFunc_SetLen(VoiceVars voice)
		{
			uword len = MyEndian.MakeWord(playerInfo.Cmd.Cd, playerInfo.Cmd.Ee);
			voice.Sample.Length = len;
			ToPaulaLength(voice, len);

			voice.Macro.Step++;
			playerInfo.MacroEvalAgain = true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void MacroFunc_Wait(VoiceVars voice)
		{
			if ((playerInfo.Cmd.Bb & 1) != 0)		// Special behaviour related to macro command Random Play
			{
				if (voice.Rnd.BlockWait)
					return;

				voice.Rnd.BlockWait = true;

				voice.Macro.Step++;
				playerInfo.MacroEvalAgain = true;
			}
			else		// Normal behaviour
			{
				voice.Macro.Wait = (sword)MyEndian.MakeWord(playerInfo.Cmd.Cd, playerInfo.Cmd.Ee);
				MacroFunc_ExtraWait(voice);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void MacroFunc_Loop(VoiceVars voice)
		{
			if (voice.Macro.Loop == 0)
			{
				voice.Macro.Loop = 0xff;
				voice.Macro.Step++;

				// Possibly unique to R-Type, which does an extra wait here
				// unlike TFMX v1 and later
				if (variant.MacroLoopExtraWait)
					return;
			}
			else
			{
				if (voice.Macro.Loop == 0xff)
					voice.Macro.Loop = (ubyte)(playerInfo.Cmd.Bb - 1);
				else
					voice.Macro.Loop--;

				voice.Macro.Step = MyEndian.MakeWord(playerInfo.Cmd.Cd, playerInfo.Cmd.Ee);
			}

			playerInfo.MacroEvalAgain = true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void MacroFunc_Cont(VoiceVars voice)
		{
			voice.Macro.Offset = GetMacroOffset((ubyte)(playerInfo.Cmd.Bb & 0x7f));
			voice.Macro.Step = MyEndian.MakeWord(playerInfo.Cmd.Cd, playerInfo.Cmd.Ee);
			voice.Macro.Loop = 0xff;
			voice.Macro.BranchIfSame = false;

			playerInfo.MacroEvalAgain = true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void MacroFunc_Stop(VoiceVars voice)
		{
			voice.Macro.State = 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void MacroFunc_AddNote(VoiceVars voice)
		{
			MacroFunc_AddNote_Sub(voice, voice.Note, voice.Detune);
			MacroFunc_ExtraWait(voice);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void MacroFunc_AddNote_Sub(VoiceVars voice, ubyte noteAdd, sword detuneAdd)
		{
			sbyte n = (sbyte)(noteAdd + (sbyte)playerInfo.Cmd.Bb);
			uword p = NoteToPeriod(n);
			sword finetune = (sword)(detuneAdd + (sword)MyEndian.MakeWord(playerInfo.Cmd.Cd, playerInfo.Cmd.Ee));

			if (variant.FinetuneUnscaled)
				p = (uword)(p + finetune);
			else if (finetune != 0)
				p = (uword)(((finetune + 0x100) * p) >> 8);

			voice.Period = p;

			if (voice.Portamento.Speed == 0)
				voice.OutputPeriod = p;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void MacroFunc_SetNote(VoiceVars voice)
		{
			sword detune = voice.Detune;

			// TFMX v1 SetNote ignores voice detune
			if (variant.SetNoteV1)
				detune = 0;

			MacroFunc_AddNote_Sub(voice, 0, detune);
			MacroFunc_ExtraWait(voice);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void MacroFunc_Reset(VoiceVars voice)
		{
			voice.AddBeginCount = 0;
			voice.Envelope.Flag = 0;
			voice.Vibrato.Time = 0;
			voice.Portamento.Speed = 0;

			voice.Macro.Step++;
			playerInfo.MacroEvalAgain = true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void MacroFunc_Portamento(VoiceVars voice)
		{
			voice.Portamento.Count = playerInfo.Cmd.Bb;
			voice.Portamento.Wait = 1;

			if (variant.PortaOverride || (voice.Portamento.Speed == 0))
				voice.Portamento.Period = voice.Period;

			voice.Portamento.Speed = MyEndian.MakeWord(playerInfo.Cmd.Cd, playerInfo.Cmd.Ee);

			voice.Macro.Step++;
			playerInfo.MacroEvalAgain = true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void MacroFunc_Vibrato(VoiceVars voice)
		{
			voice.Vibrato.Time = playerInfo.Cmd.Bb;

			// Original v1 and v2 apply this bit mask here.
			// Since a composer may have entered an uneven vibrato parameter,
			// the masked value can affect vibrato amplitude
			if (variant.VibratoTimeMask)
				voice.Vibrato.Time &= 0xfe;

			voice.Vibrato.Count = (ubyte)(playerInfo.Cmd.Bb >> 1);
			voice.Vibrato.Intensity = (sbyte)playerInfo.Cmd.Ee;

			if (voice.Portamento.Speed == 0)
			{
				voice.OutputPeriod = voice.Period;
				voice.Vibrato.Delta = 0;
			}

			voice.Macro.Step++;
			playerInfo.MacroEvalAgain = true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void MacroFunc_AddVolume(VoiceVars voice)
		{
			ubyte vol = (ubyte)(voice.NoteVolume + voice.NoteVolume + voice.NoteVolume);
			vol += playerInfo.Cmd.Ee;	// Ignore cmd.cd as only a byte value is used
			voice.Volume = (sbyte)vol;

			voice.Macro.Step++;
			playerInfo.MacroEvalAgain = true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void MacroFunc_AddVolNote(VoiceVars voice)
		{
			// Replaces AddVolume by default. Potentially harmless,
			// since 'bb' arg must be set to 0xfe in order to activate the
			// extra behaviour, and AddVolume doesn't use 'bb' arg, so
			// it's set to 0 in macro scripts
			if (playerInfo.Cmd.Cd == 0xfe)
			{
				ubyte ee = playerInfo.Cmd.Ee;
				playerInfo.Cmd.Cd = playerInfo.Cmd.Ee = 0;

				MacroFunc_AddNote_Sub(voice, voice.Note, voice.Detune);

				playerInfo.Cmd.Ee = ee;
			}

			ubyte vol = (ubyte)(voice.NoteVolume + voice.NoteVolume + voice.NoteVolume);
			vol += playerInfo.Cmd.Ee;	// Ignore cmd.cd as only a byte value is used
			voice.Volume = (sbyte)vol;

			voice.Macro.Step++;
			playerInfo.MacroEvalAgain = true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void MacroFunc_SetVolume(VoiceVars voice)
		{
			if (playerInfo.Cmd.Cd == 0xfe)		// +AddNote variant
			{
				ubyte ee = playerInfo.Cmd.Ee;
				playerInfo.Cmd.Cd = playerInfo.Cmd.Ee = 0;

				MacroFunc_AddNote_Sub(voice, voice.Note, voice.Detune);

				playerInfo.Cmd.Ee = ee;
			}

			voice.Volume = (sbyte)playerInfo.Cmd.Ee;

			voice.Macro.Step++;
			playerInfo.MacroEvalAgain = true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void MacroFunc_Envelope(VoiceVars voice)
		{
			voice.Envelope.Speed = playerInfo.Cmd.Bb;
			voice.Envelope.Flag = voice.Envelope.Count = playerInfo.Cmd.Cd;
			voice.Envelope.Target = playerInfo.Cmd.Ee;

			voice.Macro.Step++;
			playerInfo.MacroEvalAgain = true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void MacroFunc_LoopKeyUp(VoiceVars voice)
		{
			if (!voice.KeyUp)
				MacroFunc_Loop(voice);
			else
			{
				voice.Macro.Step++;
				playerInfo.MacroEvalAgain = true;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void MacroFunc_AddBegin(VoiceVars voice)
		{
			// If the .bb parameter is non-zero, that's extra behaviour not
			// supported by old TFMX (particularly not by the official v1.5
		    // and v2.2 releases and some variants used during that era).
		    // Those don't use that parameter, it defaults to 0, and they
		    // accept only a 16-bit offset. Lacking the count parameter,
		    // the AddBegin macro command had to be run whenever wanting
		    // to apply the sample offset.
		    //
		    // Modernized TFMX variants introduced the count parameter as
		    // to trigger automatic updates of the sample offset during
		    // modulation/effects processing.
		    //
		    // So, the core of this macro implementation can become the default.
		    //
		    // However: Some music files based on the old TFMX design set
		    // that parameter by mistake when entering a negative offset
		    // (and expanding the two's complement value to 24 bits and
		    // storing the upper bits in the first .bb parameter). That would
		    // activate the modernized behaviour and cause a conflict, if
		    // the player added automatic updates to the effect.
		    //
		    // NB! We handle variant.noAddBeginCount during modulation
		    voice.AddBeginCount = voice.AddBeginArg = playerInfo.Cmd.Bb;
			sdword offset = (sword)MyEndian.MakeWord(playerInfo.Cmd.Cd, playerInfo.Cmd.Ee);
			voice.AddBeginOffset = offset;

			udword begin = (udword)(voice.Sample.Start + offset);

			if (begin < offsets.SampleData)
				begin = offsets.SampleData;

			if (voice.Sid.TargetLength == 0)
				MacroFunc_SetBegin_Sub(voice, begin);
			else
			{
				voice.Sample.Start = begin;
				voice.Sid.SourceOffset = begin;

				voice.Macro.Step++;
				playerInfo.MacroEvalAgain = true;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void MacroFunc_AddLen(VoiceVars voice)
		{
			voice.Macro.Step++;
			playerInfo.MacroEvalAgain = true;

			sword len = (sword)MyEndian.MakeWord(playerInfo.Cmd.Cd, playerInfo.Cmd.Ee);
			voice.Sample.Length = (uword)(voice.Sample.Length + len);

			if (voice.Sid.TargetLength == 0)
				ToPaulaLength(voice, voice.Sample.Length);
			else
				voice.Sid.SourceLength = (uword)len;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void MacroFunc_StopSample(VoiceVars voice)
		{
			MacroFunc_StopSample_Sub(voice);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void MacroFunc_StopSample_Sub(VoiceVars voice)
		{
			PaulaVoice paulaVoice = paulaVoices[voice.VoiceNum];

			voice.Macro.Step++;

			// Rare variants of TFMX implement it as a count value, but no module
			// sets the value to anything above 1
			if (playerInfo.Cmd.Bb != 0)
			{
				voice.Macro.ExtraWait = false;
				voice.Macro.DelayedOff = true;
			}
			else
			{
				paulaVoice.Off();
				playerInfo.MacroEvalAgain = true;
			}

			// The variant that also does AddVolume/SetVolume
			if (playerInfo.Cmd.Cd == 0)
			{
				if (playerInfo.Cmd.Ee == 0)
					return;

				ubyte vol1 = (ubyte)(voice.NoteVolume + voice.NoteVolume + voice.NoteVolume);
				voice.Volume = (sbyte)(vol1 + MyEndian.MakeWord(playerInfo.Cmd.Cd, playerInfo.Cmd.Ee));
			}
			else
				voice.Volume = (sbyte)playerInfo.Cmd.Ee;

			// Apply current fade volume
			sbyte vol = voice.Volume;

			if (playerInfo.Fade.Volume < 64)
				vol = (sbyte)((4 * voice.Volume * playerInfo.Fade.Volume) / (4 * 0x40));

			paulaVoice.Paula.Volume = (uword)vol;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void MacroFunc_WaitKeyUp(VoiceVars voice)
		{
			if (voice.KeyUp)
			{
				voice.Macro.Step++;
				playerInfo.MacroEvalAgain = true;
			}
			else
			{
				if (voice.Macro.Loop == 0)
				{
					voice.Macro.Loop = 0xff;

					voice.Macro.Step++;
					playerInfo.MacroEvalAgain = true;
				}
				else if (voice.Macro.Loop == 0xff)
					voice.Macro.Loop = (ubyte)(playerInfo.Cmd.Ee - 1);
				else
					voice.Macro.Loop--;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void MacroFunc_Goto(VoiceVars voice)
		{
			voice.Macro.OffsetSaved = voice.Macro.Offset;
			voice.Macro.StepSaved = voice.Macro.Step;

			MacroFunc_Cont(voice);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void MacroFunc_Return(VoiceVars voice)
		{
			voice.Macro.Offset = voice.Macro.OffsetSaved;
			voice.Macro.Step = voice.Macro.StepSaved;

			voice.Macro.Step++;
			playerInfo.MacroEvalAgain = true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void MacroFunc_SetPeriod(VoiceVars voice)
		{
			uword period = MyEndian.MakeWord(playerInfo.Cmd.Cd, playerInfo.Cmd.Ee);
			voice.Period = period;

			if (voice.Portamento.Speed == 0)
				voice.OutputPeriod = period;

			voice.Macro.Step++;
			playerInfo.MacroEvalAgain = true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void MacroFunc_SampleLoop(VoiceVars voice)
		{
			udword offset = MyEndian.MakeDword(0, playerInfo.Cmd.Bb, playerInfo.Cmd.Cd, playerInfo.Cmd.Ee);
			voice.Sample.Start += offset;
			voice.Sample.Length = (uword)(voice.Sample.Length - (offset >> 1));

			ToPaulaStart(voice, voice.Sample.Start);
			ToPaulaLength(voice, voice.Sample.Length);

			voice.Macro.Step++;
			playerInfo.MacroEvalAgain = true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void MacroFunc_OneShot(VoiceVars voice)
		{
			voice.AddBeginCount = 0;
			voice.Sample.Start = offsets.SampleData;
			voice.Sample.Length = 1;

			ToPaulaStart(voice, voice.Sample.Start);
			ToPaulaLength(voice, voice.Sample.Length);

			voice.Macro.Step++;
			playerInfo.MacroEvalAgain = true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void MacroFunc_WaitOnDma(VoiceVars voice)
		{
			PaulaVoice paulaVoice = paulaVoices[voice.VoiceNum];

			// The rarely used variant where 'cdee' arg is the number of waits
			voice.WaitOnDmaCount = (sword)MyEndian.MakeWord(playerInfo.Cmd.Cd, playerInfo.Cmd.Ee);
			voice.Macro.State = 0;
			voice.WaitOnDmaPrevLoops = paulaVoice.GetLoopCount();

			MacroFunc_ExtraWait(voice);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void MacroFunc_RandomPlay(VoiceVars voice)
		{
			voice.Rnd.Macro = playerInfo.Cmd.Bb;
			voice.Rnd.Speed = (sbyte)playerInfo.Cmd.Cd;
			voice.Rnd.Mode = playerInfo.Cmd.Ee;
			voice.Rnd.Count = 1;
			voice.Rnd.Flag = 1;

			RandomPlay(voice);

			voice.Rnd.BlockWait = true;

			voice.Macro.Step++;
			playerInfo.MacroEvalAgain = true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void MacroFunc_SplitKey(VoiceVars voice)
		{
			if (playerInfo.Cmd.Bb >= voice.Note)
				voice.Macro.Step++;
			else
				voice.Macro.Step = MyEndian.MakeWord(playerInfo.Cmd.Cd, playerInfo.Cmd.Ee);

			playerInfo.MacroEvalAgain = true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void MacroFunc_SplitVolume(VoiceVars voice)
		{
			if (playerInfo.Cmd.Bb >= voice.Volume)
				voice.Macro.Step++;
			else
				voice.Macro.Step = MyEndian.MakeWord(playerInfo.Cmd.Cd, playerInfo.Cmd.Ee);

			playerInfo.MacroEvalAgain = true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void MacroFunc_RandomMask(VoiceVars voice)
		{
			voice.Rnd.Mask = playerInfo.Cmd.Bb;

			voice.Macro.Step++;
			playerInfo.MacroEvalAgain = true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void MacroFunc_SetPrevNote(VoiceVars voice)
		{
			// Non-existent in TFMX v1, so add voice detune by default
			MacroFunc_AddNote_Sub(voice, voice.NotePrevious, voice.Detune);
			MacroFunc_ExtraWait(voice);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void MacroFunc_PlayMacro(VoiceVars voice)
		{
			playerInfo.Cmd.Aa = voice.Note;
			playerInfo.Cmd.Cd |= (ubyte)(voice.NoteVolume << 4);

			NoteCmd();

			voice.Macro.Step++;
			playerInfo.MacroEvalAgain = true;
		}



		/********************************************************************/
		/// <summary>
		/// SID setbeg
		/// </summary>
		/********************************************************************/
		private void MacroFunc_22(VoiceVars voice)
		{
			voice.AddBeginCount = 0;

			voice.Sid.SourceOffset = offsets.SampleData + MyEndian.MakeDword(0, playerInfo.Cmd.Bb, playerInfo.Cmd.Cd, playerInfo.Cmd.Ee);
			voice.Sample.Start = voice.Sid.SourceOffset;

			ToPaulaStart(voice, offsets.SampleData + voice.Sid.TargetOffset);

			voice.Macro.Step++;
			playerInfo.MacroEvalAgain = true;
		}



		/********************************************************************/
		/// <summary>
		/// SID setlen
		/// </summary>
		/********************************************************************/
		private void MacroFunc_23(VoiceVars voice)
		{
			uword len = MyEndian.MakeWord(playerInfo.Cmd.Aa, playerInfo.Cmd.Bb);
			if (len == 0)
				len = 0x100;

			ToPaulaLength(voice, (uword)(len >> 1));
			voice.Sid.TargetLength = (uword)((len - 1) & 0xff);

			uword len2 = MyEndian.MakeWord(playerInfo.Cmd.Cd, playerInfo.Cmd.Ee);
			voice.Sid.SourceLength = len2;
			voice.Sample.Length = len2;

			voice.Macro.Step++;
			playerInfo.MacroEvalAgain = true;
		}



		/********************************************************************/
		/// <summary>
		/// SID op3 ofs
		/// </summary>
		/********************************************************************/
		private void MacroFunc_24(VoiceVars voice)
		{
			voice.Sid.Op3.Offset = MyEndian.MakeDword(playerInfo.Cmd.Bb, playerInfo.Cmd.Cd, playerInfo.Cmd.Ee, 0);

			voice.Macro.Step++;
			playerInfo.MacroEvalAgain = true;
		}



		/********************************************************************/
		/// <summary>
		/// SID op3 frq
		/// </summary>
		/********************************************************************/
		private void MacroFunc_25(VoiceVars voice)
		{
			// SID op3 modifies SOURCE sample start offset
			voice.Sid.Op3.Speed = voice.Sid.Op3.Count = MyEndian.MakeWord(playerInfo.Cmd.Aa, playerInfo.Cmd.Bb);
			voice.Sid.Op3.Delta = (sword)MyEndian.MakeWord(playerInfo.Cmd.Cd, playerInfo.Cmd.Ee);

			voice.Macro.Step++;
			playerInfo.MacroEvalAgain = true;
		}



		/********************************************************************/
		/// <summary>
		/// SID op2 ofs
		/// </summary>
		/********************************************************************/
		private void MacroFunc_26(VoiceVars voice)
		{
			voice.Sid.Op2.Offset = MyEndian.MakeDword(0, playerInfo.Cmd.Bb, playerInfo.Cmd.Cd, playerInfo.Cmd.Ee);

			voice.Macro.Step++;
			playerInfo.MacroEvalAgain = true;
		}



		/********************************************************************/
		/// <summary>
		/// SID op2 frq
		/// </summary>
		/********************************************************************/
		private void MacroFunc_27(VoiceVars voice)
		{
			// SID op2 modifies step freq
			voice.Sid.Op2.Speed = voice.Sid.Op2.Count = MyEndian.MakeWord(playerInfo.Cmd.Aa, playerInfo.Cmd.Bb);
			voice.Sid.Op2.Delta = (sword)MyEndian.MakeWord(playerInfo.Cmd.Cd, playerInfo.Cmd.Ee);

			voice.Macro.Step++;
			playerInfo.MacroEvalAgain = true;
		}



		/********************************************************************/
		/// <summary>
		/// SID op1
		/// </summary>
		/********************************************************************/
		private void MacroFunc_28(VoiceVars voice)
		{
			// SID op1 modifies interpolation step
			voice.Sid.Op1.InterDelta = MyEndian.MakeWord(playerInfo.Cmd.Ee, (ubyte)(voice.Sid.Op1.InterDelta & 0xff));

			// Change the high-byte, keep the low-byte
			voice.Sid.Op1.InterMod = (sword)(16 * (sbyte)playerInfo.Cmd.Cd);
			voice.Sid.Op1.Speed = voice.Sid.Op1.Count = MyEndian.MakeWord(playerInfo.Cmd.Aa, playerInfo.Cmd.Bb);

			voice.Macro.Step++;
			playerInfo.MacroEvalAgain = true;
		}



		/********************************************************************/
		/// <summary>
		/// SID stop
		/// </summary>
		/********************************************************************/
		private void MacroFunc_29(VoiceVars voice)
		{
			voice.Macro.Step++;
			voice.Sid.TargetLength = 0;

			if (playerInfo.Cmd.Bb != 0)
			{
				voice.Sid.Op1.Speed = voice.Sid.Op1.Count = 0;
				voice.Sid.Op1.InterDelta = 0;
				voice.Sid.Op1.InterMod = 0;
				voice.Sid.Op2.Speed = voice.Sid.Op2.Count = 0;
				voice.Sid.Op2.Offset = 0;
				voice.Sid.Op2.Delta = 0;
				voice.Sid.Op3.Speed = voice.Sid.Op3.Count = 0;
				voice.Sid.Op3.Offset = 0;
				voice.Sid.Op3.Delta = 0;
			}

			playerInfo.MacroEvalAgain = true;
		}



		/********************************************************************/
		/// <summary>
		/// Macro command $30. Available in e.g. Gem'Z and Turrican III
		/// players, used also by Denny (unreleased game), but the way it's
		/// used, it only triggers in Gem'Z soundtrack, because on new note
		/// and Cont/Goto the boolean variable is reset.
		/// 
		/// If this voice uses the same macro script since last note, branch
		/// to specified macro position
		/// </summary>
		/********************************************************************/
		private void MacroFunc_BranchIfSame(VoiceVars voice)
		{
			playerInfo.MacroEvalAgain = true;

			if (voice.Macro.BranchIfSame)
				voice.Macro.Step = MyEndian.MakeWord(playerInfo.Cmd.Cd, playerInfo.Cmd.Ee);
			else
			{
				voice.Macro.BranchIfSame = true;
				voice.Macro.Step++;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Macro command $31. Available in e.g. Gem'Z and Turrican III
		/// players, but only used by T3 title
		/// </summary>
		/********************************************************************/
		private void MacroFunc_KeyUp(VoiceVars voice)
		{
			playerInfo.Cmd.Aa = 0xf5;	// Key up command
			NoteCmd();					// with cmd.cd = channel number

			voice.Macro.Step++;
			playerInfo.MacroEvalAgain = true;
		}
	}
}
