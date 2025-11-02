/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Ports.LibOpus.Containers;
using Polycode.NostalgicPlayer.Ports.LibOpus.Internal.Celt;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibOpus.Test.Celt
{
	/// <summary>
	/// </summary>
	[TestClass]
	public class Test_Unit_Dft
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Dft()
		{
			c_int arch = Cpu_Support.Opus_Select_Arch();

			Test1d(32, false, arch);
			Test1d(32, true, arch);
			Test1d(128, false, arch);
			Test1d(128, true, arch);
			Test1d(256, false, arch);
			Test1d(256, true, arch);

			Test1d(36, false, arch);
			Test1d(36, true, arch);
			Test1d(50, false, arch);
			Test1d(50, true, arch);
			Test1d(60, false, arch);
			Test1d(60, true, arch);
			Test1d(120, false, arch);
			Test1d(120, true, arch);
			Test1d(240, false, arch);
			Test1d(240, true, arch);
			Test1d(480, false, arch);
			Test1d(480, true, arch);
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Check(CPointer<Kiss_Fft_Cpx> _in, CPointer<Kiss_Fft_Cpx> _out, c_int nfft, bool isinverse)
		{
			c_double errpow = 0, sigpow = 0;

			for (c_int bin = 0; bin < nfft; ++bin)
			{
				c_double ansr = 0;
				c_double ansi = 0;

				for (c_int k = 0; k < nfft; ++k)
				{
					c_double phase = -2 * Math.PI * bin * k / nfft;
					c_double re = Math.Cos(phase);
					c_double im = Math.Sin(phase);

					if (isinverse)
						im = -im;

					if (!isinverse)
					{
						re /= nfft;
						im /= nfft;
					}

					ansr += _in[k].r * re - _in[k].i * im;
					ansi += _in[k].r * im + _in[k].i * re;
				}

				c_double difr = ansr - _out[bin].r;
				c_double difi = ansi - _out[bin].i;

				errpow += difr * difr + difi * difi;
				sigpow += ansr * ansr + ansi * ansi;
			}

			c_double snr = 10 * Math.Log10(sigpow / errpow);
			Console.WriteLine($"nfft={nfft} inverse={isinverse},snr = {snr}");

			if (snr < 60)
				Assert.Fail($"** poor snr: {snr}");
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Test1d(c_int nfft, bool isinverse, c_int arch)
		{
			size_t buflen = (size_t)nfft;

			CeltMode mode = Modes.Opus_Custom_Mode_Create(48000, 960, out _);

			c_int id;
			if (nfft == 480)
				id = 0;
			else if (nfft == 240)
				id = 1;
			else if (nfft == 120)
				id = 2;
			else if (nfft == 60)
				id = 3;
			else
				return;

			Kiss_Fft_State cfg = mode.mdct.kfft[id];

			CPointer<Kiss_Fft_Cpx> _in = CMemory.malloc<Kiss_Fft_Cpx>(buflen);
			CPointer<Kiss_Fft_Cpx> _out = CMemory.malloc<Kiss_Fft_Cpx>(buflen);

			for (c_int k = 0; k < nfft; ++k)
			{
				_in[k].r = (RandomGenerator.GetRandomNumber() % 32767) - 16384;
				_in[k].i = (RandomGenerator.GetRandomNumber() % 32767) - 16384;
			}

			for (c_int k = 0; k < nfft; ++k)
			{
				_in[k].r *= 32768;
				_in[k].i *= 32768;
			}

			if (isinverse)
			{
				for (c_int k = 0; k < nfft; ++k)
				{
					_in[k].r /= nfft;
					_in[k].i /= nfft;
				}
			}

			if (isinverse)
				Kiss_Fft.Opus_Ifft(cfg, _in, _out.AsSpan(), arch);
			else
				Kiss_Fft.Opus_Fft(cfg, _in, _out.AsSpan(), arch);

			Check(_in, _out, nfft, isinverse);
		}
		#endregion
	}
}
