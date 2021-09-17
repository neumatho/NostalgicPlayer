/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.ReSid.Containers;

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.ReSid
{
	/// <summary>
	/// 
	/// </summary>
	internal class ExternalFilter
	{
		// Filter enabled
		private bool enabled;

		// Maximum mixer DC offset
		private int mixerDc;

		// State of filter
		private int vlp;		// Lowpass
		private int vhp;		// highpass
		private int vo;

		// Cutoff frequencies
		private int w0lp;
		private int w0hp;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ExternalFilter()
		{
			Reset();
			EnableFilter(true);
			SetChipModel(ChipModel.Mos6581);

			// Low-pass:  R = 10kOhm, C = 1000pF; w0l = 1/RC = 1/(1e4*1e-9) = 100000
			// High-pass: R =  1kOhm, C =   10uF; w0h = 1/RC = 1/(1e3*1e-5) =    100
			// Multiply with 1.048576 to facilitate division by 1 000 000 by right-
			// shifting 20 times (2 ^ 20 = 1048576)
			w0lp = 104858;
			w0hp = 105;
		}



		/********************************************************************/
		/// <summary>
		/// Enable filter
		/// </summary>
		/********************************************************************/
		public void EnableFilter(bool enable)
		{
			enabled = enable;
		}



		/********************************************************************/
		/// <summary>
		/// Set chip model
		/// </summary>
		/********************************************************************/
		public void SetChipModel(ChipModel model)
		{
			if (model == ChipModel.Mos6581)
			{
				// Maximum mixer DC output level; to be removed if the external
				// filter is turned off: ((wave DC + voice DC) * voices + mixer DC) * volume
				// See Voices.cs and Filter.cs for an explanation of the values
				mixerDc = ((((0x800 - 0x380) + 0x800) * 0xff * 3 - 0xfff * 0xff / 18) >> 7) * 0x0f;
			}
			else
			{
				// No DC offset in MOS8580
				mixerDc = 0;
			}
		}



		/********************************************************************/
		/// <summary>
		/// SID reset
		/// </summary>
		/********************************************************************/
		public void Reset()
		{
			// State of filter
			vlp = 0;
			vhp = 0;
			vo = 0;
		}



		/********************************************************************/
		/// <summary>
		/// SID clocking - 1 cycle
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clock(int vi)
		{
			// This is handy for testing
			if (!enabled)
			{
				// Remove maximum DC level since there is no filter to do it
				vlp = vhp = 0;
				vo = vi - mixerDc;
				return;
			}

			// delta is converted to seconds given a 1 MHz clock by dividing
			// with 1 000 000

			// Calculate filter outputs.
			// Vo  = Vlp - Vhp
			// Vlp = Vlp + w0lp*(Vi - Vlp)*delta
			// Vhp = Vhp + w0hp*(Vlp - Vhp)*delta
			int dVlp = (w0lp >> 8) * (vi - vlp) >> 12;
			int dVhp = w0hp * (vlp - vhp) >> 20;
			vo = vlp - vhp;
			vlp += dVlp;
			vhp += dVhp;
		}



		/********************************************************************/
		/// <summary>
		/// SID clocking - delta cycles
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clock(int delta, int vi)
		{
			// This is handy for testing
			if (!enabled)
			{
				// Remove maximum DC level since there is no filter to do it
				vlp = vhp = 0;
				vo = vi - mixerDc;
				return;
			}

			// Maximum delta cycles for the external filter to work satisfactorily
			// is approximately 8
			int deltaFlt = 8;

			while (delta != 0)
			{
				if (delta < deltaFlt)
					deltaFlt = delta;

				// delta is converted to seconds given a 1 MHz clock by dividing
				// with 1 000 000

				// Calculate filter outputs.
				// Vo  = Vlp - Vhp
				// Vlp = Vlp + w0lp*(Vi - Vlp)*delta
				// Vhp = Vhp + w0hp*(Vlp - Vhp)*delta
				int dVlp = (w0lp * deltaFlt >> 8) * (vi - vlp) >> 12;
				int dVhp = w0hp * deltaFlt * (vlp - vhp) >> 20;
				vo = vlp - vhp;
				vlp += dVlp;
				vhp += dVhp;

				delta -= deltaFlt;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Audio output (19.5 bits)
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int Output()
		{
			return vo;
		}
	}
}
