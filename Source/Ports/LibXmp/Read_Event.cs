/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Drawing.Imaging.Effects;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Common;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Player;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp;

namespace Polycode.NostalgicPlayer.Ports.LibXmp
{
	/// <summary>
	/// 
	/// </summary>
	internal partial class Player
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private Xmp_SubInstrument Get_SubInstrument(c_int ins, c_int key)
		{
			Module_Data m = ctx.M;
			Xmp_Module mod = m.Mod;

			if (Is_Valid_Instrument(mod, ins))
			{
				Xmp_Instrument instrument = mod.Xxi[ins];

				if (Is_Valid_Note(key))
				{
					c_int mapped = instrument.Map[key].Ins;
					if ((mapped != 0xff) && (mapped >= 0) && (mapped < instrument.Nsm))
						return instrument.Sub[mapped];
				}
				else
				{
					if (mod.Xxi[ins].Nsm > 0)
						return instrument.Sub[0];
				}
			}

			return null;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Reset_Envelopes(Channel_Data xc)
		{
			Module_Data m = ctx.M;
			Xmp_Module mod = m.Mod;

			if (!Is_Valid_Instrument(mod, xc.Ins))
				return;

			Reset_Note(xc, Note_Flag.Env_End);

			xc.V_Idx = -1;
			xc.P_Idx = -1;
			xc.F_Idx = -1;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Reset_Envelope_Volume(Channel_Data xc)
		{
			Module_Data m = ctx.M;
			Xmp_Module mod = m.Mod;

			if (!Is_Valid_Instrument(mod, xc.Ins))
				return;

			Reset_Note(xc, Note_Flag.Env_End);

			xc.V_Idx = -1;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Reset_Envelopes_Carry(Channel_Data xc)
		{
			Module_Data m = ctx.M;
			Xmp_Module mod = m.Mod;

			if (!Is_Valid_Instrument(mod, xc.Ins))
				return;

			Reset_Note(xc, Note_Flag.Env_End);

			Xmp_Instrument xxi = lib.sMix.LibXmp_Get_Instrument(xc.Ins);

			// Reset envelope positions
			if ((~xxi.Aei.Flg & Xmp_Envelope_Flag.Carry) != 0)
				xc.V_Idx = -1;

			if ((~xxi.Pei.Flg & Xmp_Envelope_Flag.Carry) != 0)
				xc.P_Idx = -1;

			if ((~xxi.Fei.Flg & Xmp_Envelope_Flag.Carry) != 0)
				xc.F_Idx = -1;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Set_Effect_Defaults(c_int note, Xmp_SubInstrument sub, Channel_Data xc, bool is_TonePorta)
		{
			Module_Data m = ctx.M;

			if ((sub != null) && (note >= 0))
			{
				if (!Common.Has_Quirk(m, Quirk_Flag.ProTrack))
					xc.FineTune = sub.Fin;

				xc.Gvl = sub.Gvl;

				if ((sub.Ifc & 0x80) != 0)
					xc.Filter.CutOff = (sub.Ifc - 0x80) * 2;

				xc.Filter.Envelope = 0x100;

				if ((sub.Ifr & 0x80) != 0)
					xc.Filter.Resonance = (sub.Ifr - 0x80) * 2;

				// IT: On a new note without toneporta, allow a computed cutoff
				// of 127 with resonance 0 to disable the filter
				xc.Filter.Can_Disable = !is_TonePorta;

				// TODO: Should probably expand the LFO period size instead
				// of reducing the vibrato rate precision here
				lib.lfo.LibXmp_Lfo_Set_Depth(xc.InsVib.Lfo, sub.Vde);
				lib.lfo.LibXmp_Lfo_Set_Rate(xc.InsVib.Lfo, (sub.Vra + 2) >> 2);
				lib.lfo.LibXmp_Lfo_Set_Waveform(xc.InsVib.Lfo, sub.Vwf);
				xc.InsVib.Sweep = sub.Vsw;

				lib.lfo.LibXmp_Lfo_Set_Phase(xc.Vibrato.Lfo, 0);
				lib.lfo.LibXmp_Lfo_Set_Phase(xc.Tremolo.Lfo, 0);
			}

			xc.Delay = 0;
			xc.Tremor.Up = xc.Tremor.Down = 0;

			// Reset arpeggio
			xc.Arpeggio.Val[0] = 0;
			xc.Arpeggio.Count = 0;
			xc.Arpeggio.Size = 1;

			// Reset toneporta--each libxmp_process_fx may add to the rate
			if (is_TonePorta)
				xc.Porta.Slide = 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Set_Channel_Volume(Channel_Data xc, c_int vol)
		{
			if (vol >= 0)
			{
				xc.Volume = vol;
				Set(xc, Channel_Flag.New_Vol);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Set_Channel_Pan(Channel_Data xc, c_int pan)
		{
			// TODO: LIQ supports surround for default panning, unclear if it works.
			// TODO: Imago Orpheus only temporarily sets channel panning for
			//       instrument numbers with no note
			if (pan >= 0)
			{
				xc.Pan.Val = pan;
				xc.Pan.Surround = false;
			}
		}



		/********************************************************************/
		/// <summary>
		/// From OpenMPT PortaTarget.mod:
		/// "A new note (with no portamento command next to it) does not
		/// reset the portamento target. That is, if a previous portamento
		/// has not finished yet, calling 3xx or 5xx after the new note will
		/// slide it towards the old target. Once the portamento target
		/// period is reached, the target is reset. This means that if the
		/// period is modified by another slide (e.g. 1xx or 2xx), a
		/// following 3xx will not slide back to the original target."
		/// </summary>
		/********************************************************************/
		private void Set_Period(c_int note, Xmp_SubInstrument sub, Channel_Data xc, bool is_TonePorta)
		{
			Module_Data m = ctx.M;

			// TODO: blocking period updates on whether or not the event has a
			//       valid instrument seems suspicious, but almost every format uses
			//       this. Only allow Protracker to update without it for now
			if ((sub == null) && !Common.Has_Quirk(m, Quirk_Flag.ProTrack))
				return;

			if (note >= 0)
			{
				c_double per = lib.period.LibXmp_Note_To_Period(note, xc.FineTune, xc.Per_Adj);

				if (!Common.Has_Quirk(m, Quirk_Flag.ProTrack) || ((note > 0) && is_TonePorta))
					xc.Porta.Target = per;

				if ((xc.Period < 1) || !is_TonePorta)
					xc.Period = per;
			}
		}



		/********************************************************************/
		/// <summary>
		/// From OpenMPT Porta-Pickup.xm:
		/// "An instrument number should not reset the current portamento
		/// target. The portamento target is valid until a new target is
		/// specified by combining a note and a portamento effect."
		/// </summary>
		/********************************************************************/
		private void Set_Period_Ft2(c_int key, c_int note, Xmp_SubInstrument sub, Channel_Data xc, bool is_TonePorta)
		{
			if (Is_Valid_Note(key - 1) && is_TonePorta)
			{
				// Toneporta target updates even for invalid instruments, using
				// the default transpose +0 (ft2_invalid_porta_target.xm)
				c_int n = key - 1;

				if (sub != null)
				{
					n += ctx.M.Mod.Xxi[xc.Ins].Map[key - 1].Xpo;
					n += sub.Xpo;
				}

				xc.Porta.Target = lib.period.LibXmp_Note_To_Period(n, xc.FineTune, xc.Per_Adj);
			}

			if ((sub != null) && (note >= 0))
			{
				if ((xc.Period < 1) || !is_TonePorta)
					xc.Period = lib.period.LibXmp_Note_To_Period(note, xc.FineTune, xc.Per_Adj);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private bool Is_Sfx_Pitch(byte x)
		{
			return (x == Effects.Fx_Pitch_Add) || (x == Effects.Fx_Pitch_Sub);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private bool Is_TonePorta(byte x)
		{
			return (x == Effects.Fx_TonePorta) || (x == Effects.Fx_Tone_VSlide) || (x == Effects.Fx_Per_TPorta) || (x == Effects.Fx_Ult_TPorta) || (x == Effects.Fx_Far_TPorta);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private bool Is_Mod_Retrig(byte x, byte p)
		{
			return (x == Effects.Fx_Extended) && (Common.Msn(p) == Effects.Ex_Retrig) && (Common.Lsn(p) != 0);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Set_Patch(c_int chn, c_int ins, c_int smp, c_int note)
		{
			lib.virt.LibXmp_Virt_SetPatch(chn, ins, smp, note, 0, 0, 0, 0);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Read_Event_Mod(Xmp_Event e, c_int chn)
		{
			Player_Data p = ctx.P;
			Module_Data m = ctx.M;
			Xmp_Module mod = m.Mod;
			Channel_Data xc = p.Xc_Data[chn];
			Xmp_SubInstrument sub = null;
			bool new_Invalid_Ins = false;
			bool new_Swap_Ins = false;

			xc.Flags = Channel_Flag.None;
			c_int note = -1;
			bool is_TonePorta = false;
			bool is_Retrig = false;

			if (Is_TonePorta(e.FxT) || Is_TonePorta(e.F2T))
				is_TonePorta = true;

			if (Is_Mod_Retrig(e.FxT, e.FxP) || Is_Mod_Retrig(e.F2T, e.F2P))
				is_Retrig = true;

			// Check instrument
			if (Is_Valid_Note(e.Note - 1) && !is_TonePorta)
				xc.Key = e.Note - 1;

			if (e.Ins != 0)
			{
				c_int ins = e.Ins - 1;

				Set(xc, Channel_Flag.New_Ins);
				xc.FadeOut = 0x10000;		// For painlance.mod pat 0 ch 3 echo

				// TODO: FunkTracker instruments probably don't reset
				// effects, investigate: fnk_note_vslide_cancel.fnk

				xc.Per_Flags = Channel_Flag.None;
				xc.Offset.Val = 0;
				Reset_Note(xc, Note_Flag.Release | Note_Flag.FadeOut);

				if (Is_Valid_Instrument(mod, ins))
				{
					sub = Get_SubInstrument(ins, xc.Key);

					if (sub != null)
					{
						new_Swap_Ins = true;

						// Finetune is always loaded, but only applies
						// when the period is updated by a note/porta
						// (OpenMPT finetune.mod, PortaSwapPT.mod)
						if (Common.Has_Quirk(m, Quirk_Flag.ProTrack))
						{
							xc.FineTune = sub.Fin;
							xc.Ins = ins;
						}

						// Dennis Lindroos: instrument volume
						// is not used on split channels
						if ((xc.Split == 0) && (e.Note != Constants.Xmp_Key_Off))
						{
							Set_Channel_Volume(xc, sub.Vol);
							Set_Channel_Pan(xc, sub.Pan);
						}
					}

					if (!is_TonePorta)
					{
						xc.Ins = ins;
						xc.Ins_Fade = mod.Xxi[ins].Rls;
					}
				}
				else
				{
					new_Invalid_Ins = true;

					// Invalid instruments do not reset the channel in
					// Protracker; instead, they set the current sample
					// to the invalid sample, which stops the current
					// sample at the end of its loop.
					//
					// OpenMPT PTInstrSwap.mod: uses a null sample to pause
					// a looping sample, plays several on a channel with no note.
					//
					// OpenMPT PTSwapEmpty.mod: repeatedly pauses and
					// restarts a sample using a null sample
					if (!Common.Has_Quirk(m, Quirk_Flag.ProTrack) || is_Retrig)
						lib.virt.LibXmp_Virt_ResetChannel(chn);
					else
						lib.virt.LibXmp_Virt_QueuePatch(chn, -1, -1, 0);
				}
			}

			// Check note
			if (e.Note != 0)
			{
				Set(xc, Channel_Flag.New_Note);

				// FunkTracker - new notes cancel persistent volume slide.
				// Farandole Composer notes are always paired with volume,
				// so this doesn't notably affect it
				Reset_Per(xc, Channel_Flag.Vol_Slide);

				if (e.Note == Constants.Xmp_Key_Off)
				{
					Set_Note(xc, Note_Flag.Release);
				}
				else if (!is_TonePorta && Is_Valid_Note(e.Note - 1))
				{
					Reset_Note(xc, Note_Flag.End);

					sub = Get_SubInstrument(xc.Ins, xc.Key);

					if (sub != null)
					{
						c_int transp = mod.Xxi[xc.Ins].Map[xc.Key].Xpo;

						note = xc.Key + sub.Xpo + transp;
						c_int smp = sub.Sid;

						if (new_Invalid_Ins || !Is_Valid_Sample(mod, smp))
							smp = -1;

						if ((smp >= 0) && (smp < mod.Smp))
						{
							Set_Patch(chn, xc.Ins, smp, note);
							new_Swap_Ins = false;
							xc.Smp = smp;
						}
					}
					else
					{
						xc.Flags = Channel_Flag.None;
						note = xc.Key;
					}
				}

				if (note >= 0)
				{
					xc.Note = note;
					Set_Note(xc, Note_Flag.Set);
				}
			}

			// Protracker 1/2 sample swap occurs when a sample number is
			// encountered without a note or with a note and toneporta. The new
			// instrument is switched to when the current sample reaches its loop
			// end. A valid note must have been played in this channel before.
			//
			// Empty samples can also be set, which stops the sample at the end
			// of its loop (see above)
			if (new_Swap_Ins && (sub != null) && Common.Has_Quirk(m, Quirk_Flag.ProTrack) && Test_Note(xc, Note_Flag.Set))
			{
				lib.virt.LibXmp_Virt_QueuePatch(chn, e.Ins - 1, sub.Sid, xc.Note);
				xc.Smp = sub.Sid;
			}

			// sub is now the currently playing subinstrument, which may not be
			// related to e->ins if there is active toneporta!
			sub = Get_SubInstrument(xc.Ins, xc.Key);

			Set_Effect_Defaults(note, sub, xc, is_TonePorta);
			if ((e.Ins != 0) && (sub != null))
				Reset_Envelopes(xc);

			// Process new volume
			Set_Channel_Volume(xc, e.Vol - 1);

			if (e.Vol != 0)
			{
				// Farandole Composer - volume resets slide to volume
				Reset_Per(xc, Channel_Flag.Vol_Slide);
			}

			// Secondary effect handled first
			LibXmp_Process_Fx(xc, chn, e, 1);
			LibXmp_Process_Fx(xc, chn, e, 0);

			if (Is_Sfx_Pitch(e.FxT))
				xc.Period = lib.period.LibXmp_Note_To_Period(note, xc.FineTune, xc.Per_Adj);
			else
				Set_Period(note, sub, xc, is_TonePorta);

			if (sub == null)
				return 0;

			if ((note >= 0) && !new_Invalid_Ins)
				lib.virt.LibXmp_Virt_VoicePos(chn, xc.Offset.Val);
			else if (new_Swap_Ins && is_Retrig && Common.Has_Quirk(m, Quirk_Flag.ProTrack))
			{
				// Protracker: an instrument number with no note and retrigger
				// triggers the new sample on tick 0. Other effects that set
				// RETRIG should not. (OpenMPT InstrSwapRetrigger.mod)
				lib.virt.LibXmp_Virt_VoicePos(chn, 0);
			}

			if (Test(xc, Channel_Flag.Offset))
			{
				if (Common.Has_Quirk(m, Quirk_Flag.ProTrack) || ((p.Flags & Xmp_Flags.Fx9Bug) != 0))
					xc.Offset.Val += xc.Offset.Val2;

				Reset(xc, Channel_Flag.Offset);
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private bool Sustain_Check(Xmp_Envelope env, c_int idx)
		{
			return (env != null) && ((env.Flg & Xmp_Envelope_Flag.On) != 0) && ((env.Flg & Xmp_Envelope_Flag.Sus) != 0) && ((~env.Flg & Xmp_Envelope_Flag.Loop) != 0) && (idx == env.Data[env.Sus << 1]);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Read_Event_Ft2(Xmp_Event e, c_int chn)
		{
			Player_Data p = ctx.P;
			Module_Data m = ctx.M;
			Xmp_Module mod = m.Mod;
			Channel_Data xc = p.Xc_Data[chn];
			Xmp_SubInstrument sub;
			Xmp_Event ev = new Xmp_Event();

			// From the OpenMPT DelayCombination.xm test case:
			// "Naturally, Fasttracker 2 ignores notes next to an out-of-range
			// note delay. However, to check whether the delay is out of range,
			// it is simply compared against the current song speed, not taking
			// any pattern delays into account."
			if (p.Frame >= p.Speed)
				return 0;

			ev.CopyFrom(e);

			xc.Flags = Channel_Flag.None;
			c_int note = -1;
			c_int key = ev.Note;
			bool is_TonePorta = false;
			bool is_Delayed = false;

			// Delay has a few bizarre hacks that need to be supported
			if ((ev.FxT == Effects.Fx_Extended) && (Common.Msn(ev.FxP) == Effects.Ex_Delay) && (Common.Lsn(ev.FxP) != 0))
			{
				// No note + delay -> note memory (ft2_delay_note_memory.xm).
				// Combined with ins# memory, effectively causes a retrigger
				key = key != 0 ? key : xc.Key_Memory != 0 ? xc.Key_Memory : xc.Key + 1;

				// Key off + no ins# + delay + volume column pan -> ignore pan
				// (OpenMPT PanOff.xm) (ft2_delay_volume_column.xm)
				if (Common.Has_Quirk(m, Quirk_Flag.Ft2Bugs) && (key == Constants.Xmp_Key_Off) && (ev.Ins == 0) && (ev.F2T == Effects.Fx_SetPan))
					ev.F2T = ev.F2P = 0;

				is_Delayed = true;
			}

			if (Is_TonePorta(ev.FxT) || Is_TonePorta(ev.F2T))
			{
				is_TonePorta = true;

				// Mx + 3xx/5xy applies toneporta for both commands, but 3xx
				// uses the rate from the volume slot (ft2_double_toneporta.xm,
				// OpenMPT TonePortamentoMemory.xm)
				if (Common.Has_Quirk(m, Quirk_Flag.Ft2Bugs) && (ev.FxT == Effects.Fx_TonePorta) && Is_TonePorta(ev.F2T))
					ev.FxP = 0;
			}

			// FT2 deletes K00 and, if there is no volume fx toneporta, overwrites
			// the note with keyoff (ft2_k00_is_note_off.xm, OpenMPT key_off.xm,
			// ft2_envelope_reset.xm)
			if ((ev.FxT == Effects.Fx_Keyoff) && (ev.FxP == 0))
			{
				ev.FxT = 0;

				if (!is_TonePorta)
					key = Constants.Xmp_Key_Off;
			}

			// Check instrument
			// 
			// Only update instrument/sample on new valid note + no toneporta/K00.
			// Lamb/forgotten city.xm relies heavily on quirks here
			if (ev.Ins != 0)
			{
				Set(xc, Channel_Flag.New_Ins);
				xc.Per_Flags = Channel_Flag.None;	// For posterity; not used by XM
			}

			if (Is_Valid_Note(key - 1) && !is_TonePorta)
			{
				Xmp_Instrument xxi = null;

				// Note w/o instrument loads the last referenced instrument
				c_int ins = ev.Ins != 0 ? ev.Ins : xc.Old_Ins;
				c_int smp = -1;
				c_int n = key - 1;

				// Updates on note + !toneporta + !K00 and is unaffected by
				// out-of-range transposition checks (ft2_note_range.xm)
				xc.Key_Memory = key;

				// Unused instruments have fade 0x80, no envelopes, no vibrato.
				// Unused samples have volume 0, pan 0x80, transpose 0, no data
				// libxmp represents unused/invalid instruments/samples as -1
				// (test_player_ft2_note_off_after_invalid_ins)
				if (Is_Valid_Instrument(mod, ins - 1))
				{
					xxi = mod.Xxi[ins - 1];
					sub = lib.player.Get_SubInstrument(ins - 1, key - 1);

					if (sub != null)
					{
						n += xxi.Map[key - 1].Xpo;
						n += sub.Xpo;
						smp = sub.Sid;
					}
				}

				// TODO: out-of-range notes update envelopes, no ins.# req

				// Fade update requires ins.# (ft2_instrument_fade_update.xm).
				// Out-of-range notes update (ft2_note_range_instrument_fade.xm)
				if (ev.Ins != 0)
					xc.Ins_Fade = xxi != null ? xxi.Rls : 0x80 /* FT2 default */ << 1 /* conv */;

				// Valid notes update the instrument, sample, key, and note.
				// Note B-(-1) updates key/instrument/sample, but not the note.
				// Notes >A#9, <B-(-1) act as if the key does not exist at all.
				// (ft2_note_range.xm, OpenMPT NoteLimit.xm/NoteLimit2.xm)
				if ((n >= Constants.Ft2_Note_Bn1) && (n <= Constants.Ft2_Note_As9))
				{
					if (n >= Constants.Ft2_Note_C0)
						xc.Note = n;

					xc.Key = key - 1;
					xc.Ins = Is_Valid_Instrument(mod, ins - 1) ? ins - 1 : -1;
					xc.Smp = Is_Valid_Sample(mod, smp) ? smp : -1;
				}
				else
					key = 0;
			}

			// Get the new instrument. If the instrument/key wasn't updated, this
			// is equivalent to FT2 retaining the previous instrument/sample
			sub = Get_SubInstrument(xc.Ins, xc.Key);

			// Check note
			//
			// Do not send a new note for toneporta (Quazar/funky stars.xm pos 5
			// ch 9, Mark Birch/comic bakery remix.xm pos 1 ch 3)
			if (key != 0)
				Set(xc, Channel_Flag.New_Note);

			if (Is_Valid_Note(key - 1) && !is_TonePorta)
			{
				Reset_Note(xc, Note_Flag.End);

				// Send note even if the current sample is invalid.
				// Playing with an active invalid sample cuts the channel:
				// invalid instrument, valid instrument with invalid subins, or
				// zero-length sample (test_player_ft2_invalid_ins_defaults)
				Set_Patch(chn, xc.Ins, xc.Smp, xc.Note);

				note = xc.Note;
			}

			// Check key off/envelopes

			if (key != 0)
				Set(xc, Channel_Flag.New_Note);

			if (key == Constants.Xmp_Key_Off)
			{
				Xmp_Envelope env = null;

				if (Is_Valid_Instrument(mod, xc.Ins))
					env = mod.Xxi[xc.Ins].Aei;

				if ((env != null) && ((env.Flg & Xmp_Envelope_Flag.On) != 0))
				{
					if (Sustain_Check(env, xc.V_Idx))
					{
						// See OpenMPT EnvOff.xm. In certain
						// cases a release event is effective
						// only in the next frame
						Set_Note(xc, Note_Flag.SusExit);
					}
					else
						Set_Note(xc, Note_Flag.Release);
				}
				else
				{
					// No volume envelope -> cut volume to 0
					// (ft2_note_off_fade.xm, OpenMPT NoteOffVolume.xm)
					Set_Channel_Volume(xc, 0);
				}

				// Keyoff always begins fadeout (ft2_note_off_fade.xm)
				Set_Note(xc, Note_Flag.FadeOut);
			}

			if (((ev.Ins != 0) && (key != Constants.Xmp_Key_Off)) || is_Delayed)
			{
				// Reset release/fadeout for instrument numbers with no keyoff/K00
				// (xyce-dans_la_rue.xm chn 0 pat. 0E/0F, chn 10 pat. 0D/0E;
				// ft2_k00_defaults.xm; ft2_note_off_sustain.xm), and on
				// delayed rows (ft2_delay_envelope_*.xm, ft2_note_off_fade.xm,
				// OpenMPT NoteOff.xm). Other cases like note w/o ins# don't
				// reset fadeout (Cave Story - Last Battle.xm pos 11 chn 2,
				// ft2_note_no_fadeout_reset.xm)
				xc.FadeOut = 0x10000;
				Reset_Note(xc, Note_Flag.FadeOut);
				Reset_Note(xc, Note_Flag.Release | Note_Flag.SusExit);

				if (sub != null)
				{
					// Only reset envelopes with a valid active instrument
					// (ft2_envelope_invalid_ins.xm)
					// (Ghidorah/olympic dance.xm pos 10)
					// (Jeroen Tel/letting go.xm pos 4 chn 20)
					Reset_Envelopes(xc);
				}

				// Tremor count resets with fadeout (ft2_tremor_reset.xm)
				xc.Tremor.Count = Tremor_Flag.Suppress;
			}

			// TODO: this function needs checking, probably split
			Set_Effect_Defaults(note, sub, xc, is_TonePorta);

			if (ev.Ins != 0)
			{
				// Any ins.#: use active sample for defaults. Invalid samples
				// have volume 0 panning 0x80 (ft2_invalid_ins_defaults.xm).
				// Works on lines with K00 (ft2_k00_defaults.xm)
				Set_Channel_Volume(xc, sub != null ? sub.Vol : 0);
				Set_Channel_Pan(xc, sub != null ? sub.Pan : 0x80);
			}

			// Process new volume
			Set_Channel_Volume(xc, ev.Vol - 1);

			// FT2: Always reset sample offset
			xc.Offset.Val = 0;

			// Secondary effect handled first
			LibXmp_Process_Fx(xc, chn, ev, 1);
			LibXmp_Process_Fx(xc, chn, ev, 0);
			Set_Period_Ft2(key, note, sub, xc, is_TonePorta);

			// TODO: Modplug, MT2, rstST process some FX every tick (affects toneporta memory)

			if (Test(xc, Channel_Flag.New_Vol))
			{
				// Tremor is reset by ins# without keyoff or by delay rows.
				// Other events that set volume (volume column/Cxx, keyoff+ins#)
				// also temporarily override tremor, but don't reset it.
				// (Tremor likely just overwrites the channel volume in FT2.)
				// (ft2_tremor_reset.xm, OpenMPT TremorRecover.xm)
				xc.Tremor.Count |= Tremor_Flag.Suppress;
			}

			if (note >= 0)
			{
				// Sample offset requires valid note + 9xx + !toneporta
				// (Decibelter/Cosmic 'Wegian Mamas.xm pattern 4 channel 8).
				// In FT2, memory is set ONLY in these cases, and offsets past
				// the end of the sample cut. (Skale Tracker is different, so
				// limit this to QUIRK_FT2BUGS; reported by Vladislav Suschikh.)
				// (ft2_offset_memory.xm, OpenMPT 3xx-no-old-samp.xm/porta-offset.xm)
				if (Common.Has_Quirk(m, Quirk_Flag.Ft2Bugs) && Test(xc, Channel_Flag.Offset))
				{
					xc.Offset.Memory = (xc.Offset.Val & 0xff00) >> 8;

					if (!Is_Valid_Sample(mod, xc.Smp) || (xc.Offset.Val >= mod.Xxs[xc.Smp].Len))
						lib.virt.LibXmp_Virt_ResetChannel(chn);
				}

				lib.virt.LibXmp_Virt_VoicePos(chn, xc.Offset.Val);
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Read_Event_St3(Xmp_Event e, c_int chn)
		{
			Player_Data p = ctx.P;
			Module_Data m = ctx.M;
			Xmp_Module mod = m.Mod;
			Channel_Data xc = p.Xc_Data[chn];
			Xmp_SubInstrument sub;

			xc.Flags = Channel_Flag.None;
			c_int note = -1;
			bool not_Same_Ins = false;
			bool is_TonePorta = false;

			if (Is_TonePorta(e.FxT) || Is_TonePorta(e.F2T))
				is_TonePorta = true;

			if ((lib.virt.LibXmp_Virt_MapChannel(chn) < 0) && (xc.Ins != (e.Ins - 1)))
				is_TonePorta = false;

			// Check instrument
			if (Is_Valid_Note(e.Note - 1) && !is_TonePorta)
				xc.Key = e.Note - 1;

			if (e.Ins != 0)
			{
				c_int ins = e.Ins - 1;
				Set(xc, Channel_Flag.New_Ins);
				xc.FadeOut = 0x10000;
				xc.Per_Flags = Channel_Flag.None;
				xc.Offset.Val = 0;
				Reset_Note(xc, Note_Flag.Release | Note_Flag.FadeOut);

				if (Is_Valid_Instrument(mod, ins))
				{
					if (xc.Ins != ins)
					{
						not_Same_Ins = true;

						if (!is_TonePorta)
						{
							xc.Ins = ins;
							xc.Ins_Fade = mod.Xxi[ins].Rls;
						}
					}

					// Get new instrument volume
					sub = Get_SubInstrument(ins, xc.Key);

					if ((sub != null) && (e.Note != Constants.Xmp_Key_Off))
					{
						Set_Channel_Volume(xc, sub.Vol);
						Set_Channel_Pan(xc, sub.Pan);
					}
				}
				else
				{
					// Ignore invalid instruments
					xc.Flags = Channel_Flag.None;
				}
			}

			// Check note
			if (e.Note != 0)
			{
				Set(xc, Channel_Flag.New_Note);

				if (e.Note == Constants.Xmp_Key_Off)
				{
					Set_Note(xc, Note_Flag.Release);
				}
				else if (is_TonePorta)
				{
					// Always retrig in tone portamento: Fix portamento in
					// 7spirits.s3m, mod.Biomechanoid
					if (not_Same_Ins)
						xc.Offset.Val = 0;
				}
				else if (Is_Valid_Note(e.Note - 1))
				{
					Reset_Note(xc, Note_Flag.End);

					sub = Get_SubInstrument(xc.Ins, xc.Key);

					if (sub != null)
					{
						c_int transp = mod.Xxi[xc.Ins].Map[xc.Key].Xpo;

						note = xc.Key + sub.Xpo + transp;
						c_int smp = sub.Sid;

						if (!Is_Valid_Sample(mod, smp))
							smp = -1;

						if ((smp >= 0) && (smp < mod.Smp))
						{
							Set_Patch(chn, xc.Ins, smp, note);
							xc.Smp = smp;
						}
					}
					else
						xc.Flags = Channel_Flag.None;
				}
			}

			// sub is now the currently playing subinstrument, which may not be
			// related to e->ins if there is active toneporta!
			sub = Get_SubInstrument(xc.Ins, xc.Key);

			Set_Effect_Defaults(note, sub, xc, is_TonePorta);
			if ((e.Ins != 0) && (sub != null))
				Reset_Envelopes(xc);

			// Process new volume
			Set_Channel_Volume(xc, e.Vol - 1);

			// Secondary effect handled first
			LibXmp_Process_Fx(xc, chn, e, 1);
			LibXmp_Process_Fx(xc, chn, e, 0);
			Set_Period(note, sub, xc, is_TonePorta);

			// TODO: Orpheus processes some FX every tick (affects toneporta memory);
			// toneporta is additive only if both values are the same (otherwise,
			// use the value from slot 2)

			if (sub == null)
				return 0;

			if (note >= 0)
			{
				xc.Note = note;
				lib.virt.LibXmp_Virt_VoicePos(chn, xc.Offset.Val);
			}

			if (Common.Has_Quirk(m, Quirk_Flag.St3Bugs) && Test(xc, Channel_Flag.New_Vol))
				xc.Volume = xc.Volume * p.GVol / m.VolBase;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Copy_Channel(Player_Data p, c_int to, c_int from)
		{
			if ((to > 0) && (to != from))
				p.Xc_Data[to].CopyFrom(p.Xc_Data[from]);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private bool Check_FadeOut(Channel_Data xc, c_int ins)
		{
			Xmp_Instrument xxi = lib.sMix.LibXmp_Get_Instrument(ins);

			if (xxi == null)
				return true;

			return ((~xxi.Aei.Flg & Xmp_Envelope_Flag.On) != 0) || ((~xxi.Aei.Flg & Xmp_Envelope_Flag.Carry) != 0) || (xc.Ins_Fade == 0) || (xc.FadeOut <= xc.Ins_Fade);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private bool Check_Invalid_Sample(c_int ins, c_int key)
		{
			Module_Data m = ctx.M;
			Xmp_Module mod = m.Mod;

			if (ins < mod.Ins)
			{
				c_int smp = mod.Xxi[ins].Map[key].Ins;
				if ((smp == 0xff) || (smp >= mod.Smp))
					return true;
			}

			return false;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Fix_Period(c_int chn, Xmp_SubInstrument sub)
		{
			if (sub.Nna == Xmp_Inst_Nna.Cont)
			{
				Player_Data p = ctx.P;
				Channel_Data xc = p.Xc_Data[chn];
				Xmp_Instrument xxi = lib.sMix.LibXmp_Get_Instrument(xc.Ins);

				xc.Period = lib.period.LibXmp_Note_To_Period(xc.Key + sub.Xpo + xxi.Map[xc.Key_Porta].Xpo, xc.FineTune, xc.Per_Adj);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private bool Is_Same_Sid(c_int chn, c_int ins, c_int key)
		{
			Player_Data p = ctx.P;
			Channel_Data xc = p.Xc_Data[chn];

			Xmp_SubInstrument s1 = Get_SubInstrument(ins, key);
			Xmp_SubInstrument s2 = Get_SubInstrument(xc.Ins, xc.Key);

			return (s1 != null) && (s2 != null) && (s1.Sid == s2.Sid);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Read_Event_It(Xmp_Event e, c_int chn)
		{
			Player_Data p = ctx.P;
			Module_Data m = ctx.M;
			Xmp_Module mod = m.Mod;
			Channel_Data xc = p.Xc_Data[chn];
			Xmp_SubInstrument sub;

			Xmp_Event ev = new Xmp_Event();
			ev.CopyFrom(e);

			// Emulate Impulse Tracker "always read instrument" bug
			if (ev.Ins != 0)
				xc.Delayed_Ins = 0;
			else if ((ev.Note != 0) && (xc.Delayed_Ins != 0))
			{
				ev.Ins = (byte)xc.Delayed_Ins;
				xc.Delayed_Ins = 0;
			}

			xc.Flags = Channel_Flag.None;
			c_int note = -1;
			c_int key = ev.Note;
			bool not_Same_Ins = false;
			bool not_Same_Smp = false;
			bool new_Invalid_Ins = false;
			bool is_TonePorta = false;
			bool is_Release = false;
			bool reset_Env = false;
			bool reset_SusLoop = false;
			bool use_Ins_Vol = false;
			c_int candidate_Ins = xc.Ins;
			bool sample_Mode = !Common.Has_Quirk(m, Quirk_Flag.Virtual);
			bool tonePorta_Offset = false;
			bool retrig_Ins = false;

			// Keyoff + instrument retrigs current instrument in old fx mode
			if (Common.Has_Quirk(m, Quirk_Flag.ItOldFx))
			{
				if ((ev.Note == Constants.Xmp_Key_Off) && Is_Valid_Instrument(mod, ev.Ins - 1))
					retrig_Ins = true;
			}

			// Notes with unmapped instruments are ignored
			if (ev.Ins != 0)
			{
				if ((ev.Ins <= mod.Ins) && Is_Valid_Note(ev.Note - 1))
				{
					c_int ins = ev.Ins - 1;

					if (Check_Invalid_Sample(ins, ev.Note - 1))
					{
						candidate_Ins = ins;
						ev.Clear();
					}
				}
			}
			else
			{
				if (Is_Valid_Note(ev.Note - 1))
				{
					c_int ins = xc.Old_Ins - 1;

					if (!Is_Valid_Instrument(mod, ins))
						new_Invalid_Ins = true;
					else if (Check_Invalid_Sample(ins, ev.Note - 1))
						ev.Clear();
				}
			}

			if (Is_TonePorta(ev.FxT) || Is_TonePorta(ev.F2T))
				is_TonePorta = true;

			if (Test_Note(xc, Note_Flag.Env_Release | Note_Flag.FadeOut))
				is_Release = true;

			if ((xc.Period <= 0) || Test_Note(xc, Note_Flag.End))
				is_TonePorta = false;

			// Off-Porta.it
			if (is_TonePorta && (ev.FxT == Effects.Fx_Offset))
			{
				tonePorta_Offset = true;

				if (!Common.Has_Quirk(m, Quirk_Flag.PrEnv))
					Reset_Note(xc, Note_Flag.Env_End);
			}

			// Check instrument
			if (ev.Ins != 0)
			{
				c_int ins = ev.Ins - 1;
				bool set_New_Ins = true;

				// Portamento_after_keyoff.it test case
				if (is_Release && (key == 0))
				{
					if (is_TonePorta)
					{
						if (Common.Has_Quirk(m, Quirk_Flag.PrEnv) || Test_Note(xc, Note_Flag.Set))
						{
							is_TonePorta = false;
							Reset_Envelopes_Carry(xc);
						}
					}
					else
					{
						// Fixes OpenMPT wnoteoff.it
						Reset_Envelopes_Carry(xc);
					}
				}

				if (is_TonePorta && (xc.Ins == ins))
				{
					if (!Common.Has_Quirk(m, Quirk_Flag.PrEnv))
					{
						if (Is_Same_Sid(chn, ins, key - 1))
						{
							// Same instrument and same sample
							set_New_Ins = !is_Release;
						}
						else
						{
							// Same instrument, different sample
							not_Same_Ins = true;	// Need this too
							not_Same_Smp = true;
						}
					}
				}

				if (set_New_Ins)
				{
					Set(xc, Channel_Flag.New_Ins);
					reset_Env = true;
				}

				// Sample default volume is always enabled if a valid sample
				// is provided (Atomic Playboy, default_volume.it)
				use_Ins_Vol = true;
				xc.Per_Flags = Channel_Flag.None;

				if (Is_Valid_Instrument(mod, ins))
				{
					// Valid ins

					// See OpenMPT StoppedInstrSwap.it for cut case
					if ((key == 0) && !Test_Note(xc, Note_Flag.Key_Cut))
					{
						// Retrig in new ins in sample mode
						if (sample_Mode && Test_Note(xc, Note_Flag.End))
							lib.virt.LibXmp_Virt_VoicePos(chn, 0);

						// IT: Reset note for every new != ins
						if (xc.Ins == ins)
						{
							Set(xc, Channel_Flag.New_Ins);
							use_Ins_Vol = true;
						}
						else
							key = xc.Key + 1;

						Reset_Note(xc, Note_Flag.Set);
					}

					if ((xc.Ins != ins) && (!is_TonePorta || !Common.Has_Quirk(m, Quirk_Flag.PrEnv)))
					{
						candidate_Ins = ins;

						if (!Is_Same_Sid(chn, ins, key - 1))
						{
							not_Same_Ins = true;

							if (is_TonePorta)
							{
								// Get new instrument volume
								sub = Get_SubInstrument(ins, key);
								if (sub != null)
								{
									xc.Volume = sub.Vol;
									use_Ins_Vol = false;
								}
							}
						}
					}
				}
				else
				{
					// In sample mode invalid instruments cut the current
					// note (OpenMPT SampleNumberChange.it).
					// TODO: portamento_sustain.it order 3 row 19: when
					// sample release is set, this isn't always done?
					if (sample_Mode)
						xc.Volume = 0;

					// Ignore invalid instruments
					new_Invalid_Ins = true;
					xc.Flags = Channel_Flag.None;
					use_Ins_Vol = false;
				}
			}

			// Check note
			if (key != 0)
			{
				Set(xc, Channel_Flag.New_Note);
				Set_Note(xc, Note_Flag.Set);

				if (key == Constants.Xmp_Key_Fade)
				{
					Set_Note(xc, Note_Flag.FadeOut);

					reset_Env = false;
					reset_SusLoop = false;
					use_Ins_Vol = false;
				}
				else if (key == Constants.Xmp_Key_Cut)
				{
					Set_Note(xc, Note_Flag.End | Note_Flag.Cut | Note_Flag.Key_Cut);
					xc.Period = 0;

					lib.virt.LibXmp_Virt_ResetChannel(chn);
				}
				else if (key == Constants.Xmp_Key_Off)
				{
					Xmp_Envelope env = null;

					if (Is_Valid_Instrument(mod, xc.Ins))
						env = mod.Xxi[xc.Ins].Aei;

					if (Sustain_Check(env, xc.V_Idx))
						Set_Note(xc, Note_Flag.SusExit);
					else
						Set_Note(xc, Note_Flag.Release);

					Set(xc, Channel_Flag.Key_Off);

					// Use instrument volume if an instrument was explicity
					// provided on this row (see OpenMPT NoteOffInstr.it row 4).
					// However, never reset the envelope (see OpenMPT wnoteoff.it)
					reset_Env = false;
					reset_SusLoop = false;

					if (ev.Ins == 0)
						use_Ins_Vol = false;
				}
				else if (!new_Invalid_Ins)
				{
					// Sample sustain release should always carry for tone
					// portamento, and is not reset unless a note is
					// present (Atomic Playboy, portamento_sustain.it).
					//
					// portamento_after_keyoff.it test case
					// also see suburban_streets o13 c45
					if (!is_TonePorta)
					{
						reset_Env = true;
						reset_SusLoop = true;
					}

					if (is_TonePorta)
					{
						if (not_Same_Ins || Test_Note(xc, Note_Flag.End))
						{
							Set(xc, Channel_Flag.New_Ins);
							Reset_Note(xc, Note_Flag.Env_Release | Note_Flag.SusExit | Note_Flag.FadeOut);
						}
						else
						{
							if (Is_Valid_Note(key - 1))
								xc.Key_Porta = key - 1;

							key = 0;
						}
					}
				}
			}

			// TODO: Instrument change+porta(+release?) doesn't require a key.
			// Order 3/row 11 of portamento_sustain.it should change the sample
			if (Is_Valid_Note(key - 1) && !new_Invalid_Ins)
			{
				if (Test_Note(xc, Note_Flag.Cut))
					use_Ins_Vol = true;		// See OpenMPT NoteOffInstr.it

				xc.Key = --key;
				Reset_Note(xc, Note_Flag.End);

				sub = Get_SubInstrument(candidate_Ins, key);

				if (sub != null)
				{
					c_int transp = mod.Xxi[candidate_Ins].Map[key].Xpo;

					// Clear note delay before duplicating channels:
					// it_note_delay_nna.it
					xc.Delay = 0;

					note = key + sub.Xpo + transp;
					c_int smp = sub.Sid;

					if (!Is_Valid_Sample(mod, smp))
						smp = -1;

					Xmp_Inst_Dct dct = sub.Dct;

					if (not_Same_Smp)
					{
						Fix_Period(chn, sub);

						// Toneporta, even when not executed, disables
						// NNA and DCAs for the current note:
						// portamento_nna_sample.it, gxsmp2.it
						lib.virt.LibXmp_Virt_SetNna(chn, Xmp_Inst_Nna.Cut);
						dct = Xmp_Inst_Dct.Off;
					}

					c_int to = lib.virt.LibXmp_Virt_SetPatch(chn, candidate_Ins, smp, note, key, sub.Nna, dct, sub.Dca);

					// Random value for volume swing
					c_int rvv = sub.Rvv & 0xff;
					if (rvv != 0)
					{
						Common.Clamp(ref rvv, 0, 100);
						xc.Rvv = (c_int)Rng.LibXmp_Get_Random(ctx.Rng, (c_uint)(rvv + 1));
					}
					else
						xc.Rvv = 0;

					// Random value for pan swing
					rvv = (sub.Rvv & 0xff00) >> 8;
					if (rvv != 0)
					{
						Common.Clamp(ref rvv, 0, 64);
						xc.Rpv = (c_int)Rng.LibXmp_Get_Random(ctx.Rng, (c_uint)(rvv + 1)) - (rvv / 2);
					}
					else
						xc.Rpv = 0;

					if (to < 0)
						return -1;

					if (to != chn)
					{
						Copy_Channel(p, to, chn);
						p.Xc_Data[to].Flags = Channel_Flag.None;
					}

					if (smp >= 0)	// Not sure if needed
						xc.Smp = smp;
				}
				else
				{
					xc.Flags = Channel_Flag.None;
					use_Ins_Vol = false;
				}
			}

			// Do after virtual channel copy
			if (is_TonePorta || retrig_Ins)
			{
				if (Common.Has_Quirk(m, Quirk_Flag.PrEnv) && (ev.Ins != 0))
					Reset_Envelopes_Carry(xc);
			}

			if (Is_Valid_Instrument(mod, candidate_Ins))
			{
				if (xc.Ins != candidate_Ins)
				{
					// Reset envelopes if instrument changes
					Reset_Envelopes(xc);
				}

				xc.Ins = candidate_Ins;
				xc.Ins_Fade = mod.Xxi[candidate_Ins].Rls;
			}

			// Reset in case of new instrument and the previous envelope has
			// finished (OpenMPT test EnvReset.it). This must take place after
			// channel copies in case of NNA (see test/test.it)
			// Also if we have envelope in carry mode, check fadeout
			// Also, only reset the volume envelope. (it_fade_env_reset_carry.it)
			if ((ev.Ins != 0) && Test_Note(xc, Note_Flag.Env_End))
			{
				if (Check_FadeOut(xc, candidate_Ins))
					Reset_Envelope_Volume(xc);
				else
					reset_Env = false;
			}

			if (reset_Env)
			{
				if (ev.Note != 0)
					Reset_Note(xc, Note_Flag.Env_Release | Note_Flag.SusExit | Note_Flag.FadeOut);

				// Set after copying to new virtual channel (see ambio.it)
				xc.FadeOut = 0x10000;
			}

			if (reset_SusLoop && (ev.Note != 0))
				Reset_Note(xc, Note_Flag.Sample_Release);

			// See OpenMPT wnoteoff.it vs noteoff3.it
			if (retrig_Ins && not_Same_Ins)
			{
				Set(xc, Channel_Flag.New_Ins);
				lib.virt.LibXmp_Virt_VoicePos(chn, 0);

				xc.FadeOut = 0x10000;
				Reset_Note(xc, Note_Flag.Release | Note_Flag.SusExit | Note_Flag.FadeOut);
			}

			sub = Get_SubInstrument(xc.Ins, xc.Key);

			Set_Effect_Defaults(note, sub, xc, is_TonePorta);

			if (sub != null)
			{
				if (note >= 0)
				{
					// Reset pan, see OpenMPT PanReset.it
					Set_Channel_Pan(xc, sub.Pan);

					if (Test_Note(xc, Note_Flag.Cut))
						Reset_Envelopes(xc);
					else if (!tonePorta_Offset || Common.Has_Quirk(m, Quirk_Flag.PrEnv))
						Reset_Envelopes_Carry(xc);

					Reset_Note(xc, Note_Flag.Cut);
				}
			}

			// Process new volume
			if ((ev.Vol != 0) && (!Test_Note(xc, Note_Flag.Cut) || (ev.Ins != 0)))
			{
				// Do this even for XMP_KEY_OFF (see OpenMPT NoteOffInstr.it row 4)
				Set_Channel_Volume(xc, ev.Vol - 1);
			}

			// IT: Always reset sample offset
			xc.Offset.Val &= ~0xffff;

			// According to Storlek test 25, Impulse Tracker handles the volume
			// column effects after the standard effects.
			// TODO: IT updates toneporta memory on first tick but reloads memory
			// to use for the rate for both channels every tick afterward.
			// TODO: Modplug processes some FX every tick (affects toneporta memory)
			LibXmp_Process_Fx(xc, chn, ev, 0);
			LibXmp_Process_Fx(xc, chn, ev, 1);
			Set_Period(note, sub, xc, is_TonePorta);

			if (sub == null)
				return 0;

			if (note >= 0)
				xc.Note = note;

			if ((note >= 0) || tonePorta_Offset)
				lib.virt.LibXmp_Virt_VoicePos(chn, xc.Offset.Val);

			if (use_Ins_Vol && !Test(xc, Channel_Flag.New_Vol))
				xc.Volume = sub.Vol;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Read_Event_Med(Xmp_Event e, c_int chn)
		{
			throw new NotImplementedException();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Read_Event_SMix(Xmp_Event e, c_int chn)
		{
			throw new NotImplementedException();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int LibXmp_Read_Event(Xmp_Event e, c_int chn)
		{
			Player_Data p = ctx.P;
			Module_Data m = ctx.M;
			Channel_Data xc = p.Xc_Data[chn];

			if (e.Ins != 0)
				xc.Old_Ins = e.Ins;

			if (Test_Note(xc, Note_Flag.Sample_End))
				Set_Note(xc, Note_Flag.End);

			if (chn >= m.Mod.Chn)
				return Read_Event_SMix(e, chn);

			switch (m.Read_Event_Type)
			{
				case Read_Event.Mod:
					return Read_Event_Mod(e, chn);

				case Read_Event.Ft2:
					return Read_Event_Ft2(e, chn);

				case Read_Event.St3:
					return Read_Event_St3(e, chn);

				case Read_Event.It:
					return Read_Event_It(e, chn);

				case Read_Event.Med:
					return Read_Event_Med(e, chn);

				default:
					return Read_Event_Mod(e, chn);
			}
		}
	}
}
