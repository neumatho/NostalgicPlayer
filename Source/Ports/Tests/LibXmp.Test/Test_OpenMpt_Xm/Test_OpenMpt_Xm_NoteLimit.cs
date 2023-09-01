/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_OpenMpt_Xm
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_OpenMpt_Xm
	{
		/********************************************************************/
		/// <summary>
		/// I think one of the first things Fasttracker 2 does when parsing
		/// a pattern cell is calculating the “real” note (i.e. pattern note
		/// + sample transpose), and if this “real” note falls out of its
		/// note range, it is ignored completely (wiped from its internal
		/// channel memory). The instrument number next it, however, is not
		/// affected and remains in the memory
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_OpenMpt_Xm_NoteLimit()
		{
			Compare_Mixer_Data(Path.Combine(dataDirectory, "OpenMpt", "Xm"), "NoteLimit.xm", "NoteLimit.data");
		}
	}
}
