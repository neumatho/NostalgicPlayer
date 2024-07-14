/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Text;

namespace Polycode.NostalgicPlayer.Kit.Extensions
{
	/// <summary>
	/// Extension methods to the string class
	/// </summary>
	public static class StringExtension
	{
		/********************************************************************/
		/// <summary>
		/// Remove any invalid characters from the given string
		/// </summary>
		/********************************************************************/
		public static string RemoveInvalidChars(this string str)
		{
			char[] strArray = str.ToCharArray();

			for (int i = 0, cnt = str.Length; i < cnt; i++)
			{
				char chr = str[i];

				if (chr < 32)
					chr = ' ';

				strArray[i] = chr;
			}

			return new string(strArray);
		}



		/********************************************************************/
		/// <summary>
		/// Convert tabs to spaces
		/// </summary>
		/********************************************************************/
		public static string ConvertTabs(this string str, int tabSize)
		{
			int index = str.IndexOf('\t');
			if (index == -1)
				return str;

			StringBuilder sb = new StringBuilder();
			int startIndex = 0;

			do
			{
				sb.Append(str.Substring(startIndex, index - startIndex));
				startIndex = index + 1;

				int spacesToInsert = tabSize - (sb.Length % tabSize);
				sb.Append(new string(' ', spacesToInsert));

				index = str.IndexOf('\t', startIndex);
			}
			while (index != -1);

			sb.Append(str.Substring(startIndex));

			return sb.ToString();
		}
	}
}
