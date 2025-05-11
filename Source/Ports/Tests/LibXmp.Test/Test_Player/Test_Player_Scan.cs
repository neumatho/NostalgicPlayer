/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Common;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_Player
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_Player
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Player_Scan()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();
			Assert.IsNotNull(opaque, "Can't create context");

			c_int ret = LoadModule(dataDirectory, "Ode2Ptk.mod", opaque);
			Assert.AreEqual(0, ret, "Can't load module");

			opaque.Xmp_Start_Player(44100, Xmp_Format.Default);
			opaque.Xmp_Get_Frame_Info(out Xmp_Frame_Info info);
			Assert.AreEqual(85472, info.Total_Time, "Incorrect total time");
			Check_Sequences(opaque, "ode2pth");

			// Make sure S3M/IT end markers aren't assigned invalid sequences
			opaque.Xmp_Release_Module();

			Create_Simple_Module(opaque, 2, 2);
			opaque.loadHelpers.LibXmp_Free_Scan();

			Set_Order(opaque, 0, Constants.Xmp_Mark_Skip);
			Set_Order(opaque, 1, 0);
			Set_Order(opaque, 2, Constants.Xmp_Mark_End);
			Set_Order(opaque, 3, Constants.Xmp_Mark_End);   // May not be given a real sequence
			Set_Quirk(opaque, Quirk_Flag.Marker, Read_Event.It);

			opaque.loadHelpers.LibXmp_Prepare_Scan();
			opaque.scan.LibXmp_Scan_Sequences();

			opaque.Xmp_Start_Player(Constants.Xmp_Min_SRate, Xmp_Format.Default);
			Check_Sequences(opaque, "s3m/it markers");

			opaque.Xmp_End_Player();
			opaque.Xmp_Release_Module();
			opaque.Xmp_Free_Context();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Check_Sequences(Ports.LibXmp.LibXmp opaque, string test)
		{
			Xmp_Context ctx = GetContext(opaque);
			Module_Data m = ctx.M;
			Player_Data p = ctx.P;

			// There should be no junk sequences after the invalid ones
			for (c_int i = m.Num_Sequences; i < Constants.Max_Sequences; i++)
			{
				Assert.AreEqual(0, m.Seq_Data[i].Entry_Point, $"Junk entry point in '{test}' @ {i}");
				Assert.AreEqual(0, m.Seq_Data[i].Duration, $"Junk duration in '{test}' @ {i}");
			}

			// There should be no sequences past the last sequence
			for (c_int i = 0; i < m.Mod.Len; i++)
				Assert.IsTrue((p.Sequence_Control[i] < m.Num_Sequences) || (p.Sequence_Control[i] == Constants.No_Sequence), string.Format("Bad sequence '{0:x2}' in '{1}' @ {2}", p.Sequence_Control[i], test, i));
		}
	}
}
