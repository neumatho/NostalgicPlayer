/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.ReSidFp
{
	/// <summary>
	/// 
	/// </summary>
	internal class Integrator6581 : Integrator
	{
		// Find output voltage in inverting integrator SID op-amp circuits, using a
		// single fixpoint iteration step.
		//
		// A circuit diagram of a MOS 6581 integrator is shown below.
		//
		//                   +---C---+
		//                   |       |
		//     vi --o--Rw--o-o--[A>--o-- vo
		//          |      | vx
		//          +--Rs--+
		//
		// From Kirchoff's current law it follows that
		//
		//     IRw + IRs + ICr = 0
		//
		// Using the formula for current through a capacitor, i = C*dv/dt, we get
		//
		//     IRw + IRs + C*(vc - vc0)/dt = 0
		//     dt/C*(IRw + IRs) + vc - vc0 = 0
		//     vc = vc0 - n*(IRw(vi,vx) + IRs(vi,vx))
		//
		// which may be rewritten as the following iterative fixpoint function:
		//
		//     vc = vc0 - n*(IRw(vi,g(vc)) + IRs(vi,g(vc)))
		//
		// To accurately calculate the currents through Rs and Rw, we need to use
		// transistor models. Rs has a gate voltage of Vdd = 12V, and can be
		// assumed to always be in triode mode. For Rw, the situation is rather
		// more complex, as it turns out that this transistor will operate in
		// both subthreshold, triode, and saturation modes.
		//
		// The Shichman-Hodges transistor model routinely used in textbooks may
		// be written as follows:
		//
		//     Ids = 0                          , Vgst < 0               (subthreshold mode)
		//     Ids = K*W/L*(2*Vgst - Vds)*Vds   , Vgst >= 0, Vds < Vgst  (triode mode)
		//     Ids = K*W/L*Vgst^2               , Vgst >= 0, Vds >= Vgst (saturation mode)
		//
		// where
		//     K   = u*Cox/2 (transconductance coefficient)
		//     W/L = ratio between substrate width and length
		//     Vgst = Vg - Vs - Vt (overdrive voltage)
		//
		// This transistor model is also called the quadratic model.
		//
		// Note that the equation for the triode mode can be reformulated as
		// independent terms depending on Vgs and Vgd, respectively, by the
		// following substitution:
		//
		//     Vds = Vgst - (Vgst - Vds) = Vgst - Vgdt
		//
		//     Ids = K*W/L*(2*Vgst - Vds)*Vds
		//         = K*W/L*(2*Vgst - (Vgst - Vgdt)*(Vgst - Vgdt)
		//         = K*W/L*(Vgst + Vgdt)*(Vgst - Vgdt)
		//         = K*W/L*(Vgst^2 - Vgdt^2)
		//
		// This turns out to be a general equation which covers both the triode
		// and saturation modes (where the second term is 0 in saturation mode).
		// The equation is also symmetrical, i.e. it can calculate negative
		// currents without any change of parameters (since the terms for drain
		// and source are identical except for the sign).
		//
		// FIXME: Subthreshold as function of Vgs, Vgd.
		//
		//     Ids = I0*W/L*e^(Vgst/(Ut/k))   , Vgst < 0               (subthreshold mode)
		//
		// where
		//     I0 = (2 * uCox * Ut^2) / k
		//
		// The remaining problem with the textbook model is that the transition
		// from subthreshold to triode/saturation is not continuous.
		//
		// Realizing that the subthreshold and triode/saturation modes may both
		// be defined by independent (and equal) terms of Vgs and Vds,
		// respectively, the corresponding terms can be blended into (equal)
		// continuous functions suitable for table lookup.
		//
		// The EKV model (Enz, Krummenacher and Vittoz) essentially performs this
		// blending using an elegant mathematical formulation:
		//
		//     Ids = Is * (if - ir)
		//     Is = ((2 * u*Cox * Ut^2)/k) * W/L
		//     if = ln^2(1 + e^((k*(Vg - Vt) - Vs)/(2*Ut))
		//     ir = ln^2(1 + e^((k*(Vg - Vt) - Vd)/(2*Ut))
		//
		// For our purposes, the EKV model preserves two important properties
		// discussed above:
		//
		// - It consists of two independent terms, which can be represented by
		//   the same lookup table.
		// - It is symmetrical, i.e. it calculates current in both directions,
		//   facilitating a branch-free implementation.
		//
		// Rw in the circuit diagram above is a VCR (voltage controlled resistor),
		// as shown in the circuit diagram below.
		//
		//
		//                        Vdd
		//                           |
		//              Vdd         _|_
		//                 |    +---+ +---- Vw
		//                _|_   |
		//             +--+ +---o Vg
		//             |      __|__
		//             |      -----  Rw
		//             |      |   |
		//     vi -----o------+   +-------- vo
		//
		//
		// In order to calculalate the current through the VCR, its gate voltage
		// must be determined.
		//
		// Assuming triode mode and applying Kirchoff's current law, we get the
		// following equation for Vg:
		//
		//     u*Cox/2*W/L*((nVddt - Vg)^2 - (nVddt - vi)^2 + (nVddt - Vg)^2 - (nVddt - Vw)^2) = 0
		//     2*(nVddt - Vg)^2 - (nVddt - vi)^2 - (nVddt - Vw)^2 = 0
		//     (nVddt - Vg) = sqrt(((nVddt - vi)^2 + (nVddt - Vw)^2)/2)
		//
		//     Vg = nVddt - sqrt(((nVddt - vi)^2 + (nVddt - Vw)^2)/2)

		private readonly double wlSnake;

		private uint nVddt_vw_2;

		private readonly ushort nVddt;
		private readonly ushort nVt;
		private readonly ushort nVMin;

		private readonly FilterModelConfig6581 fmc;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Integrator6581(FilterModelConfig6581 fmc)
		{
			wlSnake = fmc.GetWl_Snake();
			nVddt_vw_2 = 0;
			nVddt = fmc.GetNormalizedValue(fmc.GetVddt());
			nVt = fmc.GetNormalizedValue(fmc.GetVth());
			nVMin = fmc.GetNVMin();
			this.fmc = fmc;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void SetVw(ushort vw)
		{
			nVddt_vw_2 = (uint)(((nVddt - vw) * (nVddt - vw)) >> 1);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override int Solve(int vi)
		{
			// "Snake" voltages for triode mode calculation
			uint vgst = (uint)(nVddt - vx);
			uint vgdt = (uint)(nVddt - vi);

			uint vgst_2 = vgst * vgst;
			uint vgdt_2 = vgdt * vgdt;

			// "Snake" current, scaled by (1/m)*2^13*m*2^16*m*2^16*2^-15 = m*2^30
			int n_I_snake = fmc.GetNormalizedCurrentFactor(13, wlSnake) * ((int)(vgst_2 - vgdt_2) >> 15);

			// VCR gate voltage.		// Scaled by m*2^16
			// Vg = Vddt - sqrt(((Vddt - vW)^2 + Vgdt^2)/2)
			int nVg = fmc.GetVcr_nVg((nVddt_vw_2 + (vgdt_2 >> 1)) >> 16);
			int kVgt = (nVg - nVt) - nVMin;

			// VCR voltages for EKV model table lookup
			int kVgt_Vs = (kVgt - vx) - Int16.MinValue;
			int kVgt_Vd = (kVgt - vi) - Int16.MinValue;

			// VCR current, scaled by m*2^15*2^15 = m*2^30
			uint @if = (uint)(fmc.GetVcr_n_Ids_Term(kVgt_Vs)) << 15;
			uint ir = (uint)(fmc.GetVcr_n_Ids_Term(kVgt_Vd)) << 15;
			int n_I_vcr = (int)(@if - ir);

			// Change in capacitor charge
			vc += n_I_snake + n_I_vcr;

			// vx = g(vc)
			int tmp = (vc >> 15) - Int16.MinValue;
			vx = fmc.GetOpampRev(tmp);

			// Return vo
			return vx - (vc >> 14);
		}
	}
}
