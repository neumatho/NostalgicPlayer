﻿/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Common;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_Effect
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_Effect
	{
		private static readonly c_int[] patternLoop_Vals1 =
		{
			0, 0, 0,
			1, 1, 1,
			1, 1, 1,
			1, 1, 1,
			1, 1, 1,
			1, 1, 1,
			2, 2, 2
		};

		/********************************************************************/
		/// <summary>
		/// Atari Octalyser seems to have the loop arguments as global
		/// instead of channel separated. At least what I can see from
		/// 8er-mod.
		///
		/// The replay sources that I got my hands on, does not support E6x,
		/// so I can not verify it.
		///
		/// This test simulate what 8er-mod module does by having both E60
		/// and E6x on the same row in different channels. The behaviour
		/// should loop the same row and then continue
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Effect_Oct_Pattern_Loop1()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();
			Xmp_Frame_Info info;

			Create_Simple_Module(opaque, 2, 2);
			Set_Quirk(opaque, Quirk_Flag.OctalyserLoop, Read_Event.Mod);

			New_Event(opaque, 0, 0, 0, 60, 1, 0, 0, 0, Effects.Fx_Speed, 3);
			New_Event(opaque, 0, 1, 0, 0, 0, 0, Effects.Fx_Extended, 0x60, 0, 0);
			New_Event(opaque, 0, 1, 1, 0, 0, 0, Effects.Fx_Extended, 0x64, 0, 0);
			New_Event(opaque, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0);

			// Play it
			opaque.Xmp_Start_Player(44100, 0);

			for (c_int i = 0; i < 7 * 3; i++)
			{
				opaque.Xmp_Play_Frame();
				opaque.Xmp_Get_Frame_Info(out info);
				Assert.AreEqual(patternLoop_Vals1[i], info.Row, "Row set error");
			}

			opaque.Xmp_Release_Module();
			opaque.Xmp_Free_Context();
		}
	}
}
