﻿/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_OpenMpt_It
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_OpenMpt_It
	{
		/********************************************************************/
		/// <summary>
		/// Probably a very MPT-specific bug, as OpenMPT was adding the
		/// volume swing to the current volume, so even after setting the
		/// volume to 0, it was possible that you could hear the sample
		/// playing
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_OpenMpt_It_Swing1()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();

			LoadModule(Path.Combine(dataDirectory, "OpenMpt", "It"), "Swing1.it", opaque);
			opaque.Xmp_Start_Player(8000, 0);

			while (true)
			{
				opaque.Xmp_Play_Frame();
				opaque.Xmp_Get_Frame_Info(out Xmp_Frame_Info info);

				if (info.Loop_Count > 0)
					break;

				Xmp_Channel_Info ci = info.Channel_Info[0];
				if (info.Row >= 2)
					Assert.AreEqual(0, ci.Volume, "Volume not zero");
			}

			opaque.Xmp_End_Player();
			opaque.Xmp_Release_Module();
			opaque.Xmp_Free_Context();
		}
	}
}
