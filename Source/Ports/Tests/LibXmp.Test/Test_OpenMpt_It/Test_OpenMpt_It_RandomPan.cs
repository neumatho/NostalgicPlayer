/******************************************************************************/
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
		/// Pan swing should not be overridden by effects such as instrument
		/// panning or panning envelopes. Previously, pan swing was
		/// overridden in OpenMPT if the instrument also had a panning
		/// envelope. In this file, pan swing should be applied to every note
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_OpenMpt_It_RandomPan()
		{
			c_int[] values = new c_int[64];

			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();

			LoadModule(Path.Combine(dataDirectory, "OpenMpt", "It"), "RandomPan.it", opaque);
			opaque.Xmp_Start_Player(8000, 0);
			opaque.Xmp_Set_Player(Xmp_Player.Mix, 100);

			while (true)
			{
				opaque.Xmp_Play_Frame();
				opaque.Xmp_Get_Frame_Info(out Xmp_Frame_Info info);

				if (info.Loop_Count > 0)
					break;

				Xmp_Channel_Info ci = info.Channel_Info[0];
				if (info.Frame == 0)
					values[info.Row] = ci.Pan;
			}

			// Check pan randomness
			Assert.IsTrue(Check_Randomness(values, 64, 10), "Randomness error");

			opaque.Xmp_End_Player();
			opaque.Xmp_Release_Module();
			opaque.Xmp_Free_Context();
		}
	}
}
