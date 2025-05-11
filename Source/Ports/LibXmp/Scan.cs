/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.CKit;
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Common;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp;
using Polycode.NostalgicPlayer.Ports.LibXmp.FormatExtras;

namespace Polycode.NostalgicPlayer.Ports.LibXmp
{
	/// <summary>
	/// 
	/// </summary>
	internal class Scan
	{
		private const c_int VBlank_Time_Threshold = 480000;		// 8 minutes

		private readonly LibXmp lib;
		private readonly Xmp_Context ctx;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Scan(LibXmp libXmp, Xmp_Context ctx)
		{
			lib = libXmp;
			this.ctx = ctx;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int LibXmp_Get_Sequence(c_int ord)
		{
			Player_Data p = ctx.P;

			if ((ord < 0) || (ord > ctx.M.Mod.Len))
				return Constants.No_Sequence;

			return p.Sequence_Control[ord];
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int LibXmp_Scan_Sequences()
		{
			Player_Data p = ctx.P;
			Module_Data m = ctx.M;
			Xmp_Module mod = m.Mod;
			c_int i;
			byte[] temp_Ep = new byte[Constants.Xmp_Max_Mod_Length];

			Scan_Data[] s = p.Scan;
			s = ReallocateScanData(s, Math.Max(1, mod.Len));
			if (s == null)
				return -1;

			p.Scan = s;

			// Initialize order data to prevent overwrite when a position is used
			// multiple times at different starting points (see janosik.xm)
			Reset_Scan_Data();

			c_int ep = 0;
			temp_Ep[0] = 0;
			p.Scan[0].Time = Scan_Module(ep, 0);
			c_int seq = 1;

			if (m.Compare_VBlank && ((p.Flags & Xmp_Flags.VBlank) == 0) && (p.Scan[0].Time >= VBlank_Time_Threshold))
				Compare_VBlank_Scan();

			if (p.Scan[0].Time < 0)
				return -1;

			while (true)
			{
				// Scan song starting at given entry point
				// Check if any patterns left
				for (i = 0; i < mod.Len; i++)
				{
					if (p.Sequence_Control[i] == Constants.No_Sequence)
						break;
				}

				if ((i != mod.Len) && (seq < Constants.Max_Sequences))
				{
					// New entry point
					ep = i;
					temp_Ep[seq] = (uint8)ep;
					p.Scan[seq].Time = Scan_Module(ep, seq);

					if (p.Scan[seq].Time > 0)
						seq++;
				}
				else
					break;
			}

			if (seq < mod.Len)
			{
				s = ReallocateScanData(p.Scan, seq);
				if (s != null)
					p.Scan = s;
			}

			m.Num_Sequences = seq;

			// Correct playback time calculation to match new base tempo factor
			p.Current_Time = p.Current_Time * (m.Time_Factor / p.Scan_Time_Factor);
			p.Scan_Time_Factor = m.Time_Factor;

			// Now place entry points in the public accessible array
			for (i = 0; i < m.Num_Sequences; i++)
			{
				m.Seq_Data[i].Entry_Point = temp_Ep[i];
				m.Seq_Data[i].Duration = p.Scan[i].Time;
			}

			// Wipe the remaining entries so the player doesn't think they're
			// valid e.g. when handling end of module markers
			for (; i < Constants.Max_Sequences; i++)
			{
				m.Seq_Data[i].Entry_Point = 0;
				m.Seq_Data[i].Duration = 0;
			}

			// Correct any zero length temporary sequences from the scan.
			// There's no "correct" sequence for these, so copy whichever
			// valid sequence precedes the affected position
			for (i = 0; i < mod.Len; i++)
			{
				if (p.Sequence_Control[i] >= m.Num_Sequences)
					p.Sequence_Control[i] = (byte)((i > 0) ? p.Sequence_Control[i - 1] : 0);
			}

			return 0;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Scan_Module(c_int ep, c_int chain)
		{
			Player_Data p = ctx.P;
			Module_Data m = ctx.M;
			Xmp_Module mod = m.Mod;
			Xmp_Track[] tracks = new Xmp_Track[Constants.Xmp_Max_Channels];
			c_int pDelay = 0;
			Pattern_Loop[] loop = ArrayHelper.InitializeArray<Pattern_Loop>(Constants.Xmp_Max_Channels);
			c_int pat;
			c_double time_calc;

			// Was 255, but Global trash goes to 318.
			// Higher limit for MEDs, defiance.crybaby.5 has blocks with 2048+ rows
			c_int row_Limit = Common.Is_Player_Mode_Med(m) ? 3200 : 512;

			if (mod.Len == 0)
				return 0;

			for (c_int i = 0; i < mod.Len; i++)
			{
				pat = mod.Xxo[i];
				CMemory.MemSet<uint8>(m.Scan_Cnt[i], 0, pat >= mod.Pat ? 1 : mod.Xxp[pat].Rows != 0 ? mod.Xxp[pat].Rows : 1);
			}

			// Use temporary flow control so the scan can borrow the player's
			// Pattern Loop Handler
			Flow_Control f = new Flow_Control();

			f.Loop = loop;

			for (c_int i = 0; i < mod.Chn; i++)
			{
				loop[i].Start = 0;
				loop[i].Count = 0;
			}

			f.Loop_Dest = -1;
			f.Loop_Param = -1;
			f.Loop_Start = -1;
			f.Loop_Count = 0;
			f.Loop_Active_Num = 0;

			c_int line_Jump = 0;

			c_int gvl = mod.Gvl;
			c_int bpm = mod.Bpm;

			c_int speed = mod.Spd;
			c_int base_Time = (c_int)m.RRate;
			c_int st26_Speed = 0;
			c_int far_Tempo_Coarse = 4;
			c_int far_Tempo_Fine = 0;
			c_int far_Tempo_Mode = 1;

			if (ctx.M.Extra is Far_Module_Extra farExtras)
			{
				far_Tempo_Coarse = ((Far_Module_Extra.Far_Module_Extra_Info)farExtras.Module_Extras).Coarse_Tempo;
				farExtras.LibXmp_Far_Translate_Tempo(far_Tempo_Mode, 0, far_Tempo_Coarse, ref far_Tempo_Fine, ref speed, ref bpm);
			}

			bool has_Marker = Common.Has_Quirk(m, Quirk_Flag.Marker);

			// By erlk ozlr <erlk.ozlr@gmail.com>
			//
			// xmp doesn't handle really properly the "start" option (-s for the
			// command-line) for DeusEx's .umx files. These .umx files contain
			// several loop "tracks" that never join together. That's how they put
			// multiple musics on each level with a file per level. Each "track"
			// starts at the same order in all files. The problem is that xmp starts
			// decoding the module at order 0 and not at the order specified with
			// the start option. If we have a module that does "0 -> 2 -> 0 -> ...",
			// we cannot play order 1, even with the supposed right option.
			//
			// was: ord2 = ord = -1;
			//
			// CM: Fixed by using different "sequences" for each loop or subsong.
			//     Each sequence has its entry point. Sequences don't overlap.
			c_int ord2 = -1;
			c_int ord = ep - 1;

			c_int gVol_Memory = 0, break_Row = 0, row_Count = 0, row_Count_Total = 0, frame_Count = 0;
			c_int orders_Since_Last_Valid = 0;
			bool any_Valid = false;
			c_double start_Time = 0.0, time = 0.0;
			bool inside_Loop = false;
			c_int row;

			while (true)
			{
				// Sanity check to prevent getting stuck due to broken patterns
				if (orders_Since_Last_Valid > 512)
					break;

				orders_Since_Last_Valid++;

				if (++ord >= (uint32)mod.Len)
				{
					if ((mod.Rst > mod.Len) || (mod.Xxo[mod.Rst] >= mod.Pat))
						ord = ep;
					else
					{
						if (LibXmp_Get_Sequence(mod.Rst) == chain)
							ord = mod.Rst;
						else
							ord = ep;
					}

					pat = mod.Xxo[ord];
					if (has_Marker && (pat == Constants.Xmp_Mark_End))
						break;
				}

				pat = mod.Xxo[ord];
				Ord_Data info = m.Xxo_Info[ord];

				// Allow more complex order reuse only in main sequence
				if ((ep != 0) && (p.Sequence_Control[ord] != Constants.No_Sequence))
				{
					// Currently to detect the end of the sequence, the player needs the
					// end to be a real position and row, so skip invalid and S3M_SKIP.
					// "amazonas-dynomite mix.it" by Skaven has a sequence (9) where an
					// S3M_END repeats into an S3M_SKIP.
					//
					// Two sequences (7 and 8) in "alien incident - leohou2.s3m" by
					// Purple Motion share the same S3M_END due to an off-by-one jump,
					// so check S3M_END here too
					if (pat >= mod.Pat)
					{
						if (has_Marker && (pat == Constants.Xmp_Mark_End))
							ord = mod.Len;

						continue;
					}
					break;
				}

				p.Sequence_Control[ord] = (uint8)chain;

				// All invalid patterns skipped, only S3M_End aborts replay
				if (pat >= mod.Pat)
				{
					if (has_Marker && (pat == Constants.Xmp_Mark_End))
					{
						ord = mod.Len;
						continue;
					}
					continue;
				}

				if (break_Row >= mod.Xxp[pat].Rows)
					break_Row = 0;

				// Changing patterns may reset loop vars
				if (Common.Has_Flow_Mode(m, FlowMode_Flag.Loop_Pattern_Reset))
				{
					f.Loop_Start = -1;
					f.Loop_Count = 0;

					for (c_int i = 0; i < mod.Chn; i++)
					{
						f.Loop[i].Start = 0;
						f.Loop[i].Count = 0;
					}
				}

				// Loops can cross pattern boundaries, so check if we're not looping
				if ((m.Scan_Cnt[ord][break_Row] != 0) && !inside_Loop)
					break;

				// Only update pattern information if we weren't here before. This also
				// means that we don't update pattern information if we're inside a loop,
				// otherwise a loop containing e.g. a global volume fade can make the
				// pattern start with the wrong volume. (fixes xyce-dans_la_rue.xm replay,
				// see https://github.com/libxmp/libxmp/issues/153 for more details)
				if (info.Time < 0)
				{
					info.Gvl = gvl;
					info.Bpm = bpm;
					info.Speed = speed;

					// TODO: double ord_data::time
					time_calc = time + m.Time_Factor * frame_Count * base_Time / bpm;
					info.Time = time_calc > c_int.MaxValue ? c_int.MaxValue : (c_int)time_calc;

					info.St26_Speed = st26_Speed;
				}

				if ((info.Start_Row == 0) && (ord != 0))
				{
					if (ord == ep)
						start_Time = time + m.Time_Factor * frame_Count * base_Time / bpm;

					info.Start_Row = break_Row;
				}

				// Get tracks in advance to speed up the event parsing loop
				for (c_int chn = 0; chn < mod.Chn; chn++)
					tracks[chn] = mod.Xxt[Common.Track_Num(m, pat, chn)];

				c_int last_Row = mod.Xxp[pat].Rows;

				row = break_Row;
				break_Row = 0;

				for (; row < last_Row; row++, row_Count++, row_Count_Total++)
				{
					// Prevent crashes caused by large softmixer frames
					if (bpm < Constants.Xmp_Min_Bpm)
						bpm = Constants.Xmp_Min_Bpm;

					// Date: Sat, 8 Sep 2007 04:01:06 +0200
					// Reported by Zbigniew Luszpinski <zbiggy@o2.pl>
					// The scan routine falls into infinite looping and doesn't let
					// xmp play jos-dr4k.xm.
					// Claudio's workaround: we'll break infinite loops here.
					//
					// Date: Oct 27, 2007 8:05 PM
					// From: Adric Riedel <adric.riedel@gmail.com>
					// Jesper Kyd: Global Trash 3.mod (the 'Hardwired' theme) only
					// plays the first 4:41 of what should be a 10 minute piece.
					// (...) it dies at the end of position 2F
					if (row_Count_Total > row_Limit)
						goto End_Module;

					if ((f.Loop_Active_Num == 0) && (line_Jump == 0) && (m.Scan_Cnt[ord][row] != 0))
					{
						row_Count--;
						goto End_Module;
					}

					m.Scan_Cnt[ord][row]++;
					orders_Since_Last_Valid = 0;
					any_Valid = true;

					// If the scan count for this row overflows, break.
					// A scan count of 0 will help break this loop in playback (storlek_11.it)
					if (m.Scan_Cnt[ord][row] == 0)
						goto End_Module;

					pDelay = 0;
					line_Jump = 0;

					for (c_int chn = 0; chn < mod.Chn; chn++)
					{
						if (row >= tracks[chn].Rows)
							continue;

						Xmp_Event @event = tracks[chn].Event[row];

						c_int f1 = @event.FxT;
						c_int p1 = @event.FxP;
						c_int f2 = @event.F2T;
						c_int p2 = @event.F2P;

						if ((f1 == 0) && (f2 == 0))
							continue;

						c_int parm;

						if ((f1 == Effects.Fx_GlobalVol) || (f2 == Effects.Fx_GlobalVol))
						{
							gvl = (f1 == Effects.Fx_GlobalVol) ? p1 : p2;
							gvl = gvl > m.GVolBase ? m.GVolBase : gvl < 0 ? 0 : gvl;
						}

						// Process fine global volume slide
						if ((f1 == Effects.Fx_GVol_Slide) || (f2 == Effects.Fx_GVol_Slide))
						{
							c_int h, l;
							parm = (f1 == Effects.Fx_GVol_Slide) ? p1 : p2;

							Process_GVol:
							if (parm != 0)
							{
								gVol_Memory = parm;
								h = Common.Msn(parm);
								l = Common.Lsn(parm);

								if (Common.Has_Quirk(m, Quirk_Flag.FineFx))
								{
									if ((l == 0xf) && (h != 0))
										gvl += h;
									else if ((h == 0xf) && (l != 0))
										gvl -= l;
									else
									{
										if ((m.Quirk & Quirk_Flag.VsAll) != 0)
											gvl += (h - l) * speed;
										else
											gvl += (h - l) * (speed - 1);
									}
								}
								else
								{
									if ((m.Quirk & Quirk_Flag.VsAll) != 0)
										gvl += (h - l) * speed;
									else
										gvl += (h - l) * (speed - 1);
								}
							}
							else
							{
								parm = gVol_Memory;
								if (parm != 0)
									goto Process_GVol;
							}
						}

						// Some formats can have two FX_SPEED effects, and both need
						// to be checked. Slot 2 is currently handled first
						for (c_int i = 0; i < 2; i++)
						{
							parm = i != 0 ? p1 : p2;

							if (((i != 0 ? f1 : f2) != Effects.Fx_Speed) || (parm == 0))
								continue;

							frame_Count += row_Count * speed;
							row_Count = 0;

							if (Common.Has_Quirk(m, Quirk_Flag.NoBpm) || ((p.Flags & Xmp_Flags.VBlank) != 0) || (parm < 0x20))
							{
								speed = parm;
								st26_Speed = 0;
							}
							else
							{
								time += m.Time_Factor * frame_Count * base_Time / bpm;
								frame_Count = 0;
								bpm = parm;
							}
						}

						if (f1 == Effects.Fx_Speed_Cp)
							f1 = Effects.Fx_S3M_Speed;

						if (f2 == Effects.Fx_Speed_Cp)
							f2 = Effects.Fx_S3M_Speed;

						// ST2.6 speed processing
						if ((f1 == Effects.Fx_Ice_Speed) && (p1 != 0))
						{
							if (Common.Lsn(p1) != 0)
								st26_Speed = (Common.Msn(p1) << 8) | Common.Lsn(p1);
							else
								st26_Speed = Common.Msn(p1);
						}

						// FAR tempo processing
						if ((f1 == Effects.Fx_Far_Tempo) || (f1 == Effects.Fx_Far_F_Tempo))
						{
							c_int far_Speed = 0, far_Bpm = 0, fine_Change = 0;

							if (f1 == Effects.Fx_Far_Tempo)
							{
								if (Common.Msn(p1) != 0)
									far_Tempo_Mode = Common.Msn(p1) - 1;
								else
									far_Tempo_Coarse = Common.Lsn(p1);
							}

							if (f1 == Effects.Fx_Far_F_Tempo)
							{
								if (Common.Msn(p1) != 0)
								{
									far_Tempo_Fine += Common.Msn(p1);
									fine_Change = Common.Msn(p1);
								}
								else if (Common.Lsn(p1) != 0)
								{
									far_Tempo_Fine -= Common.Lsn(p1);
									fine_Change = -Common.Lsn(p1);
								}
								else
									far_Tempo_Fine = 0;
							}

							if (((Far_Module_Extra)m.Extra).LibXmp_Far_Translate_Tempo(far_Tempo_Mode, fine_Change, far_Tempo_Coarse, ref far_Tempo_Fine, ref far_Speed, ref far_Bpm) == 0)
							{
								frame_Count += row_Count * speed;
								row_Count = 0;
								time += m.Time_Factor * frame_Count * base_Time / bpm;
								frame_Count = 0;
								speed = far_Speed;
								bpm = far_Bpm;
							}
						}

						// ULT tempo processing

						if ((f1 == Effects.Fx_Ult_Tempo) || (f2 == Effects.Fx_Ult_Tempo))
						{
							c_int parm2 = 0;
							parm = 0;

							if (f2 == Effects.Fx_Ult_Tempo)
							{
								if (p2 == 0)
								{
									parm = 6;
									parm2 = 125;
								}
								else if (p2 < 0x30)
									parm = p2;
								else
									parm2 = p2;
							}

							if (f1 == Effects.Fx_Ult_Tempo)
							{
								if (p1 == 0)
								{
									parm = 6;
									parm2 = 125;
								}
								else if (p1 < 0x30)
									parm = p1;
								else
									parm2 = p1;
							}

							frame_Count += row_Count * speed;
							row_Count = 0;

							if (parm > 0)
							{
								speed = parm;
								st26_Speed = 0;
							}

							if (parm2 > 0)
							{
								time += m.Time_Factor * frame_Count * base_Time / bpm;
								frame_Count = 0;
								bpm = parm2;
							}
						}

						if (((f1 == Effects.Fx_S3M_Speed) && (p1 != 0)) || ((f2 == Effects.Fx_S3M_Speed) && (p2 != 0)))
						{
							parm = (f1 == Effects.Fx_S3M_Speed) ? p1 : p2;
							if (parm > 0)
							{
								frame_Count += row_Count * speed;
								row_Count = 0;
								speed = parm;
								st26_Speed = 0;
							}
						}

						if (((f1 == Effects.Fx_S3M_Bpm) && (p1 != 0)) || ((f2 == Effects.Fx_S3M_Bpm) && (p2 != 0)))
						{
							parm = (f1 == Effects.Fx_S3M_Bpm) ? p1 : p2;
							if (parm >= Constants.Xmp_Min_Bpm)
							{
								frame_Count += row_Count * speed;
								row_Count = 0;
								time += m.Time_Factor * frame_Count * base_Time / bpm;
								frame_Count = 0;
								bpm = parm;
							}
						}

						if (((f1 == Effects.Fx_It_Bpm) && (p1 != 0)) || ((f2 == Effects.Fx_It_Bpm) && (p2 != 0)))
						{
							parm = (f1 == Effects.Fx_It_Bpm) ? p1 : p2;
							frame_Count += row_Count * speed;
							row_Count = 0;
							time += m.Time_Factor * frame_Count * base_Time / bpm;
							frame_Count = 0;

							if (Common.Msn(parm) == 0)
							{
								time += m.Time_Factor * base_Time / bpm;

								for (c_int i = 1; i < speed; i++)
								{
									bpm -= Common.Lsn(parm);

									if (bpm < 0x20)
										bpm = 0x20;

									time += m.Time_Factor * base_Time / bpm;
								}

								// Remove one row at final bpm
								time -= m.Time_Factor * speed * base_Time / bpm;
							}
							else if (Common.Msn(parm) == 1)
							{
								time += m.Time_Factor * base_Time / bpm;

								for (c_int i = 1; i < speed; i++)
								{
									bpm += Common.Lsn(parm);

									if (bpm > 0xff)
										bpm = 0xff;

									time += m.Time_Factor * base_Time / bpm;
								}

								// Remove one row at final bpm
								time -= m.Time_Factor * speed * base_Time / bpm;
							}
							else
								bpm = parm;
						}

						if (f1 == Effects.Fx_It_RowDelay)
						{
							// Don't allow the scan count for this row to overflow here
							c_int x = m.Scan_Cnt[ord][row] + (p1 & 0x0f);
							m.Scan_Cnt[ord][row] = (uint8)Math.Min(x, 255);
							frame_Count += (p1 & 0x0f) * speed;
						}

						if ((f1 == Effects.Fx_It_Break) || (f2 == Effects.Fx_It_Break))
						{
							parm = (f1 == Effects.Fx_It_Break) ? p1 : p2;
							lib.player.LibXmp_Process_Pattern_Break(f, parm);

							// TODO: Fully replace these variables with f
							if (f.PBreak)
							{
								break_Row = f.JumpLine;
								last_Row = 0;
							}
						}

						if ((f1 == Effects.Fx_Jump) || (f2 == Effects.Fx_Jump))
						{
							lib.player.LibXmp_Process_Pattern_Jump(f, (f1 == Effects.Fx_Jump) ? p1 : p2);

							// TODO: Fully replace these variables with f
							if (f.PBreak)
							{
								ord2 = f.Jump;
								break_Row = f.JumpLine;
								last_Row = 0;

								// Prevent infinite loop, see OpenMPT PatLoop-Various.xm
								inside_Loop = false;
							}
						}

						if ((f1 == Effects.Fx_Break) || (f2 == Effects.Fx_Break))
						{
							parm = (f1 == Effects.Fx_Break) ? p1 : p2;
							parm = 10 * Common.Msn(parm) + Common.Lsn(parm);
							lib.player.LibXmp_Process_Pattern_Break(f, parm);

							// TODO: Fully replace these variables with f
							if (f.PBreak)
							{
								break_Row = f.JumpLine;
								last_Row = 0;
							}
						}

						// Archimedes line jump
						if ((f1 == Effects.Fx_Line_Jump) || (f2 == Effects.Fx_Line_Jump))
						{
							lib.player.LibXmp_Process_Line_Jump(f, ord, (f1 == Effects.Fx_Line_Jump ? p1 : p2));

							// Don't set order if preceded by jump or break.
							// TODO: Fully replace these variables with f
							if (last_Row > 0)
								ord2 = ord;

							break_Row = f.JumpLine;
							last_Row = 0;
							line_Jump = 1;
						}

						if ((f1 == Effects.Fx_Extended) || (f2 == Effects.Fx_Extended))
						{
							parm = (f1 == Effects.Fx_Extended) ? p1 : p2;

							if ((parm >> 4) == Effects.Ex_Patt_Delay)
							{
								if ((m.Read_Event_Type != Read_Event.St3) || (pDelay == 0))
									pDelay = parm & 0x0f;
							}

							if ((parm >> 4) == Effects.Ex_Pattern_Loop)
							{
								// QUIRK_FT2BUGS may set break_row
								f.JumpLine = break_Row;
								lib.player.LibXmp_Process_Pattern_Loop(f, chn, row, Common.Lsn(parm));
								break_Row = f.JumpLine;

								// Attempt to detect the inside of a loop
								// TODO: This won't detect all cases
								if ((Common.Lsn(parm) > 0) && (f.Loop_Dest < 0))
									inside_Loop = false;
								else if (Common.Lsn(parm) == 0)
									inside_Loop = true;
							}
						}
					}

					if (pDelay > 0)
						frame_Count += pDelay * speed;

					f.Loop_Param = -1;

					if (f.Loop_Dest >= 0)
					{
						// -1 as it will be incremented immediately by the loop
						row = f.Loop_Dest - 1;
						f.Loop_Dest = -1;
					}

					if (st26_Speed != 0)
					{
						frame_Count += row_Count * speed;
						row_Count = 0;

						if ((st26_Speed & 0x10000) != 0)
							speed = (st26_Speed & 0xff00) >> 8;
						else
							speed = st26_Speed & 0xff;

						st26_Speed ^= 0x10000;
					}
				}

				if ((break_Row != 0) && (pDelay != 0))
					break_Row++;

				if (ord2 >= 0)
				{
					ord = ord2 - 1;
					ord2 = -1;
				}

				frame_Count += row_Count * speed;
				row_Count_Total = 0;
				row_Count = 0;
			}

			row = break_Row;

			End_Module:
			// Sanity check
			{
				if (!any_Valid)
					return -1;

				pat = mod.Xxo[ord];
				if ((pat >= mod.Pat) || (row >= mod.Xxp[pat].Rows))
					row = 0;
			}

			p.Scan[chain].Num = m.Scan_Cnt[ord][row];
			p.Scan[chain].Row = row;
			p.Scan[chain].Ord = ord;

			time -= start_Time;
			frame_Count += row_Count * speed;

			// TODO: double ord_data::time
			time_calc = time + m.Time_Factor * frame_Count * base_Time / bpm;

			return time_calc > c_int.MaxValue ? c_int.MaxValue : (c_int)time_calc;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Reset_Scan_Data()
		{
			for (c_int i = 0; i < Constants.Xmp_Max_Mod_Length; i++)
				ctx.M.Xxo_Info[i].Time = -1;

			Array.Fill(ctx.P.Sequence_Control, (uint8)Constants.No_Sequence, 0, Constants.Xmp_Max_Mod_Length);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Compare_VBlank_Scan()
		{
			// Calculate both CIA and VBlank time for certain long MODs
			// and pick the more likely (i.e. shorter) one. The same logic
			// works regardless of the initial mode selected--either way,
			// the wrong timing mode usually makes modules MUCH longer
			Player_Data p = ctx.P;
			Module_Data m = ctx.M;
			byte[] ctrl_Backup = new byte[256];

			// Back up the current info to avoid a third scan
			Scan_Data scan_Backup = p.Scan[0];
			Ord_Data[] info_Backup = m.Xxo_Info;
			Array.Copy(p.Sequence_Control, ctrl_Backup, p.Sequence_Control.Length);

			p.Scan[0] = new Scan_Data();
			m.Xxo_Info = ArrayHelper.InitializeArray<Ord_Data>(m.Xxo_Info.Length);

			Reset_Scan_Data();

			m.Quirk ^= Quirk_Flag.NoBpm;
			p.Scan[0].Time = Scan_Module(0, 0);

			if (p.Scan[0].Time >= scan_Backup.Time)
			{
				m.Quirk ^= Quirk_Flag.NoBpm;
				p.Scan[0] = scan_Backup;
				m.Xxo_Info = info_Backup;
				Array.Copy(ctrl_Backup, p.Sequence_Control, p.Sequence_Control.Length);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Reallocate the scanning array if needed
		/// </summary>
		/********************************************************************/
		private Scan_Data[] ReallocateScanData(Scan_Data[] s, c_int num)
		{
			int oldSize = s?.Length ?? 0;

			if (s == null)
				s = new Scan_Data[num];
			else
				Array.Resize(ref s, num);

			for (int i = oldSize; i < num; i++)
				s[i] = new Scan_Data();

			return s;
		}
		#endregion
	}
}
