/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Common;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Mixer;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Virt;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp;

namespace Polycode.NostalgicPlayer.Ports.LibXmp
{
	/// <summary>
	/// 
	/// </summary>
	internal class Virtual
	{
		private const c_int Free = -1;

		private readonly LibXmp lib;
		private readonly Xmp_Context ctx;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Virtual(LibXmp libXmp, Xmp_Context ctx)
		{
			lib = libXmp;
			this.ctx = ctx;
		}



		/********************************************************************/
		/// <summary>
		/// Get parent channel
		/// </summary>
		/********************************************************************/
		public c_int LibXmp_Virt_GetRoot(c_int chn)
		{
			Player_Data p = ctx.P;

			c_int voc = p.Virt.Virt_Channel[chn].Map;
			if (voc < 0)
				return -1;

			Mixer_Voice vi = p.Virt.Voice_Array[voc];

			return vi.Root;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void LibXmp_Virt_ResetVoice(c_int voc, bool mute)
		{
			Player_Data p = ctx.P;
			Mixer_Voice vi = p.Virt.Voice_Array[voc];

			if ((uint32)voc >= p.Virt.MaxVoc)
				return;

			if (mute)
			{
				lib.mixer.LibXmp_Mixer_SetVol(voc, 0);
				lib.mixer.LibXmp_Mixer_ResetChannel(voc);
			}

			p.Virt.Virt_Used--;
			p.Virt.Virt_Channel[vi.Root].Count--;
			p.Virt.Virt_Channel[vi.Chn].Map = Free;

			vi.Clear();
			vi.Chn = vi.Root = Free;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int LibXmp_Virt_On(c_int num)
		{
			Player_Data p = ctx.P;
			Module_Data m = ctx.M;

			p.Virt.Num_Tracks = num;
			num = lib.mixer.LibXmp_Mixer_NumVoices(-1);

			p.Virt.Virt_Channels = p.Virt.Num_Tracks;

			if (Common.Has_Quirk(m, Quirk_Flag.Virtual))
				p.Virt.Virt_Channels += num;
			else if (num > p.Virt.Virt_Channels)
				num = p.Virt.Virt_Channels;

			p.Virt.MaxVoc = lib.mixer.LibXmp_Mixer_NumVoices(num);

			p.Virt.Voice_Array = ArrayHelper.InitializeArray<Mixer_Voice>(p.Virt.MaxVoc);
			if (p.Virt.Voice_Array == null)
				goto Err;

			for (c_int i = 0; i < p.Virt.MaxVoc; i++)
			{
				p.Virt.Voice_Array[i].Chn = Free;
				p.Virt.Voice_Array[i].Root = Free;
			}

			p.Virt.Virt_Channel = ArrayHelper.InitializeArray<Virt_Channel>(p.Virt.Virt_Channels);
			if (p.Virt.Virt_Channel == null)
				goto Err2;

			for (c_int i = 0; i < p.Virt.Virt_Channels; i++)
			{
				p.Virt.Virt_Channel[i].Map = Free;
				p.Virt.Virt_Channel[i].Count = 0;
			}

			p.Virt.Virt_Used = 0;

			return 0;

			Err2:
			p.Virt.Voice_Array = null;

			Err:
			return -1;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void LibXmp_Virt_Off()
		{
			Player_Data p = ctx.P;

			p.Virt.Virt_Used = p.Virt.MaxVoc = 0;
			p.Virt.Virt_Channels = 0;
			p.Virt.Num_Tracks = 0;

			p.Virt.Voice_Array = null;
			p.Virt.Virt_Channel = null;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void LibXmp_Virt_Reset()
		{
			Player_Data p = ctx.P;

			if (p.Virt.Virt_Channels < 1)
				return;

			// CID 129203 (#1 of 1): Useless call (USELESS_CALL)
			// Call is only useful for its return value, which is ignored.
			//
			// libxmp_mixer_numvoices(ctx, p->virt.maxvoc);
			for (c_int i = 0; i < p.Virt.MaxVoc; i++)
			{
				Mixer_Voice vi = p.Virt.Voice_Array[i];
				vi.Clear();
				vi.Chn = Free;
				vi.Root = Free;
			}

			for (c_int i = 0; i < p.Virt.Virt_Channels; i++)
			{
				p.Virt.Virt_Channel[i].Map = Free;
				p.Virt.Virt_Channel[i].Count = 0;
			}

			p.Virt.Virt_Used = 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int LibXmp_Virt_MapChannel(c_int chn)
		{
			return Map_Virt_Channel(ctx.P, chn);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void LibXmp_Virt_ResetChannel(c_int chn)
		{
			Player_Data p = ctx.P;

			c_int voc = Map_Virt_Channel(p, chn);
			if (voc < 0)
				return;

			lib.mixer.LibXmp_Mixer_SetVol(voc, 0);
			lib.mixer.LibXmp_Mixer_ResetChannel(voc);

			p.Virt.Virt_Used--;
			p.Virt.Virt_Channel[p.Virt.Voice_Array[voc].Root].Count--;
			p.Virt.Virt_Channel[chn].Map = Free;

			Mixer_Voice vi = p.Virt.Voice_Array[voc];
			vi.Clear();

			vi.Chn = vi.Root = Free;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void LibXmp_Virt_SetVol(c_int chn, c_int vol)
		{
			Player_Data p = ctx.P;

			c_int voc = Map_Virt_Channel(p, chn);
			if (voc < 0)
				return;

			c_int root = p.Virt.Voice_Array[voc].Root;

			if ((root < Constants.Xmp_Max_Channels) && p.Channel_Mute[root])
				vol = 0;

			lib.mixer.LibXmp_Mixer_SetVol(voc, vol);

			if ((vol == 0) && (chn >= p.Virt.Num_Tracks))
				LibXmp_Virt_ResetVoice(voc, true);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void LibXmp_Virt_Release(c_int chn, bool rel)
		{
			Player_Data p = ctx.P;

			c_int voc = Map_Virt_Channel(p, chn);
			if (voc < 0)
				return;

			lib.mixer.LibXmp_Mixer_Release(voc, rel);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void LibXmp_Virt_Reverse(c_int chn, bool rev)
		{
			Player_Data p = ctx.P;

			c_int voc = Map_Virt_Channel(p, chn);
			if (voc < 0)
				return;

			lib.mixer.LibXmp_Mixer_Reverse(voc, rev);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void LibXmp_Virt_SetPan(c_int chn, c_int pan)
		{
			Player_Data p = ctx.P;

			c_int voc = Map_Virt_Channel(p, chn);
			if (voc < 0)
				return;

			lib.mixer.LibXmp_Mixer_SetPan(voc, pan);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void LibXmp_Virt_SetNna(c_int chn, Xmp_Inst_Nna nna)
		{
			Player_Data p = ctx.P;
			Module_Data m = ctx.M;

			if (!Common.Has_Quirk(m, Quirk_Flag.Virtual))
				return;

			c_int voc = Map_Virt_Channel(p, chn);
			if (voc < 0)
				return;

			p.Virt.Voice_Array[voc].Act = (Virt_Action)nna;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void LibXmp_Virt_SetEffect(c_int chn, Dsp_Effect type, c_int val)
		{
			Player_Data p = ctx.P;

			c_int voc = Map_Virt_Channel(p, chn);
			if (voc < 0)
				return;

			lib.mixer.LibXmp_Mixer_SetEffect(voc, type, val);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_double LibXmp_Virt_GetVoicePos(c_int chn)
		{
			Player_Data p = ctx.P;

			c_int voc = Map_Virt_Channel(p, chn);
			if (voc < 0)
				return -1;

			return lib.mixer.LibXmp_Mixer_GetVoicePos(voc);
		}



		/********************************************************************/
		/// <summary>
		/// For note slides
		/// </summary>
		/********************************************************************/
		public void LibXmp_Virt_SetNote(c_int chn, c_int note)
		{
			Player_Data p = ctx.P;

			c_int voc = Map_Virt_Channel(p, chn);
			if (voc < 0)
				return;

			lib.mixer.LibXmp_Mixer_SetNote(voc, note);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void LibXmp_Virt_SetPeriod(c_int chn, c_double period)
		{
			Player_Data p = ctx.P;

			c_int voc = Map_Virt_Channel(p, chn);
			if (voc < 0)
				return;

			lib.mixer.LibXmp_Mixer_SetPeriod(voc, period);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int LibXmp_Virt_SetPatch(c_int chn, c_int ins, c_int smp, c_int note, c_int key, Xmp_Inst_Nna nna, Xmp_Inst_Dct dct, Xmp_Inst_Dca dca)
		{
			Player_Data p = ctx.P;

			if ((uint32)chn >= p.Virt.Virt_Channels)
				return -1;

			if (ins < 0)
				smp = -1;

			if (dct != 0)
			{
				for (c_int i = 0; i < p.Virt.MaxVoc; i++)
					Check_Dct(i, chn, ins, smp, key, nna, dct, dca);
			}

			c_int voc = p.Virt.Virt_Channel[chn].Map;

			if (voc > Free)
			{
				if (p.Virt.Voice_Array[voc].Act != 0)
				{
					c_int vFree = Alloc_Voice(chn);

					if (vFree < 0)
						return -1;

					for (chn = p.Virt.Num_Tracks; (chn < p.Virt.Virt_Channels) && (p.Virt.Virt_Channel[chn++].Map > Free);)
					{
					}

					p.Virt.Voice_Array[voc].Chn = --chn;
					p.Virt.Virt_Channel[chn].Map = voc;
					voc = vFree;
				}
			}
			else
			{
				voc = Alloc_Voice(chn);
				if (voc < 0)
					return -1;
			}

			if (smp < 0)
			{
				LibXmp_Virt_ResetVoice(voc, true);
				return chn;		// was -1
			}

			lib.mixer.LibXmp_Mixer_SetPatch(voc, smp, true);
			lib.mixer.LibXmp_Mixer_SetNote(voc, note);

			p.Virt.Voice_Array[voc].Ins = ins;
			p.Virt.Voice_Array[voc].Act = (Virt_Action)nna;
			p.Virt.Voice_Array[voc].Key = key;

			return chn;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int LibXmp_Virt_QueuePatch(c_int chn, c_int ins, c_int smp, c_int note)
		{
			// Protracker 1/2 implements instrument swap in a strange way--the
			// volume/finetune take effect immediately but the sample change
			// does not apply until the current playing sample reaches the end of
			// its loop (or stops, if it's a one-off)
			Player_Data p = ctx.P;

			if ((uint32)chn >= p.Virt.Virt_Channels)
				return -1;

			if (ins < 0)
				smp = -1;

			c_int voc = p.Virt.Virt_Channel[chn].Map;

			if (voc > Free)
			{
				lib.mixer.LibXmp_Mixer_QueuePatch(voc, smp);

				if (ins >= 0)
					p.Virt.Voice_Array[voc].Ins = ins;

				return chn;
			}

			// Original sample stopped--start a new note
			if (smp < 0)
				return -1;

			return LibXmp_Virt_SetPatch(chn, ins, smp, note, 0, Xmp_Inst_Nna.Cut, Xmp_Inst_Dct.Off, Xmp_Inst_Dca.Cut);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void LibXmp_Virt_VoicePos(c_int chn, c_double pos)
		{
			Player_Data p = ctx.P;

			c_int voc = Map_Virt_Channel(p, chn);
			if (voc < 0)
				return;

			lib.mixer.LibXmp_Mixer_VoicePos(voc, pos, true);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void LibXmp_Virt_PastNote(c_int chn, Virt_Action act)
		{
			Player_Data p = ctx.P;

			for (c_int c = p.Virt.Num_Tracks; c < p.Virt.Virt_Channels; c++)
			{
				c_int voc = Map_Virt_Channel(p, c);
				if (voc < 0)
					continue;

				if (p.Virt.Voice_Array[voc].Root == chn)
				{
					switch (act)
					{
						case Virt_Action.Cut:
						{
							LibXmp_Virt_ResetVoice(voc, true);
							break;
						}

						case Virt_Action.Off:
						{
							lib.player.LibXmp_Player_Set_Release(c);
							break;
						}

						case Virt_Action.Fade:
						{
							lib.player.LibXmp_Player_Set_FadeOut(c);
							break;
						}
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Virt_Action LibXmp_Virt_CStat(c_int chn)
		{
			Player_Data p = ctx.P;

			c_int voc = Map_Virt_Channel(p, chn);
			if (voc < 0)
				return Virt_Action.Invalid;

			if (chn < p.Virt.Num_Tracks)
				return Virt_Action.Active;

			return p.Virt.Voice_Array[voc].Act;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Free_Voice()
		{
			Player_Data p = ctx.P;

			// Find background voice with lowest volume
			c_int num = Free;
			c_int vol = c_int.MaxValue;

			for (c_int i = 0; i < p.Virt.MaxVoc; i++)
			{
				Mixer_Voice vi = p.Virt.Voice_Array[i];

				if ((vi.Chn >= p.Virt.Num_Tracks) && (vi.Vol < vol))
				{
					num = i;
					vol = vi.Vol;
				}
			}

			// Free voice
			if (num >= 0)
			{
				p.Virt.Virt_Channel[p.Virt.Voice_Array[num].Chn].Map = Free;
				p.Virt.Virt_Channel[p.Virt.Voice_Array[num].Root].Count--;
				p.Virt.Virt_Used--;
			}

			return num;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Alloc_Voice(c_int chn)
		{
			Player_Data p = ctx.P;
			c_int i;

			// Find free voice
			for (i = 0; i < p.Virt.MaxVoc; i++)
			{
				if (p.Virt.Voice_Array[i].Chn == Free)
					break;
			}

			// Not found
			if (i == p.Virt.MaxVoc)
				i = Free_Voice();

			if (i >= 0)
			{
				p.Virt.Virt_Channel[chn].Count++;
				p.Virt.Virt_Used++;

				p.Virt.Voice_Array[i].Chn = chn;
				p.Virt.Voice_Array[i].Root = chn;
				p.Virt.Virt_Channel[chn].Map = i;
			}

			return i;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Map_Virt_Channel(Player_Data p, c_int chn)
		{
			if ((uint32)chn >= p.Virt.Virt_Channels)
				return -1;

			c_int voc = p.Virt.Virt_Channel[chn].Map;

			if ((uint32)voc >= p.Virt.MaxVoc)
				return -1;

			return voc;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Check_Dct(c_int i, c_int chn, c_int ins, c_int smp, c_int key, Xmp_Inst_Nna nna, Xmp_Inst_Dct dct, Xmp_Inst_Dca dca)
		{
			Player_Data p = ctx.P;
			Mixer_Voice vi = p.Virt.Voice_Array[i];

			c_int voc = p.Virt.Virt_Channel[chn].Map;

			if ((vi.Root == chn) && (vi.Ins == ins))
			{
				if (nna == Xmp_Inst_Nna.Cut)
				{
					LibXmp_Virt_ResetVoice(i, true);
					return;
				}

				vi.Act = (Virt_Action)nna;

				if ((dct == Xmp_Inst_Dct.Inst) || ((dct == Xmp_Inst_Dct.Smp) && (vi.Smp == smp)) || ((dct == Xmp_Inst_Dct.Note) && (vi.Key == key)))
				{
					if ((nna == Xmp_Inst_Nna.Off) && (dca == Xmp_Inst_Dca.Fade))
						vi.Act = Virt_Action.Off;
					else if (dca != 0)
					{
						if ((i != voc) || (vi.Act != 0))
							vi.Act = (Virt_Action)dca;
					}
					else
						LibXmp_Virt_ResetVoice(i, true);
				}
			}
		}
		#endregion
	}
}
