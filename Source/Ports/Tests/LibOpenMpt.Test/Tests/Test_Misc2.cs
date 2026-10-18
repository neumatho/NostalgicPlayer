/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common.Srlztn;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibOpenMpt.Test.Tests
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Tests
	{
		private static readonly int32[] transposeToFrequency =
		[
			      5,       5,       5,       5,
			     31,      32,      33,      34,
			    196,     202,     207,     214,
			   1243,    1280,    1317,    1356,
			   7894,    8125,    8363,    8608,
			  50121,   51590,   53102,   54658,
			 318251,  327576,  337175,  347055,
			2020767, 2079980, 2140928, 2203663,
		];

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Misc2()
		{
			// Check for completeness of supported effect list in mod specifications
			foreach (CModSpecifications spec in ModSpecs.Collection)
			{
				Assert.AreEqual((size_t)EffectCommand.Max_Effects, CString.strlen(spec.Commands));
				Assert.AreEqual((size_t)VolumeCommand.Max_VolCmds, CString.strlen(spec.VolCommands));
			}

			// Check floating-point accuracy in TransposeToFrequency
			c_int freqIndex = 0;

			for (int32 transpose = -128; transpose < 128; transpose += 32)
			{
				for (int32 fineTune = -128; fineTune < 128; fineTune += 64, freqIndex++)
				{
					uint32 freq = ModSample.TransposeToFrequency(transpose, fineTune);
					Assert.AreEqual(transposeToFrequency[freqIndex], (int32)freq, 1);

					if (transpose >= -96)
					{
						// Verify transpose+finetune <-> frequency roundtrip
						// (not for transpose = -128 because it would require fractional precision that we don't have here)
						ModSample smp = new ModSample();
						smp.nC5Speed = freq;
						smp.FrequencyToTranspose();
						smp.TransposeToFrequency();
						Assert.AreEqual(freq, smp.nC5Speed);
					}
				}
			}

			{
				ModSample smp = new ModSample();
				smp.nC5Speed = 9999;
				smp.Transpose(1.5);
				Assert.AreEqual(28281, (int32)smp.nC5Speed, 1);

				smp.Transpose(-1.3);
				Assert.AreEqual(11486, (int32)smp.nC5Speed, 1);
			}

			// Check SamplePosition fixed-point type
			SamplePosition samplePos;

			Assert.AreEqual(1, new SamplePosition(1).GetRaw());

			samplePos = new SamplePosition(2);
			samplePos.Set(1);
			Assert.AreEqual(new SamplePosition(1, 0), samplePos);

			samplePos = new SamplePosition(2);
			samplePos.SetInt(1);
			Assert.AreEqual(new SamplePosition(1, 2), samplePos);

			Assert.IsTrue(new SamplePosition(1).IsPositive());
			Assert.IsTrue(new SamplePosition(0).IsZero());
			Assert.IsTrue(new SamplePosition(1, 0).IsUnity());
			Assert.IsFalse(new SamplePosition(1, 1).IsUnity());
			Assert.IsTrue(new SamplePosition(-1).IsNegative());
			Assert.AreEqual(int64.MaxValue, new SamplePosition(int64.MaxValue).GetRaw());
			Assert.AreEqual(2, new SamplePosition(2, SamplePosition.FractMax).GetInt());
			Assert.AreEqual(SamplePosition.FractMax, new SamplePosition(2, SamplePosition.FractMax).GetFract());
			Assert.AreEqual(new SamplePosition(0, 1), new SamplePosition(1, SamplePosition.FractMax).GetInvertedFract());
			Assert.AreEqual(new SamplePosition(1, 0), new SamplePosition(1, 0).GetInvertedFract());

			samplePos = new SamplePosition(2, 0);
			samplePos.Negate();
			Assert.AreEqual(new SamplePosition(-2, 0), samplePos);

			Assert.AreEqual(new SamplePosition(2, 0), SamplePosition.Ratio(10, 5));
			Assert.AreEqual(new SamplePosition(3, 3), new SamplePosition(1, 1) + new SamplePosition(2, 2));
			Assert.AreEqual(new SamplePosition(3, 0), new SamplePosition(1, 0) * 3);
			Assert.AreEqual(3, new SamplePosition(6, 0) / new SamplePosition(2, 0));

			Assert.AreEqual(new Id("\x21\x43\x65\x87".ToPointer()), Id.FromInt(0x87654321));
		}
	}
}
