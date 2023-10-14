/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Common;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Mixer;
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
		#region Retrig_Control class
		private class Retrig_Control
		{
			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public Retrig_Control(c_int s, c_int m, c_int d)
			{
				S = s;
				M = m;
				D = d;
			}

			public c_int S;
			public c_int M;
			public c_int D;
		}
		#endregion

		#region Midi_Stream class
		private class Midi_Stream
		{
			public sbyte[] Data;
			public c_int Pos;
			public c_int Buffer;
			public c_int Param;
		}
		#endregion

		// Values for multi-retrig
		private static readonly Retrig_Control[] rVal = new Retrig_Control[]
		{
			new Retrig_Control( 0, 1, 1), new Retrig_Control( -1, 1, 1), new Retrig_Control(-2, 1, 1), new Retrig_Control(-4, 1, 1),
			new Retrig_Control(-8, 1, 1), new Retrig_Control(-16, 1, 1), new Retrig_Control( 0, 2, 3), new Retrig_Control( 0, 1, 2),
			new Retrig_Control( 0, 1, 1), new Retrig_Control(  1, 1, 1), new Retrig_Control( 2, 1, 1), new Retrig_Control( 4, 1, 1),
			new Retrig_Control( 8, 1, 1), new Retrig_Control( 16, 1, 1), new Retrig_Control( 0, 3, 2), new Retrig_Control( 0, 2, 1),
			new Retrig_Control (0, 0, 1)
		};

		private static readonly c_int[] invLoop_Table =
		{
			0, 5, 6, 7, 8, 10, 11, 13, 16, 19, 22, 26, 32, 43, 64, 128
		};

		private readonly LibXmp lib;
		private readonly Xmp_Context ctx;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Player(LibXmp libXmp, Xmp_Context ctx)
		{
			lib = libXmp;
			this.ctx = ctx;
		}



		/********************************************************************/
		/// <summary>
		/// Set note action for LibXmp_Virt_PastNote
		/// </summary>
		/********************************************************************/
		public void LibXmp_Player_Set_Release(c_int chn)
		{
			Player_Data p = ctx.P;
			Channel_Data xc = p.Xc_Data[chn];

			Set_Note(xc, Note_Flag.Release);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void LibXmp_Player_Set_FadeOut(c_int chn)
		{
			Player_Data p = ctx.P;
			Channel_Data xc = p.Xc_Data[chn];

			Set_Note(xc, Note_Flag.FadeOut);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void LibXmp_Reset_Flow()
		{
			Flow_Control f = ctx.P.Flow;

			f.JumpLine = 0;
			f.Jump = -1;
			f.PBreak = false;
			f.Loop_Chn = 0;
			f.Delay = 0;
			f.RowDelay = RowDelay_Flag.None;
			f.RowDelay_Set = RowDelay_Flag.None;
			f.Jump_In_Pat = -1;
		}



		/********************************************************************/
		/// <summary>
		/// Start playing the currently loaded module
		/// </summary>
		/********************************************************************/
		public c_int Xmp_Start_Player(c_int rate, Xmp_Format format)
		{
			Player_Data p = ctx.P;
			Module_Data m = ctx.M;
			Xmp_Module mod = m.Mod;
			Flow_Control f = p.Flow;
			c_int ret = 0;

			if ((rate < Constants.Xmp_Min_SRate) || (rate > Constants.Xmp_Max_SRate))
				return -(c_int)Xmp_Error.Invalid;

			if (ctx.State < Xmp_State.Loaded)
				return -(c_int)Xmp_Error.State;

			if (ctx.State > Xmp_State.Loaded)
				lib.Xmp_End_Player();

			if (lib.mixer.LibXmp_Mixer_On(rate, format, m.C4Rate) < 0)
				return -(c_int)Xmp_Error.Internal;

			p.Master_Vol = 100;
			p.GVol = m.VolBase;
			p.Pos = p.Ord = 0;
			p.Frame = -1;
			p.Row = 0;
			p.Current_Time = 0;
			p.Loop_Count = 0;
			p.Sequence = 0;

			// Set default volume and mute status
			for (c_int i = 0; i < mod.Chn; i++)
			{
				if ((mod.Xxc[i].Flg & Xmp_Channel_Flag.Mute) != 0)
					p.Channel_Mute[i] = true;

				p.Channel_Vol[i] = 100;
			}

			for (c_int i = mod.Chn; i < Constants.Xmp_Max_Channels; i++)
			{
				p.Channel_Mute[i] = false;
				p.Channel_Vol[i] = 100;
			}

			// Skip invalid patterns at start (the seventh laboratory.it)
			while ((p.Ord < mod.Len) && (mod.Xxo[p.Ord] >= mod.Pat))
				p.Ord++;

			// Check if all positions skipped
			if (p.Ord >= mod.Len)
				mod.Len = 0;

			if (mod.Len == 0)
			{
				// Set variables to sane state
				// Note: previously did this for mod->chn == 0, which caused
				// crashes on invalid order 0s. 0 channel modules are technically
				// valid (if useless) so just let them play normally
				p.Ord = p.Scan[0].Ord = 0;
				p.Row = p.Scan[0].Row = 0;
				f.End_Point = 0;
				f.Num_Rows = 0;
			}
			else
			{
				f.Num_Rows = mod.Xxp[mod.Xxo[p.Ord]].Rows;
				f.End_Point = p.Scan[0].Num;
			}

			Update_From_Ord_Info();

			if (lib.virt.LibXmp_Virt_On(mod.Chn) != 0)
			{
				ret = -(c_int)Xmp_Error.Internal;
				goto Err;
			}

			LibXmp_Reset_Flow();

			f.Loop = ArrayHelper.InitializeArray<Pattern_Loop>(p.Virt.Virt_Channels);
			if (f.Loop == null)
			{
				ret = -(c_int)Xmp_Error.System;
				goto Err;
			}

			p.Xc_Data = ArrayHelper.InitializeArray<Channel_Data>(p.Virt.Virt_Channels);
			if (p.Xc_Data == null)
			{
				ret = -(c_int)Xmp_Error.System;
				goto Err1;
			}

			// Reset our buffer pointers
			lib.Xmp_Play_Buffer(null, 0, 0);

			for (c_int i = 0; i < p.Virt.Virt_Channels; i++)
			{
				Channel_Data xc = p.Xc_Data[i];
				xc.Filter.CutOff = 0xff;

				if (lib.extras.LibXmp_New_Channel_Extras(xc) < 0)
					goto Err2;
			}

			Reset_Channels();

			ctx.State = Xmp_State.Playing;

			return 0;

			Err2:
			p.Xc_Data = null;

			Err1:
			f.Loop = null;

			Err:
			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// Play one frame of the module. Modules usually play at 50 frames
		/// per second
		/// </summary>
		/********************************************************************/
		public c_int Xmp_Play_Frame()
		{
			Player_Data p = ctx.P;
			Module_Data m = ctx.M;
			Xmp_Module mod = m.Mod;
			Flow_Control f = p.Flow;

			if (ctx.State < Xmp_State.Playing)
				return -(c_int)Xmp_Error.State;

			if (mod.Len <= 0)
				return -(c_int)Xmp_Error.End;

			if (Common.Has_Quirk(m, Quirk_Flag.Marker) && (mod.Xxo[p.Ord] == 0xff))
				return -(c_int)Xmp_Error.End;

			lib.mixer.LibXmp_Mixer_Prepare_Frame();

			// Check reposition
			if (p.Ord != p.Pos)
			{
				c_int start = m.Seq_Data[p.Sequence].Entry_Point;

				if (p.Pos == -2)					// Set by xmp_module_stop
					return -(c_int)Xmp_Error.End;	// That's all folks

				if (p.Pos == -1)
				{
					// Restart sequence
					p.Pos = start;
				}

				if (p.Pos == start)
					f.End_Point = p.Scan[p.Sequence].Num;

				// Check if lands after a loop point
				if (p.Pos > p.Scan[p.Sequence].Ord)
					f.End_Point = 0;

				f.JumpLine = 0;
				f.Jump = -1;

				p.Ord = p.Pos - 1;

				// Stay inside our subsong
				if (p.Ord < start)
					p.Ord = start - 1;

				Next_Order();

				Update_From_Ord_Info();

				lib.virt.LibXmp_Virt_Reset();
				Reset_Channels();
			}
			else
			{
				p.Frame++;

				if (p.Frame >= (p.Speed * (1 + f.Delay)))
				{
					// If break during pattern delay, next row is skipped.
					// See corruption.mod order 1D (pattern 0D) last line:
					// EE2 + D31 ignores D00 in order 1C line 31. Reported
					// by The Welder <welder@majesty.net>, Jan 14 2012
					if (Common.Has_Quirk(m, Quirk_Flag.ProTrack) && (f.Delay != 0) && f.PBreak)
					{
						Next_Row();
						Check_End_Of_Module();
					}

					Next_Row();
				}
			}

			for (c_int i = 0; i < mod.Chn; i++)
			{
				Channel_Data xc = p.Xc_Data[i];
				Reset(xc, Channel_Flag.Key_Off);
			}

			// Check new row

			if (p.Frame == 0)		// First frame in row
			{
				Check_End_Of_Module();
				Read_Row(mod.Xxo[p.Ord], p.Row);

				if (p.St26_Speed != 0)
				{
					if ((p.St26_Speed & 0x10000) != 0)
						p.Speed = (p.St26_Speed & 0xff00) >> 8;
					else
						p.Speed = p.St26_Speed & 0xff;

					p.St26_Speed ^= 0x10000;
				}
			}

			Inject_Event();

			// Play frame
			for (c_int i = 0; i < p.Virt.Virt_Channels; i++)
				Play_Channel(i);

			f.RowDelay_Set &= ~RowDelay_Flag.First_Frame;

			p.Frame_Time = m.Time_Factor * m.RRate / p.Bpm;
			p.Current_Time += p.Frame_Time;

			lib.mixer.LibXmp_Mixer_SoftMixer();

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Fill the buffer with PCM data up to the specified size. This is a
		/// convenience function that calls xmp_play_frame() internally to
		/// fill the user-supplied buffer. Don't call both xmp_play_frame()
		/// and xmp_play_buffer() in the same replay loop. If you don't need
		/// equally sized data chunks, xmp_play_frame() may result in better
		/// performance. Also note that silence is added at the end of a
		/// buffer if the module ends and no loop is to be performed
		/// </summary>
		/********************************************************************/
		public c_int Xmp_Play_Buffer(Array out_Buffer, c_int size, c_int loop)
		{
			Player_Data p = ctx.P;

			// Reset internal state
			// Syncs buffer start with frame start
			if (out_Buffer == null)
			{
				p.Loop_Count = 0;
				p.Buffer_Data.Consumed = 0;
				p.Buffer_Data.In_Size = 0;

				return 0;
			}

			if (ctx.State < Xmp_State.Playing)
				return -(c_int)Xmp_Error.State;

			// This port do not do any mixing by itself, so we return with an error
			return -(c_int)Xmp_Error.Internal;
		}



		/********************************************************************/
		/// <summary>
		/// End module replay and release player memory
		/// </summary>
		/********************************************************************/
		public void Xmp_End_Player()
		{
			Player_Data p = ctx.P;
			Flow_Control f = p.Flow;

			if (ctx.State < Xmp_State.Playing)
				return;

			ctx.State = Xmp_State.Loaded;

			// Free channel extras
			for (c_int i = 0; i < p.Virt.Virt_Channels; i++)
			{
				Channel_Data xc = p.Xc_Data[i];
				lib.extras.LibXmp_Release_Channel_Extras(xc);
			}

			lib.virt.LibXmp_Virt_Off();

			p.Xc_Data = null;
			f.Loop = null;

			lib.mixer.LibXmp_Mixer_Off();
		}



		/********************************************************************/
		/// <summary>
		/// Retrieve current module data
		/// </summary>
		/********************************************************************/
		public void Xmp_Get_Module_Info(out Xmp_Module_Info info)
		{
			Module_Data m = ctx.M;
			Xmp_Module mod = m.Mod;

			if (ctx.State < Xmp_State.Loaded)
			{
				info = null;
				return;
			}

			info = new Xmp_Module_Info();
			Array.Copy(m.Md5, info.Md5, 16);
			info.Mod = mod;
			info.Comment = m.Comment;
			info.Num_Sequences = m.Num_Sequences;
			info.Seq_Data = m.Seq_Data;
			info.Vol_Base = m.VolBase;
			info.C5Speeds = m.Xtra?.Select(x => x.C5Spd).ToArray();
		}



		/********************************************************************/
		/// <summary>
		/// Retrieve the current frame data
		/// </summary>
		/********************************************************************/
		public void Xmp_Get_Frame_Info(out Xmp_Frame_Info info)
		{
			Player_Data p = ctx.P;
			Mixer_Data s = ctx.S;
			Module_Data m = ctx.M;
			Xmp_Module mod = m.Mod;

			if (ctx.State < Xmp_State.Loaded)
			{
				info = null;
				return;
			}

			info = new Xmp_Frame_Info();

			c_int chn = mod.Chn;

			if ((p.Pos >= 0) && (p.Pos < mod.Len))
				info.Pos = p.Pos;
			else
				info.Pos = 0;

			info.Pattern = mod.Xxo[info.Pos];

			if (info.Pattern < mod.Pat)
				info.Num_Rows = mod.Xxp[info.Pattern].Rows;
			else
				info.Num_Rows = 0;

			info.Row = p.Row;
			info.Frame = p.Frame;
			info.Speed = p.Speed;
			info.Bpm = p.Bpm;
			info.Total_Time = p.Scan[p.Sequence].Time;
			info.Frame_Time = (c_int)(p.Frame_Time * 1000);
			info.Time = (c_int)p.Current_Time;
			info.Buffer = s.Buffer;

			info.Total_Size = Constants.Xmp_Max_FrameSize;
			info.Buffer_Size = s.TickSize;

			if ((~s.Format & Xmp_Format.Mono) != 0)
				info.Buffer_Size *= 2;

			if ((~s.Format & Xmp_Format._8Bit) != 0)
				info.Buffer_Size *= 2;

			info.Volume = p.GVol;
			info.Loop_Count = p.Loop_Count;
			info.Virt_Channels = p.Virt.Virt_Channels;
			info.Virt_Used = p.Virt.Virt_Used;

			info.Sequence = p.Sequence;
			info.Filter = p.Filter;

			if (p.Xc_Data != null)
			{
				for (c_int i = 0; i < chn; i++)
				{
					Channel_Data c = p.Xc_Data[i];
					Xmp_Channel_Info ci = info.Channel_Info[i];

					ci.Note = (byte)c.Key;
					ci.PitchBend = (c_short)c.Info_PitchBend;
					ci.Period = (c_uint)c.Info_Period;
					ci.Position = (c_uint)c.Info_Position;
					ci.Instrument = (byte)c.Ins;
					ci.Sample = (byte)c.Smp;
					ci.Volume = (byte)(c.Info_FinalVol >> 4);
					ci.Pan = (byte)c.Info_FinalPan;
					ci.Reserved = 0;
					ci.Event.Clear();

					if ((info.Pattern < mod.Pat) && (info.Row < info.Num_Rows))
					{
						c_int trk = mod.Xxp[info.Pattern].Index[i];
						Xmp_Track track = mod.Xxt[trk];

						if (info.Row < track.Rows)
						{
							Xmp_Event @event = track.Event[info.Row];
							ci.Event.CopyFrom(@event);
						}
					}
				}
			}
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Set(Channel_Data xc, Channel_Flag flag)
		{
			xc.Flags |= flag;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Reset(Channel_Data xc, Channel_Flag flag)
		{
			xc.Flags &= ~flag;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private bool Test(Channel_Data xc, Channel_Flag flag)
		{
			return (xc.Flags & flag) != 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Set_Per(Channel_Data xc, Channel_Flag flag)
		{
			xc.Per_Flags |= flag;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Reset_Per(Channel_Data xc, Channel_Flag flag)
		{
			xc.Per_Flags &= ~flag;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private bool Test_Per(Channel_Data xc, Channel_Flag flag)
		{
			return (xc.Per_Flags & flag) != 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Set_Note(Channel_Data xc, Note_Flag flag)
		{
			xc.Note_Flags |= flag;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Reset_Note(Channel_Data xc, Note_Flag flag)
		{
			xc.Note_Flags &= ~flag;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private bool Test_Note(Channel_Data xc, Note_Flag flag)
		{
			return (xc.Note_Flags & flag) != 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private bool Is_Valid_Instrument(Xmp_Module mod, c_int x)
		{
			return ((uint32)x < mod.Ins) && (mod.Xxi[x].Nsm > 0);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private bool Is_Valid_Instrument_Or_Sfx(Xmp_Module mod, c_int x)
		{
			return ((uint32)x < mod.Ins) && (mod.Xxi[x].Nsm > 0);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private bool Is_Valid_Sample(Xmp_Module mod, c_int x)
		{
			return ((uint32)x < mod.Smp) && (mod.Xxs[x].Data != null);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private bool Is_Valid_Note(c_int x)
		{
			return (uint32)x < Constants.Xmp_Max_Keys;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private bool Check_Envelope_End(Xmp_Envelope env, c_int x)
		{
			int16[] data = env.Data;

			if (((~env.Flg & Xmp_Envelope_Flag.On) != 0) || (env.Npt <= 0))
				return false;

			c_int idx = (env.Npt - 1) * 2;

			// Last node
			if ((x >= data[idx] || (idx == 0)))
			{
				if ((~env.Flg & Xmp_Envelope_Flag.Loop) != 0)
					return true;
			}

			return false;
		}



		/********************************************************************/
		/// <summary>
		/// Returns: 0 if do nothing, &lt;0 to reset channel, &gt;0 if has fade
		/// </summary>
		/********************************************************************/
		private c_int Check_Envelope_Fade(Xmp_Envelope env, c_int x)
		{
			int16[] data = env.Data;

			if ((~env.Flg & Xmp_Envelope_Flag.On) != 0)
				return 0;

			c_int idx = (env.Npt - 1) * 2;		// Last node

			if (x > data[idx])
			{
				if (data[idx + 1] == 0)
					return -1;
				else
					return 1;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Get_Envelope(Xmp_Envelope env, c_int x, c_int def)
		{
			int16[] data = env.Data;

			if ((x < 0) || ((~env.Flg & Xmp_Envelope_Flag.On) != 0) || (env.Npt <= 0))
				return def;

			c_int idx = (env.Npt - 1) * 2;

			c_int x1 = data[idx];	// Last node
			if ((x >= x1) || (idx == 0))
				return data[idx + 1];

			do
			{
				idx -= 2;
				x1 = data[idx];
			}
			while ((idx > 0) && (x1 > x));

			// Interpolate
			c_int y1 = data[idx + 1];
			c_int x2 = data[idx + 2];
			c_int y2 = data[idx + 3];

			// Interpolation requires x1 <= x <= x2
			if ((x < x1) || (x2 < x1))
				return y1;

			return x2 == x1 ? y2 : ((y2 - y1) * (x - x1) / (x2 - x1)) + y1;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Update_Envelope_Default(Xmp_Envelope env, c_int x, bool release)
		{
			int16[] data = env.Data;

			bool has_Loop = (env.Flg & Xmp_Envelope_Flag.Loop) != 0;
			bool has_Sus = (env.Flg & Xmp_Envelope_Flag.Sus) != 0;

			c_int lps = env.Lps << 1;
			c_int lpe = env.Lpe << 1;
			c_int sus = env.Sus << 1;

			// FT2 and IT envelopes behave in a different way regarding loops,
			// sustain and release. When the sustain point is at the end of the
			// envelope loop end and the key is released, FT2 escapes the loop
			// while IT runs another iteration. (See EnvLoops.xm in the OpenMPT
			// test cases)
			if (has_Loop && has_Sus && sus == lpe)
			{
				if (!release)
					has_Sus = false;
			}

			// If the envelope point is set to somewhere after the sustain point
			// or sustain loop, enable release to prevent the envelope point to
			// return to the sustain point or loop start. (See Filip Skutela's
			// farewell_tear.xm)
			if (has_Loop && (x > data[lpe] + 1))
				release = true;
			else if (has_Sus && x > data[sus] + 1)
				release = true;

			// If enabled, stay at the sustain loop
			if (has_Sus && !release)
			{
				if (x >= data[sus])
					x = data[sus];
			}

			// Envelope loops
			if (has_Loop && (x >= data[lpe]))
			{
				if (!(release && has_Sus && (sus == lpe)))
					x = data[lps];
			}

			return x;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Update_Envelope_Xm(Xmp_Envelope env, c_int x, bool release)
		{
			int16[] data = env.Data;

			bool has_Loop = (env.Flg & Xmp_Envelope_Flag.Loop) != 0;
			bool has_Sus = (env.Flg & Xmp_Envelope_Flag.Sus) != 0;

			c_int lps = env.Lps << 1;
			c_int lpe = env.Lpe << 1;
			c_int sus = env.Sus << 1;

			// FT2 and IT envelopes behave in a different way regarding loops,
			// sustain and release. When the sustain point is at the end of the
			// envelope loop end and the key is released, FT2 escapes the loop
			// while IT runs another iteration. (See EnvLoops.xm in the OpenMPT
			// test cases)
			if (has_Loop && has_Sus && sus == lpe)
			{
				if (!release)
					has_Sus = false;
			}

			if (has_Sus && x > data[sus] + 1)
				release = true;

			// If enabled, stay at the sustain loop
			if (has_Sus && !release)
			{
				if (x >= data[sus])
					x = data[sus];
			}

			// Envelope loops
			//
			// If the envelope point is set to somewhere after the sustain point
			// or sustain loop, the loop point is ignored to prevent the envelope
			// point to return to the sustain point or loop start. (See Filip Skutela's
			// farewell_tear.xm or Ebony Owl Netsuke.xm)
			if (has_Loop && (x == data[lpe]))
			{
				if (!(release && has_Sus && (sus == lpe)))
					x = data[lps];
			}

			return x;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Update_Envelope_It(Xmp_Envelope env, c_int x, bool release, bool key_Off)
		{
			int16[] data = env.Data;

			bool has_Loop = (env.Flg & Xmp_Envelope_Flag.Loop) != 0;
			bool has_Sus = (env.Flg & Xmp_Envelope_Flag.Sus) != 0;

			c_int lps = env.Lps << 1;
			c_int lpe = env.Lpe << 1;
			c_int sus = env.Sus << 1;
			c_int sue = env.Sue << 1;

			// Release at the end of a sustain loop, run another loop
			if (has_Sus && key_Off && (x == data[sue] + 1))
				x = data[sus];
			else
			{
				// If enabled, stay in the sustain loop
				if (has_Sus && !release)
				{
					if (x == data[sue] + 1)
						x = data[sus];
				}
				else
				{
					// Finally, execute the envelope loop
					if (has_Loop)
					{
						if (x > data[lpe])
							x = data[lps];
					}
				}
			}

			return x;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Update_Envelope(Xmp_Envelope env, c_int x, bool release, bool key_Off)
		{
			Module_Data m = ctx.M;

			if (x < 0xffff)		// Increment tick
				x++;

			if (x < 0)
				return -1;

			if (((~env.Flg & Xmp_Envelope_Flag.On) != 0) || (env.Npt <= 0))
				return x;

			return Common.Is_Player_Mode_It(m)
				? Update_Envelope_It(env, x, release, key_Off)
				: Common.Has_Quirk(m, Quirk_Flag.Ft2Env)
					? Update_Envelope_Xm(env, x, release)
					: Update_Envelope_Default(env, x, release);
		}


		// Impulse Tracker's filter effects are implemented using its MIDI macros.
		// Any module can customize these and they are parameterized using various
		// player and mixer values, which requires parsing them here instead of in
		// the loader. Since they're MIDI macros, they can contain actual MIDI junk
		// that needs to be skipped, and one macro may have multiple IT commands


		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Midi_Nibble(Channel_Data xc, c_int chn, Midi_Stream @in)
		{
			c_int @byte = -1;

			if (@in.Buffer >= 0)
			{
				c_int val = @in.Buffer;
				@in.Buffer = -1;

				return val;
			}

			while (@in.Data[@in.Pos] != 0)
			{
				c_int val = @in.Data[@in.Pos++];

				if ((val >= '0') && (val <= '9'))
					return val - '0';

				if ((val >= 'A') && (val <= 'F'))
					return val - 'A' + 10;

				switch (val)
				{
					// Macro parameter
					case 'z':
					{
						@byte = @in.Param;
						break;
					}

					// Host key
					case 'n':
					{
						@byte = xc.Key & 0x7f;
						break;
					}

					// Host channel
					case 'h':
					{
						@byte = chn;
						break;
					}

					// Offset effect memory
					case 'o':
					{
						// Intentionally not clamped, see ZxxSecrets.it
						@byte = xc.Offset.Memory;
						break;
					}

					// Voice reverse flag
					case 'm':
					{
						c_int voc = lib.virt.LibXmp_Virt_MapChannel(chn);
						Mixer_Voice vi = (voc >= 0) ? ctx.P.Virt.Voice_Array[voc] : null;
						@byte = vi != null ? (vi.Flags & Mixer_Flag.Voice_Reverse) != 0 ? 1 : 0 : 0;
						break;
					}

					// Note velocity
					case 'v':
					{
						Xmp_Instrument xxi = lib.sMix.LibXmp_Get_Instrument(xc.Ins);
						@byte = (c_int)(((uint32)ctx.P.GVol * (uint32)xc.Volume * (uint32)xc.MasterVol * (uint32)xc.Gvl * (uint32)(xxi != null ? xxi.Vol : 0x40)) >> 24);
						Common.Clamp(ref @byte, 1, 127);
						break;
					}

					// Computed velocity
					case 'u':
					{
						@byte = xc.Macro.FinalVol >> 3;
						Common.Clamp(ref @byte, 1, 127);
						break;
					}

					// Note panning
					case 'x':
					{
						@byte = xc.Macro.NotePan >> 1;
						Common.Clamp(ref @byte, 0, 127);
						break;
					}

					// Computed panning
					case 'y':
					{
						@byte = xc.Info_FinalPan >> 1;
						Common.Clamp(ref @byte, 0, 127);
						break;
					}

					// Ins MIDI Bank Hi
					// Ins MIDI Bank Lo
					// Ins MIDI Program
					// MPT: SysEx checksum
					case 'a':
					case 'b':
					case 'p':
					case 's':
					{
						@byte = 0;
						break;
					}

					// Ins MIDI Channel
					case 'c':
						return 0;
				}

				// Byte output
				if (@byte >= 0)
				{
					@in.Buffer = @byte & 0xf;
					return (@byte >> 4) & 0xf;
				}
			}

			return -1;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Midi_Byte(Channel_Data xc, c_int chn, Midi_Stream @in)
		{
			c_int a = Midi_Nibble(xc, chn, @in);
			c_int b = Midi_Nibble(xc, chn, @in);

			return (a >= 0) && (b >= 0) ? (a << 4) | b : -1;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Apply_Midi_Macro_Effect(Channel_Data xc, c_int type, c_int val)
		{
			switch (type)
			{
				// Filter cutoff
				case 0:
				{
					xc.Filter.CutOff = val << 1;
					break;
				}

				// Filter resonance
				case 1:
				{
					xc.Filter.Resonance = val << 1;
					break;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Execute_Midi_Macro(Channel_Data xc, c_int chn, Midi_Macro midi, c_int param)
		{
			Midi_Stream @in = new Midi_Stream();

			@in.Data = midi.Data;
			@in.Pos = 0;
			@in.Buffer = -1;
			@in.Param = param;

			while (@in.Data[@in.Pos] != 0)
			{
				// Very simple MIDI 1.0 parser--most bytes can just be ignored
				// (or passed through, if libxmp gets MIDI output). All bytes
				// with bit 7 are statuses which interrupt unfinished messages
				// ("Data Types: Status Bytes") or are real time messages.
				// This holds even for SysEx messages, which end at ANY non-
				// real time status ("System Common Messages: EOX").
				//
				// IT intercepts internal "messages" that begin with F0 F0,
				// which in MIDI is a useless zero-length SysEx followed by
				// a second SysEx. They are four bytes long including F0 F0,
				// and shouldn't be passed through. OpenMPT also uses F0 F1
				c_int cmd = -1;
				c_int @byte = Midi_Byte(xc, chn, @in);

				if (@byte == 0xf0)
				{
					@byte = Midi_Byte(xc, chn, @in);
					if ((@byte == 0xf0) || (@byte == 0xf1))
						cmd = @byte & 0xf;
				}

				if (cmd < 0)
				{
					if ((@byte == 0xfa) || (@byte == 0xfc) || (@byte == 0xff))
					{
						// These real time statuses can appear anywhere
						// (even in SysEx) and reset the channel filter
						// params. See: OpenMPT ZxxSecrets.it
						Apply_Midi_Macro_Effect(xc, 0, 127);
						Apply_Midi_Macro_Effect(xc, 1, 0);
					}
					continue;
				}

				cmd = Midi_Byte(xc, chn, @in) | (cmd << 8);
				c_int val = Midi_Byte(xc, chn, @in);

				if ((cmd < 0) || (cmd >= 0x80) || (val < 0) || (val >= 0x80))
					continue;

				Apply_Midi_Macro_Effect(xc, cmd, val);
			}
		}



		/********************************************************************/
		/// <summary>
		/// This needs to occur before all process_* functions:
		/// - It modifies the filter parameters, used by process_frequency.
		/// - process_volume and process_pan apply slide effects, which the
		///   filter parameters expect to occur after macro effect parsing
		/// </summary>
		/********************************************************************/
		private void Update_Midi_Macro(c_int chn)
		{
			Player_Data p = ctx.P;
			Module_Data m = ctx.M;
			Channel_Data xc = p.Xc_Data[chn];
			Midi_Macro_Data midiCfg = m.Midi;

			if (Test(xc, Channel_Flag.Midi_Macro) && Common.Has_Quirk(m, Quirk_Flag.Filter))
			{
				if (xc.Macro.Slide > 0)
				{
					xc.Macro.Val += xc.Macro.Slide;

					if (xc.Macro.Val > xc.Macro.Target)
					{
						xc.Macro.Val = xc.Macro.Target;
						xc.Macro.Slide = 0;
					}
				}
				else if (xc.Macro.Slide < 0)
				{
					xc.Macro.Val += xc.Macro.Slide;

					if (xc.Macro.Val < xc.Macro.Target)
					{
						xc.Macro.Val = xc.Macro.Target;
						xc.Macro.Slide = 0;
					}
				}
				else if (p.Frame != 0)
				{
					// Execute non-smooth macros on frame 0 only
					return;
				}

				c_int val = (c_int)xc.Macro.Val;

				if (val >= 0x80)
				{
					if (midiCfg != null)
					{
						Midi_Macro macro = midiCfg.Fixed[val - 0x80];
						Execute_Midi_Macro(xc, chn, macro, val);
					}
					else if (val < 0x90)
					{
						// Default fixed macro: set resonance
						Apply_Midi_Macro_Effect(xc, 1, (val - 0x80) << 3);
					}
				}
				else if (midiCfg != null)
				{
					Midi_Macro macro = midiCfg.Param[xc.Macro.Active];
					Execute_Midi_Macro(xc, chn, macro, val);
				}
				else if (xc.Macro.Active == 0)
				{
					// Default parameterized macro 0: set filter cutoff
					Apply_Midi_Macro_Effect(xc, 0, val);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// From http://www.un4seen.com/forum/?topic=7554.0
		///
		/// "Invert loop" effect replaces (!) sample data bytes within loop
		/// with their bitwise complement (NOT). The parameter sets speed of
		/// altering the samples. This effectively trashes the sample data.
		/// Because of that this effect was supposed to be removed in the
		/// very next ProTracker versions, but it was never removed.
		///
		/// Prior to [Protracker 1.1A] this effect is called "Funk Repeat"
		/// and it moves loop of the instrument (just the loop information -
		/// sample data is not altered). The parameter is the speed of moving
		/// the loop
		/// </summary>
		/********************************************************************/
		private void Update_InvLoop(Channel_Data xc)
		{
			Xmp_Sample xxs = lib.sMix.LibXmp_Get_Sample(xc.Smp);
			Module_Data m = ctx.M;
			c_int lps = 0, len = -1;

			xc.InvLoop.Count += invLoop_Table[xc.InvLoop.Speed];

			if (xxs != null)
			{
				if ((xxs.Flg & Xmp_Sample_Flag.Loop) != 0)
				{
					lps = xxs.Lps;
					len = xxs.Lpe - lps;
				}
				else if ((xxs.Flg & Xmp_Sample_Flag.SLoop) != 0)
				{
					// Some formats that support invert loop use sustain
					// loops instead (Digital Symphony)
					lps = m.Xtra[xc.Smp].Sus;
					len = m.Xtra[xc.Smp].Sue - lps;
				}
			}

			if ((len >= 0) && (xc.InvLoop.Count >= 128))
			{
				xc.InvLoop.Count = 0;

				if (++xc.InvLoop.Pos > len)
					xc.InvLoop.Pos = 0;

				if (xxs.Data == null)
					return;

				if ((~xxs.Flg & Xmp_Sample_Flag._16Bit) != 0)
					xxs.Data[xxs.DataOffset + lps + xc.InvLoop.Pos] ^= 0xff;
			}
		}



		/********************************************************************/
		/// <summary>
		/// From OpenMPT Arpeggio.xm test:
		///
		/// "[FT2] Arpeggio behavior is very weird with more than 16 ticks
		/// per row. This comes from the fact that Fasttracker 2 uses a LUT
		/// for computing the arpeggio note (instead of doing something like
		/// tick%3 or similar). The LUT only has 16 entries, so when there
		/// are more than 16 ticks, it reads beyond array boundaries. The
		/// vibrato table happens to be stored right after arpeggio table.
		/// The tables look like this in memory:
		///
		///   ArpTab: 0,1,2,0,1,2,0,1,2,0,1,2,0,1,2,0
		///   VibTab: 0,24,49,74,97,120,141,161,180,197,...
		///
		/// All values except for the first in the vibrato table are greater
		/// than 1, so they trigger the third arpeggio note. Keep in mind
		/// that Fasttracker 2 counts downwards, so the table has to be read
		/// from back to front, i.e. at 16 ticks per row, the 16th entry in
		/// the LUT is the first to be read. This is also the reason why
		/// Arpeggio is played 'backwards' in Fasttracker 2."
		/// </summary>
		/********************************************************************/
		private c_int Ft2_Arpeggio(Channel_Data xc)
		{
			Player_Data p = ctx.P;

			if ((xc.Arpeggio.Val[1] == 0) && (xc.Arpeggio.Val[2] == 0))
				return 0;

			if (p.Frame == 0)
				return 0;

			c_int i = p.Speed - (p.Frame % p.Speed);

			if (i == 16)
				return 0;
			else if (i > 16)
				return xc.Arpeggio.Val[2];

			return xc.Arpeggio.Val[i % 3];
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Arpeggio(Channel_Data xc)
		{
			Module_Data m = ctx.M;
			c_int arp;

			if (Common.Has_Quirk(m, Quirk_Flag.Ft2Bugs))
				arp = Ft2_Arpeggio(xc);
			else
				arp = xc.Arpeggio.Val[xc.Arpeggio.Count];

			xc.Arpeggio.Count++;
			xc.Arpeggio.Count %= xc.Arpeggio.Size;

			return arp;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private bool Is_First_Frame()
		{
			Player_Data p = ctx.P;
			Module_Data m = ctx.M;

			switch (m.Read_Event_Type)
			{
				case Read_Event.It:
				case Read_Event.St3:
					return (p.Frame % p.Speed) == 0;

				default:
					return p.Frame == 0;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Reset_Channels()
		{
			Player_Data p = ctx.P;
			Module_Data m = ctx.M;
			Xmp_Module mod = m.Mod;

			for (c_int i = 0; i < p.Virt.Virt_Channels; i++)
			{
				Channel_Data xc = p.Xc_Data[i];
				object extra = xc.Extra;
				xc.Clear();
				xc.Extra = extra;

				lib.extras.LibXmp_Reset_Channel_Extras(xc);
				xc.Ins = -1;
				xc.Old_Ins = -1;	// Raw value
				xc.Key = -1;
				xc.Volume = m.VolBase;
			}

			for (c_int i = 0; i < p.Virt.Num_Tracks; i++)
			{
				Channel_Data xc = p.Xc_Data[i];

				if ((i >= mod.Chn) && (i < mod.Chn))	// TNE: Looks funny, but that's because I have removed the smix usage
				{
					xc.MasterVol = 0x40;
					xc.Pan.Val = 0x80;
				}
				else
				{
					xc.MasterVol = mod.Xxc[i].Vol;
					xc.Pan.Val = mod.Xxc[i].Pan;
				}

				xc.Filter.CutOff = 0xff;

				// Amiga split channel
				if ((mod.Xxc[i].Flg & Xmp_Channel_Flag.Split) != 0)
				{
					xc.Split = (uint8)((((uint8)(mod.Xxc[i].Flg) & 0x30) >> 4) + 1);

					// Connect split channel pairs
					for (c_int j = 0; j < i; j++)
					{
						if ((mod.Xxc[j].Flg & Xmp_Channel_Flag.Split) != 0)
						{
							if (p.Xc_Data[j].Split == xc.Split)
							{
								p.Xc_Data[j].Pair = (uint8)i;
								xc.Pair = (uint8)j;
							}
						}
					}
				}
				else
					xc.Split = 0;

				// Surround channel
				if ((mod.Xxc[i].Flg & Xmp_Channel_Flag.Surround) != 0)
					xc.Pan.Surround = true;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Check_Delay(Xmp_Event e, c_int chn)
		{
			Player_Data p = ctx.P;
			Channel_Data xc = p.Xc_Data[chn];
			Module_Data m = ctx.M;

			// Tempo affects delay and must be computed first
			if (((e.FxT == Effects.Fx_Speed) && (e.FxP < 0x20)) || (e.FxT == Effects.Fx_S3M_Speed))
			{
				if (e.FxP != 0)
					p.Speed = e.FxP;
			}

			if (((e.F2T == Effects.Fx_Speed) && (e.F2P < 0x20)) || (e.F2T == Effects.Fx_S3M_Speed))
			{
				if (e.F2P != 0)
					p.Speed = e.F2P;
			}

			// Delay event read
			if ((e.FxT == Effects.Fx_Extended) && (Common.Msn(e.FxP) == Effects.Ex_Delay) && (Common.Lsn(e.FxP) != 0))
			{
				xc.Delay = Common.Lsn(e.FxP) + 1;
				goto Do_Delay;
			}

			if ((e.F2T == Effects.Fx_Extended) && (Common.Msn(e.F2P) == Effects.Ex_Delay) && (Common.Lsn(e.F2P) != 0))
			{
				xc.Delay = Common.Lsn(e.F2P) + 1;
				goto Do_Delay;
			}

			return 0;

			Do_Delay:
			xc.Delayed_Event.CopyFrom(e);

			if (e.Ins != 0)
				xc.Delayed_Ins = e.Ins;

			if (Common.Has_Quirk(m, Quirk_Flag.RtDelay))
			{
				if ((e.Vol == 0) && (e.F2T == 0) && (e.Ins == 0) && (e.Note != Constants.Xmp_Key_Off))
					xc.Delayed_Event.Vol = (byte)(xc.Volume + 1);

				if (e.Note == 0)
					xc.Delayed_Event.Note = (byte)(xc.Key + 1);

				if (e.Ins == 0)
					xc.Delayed_Event.Ins = (byte)xc.Old_Ins;
			}

			return 1;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Read_Row(c_int pat, c_int row)
		{
			Module_Data m = ctx.M;
			Xmp_Module mod = m.Mod;
			Player_Data p = ctx.P;
			Flow_Control f = p.Flow;
			Xmp_Event ev = new Xmp_Event();

			for (c_int chn = 0; chn < mod.Chn; chn++)
			{
				c_int num_Rows = mod.Xxt[Common.Track_Num(m, pat, chn)].Rows;

				if (row < num_Rows)
					ev.CopyFrom(Common.Event(m, pat, chn, row));
				else
					ev.Clear();

				if (ev.Note == Constants.Xmp_Key_Off)
				{
					bool env_On = false;
					c_int ins = ev.Ins - 1;

					if (Is_Valid_Instrument(mod, ins) && ((mod.Xxi[ins].Aei.Flg & Xmp_Envelope_Flag.On) != 0))
						env_On = true;

					if ((ev.FxT == Effects.Fx_Extended) && (Common.Msn(ev.FxP) == Effects.Ex_Delay))
					{
						if ((ev.Ins != 0) && ((Common.Lsn(ev.FxP) != 0) || env_On))
						{
							if (Common.Lsn(ev.FxP) != 0)
								ev.Note = 0;

							ev.FxP = ev.FxT = 0;
						}
					}
				}

				if (Check_Delay(ev, chn) == 0)
				{
					// rowdelay_set bit 1 is set only in the first tick of the row
					// event if the delay causes the tick count resets to 0. We test
					// it to read row events only in the start of the row. (see the
					// OpenMPT test case FineVolColSlide.it)
					if ((f.RowDelay_Set == RowDelay_Flag.None) || (((f.RowDelay_Set & RowDelay_Flag.First_Frame) != 0) && (f.RowDelay > 0)))
					{
						LibXmp_Read_Event(ev, chn);
//XX						LibXmp_Med_Hold_Hack(pat, chn, row);
					}
				}
				else
				{
					if (Common.Is_Player_Mode_It(m))
					{
						// Reset flags. See SlideDelay.it
						p.Xc_Data[chn].Flags = Channel_Flag.None;
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private c_int Get_Channel_Vol(c_int chn)
		{
			Player_Data p = ctx.P;

			// Channel is a root channel
			if (chn < p.Virt.Num_Tracks)
				return p.Channel_Vol[chn];

			// Channel is invalid
			if (chn >= p.Virt.Virt_Channels)
				return 0;

			// Root is invalid
			c_int root = lib.virt.LibXmp_Virt_GetRoot(chn);
			if (root < 0)
				return 0;

			return p.Channel_Vol[root];
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Tremor_Ft2(c_int chn, c_int finalVol)
		{
			Player_Data p = ctx.P;
			Channel_Data xc = p.Xc_Data[chn];

			if ((xc.Tremor.Count & 0x80) != 0)
			{
				if (Test(xc, Channel_Flag.Tremor) && (p.Frame != 0))
				{
					xc.Tremor.Count &= ~0x20;

					if (xc.Tremor.Count == 0x80)
					{
						// End of down cycle, set up counter for up
						xc.Tremor.Count = xc.Tremor.Up | 0xc0;
					}
					else if (xc.Tremor.Count == 0xc0)
					{
						// End of up cycle, set up counter for down
						xc.Tremor.Count = xc.Tremor.Down | 0x80;
					}
					else
						xc.Tremor.Count--;
				}

				if ((xc.Tremor.Count & 0xe0) == 0x80)
					finalVol = 0;
			}

			return finalVol;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Tremor_S3M(c_int chn, c_int finalVol)
		{
			Player_Data p = ctx.P;
			Channel_Data xc = p.Xc_Data[chn];

			if (Test(xc, Channel_Flag.Tremor))
			{
				if (xc.Tremor.Count == 0)
				{
					// End of down cycle, set up counter for up
					xc.Tremor.Count = xc.Tremor.Up | 0x80;
				}
				else if (xc.Tremor.Count == 0x80)
				{
					// End of up cycle, set up counter for down
					xc.Tremor.Count = xc.Tremor.Down;
				}

				xc.Tremor.Count--;

				if ((~xc.Tremor.Count & 0x80) != 0)
					finalVol = 0;
			}

			return finalVol;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private bool DoEnv_Release(Channel_Data xc, Virt_Action act)
		{
			return Test_Note(xc, Note_Flag.Env_Release) || (act == Virt_Action.Off);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Process_Volume(c_int chn, Virt_Action act)
		{
			Player_Data p = ctx.P;
			Module_Data m = ctx.M;
			Channel_Data xc = p.Xc_Data[chn];
			bool fade = false;

			Xmp_Instrument instrument = lib.sMix.LibXmp_Get_Instrument(xc.Ins);

			// Keyoff and fadeout

			// Keyoff event in IT doesn't reset fadeout (see jeff93.it)
			// In XM it depends on envelope (see graff-strange_land.xm vs
			// Decibelter - Cosmic 'Wegian Mamas.xm)
			if (Common.Has_Quirk(m, Quirk_Flag.KeyOff))
			{
				// If IT, only apply fadeout on note release if we don't
				// have envelope, or if we have envelope loop
				if (Test_Note(xc, Note_Flag.Env_Release) || (act == Virt_Action.Off))
				{
					if (((~instrument.Aei.Flg & Xmp_Envelope_Flag.On) != 0) || ((instrument.Aei.Flg & Xmp_Envelope_Flag.Loop) != 0))
						fade = true;
				}
			}
			else
			{
				if ((~instrument.Aei.Flg & Xmp_Envelope_Flag.On) != 0)
				{
					if (Test_Note(xc, Note_Flag.Env_Release))
						xc.FadeOut = 0;
				}

				if (Test_Note(xc, Note_Flag.Env_Release) || (act == Virt_Action.Off))
					fade = true;
			}

			if (!Test_Per(xc, Channel_Flag.VEnv_Pause))
				xc.V_Idx = Update_Envelope(instrument.Aei, xc.V_Idx, DoEnv_Release(xc, act), Test(xc, Channel_Flag.Key_Off));

			uint16 vol_Envelope = (uint16)Get_Envelope(instrument.Aei, xc.V_Idx, 64);

			if (Check_Envelope_End(instrument.Aei, xc.V_Idx))
			{
				if (vol_Envelope == 0)
					Set_Note(xc, Note_Flag.End);

				Set_Note(xc, Note_Flag.Env_End);
			}

			// IT starts fadeout automatically at the end of the volume envelope
			switch (Check_Envelope_Fade(instrument.Aei, xc.V_Idx))
			{
				case -1:
				{
					Set_Note(xc, Note_Flag.End);

					// Don't reset channel, we may have a tone portamento later
					break;
				}

				case 0:
					break;

				default:
				{
					if (Common.Has_Quirk(m, Quirk_Flag.EnvFade))
						Set_Note(xc, Note_Flag.FadeOut);

					break;
				}
			}

			// IT envelope fadeout starts immediately after the envelope tick,
			// so process fadeout after the volume envelope
			if (Test_Note(xc, Note_Flag.FadeOut) || (act == Virt_Action.Fade))
				fade = true;

			if (fade)
			{
				if (xc.FadeOut > xc.Ins_Fade)
					xc.FadeOut -= xc.Ins_Fade;
				else
				{
					xc.FadeOut = 0;
					Set_Note(xc, Note_Flag.End);
				}
			}

			// If note ended in the background channel, we can safely reset it
			if (Test_Note(xc, Note_Flag.End) && (chn >= p.Virt.Num_Tracks))
			{
				lib.virt.LibXmp_Virt_ResetChannel(chn);
				return;
			}

			c_int finalVol = lib.extras.LibXmp_Extras_Get_Volume(xc);

			if (Common.Is_Player_Mode_It(m))
				finalVol = xc.Volume * (100 - xc.Rvv) / 100;

			if (Test(xc, Channel_Flag.Tremolo))
			{
				// OpenMPT VibratoReset.mod
				if (!Is_First_Frame() || !Common.Has_Quirk(m, Quirk_Flag.ProTrack))
					finalVol += lib.lfo.LibXmp_Lfo_Get(xc.Tremolo.Lfo, false) / (1 << 6);

				if (!Is_First_Frame() || Common.Has_Quirk(m, Quirk_Flag.VibAll))
					lib.lfo.LibXmp_Lfo_Update(xc.Tremolo.Lfo);
			}

			Common.Clamp(ref finalVol, 0, m.VolBase);

			finalVol = (finalVol * xc.FadeOut) >> 6;	// 16 bit output

			finalVol = (c_int)(uint32)(vol_Envelope * p.GVol * xc.MasterVol / m.GVolBase * ((c_int)finalVol * 0x40 / m.VolBase)) >> 18;

			// Apply channel volume
			finalVol = finalVol * Get_Channel_Vol(chn) / 100;

			// Volume translation table (for PTM, ARCH, COCO)
			if (m.Vol_Table != null)
				finalVol = m.VolBase == 0xff ? m.Vol_Table[finalVol >> 2] << 2 : m.Vol_Table[finalVol >> 4] << 4;

			if (Common.Has_Quirk(m, Quirk_Flag.InsVol))
				finalVol = (finalVol * instrument.Vol * xc.Gvl) >> 12;

			if (Common.Is_Player_Mode_Ft2(m))
				finalVol = Tremor_Ft2(chn, finalVol);
			else
				finalVol = Tremor_S3M(chn, finalVol);

			xc.Macro.FinalVol = finalVol;

			if (chn < m.Mod.Chn)
				finalVol = finalVol * p.Master_Vol / 100;
			else
				finalVol = finalVol * p.Master_Vol / 100;	// Changed from smix_vol to Master_Vol, to make NNA work correctly

			xc.Info_FinalVol = Test_Note(xc, Note_Flag.Sample_End) ? 0 : finalVol;

			lib.virt.LibXmp_Virt_SetVol(chn, finalVol);

			// Check Amiga split channel
			if (xc.Split != 0)
				lib.virt.LibXmp_Virt_SetVol(xc.Pair, finalVol);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Process_Frequency(c_int chn, Virt_Action act)
		{
			Mixer_Data s = ctx.S;
			Player_Data p = ctx.P;
			Module_Data m = ctx.M;
			Channel_Data xc = p.Xc_Data[chn];

			Xmp_Instrument instrument = lib.sMix.LibXmp_Get_Instrument(xc.Ins);

			if (!Test(xc, Channel_Flag.FEnv_Pause))
				xc.F_Idx = Update_Envelope(instrument.Fei, xc.F_Idx, DoEnv_Release(xc, act), Test(xc, Channel_Flag.Key_Off));

			c_int frq_Envelope = Get_Envelope(instrument.Fei, xc.F_Idx, 0);

			// Do note slide
			if (Test(xc, Channel_Flag.Note_Slide))
			{
				if (xc.NoteSlide.Count == 0)
				{
					xc.Note += xc.NoteSlide.Slide;
					xc.Period = lib.period.LibXmp_Note_To_Period(xc.Note, xc.FineTune, xc.Per_Adj);
					xc.NoteSlide.Count = xc.NoteSlide.Speed;
				}

				xc.NoteSlide.Count--;

				lib.virt.LibXmp_Virt_SetNote(chn, xc.Note);
			}

			// Instrument vibrato
			c_double vibrato = 1.0 * lib.lfo.LibXmp_Lfo_Get(xc.InsVib.Lfo, true) / (4096 * (1 + xc.InsVib.Sweep));

			lib.lfo.LibXmp_Lfo_Update(xc.InsVib.Lfo);

			if (xc.InsVib.Sweep > 1)
				xc.InsVib.Sweep -= 2;
			else
				xc.InsVib.Sweep = 0;

			// Vibrato
			if (Test(xc, Channel_Flag.Vibrato) || Test_Per(xc, Channel_Flag.Vibrato))
			{
				// OpenMPT VibratoReset.mod
				if (!Is_First_Frame() || !Common.Has_Quirk(m, Quirk_Flag.ProTrack))
				{
					c_int shift = Common.Has_Quirk(m, Quirk_Flag.VibHalf) ? 10 : 9;
					c_int vib = lib.lfo.LibXmp_Lfo_Get(xc.Vibrato.Lfo, true) / (1 << shift);

					if (Common.Has_Quirk(m, Quirk_Flag.VibInv))
						vibrato -= vib;
					else
						vibrato += vib;
				}

				if (!Is_First_Frame() || Common.Has_Quirk(m, Quirk_Flag.VibAll))
					lib.lfo.LibXmp_Lfo_Update(xc.Vibrato.Lfo);
			}

			c_double period = xc.Period;
			period += lib.extras.LibXmp_Extras_Get_Period(xc);

			if (Common.Has_Quirk(m, Quirk_Flag.St3Bugs))
			{
				if (period < 0.25)
					lib.virt.LibXmp_Virt_ResetChannel(chn);
			}

			// Sanity check
			if (period < 0.1)
				period = 0.1;

			// Arpeggio
			c_int arp = Arpeggio(xc);

			// Pitch bend
			c_int linear_Bend = lib.period.LibXmp_Period_To_Bend(period + vibrato, xc.Note, xc.Per_Adj);

			if (Test_Note(xc, Note_Flag.Glissando) && Test(xc, Channel_Flag.TonePorta))
			{
				if (linear_Bend > 0)
					linear_Bend = (linear_Bend + 6400) / 12800 * 12800;
				else if (linear_Bend < 0)
					linear_Bend = (linear_Bend - 6400) / 12800 * 12800;
			}

			if (Common.Has_Quirk(m, Quirk_Flag.Ft2Bugs))
			{
				if (arp != 0)
				{
					// OpenMPT ArpSlide.xm
					linear_Bend = linear_Bend / 12800 * 12800 + xc.FineTune * 100;

					// OpenMPT ArpeggioClamp.xm
					if ((xc.Note + arp) > 107)
					{
						if ((p.Speed - (p.Frame % p.Speed)) > 0)
							arp = 108 - xc.Note;
					}
				}
			}

			// Envelope
			if ((xc.F_Idx >= 0) && ((~instrument.Fei.Flg & Xmp_Envelope_Flag.Flt) != 0))
			{
				// IT pitch envelopes are always linear, even in Amiga period
				// mode. Each unit in the envelope scale is 1/25 semitone
				linear_Bend += frq_Envelope << 7;
			}

			// Arpeggio
			if (arp != 0)
			{
				linear_Bend += (100 << 7) * arp;

				// OpenMPT ArpWrapAround.mod
				if (Common.Has_Quirk(m, Quirk_Flag.ProTrack))
				{
					if ((xc.Note + arp) > (Constants.Max_Note_Mod + 1))
						linear_Bend -= 12800 * (3 * 12);
					else if ((xc.Note + arp) > Constants.Max_Note_Mod)
						lib.virt.LibXmp_Virt_SetVol(chn, 0);
				}
			}

			linear_Bend += lib.extras.LibXmp_Extras_Get_Linear_Bend(xc);

			c_double final_Period = lib.period.LibXmp_Note_To_Period_Mix(xc.Note, linear_Bend);

			// From OpenMPT PeriodLimit.s3m:
			// "ScreamTracker 3 limits the final output period to be at least 64,
			// i.e. when playing a note that is too high or when sliding the
			// period lower than 64, the output period will simply be clamped to
			// 64. However, when reaching a period of 0 through slides, the
			// output on the channel should be stopped."
			//
			// ST3 uses periods*4, so the limit is 16. Adjusted to the exact
			// A6 value because we compute periods in floating point
			if (Common.Has_Quirk(m, Quirk_Flag.St3Bugs))
			{
				if (final_Period < 16.239270)	// A6
					final_Period = 16.239270;
			}

			lib.virt.LibXmp_Virt_SetPeriod(chn, final_Period);

			// For xmp_get_frame_info()
			xc.Info_PitchBend = linear_Bend >> 7;
			xc.Info_Period = Math.Min((c_int)(final_Period * 4096), c_int.MaxValue);

			if (Common.Is_Period_ModRng(m))
			{
				c_double min_Period = lib.period.LibXmp_Note_To_Period(Constants.Max_Note_Mod, xc.FineTune, 0) * 4096;
				c_double max_Period = lib.period.LibXmp_Note_To_Period(Constants.Min_Note_Mod, xc.FineTune, 0) * 4096;
				Common.Clamp(ref xc.Info_Period, (c_int)min_Period, (c_int)max_Period);
			}
			else if (xc.Info_Period < (1 << 12))
				xc.Info_Period = (1 << 12);

			// Process filter
			if (!Common.Has_Quirk(m, Quirk_Flag.Filter))
				return;

			c_int cutOff;

			if ((xc.F_Idx >= 0) && ((instrument.Fei.Flg & Xmp_Envelope_Flag.Flt) != 0))
			{
				if (frq_Envelope < 0xfe)
					xc.Filter.Envelope = frq_Envelope;

				cutOff = xc.Filter.CutOff * xc.Filter.Envelope >> 8;
			}
			else
				cutOff = xc.Filter.CutOff;

			c_int resonance = xc.Filter.Resonance;

			if (cutOff > 0xff)
				cutOff = 0xff;

			// IT: cutoff 127 + resonance 0 turns off the filter, but this
			// is only applied when playing a new note without toneporta.
			// All other combinations take effect immediately.
			// See OpenMPT filter-reset.it, filter-reset-carry.it
			if ((cutOff < 0xfe) || (resonance > 0) || xc.Filter.Can_Disable)
			{
				Filter.LibXmp_Filter_Setup(s.Freq, cutOff, resonance, out c_int a0, out c_int b0, out c_int b1);
				lib.virt.LibXmp_Virt_SetEffect(chn, Dsp_Effect.Filter_A0, a0);
				lib.virt.LibXmp_Virt_SetEffect(chn, Dsp_Effect.Filter_B0, b0);
				lib.virt.LibXmp_Virt_SetEffect(chn, Dsp_Effect.Filter_B1, b1);
				lib.virt.LibXmp_Virt_SetEffect(chn, Dsp_Effect.Resonance, resonance);
				lib.virt.LibXmp_Virt_SetEffect(chn, Dsp_Effect.CutOff, cutOff);

				xc.Filter.Can_Disable = false;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Process_Pan(c_int chn, Virt_Action act)
		{
			Player_Data p = ctx.P;
			Module_Data m = ctx.M;
			Mixer_Data s = ctx.S;
			Channel_Data xc = p.Xc_Data[chn];
			c_int panbrello = 0;

			Xmp_Instrument instrument = lib.sMix.LibXmp_Get_Instrument(xc.Ins);

			if (!Test_Per(xc, Channel_Flag.PEnv_Pause))
				xc.P_Idx = Update_Envelope(instrument.Pei, xc.P_Idx, DoEnv_Release(xc, act), Test(xc, Channel_Flag.Key_Off));

			c_int pan_Envelope = Get_Envelope(instrument.Pei, xc.P_Idx, 32);

			if (Test(xc, Channel_Flag.Panbrello))
			{
				panbrello = lib.lfo.LibXmp_Lfo_Get(xc.Panbrello.Lfo, false) / 512;

				if (Is_First_Frame())
					lib.lfo.LibXmp_Lfo_Update(xc.Panbrello.Lfo);
			}

			xc.Macro.NotePan = xc.Pan.Val + panbrello + 0x80;

			c_int channel_Pan = xc.Pan.Val;

			c_int finalPan = channel_Pan + panbrello + (pan_Envelope - 32) * (128 - Math.Abs(xc.Pan.Val - 128)) / 32;

			if (Common.Is_Player_Mode_It(m))
				finalPan = finalPan + xc.Rpv * 4;

			Common.Clamp(ref finalPan, 0, 255);

			if (xc.Pan.Surround)
				finalPan = 0;
			else
				finalPan = (finalPan - 0x80) * s.Mix / 100;

			xc.Info_FinalPan = finalPan + 0x80;

			if (xc.Pan.Surround)
				lib.virt.LibXmp_Virt_SetPan(chn, Constants.Pan_Surround);
			else
				lib.virt.LibXmp_Virt_SetPan(chn, finalPan);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Update_Volume(c_int chn)
		{
			Player_Data p = ctx.P;
			Module_Data m = ctx.M;
			Flow_Control f = p.Flow;
			Channel_Data xc = p.Xc_Data[chn];

			// Volume slides happen in all frames but the first, except when the
			// "volume slide on all frames" flag is set
			if (((p.Frame % p.Speed) != 0) || Common.Has_Quirk(m, Quirk_Flag.VsAll))
			{
				if (Test(xc, Channel_Flag.GVol_Slide))
					p.GVol += xc.GVol.Slide;

				if (Test(xc, Channel_Flag.Vol_Slide) || Test_Per(xc, Channel_Flag.Vol_Slide))
					xc.Volume += xc.Vol.Slide;

				if (Test_Per(xc, Channel_Flag.Vol_Slide))
				{
					if (xc.Vol.Slide > 0)
					{
						c_int target = Math.Max(xc.Vol.Target - 1, m.VolBase);

						if (xc.Volume > target)
						{
							xc.Volume = target;
							Reset_Per(xc, Channel_Flag.Vol_Slide);
						}
					}

					if (xc.Vol.Slide < 0)
					{
						c_int target = xc.Vol.Target > 0 ? Math.Min(0, xc.Vol.Target - 1) : 0;

						if (xc.Volume < target)
						{
							xc.Volume = target;
							Reset_Per(xc, Channel_Flag.Vol_Slide);
						}
					}
				}

				if (Test(xc, Channel_Flag.Vol_Slide_2))
					xc.Volume += xc.Vol.Slide2;

				if (Test(xc, Channel_Flag.Trk_VSlide))
					xc.MasterVol += xc.TrackVol.Slide;
			}

			if ((p.Frame % p.Speed) == 0)
			{
				// Process "fine" effects
				if (Test(xc, Channel_Flag.Fine_Vols))
					xc.Volume += xc.Vol.FSlide;

				if (Test(xc, Channel_Flag.Fine_Vols_2))
				{
					// OpenMPT FineVolColSlide.it:
					// Unlike fine volume slides in the effect column,
					// fine volume slides in the volume column are only
					// ever executed on the first tick -- not on multiples
					// of the first tick if there is a pattern delay
					if ((f.RowDelay_Set == RowDelay_Flag.None) || ((f.RowDelay_Set & RowDelay_Flag.First_Frame) != 0))
						xc.Volume += xc.Vol.FSlide2;
				}

				if (Test(xc, Channel_Flag.Trk_FVSlide))
					xc.MasterVol += xc.TrackVol.FSlide;

				if (Test(xc, Channel_Flag.GVol_Slide))
					p.GVol += xc.GVol.FSlide;
			}

			// Clamp volumes
			Common.Clamp(ref xc.Volume, 0, m.VolBase);
			Common.Clamp(ref p.GVol, 0, m.GVolBase);
			Common.Clamp(ref xc.MasterVol, 0, m.VolBase);

			if (xc.Split != 0)
				p.Xc_Data[xc.Pair].Volume = xc.Volume;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Update_Frequency(c_int chn)
		{
			Player_Data p = ctx.P;
			Module_Data m = ctx.M;
			Channel_Data xc = p.Xc_Data[chn];

			if (!Is_First_Frame() || Common.Has_Quirk(m, Quirk_Flag.PbAll))
			{
				if (Test(xc, Channel_Flag.PitchBend) || Test_Per(xc, Channel_Flag.PitchBend))
				{
					xc.Period += xc.Freq.Slide;

					if (Common.Has_Quirk(m, Quirk_Flag.ProTrack))
						xc.Porta.Target = xc.Period;
				}

				// Do tone portamento
				if (Test(xc, Channel_Flag.TonePorta) || Test_Per(xc, Channel_Flag.TonePorta))
				{
					if (xc.Porta.Target > 0)
					{
						bool end = false;

						if (xc.Porta.Dir > 0)
						{
							xc.Period += xc.Porta.Slide;

							if (xc.Period >= xc.Porta.Target)
								end = true;
						}
						else
						{
							xc.Period -= xc.Porta.Slide;

							if (xc.Period <= xc.Porta.Target)
								end = true;
						}

						if (end)
						{
							// Reached end
							xc.Period = xc.Porta.Target;
							xc.Porta.Dir = 0;

							Reset(xc, Channel_Flag.TonePorta);
							Reset_Per(xc, Channel_Flag.TonePorta);

							if (Common.Has_Quirk(m, Quirk_Flag.ProTrack))
								xc.Porta.Target = -1;
						}
					}
				}
			}

			if (Is_First_Frame())
			{
				if (Test(xc, Channel_Flag.Fine_Bend))
					xc.Period += xc.Freq.FSlide;

				if (Test(xc, Channel_Flag.Fine_NSlide))
				{
					xc.Note += xc.NoteSlide.FSlide;
					xc.Period = lib.period.LibXmp_Note_To_Period(xc.Note, xc.FineTune, xc.Per_Adj);
				}
			}

			switch (m.Period_Type)
			{
				case Containers.Common.Period.Linear:
				{
					Common.Clamp(ref xc.Period, Constants.Min_Period_L, Constants.Max_Period_L);
					break;
				}

				case Containers.Common.Period.ModRng:
				{
					c_double min_Period = lib.period.LibXmp_Note_To_Period(Constants.Max_Note_Mod, xc.FineTune, 0);
					c_double max_Period = lib.period.LibXmp_Note_To_Period(Constants.Min_Note_Mod, xc.FineTune, 0);
					Common.Clamp(ref xc.Period, min_Period, max_Period);
					break;
				}
			}

			// Check for invalid periods (from Toru Egashira's NSPmod)
			// panic.s3m has negative periods
			// ambio.it uses low (~8) period values
			if (xc.Period < 0.25)
				lib.virt.LibXmp_Virt_SetVol(chn, 0);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Update_Pan(c_int chn)
		{
			Player_Data p = ctx.P;
			Channel_Data xc = p.Xc_Data[chn];

			if (Test(xc, Channel_Flag.Pan_Slide))
			{
				if (Is_First_Frame())
					xc.Pan.Val += xc.Pan.FSlide;
				else
					xc.Pan.Val += xc.Pan.Slide;

				if (xc.Pan.Val < 0)
					xc.Pan.Val = 0;
				else if (xc.Pan.Val > 0xff)
					xc.Pan.Val = 0xff;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Play_Channel(c_int chn)
		{
			Player_Data p = ctx.P;
			Module_Data m = ctx.M;
			Xmp_Module mod = m.Mod;
			Channel_Data xc = p.Xc_Data[chn];

			xc.Info_FinalVol = 0;

			// IT tempo slide
			if (!Is_First_Frame() && Test(xc, Channel_Flag.Tempo_Slide))
			{
				p.Bpm += xc.Tempo.Slide;
				Common.Clamp(ref p.Bpm, 0x20, 0xff);
			}

			// Do delay
			if (xc.Delay > 0)
			{
				if (--xc.Delay == 0)
					LibXmp_Read_Event(xc.Delayed_Event, chn);
			}

			// IT MIDI macros need to update regardless of the current voice state
			Update_Midi_Macro(chn);

			Virt_Action act = lib.virt.LibXmp_Virt_CStat(chn);
			if (act == Virt_Action.Invalid)
			{
				// We need this to keep processing global volume slides
				Update_Volume(chn);
				return;
			}

			if ((p.Frame == 0) && (act != Virt_Action.Active))
			{
				if (!Is_Valid_Instrument_Or_Sfx(mod, xc.Ins) || (act == Virt_Action.Cut))
				{
					lib.virt.LibXmp_Virt_ResetChannel(chn);
					return;
				}
			}

			if (!Is_Valid_Instrument_Or_Sfx(mod, xc.Ins))
				return;

			lib.extras.LibXmp_Play_Extras(xc, chn);

			// Do cut/retrig
			if (Test(xc, Channel_Flag.Retrig))
			{
				bool cond = Common.Has_Quirk(m, Quirk_Flag.S3MRtg) ? --xc.Retrig.Count <= 0 : --xc.Retrig.Count == 0;

				if (cond)
				{
					if (xc.Retrig.Type < 0x10)
					{
						// Don't retrig on cut
						lib.virt.LibXmp_Virt_VoicePos(chn, 0);
					}
					else
						Set_Note(xc, Note_Flag.End);

					xc.Volume += rVal[xc.Retrig.Type].S;
					xc.Volume *= rVal[xc.Retrig.Type].M;
					xc.Volume /= rVal[xc.Retrig.Type].D;
					xc.Retrig.Count = Common.Lsn(xc.Retrig.Val);

					if (xc.Retrig.Limit > 0)
					{
						// Limit the number of retriggers
						--xc.Retrig.Limit;
						if (xc.Retrig.Limit == 0)
							Reset(xc, Channel_Flag.Retrig);
					}
				}
			}

			// Do keyoff
			if (xc.KeyOff != 0)
			{
				if (--xc.KeyOff == 0)
					Set_Note(xc, Note_Flag.Release);
			}

			lib.virt.LibXmp_Virt_Release(chn, Test_Note(xc, Note_Flag.Sample_Release));

			Update_Volume(chn);
			Update_Frequency(chn);
			Update_Pan(chn);

			Process_Volume(chn, act);
			Process_Frequency(chn, act);
			Process_Pan(chn, act);

			if (Common.Has_Quirk(m, Quirk_Flag.ProTrack | Quirk_Flag.InvLoop) && (xc.Ins < mod.Ins))
				Update_InvLoop(xc);

			if (Test_Note(xc, Note_Flag.SusExit))
				Set_Note(xc, Note_Flag.Env_Release);

			xc.Info_Position = (c_int)lib.virt.LibXmp_Virt_GetVoicePos(chn);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Inject_Event()
		{
			Player_Data p = ctx.P;
			Module_Data m = ctx.M;
			Xmp_Module mod = m.Mod;

			for (c_int chn = 0; chn < mod.Chn; chn++)
			{
				Xmp_Event e = p.Inject_Event[chn];
				if (e._Flag > 0)
				{
					LibXmp_Read_Event(e, chn);
					e._Flag = 0;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Next_Order()
		{
			Player_Data p = ctx.P;
			Flow_Control f = p.Flow;
			Module_Data m = ctx.M;
			Xmp_Module mod = m.Mod;
			bool reset_GVol = false;

			do
			{
				p.Ord++;

				// Restart module
				bool mark = Common.Has_Quirk(m, Quirk_Flag.Marker) && (p.Ord < mod.Len) && (mod.Xxo[p.Ord] == 0xff);

				if ((p.Ord >= mod.Len) || mark)
				{
					if ((mod.Rst > mod.Len) || (mod.Xxo[mod.Rst] >= mod.Pat) || (p.Ord < m.Seq_Data[p.Sequence].Entry_Point))
					{
						// Increment loop count. This will make the player to quit like other modules when
						// playing sequence 8 of "alien incident - leohou2.s3m" by Purple Motion
						if (p.Ord < m.Seq_Data[p.Sequence].Entry_Point)
							p.Loop_Count++;

						p.Ord = m.Seq_Data[p.Sequence].Entry_Point;
					}
					else
					{
						if (lib.scan.LibXmp_Get_Sequence(mod.Rst) == p.Sequence)
							p.Ord = mod.Rst;
						else
						{
							p.Ord = m.Seq_Data[p.Sequence].Entry_Point;

							// Increment loop count here too. This will fix e.g. "amazonas-dynomite mix.it" by Skaven
							p.Loop_Count++;
						}
					}

					// This might be a marker, so delay updating global
					// volume until an actual pattern is found
					reset_GVol = true;
				}
			}
			while (mod.Xxo[p.Ord] >= mod.Pat);

			if (reset_GVol)
				p.GVol = m.Xxo_Info[p.Ord].Gvl;

			// Archimedes line jump -- don't reset time tracking
			if (f.Jump_In_Pat != p.Ord)
				p.Current_Time = m.Xxo_Info[p.Ord].Time;

			f.Num_Rows = mod.Xxp[mod.Xxo[p.Ord]].Rows;

			if (f.JumpLine >= f.Num_Rows)
				f.JumpLine = 0;

			p.Row = f.JumpLine;
			f.JumpLine = 0;

			p.Pos = p.Ord;
			p.Frame = 0;

			f.Jump_In_Pat = -1;

			// Reset persistent effects at new pattern
			if (Common.Has_Quirk(m, Quirk_Flag.PerPat))
			{
				for (c_int chn = 0; chn < mod.Chn; chn++)
					p.Xc_Data[chn].Per_Flags = 0;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Next_Row()
		{
			Player_Data p = ctx.P;
			Flow_Control f = p.Flow;

			p.Frame = 0;
			f.Delay = 0;

			if (f.PBreak)
			{
				f.PBreak = false;

				if (f.Jump != -1)
				{
					p.Ord = f.Jump - 1;
					f.Jump = -1;
				}

				Next_Order();
			}
			else
			{
				if (f.RowDelay == 0)
				{
					p.Row++;
					f.RowDelay_Set = 0;
				}
				else
					f.RowDelay--;

				if (f.Loop_Chn != 0)
				{
					p.Row = f.Loop[f.Loop_Chn - 1].Start;
					f.Loop_Chn = 0;
				}

				// Check end of pattern
				if (p.Row >= f.Num_Rows)
					Next_Order();
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Update_From_Ord_Info()
		{
			Player_Data p = ctx.P;
			Module_Data m = ctx.M;
			Ord_Data oInfo = m.Xxo_Info[p.Ord];

			if (oInfo.Speed != 0)
				p.Speed = oInfo.Speed;

			p.Bpm = oInfo.Bpm;
			p.GVol = oInfo.Gvl;
			p.Current_Time = oInfo.Time;
			p.Frame_Time = m.Time_Factor * m.RRate / p.Bpm;

			p.St26_Speed = oInfo.St26_Speed;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Check_End_Of_Module()
		{
			Player_Data p = ctx.P;
			Flow_Control f = p.Flow;

			// Check end of module
			if ((p.Ord == p.Scan[p.Sequence].Ord) && (p.Row == p.Scan[p.Sequence].Row))
			{
				if (f.End_Point == 0)
				{
					p.Loop_Count++;
					f.End_Point = p.Scan[p.Sequence].Num;
				}

				f.End_Point--;
			}
		}
		#endregion
	}
}
