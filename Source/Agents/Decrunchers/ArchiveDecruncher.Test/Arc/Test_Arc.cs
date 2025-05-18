/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.Decruncher.ArchiveDecruncher.Test.Arc
{
	/// <summary>
	/// 
	/// </summary>
	[TestClass]
	public partial class Test_Arc : Test
	{
		private class ArcInfo
		{
			public byte Method;
			public int CrunchedSize;
			public int DecrunchedSize;
			public byte[] CrunchedData;
			public byte[] DecrunchedData;
		}

		/********************************************************************/
		/// <summary>
		/// All our test files are arc archive files, but it is the
		/// decrunching itself we want to test, so read need needed
		/// information from the archive and load the crunched data
		/// </summary>
		/********************************************************************/
		private ArcInfo GetArcInfo(string fileName)
		{
			ArcInfo result = new ArcInfo();

			string fullPath = Path.Combine(dataDirectory, fileName);

			using (ReaderStream rs = new ReaderStream(new FileStream(fullPath, FileMode.Open, FileAccess.Read)))
			{
				rs.Seek(1, SeekOrigin.Begin);
				result.Method = rs.Read_UINT8();

				rs.Seek(15, SeekOrigin.Begin);
				result.CrunchedSize = rs.Read_L_INT32();

				int m = result.Method & 0x7f;

				if ((m == 1) || (m == 2))
					result.DecrunchedSize = result.CrunchedSize;
				else
				{
					rs.Seek(25, SeekOrigin.Begin);
					result.DecrunchedSize = rs.Read_L_INT32();
				}

				rs.Seek(29, SeekOrigin.Begin);

				if ((result.Method & 0x80) != 0)
					rs.Seek(12, SeekOrigin.Current);

				result.CrunchedData = new byte[result.CrunchedSize];
				rs.ReadExactly(result.CrunchedData);

				result.DecrunchedData = new byte[result.DecrunchedSize];
			}

			return result;
		}
	}
}
