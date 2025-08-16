/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Kit.C;

namespace Polycode.NostalgicPlayer.Ports.LibVorbis.Internal
{
	/// <summary>
	/// LSP (also called LSF) conversion routines
	///
	/// The LSP generation code is taken (with minimal modification and a
	/// few bugfixes) from "On the Computation of the LSP Frequencies" by
	/// Joseph Rothweiler (see http://www.rothweiler.us for contact info).
	///
	/// The paper is available at:
	///
	/// https://web.archive.org/web/20110810174000/http://home.myfairpoint.net/vzenxj75/myown1/joe/lsf/index.html
	///
	/// Note that the lpc-lsp conversion finds the roots of polynomial with
	/// an iterative root polisher (CACM algorithm 283). It *is* possible
	/// to confuse this algorithm into not converging; that should only
	/// happen with absurdly closely spaced roots (very sharp peaks in the
	/// LPC f response) which in turn should be impossible in our use of
	/// the code. If this *does* happen anyway, it's a bug in the floor
	/// finder; find the cause of the confusion (probably a single bin
	/// spike or accidental near-float-limit resolution problems) and
	/// correct it
	/// </summary>
	internal static class Lsp
	{
		/********************************************************************/
		/// <summary>
		/// Old, nonoptimized but simple version for any poor sap who needs
		/// to figure out what the hell this code does, or wants the other
		/// fraction of a dB precision.
		/// 
		/// Side effect: changes *lsp to cosines of lsp
		/// </summary>
		/********************************************************************/
		public static void Vorbis_Lsp_To_Curve(CPointer<c_float> curve, CPointer<c_int> map, c_int n, c_int ln, Span<c_float> lsp, c_int m, c_float amp, c_float ampoffset)
		{
			c_int i;
			c_float wdel = (c_float)Math.PI / ln;

			for (i = 0; i < m; i++)
				lsp[i] = (c_float)(2.0f * Math.Cos(lsp[i]));

			i = 0;

			while (i < n)
			{
				c_int j;
				c_int k = map[i];
				c_float p = 0.5f;
				c_float q = 0.5f;
				c_float w = (c_float)(2.0f * Math.Cos(wdel * k));

				for (j = 1; j < m; j += 2)
				{
					q *= w - lsp[j - 1];
					p *= w - lsp[j];
				}

				if (j == m)
				{
					// Odd order filter; slightly asymmetric
					// the last coefficient
					q *= w - lsp[j - 1];
					p *= p * (4.0f - w * w);
					q *= q;
				}
				else
				{
					// Even order filter; still symmetric
					p *= p * (2.0f - w);
					q *= q * (2.0f + w);
				}

				q = Scales.FromDb(amp / (c_float)Math.Sqrt(p + q) - ampoffset);

				curve[i] *= q;

				while (map[++i] == k)
					curve[i] *= q;
			}
		}
	}
}
