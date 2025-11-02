/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Ports.LibOpus;
using Polycode.NostalgicPlayer.Ports.LibOpus.Containers;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibOpus.Test
{
	/// <summary>
	/// </summary>
	[TestClass]
	public class Test_Opus_Decode : TestCommon
	{
		private const int Max_Packet = 1500;
		private const int Max_Frame_Samp = 5760;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_OpusDecode()
		{
			double time = (DateTime.Now - new DateTime(1970, 1, 1)).TotalSeconds;
			iseed = (opus_uint32)time ^ (((opus_uint32)Process.GetCurrentProcess().Id & 65535) << 16);
			Rw = Rz = iseed;

			Console.WriteLine(string.Format("Testing decoder. Random seed: {0} ({1:X4})", iseed, Fast_Rand() % 65535));

			Test_Decoder_Code0();
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Test_Decoder_Code0()
		{
			opus_int32[] fsv = [ 48000, 24000, 16000, 12000, 8000 ];
			OpusDecoder[] dec = new OpusDecoder[5 * 2];

			opus_uint32 dec_final_range1 = 2;
			opus_uint32 dec_final_range2 = 2;

			CPointer<byte> packet = CMemory.malloc<byte>(Max_Packet);

			CPointer<c_short> outbuf_int = new CPointer<c_short>((Max_Frame_Samp + 16) * 2);

			for (opus_int32 i = 0; i < (Max_Frame_Samp + 16) * 2; i++)
				outbuf_int[i] = 32749;

			CPointer<c_short> outbuf = outbuf_int + 8 * 2;

			Console.WriteLine($"Starting {5*2} decoders");

			for (c_int t = 0; t < 5 * 2; t++)
			{
				c_int fs = fsv[t >> 1];
				c_int c = (t & 1) + 1;

				dec[t] = OpusDecoder.Create(fs, c, out OpusError err);
				if ((err != OpusError.Ok) || (dec[t] == null))
					Test_Failed();

				Console.Write($"     opus_decoder_create({fs:D5},{c}) OK. Copy ");

				{
					OpusDecoder dec2 = dec[t].MakeDeepClone();
					dec[t].Destroy();
					Console.WriteLine("OK");

					dec[t] = dec2;
				}
			}

			for (c_int t = 0; t < 5 * 2; t++)
			{
				c_int factor = 48000 / fsv[t >> 1];

				for (c_int fec = 0; fec < 2; fec++)
				{
					// Test PLC on a fresh decoder
					c_int out_samples = dec[t].Decode(null, 0, outbuf, 120 / factor, fec != 0);
					Assert.AreEqual(120 / factor, out_samples);

					if (dec[t].Decoder_Ctl_Get(OpusControlGetRequest.Opus_Get_Last_Packet_Duration, out opus_int32 dur) != OpusError.Ok)
						Test_Failed();

					Assert.AreEqual(120 / factor, dur);

					// Test on a site which isn't a multiple of 2.5 ms
					out_samples = dec[t].Decode(null, 0, outbuf, 120 / factor + 2, fec != 0);
					Assert.AreEqual((c_int)OpusError.Bad_Arg, out_samples);

					// Test null pointer input
					out_samples = dec[t].Decode(null, -1, outbuf, 120 / factor, fec != 0);
					Assert.AreEqual(120 / factor, out_samples);

					out_samples = dec[t].Decode(null, 1, outbuf, 120 / factor, fec != 0);
					Assert.AreEqual(120 / factor, out_samples);

					out_samples = dec[t].Decode(null, 10, outbuf, 120 / factor, fec != 0);
					Assert.AreEqual(120 / factor, out_samples);

					out_samples = dec[t].Decode(null, RandomGenerator.GetRandomNumber(), outbuf, 120 / factor, fec != 0);
					Assert.AreEqual(120 / factor, out_samples);

					if (dec[t].Decoder_Ctl_Get(OpusControlGetRequest.Opus_Get_Last_Packet_Duration, out dur) != OpusError.Ok)
						Test_Failed();

					Assert.AreEqual(120 / factor, dur);

					// Zero lengths
					out_samples = dec[t].Decode(packet, 0, outbuf, 120 / factor, fec != 0);
					Assert.AreEqual(120 / factor, out_samples);

					// Zero buffer
					outbuf[0] = 32749;

					out_samples = dec[t].Decode(packet, 0, outbuf, 0, fec != 0);
					if (out_samples > 0)
						Test_Failed();

					out_samples = dec[t].Decode(packet, 0, null, 0, fec != 0);
					if (out_samples > 0)
						Test_Failed();

					Assert.AreEqual(32749, outbuf[0]);

					// Invalid lengths
					out_samples = dec[t].Decode(packet, -1, outbuf, Max_Frame_Samp, fec != 0);
					if (out_samples >= 0)
						Test_Failed();

					out_samples = dec[t].Decode(packet, c_int.MinValue, outbuf, Max_Frame_Samp, fec != 0);
					if (out_samples >= 0)
						Test_Failed();

					out_samples = dec[t].Decode(packet, -1, outbuf, -1, fec != 0);
					if (out_samples >= 0)
						Test_Failed();

					// Reset the decoder
					if (dec[t].Decoder_Ctl_Set(OpusControlSetRequest.Opus_Reset_State) != OpusError.Ok)
						Test_Failed();
				}
			}

			Console.WriteLine("  dec[all] initial frame PLC OK");

			// Count code 0 tests
			for (opus_int32 i = 0; i < 64; i++)
			{
				c_int[] expected = new c_int[5 * 2];
				packet[0] = (byte)(i << 2);
				packet[1] = 255;
				packet[2] = 255;

				c_int err = OpusDecoder.Packet_Get_Nb_Channels(packet);
				if (err != ((i & 1) + 1))
					Test_Failed();

				for (c_int t = 0; t < 5 * 2; t++)
				{
					expected[t] = dec[t].Get_Nb_Samples(packet, 1);
					if (expected[t] > 2880)
						Test_Failed();
				}

				for (c_int j = 0; j < 256; j++)
				{
					packet[1] = (byte)j;

					for (c_int t = 0; t < 5 * 2; t++)
					{
						c_int out_samples = dec[t].Decode(packet, 3, outbuf, Max_Frame_Samp, false);
						Assert.AreEqual(expected[t], out_samples);

						if (dec[t].Decoder_Ctl_Get(OpusControlGetRequest.Opus_Get_Last_Packet_Duration, out opus_int32 dur) != OpusError.Ok)
							Test_Failed();

						Assert.AreEqual(out_samples, dur);

						dec[t].Decoder_Ctl_Get(OpusControlGetRequest.Opus_Get_Final_Range, out dec_final_range1);

						if (t == 0)
							dec_final_range2 = dec_final_range1;
						else if (dec_final_range1 != dec_final_range2)
							Test_Failed();
					}
				}

				for (c_int t = 0; t < 5 * 2; t++)
				{
					c_int out_samples;
					c_int factor = 48000 / fsv[t >> 1];

					// The PLC is run for 6 frames in order to get better PLC coverage
					for (c_int j = 0; j < 6; j++)
					{
						out_samples = dec[t].Decode(null, 0, outbuf, expected[t], false);
						Assert.AreEqual(expected[t], out_samples);

						if (dec[t].Decoder_Ctl_Get(OpusControlGetRequest.Opus_Get_Last_Packet_Duration, out opus_int32 dur) != OpusError.Ok)
							Test_Failed();

						Assert.AreEqual(out_samples, dur);
					}

					// Run the PLC once at 2.5 ms, asd a simulation of someone trying to do small drift corrections
					if (expected[t] != (120 / factor))
					{
						out_samples = dec[t].Decode(null, 0, outbuf, 120 / factor, false);
						Assert.AreEqual(120 / factor, out_samples);

						if (dec[t].Decoder_Ctl_Get(OpusControlGetRequest.Opus_Get_Last_Packet_Duration, out opus_int32 dur) != OpusError.Ok)
							Test_Failed();

						Assert.AreEqual(out_samples, dur);
					}

					out_samples = dec[t].Decode(packet, 2, outbuf, expected[t] - 1, false);
					if (out_samples > 0)
						Test_Failed();
				}
			}

			Console.WriteLine("  dec[all] all 2-byte prefix for length 3 and PLC, all modes (64) OK");

			for (c_int t = 0; t < 5 * 2; t++)
				dec[t].Destroy();

			Console.WriteLine("  Decoders stopped");

			bool err1 = false;

			for (opus_int i = 0; i < 8 * 2; i++)
				err1 |= outbuf_int[i] != 32749;

			Assert.IsFalse(err1);

			CMemory.free(outbuf_int);
			CMemory.free(packet);
		}
		#endregion
	}
}
