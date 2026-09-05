/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers;

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
		public void Test_Misc2()
		{
			foreach (CModSpecifications spec in ModSpecs.Collection)
			{
				Assert.AreEqual((size_t)EffectCommand.Max_Effects, CString.strlen(spec.Commands));
				Assert.AreEqual((size_t)VolumeCommand.Max_VolCmds, CString.strlen(spec.VolCommands));
			}

			//XX ModSample::TransposeToFrequency
			//XX ModSample::Transpose

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

			//XX srlztn::ID::FromInt
			//XX mpt::date
		}
	}
}
