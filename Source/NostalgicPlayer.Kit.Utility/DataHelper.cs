/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Kit.Utility
{
	/// <summary>
	/// Data helper methods
	/// </summary>
	public static class DataHelper
	{
		/********************************************************************/
		/// <summary>
		/// Convert a byte array to a hexadecimal string
		/// </summary>
		/********************************************************************/
		public static string ToHex(byte[] data)
		{
			char[] result = new char[data.Length * 2];

			for (int y = 0, x = 0; y < data.Length; y++)
			{
				byte b = (byte)(data[y] >> 4);
				result[x++] = (char)(b > 9 ? b + 0x37 : b + 0x30);

				b = (byte)(data[y] & 0x0f);
				result[x++] = (char)(b > 9 ? b + 0x37 : b + 0x30);
			}

			return new string(result);
		}
	}
}
