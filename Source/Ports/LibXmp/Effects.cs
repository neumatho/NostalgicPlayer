/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Common;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Player;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Virt;
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
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Set_Lfo_NotZero(Containers.Lfo.Lfo lfo, c_int depth, c_int rate)
		{
			if (depth != 0)
				lib.lfo.LibXmp_Lfo_Set_Depth(lfo, depth);

			if (rate != 0)
				lib.lfo.LibXmp_Lfo_Set_Rate(lfo, rate);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Effect_Memory__(ref uint8 p, ref c_int m)
		{
			if (p == 0)
				p = (uint8)m;
			else
				m = p;
		}



		/********************************************************************/
		/// <summary>
		/// ST3 effect memory is not a bug, but it's a weird implementation
		/// and it's unlikely to be supported in anything other than ST3
		/// (or OpenMPT)
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Effect_Memory(Module_Data md, Channel_Data xc, ref uint8 p, ref c_int m)
		{
			if (Common.Has_Quirk(md, Quirk_Flag.St3Bugs))
				Effect_Memory__(ref p, ref xc.Vol.Memory);
			else
				Effect_Memory__(ref p, ref m);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Effect_Memory_SetOnly(Module_Data md, Channel_Data xc, ref uint8 p, ref c_int m)
		{
			Effect_Memory__(ref p, ref m);

			if (Common.Has_Quirk(md, Quirk_Flag.St3Bugs))
			{
				if (p != 0)
					xc.Vol.Memory = p;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Effect_Memory_S3M(Module_Data md, Channel_Data xc, ref uint8 p)
		{
			if (Common.Has_Quirk(md, Quirk_Flag.St3Bugs))
				Effect_Memory__(ref p, ref xc.Vol.Memory);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Do_TonePorta(Channel_Data xc, c_int note)
		{
			Module_Data m = ctx.M;
			Xmp_Instrument instrument = m.Mod.Xxi[xc.Ins];
			c_int mapped_Xpo = 0;
			c_int mapped = 0;

			if (Is_Valid_Note(xc.Key))
				mapped = instrument.Map[xc.Key].Ins;

			if (mapped >= instrument.Nsm)
				mapped = 0;

			Xmp_SubInstrument sub = instrument.Sub[mapped];

			if (Is_Valid_Note(note - 1) && ((uint32)xc.Ins < m.Mod.Ins))
			{
				note--;

				if (Is_Valid_Note(xc.Key_Porta))
					mapped_Xpo = instrument.Map[xc.Key_Porta].Xpo;

				xc.Porta.Target = lib.period.LibXmp_Note_To_Period(note + sub.Xpo + mapped_Xpo, xc.FineTune, xc.Per_Adj);
			}

			xc.Porta.Dir = xc.Period < xc.Porta.Target ? 1 : -1;
		}



		/********************************************************************/
		/// <summary>
		/// Fine portamento up
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Do_Fx_F_Porta_Up(Channel_Data xc, uint8 fxP)
		{
			if (fxP != 0)
			{
				Set(xc, Channel_Flag.Fine_Bend);
				xc.Freq.FSlide = -fxP;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Fine portamento down
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Do_Fx_F_Porta_Dn(Channel_Data xc, uint8 fxP)
		{
			if (fxP != 0)
			{
				Set(xc, Channel_Flag.Fine_Bend);
				xc.Freq.FSlide = fxP;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Fine volume slide up
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Do_Fx_F_VSlide_Up(Channel_Data xc, uint8 fxP)
		{
			Set(xc, Channel_Flag.Fine_Vols);
			xc.Vol.FSlide = fxP;
		}



		/********************************************************************/
		/// <summary>
		/// Fine volume slide down
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Do_Fx_F_VSlide_Dn(Channel_Data xc, uint8 fxP)
		{
			Set(xc, Channel_Flag.Fine_Vols);
			xc.Vol.FSlide = -fxP;
		}



		/********************************************************************/
		/// <summary>
		/// Extra fine portamento up
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Do_Fx_Xf_Porta_Up(Channel_Data xc, uint8 fxP)
		{
			Set(xc, Channel_Flag.Fine_Bend);
			xc.Freq.FSlide = -0.25 * fxP;
		}



		/********************************************************************/
		/// <summary>
		/// Extra fine portamento down
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Do_Fx_Xf_Porta_Dn(Channel_Data xc, uint8 fxP)
		{
			Set(xc, Channel_Flag.Fine_Bend);
			xc.Freq.FSlide = 0.25 * fxP;
		}



		/********************************************************************/
		/// <summary>
		/// Set pan
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Do_Fx_SetPan(Channel_Data xc, uint8 fxP, Module_Data m, Xmp_Event e, c_int fNum)
		{
			// From OpenMPT PanOff.xm:
			// "Another chapter of weird FT2 bugs: Note-Off + Note Delay
			//  + Volume Column Panning = Panning effect is ignored
			if (!Common.Has_Quirk(m, Quirk_Flag.Ft2Bugs)	// If not FT2
				|| (fNum == 0)								// or not vol column
				|| (e.Note != Constants.Xmp_Key_Off)		// or not keyoff
				|| (e.FxT != Effects.Fx_Extended)			// or not delay
				|| (Common.Msn(e.FxP) != Effects.Ex_Delay))
			{
				xc.Pan.Val = fxP;
				xc.Pan.Surround = false;
			}

			xc.Rpv = 0;		// storlek_20: set pan overrides random pan
			xc.Pan.Surround = false;
		}



		/********************************************************************/
		/// <summary>
		/// Set pan
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Do_Fx_Patt_Delay(uint8 fxP, Module_Data m, Player_Data p)
		{
			if ((m.Read_Event_Type != Read_Event.St3) || (p.Flow.Delay == 0))
				p.Flow.Delay = fxP;
		}



		/********************************************************************/
		/// <summary>
		/// Set pan
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Do_Fx_S3M_Speed(uint8 fxP, Player_Data p)
		{
			if (fxP != 0)
			{
				p.Speed = fxP;
				p.St26_Speed = 0;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Retrigger
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Do_Fx_Retrigger(Channel_Data xc, uint8 fxP, Module_Data m)
		{
			Set(xc, Channel_Flag.Retrig);

			xc.Retrig.Val = fxP;
			xc.Retrig.Count = fxP + 1;
			xc.Retrig.Type = 0;
			xc.Retrig.Limit = Common.Has_Quirk(m, Quirk_Flag.RtOnce) ? 1 : 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void LibXmp_Process_Fx(Channel_Data xc, c_int chn, Xmp_Event e, c_int fNum)
		{
			Player_Data p = ctx.P;
			Module_Data m = ctx.M;
			Xmp_Module mod = m.Mod;
			Flow_Control f = p.Flow;
			uint8 fxP, fxT;
			c_int h, l;

			// Key_porta is IT only
			if (m.Read_Event_Type != Read_Event.It)
				xc.Key_Porta = xc.Key;

			uint8 note = e.Note;

			if (fNum == 0)
			{
				fxT = e.FxT;
				fxP = e.FxP;
			}
			else
			{
				fxT = e.F2T;
				fxP = e.F2P;
			}

			switch (fxT)
			{
				case Effects.Fx_Arpeggio:
				{
					if (!Common.Has_Quirk(m, Quirk_Flag.ArpMem) || (fxP != 0))
					{
						xc.Arpeggio.Val[0] = 0;
						xc.Arpeggio.Val[1] = (sbyte)Common.Msn(fxP);
						xc.Arpeggio.Val[2] = (sbyte)Common.Lsn(fxP);
						xc.Arpeggio.Size = 3;
					}
					break;
				}

				case Effects.Fx_S3M_Arpeggio:
				{
					Effect_Memory(m, xc, ref fxP, ref xc.Arpeggio.Memory);
					goto case Effects.Fx_Arpeggio;
				}

				case Effects.Fx_Okt_Arp3:
				{
					if (fxP != 0)
					{
						xc.Arpeggio.Val[0] = (sbyte)-Common.Msn(fxP);
						xc.Arpeggio.Val[1] = 0;
						xc.Arpeggio.Val[2] = (sbyte)Common.Lsn(fxP);
						xc.Arpeggio.Size = 3;
					}
					break;
				}

				case Effects.Fx_Okt_Arp4:
				{
					if (fxP != 0)
					{
						xc.Arpeggio.Val[0] = 0;
						xc.Arpeggio.Val[1] = (sbyte)Common.Lsn(fxP);
						xc.Arpeggio.Val[2] = 0;
						xc.Arpeggio.Val[3] = (sbyte)-Common.Msn(fxP);
						xc.Arpeggio.Size = 4;
					}
					break;
				}

				case Effects.Fx_Okt_Arp5:
				{
					if (fxP != 0)
					{
						xc.Arpeggio.Val[0] = (sbyte)Common.Lsn(fxP);
						xc.Arpeggio.Val[1] = (sbyte)Common.Lsn(fxP);
						xc.Arpeggio.Val[2] = 0;
						xc.Arpeggio.Size = 3;
					}
					break;
				}

				// Portamento up
				case Effects.Fx_Porta_Up:
				{
					Effect_Memory(m, xc, ref fxP, ref xc.Freq.Memory);

					if (Common.Has_Quirk(m, Quirk_Flag.FineFx) && ((fNum == 0) || !Common.Has_Quirk(m, Quirk_Flag.ItVPor)))
					{
						switch (Common.Msn(fxP))
						{
							case 0xf:
							{
								fxP &= 0x0f;
								Do_Fx_F_Porta_Up(xc, fxP);
								return;
							}

							case 0xe:
							{
								fxP &= 0x0f;
								Do_Fx_Xf_Porta_Up(xc, fxP);
								return;
							}
						}
					}

					if (fxP != 0)
					{
						Set(xc, Channel_Flag.PitchBend);

						xc.Freq.Slide = -fxP;

						if (Common.Has_Quirk(m, Quirk_Flag.UniSld))
							xc.Porta.Memory = fxP;
					}
					break;
				}

				// Portamento down
				case Effects.Fx_Porta_Dn:
				{
					// FT2 has separate up and down memory
					if (Common.Has_Quirk(m, Quirk_Flag.Ft2Bugs))
						Effect_Memory(m, xc, ref fxP, ref xc.Freq.Down_Memory);
					else
						Effect_Memory(m, xc, ref fxP, ref xc.Freq.Memory);

					if (Common.Has_Quirk(m, Quirk_Flag.FineFx) && ((fNum == 0) || !Common.Has_Quirk(m, Quirk_Flag.ItVPor)))
					{
						switch (Common.Msn(fxP))
						{
							case 0xf:
							{
								fxP &= 0x0f;
								Do_Fx_F_Porta_Dn(xc, fxP);
								return;
							}

							case 0xe:
							{
								fxP &= 0x0f;
								Do_Fx_Xf_Porta_Dn(xc, fxP);
								return;
							}
						}
					}

					if (fxP != 0)
					{
						Set(xc, Channel_Flag.PitchBend);

						xc.Freq.Slide = fxP;

						if (Common.Has_Quirk(m, Quirk_Flag.UniSld))
							xc.Porta.Memory = fxP;
					}
					break;
				}

				// Tone portamento
				case Effects.Fx_TonePorta:
				{
					Effect_Memory_SetOnly(m, xc, ref fxP, ref xc.Porta.Memory);

					if (fxP != 0)
					{
						if (Common.Has_Quirk(m, Quirk_Flag.UniSld))		// IT compatible Gxx off
							xc.Freq.Memory = fxP;

						xc.Porta.Slide = fxP;
					}

					if (Common.Has_Quirk(m, Quirk_Flag.IgStPor))
					{
						if ((note == 0) && (xc.Porta.Dir == 0))
							break;
					}

					if (!Is_Valid_Instrument(mod, xc.Ins))
						break;

					Do_TonePorta(xc, note);

					Set(xc, Channel_Flag.TonePorta);
					break;
				}

				// Vibrato
				case Effects.Fx_Vibrato:
				{
					Effect_Memory_SetOnly(m, xc, ref fxP, ref xc.Vibrato.Memory);

					Set(xc, Channel_Flag.Vibrato);
					Set_Lfo_NotZero(xc.Vibrato.Lfo, Common.Lsn(fxP) << 2, Common.Msn(fxP));
					break;
				}

				// Fine vibrato
				case Effects.Fx_Fine_Vibrato:
				{
					Effect_Memory_SetOnly(m, xc, ref fxP, ref xc.Vibrato.Memory);

					Set(xc, Channel_Flag.Vibrato);
					Set_Lfo_NotZero(xc.Vibrato.Lfo, Common.Lsn(fxP), Common.Msn(fxP));
					break;
				}

				// Toneporta + vol slide
				case Effects.Fx_Tone_VSlide:
				{
					if (!Is_Valid_Instrument(mod, xc.Ins))
						break;

					Do_TonePorta(xc, note);

					Set(xc, Channel_Flag.TonePorta);
					goto case Effects.Fx_VolSlide;
				}

				// Vibrato + vol slide
				case Effects.Fx_Vibra_VSlide:
				{
					Set(xc, Channel_Flag.Vibrato);
					goto case Effects.Fx_VolSlide;
				}

				// Tremolo
				case Effects.Fx_Tremolo:
				{
					Effect_Memory(m, xc, ref fxP, ref xc.Tremolo.Memory);
					Set(xc, Channel_Flag.Tremolo);
					Set_Lfo_NotZero(xc.Tremolo.Lfo, Common.Lsn(fxP), Common.Msn(fxP));
					break;
				}

				// Set pan
				case Effects.Fx_SetPan:
				{
					if (Common.Has_Quirk(m, Quirk_Flag.ProTrack))
						break;

					Do_Fx_SetPan(xc, fxP, m, e, fNum);
					break;
				}

				// Set sample offset
				case Effects.Fx_Offset:
				{
					Effect_Memory(m, xc, ref fxP, ref xc.Offset.Memory);
					Set(xc, Channel_Flag.Offset);

					if (note != 0)
					{
						xc.Offset.Val &= xc.Offset.Val & ~0xffff;
						xc.Offset.Val |= fxP << 8;
						xc.Offset.Val2 = fxP << 8;
					}

					if (e.Ins != 0)
						xc.Offset.Val2 = fxP << 8;

					break;
				}

				// Volume slide
				case Effects.Fx_VolSlide:
				{
					// S3M file volume slide note:
					// DFy  Fine volume down by y (...) If y is 0, the command will
					//      be treated as a volume slide up with a value of f (15).
					//      If a DFF command is specified, the volume will be slid
					//      up
					if (Common.Has_Quirk(m, Quirk_Flag.FineFx))
					{
						h = Common.Msn(fxP);
						l = Common.Lsn(fxP);

						if ((l == 0xf) && (h != 0))
						{
							xc.Vol.Memory = fxP;
							fxP >>= 4;
							Do_Fx_F_VSlide_Up(xc, fxP);
							return;
						}
						else if ((h == 0xf && (l != 0)))
						{
							xc.Vol.Memory = fxP;
							fxP &= 0x0f;
							Do_Fx_F_VSlide_Dn(xc, fxP);
							return;
						}
					}

					// Recover memory
					if (fxP == 0x00)
					{
						fxP = (uint8)xc.Vol.Memory;
						if (fxP != 0)
							goto case Effects.Fx_VolSlide;
					}

					Set(xc, Channel_Flag.Vol_Slide);

					// Skaven's 2nd reality (S3M) has volslide parameter D7 => pri
					// down. Other trackers only compute volumes if the other
					// parameter is 0, Fall from sky.xm has 2C => do nothing.
					// Also don't assign xc->vol.memory if fxp is 0, see Guild
					// of Sounds.xm
					if (fxP != 0)
					{
						xc.Vol.Memory = fxP;

						h = Common.Msn(fxP);
						l = Common.Lsn(fxP);

						if (fxP != 0)
						{
							if (Common.Has_Quirk(m, Quirk_Flag.VolPdn))
								xc.Vol.Slide = l != 0 ? -l : h;
							else
								xc.Vol.Slide = h != 0 ? h : -l;
						}
					}

					// Mirko reports that a S3M with D0F effects created with ST321
					// should process volume slides in all frames like ST300. I
					// suspect ST3/IT could be handling D0F effects like this
					if (Common.Has_Quirk(m, Quirk_Flag.FineFx))
					{
						if ((Common.Msn(xc.Vol.Memory) == 0xf) || (Common.Lsn(xc.Vol.Memory) == 0xf))
						{
							Set(xc, Channel_Flag.Fine_Vols);
							xc.Vol.FSlide = xc.Vol.Slide;
						}
					}
					break;
				}

				// Secondary volume slide
				case Effects.Fx_VolSlide_2:
				{
					Set(xc, Channel_Flag.Vol_Slide_2);

					if (fxP != 0)
					{
						h = Common.Msn(fxP);
						l = Common.Lsn(fxP);
						xc.Vol.Slide2 = h != 0 ? h : -l;
					}
					break;
				}

				// Order jump
				case Effects.Fx_Jump:
				{
					LibXmp_Process_Pattern_Jump(f, fxP);
					break;
				}

				// Volume set
				case Effects.Fx_VolSet:
				{
					Set(xc, Channel_Flag.New_Vol);
					xc.Volume = fxP;

					if (xc.Split != 0)
						p.Xc_Data[xc.Pair].Volume = xc.Volume;

					break;
				}

				// Pattern break
				case Effects.Fx_Break:
				{
					LibXmp_Process_Pattern_Break(f, 10 * Common.Msn(fxP) + Common.Lsn(fxP));
					break;
				}

				// Extended effect
				case Effects.Fx_Extended:
				{
					Effect_Memory_S3M(m, xc, ref fxP);

					fxT = (uint8)(fxP >> 4);
					fxP &= 0x0f;

					switch (fxT)
					{
						// Amiga led filter
						case Effects.Ex_Filter:
						{
							if (Common.Is_Amiga_Mod(m))
								p.Filter = !((fxP & 1) != 0);

							break;
						}

						// Fine portamento up
						case Effects.Ex_F_Porta_Up:
						{
							Effect_Memory(m, xc, ref fxP, ref xc.Fine_Porta.Up_Memory);
							Do_Fx_F_Porta_Up(xc, fxP);
							return;
						}

						// Fine portamento down
						case Effects.Ex_F_Porta_Dn:
						{
							Effect_Memory(m, xc, ref fxP, ref xc.Fine_Porta.Down_Memory);
							Do_Fx_F_Porta_Dn(xc, fxP);
							return;
						}

						// Glissando toggle
						case Effects.Ex_Gliss:
						{
							if (fxP != 0)
								Set_Note(xc, Note_Flag.Glissando);
							else
								Reset_Note(xc, Note_Flag.Glissando);

							break;
						}

						// Set vibrato waveform
						case Effects.Ex_Vibrato_Wf:
						{
							fxP &= 3;
							lib.lfo.LibXmp_Lfo_Set_Waveform(xc.Vibrato.Lfo, fxP);
							break;
						}

						// Set finetune
						case Effects.Ex_FineTune:
						{
							if (!Common.Has_Quirk(m, Quirk_Flag.Ft2Bugs) || (note > 0))
								xc.FineTune = (int8)(fxP << 4);

							break;
						}

						// Loop pattern
						case Effects.Ex_Pattern_Loop:
						{
							LibXmp_Process_Pattern_Loop(f, chn, p.Row, fxP);
							break;
						}

						// Set tremolo waveform
						case Effects.Ex_Tremolo_Wf:
						{
							lib.lfo.LibXmp_Lfo_Set_Waveform(xc.Tremolo.Lfo, fxP & 3);
							break;
						}

						case Effects.Ex_SetPan:
						{
							fxP <<= 4;
							Do_Fx_SetPan(xc, fxP, m, e, fNum);
							return;
						}

						// Retrig note
						case Effects.Ex_Retrig:
						{
							Do_Fx_Retrigger(xc, fxP, m);
							break;
						}

						// Fine volume slide up
						case Effects.Ex_F_VSlide_Up:
						{
							Effect_Memory(m, xc, ref fxP, ref xc.Fine_Vol.Up_Memory);
							Do_Fx_F_VSlide_Up(xc, fxP);
							return;
						}

						// Fine volume slide down
						case Effects.Ex_F_VSlide_Dn:
						{
							Effect_Memory(m, xc, ref fxP, ref xc.Fine_Vol.Down_Memory);
							Do_Fx_F_VSlide_Dn(xc, fxP);
							return;
						}

						// Cut note
						case Effects.Ex_Cut:
						{
							Set(xc, Channel_Flag.Retrig);
							Set_Note(xc, Note_Flag.Cut);	// For IT cut-carry

							xc.Retrig.Val = fxP + 1;
							xc.Retrig.Count = xc.Retrig.Val;
							xc.Retrig.Type = 0x10;
							break;
						}

						// Note delay
						case Effects.Ex_Delay:
						{
							// Computed at frame loop
							break;
						}

						// Pattern delay
						case Effects.Ex_Patt_Delay:
						{
							Do_Fx_Patt_Delay(fxP, m, p);
							return;
						}

						// Invert loop / funk repeat
						case Effects.Ex_InvLoop:
						{
							xc.InvLoop.Speed = fxP;
							break;
						}
					}
					break;
				}

				// Set speed
				case Effects.Fx_Speed:
				{
					if (Common.Has_Quirk(m, Quirk_Flag.NoBpm) || ((p.Flags & Xmp_Flags.VBlank) != 0))
					{
						Do_Fx_S3M_Speed(fxP, p);
						return;
					}

					// Speedup.xm needs BPM = 20
					if (fxP < 0x20)
					{
						Do_Fx_S3M_Speed(fxP, p);
						return;
					}

					goto case Effects.Fx_S3M_Bpm;
				}

				case Effects.Fx_FineTune:
				{
					xc.FineTune = (int16)(fxP - 0x80);
					break;
				}

				// Fine volume slide up
				case Effects.Fx_F_VSlide_Up:
				{
					Effect_Memory(m, xc, ref fxP, ref xc.Fine_Vol.Up_Memory);
					Do_Fx_F_VSlide_Up(xc, fxP);
					break;
				}

				// Fine volume slide down
				case Effects.Fx_F_VSlide_Dn:
				{
					Effect_Memory(m, xc, ref fxP, ref xc.Fine_Vol.Up_Memory);
					Do_Fx_F_VSlide_Dn(xc, fxP);
					break;
				}

				// Fine portamento up
				case Effects.Fx_F_Porta_Up:
				{
					Do_Fx_F_Porta_Up(xc, fxP);
					break;
				}

				// Fine portamento down
				case Effects.Fx_F_Porta_Dn:
				{
					Do_Fx_F_Porta_Dn(xc, fxP);
					break;
				}

				case Effects.Fx_Patt_Delay:
				{
					Do_Fx_Patt_Delay(fxP, m, p);
					break;
				}

				// Set S3M speed
				case Effects.Fx_S3M_Speed:
				{
					Effect_Memory_S3M(m, xc, ref fxP);
					Do_Fx_S3M_Speed(fxP, p);
					break;
				}

				// Set S3M BPM
				case Effects.Fx_S3M_Bpm:
				{
					// Lower time factor in MED allows lower BPM values
					c_int min_Bpm = (c_int)(0.5 + m.Time_Factor * Constants.Xmp_Min_Bpm / 10);

					if (fxP < min_Bpm)
						fxP = (uint8)min_Bpm;

					p.Bpm = fxP;
					break;
				}

				// Set IT BPM
				case Effects.Fx_It_Bpm:
				{
					if (Common.Msn(fxP) == 0)
					{
						Set(xc, Channel_Flag.Tempo_Slide);

						if (Common.Lsn(fxP) != 0)	// T0x - Tempo slide down by x
							xc.Tempo.Slide = -Common.Lsn(fxP);

						// T00 - Repeat previous slide
					}
					else if (Common.Msn(fxP) == 1)	// T1x - Tempo slide up by x
					{
						Set(xc, Channel_Flag.Tempo_Slide);
						xc.Tempo.Slide = Common.Lsn(fxP);
					}
					else
					{
						if (fxP < Constants.Xmp_Min_Bpm)
							fxP = Constants.Xmp_Min_Bpm;

						p.Bpm = fxP;
					}
					break;
				}

				case Effects.Fx_It_RowDelay:
				{
					if (f.RowDelay_Set == RowDelay_Flag.None)
					{
						f.RowDelay = (RowDelay_Flag)fxP;
						f.RowDelay_Set = RowDelay_Flag.On | RowDelay_Flag.First_Frame;
					}
					break;
				}

				// From the OpenMPT VolColMemory.it test case:
				// "Volume column commands a, b, c and d (volume slide) share one
				// effect memory, but it should not be shared with Dxy in the effect
				// column

				// Fine volume slide up
				case Effects.Fx_VSlide_Up_2:
				{
					Effect_Memory(m, xc, ref fxP, ref xc.Vol.Memory2);
					Set(xc, Channel_Flag.Vol_Slide_2);

					xc.Vol.Slide2 = fxP;
					break;
				}

				// Fine volume slide down
				case Effects.Fx_VSlide_Dn_2:
				{
					Effect_Memory(m, xc, ref fxP, ref xc.Vol.Memory2);
					Set(xc, Channel_Flag.Vol_Slide_2);

					xc.Vol.Slide2 = -fxP;
					break;
				}

				// Fine volume slide up
				case Effects.Fx_F_VSlide_Up_2:
				{
					Effect_Memory(m, xc, ref fxP, ref xc.Vol.Memory2);
					Set(xc, Channel_Flag.Fine_Vols_2);

					xc.Vol.FSlide2 = fxP;
					break;
				}

				// Fine volume slide down
				case Effects.Fx_F_VSlide_Dn_2:
				{
					Effect_Memory(m, xc, ref fxP, ref xc.Vol.Memory2);
					Set(xc, Channel_Flag.Fine_Vols_2);

					xc.Vol.FSlide2 = -fxP;
					break;
				}

				// Pattern break with hex parameter
				case Effects.Fx_It_Break:
				{
					LibXmp_Process_Pattern_Break(f, fxP);
					break;
				}

				// Set global volume
				case Effects.Fx_GlobalVol:
				{
					if (fxP > m.GVolBase)
						p.GVol = m.GVolBase;
					else
						p.GVol = fxP;

					break;
				}

				// Global volume slide
				case Effects.Fx_GVol_Slide:
				{
					if (fxP != 0)
					{
						Set(xc, Channel_Flag.GVol_Slide);
						xc.GVol.Memory = fxP;

						h = Common.Msn(fxP);
						l = Common.Lsn(fxP);

						if (Common.Has_Quirk(m, Quirk_Flag.FineFx))
						{
							if ((l == 0xf) && (h != 0))
							{
								xc.GVol.Slide = 0;
								xc.GVol.FSlide = h;
							}
							else if ((h == 0xf) && (l != 0))
							{
								xc.GVol.Slide = 0;
								xc.GVol.FSlide = -l;
							}
							else
							{
								xc.GVol.Slide = h != 0 ? h : -l;
								xc.GVol.FSlide = 0;
							}
						}
						else
						{
							xc.GVol.Slide = h != 0 ? h : -l;
							xc.GVol.FSlide = 0;
						}
					}
					else
					{
						fxP = (uint8)xc.GVol.Memory;
						if (fxP != 0)
							goto case Effects.Fx_GVol_Slide;
					}
					break;
				}

				// Key off
				case Effects.Fx_Keyoff:
				{
					xc.KeyOff = fxP + 1;
					break;
				}

				// Set envelope position
				case Effects.Fx_EnvPos:
				{
					// From OpenMPT SetEnvPos.xm:
					// "When using the Lxx effect, Fasttracker 2 only sets the
					//  panning envelope position if the volume envelope’s sustain
					//  flag is set
					if (Common.Has_Quirk(m, Quirk_Flag.Ft2Bugs))
					{
						Xmp_Instrument instrument = lib.sMix.LibXmp_Get_Instrument(xc.Ins);
						if (instrument != null)
						{
							if ((instrument.Aei.Flg & Xmp_Envelope_Flag.Sus) != 0)
								xc.P_Idx = fxP;
						}
					}
					else
						xc.P_Idx = fxP;

					xc.V_Idx = fxP;
					xc.F_Idx = fxP;
					break;
				}

				// Pan slide (XM)
				case Effects.Fx_PanSlide:
				{
					Effect_Memory(m, xc, ref fxP, ref xc.Pan.Memory);
					Set(xc, Channel_Flag.Pan_Slide);

					xc.Pan.Slide = Common.Lsn(fxP) - Common.Msn(fxP);
					break;
				}

				// Pan slide (XM volume column)
				case Effects.Fx_PanSl_NoMem:
				{
					Set(xc, Channel_Flag.Pan_Slide);

					xc.Pan.Slide = Common.Lsn(fxP) - Common.Msn(fxP);
					break;
				}

				// Pan slide w/ fine pan (IT)
				case Effects.Fx_It_PanSlide:
				{
					Set(xc, Channel_Flag.Pan_Slide);

					if (fxP != 0)
					{
						if (Common.Msn(fxP) == 0xf)
						{
							xc.Pan.Slide = 0;
							xc.Pan.FSlide = Common.Lsn(fxP);
						}
						else if (Common.Lsn(fxP) == 0xf)
						{
							xc.Pan.Slide = 0;
							xc.Pan.FSlide = -Common.Msn(fxP);
						}
						else
						{
							Set(xc, Channel_Flag.Pan_Slide);
							xc.Pan.Slide = Common.Lsn(fxP) - Common.Msn(fxP);
							xc.Pan.FSlide = 0;
						}
					}
					break;
				}

				// Multi retrig
				case Effects.Fx_Multi_Retrig:
				{
					Effect_Memory_S3M(m, xc, ref fxP);

					if (fxP != 0)
					{
						xc.Retrig.Val = Common.Lsn(fxP);
						xc.Retrig.Type = Common.Msn(fxP);
					}

					if (note != 0)
						xc.Retrig.Count = xc.Retrig.Val + 1;

					xc.Retrig.Limit = 0;

					Set(xc, Channel_Flag.Retrig);
					break;
				}

				// Tremor
				case Effects.Fx_Tremor:
				{
					Effect_Memory(m, xc, ref fxP, ref xc.Tremor.Memory);

					xc.Tremor.Up = Common.Msn(fxP);
					xc.Tremor.Down = Common.Lsn(fxP);

					if (Common.Is_Player_Mode_Ft2(m))
						xc.Tremor.Count |= 0x80;
					else
					{
						if (xc.Tremor.Up == 0)
							xc.Tremor.Up++;

						if (xc.Tremor.Down == 0)
							xc.Tremor.Down++;
					}

					Set(xc, Channel_Flag.Tremor);
					break;
				}

				// Extra fine portamento
				case Effects.Fx_Xf_Porta:
				{
					h = Common.Msn(fxP);
					fxP &= 0x0f;

					switch (h)
					{
						// Extra fine portamento up
						case Effects.Xx_Xf_Porta_Up:
						{
							Effect_Memory(m, xc, ref fxP, ref xc.Fine_Porta.Xf_Up_Memory);
							Do_Fx_Xf_Porta_Up(xc, fxP);
							break;
						}

						// Extra fine portamento down
						case Effects.Xx_Xf_Porta_Dn:
						{
							Effect_Memory(m, xc, ref fxP, ref xc.Fine_Porta.Xf_Dn_Memory);
							Do_Fx_Xf_Porta_Dn(xc, fxP);
							break;
						}
					}
					break;
				}

				case Effects.Fx_Surround:
				{
					xc.Pan.Surround = fxP != 0;
					break;
				}

				// Play forward/backward
				case Effects.Fx_Reverse:
				{
					lib.virt.LibXmp_Virt_Reverse(chn, fxP != 0);
					break;
				}

				// Track volume setting
				case Effects.Fx_Trk_Vol:
				{
					if (fxP <= m.VolBase)
						xc.MasterVol = fxP;

					break;
				}

				// Track volume slide
				case Effects.Fx_Trk_VSlide:
				{
					if (fxP == 0)
					{
						fxP = (uint8)xc.TrackVol.Memory;
						if (fxP == 0)
							break;
					}

					if (Common.Has_Quirk(m, Quirk_Flag.FineFx))
					{
						h = Common.Msn(fxP);
						l = Common.Lsn(fxP);

						if ((h == 0xf) && (l != 0))
						{
							xc.TrackVol.Memory = fxP;
							fxP &= 0x0f;
							goto case Effects.Fx_Trk_FVSlide;
						}
						else if ((l == 0xf) && (h != 0))
						{
							xc.TrackVol.Memory = fxP;
							fxP &= 0xf0;
							goto case Effects.Fx_Trk_FVSlide;
						}
					}

					Set(xc, Channel_Flag.Trk_VSlide);

					if (fxP != 0)
					{
						h = Common.Msn(fxP);
						l = Common.Lsn(fxP);

						xc.TrackVol.Memory = fxP;

						if (Common.Has_Quirk(m, Quirk_Flag.VolPdn))
							xc.TrackVol.Slide = l != 0 ? -l : h;
						else
							xc.TrackVol.Slide = h != 0 ? h : -l;
					}
					break;
				}

				// Track fine volume slide
				case Effects.Fx_Trk_FVSlide:
				{
					Set(xc, Channel_Flag.Trk_FVSlide);

					if (fxP != 0)
						xc.TrackVol.FSlide = Common.Msn(fxP) - Common.Lsn(fxP);

					break;
				}

				case Effects.Fx_It_InstFunc:
				{
					switch (fxP)
					{
						// Past note cut
						case 0:
						{
							lib.virt.LibXmp_Virt_PastNote(chn, Virt_Action.Cut);
							break;
						}

						// Past note off
						case 1:
						{
							lib.virt.LibXmp_Virt_PastNote(chn, Virt_Action.Off);
							break;
						}

						// Past note fade
						case 2:
						{
							lib.virt.LibXmp_Virt_PastNote(chn, Virt_Action.Fade);
							break;
						}

						// Set NNA to note cut
						case 3:
						{
							lib.virt.LibXmp_Virt_SetNna(chn, Xmp_Inst_Nna.Cut);
							break;
						}

						// Set NNA to continue
						case 4:
						{
							lib.virt.LibXmp_Virt_SetNna(chn, Xmp_Inst_Nna.Cont);
							break;
						}

						// Set NNA to note off
						case 5:
						{
							lib.virt.LibXmp_Virt_SetNna(chn, Xmp_Inst_Nna.Off);
							break;
						}

						// Set NNA to note fade
						case 6:
						{
							lib.virt.LibXmp_Virt_SetNna(chn, Xmp_Inst_Nna.Fade);
							break;
						}

						// Turn off volume envelope
						case 7:
						{
							Set_Per(xc, Channel_Flag.VEnv_Pause);
							break;
						}

						// Turn on volume envelope
						case 8:
						{
							Reset_Per(xc, Channel_Flag.VEnv_Pause);
							break;
						}

						// Turn off pan envelope
						case 9:
						{
							Set_Per(xc, Channel_Flag.PEnv_Pause);
							break;
						}

						// Turn on pan envelope
						case 0xa:
						{
							Reset_Per(xc, Channel_Flag.PEnv_Pause);
							break;
						}

						// Turn off pitch envelope
						case 0xb:
						{
							Set_Per(xc, Channel_Flag.FEnv_Pause);
							break;
						}

						// Turn on pitch envelope
						case 0xc:
						{
							Reset_Per(xc, Channel_Flag.FEnv_Pause);
							break;
						}
					}
					break;
				}

				case Effects.Fx_Flt_CutOff:
				{
					xc.Filter.CutOff = fxP;
					break;
				}

				case Effects.Fx_Flt_Resn:
				{
					xc.Filter.Resonance = fxP;
					break;
				}

				case Effects.Fx_Macro_Set:
				{
					xc.Macro.Active = Common.Lsn(fxP);
					break;
				}

				case Effects.Fx_Macro:
				{
					Set(xc, Channel_Flag.Midi_Macro);

					xc.Macro.Val = fxP;
					xc.Macro.Slide = 0;
					break;
				}

				case Effects.Fx_MacroSmooth:
				{
					if ((ctx.P.Speed != 0) && (xc.Macro.Val < 0x80))
					{
						Set(xc, Channel_Flag.Midi_Macro);

						xc.Macro.Target = fxP;
						xc.Macro.Slide = (fxP - xc.Macro.Val) / ctx.P.Speed;
					}
					break;
				}

				// Panbrello
				case Effects.Fx_Panbrello:
				{
					Set(xc, Channel_Flag.Panbrello);
					Set_Lfo_NotZero(xc.Panbrello.Lfo, Common.Lsn(fxP) << 4, Common.Msn(fxP));
					break;
				}

				// Panbrello waveform
				case Effects.Fx_Panbrello_Wf:
				{
					lib.lfo.LibXmp_Lfo_Set_Waveform(xc.Panbrello.Lfo, fxP & 3);
					break;
				}

				// High offset
				case Effects.Fx_HiOffset:
				{
					xc.Offset.Val &= 0xffff;
					xc.Offset.Val |= fxP << 16;
					break;
				}

				// SFX effects
				case Effects.Fx_Vol_Add:
				{
					if (!Is_Valid_Instrument(mod, xc.Ins))
						break;

					Set(xc, Channel_Flag.New_Vol);

					xc.Volume = m.Mod.Xxi[xc.Ins].Sub[0].Vol + fxP;
					if (xc.Volume > m.VolBase)
						xc.Volume = m.VolBase;

					break;
				}

				case Effects.Fx_Vol_Sub:
				{
					if (!Is_Valid_Instrument(mod, xc.Ins))
						break;

					Set(xc, Channel_Flag.New_Vol);

					xc.Volume = m.Mod.Xxi[xc.Ins].Sub[0].Vol - fxP;
					if (xc.Volume < 0)
						xc.Volume = 0;

					break;
				}

				case Effects.Fx_Pitch_Add:
				{
					Set_Per(xc, Channel_Flag.TonePorta);

					xc.Porta.Target = lib.period.LibXmp_Note_To_Period(note - 1, xc.FineTune, 0) + fxP;
					xc.Porta.Slide = 2;
					xc.Porta.Dir = 1;
					break;
				}

				case Effects.Fx_Pitch_Sub:
				{
					Set_Per(xc, Channel_Flag.TonePorta);

					xc.Porta.Target = lib.period.LibXmp_Note_To_Period(note - 1, xc.FineTune, 0) - fxP;
					xc.Porta.Slide = 2;
					xc.Porta.Dir = -1;
					break;
				}

				// Saga Musix says:
				//
				// "When both nibbles of an Fxx command are set, SoundTracker 2.6
				// applies the both values alternatingly, first the high nibble,
				// then the low nibble on the next row, then the high nibble again...
				// If only the high nibble is set, it should act like if only the low
				// nibble is set (i.e. F30 is the same as F03)
				case Effects.Fx_Ice_Speed:
				{
					if (fxP != 0)
					{
						if (Common.Lsn(fxP) != 0)
							p.St26_Speed = (Common.Msn(fxP) << 8) | Common.Lsn(fxP);
						else
							p.St26_Speed = Common.Msn(fxP);
					}
					break;
				}

				// Vol slide with uint8 arg
				case Effects.Fx_VolSlide_Up:
				{
					if (Common.Has_Quirk(m, Quirk_Flag.FineFx))
					{
						h = Common.Msn(fxP);
						l = Common.Lsn(fxP);

						if ((h == 0xf) && (l != 0))
						{
							fxP &= 0x0f;
							Do_Fx_F_VSlide_Up(xc, fxP);
							return;
						}
					}

					if (fxP != 0)
						xc.Vol.Slide = fxP;

					Set(xc, Channel_Flag.Vol_Slide);
					break;
				}

				// Vol slide with uint8 arg
				case Effects.Fx_VolSlide_Dn:
				{
					if (Common.Has_Quirk(m, Quirk_Flag.FineFx))
					{
						h = Common.Msn(fxP);
						l = Common.Lsn(fxP);

						if ((h == 0xf) && (l != 0))
						{
							fxP &= 0x0f;
							Do_Fx_F_VSlide_Dn(xc, fxP);
							return;
						}
					}

					if (fxP != 0)
						xc.Vol.Slide = -fxP;

					Set(xc, Channel_Flag.Vol_Slide);
					break;
				}

				// Fine volume slide
				case Effects.Fx_F_VSlide:
				{
					Set(xc, Channel_Flag.Fine_Vols);

					if (fxP != 0)
					{
						h = Common.Msn(fxP);
						l = Common.Lsn(fxP);

						xc.Vol.FSlide = h != 0 ? h : -l;
					}
					break;
				}

				case Effects.Fx_NSlide_Dn:
				case Effects.Fx_NSlide_Up:
				case Effects.Fx_NSlide_R_Dn:
				case Effects.Fx_NSlide_R_Up:
				{
					if (fxP != 0)
					{
						if ((fxT == Effects.Fx_NSlide_R_Dn) || (fxT == Effects.Fx_NSlide_R_Up))
						{
							xc.Retrig.Val = Common.Msn(fxP);
							xc.Retrig.Count = Common.Msn(fxP) + 1;
							xc.Retrig.Type = 0;
							xc.Retrig.Limit = 0;
						}

						if ((fxT == Effects.Fx_NSlide_Up) || (fxT == Effects.Fx_NSlide_R_Up))
							xc.NoteSlide.Slide = Common.Lsn(fxP);
						else
							xc.NoteSlide.Slide = -Common.Lsn(fxP);

						xc.NoteSlide.Count = xc.NoteSlide.Speed = Common.Msn(fxP);
					}

					if ((fxT == Effects.Fx_NSlide_R_Dn) || (fxT == Effects.Fx_NSlide_R_Up))
						Set(xc, Channel_Flag.Retrig);

					Set(xc, Channel_Flag.Note_Slide);
					break;
				}

				case Effects.Fx_NSlide2_Dn:
				{
					Set(xc, Channel_Flag.Note_Slide);

					xc.NoteSlide.Slide = -fxP;
					xc.NoteSlide.Count = xc.NoteSlide.Speed = 1;
					break;
				}

				case Effects.Fx_NSlide2_Up:
				{
					Set(xc, Channel_Flag.Note_Slide);

					xc.NoteSlide.Slide = fxP;
					xc.NoteSlide.Count = xc.NoteSlide.Speed = 1;
					break;
				}

				case Effects.Fx_F_NSlide_Dn:
				{
					Set(xc, Channel_Flag.Fine_NSlide);

					xc.NoteSlide.FSlide = -fxP;
					break;
				}

				case Effects.Fx_F_NSlide_Up:
				{
					Set(xc, Channel_Flag.Fine_NSlide);

					xc.NoteSlide.FSlide = fxP;
					break;
				}

				// Persistent vibrato
				case Effects.Fx_Per_Vibrato:
				{
					if (Common.Lsn(fxP) != 0)
						Set_Per(xc, Channel_Flag.Vibrato);
					else
						Reset_Per(xc, Channel_Flag.Vibrato);

					Set_Lfo_NotZero(xc.Vibrato.Lfo, Common.Lsn(fxP) << 2, Common.Msn(fxP));
					break;
				}

				// Persistent portamento up
				case Effects.Fx_Per_Porta_Up:
				{
					Set_Per(xc, Channel_Flag.PitchBend);

					xc.Freq.Slide = -fxP;
					xc.Freq.Memory = fxP;

					if (fxP == 0)
						Reset_Per(xc, Channel_Flag.PitchBend);

					break;
				}

				// Persistent portamento down
				case Effects.Fx_Per_Porta_Dn:
				{
					Set_Per(xc, Channel_Flag.PitchBend);

					xc.Freq.Slide = fxP;
					xc.Freq.Memory = fxP;

					if (fxP == 0)
						Reset_Per(xc, Channel_Flag.PitchBend);

					break;
				}

				// Persistent tone portamento
				case Effects.Fx_Per_TPorta:
				{
					if (!Is_Valid_Instrument(mod, xc.Ins))
						break;

					Set_Per(xc, Channel_Flag.TonePorta);
					Do_TonePorta(xc, note);

					xc.Porta.Slide = fxP;

					if (fxP == 0)
						Reset_Per(xc, Channel_Flag.TonePorta);

					break;
				}

				// Persistent volslide up
				case Effects.Fx_Per_VSld_Up:
				{
					Set_Per(xc, Channel_Flag.Vol_Slide);
					xc.Vol.Slide = fxP;

					if (fxP == 0)
						Reset_Per(xc, Channel_Flag.Vol_Slide);

					break;
				}

				// Persistent volslide down
				case Effects.Fx_Per_VSld_Dn:
				{
					Set_Per(xc, Channel_Flag.Vol_Slide);
					xc.Vol.Slide = -fxP;

					if (fxP == 0)
						Reset_Per(xc, Channel_Flag.Vol_Slide);

					break;
				}

				// Deep vibrato (2x)
				case Effects.Fx_Vibrato2:
				{
					Set(xc, Channel_Flag.Vibrato);
					Set_Lfo_NotZero(xc.Vibrato.Lfo, Common.Lsn(fxP) << 3, Common.Msn(fxP));
					break;
				}

				// MED 1Fxy delay x, then retrig every y
				case Effects.Fx_Med_Retrig:
				{
					// Initial delay is computed at frame loop
					Set(xc, Channel_Flag.Retrig);
					xc.Retrig.Val = Common.Lsn(fxP);
					xc.Retrig.Count = Common.Lsn(fxP) + 1;
					xc.Retrig.Type = 0;
					xc.Retrig.Limit = 0;
					break;
				}

				// Set speed and ...
				case Effects.Fx_Speed_Cp:
				{
					if (fxP != 0)
					{
						p.Speed = fxP;
						p.St26_Speed = 0;
					}

					goto case Effects.Fx_Per_Cancel;
				}

				// Cancel persistent effects
				case Effects.Fx_Per_Cancel:
				{
					xc.Per_Flags = Channel_Flag.None;
					break;
				}

				// 669 effects

				// 669 portamento up
				case Effects.Fx_669_Porta_Up:
				{
					Set_Per(xc, Channel_Flag.PitchBend);

					xc.Freq.Slide = 80 * fxP;
					xc.Freq.Memory = fxP;

					if (fxP == 0)
						Reset_Per(xc, Channel_Flag.PitchBend);

					break;
				}

				// 669 portamento down
				case Effects.Fx_669_Porta_Dn:
				{
					Set_Per(xc, Channel_Flag.PitchBend);

					xc.Freq.Slide = -80 * fxP;
					xc.Freq.Memory = fxP;

					if (fxP == 0)
						Reset_Per(xc, Channel_Flag.PitchBend);

					break;
				}

				// 669 tone portamento
				case Effects.Fx_669_TPorta:
				{
					if (!Is_Valid_Instrument(mod, xc.Ins))
						break;

					Set_Per(xc, Channel_Flag.TonePorta);
					Do_TonePorta(xc, note);

					xc.Porta.Slide = 40 * fxP;

					if (fxP == 0)
						Reset_Per(xc, Channel_Flag.TonePorta);

					break;
				}

				// 669 finetune
				case Effects.Fx_669_FineTune:
				{
					xc.FineTune = 80 * (int8)fxP;
					break;
				}

				// 669 vibrato
				case Effects.Fx_669_Vibrato:
				{
					if (Common.Lsn(fxP) != 0)
					{
						lib.lfo.LibXmp_Lfo_Set_Waveform(xc.Vibrato.Lfo, 669);
						Set_Per(xc, Channel_Flag.Vibrato);
					}
					else
						Reset_Per(xc, Channel_Flag.Vibrato);

					Set_Lfo_NotZero(xc.Vibrato.Lfo, 669, 1);
					break;
				}

				// ULT effects

				// ULT tempo
				case Effects.Fx_Ult_Tempo:
				{
					// Has unusual semantics and is hard to split into multiple
					// effects, due to ULT's two effects lanes per channel:
					// 
					// 00:    Reset both speed and BPM to the default 6/125.
					// 01-2f: Set speed
					// 30-ff: Set BPM (CIA compatible)
					if (fxP == 0)
					{
						p.Speed = 6;
						p.St26_Speed = 0;
						fxP = 125;
					}
					else if (fxP < 0x30)
						goto case Effects.Fx_S3M_Speed;

					goto case Effects.Fx_S3M_Bpm;
				}

				// ULT tone portamento
				case Effects.Fx_Ult_TPorta:
				{
					// Like normal persistent tone portamento, except:
					//
					// 1) Despite the documentation claiming 300 cancels tone
					// portamento, it actually reuses the last parameter.
					//
					// 2) A 3xx without a note will reuse the last target note.
					if (!Is_Valid_Instrument(mod, xc.Ins))
						break;

					Set_Per(xc, Channel_Flag.TonePorta);
					Effect_Memory(m, xc, ref fxP, ref xc.Porta.Memory);
					Effect_Memory(m, xc, ref note, ref xc.Porta.Note_Memory);
					Do_TonePorta(xc, note);

					xc.Porta.Slide = fxP;

					if (fxP == 0)
						Reset_Per(xc, Channel_Flag.TonePorta);

					break;
				}

				// Archimedes (!Tracker, Digital Symphony, et al.) effects

				// !Tracker and Digital Symphony "Line Jump"
				// Jump to a line within the current order. In Digital Symphony
				// this can be combined with position jump (like pattern break)
				// and overrides the pattern break line in lower channels
				case Effects.Fx_Line_Jump:
				{
					LibXmp_Process_Line_Jump(f, p.Ord, fxP);
					break;
				}

				// Retrigger with extended range
				case Effects.Fx_Retrig:
				{
					Do_Fx_Retrigger(xc, fxP, m);
					break;
				}

				default:
				{
					lib.extras.LibXmp_Extras_Process_Fx(xc, chn, note, fxT, fxP, fNum);
					break;
				}
			}
		}
	}
}
