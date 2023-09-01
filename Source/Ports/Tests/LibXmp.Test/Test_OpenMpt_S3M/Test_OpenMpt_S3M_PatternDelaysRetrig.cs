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
		/// Rows on which a row delay (SEx) effect is placed have multiple
		/// “first ticks”, i.e. you should set your “first tick flag” on
		/// every tick that is a multiple of the song speed (or speed + tick
		/// delay if you support tick delays in your S3M player). In this
		/// test module, the note pitch is changed multiple times per row,
		/// depending on the row delay values
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_OpenMpt_S3M_PatternDelaysRetrig()
		{
			Compare_Mixer_Data(Path.Combine(dataDirectory, "OpenMpt", "S3M"), "PatternDelaysRetrig.s3m", "PatternDelaysRetrig.data");
		}
	}
}
