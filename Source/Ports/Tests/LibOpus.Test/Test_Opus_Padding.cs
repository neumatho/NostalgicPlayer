/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Ports.LibOpus;
using Polycode.NostalgicPlayer.Ports.LibOpus.Containers;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibOpus.Test
{
	/// <summary>
	/// </summary>
	[TestClass]
	public class Test_Opus_Padding : TestCommon
	{
		private const int PacketSize = 16909318;
		private const int Channels = 2;
		private const int FrameSize = 5760;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_OpusPadding()
		{
			Console.WriteLine("Testing padding");

			Test_Overflow();

			Console.WriteLine("All padding tests passed");
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Test_Overflow()
		{
			Pointer<byte> _in = CMemory.MAlloc<byte>(PacketSize);
			Pointer<opus_int16> _out = CMemory.MAlloc<opus_int16>(FrameSize * Channels);

			Console.Write("  Checking for padding overflow...");

			if (_in.IsNull || _out.IsNull)
			{
				Console.WriteLine("FAIL (out of memory)");
				Test_Failed();
			}

			_in[0] = 0xff;
			_in[1] = 0x41;
			CMemory.MemSet<byte>(_in + 2, 0xff, PacketSize - 3);
			_in[PacketSize - 1] = 0x0b;

			OpusDecoder decoder = OpusDecoder.Create(48000, Channels, out _);
			c_int result = decoder.Decode(_in, PacketSize, _out, FrameSize, false);
			decoder.Destroy();

			CMemory.Free(_in);
			CMemory.Free(_out);

			if ((OpusError)result != OpusError.Invalid_Packet)
			{
				Console.WriteLine("FAIL!");
				Test_Failed();
			}

			Console.WriteLine("OK");
		}
		#endregion
	}
}
