/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_Storlek
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_Storlek
	{
		/********************************************************************/
		/// <summary>
		/// 14 - Ping-pong loop and sample number
		///
		/// A lone sample number should not reset ping pong loop direction.
		///
		/// In this test, the sample should loop back and forth in both
		/// cases. If the sample gets "stuck", check that the player is not
		/// touching the loop direction unnecessarily
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Storlek_14_PingPong_Loop_And_Sample_Number()
		{
			c_int[] position = new c_int[100];
			c_int i = 0, j = 0;

			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();

			LoadModule(dataDirectory, "Storlek_14.it", opaque);
			opaque.Xmp_Start_Player(44100, 0);

			while (true)
			{
				opaque.Xmp_Play_Frame();
				opaque.Xmp_Get_Frame_Info(out Xmp_Frame_Info info);

				if (info.Loop_Count > 0)
					break;

				Xmp_Channel_Info ci = info.channel_Info[0];

				if (info.Row < 16)
				{
					position[i] = (c_int)ci.Position;
					i++;
				}
				else
				{
					Assert.AreEqual(position[j], (c_int)ci.Position, "Position error");
					j++;
				}
			}

			opaque.Xmp_End_Player();
			opaque.Xmp_Release_Module();
			opaque.Xmp_Free_Context();
		}
	}
}
