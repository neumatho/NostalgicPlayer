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
		/// Note delays retrigger envelopes
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_OpenMpt_Xm_EnvRetrig()
		{
			Compare_Mixer_Data(Path.Combine(dataDirectory, "OpenMpt", "Xm"), "EnvRetrig.xm", "EnvRetrig.data");
		}
	}
}
