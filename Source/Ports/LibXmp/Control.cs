/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Common;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp;

namespace Polycode.NostalgicPlayer.Ports.LibXmp
{
	/// <summary>
	/// 
	/// </summary>
	internal class Control
	{
		private readonly LibXmp lib;
		private readonly Xmp_Context ctx;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Control(LibXmp libXmp, Xmp_Context ctx)
		{
			lib = libXmp;
			this.ctx = ctx;
		}



		/********************************************************************/
		/// <summary>
		/// Create a new player context and return an opaque handle to be
		/// used in subsequent accesses to this context
		/// </summary>
		/********************************************************************/
		public static Xmp_Context Xmp_Create_Context()
		{
			Context_Data ctx = new Context_Data();
			if (ctx == null)
				return null;

			ctx.State = Xmp_State.Unloaded;
			ctx.M.DefPan = 100;
			ctx.S.NumVoc = Constants.SMix_NumVoc;
			Rng.LibXmp_Init_Random(ctx.Rng);

			return ctx;
		}



		/********************************************************************/
		/// <summary>
		/// Destroy a player context previously created using
		/// Xmp_Create_Context()
		/// </summary>
		/********************************************************************/
		public void Xmp_Free_Context()
		{
			Module_Data m = ctx.M;

			if (ctx.State > Xmp_State.Unloaded)
				lib.Xmp_Release_Module();
		}



		/********************************************************************/
		/// <summary>
		/// Skip replay to the start of the next position
		/// </summary>
		/********************************************************************/
		public c_int Xmp_Next_Position()
		{
			Player_Data p = ctx.P;
			Module_Data m = ctx.M;

			if (ctx.State < Xmp_State.Playing)
				return -(c_int)Xmp_Error.State;

			if (p.Pos < m.Mod.Len)
				Set_Position(p.Pos + 1, 1);

			return p.Pos;
		}



		/********************************************************************/
		/// <summary>
		/// Skip replay to the start of the previous position
		/// </summary>
		/********************************************************************/
		public c_int Xmp_Prev_Position()
		{
			Player_Data p = ctx.P;
			Module_Data m = ctx.M;

			if (ctx.State < Xmp_State.Playing)
				return -(c_int)Xmp_Error.State;

			if (p.Pos == m.Seq_Data[p.Sequence].Entry_Point)
				Set_Position(-1, -1);
			else if (p.Pos > m.Seq_Data[p.Sequence].Entry_Point)
				Set_Position(p.Pos - 1, -1);

			return p.Pos < 0 ? 0 : p.Pos;
		}



		/********************************************************************/
		/// <summary>
		/// Skip replay to the start of the given position
		/// </summary>
		/********************************************************************/
		public c_int Xmp_Set_Position(c_int pos)
		{
			Player_Data p = ctx.P;
			Module_Data m = ctx.M;

			if (ctx.State < Xmp_State.Playing)
				return -(c_int)Xmp_Error.State;

			if (pos >= m.Mod.Len)
				return -(c_int)Xmp_Error.Invalid;

			Set_Position(pos, 0);

			return p.Pos;
		}



		/********************************************************************/
		/// <summary>
		/// Skip replay to the given row
		/// </summary>
		/********************************************************************/
		public c_int Xmp_Set_Row(c_int row)
		{
			Player_Data p = ctx.P;
			Module_Data m = ctx.M;
			Xmp_Module mod = m.Mod;
			Flow_Control f = p.Flow;
			c_int pos = p.Pos;

			if ((pos < 0) || (pos >= mod.Len))
				pos = 0;

			c_int pattern = mod.Xxo[pos];

			if (ctx.State < Xmp_State.Playing)
				return -(c_int)Xmp_Error.State;

			if ((pattern >= mod.Pat) || (row >= mod.Xxp[pattern].Rows))
				return -(c_int)Xmp_Error.Invalid;

			// See set position
			if (p.Pos < 0)
				p.Pos = 0;

			p.Ord = p.Pos;
			p.Row = row;
			p.Frame = -1;
			f.Num_Rows = mod.Xxp[mod.Xxo[p.Ord]].Rows;

			return row;
		}



		/********************************************************************/
		/// <summary>
		/// Stop the currently playing module
		/// </summary>
		/********************************************************************/
		public void Xmp_Stop_Module()
		{
			Player_Data p = ctx.P;

			if (ctx.State < Xmp_State.Playing)
				return;

			p.Pos = -2;
		}



		/********************************************************************/
		/// <summary>
		/// Restart the currently playing module
		/// </summary>
		/********************************************************************/
		public void Xmp_Restart_Module()
		{
			Player_Data p = ctx.P;

			if (ctx.State < Xmp_State.Playing)
				return;

			p.Loop_Count = 0;
			p.Pos = -1;
		}



		/********************************************************************/
		/// <summary>
		/// Skip replay to the specified time
		/// </summary>
		/********************************************************************/
		public c_int Xmp_Seek_Time(c_int time)
		{
			Player_Data p = ctx.P;
			Module_Data m = ctx.M;
			c_int i;

			if (ctx.State < Xmp_State.Playing)
				return -(c_int)Xmp_Error.State;

			for (i = m.Mod.Len - 1; i >= 0; i--)
			{
				c_int pat = m.Mod.Xxo[i];
				if (pat >= m.Mod.Pat)
					continue;

				if (lib.scan.LibXmp_Get_Sequence(i) != p.Sequence)
					continue;

				c_int t = m.Xxo_Info[i].Time;
				if (time >= t)
				{
					Set_Position(i, 1);
					break;
				}
			}

			if (i < 0)
				lib.Xmp_Set_Position(0);

			return p.Pos < 0 ? 0 : p.Pos;
		}



		/********************************************************************/
		/// <summary>
		/// Mute or unmute the specified channel
		/// </summary>
		/********************************************************************/
		public c_int Xmp_Channel_Mute(c_int chn, c_int status)
		{
			Player_Data p = ctx.P;

			if (ctx.State < Xmp_State.Playing)
				return -(c_int)Xmp_Error.State;

			if ((chn < 0) || (chn >= Constants.Xmp_Max_Channels))
				return -(c_int)Xmp_Error.Invalid;

			c_int ret = p.Channel_Mute[chn] ? 1 : 0;

			if (status >= 2)
				p.Channel_Mute[chn] = !p.Channel_Mute[chn];
			else if (status >= 0)
				p.Channel_Mute[chn] = status != 0;

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// Set or retrieve the volume of the specified channel
		/// </summary>
		/********************************************************************/
		public c_int Xmp_Channel_Vol(c_int chn, c_int vol)
		{
			Player_Data p = ctx.P;

			if (ctx.State < Xmp_State.Playing)
				return -(c_int)Xmp_Error.State;

			if ((chn < 0) || (chn >= Constants.Xmp_Max_Channels))
				return -(c_int)Xmp_Error.Invalid;

			c_int ret = p.Channel_Vol[chn];

			if ((vol >= 0) && (vol <= 100))
				p.Channel_Vol[chn] = vol;

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// Set player parameter with the specified value
		/// </summary>
		/********************************************************************/
		public c_int Xmp_Set_Player(Xmp_Player parm, c_int val)
		{
			Player_Data p = ctx.P;
			Module_Data m = ctx.M;
			Mixer_Data s = ctx.S;
			c_int ret = -(c_int)Xmp_Error.Invalid;

			if ((parm == Xmp_Player.SmpCtl) || (parm == Xmp_Player.DefPan))
			{
				// These should be set before loading the module
				if (ctx.State >= Xmp_State.Loaded)
					return -(c_int)Xmp_Error.State;
			}
			else if (parm == Xmp_Player.Voices)
			{
				// These should be set before start playing
				if (ctx.State >= Xmp_State.Playing)
					return -(c_int)Xmp_Error.State;
			}
			else if (ctx.State < Xmp_State.Playing)
				return -(c_int)Xmp_Error.State;

			switch (parm)
			{
				case Xmp_Player.Amp:
				{
					if ((val >= 0) && (val <= 3))
					{
						s.Amplify = val;
						ret = 0;
					}
					break;
				}

				case Xmp_Player.Mix:
				{
					if ((val >= -100) && (val <= 100))
					{
						s.Mix = val;
						ret = 0;
					}
					break;
				}

				case Xmp_Player.Interp:
				{
					Xmp_Interp v = (Xmp_Interp)val;

					if ((v >= Xmp_Interp.None) && (v <= Xmp_Interp.Spline))
					{
						s.Interp = v;
						ret = 0;
					}
					break;
				}

				case Xmp_Player.Dsp:
				{
					s.Dsp = (Xmp_Dsp)val;
					ret = 0;
					break;
				}

				case Xmp_Player.Flags:
				{
					p.Player_Flags = (Xmp_Flags)val;
					ret = 0;
					break;
				}

				// 4.1
				case Xmp_Player.CFlags:
				{
					bool vBlank = (p.Flags & Xmp_Flags.VBlank) != 0;
					p.Flags = (Xmp_Flags)val;

					if (vBlank != ((p.Flags & Xmp_Flags.VBlank) != 0))
						lib.scan.LibXmp_Scan_Sequences();

					ret = 0;
					break;
				}

				case Xmp_Player.SmpCtl:
				{
					m.SmpCtl = (Xmp_SmpCtl_Flag)val;
					ret = 0;
					break;
				}

				case Xmp_Player.Volume:
				{
					if ((val >= 0) && (val <= 200))
					{
						p.Master_Vol = val;
						ret = 0;
					}
					break;
				}

				// 4.3
				case Xmp_Player.DefPan:
				{
					if ((val >= 0) && (val <= 100))
					{
						m.DefPan = val;
						ret = 0;
					}
					break;
				}

				// 4.4
				case Xmp_Player.Mode:
				{
					p.Mode = (Xmp_Mode)val;
					lib.loadHelpers.LibXmp_Set_Player_Mode();
					lib.scan.LibXmp_Scan_Sequences();
					ret = 0;
					break;
				}

				case Xmp_Player.Voices:
				{
					s.NumVoc = val;
					break;
				}

				// NostalgicPlayer
				case Xmp_Player.MixerFrequency:
				{
					if ((val >= Constants.Xmp_Min_SRate) && (val <= Constants.Xmp_Max_SRate))
						s.Freq = val;

					break;
				}

				case Xmp_Player.MixerChannels:
				{
					if (val == 1)
						s.Format |= Xmp_Format.Mono;
					else if (val == 2)
						s.Format &= ~Xmp_Format.Mono;

					break;
				}

				case Xmp_Player.Surround:
				{
					s.EnableSurround = val != 0;
					break;
				}
			}

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// Retrieve current value of the specified player parameter
		/// </summary>
		/********************************************************************/
		public c_int Xmp_Get_Player(Xmp_Player parm)
		{
			Player_Data p = ctx.P;
			Module_Data m = ctx.M;
			Mixer_Data s = ctx.S;
			c_int ret = -(c_int)Xmp_Error.Invalid;

			if ((parm == Xmp_Player.SmpCtl) || (parm == Xmp_Player.DefPan))
			{
				// Can read these at any time
			}
			else if ((parm != Xmp_Player.State) && (ctx.State < Xmp_State.Playing))
				return -(c_int)Xmp_Error.State;

			switch (parm)
			{
				case Xmp_Player.Amp:
				{
					ret = s.Amplify;
					break;
				}

				case Xmp_Player.Mix:
				{
					ret = s.Mix;
					break;
				}

				case Xmp_Player.Interp:
				{
					ret = (c_int)s.Interp;
					break;
				}

				case Xmp_Player.Dsp:
				{
					ret = (c_int)s.Dsp;
					break;
				}

				case Xmp_Player.Flags:
				{
					ret = (c_int)p.Player_Flags;
					break;
				}

				// 4.1
				case Xmp_Player.CFlags:
				{
					ret = (c_int)p.Flags;
					break;
				}

				case Xmp_Player.SmpCtl:
				{
					ret = (c_int)m.SmpCtl;
					break;
				}

				case Xmp_Player.Volume:
				{
					ret = p.Master_Vol;
					break;
				}

				// 4.2
				case Xmp_Player.State:
				{
					ret = (c_int)ctx.State;
					break;
				}

				// 4.3
				case Xmp_Player.DefPan:
				{
					ret = m.DefPan;
					break;
				}

				// 4.4
				case Xmp_Player.Mode:
				{
					ret = (c_int)p.Mode;
					break;
				}

				case Xmp_Player.Mixer_Type:
				{
					ret = (c_int)Xmp_Mixer.Standard;
					break;
				}

				case Xmp_Player.Voices:
				{
					ret = s.NumVoc;
					break;
				}
			}

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// Query the list of supported module formats
		/// </summary>
		/********************************************************************/
		public static string[] Xmp_Get_Format_List()
		{
			return Format.Format_List();
		}



		/********************************************************************/
		/// <summary>
		/// Query the list of supported module formats
		/// </summary>
		/********************************************************************/
		public static Xmp_Format_Info[] Xmp_Get_Format_Info_List()
		{
			return Format.Format_Info_List();
		}



		/********************************************************************/
		/// <summary>
		/// Dynamically insert a new event into a playing module
		/// </summary>
		/********************************************************************/
		public void Xmp_Inject_Event(c_int channel, Xmp_Event e)
		{
			Player_Data p = ctx.P;

			if (ctx.State < Xmp_State.Playing)
				return;

			p.Inject_Event[channel].CopyFrom(e);
			p.Inject_Event[channel]._Flag = 1;
		}



		/********************************************************************/
		/// <summary>
		/// Modify the replay tempo multiplier
		/// </summary>
		/********************************************************************/
		public c_int Xmp_Set_Tempo_Factor(c_double val)
		{
			Player_Data p = ctx.P;
			Module_Data m = ctx.M;
			Mixer_Data s = ctx.S;

			// This function relies on values initialized by xmp_start_player
			// and will behave in an undefined manner if called prior
			if (ctx.State < Xmp_State.Playing)
				return -(c_int)Xmp_Error.State;

			if ((val < 0.0) || (c_double.IsNaN(val)))
				return -1;

			val *= 10;

			// s->freq can change between xmp_start_player calls and p->bpm can
			// change during playback, so repeat these checks in the mixer
			c_int tickSize = lib.mixer.LibXmp_Mixer_Get_TickSize(s.Freq, val, m.RRate, p.Bpm);

			// ticksize is in frames, XMP_MAX_FRAMESIZE is in frames * 2
			if ((tickSize < 0) || (tickSize > (Constants.Xmp_Max_FrameSize / 2)))
				return -1;

			m.Time_Factor = val;

			return 0;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Set_Position(c_int pos, c_int dir)
		{
			Player_Data p = ctx.P;
			Module_Data m = ctx.M;
			Xmp_Module mod = m.Mod;
			Flow_Control f = p.Flow;
			c_int seq;

			// If dir is 0, we can jump to a different sequence
			if (dir == 0)
				seq = lib.scan.LibXmp_Get_Sequence(pos);
			else
				seq = p.Sequence;

			if (seq == 0xff)
				return;

			bool has_Marker = Common.Has_Quirk(m, Quirk_Flag.Marker);

			if (seq >= 0)
			{
				c_int start = m.Seq_Data[seq].Entry_Point;

				p.Sequence = seq;

				if (pos >= 0)
				{
					while (has_Marker && (mod.Xxo[pos] == 0xfe))
					{
						if (dir < 0)
						{
							if (pos > start)
								pos--;
						}
						else
							pos++;
					}

					c_int pat = mod.Xxo[pos];

					if (pat < mod.Pat)
					{
						if (has_Marker && (pat == 0xff))
							return;

						if (pos > p.Scan[seq].Ord)
							f.End_Point = 0;
						else
						{
							f.Num_Rows = mod.Xxp[pat].Rows;
							f.End_Point = p.Scan[seq].Num;
							f.JumpLine = 0;
						}
					}
				}

				if (pos < mod.Len)
				{
					if (pos == 0)
						p.Pos = -1;
					else
						p.Pos = pos;

					// Clear flow vars to prevent old pattern jumps and
					// other junk from executing in the new position
					lib.player.LibXmp_Reset_Flow();
				}
			}
		}
		#endregion
	}
}
