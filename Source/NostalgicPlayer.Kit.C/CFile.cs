/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using System.Text;

namespace Polycode.NostalgicPlayer.Kit.C
{
	/// <summary>
	/// C like file methods
	/// </summary>
	public static class CFile
	{
		/********************************************************************/
		/// <summary>
		/// Writes a string to the specified stream up to but not including
		/// the null character.
		/// </summary>
		/********************************************************************/
		public static c_int fputs(CPointer<char> str, Stream stream)
		{
			byte[] buffer = Encoding.UTF8.GetBytes(str.ToString());

			stream.Write(buffer);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Writes a string to the specified stream up to but not including
		/// the null character.
		/// </summary>
		/********************************************************************/
		public static c_int fputs(CPointer<char> str, TextWriter textWriter)
		{
			textWriter.Write(str);

			return 0;
		}
	}
}
