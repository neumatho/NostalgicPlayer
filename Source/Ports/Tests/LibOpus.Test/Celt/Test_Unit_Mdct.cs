/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.CKit;
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Ports.LibOpus.Containers;
using Polycode.NostalgicPlayer.Ports.LibOpus.Internal.Celt;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibOpus.Test.Celt
{
	/// <summary>
	/// </summary>
	[TestClass]
	public class Test_Unit_Mdct
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Mdct()
		{
			c_int arch = Cpu_Support.Opus_Select_Arch();

			Test1d(32, false, arch);
			Test1d(32, true, arch);
			Test1d(256, false, arch);
			Test1d(256, true, arch);
			Test1d(512, false, arch);
			Test1d(512, true, arch);
			Test1d(1024, false, arch);
			Test1d(1024, true, arch);
			Test1d(2048, false, arch);
			Test1d(2048, true, arch);

			Test1d(36, false, arch);
			Test1d(36, true, arch);
			Test1d(40, false, arch);
			Test1d(40, true, arch);
			Test1d(60, false, arch);
			Test1d(60, true, arch);
			Test1d(120, false, arch);
			Test1d(120, true, arch);
			Test1d(240, false, arch);
			Test1d(240, true, arch);
			Test1d(480, false, arch);
			Test1d(480, true, arch);
			Test1d(960, false, arch);
			Test1d(960, true, arch);
			Test1d(1920, false, arch);
			Test1d(1920, true, arch);
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Check(CPointer<kiss_fft_scalar> _in, CPointer<kiss_fft_scalar> _out, c_int nfft, bool isinverse)
		{
			c_double errpow = 0, sigpow = 0;

			for (c_int bin = 0; bin < (nfft / 2); ++bin)
			{
				c_double ansr = 0;

				for (c_int k = 0; k < nfft; ++k)
				{
					c_double phase = 2 * Math.PI * (k + 0.5f + 0.25f * nfft) * (bin + 0.5f) / nfft;
					c_double re = Math.Cos(phase);

					re /= nfft / 4;

					ansr += _in[k] * re;
				}

				c_double difr = ansr - _out[bin];
				errpow += difr * difr;
				sigpow += ansr * ansr;
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
		private void Check_Inv(CPointer<kiss_fft_scalar> _in, CPointer<kiss_fft_scalar> _out, c_int nfft, bool isinverse)
		{
			c_double errpow = 0, sigpow = 0;

			for (c_int bin = 0; bin < nfft; ++bin)
			{
				c_double ansr = 0;

				for (c_int k = 0; k < (nfft / 2); ++k)
				{
					c_double phase = 2 * Math.PI * (bin + 0.5f + 0.25f * nfft) * (k + 0.5f) / nfft;
					c_double re = Math.Cos(phase);

					ansr += _in[k] * re;
				}

				c_double difr = ansr - _out[bin];
				errpow += difr * difr;
				sigpow += ansr * ansr;
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

			c_int shift;
			if (nfft == 1920)
				shift = 0;
			else if (nfft == 960)
				shift = 1;
			else if (nfft == 480)
				shift = 2;
			else if (nfft == 240)
				shift = 3;
			else
				return;

			Mdct_Lookup cfg = mode.mdct;

			CPointer<kiss_fft_scalar> _in = CMemory.MAlloc<kiss_fft_scalar>((int)buflen);
			CPointer<kiss_fft_scalar> in_copy = CMemory.MAlloc<kiss_fft_scalar>((int)buflen);
			CPointer<kiss_fft_scalar> _out = CMemory.MAlloc<kiss_fft_scalar>((int)buflen);
			CPointer<opus_val16> window = CMemory.MAlloc<opus_val16>(nfft / 2);

			for (c_int k = 0; k < nfft; ++k)
				_in[k] = (RandomGenerator.GetRandomNumber() % 32768) - 16384;

			for (c_int k = 0; k < (nfft / 2); ++k)
				window[k] = Constants.Q15One;

			for (c_int k = 0; k < nfft; ++k)
				_in[k] *= 32768;

			if (isinverse)
			{
				for (c_int k = 0; k < nfft; ++k)
					_in[k] /= nfft;
			}

			for (c_int k = 0; k < nfft; ++k)
				in_copy[k] = _in[k];

			if (isinverse)
			{
				for (c_int k = 0; k < nfft; ++k)
					_out[k] = 0;

				Mdct.Clt_Mdct_Backward(cfg, _in, _out, window, nfft / 2, shift, 1, arch);

				// Apply TDAC because clt_mdct_backward() no longer does that
				for (c_int k = 0; k < (nfft / 4); ++k)
					_out[nfft - k - 1] = _out[nfft / 2 + k];

				Check_Inv(_in, _out, nfft, isinverse);
			}
			else
			{
				Mdct.Clt_Mdct_Forward(cfg, _in, _out, window, nfft / 2, shift, 1, arch);
				Check(in_copy, _out, nfft, isinverse);
			}
		}
		#endregion
	}
}
