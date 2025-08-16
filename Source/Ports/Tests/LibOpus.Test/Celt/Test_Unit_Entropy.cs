/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Ports.LibOpus.Internal.Celt;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibOpus.Test.Celt
{
	/// <summary>
	/// </summary>
	[TestClass]
	public class Test_Unit_Entropy
	{
		private const c_double M_Log2E = 1.4426950408889634074;

		private const int Data_Size = 10000000;
		private const int Data_Size2 = 10000;

		private const int Rand_Max = int.MaxValue;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Entropy()
		{
			c_long nbits;
			c_long nbits2;
			c_double entropy = 0;

			// Testing encoding of raw bit values
			CPointer<byte> ptr = CMemory.MAlloc<byte>(Data_Size);

			EntEnc.Ec_Enc_Init(out Ec_Enc enc, ptr, Data_Size);

			for (c_int ft = 2; ft < 1024; ft++)
			{
				for (c_int i = 0; i < ft; i++)
				{
					entropy += Math.Log(ft) * M_Log2E;
					EntEnc.Ec_Enc_UInt(enc, (opus_uint32)i, (opus_uint32)ft);
				}
			}

			// Testing encoding of raw bit values
			for (c_int ftb = 1; ftb < 16; ftb++)
			{
				for (c_int i = 0; i < (1 << ftb); i++)
				{
					entropy += ftb;
					nbits = EntCode.Ec_Tell(enc);
					EntEnc.Ec_Enc_Bits(enc, (opus_uint32)i, (c_uint)ftb);
					nbits2 = EntCode.Ec_Tell(enc);

					if ((nbits2 - nbits) != ftb)
						Assert.Fail($"Used {nbits2 - nbits} bits to encode {ftb} bits directly");
				}
			}

			nbits = (c_int)EntCode.Ec_Tell_Frac(enc);
			EntEnc.Ec_Enc_Done(enc);
			Console.WriteLine(string.Format("Encoded {0:F2} bits of entropy to {1:F2} bits ({2:F3}% wasted)", entropy, CMath.ldexp(nbits, -3), 100 * (nbits - CMath.ldexp(entropy, 3)) / nbits));
			Console.WriteLine(string.Format("Packed to {0} bytes", EntCode.Ec_Range_Bytes(enc)));

			EntDec.Ec_Dec_Init(out Ec_Dec dec, ptr, Data_Size);

			for (c_int ft = 2; ft < 1024; ft++)
			{
				for (c_int i = 0; i < ft; i++)
				{
					c_uint sym = EntDec.Ec_Dec_UInt(dec, (opus_uint32)ft);

					if (sym != i)
						Assert.Fail($"Decoded {sym} instead of {i} with ft of {ft}");
				}
			}

			for (c_int ftb = 1; ftb < 16; ftb++)
			{
				for (c_int i = 0; i < (1 << ftb); i++)
				{
					c_uint sym = EntDec.Ec_Dec_Bits(dec, (c_uint)ftb);

					if (sym != i)
						Assert.Fail($"Decoded {sym} instead of {i} with ftb of {ftb}");
				}
			}

			nbits2 = (c_int)EntCode.Ec_Tell_Frac(dec);

			if (nbits != nbits2)
				Assert.Fail(string.Format("Reported number of bits used was {0:F2}, should be {1:F2}", CMath.ldexp(nbits2, -3), CMath.ldexp(nbits, -3)));

			// Testing an encoder bust prefers range coder data over raw bits.
			// This isn't a general guarantee, will only work for data that is buffered in
			// the encoder state and not yet stored in the user buffer, and should never
			// get used in practice.
			// It's mostly here for code coverage completeness

			// Start with a 16-bit buffer
			EntEnc.Ec_Enc_Init(out enc, ptr, 2);

			// Write 7 raw bits
			EntEnc.Ec_Enc_Bits(enc, 0x55, 7);

			// Write 12.3 bits of range coder data
			EntEnc.Ec_Enc_UInt(enc, 1, 2);
			EntEnc.Ec_Enc_UInt(enc, 1, 3);
			EntEnc.Ec_Enc_UInt(enc, 1, 4);
			EntEnc.Ec_Enc_UInt(enc, 1, 5);
			EntEnc.Ec_Enc_UInt(enc, 2, 6);
			EntEnc.Ec_Enc_UInt(enc, 6, 7);
			EntEnc.Ec_Enc_Done(enc);

			EntDec.Ec_Dec_Init(out dec, ptr, 2);

			if (!enc.error
				// The raw bits should have been overwritten by the range coder data
				|| (EntDec.Ec_Dec_Bits(dec, 7) != 0x05)
				// And all the range coder data should have been encoded correctly
				|| (EntDec.Ec_Dec_UInt(dec, 2) != 1)
				|| (EntDec.Ec_Dec_UInt(dec, 3) != 1)
				|| (EntDec.Ec_Dec_UInt(dec, 4) != 1)
				|| (EntDec.Ec_Dec_UInt(dec, 5) != 1)
				|| (EntDec.Ec_Dec_UInt(dec, 6) != 2)
				|| (EntDec.Ec_Dec_UInt(dec, 7) != 6))
			{
				Assert.Fail("Encoder bust overwrote range coder data with raw bits");
			}

			Console.WriteLine("Testing random streams...");

			for (c_int i = 0; i < 409600; i++)
			{
				c_int ft = RandomGenerator.GetRandomNumber() / ((Rand_Max >> (RandomGenerator.GetRandomNumber() % 11)) + 1) + 10;
				c_int sz = RandomGenerator.GetRandomNumber() / ((Rand_Max >> (RandomGenerator.GetRandomNumber() % 9)) + 1);

				CPointer<c_uint> data = CMemory.MAlloc<c_uint>(sz);
				CPointer<c_uint> tell = CMemory.MAlloc<c_uint>(sz + 1);

				EntEnc.Ec_Enc_Init(out enc, ptr, Data_Size2);

				bool zeros = RandomGenerator.GetRandomNumber() % 13 == 0;
				tell[0] = EntCode.Ec_Tell_Frac(enc);

				for (c_int j = 0; j < sz; j++)
				{
					if (zeros)
						data[j] = 0;
					else
						data[j] = (c_uint)(RandomGenerator.GetRandomNumber() % ft);

					EntEnc.Ec_Enc_UInt(enc, data[j], (opus_uint32)ft);
					tell[j + 1] = EntCode.Ec_Tell_Frac(enc);
				}

				if ((RandomGenerator.GetRandomNumber() % 2) == 0)
				{
					while ((EntCode.Ec_Tell(enc) % 8) != 0)
						EntEnc.Ec_Enc_UInt(enc, (opus_uint32)(RandomGenerator.GetRandomNumber() % 2), 2);
				}

				c_uint tell_bits = (c_uint)EntCode.Ec_Tell(enc);
				EntEnc.Ec_Enc_Done(enc);

				if (tell_bits != EntCode.Ec_Tell(enc))
					Assert.Fail(string.Format("ec_tell() changed after ec_enc_done(): {0} instead of {1}", EntCode.Ec_Tell(enc), tell_bits));

				if (((tell_bits + 7) / 8) < EntCode.Ec_Range_Bytes(enc))
					Assert.Fail(string.Format("ec_tell() lied, there's {0} bytes instead of {1}", EntCode.Ec_Range_Bytes(enc), (tell_bits + 7) / 8));

				EntDec.Ec_Dec_Init(out dec, ptr, Data_Size2);

				if (EntCode.Ec_Tell_Frac(dec) != tell[0])
					Assert.Fail(string.Format("Tell mismatch between encoder and decoder at symbol {0}: {1} instead of {2}", 0, EntCode.Ec_Tell_Frac(dec), tell[0]));

				for (c_int j = 0; j < sz; j++)
				{
					c_uint sym = EntDec.Ec_Dec_UInt(dec, (opus_uint32)ft);

					if (sym != data[j])
						Assert.Fail($"Decoded {sym} instead of {data[j]} with ft of {ft} of at position {j} of {sz}");

					if (EntCode.Ec_Tell_Frac(dec) != tell[j + 1])
						Assert.Fail(string.Format("Tell mismatch between encoder and decoder at symbol {0}: {1} instead of {2}", j + 1, EntCode.Ec_Tell_Frac(dec), tell[j + 1]));
				}
			}

			// Test compatibility between multiple different encode/decode routines
			for (c_int i = 0; i < 409600; i++)
			{
				c_int sz = RandomGenerator.GetRandomNumber() / ((Rand_Max >> (RandomGenerator.GetRandomNumber() % 9)) + 1);

				CPointer<c_uint> logp1 = CMemory.MAlloc<c_uint>(sz);
				CPointer<c_uint> data = CMemory.MAlloc<c_uint>(sz);
				CPointer<c_uint> tell = CMemory.MAlloc<c_uint>(sz + 1);
				CPointer<c_uint> enc_method = CMemory.MAlloc<c_uint>(sz);

				EntEnc.Ec_Enc_Init(out enc, ptr, Data_Size2);

				tell[0] = EntCode.Ec_Tell_Frac(enc);

				for (c_int j = 0; j < sz; j++)
				{
					data[j] = (c_uint)RandomGenerator.GetRandomNumber() / ((Rand_Max >> 1) + 1);
					logp1[j] = (c_uint)((RandomGenerator.GetRandomNumber() % 15) + 1);
					enc_method[j] = (c_uint)RandomGenerator.GetRandomNumber() / ((Rand_Max >> 2) + 1);

					switch (enc_method[j])
					{
						case 0:
						{
							EntEnc.Ec_Encode(enc, (c_uint)(data[j] != 0 ? (1 << (int)logp1[j]) - 1 : 0), (c_uint)((1 << (int)logp1[j]) - (data[j] != 0 ? 0 : 1)), 1U << (int)logp1[j]);
							break;
						}

						case 1:
						{
							EntEnc.Ec_Encode_Bin(enc, (c_uint)(data[j] != 0 ? (1 << (int)logp1[j]) - 1 : 0), (c_uint)((1 << (int)logp1[j]) - (data[j] != 0 ? 0 : 1)), logp1[j]);
							break;
						}

						case 2:
						{
							EntEnc.Ec_Enc_Bit_Logp(enc, data[j] != 0, logp1[j]);
							break;
						}

						case 3:
						{
							byte[] icdf = new byte[2];
							icdf[0] = 1;
							icdf[1] = 0;

							EntEnc.Ec_Enc_Icdf(enc, (c_int)data[j], icdf, logp1[j]);
							break;
						}
					}

					tell[j + 1] = EntCode.Ec_Tell_Frac(enc);
				}

				EntEnc.Ec_Enc_Done(enc);

				if (((EntCode.Ec_Tell(enc) + 7) / 8) < EntCode.Ec_Range_Bytes(enc))
					Assert.Fail(string.Format("tell() lied, there's {0} bytes instead of {1}", EntCode.Ec_Range_Bytes(enc), (EntCode.Ec_Tell(enc) + 7) / 8));

				EntDec.Ec_Dec_Init(out dec, ptr, Data_Size2);

				if (EntCode.Ec_Tell_Frac(dec) != tell[0])
					Assert.Fail(string.Format("Tell mismatch between encoder and decoder at symbol {0}: {1} instead of {2}", 0, EntCode.Ec_Tell_Frac(dec), tell[0]));

				for (c_int j = 0; j < sz; j++)
				{
					c_int fs;
					c_uint sym = 0;
					c_int dec_method = RandomGenerator.GetRandomNumber() / ((Rand_Max >> 2) + 1);

					switch (dec_method)
					{
						case 0:
						{
							fs = (c_int)EntDec.Ec_Decode(dec, (c_uint)(1 << (int)logp1[j]));
							sym = fs >= (1 << (int)logp1[j]) - 1 ? 1U : 0;
							EntDec.Ec_Dec_Update(dec, (c_uint)(sym != 0 ? (1 << (int)logp1[j]) - 1 : 0), (c_uint)((1 << (int)logp1[j]) - (sym != 0 ? 0 : 1)), (c_uint)(1 << (int)logp1[j]));
							break;
						}

						case 1:
						{
							fs = (c_int)EntDec.Ec_Decode_Bin(dec, logp1[j]);
							sym = fs >= (1 << (int)logp1[j]) - 1 ? 1U : 0;
							EntDec.Ec_Dec_Update(dec, (c_uint)(sym != 0 ? (1 << (int)logp1[j]) - 1 : 0), (c_uint)((1 << (int)logp1[j]) - (sym != 0 ? 0 : 1)), (c_uint)(1 << (int)logp1[j]));
							break;
						}

						case 2:
						{
							sym = EntDec.Ec_Dec_Bit_Logp(dec, logp1[j]) ? 1U : 0;
							break;
						}

						case 3:
						{
							byte[] icdf = new byte[2];
							icdf[0] = 1;
							icdf[1] = 0;

							sym = (c_uint)EntDec.Ec_Dec_Icdf(dec, icdf, logp1[j]);
							break;
						}
					}

					if (sym != data[j])
						Assert.Fail($"Decoded {sym} instead of {data[j]} with logp1 of {logp1[j]} at position {j} of {sz}. Encoding method: {enc_method[j]}, decoding method: {dec_method}");

					if (EntCode.Ec_Tell_Frac(dec) != tell[j + 1])
						Assert.Fail(string.Format("Tell mismatch between encoder and decoder at symbol {0}: {1} instead of {2}", j + 1, EntCode.Ec_Tell_Frac(dec), tell[j + 1]));
				}
			}

			EntEnc.Ec_Enc_Init(out enc, ptr, Data_Size2);

			EntEnc.Ec_Enc_Bit_Logp(enc, false, 1);
			EntEnc.Ec_Enc_Bit_Logp(enc, false, 1);
			EntEnc.Ec_Enc_Bit_Logp(enc, false, 1);
			EntEnc.Ec_Enc_Bit_Logp(enc, false, 1);
			EntEnc.Ec_Enc_Bit_Logp(enc, false, 2);
			EntEnc.Ec_Enc_Patch_Initial_Bits(enc, 3, 2);

			if (enc.error)
				Assert.Fail("patch_initial_bits failed");

			EntEnc.Ec_Enc_Patch_Initial_Bits(enc, 0, 5);

			if (!enc.error)
				Assert.Fail("patch_initial_bits didn't fail when it should have");

			EntEnc.Ec_Enc_Done(enc);

			if ((EntCode.Ec_Range_Bytes(enc) != 1) || (ptr[0] != 192))
				Assert.Fail($"Got {ptr[0]} when expecting 192 for patch_initial_bits");

			EntEnc.Ec_Enc_Init(out enc, ptr, Data_Size2);

			EntEnc.Ec_Enc_Bit_Logp(enc, false, 1);
			EntEnc.Ec_Enc_Bit_Logp(enc, false, 1);
			EntEnc.Ec_Enc_Bit_Logp(enc, true, 6);
			EntEnc.Ec_Enc_Bit_Logp(enc, false, 2);
			EntEnc.Ec_Enc_Patch_Initial_Bits(enc, 0, 2);

			if (enc.error)
				Assert.Fail("patch_initial_bits failed");

			EntEnc.Ec_Enc_Done(enc);

			if ((EntCode.Ec_Range_Bytes(enc) != 2) || (ptr[0] != 63))
				Assert.Fail($"Got {ptr[0]} when expecting 63 for patch_initial_bits");

			EntEnc.Ec_Enc_Init(out enc, ptr, 2);

			EntEnc.Ec_Enc_Bit_Logp(enc, false, 2);

			for (c_int i = 0; i < 48; i++)
				EntEnc.Ec_Enc_Bits(enc, 0, 1);

			EntEnc.Ec_Enc_Done(enc);

			if (!enc.error)
				Assert.Fail("Raw bits overfill didn't fail when it should have");

			EntEnc.Ec_Enc_Init(out enc, ptr, 2);

			for (c_int i = 0; i < 17; i++)
				EntEnc.Ec_Enc_Bits(enc, 0, 1);

			EntEnc.Ec_Enc_Done(enc);

			if (!enc.error)
				Assert.Fail("17 raw bits encoded in two bytes");
		}
	}
}
