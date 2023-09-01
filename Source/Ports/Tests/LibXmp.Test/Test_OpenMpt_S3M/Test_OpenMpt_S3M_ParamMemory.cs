/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_OpenMpt_S3M
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_OpenMpt_S3M
	{
		/********************************************************************/
		/// <summary>
		/// Scream Tracker 3 uses the last non-zero effect parameter as a
		/// memory for most effects: Dxy, Kxy, Lxy, Exx, Fxx, Ixy, Jxy, Qxy,
		/// Rxy, Sxy. Other effects may have their own memory or share it
		/// with another command (such as Hxy / Uxy)
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_OpenMpt_S3M_ParamMemory()
		{
			Compare_Mixer_Data(Path.Combine(dataDirectory, "OpenMpt", "S3M"), "ParamMemory.s3m", "ParamMemory.data");
		}
	}
}
