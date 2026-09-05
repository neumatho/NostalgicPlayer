/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
global using WFir_Type = System.Int16;

using System;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib
{
	/// <summary>
	/// FIR resampling code
	/// </summary>
	internal class CWindowedFir
	{
		public readonly WFir_Type[] Lut = new WFir_Type[WindowedFir.WFir_LutLen * WindowedFir.WFir_Width];

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void InitTable(c_double wfirCutOff, WFirType wfirType)
		{
			c_double pclLen = 1 << WindowedFir.WFir_FracBits;	// Number of precalculated lines for 0..1 (-1..0)
			c_double norm = 1.0 / (2.0 * pclLen);
			c_double cut = wfirCutOff;
			Span<c_double> coefs = stackalloc c_double[WindowedFir.WFir_Width];

			for (c_int pcl = 0; pcl < WindowedFir.WFir_LutLen; pcl++)
			{
				c_double gain = 0.0;
				c_double ofs = (pcl - pclLen) * norm;
				c_int idx = pcl << WindowedFir.WFir_Log2Width;

				for (c_int cc = 0; cc < WindowedFir.WFir_Width; cc++)
				{
					coefs[cc] = Coef(cc, ofs, cut, WindowedFir.WFir_Width, wfirType);
					gain += coefs[cc];
				}

				gain = 1.0 / gain;

				for (c_int cc = 0; cc < WindowedFir.WFir_Width; cc++)
				{
					c_double coef = CMath.floor(0.5 + (WindowedFir.WFir_QuantScale * coefs[cc] * gain));
					Lut[idx + cc] = (c_short)((coef < -WindowedFir.WFir_QuantScale) ? -WindowedFir.WFir_QuantScale : ((coef > WindowedFir.WFir_QuantScale) ? WindowedFir.WFir_QuantScale : coef));
				}
			}
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_double Coef(c_int cnr, c_double ofs, c_double cut, c_int width, WFirType type)
		{
			c_double epsilon = 1e-8;
			c_double widthM1 = width - 1;
			c_double widthM1Half = 0.5 * widthM1;
			c_double posU = (cnr - ofs);
			c_double idl = (2.0 * Math.PI) / widthM1;
			c_double pos = posU - widthM1Half;
			c_double wc, si;

			if (CMath.abs(pos) < epsilon)
			{
				wc = 1.0;
				si = cut;
			}
			else
			{
				switch (type)
				{
					case WFirType.Hann:
					{
						wc = 0.50 - (0.50 * CMath.cos(idl * posU));
						break;
					}

					case WFirType.Hamming:
					{
						wc = 0.54 - (0.46 * CMath.cos(idl * posU));
						break;
					}

					case WFirType.BlackmanExact:
					{
						wc = 0.42 - (0.50 * CMath.cos(idl * posU)) + (0.08 * CMath.cos(2.0 * idl * posU));
						break;
					}

					case WFirType.Blackman3T61:
					{
						wc = 0.44959 - (0.49364 * CMath.cos(idl * posU)) + (0.05677 * CMath.cos(2.0 * idl * posU));
						break;
					}

					case WFirType.Blackman3T67:
					{
						wc = 0.42323 - (0.49755 * CMath.cos(idl * posU)) + (0.07922 * CMath.cos(2.0 * idl * posU));
						break;
					}

					case WFirType.Blackman4T92:
					{
						wc = 0.35875 - (0.48829 * CMath.cos(idl * posU)) + (0.14128 * CMath.cos(2.0 * idl * posU)) - (0.01168 * CMath.cos(3.0 * idl * posU));
						break;
					}

					case WFirType.Blackman4T74:
					{
						wc = 0.40217 - (0.49703 * CMath.cos(idl * posU)) + (0.09392 * CMath.cos(2.0 * idl * posU)) - (0.00183 * CMath.cos(3.0 * idl * posU));
						break;
					}

					case WFirType.Kaiser4T:	// Kaiser-Bessel, alpha~7.5
					{
						wc = 0.40243 - (0.49804 * CMath.cos(idl * posU)) + (0.09831 * CMath.cos(2.0 * idl * posU)) - (0.00122 * CMath.cos(3.0 * idl * posU));
						break;
					}

					default:
					{
						wc = 1.0;
						break;
					}
				}

				pos *= Math.PI;
				si = CMath.sin(cut * pos) / pos;
			}

			return wc * si;
		}
		#endregion
	}
}
