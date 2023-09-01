/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_Effect
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_Effect
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Effect_0_Arpeggio()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();

			Create_Simple_Module(opaque, 2, 2);

			// Standard arpeggio
			New_Event(opaque, 0, 0, 0, 61, 1, 0, 0, 0x00, 0, 0);
			New_Event(opaque, 0, 1, 0, 61, 1, 0, 0, 0x01, 0, 0);
			New_Event(opaque, 0, 2, 0, 61, 1, 0, 0, 0x05, 0, 0);
			New_Event(opaque, 0, 3, 0, 61, 1, 0, 0, 0x50, 0, 0);
			New_Event(opaque, 0, 4, 0, 61, 1, 0, 0, 0x35, 0, 0);

			opaque.Xmp_Start_Player(44100, 0);

			Check_Arpeggio(opaque, 60, 0x00, 6);
			Check_Arpeggio(opaque, 60, 0x01, 6);
			Check_Arpeggio(opaque, 60, 0x05, 6);
			Check_Arpeggio(opaque, 60, 0x50, 6);
			Check_Arpeggio(opaque, 60, 0x35, 6);

			opaque.Xmp_Release_Module();
			opaque.Xmp_Free_Context();
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Check_Arpeggio(Ports.LibXmp.LibXmp opaque, c_int note, c_int val, c_int spd)
		{
			c_int a1 = val >> 4;
			c_int a2 = val & 0x0f;
			c_int[] arp = new c_int[20];

			for (c_int i = 0; i < 12; )
			{
				arp[i++] = Note_To_Period(note);
				arp[i++] = Note_To_Period(note + a1);
				arp[i++] = Note_To_Period(note + a2);
			}

			for (c_int i = 0; i < spd; i++)
			{
				opaque.Xmp_Play_Frame();
				opaque.Xmp_Get_Frame_Info(out Xmp_Frame_Info info);
				Assert.AreEqual(arp[i], Period(info), $"Arpeggio 0x{val:x2} error");
			}
		}
		#endregion
	}
}
