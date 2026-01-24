/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Common;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Player;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp;

namespace Polycode.NostalgicPlayer.Ports.LibXmp.FormatExtras
{
	/// <summary>
	/// 
	/// </summary>
	internal class Far_Channel_Extra : Player_Helpers, IChannelReset, IChannelProcessFx, IChannelPlay
	{
		public class Far_Channel_Extra_Info : IChannelExtraInfo
		{
			public c_int Vib_Sustain;		// Is vibrato persistent?
			public c_int Vib_Rate;			// Vibrato rate
		}

		private readonly LibXmp lib;
		private readonly Xmp_Context ctx;

		private Channel_Data xc;
		private Far_Channel_Extra_Info extraInfo;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Far_Channel_Extra(LibXmp libXmp, Xmp_Context ctx, Channel_Data xc)
		{
			lib = libXmp;
			this.ctx = ctx;

			this.xc = xc;
			extraInfo = new Far_Channel_Extra_Info();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public IChannelExtraInfo Channel_Extras => extraInfo;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void SetChannel(Channel_Data xc)
		{
			this.xc = xc;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Play(c_int chn)
		{
			Far_Module_Extra.Far_Module_Extra_Info me = (Far_Module_Extra.Far_Module_Extra_Info)ctx.M.Extra.Module_Extras;
			Far_Channel_Extra_Info ce = extraInfo;

			// FAR vibrato depth is global, even though rate isn't. This might have
			// been changed by a different channel, so make sure it's applied
			if (Test(xc, Channel_Flag.Vibrato) || Test_Per(xc, Channel_Flag.Vibrato))
				LibXmp_Far_Update_Vibrato(xc.Vibrato.Lfo, ce.Vib_Rate, me.Vib_Depth);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Reset_Channel()
		{
			extraInfo.Vib_Sustain = 0;
			extraInfo.Vib_Rate = 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Release_Channel_Extras()
		{
			xc.Extra = null;

			xc = null;
			extraInfo = null;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Process_Fx(c_int chn, uint8 note, uint8 fxT, uint8 fxP, c_int fNum)
		{
			Xmp_Module mod = ctx.M.Mod;
			Far_Module_Extra.Far_Module_Extra_Info me = (Far_Module_Extra.Far_Module_Extra_Info)ctx.M.Extra.Module_Extras;
			Far_Channel_Extra_Info ce = extraInfo;
			bool update_Tempo = false;
			bool update_Vibrato = false;
			c_int fine_Change = 0;

			// Tempo effects and vibrato are multiplexed to reduce the effects count.
			//
			// Misc. notes: FAR pitch offset effects can overflow/underflow GUS
			// frequency, which isn't supported by libxmp (Haj/before.far)
			switch (fxT)
			{
				// FAR pitch offset up
				case Effects.Fx_Far_Porta_Up:
				{
					Set(xc, Channel_Flag.Fine_Bend);
					Reset_Per(xc, Channel_Flag.TonePorta);

					xc.Freq.FSlide = lib.period.LibXmp_Gus_Frequency_Steps(fxP << 2, Far_Module_Extra.Far_Gus_Channels);
					break;
				}

				// FAR pitch offset down
				case Effects.Fx_Far_Porta_Dn:
				{
					Set(xc, Channel_Flag.Fine_Bend);
					Reset_Per(xc, Channel_Flag.TonePorta);

					xc.Freq.FSlide = -lib.period.LibXmp_Gus_Frequency_Steps(fxP << 2, Far_Module_Extra.Far_Gus_Channels);
					break;
				}

				// Despite some claims, this effect scales with tempo and only
				// corresponds to (param) rows at tempo 4. See FORMATS.DOC
				//
				// FAR persistent tone portamento
				case Effects.Fx_Far_TPorta:
				{
					if (!Is_Valid_Instrument(mod, xc.Ins))
						break;

					c_int tempo = Far_Module_Extra.Far_Tempos[me.Coarse_Tempo] + me.Fine_Tempo;

					Set_Per(xc, Channel_Flag.TonePorta);

					if (Is_Valid_Note(note - 1))
						xc.Porta.Target = lib.period.LibXmp_Note_To_Period(note - 1, xc.FineTune, xc.Per_Adj);

					xc.Porta.Dir = xc.Period < xc.Porta.Target ? 1 : -1;

					// Parameter of 0 is equivalent to 1
					if (fxP < 1)
						fxP = 1;

					// Tempos <=0 cause crashes and other weird behavior
					// here in Farandole Composer, don't emulate that
					if (tempo < 1)
						tempo = 1;

					int32 diff = (int32)(xc.Porta.Target - xc.Period);
					int32 step = (diff > 0 ? diff : -diff) * 8 / (tempo * fxP);

					xc.Porta.Slide = (step > 0) ? step : 1;
					break;
				}

				// Despite some claims, this effect scales with tempo and only
				// corresponds to (param) rows at tempo 4. See FORMATS.DOC
				//
				// FAR persistent slide-to-volume
				case Effects.Fx_Far_SlideVol:
				{
					c_int tempo = Far_Module_Extra.Far_Tempos[me.Coarse_Tempo] + me.Fine_Tempo;
					c_int target = Common.Msn(fxP) << 4;
					fxP = Common.Lsn(fxP);

					// Parameter of 0 is equivalent to 1
					if (fxP < 1)
						fxP = 1;

					// Tempos <=0 cause crashes and other weird behavior
					// here in Farandole Composer, don't emulate that
					if (tempo < 1)
						tempo = 1;

					int32 diff = target - xc.Volume;
					int32 step = diff * 16 / (tempo * fxP);

					if (step == 0)
						step = (diff > 0) ? 1 : -1;

					Set_Per(xc, Channel_Flag.Vol_Slide);

					xc.Vol.Slide = step;
					xc.Vol.Target = target + 1;
					break;
				}

				// FAR set vibrato depth
				case Effects.Fx_Far_VibDepth:
				{
					me.Vib_Depth = Common.Lsn(fxP);
					update_Vibrato = true;
					break;
				}

				// FAR vibrato and sustained vibrato
				case Effects.Fx_Far_Vibrato:
				{
					if (ce.Vib_Sustain == 0)
					{
						// With sustain, regular vibrato only sets the rate
						ce.Vib_Sustain = Common.Msn(fxP);

						if (ce.Vib_Sustain == 0)
							Set(xc, Channel_Flag.Vibrato);
					}

					ce.Vib_Rate = Common.Lsn(fxP);
					update_Vibrato = true;
					break;
				}

				// Retrigger note param times at intervals that roughly evently
				// divide the row. A param of 0 crashes Farandole Composer
				case Effects.Fx_Far_Retrig:
				{
					c_int delay = LibXmp_Far_Retrigger_Delay(me, fxP);

					if ((note != 0) && (fxP > 1) && (delay >= 0) && (delay <= ctx.P.Speed))
					{
						Set(xc, Channel_Flag.Retrig);

						xc.Retrig.Val = delay != 0 ? delay : 1;
						xc.Retrig.Count = delay + 1;
						xc.Retrig.Type = 0;
						xc.Retrig.Limit = fxP - 1;
					}

					break;
				}

				// A better effect name would probably be "retrigger once".
				// The description/intent seems to be that this is a delay
				// effect, but an initial note always plays as well. The second
				// note always plays on the (param)th tick due to player quirks,
				// but it's supposed to be derived similar to retrigger.
				// A param of zero works like effect 4F (bug?).
				//
				// FAR note offset
				case Effects.Fx_Far_Delay:
				{
					if (note != 0)
					{
						c_int delay = me.Tempo_Mode != 0 ? fxP : fxP << Far_Module_Extra.Far_Old_Tempo_Shift;
						Set(xc, Channel_Flag.Retrig);

						xc.Retrig.Val = delay != 0 ? delay : 1;
						xc.Retrig.Count = delay + 1;
						xc.Retrig.Type = 0;
						xc.Retrig.Limit = fxP != 0 ? 1 : 0;
					}

					break;
				}

				// FAR coarse tempo and tempo mode
				case Effects.Fx_Far_Tempo:
				{
					if (Common.Msn(fxP) != 0)
						me.Tempo_Mode = Common.Msn(fxP) - 1;
					else
						me.Coarse_Tempo = Common.Lsn(fxP);

					update_Tempo = true;
					break;
				}

				// FAR fine tempo slide up/down
				case Effects.Fx_Far_F_Tempo:
				{
					if (Common.Msn(fxP) != 0)
					{
						me.Fine_Tempo += Common.Msn(fxP);
						fine_Change = Common.Msn(fxP);
					}
					else if (Common.Lsn(fxP) != 0)
					{
						me.Fine_Tempo -= Common.Lsn(fxP);
						fine_Change = -Common.Lsn(fxP);
					}
					else
						me.Fine_Tempo = 0;

					update_Tempo = true;
					break;
				}
			}

			if (update_Vibrato)
			{
				if (ce.Vib_Rate != 0)
				{
					if (ce.Vib_Sustain != 0)
						Set_Per(xc, Channel_Flag.Vibrato);
				}
				else
				{
					Reset_Per(xc, Channel_Flag.Vibrato);
					ce.Vib_Sustain = 0;
				}

				LibXmp_Far_Update_Vibrato(xc.Vibrato.Lfo, ce.Vib_Rate, me.Vib_Depth);
			}

			if (update_Tempo)
				LibXmp_Far_Update_Tempo(fine_Change);
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void LibXmp_Far_Update_Tempo(c_int fine_Change)
		{
			Player_Data p = ctx.P;
			Module_Data m = ctx.M;
			Far_Module_Extra.Far_Module_Extra_Info me = (Far_Module_Extra.Far_Module_Extra_Info)m.Extra.Module_Extras;
			c_int speed = 0, bpm = 0;

			if (((Far_Module_Extra)m.Extra).LibXmp_Far_Translate_Tempo(me.Tempo_Mode, fine_Change, me.Coarse_Tempo, ref me.Fine_Tempo, ref speed, ref bpm) == 0)
			{
				p.Speed = speed;
				p.Bpm = bpm;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void LibXmp_Far_Update_Vibrato(Containers.Lfo.Lfo lfo, c_int rate, c_int depth)
		{
			lib.lfo.LibXmp_Lfo_Set_Depth(lfo, (c_int)lib.period.LibXmp_Gus_Frequency_Steps(depth << 1, Far_Module_Extra.Far_Gus_Channels));
			lib.lfo.LibXmp_Lfo_Set_Rate(lfo, rate * 3);
		}



		/********************************************************************/
		/// <summary>
		/// Convoluted algorithm for delay times for retrigger and note
		/// offset effects
		/// </summary>
		/********************************************************************/
		private c_int LibXmp_Far_Retrigger_Delay(Far_Module_Extra.Far_Module_Extra_Info me, c_int param)
		{
			if ((me.Coarse_Tempo < 0) || (me.Coarse_Tempo > 15) || (param < 1))
				return -1;

			c_int delay = (Far_Module_Extra.Far_Tempos[me.Coarse_Tempo] + me.Fine_Tempo) / param;

			if (me.Tempo_Mode != 0)
			{
				// Effects divide by 4, timer increments by 2 (round up)
				return ((delay >> 2) + 1) >> 1;
			}
			else
			{
				// Effects divide by 2, timer increments by 2 (round up).
				// Old tempo mode handles every 8th tick (<< FAR_OLD_TEMPO_SHIFT).
				// Delay values >4 result in no retrigger
				delay = (((delay >> 1) + 1) >> 1) << Far_Module_Extra.Far_Old_Tempo_Shift;

				if (delay >= 16)
					return -1;

				if (delay < (1 << Far_Module_Extra.Far_Old_Tempo_Shift))
					return (1 << Far_Module_Extra.Far_Old_Tempo_Shift);

				return delay;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public IChannelExtra MakeDeepClone()
		{
			Far_Channel_Extra clone = (Far_Channel_Extra)MemberwiseClone();

			clone.extraInfo = new Far_Channel_Extra_Info
			{
				Vib_Sustain = extraInfo.Vib_Sustain,
				Vib_Rate = extraInfo.Vib_Rate
			};

			return clone;
		}
		#endregion
	}
}
