/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Common;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_Mixer
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_Mixer
	{
		/********************************************************************/
		/// <summary>
		/// MPT 1.16 pre-amp scaling logarithmically reduces the effective
		/// mix volume of modules based on the number of channels they
		/// contain, from 100% (&lt;=5 channels) to 50% (30+). The input
		/// module has 100 mix volume and 32 channels, so the effective mix
		/// volume should be 50.
		/// 
		/// (Note: this isn't quite true for pre-amp levels higher than 128;
		/// libxmp currently only supports a fixed value of 128, and does
		/// not attempt to emulate other aspects of MPT's pre-amp routine.
		/// This is currently applied in the loader instead of the mixer)
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Mixer_Mpt116_PreAmp()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();

			LoadModule(dataDirectory, "Mpt116_32Chn.it", opaque);

			Xmp_Context ctx = GetContext(opaque);
			Module_Data m = ctx.M;

			Assert.AreEqual(50, m.MVol, "Mix volume not adjusted correctly");

			opaque.Xmp_Release_Module();
			opaque.Xmp_Free_Context();
		}
	}
}
