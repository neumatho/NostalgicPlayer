/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.LibOpus.Containers
{
	/// <summary>
	/// Struct for Packet Loss Concealment
	/// </summary>
	internal class Silk_PLC_Struct : IDeepCloneable<Silk_PLC_Struct>
	{
		/// <summary>
		/// Pitch lag to use for voiced concealment
		/// </summary>
		public opus_int32 pitchL_Q8;

		/// <summary>
		/// LTP coefficients to use for voiced concealment
		/// </summary>
		public readonly opus_int16[] LTPCoef_Q14 = new opus_int16[Constants.Ltp_Order];
		public readonly opus_int16[] prevLPC_Q12 = new opus_int16[Constants.Max_Lpc_Order];

		/// <summary>
		/// Was previous frame lost
		/// </summary>
		public bool last_frame_lost;

		/// <summary>
		/// Seed for unvoiced signal generation
		/// </summary>
		public opus_int32 rand_seed;

		/// <summary>
		/// Scaling of unvoiced random signal
		/// </summary>
		public opus_int16 randScale_Q14;
		public opus_int32 conc_energy;
		public opus_int conc_energy_shift;
		public opus_int16 PrevLTP_scale_Q14;
		public readonly opus_int32[] prevGain_Q16 = new opus_int32[2];
		public opus_int fs_kHz;
		public opus_int nb_subfr;
		public opus_int subfr_length;
		public bool enable_deep_plc;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Clear()
		{
			pitchL_Q8 = 0;

			Array.Clear(LTPCoef_Q14);
			Array.Clear(prevLPC_Q12);

			last_frame_lost = false;
			rand_seed = 0;
			randScale_Q14 = 0;
			conc_energy = 0;
			conc_energy_shift = 0;
			PrevLTP_scale_Q14 = 0;

			Array.Clear(prevGain_Q16);

			fs_kHz = 0;
			nb_subfr = 0;
			subfr_length = 0;
			enable_deep_plc = false;
		}



		/********************************************************************/
		/// <summary>
		/// Clone the current object into a new one
		/// </summary>
		/********************************************************************/
		public Silk_PLC_Struct MakeDeepClone()
		{
			Silk_PLC_Struct clone = new Silk_PLC_Struct
			{
				pitchL_Q8 = pitchL_Q8,
				last_frame_lost = last_frame_lost,
				rand_seed = rand_seed,
				randScale_Q14 = randScale_Q14,
				conc_energy = conc_energy,
				conc_energy_shift = conc_energy_shift,
				PrevLTP_scale_Q14 = PrevLTP_scale_Q14,
				fs_kHz = fs_kHz,
				nb_subfr = nb_subfr,
				subfr_length = subfr_length,
				enable_deep_plc = enable_deep_plc
			};

			Array.Copy(LTPCoef_Q14, clone.LTPCoef_Q14, LTPCoef_Q14.Length);
			Array.Copy(prevLPC_Q12, clone.prevLPC_Q12, prevLPC_Q12.Length);
			Array.Copy(prevGain_Q16, clone.prevGain_Q16, prevGain_Q16.Length);

			return clone;
		}
	}
}
