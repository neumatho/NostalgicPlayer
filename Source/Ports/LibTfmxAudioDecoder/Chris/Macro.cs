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
		/// 
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
		private void ProcessMacroMain(VoiceVars voice)
		{
			if (voice.Macro.Skip)
				return;

			if (voice.Macro.Wait > 0)
			{
				voice.Macro.Wait--;
				return;
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
			paulaVoice.On();
			voice.EffectsMode = (sbyte)playerInfo.Cmd.Bb;

			voice.Macro.Step++;
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

			playerInfo.MacroEvalAgain = true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void MacroFunc_Stop(VoiceVars voice)
		{
			voice.Macro.Skip = true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void MacroFunc_AddNote(VoiceVars voice)
		{
			MacroFunc_AddNote_Sub(voice, voice.Note);
			MacroFunc_ExtraWait(voice);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void MacroFunc_AddNote_Sub(VoiceVars voice, ubyte noteAdd)
		{
			sbyte n = (sbyte)(noteAdd + (sbyte)playerInfo.Cmd.Bb);
			uword p = NoteToPeriod(n);
			sword finetune = (sword)(voice.Detune + (sword)MyEndian.MakeWord(playerInfo.Cmd.Cd, playerInfo.Cmd.Ee));

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
			MacroFunc_AddNote_Sub(voice, 0);
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
			if (playerInfo.Cmd.Cd == 0xfe)
			{
				ubyte ee = playerInfo.Cmd.Ee;
				playerInfo.Cmd.Cd = playerInfo.Cmd.Ee = 0;

				MacroFunc_AddNote_Sub(voice, voice.Note);

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

				MacroFunc_AddNote_Sub(voice, voice.Note);

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
			// If count is non-zero, that's extra behaviour for any TFMX
			// similar to TFMX Pro. Old TFMX doesn't use that, so it can
			// become the default
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
			paulaVoice.Off();

			// Rare variants of TFMX implement it as a count value, but no module
			// sets the value to anything above 1
			if (playerInfo.Cmd.Bb != 0)
				voice.Macro.ExtraWait = false;
			else
				playerInfo.MacroEvalAgain = true;

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
			voice.Macro.Skip = true;
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
			voice.Rnd.BlockWait = true;

			RandomPlay(voice);

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
			MacroFunc_AddNote_Sub(voice, voice.NotePrevious);
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
	}
}
