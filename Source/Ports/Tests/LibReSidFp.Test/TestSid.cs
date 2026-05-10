/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Ports.LibReSidFp;
using Polycode.NostalgicPlayer.Ports.LibReSidFp.Containers;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibReSidFp.Test
{
	/// <summary>
	/// 
	/// </summary>
	[TestClass]
	public class TestSid
	{
		private const int Buf_Size = 481;
		private const int Cycles = 10000;
		private const short Canary = 0x7fff;

		private readonly ReSidFp s = new ReSidFp();

		private readonly short[] buf = new short[Buf_Size + 1];

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public TestSid()
		{
			// Test setup
			Array.Fill(buf, Canary);
			s.SetSamplingParameters(1000000, SamplingMethod.DECIMATE, 48000.0);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void TestCycles()
		{
			int c = s.Clock(buf, 0, Buf_Size);

			Assert.AreEqual(Cycles, c);
			Assert.AreNotEqual(Canary, buf[Buf_Size - 1]);
			Assert.AreEqual(Canary, buf[Buf_Size]);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void TestBufSize()
		{
			int b = s.Clock(Cycles, buf, 0);

			Assert.AreEqual(Buf_Size, b);
			Assert.AreNotEqual(Canary, buf[Buf_Size - 1]);
			Assert.AreEqual(Canary, buf[Buf_Size]);
		}
	}
}
