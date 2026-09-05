/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.C.Std;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundBase.SampleConversion;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Dithers;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibOpenMpt.Test.Tests
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Tests
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_SampleConversion()
		{
			// Signed 8-Bit integer PCM
			// Unsigned 8-Bit integer PCM
			// Delta 8-Bit integer PCM
			{
				vector<byte> source8 = new vector<byte>(256);

				for (size_t i = 0; i < 256; i++)
					source8[i] = (byte)i;

				vector<int8> signed8 = new vector<int8>(256);
				vector<int8> unsigned8 = new vector<int8>(256);
				vector<int8> delta8 = new vector<int8>(256);
				int8 delta = 0;

				SampleCopy.CopySample<DecodeInt8, int8>(signed8.data(), 256, 1, source8.data(), 256, 1);
				SampleCopy.CopySample<DecodeUInt8, int8>(unsigned8.data(), 256, 1, source8.data(), 256, 1);
				SampleCopy.CopySample<DecodeInt8Delta, int8>(delta8.data(), 256, 1, source8.data(), 256, 1);

				for (size_t i = 0; i < 256; i++)
				{
					delta += (int8)i;

					Assert.AreEqual((int8)i, signed8[i]);
					Assert.AreEqual((int8)(i - 0x80), unsigned8[i]);
					Assert.AreEqual(delta, delta8[i]);
				}
			}

			// Signed 16-Bit integer PCM
			// Unsigned 16-Bit integer PCM
			// Delta 16-Bit integer PCM
			{
				// Little Endian

				vector<byte> source16 = new vector<byte>(65536 * 2);

				for (size_t i = 0; i < 65536; i++)
				{
					source16[(i * 2) + 0] = (byte)(i & 0xff);
					source16[(i * 2) + 1] = (byte)(i >> 8);
				}

				vector<int16> signed16 = new vector<int16>(65536);
				vector<int16> unsigned16 = new vector<int16>(65536);
				vector<int16> delta16 = new vector<int16>(65536);
				int16 delta = 0;

				SampleCopy.CopySample<DecodeInt16<Offset_0, LittleEndian16>, int16>(signed16.data(), 65536, 1, source16.data(), 65536 * 2, 1);
				SampleCopy.CopySample<DecodeInt16<Offset_0x8000, LittleEndian16>, int16>(unsigned16.data(), 65536, 1, source16.data(), 65536 * 2, 1);
				SampleCopy.CopySample<DecodeInt16Delta<LittleEndian16>, int16>(delta16.data(), 65536, 1, source16.data(), 65536 * 2, 1);

				for (size_t i = 0; i < 65536; i++)
				{
					delta += (int16)i;

					Assert.AreEqual((int16)i, signed16[i]);
					Assert.AreEqual((int16)(i - 0x8000), unsigned16[i]);
					Assert.AreEqual(delta, delta16[i]);
				}

				// Big Endian

				for (size_t i = 0; i < 65536; i++)
				{
					source16[(i * 2) + 0] = (byte)(i >> 8);
					source16[(i * 2) + 1] = (byte)(i & 0xff);
				}

				SampleCopy.CopySample<DecodeInt16<Offset_0, BigEndian16>, int16>(signed16.data(), 65536, 1, source16.data(), 65536 * 2, 1);
				SampleCopy.CopySample<DecodeInt16<Offset_0x8000, BigEndian16>, int16>(unsigned16.data(), 65536, 1, source16.data(), 65536 * 2, 1);
				SampleCopy.CopySample<DecodeInt16Delta<BigEndian16>, int16>(delta16.data(), 65536, 1, source16.data(), 65536 * 2, 1);

				delta = 0;

				for (size_t i = 0; i < 65536; i++)
				{
					delta += (int16)i;

					Assert.AreEqual((int16)i, signed16[i]);
					Assert.AreEqual((int16)(i - 0x8000), unsigned16[i]);
					Assert.AreEqual(delta, delta16[i]);
				}
			}

			//XX Signed 24-Bit integer PCM
			{
			}

			//XX Float 32-Bit
			{
			}

			//XX ALaw
			{
			}

			//XX uLaw
			{
			}

			// Range checks
			{
				byte[] oneSample = [ 1 ];
				int8[] targetBuf4 = new int8[4];
				CPointer<int8> signed8 = targetBuf4;

				CMemory.memset<int8>(signed8, 0, 4);
				SampleCopy.CopySample<DecodeInt8, int8>(targetBuf4, 4, 1, oneSample, (size_t)oneSample.Length, 1);

				Assert.AreEqual(1, signed8[0]);
				Assert.AreEqual(0, signed8[1]);
				Assert.AreEqual(0, signed8[2]);
				Assert.AreEqual(0, signed8[3]);
			}

			// Dither
			{
				vector<MixSampleInt> buffer = new vector<MixSampleInt>(64);

				DithersWrapperOpenMpt dithers = DithersWrapperOpenMpt.Create(MptRandom.Global_Random_Device(), 2 /* DitherModPlug */, 2);

				for (size_t i = 0; i < 64; ++i)
				{
					size_t frame = i;

					buffer[frame] = dithers.Variant().visit(
						dither => dither.Process(16, 0, buffer[frame]),
						dither => dither.Process(16, 0, buffer[frame]),
						dither => dither.Process(16, 0, buffer[frame]),
						dither => dither.Process(16, 0, buffer[frame]));
				}

				int32[] expected =
				[
					727, -557, -552, -727, 439, 405, 703, -337,
					235, -776, -458, 905, -110, 158, 374, -362,
					283, 306, 710, 304, -608, 536, -501, -593,
					-349, 812, 916, 53, -953, 881, -236, -20,
					-623, -895, -302, -415, 899, -948, -766, -186,
					-390, -169, 253, -622, -769, -1001, 1019, 787,
					-239, 718, -423, 988, -91, 763, -933, -510,
					484, 794, -340, 552, 866, -608, 35, 395
				];

				for (size_t i = 0; i < 64; ++i)
					Assert.AreEqual(expected[i], buffer[i]);
			}
		}
	}
}
