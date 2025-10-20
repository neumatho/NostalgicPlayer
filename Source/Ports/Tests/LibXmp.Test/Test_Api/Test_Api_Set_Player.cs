/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_Api
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_Api
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Api_Set_Player()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();

			// Sample
			#pragma warning disable MSTEST0037
			c_int ret = opaque.Xmp_Get_Player(Xmp_Player.SmpCtl);
			Assert.IsFalse((((Xmp_SmpCtl_Flag)ret) & Xmp_SmpCtl_Flag.Skip) != 0, "Default sample ctl");
			ret = opaque.Xmp_Set_Player(Xmp_Player.SmpCtl, (c_int)Xmp_SmpCtl_Flag.Skip);
			Assert.AreEqual(0, ret, "Error setting flags");
			ret = opaque.Xmp_Get_Player(Xmp_Player.SmpCtl);
			Assert.IsTrue((((Xmp_SmpCtl_Flag)ret) & Xmp_SmpCtl_Flag.Skip) != 0, "Skip sample ctl");
			ret = opaque.Xmp_Set_Player(Xmp_Player.SmpCtl, 0);
			Assert.AreEqual(0, ret, "Error setting flags");
			#pragma warning restore MSTEST0037

			// Default pan
			ret = opaque.Xmp_Get_Player(Xmp_Player.DefPan);
			Assert.AreEqual(100, ret, "Default pan");
			ret = opaque.Xmp_Set_Player(Xmp_Player.DefPan, 0);
			Assert.AreEqual(0, ret, "Error setting default pan");
			ret = opaque.Xmp_Get_Player(Xmp_Player.DefPan);
			Assert.AreEqual(0, ret, "Default pan");
			ret = opaque.Xmp_Set_Player(Xmp_Player.DefPan, 100);
			Assert.AreEqual(0, ret, "Error setting default pan");

			LoadModule(dataDirectory, "Test.xm", opaque);
			opaque.Xmp_Start_Player(8000, Xmp_Format.Mono);

			// Invalid
			ret = opaque.Xmp_Set_Player((Xmp_Player)(-2), 0);
			Assert.IsLessThan(0, ret, "Error setting invalid parameter");

			// Mixer
			ret = opaque.Xmp_Set_Player(Xmp_Player.Interp, (c_int)Xmp_Interp.Nearest);
			Assert.AreEqual(0, ret, "Can't set XMP_INTERP_NEAREST");
			ret = opaque.Xmp_Get_Player(Xmp_Player.Interp);
			Assert.AreEqual((c_int)Xmp_Interp.Nearest, ret, "Can't get XMP_INTERP_NEAREST");

			ret = opaque.Xmp_Set_Player(Xmp_Player.Interp, (c_int)Xmp_Interp.Linear);
			Assert.AreEqual(0, ret, "Can't set XMP_INTERP_LINEAR");
			ret = opaque.Xmp_Get_Player(Xmp_Player.Interp);
			Assert.AreEqual((c_int)Xmp_Interp.Linear, ret, "Can't get XMP_INTERP_LINEAR");

			ret = opaque.Xmp_Set_Player(Xmp_Player.Interp, -2);
			Assert.IsLessThan(0, ret, "Error setting invalid interpolation");

			ret = opaque.Xmp_Get_Player(Xmp_Player.Interp);
			Assert.AreEqual((c_int)Xmp_Interp.Linear, ret, "Invalid interpolation value set");

			// DSP
			ret = opaque.Xmp_Set_Player(Xmp_Player.Dsp, 255);
			Assert.AreEqual(0, ret, "Can't set XMP_PLAYER_DSP");
			ret = opaque.Xmp_Get_Player(Xmp_Player.Dsp);
			Assert.AreEqual(255, ret, "Can't get XMP_PLAYER_DSP");

			// Mix
			ret = opaque.Xmp_Set_Player(Xmp_Player.Mix, 0);
			Assert.AreEqual(0, ret, "Error setting mix");
			ret = opaque.Xmp_Get_Player(Xmp_Player.Mix);
			Assert.AreEqual(0, ret, "Can't get XMP_PLAYER_MIX");

			ret = opaque.Xmp_Set_Player(Xmp_Player.Mix, 100);
			Assert.AreEqual(0, ret, "Error setting mix");
			ret = opaque.Xmp_Get_Player(Xmp_Player.Mix);
			Assert.AreEqual(100, ret, "Can't get XMP_PLAYER_MIX");

			ret = opaque.Xmp_Set_Player(Xmp_Player.Mix, -100);
			Assert.AreEqual(0, ret, "Error setting mix");
			ret = opaque.Xmp_Get_Player(Xmp_Player.Mix);
			Assert.AreEqual(-100, ret, "Can't get XMP_PLAYER_MIX");

			ret = opaque.Xmp_Set_Player(Xmp_Player.Mix, 50);
			Assert.AreEqual(0, ret, "Error setting mix");
			ret = opaque.Xmp_Get_Player(Xmp_Player.Mix);
			Assert.AreEqual(50, ret, "Can't get XMP_PLAYER_MIX");

			ret = opaque.Xmp_Set_Player(Xmp_Player.Mix, 101);
			Assert.IsLessThan(0, ret, "Error setting invalid mix");
			ret = opaque.Xmp_Set_Player(Xmp_Player.Mix, -101);
			Assert.IsLessThan(0, ret, "Error setting invalid mix");

			ret = opaque.Xmp_Get_Player(Xmp_Player.Mix);
			Assert.AreEqual(50, ret, "Invalid mix values set");

			// Amp
			ret = opaque.Xmp_Set_Player(Xmp_Player.Amp, 0);
			Assert.AreEqual(0, ret, "Error setting amp");
			ret = opaque.Xmp_Get_Player(Xmp_Player.Amp);
			Assert.AreEqual(0, ret, "Can't get XMP_PLAYER_AMP");

			ret = opaque.Xmp_Set_Player(Xmp_Player.Amp, 3);
			Assert.AreEqual(0, ret, "Error setting amp");
			ret = opaque.Xmp_Get_Player(Xmp_Player.Amp);
			Assert.AreEqual(3, ret, "Can't get XMP_PLAYER_AMP");

			ret = opaque.Xmp_Set_Player(Xmp_Player.Mix, -100);
			Assert.AreEqual(0, ret, "Error setting mix");
			ret = opaque.Xmp_Get_Player(Xmp_Player.Mix);
			Assert.AreEqual(-100, ret, "Can't get XMP_PLAYER_MIX");

			ret = opaque.Xmp_Set_Player(Xmp_Player.Amp, 2);
			Assert.AreEqual(0, ret, "Error setting amp");
			ret = opaque.Xmp_Get_Player(Xmp_Player.Amp);
			Assert.AreEqual(2, ret, "Can't get XMP_PLAYER_AMP");

			ret = opaque.Xmp_Set_Player(Xmp_Player.Amp, 101);
			Assert.IsLessThan(0, ret, "Error setting invalid amp");
			ret = opaque.Xmp_Set_Player(Xmp_Player.Amp, -1);
			Assert.IsLessThan(0, ret, "Error setting invalid amp");

			ret = opaque.Xmp_Get_Player(Xmp_Player.Amp);
			Assert.AreEqual(2, ret, "Invalid amp values set");

			// Flags
			ret = opaque.Xmp_Set_Player(Xmp_Player.Flags, 0);
			Assert.AreEqual(0, ret, "Error setting flags");
			ret = opaque.Xmp_Get_Player(Xmp_Player.Flags);
			Assert.AreEqual(0, ret, "Can't get XMP_PLAYER_FLAGS");

			ret = opaque.Xmp_Set_Player(Xmp_Player.Flags, (c_int)(Xmp_Flags.VBlank | Xmp_Flags.Fx9Bug | Xmp_Flags.FixLoop));
			Assert.AreEqual(0, ret, "Error setting flags");
			ret = opaque.Xmp_Get_Player(Xmp_Player.Flags);
			Assert.AreEqual((c_int)(Xmp_Flags.VBlank | Xmp_Flags.Fx9Bug | Xmp_Flags.FixLoop), ret, "Can't get XMP_PLAYER_FLAGS");

			// CFlags
			ret = opaque.Xmp_Set_Player(Xmp_Player.CFlags, 0);
			Assert.AreEqual(0, ret, "Error setting flags");
			ret = opaque.Xmp_Get_Player(Xmp_Player.CFlags);
			Assert.AreEqual(0, ret, "Can't get XMP_PLAYER_CFLAGS");

			ret = opaque.Xmp_Set_Player(Xmp_Player.CFlags, (c_int)(Xmp_Flags.VBlank | Xmp_Flags.Fx9Bug | Xmp_Flags.FixLoop));
			Assert.AreEqual(0, ret, "Error setting cflags");
			ret = opaque.Xmp_Get_Player(Xmp_Player.CFlags);
			Assert.AreEqual((c_int)(Xmp_Flags.VBlank | Xmp_Flags.Fx9Bug | Xmp_Flags.FixLoop), ret, "Can't get XMP_PLAYER_CFLAGS");

			// Volume
			ret = opaque.Xmp_Set_Player(Xmp_Player.Volume, 0);
			Assert.AreEqual(0, ret, "Error setting volume");
			ret = opaque.Xmp_Get_Player(Xmp_Player.Volume);
			Assert.AreEqual(0, ret, "Can't get XMP_PLAYER_VOLUME");

			ret = opaque.Xmp_Set_Player(Xmp_Player.Volume, 200);
			Assert.AreEqual(0, ret, "Error setting volume");
			ret = opaque.Xmp_Get_Player(Xmp_Player.Volume);
			Assert.AreEqual(200, ret, "Can't get XMP_PLAYER_VOLUME");

			ret = opaque.Xmp_Set_Player(Xmp_Player.Volume, -1);
			Assert.AreEqual(-(c_int)Xmp_Error.Invalid, ret, "Error setting invalid volume");
			ret = opaque.Xmp_Set_Player(Xmp_Player.Volume, -201);
			Assert.AreEqual(-(c_int)Xmp_Error.Invalid, ret, "Error setting invalid volume");

			opaque.Xmp_Free_Context();
		}
	}
}
