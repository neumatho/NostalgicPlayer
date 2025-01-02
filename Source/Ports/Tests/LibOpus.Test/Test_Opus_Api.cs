/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.CKit;
using Polycode.NostalgicPlayer.Ports.LibOpus;
using Polycode.NostalgicPlayer.Ports.LibOpus.Containers;
using Polycode.NostalgicPlayer.Ports.LibOpus.Internal;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibOpus.Test
{
	/// <summary>
	/// </summary>
	[TestClass]
	public class Test_Opus_Api : TestCommon
	{
		private static readonly c_int[] opus_rates = [ 48000, 24000, 16000, 12000, 8000 ];

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Dec_Api()
		{
			OpusDecoder dec;
			opus_int32 i;
			byte[] packet = new byte[1276];
			c_float[] fbuf = new c_float[960 * 2];
			c_short[] sbuf = new c_short[960 * 2];
			OpusError err;

			// First test invalid configurations which should fail
			Console.WriteLine("  Decoder basic API tests");
			Console.WriteLine("  ---------------------------------------------------");

			// Test with unsupported sample rates
			for (c_int c = 0; c < 4; c++)
			{
				for (i = -7; i <= 96000; i++)
				{
					c_int fs;

					if (((i == 8000) || (i == 12000) || (i == 16000) || (i == 24000) || (i == 48000)) && ((c == 1) || (c == 2)))
						continue;

					switch (i)
					{
						case -5:
						{
							fs = -8000;
							break;
						}

						case -6:
						{
							fs = Int32.MaxValue;
							break;
						}

						case -7:
						{
							fs = Int32.MinValue;
							break;
						}

						default:
						{
							fs = i;
							break;
						}
					}

					dec = OpusDecoder.Create(fs, c, out err);
					if ((err != OpusError.Bad_Arg) || (dec != null))
						Test_Failed();
				}
			}

			dec = OpusDecoder.Create(48000, 2, out err);
			if ((err != OpusError.Ok) || (dec == null))
				Test_Failed();

			Console.WriteLine("    opus_decoder_create() ........................ OK");

			err = dec.Decoder_Ctl_Get(OpusControlGetRequest.Opus_Get_Final_Range, out opus_uint32 dec_final_range);
			if (err != OpusError.Ok)
				Test_Failed();

			Console.WriteLine("    OPUS_GET_FINAL_RANGE ......................... OK");

			err = dec.Decoder_Ctl_Get((OpusControlGetRequest)OpusError.Unimplemented, out opus_uint32 _);
			if (err != OpusError.Unimplemented)
				Test_Failed();

			Console.WriteLine("    OPUS_UNIMPLEMENTED ........................... OK");

			err = dec.Decoder_Ctl_Get(OpusControlGetRequest.Opus_Get_Bandwidth, out i);
			if ((err != OpusError.Ok) || (i != 0))
				Test_Failed();

			Console.WriteLine("    OPUS_GET_BANDWIDTH ........................... OK");

			err = dec.Decoder_Ctl_Get(OpusControlGetRequest.Opus_Get_Sample_Rate, out i);
			if ((err != OpusError.Ok) || (i != 48000))
				Test_Failed();

			Console.WriteLine("    OPUS_GET_SAMPLE_RATE ......................... OK");

			// GET_PITCH has different execution paths depending on the previously decoded frame
			err = dec.Decoder_Ctl_Get(OpusControlGetRequest.Opus_Get_Pitch, out i);
			if ((err != OpusError.Ok) || (i > 0) || (i < -1))
				Test_Failed();

			packet[0] = 63 << 2;
			packet[1] = packet[2] = 0;

			if (dec.Decode(packet, 3, sbuf, 960, false) != 960)
				Test_Failed();

			err = dec.Decoder_Ctl_Get(OpusControlGetRequest.Opus_Get_Pitch, out i);
			if ((err != OpusError.Ok) || (i > 0) || (i < -1))
				Test_Failed();

			packet[0] = 1;

			if (dec.Decode(packet, 1, sbuf, 960, false) != 960)
				Test_Failed();

			err = dec.Decoder_Ctl_Get(OpusControlGetRequest.Opus_Get_Pitch, out i);
			if ((err != OpusError.Ok) || (i > 0) || (i < -1))
				Test_Failed();

			Console.WriteLine("    OPUS_GET_PITCH ............................... OK");

			err = dec.Decoder_Ctl_Get(OpusControlGetRequest.Opus_Get_Last_Packet_Duration, out i);
			if ((err != OpusError.Ok) || (i != 960))
				Test_Failed();

			Console.WriteLine("    OPUS_GET_LAST_PACKET_DURATION ................ OK");

			err = dec.Decoder_Ctl_Get(OpusControlGetRequest.Opus_Get_Gain, out i);
			if ((err != OpusError.Ok) || (i != 0))
				Test_Failed();

			err = dec.Decoder_Ctl_Set(OpusControlSetRequest.Opus_Set_Gain, -32769);
			if (err != OpusError.Bad_Arg)
				Test_Failed();

			err = dec.Decoder_Ctl_Set(OpusControlSetRequest.Opus_Set_Gain, 32768);
			if (err != OpusError.Bad_Arg)
				Test_Failed();

			err = dec.Decoder_Ctl_Set(OpusControlSetRequest.Opus_Set_Gain, -15);
			if (err != OpusError.Ok)
				Test_Failed();

			err = dec.Decoder_Ctl_Get(OpusControlGetRequest.Opus_Get_Gain, out i);
			if ((err != OpusError.Ok) || (i != -15))
				Test_Failed();

			Console.WriteLine("    OPUS_SET_GAIN ................................ OK");
			Console.WriteLine("    OPUS_GET_GAIN ................................ OK");

			// Reset the decoder
			OpusDecoder dec2 = dec.MakeDeepClone();

			err = dec.Decoder_Ctl_Set(OpusControlSetRequest.Opus_Reset_State);
			if (err != OpusError.Ok)
				Test_Failed();

			Console.WriteLine("    OPUS_RESET_STATE ............................. OK");

			packet[0] = 0;

			if (dec.Get_Nb_Samples(packet, 1) != 480)
				Test_Failed();

			if (OpusDecoder.Packet_Get_Nb_Samples(packet, 1, 48000) != 480)
				Test_Failed();

			if (OpusDecoder.Packet_Get_Nb_Samples(packet, 1, 96000) != 960)
				Test_Failed();

			if (OpusDecoder.Packet_Get_Nb_Samples(packet, 1, 32000) != 320)
				Test_Failed();

			if (OpusDecoder.Packet_Get_Nb_Samples(packet, 1, 8000) != 80)
				Test_Failed();

			packet[0] = 3;

			if ((OpusError)OpusDecoder.Packet_Get_Nb_Samples(packet, 1, 24000) != OpusError.Invalid_Packet)
				Test_Failed();

			packet[0] = (63 << 2) | 3;
			packet[1] = 63;

			if ((OpusError)OpusDecoder.Packet_Get_Nb_Samples(packet, 0, 24000) != OpusError.Bad_Arg)
				Test_Failed();

			if ((OpusError)OpusDecoder.Packet_Get_Nb_Samples(packet, 2, 48000) != OpusError.Invalid_Packet)
				Test_Failed();

			if ((OpusError)dec.Get_Nb_Samples(packet, 2) != OpusError.Invalid_Packet)
				Test_Failed();

			Console.WriteLine("    opus_{packet,decoder}_get_nb_samples() ....... OK");

			if ((OpusError)OpusDecoder.Packet_Get_Nb_Frames(packet, 0) != OpusError.Bad_Arg)
				Test_Failed();

			for (i = 0; i < 256; i++)
			{
				c_int[] l1res = [ 1, 2, 2, (c_int)OpusError.Invalid_Packet ];
				packet[0] = (byte)i;

				if (l1res[packet[0] & 3] != OpusDecoder.Packet_Get_Nb_Frames(packet, 1))
					Test_Failed();

				for (opus_int32 j = 0; j < 256; j++)
				{
					packet[1] = (byte)j;

					if (((packet[0] & 3) != 3 ? l1res[packet[0] & 3] : packet[1] & 63) != OpusDecoder.Packet_Get_Nb_Frames(packet, 2))
						Test_Failed();
				}
			}

			Console.WriteLine("    opus_packet_get_nb_frames() .................. OK");

			for (i = 0; i < 256; i++)
			{
				packet[0] = (byte)i;

				c_int bw = packet[0] >> 4;
				Bandwidth _bw = (Bandwidth)((c_int)Bandwidth.Narrowband + (((((bw & 7) * 9) & (63 - (bw & 8))) + 2 + 12 * ((bw & 8) != 0 ? 1 : 0)) >> 4));

				if (_bw != OpusDecoder.Packet_Get_Bandwidth(packet))
					Test_Failed();
			}

			Console.WriteLine("    opus_packet_get_bandwidth() .................. OK");

			for (i = 0; i < 256; i++)
			{
				packet[0] = (byte)i;

				c_int fp3s = packet[0] >> 3;
				fp3s = ((((3 - (fp3s & 3)) * 13 & 119) + 9) >> 2) * ((fp3s > 13 ? 1 : 0) * (3 - ((fp3s & 3) == 3 ? 1 : 0)) + 1) * 25;

				for (c_int rate = 0; rate < 5; rate++)
				{
					if ((opus_rates[rate] * 3 / fp3s) != OpusDecoder.Packet_Get_Samples_Per_Frame(packet, opus_rates[rate]))
						Test_Failed();
				}
			}

			Console.WriteLine("    opus_packet_get_samples_per_frame() .......... OK");

			packet[0] = (63 << 2) + 3;
			packet[1] = 49;

			for (opus_int32 j = 2; j < 51; j++)
				packet[j] = 0;

			if ((OpusError)dec.Decode(packet, 51, sbuf, 960, false) != OpusError.Invalid_Packet)
				Test_Failed();

			packet[0] = 63 << 2;
			packet[1] = packet[2] = 0;

			if ((OpusError)dec.Decode(packet, -1, sbuf, 960, false) != OpusError.Bad_Arg)
				Test_Failed();

			if ((OpusError)dec.Decode(packet, 3, sbuf, 60, false) != OpusError.Buffer_Too_Small)
				Test_Failed();

			if ((OpusError)dec.Decode(packet, 3, sbuf, 480, false) != OpusError.Buffer_Too_Small)
				Test_Failed();

			if (dec.Decode(packet, 3, sbuf, 960, false) != 960)
				Test_Failed();

			Console.WriteLine("    opus_decode() ................................ OK");

			if (dec.Decode_Float(packet, 3, fbuf, 960, false) != 960)
				Test_Failed();

			Console.WriteLine("    opus_decode_float() .......................... OK");

			dec.Destroy();

			Console.WriteLine("                   All decoder interface tests passed");
		}



		/********************************************************************/
		/// <summary>
		/// This test exercises the heck out of the libopus parser. It is
		/// much larger than the parser itself in part because it tries to
		/// hit a lot of corner cases that could never fail with the libopus
		/// code, but might be problematic for other implementations
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Parse()
		{
			CPointer<byte> packet = new CPointer<byte>(1276);
			byte toc;
			CPointer<byte>[] frames = new CPointer<byte>[48];
			c_short[] size = new c_short[48];
			c_int payload_offset, ret;

			void Undefine_For_Parse()
			{
				toc = 255;
				frames[0].SetToNull();
				frames[1].SetToNull();
				payload_offset = -1;
			}

			Console.WriteLine("  Packet header parsing tests");
			Console.WriteLine("  ---------------------------------------------------");

			packet[0] = 63 << 2;

			if ((OpusError)Opus.Opus_Packet_Parse(packet, 1, out toc, frames, null, out payload_offset) != OpusError.Bad_Arg)
				Test_Failed();

			// Code 0
			for (opus_int32 i = 0; i < 64; i++)
			{
				packet[0] = (byte)(i << 2);
				Undefine_For_Parse();

				ret = Opus.Opus_Packet_Parse(packet, 4, out toc, frames, size, out payload_offset);
				Assert.AreEqual(1, ret);
				Assert.AreEqual(3, size[0]);
				Assert.AreEqual(packet + 1, frames[0]);
			}

			Console.WriteLine("    code 0 ....................................... OK");

			// Code 1, two frames of the same size
			for (opus_int32 i = 0; i < 64; i++)
			{
				packet[0] = (byte)((i << 2) + 1);

				for (opus_int32 jj = 0; jj <= 1275 * 2 + 3; jj++)
				{
					Undefine_For_Parse();

					ret = Opus.Opus_Packet_Parse(packet, jj, out toc, frames, size, out payload_offset);

					if (((jj & 1) == 1) && (jj <= 2551))
					{
						// Must pass if payload length even (packet length odd) and
						// size<=2551, must fail otherwise
						Assert.AreEqual(2, ret);
						Assert.AreEqual(size[0], size[1]);
						Assert.AreEqual((jj - 1) >> 1, size[0]);
						Assert.AreEqual(packet + 1, frames[0]);
						Assert.AreEqual(frames[0] + size[0], frames[1]);
						Assert.AreEqual(i, toc >> 2);
					}
					else
						Assert.AreEqual(OpusError.Invalid_Packet, (OpusError)ret);
				}
			}

			Console.WriteLine("    code 1 ....................................... OK");

			for (opus_int32 i = 0; i < 64; i++)
			{
				// Code 2, length code overflow
				packet[0] = (byte)((i << 2) + 2);
				Undefine_For_Parse();

				ret = Opus.Opus_Packet_Parse(packet, 1, out toc, frames, size, out payload_offset);
				Assert.AreEqual(OpusError.Invalid_Packet, (OpusError)ret);

				packet[1] = 252;
				Undefine_For_Parse();

				ret = Opus.Opus_Packet_Parse(packet, 2, out toc, frames, size, out payload_offset);
				Assert.AreEqual(OpusError.Invalid_Packet, (OpusError)ret);

				for (opus_int32 j = 0; j < 1275; j++)
				{
					if (j < 252)
						packet[1] = (byte)j;
					else
					{
						packet[1] = (byte)(252 + (j & 3));
						packet[2] = (byte)((j - 252) >> 2);
					}

					// Code 2, one too short
					Undefine_For_Parse();

					ret = Opus.Opus_Packet_Parse(packet, j + (j < 252 ? 2 : 3) - 1, out toc, frames, size, out payload_offset);
					Assert.AreEqual(OpusError.Invalid_Packet, (OpusError)ret);

					// Code 2, one too long
					Undefine_For_Parse();

					ret = Opus.Opus_Packet_Parse(packet, j + (j < 252 ? 2 : 3) + 1276, out toc, frames, size, out payload_offset);
					Assert.AreEqual(OpusError.Invalid_Packet, (OpusError)ret);

					// Code 2, second zero
					Undefine_For_Parse();

					ret = Opus.Opus_Packet_Parse(packet, j + (j < 252 ? 2 : 3), out toc, frames, size, out payload_offset);
					Assert.AreEqual(2, ret);
					Assert.AreEqual(j, size[0]);
					Assert.AreEqual(0, size[1]);
					Assert.AreEqual(frames[0] + size[0], frames[1]);
					Assert.AreEqual(i, toc >> 2);

					// Code 2, normal
					Undefine_For_Parse();

					ret = Opus.Opus_Packet_Parse(packet, (j << 1) + 4, out toc, frames, size, out payload_offset);
					Assert.AreEqual(2, ret);
					Assert.AreEqual(j, size[0]);
					Assert.AreEqual((j << 1) + 3 - j - (j < 252 ? 1 : 2), size[1]);
					Assert.AreEqual(frames[0] + size[0], frames[1]);
					Assert.AreEqual(i, toc >> 2);
				}
			}

			Console.WriteLine("    code 2 ....................................... OK");

			for (opus_int32 i = 0; i < 64; i++)
			{
				packet[0] = (byte)((i << 2) + 3);

				// Code 3, length code overflow
				Undefine_For_Parse();

				ret = Opus.Opus_Packet_Parse(packet, 1, out toc, frames, size, out payload_offset);
				Assert.AreEqual(OpusError.Invalid_Packet, (OpusError)ret);
			}

			Console.WriteLine("    code 3 m-truncation .......................... OK");

			for (opus_int32 i = 0; i < 64; i++)
			{
				// Code 3, m is zero or 49-63
				packet[0] = (byte)((i << 2) + 3);

				for (opus_int32 jj = 49; jj <= 64; jj++)
				{
					packet[1] = (byte)(0 + (jj & 63));		// CBR, no padding
					Undefine_For_Parse();

					ret = Opus.Opus_Packet_Parse(packet, 1275, out toc, frames, size, out payload_offset);
					Assert.AreEqual(OpusError.Invalid_Packet, (OpusError)ret);

					packet[1] = (byte)(128 + (jj & 63));	// VBR, no padding
					Undefine_For_Parse();

					ret = Opus.Opus_Packet_Parse(packet, 1275, out toc, frames, size, out payload_offset);
					Assert.AreEqual(OpusError.Invalid_Packet, (OpusError)ret);

					packet[1] = (byte)(64 + (jj & 63));		// CBR, padding
					Undefine_For_Parse();

					ret = Opus.Opus_Packet_Parse(packet, 1275, out toc, frames, size, out payload_offset);
					Assert.AreEqual(OpusError.Invalid_Packet, (OpusError)ret);

					packet[1] = (byte)(128 + 64 + (jj & 63));// VBR, padding
					Undefine_For_Parse();

					ret = Opus.Opus_Packet_Parse(packet, 1275, out toc, frames, size, out payload_offset);
					Assert.AreEqual(OpusError.Invalid_Packet, (OpusError)ret);
				}
			}

			Console.WriteLine("    code 3 m=0,49-64 ............................. OK");

			for (opus_int32 i = 0; i < 64; i++)
			{
				packet[0] = (byte)((i << 2) + 3);

				// Code 3, m is one, cbr
				packet[1] = 1;

				for (opus_int32 j = 0; j < 1276; j++)
				{
					Undefine_For_Parse();

					ret = Opus.Opus_Packet_Parse(packet, j + 2, out toc, frames, size, out payload_offset);
					Assert.AreEqual(1, ret);
					Assert.AreEqual(j, size[0]);
					Assert.AreEqual(i, toc >> 2);
				}

				Undefine_For_Parse();

				ret = Opus.Opus_Packet_Parse(packet, 1276 + 2, out toc, frames, size, out payload_offset);
				Assert.AreEqual(OpusError.Invalid_Packet, (OpusError)ret);
			}

			Console.WriteLine("    code 3 m=1 CBR ............................... OK");

			for (opus_int32 i = 0; i < 64; i++)
			{
				// Code 3, m>1 CBR
				packet[0] = (byte)((i << 2) + 3);
				c_int frame_samp = OpusDecoder.Packet_Get_Samples_Per_Frame(packet, 48000);

				for (opus_int32 j = 2; j < 49; j++)
				{
					packet[1] = (byte)j;

					for (opus_int32 sz = 2; sz < ((j + 2) * 1275); sz++)
					{
						Undefine_For_Parse();

						ret = Opus.Opus_Packet_Parse(packet, sz, out toc, frames, size, out payload_offset);

						// Must be <=120 ms, must be evenly divisible, can't have frames>1275 bytes
						if (((frame_samp * j) <= 5760) && (((sz - 2) % j) == 0) && (((sz -2) / j) < 1276))
						{
							Assert.AreEqual(j, ret);

							for (opus_int32 jj = 1; jj < ret; jj++)
								Assert.AreEqual(frames[jj - 1] + size[jj - 1], frames[jj]);

							Assert.AreEqual(i, toc >> 2);
						}
						else
							Assert.AreEqual(OpusError.Invalid_Packet, (OpusError)ret);
					}
				}

				// Super jumbo packets
				packet[1] = (byte)(5760 / frame_samp);
				Undefine_For_Parse();

				ret = Opus.Opus_Packet_Parse(packet, 1275 * packet[1] + 2, out toc, frames, size, out payload_offset);
				Assert.AreEqual(packet[1], ret);

				for (opus_int32 jj = 0; jj < ret; jj++)
					Assert.AreEqual(1275, size[jj]);
			}

			Console.WriteLine("    code 3 m=1-48 CBR ............................ OK");

			for (opus_int32 i = 0; i < 64; i++)
			{
				// Code 3 VBR, m one
				packet[0] = (byte)((i << 2) + 3);
				packet[1] = 128 + 1;
				c_int frame_samp = OpusDecoder.Packet_Get_Samples_Per_Frame(packet, 48000);

				for (opus_int32 jj = 0; jj < 1276; jj++)
				{
					Undefine_For_Parse();

					ret = Opus.Opus_Packet_Parse(packet, 2 + jj, out toc, frames, size, out payload_offset);
					Assert.AreEqual(1, ret);
					Assert.AreEqual(jj, size[0]);
					Assert.AreEqual(i, toc >> 2);
				}

				Undefine_For_Parse();

				ret = Opus.Opus_Packet_Parse(packet, 2 + 1276, out toc, frames, size, out payload_offset);
				Assert.AreEqual(OpusError.Invalid_Packet, (OpusError)ret);

				for (opus_int32 j = 2; j < 49; j++)
				{
					packet[1] = (byte)(128 + j);

					// Length code overflow
					Undefine_For_Parse();

					ret = Opus.Opus_Packet_Parse(packet, 2 + j - 2, out toc, frames, size, out payload_offset);
					Assert.AreEqual(OpusError.Invalid_Packet, (OpusError)ret);

					packet[2] = 252;
					packet[3] = 0;

					for (opus_int32 jj = 4; jj < (2 + j); jj++)
						packet[jj] = 0;

					Undefine_For_Parse();

					ret = Opus.Opus_Packet_Parse(packet, 2 + j, out toc, frames, size, out payload_offset);
					Assert.AreEqual(OpusError.Invalid_Packet, (OpusError)ret);

					// One byte too short
					for (opus_int32 jj = 2; jj < (2 + j); jj++)
						packet[jj] = 0;

					Undefine_For_Parse();

					ret = Opus.Opus_Packet_Parse(packet, 2 + j - 2, out toc, frames, size, out payload_offset);
					Assert.AreEqual(OpusError.Invalid_Packet, (OpusError)ret);

					// One byte too short thanks to length coding
					packet[2] = 252;
					packet[3] = 0;

					for (opus_int32 jj = 4; jj < (2 + j); jj++)
						packet[jj] = 0;

					Undefine_For_Parse();

					ret = Opus.Opus_Packet_Parse(packet, 2 + j + 252 - 1, out toc, frames, size, out payload_offset);
					Assert.AreEqual(OpusError.Invalid_Packet, (OpusError)ret);

					// Most expensive way of coding zeros
					for (opus_int32 jj = 2; jj < (2 + j); jj++)
						packet[jj] = 0;

					Undefine_For_Parse();

					ret = Opus.Opus_Packet_Parse(packet, 2 + j - 1, out toc, frames, size, out payload_offset);

					if ((frame_samp * j) <= 5760)
					{
						Assert.AreEqual(j, ret);

						for (opus_int32 jj = 0; jj < j; jj++)
							Assert.AreEqual(0, size[jj]);

						Assert.AreEqual(i, toc >> 2);
					}
					else
						Assert.AreEqual(OpusError.Invalid_Packet, (OpusError)ret);

					// Quasi-CBR use of mode 3
					for (opus_int32 sz = 0; sz < 8; sz++)
					{
						c_int[] tsz = [ 50, 201, 403, 700, 1472, 5110, 20400, 61298 ];
						c_int pos = 0;
						c_int _as = (tsz[sz] + i - j - 2) / j;

						for (opus_int32 jj = 0; jj < (j - 1); jj++)
						{
							if (_as < 252)
							{
								packet[2 + pos] = (byte)_as;
								pos++;
							}
							else
							{
								packet[2 + pos] = (byte)(252 + (_as & 3));
								packet[3 + pos] = (byte)((_as - 252) >> 2);
								pos += 2;
							}
						}

						Undefine_For_Parse();

						ret = Opus.Opus_Packet_Parse(packet, tsz[sz] + i, out toc, frames, size, out payload_offset);

						if (((frame_samp * j) <= 5760) && (_as < 1276) && ((tsz[sz] + i - 2 - pos - _as * (j - 1)) < 1276))
						{
							Assert.AreEqual(j, ret);

							for (opus_int32 jj = 0; jj < (j - 1); jj++)
								Assert.AreEqual(_as, size[jj]);

							Assert.AreEqual(tsz[sz] + i - 2 - pos - _as * (j - 1), size[j - 1]);
							Assert.AreEqual(i, toc >> 2);
						}
						else
							Assert.AreEqual(OpusError.Invalid_Packet, (OpusError)ret);
					}
				}
			}

			Console.WriteLine("    code 3 m=1-48 VBR ............................ OK");

			for (opus_int32 i = 0; i < 64; i++)
			{
				packet[0] = (byte)((i << 2) + 3);

				// Padding
				packet[1] = 128 + 1 + 64;

				// Overflow the length coding
				for (opus_int32 jj = 2; jj < 127; jj++)
					packet[jj] = 255;

				Undefine_For_Parse();

				ret = Opus.Opus_Packet_Parse(packet, 127, out toc, frames, size, out payload_offset);
				Assert.AreEqual(OpusError.Invalid_Packet, (OpusError)ret);

				for (opus_int32 sz = 0; sz < 4; sz++)
				{
					c_int[] tsz = [ 0, 72, 512, 1275 ];

					for (opus_int32 jj = sz; jj < 65025; jj += 11)
					{
						c_int pos;

						for (pos = 0; pos < (jj / 254); pos++)
							packet[2 + pos] = 255;

						packet[2 + pos] = (byte)(jj % 254);
						pos++;

						if ((sz == 0) && (i == 63))
						{
							// Code more padding than there is room in the packet
							Undefine_For_Parse();

							ret = Opus.Opus_Packet_Parse(packet, 2 + jj + pos - 1, out toc, frames, size, out payload_offset);
							Assert.AreEqual(OpusError.Invalid_Packet, (OpusError)ret);
						}

						Undefine_For_Parse();

						ret = Opus.Opus_Packet_Parse(packet, 2 + jj + tsz[sz] + i + pos, out toc, frames, size, out payload_offset);

						if ((tsz[sz] + i) < 1276)
						{
							Assert.AreEqual(1, ret);
							Assert.AreEqual(tsz[sz] + i, size[0]);
							Assert.AreEqual(i, toc >> 2);
						}
						else
							Assert.AreEqual(OpusError.Invalid_Packet, (OpusError)ret);
					}
				}
			}

			Console.WriteLine("    code 3 padding ............................... OK");
			Console.WriteLine("    opus_packet_parse ............................ OK");
			Console.WriteLine("                      All packet parsing tests passed");
		}
	}
}
