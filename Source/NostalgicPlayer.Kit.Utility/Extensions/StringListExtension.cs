/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;

namespace Polycode.NostalgicPlayer.Kit.Utility.Extensions
{
	/// <summary>
	/// Extension methods to lists of strings (lines of text)
	/// </summary>
	public static class StringListExtension
	{
		/********************************************************************/
		/// <summary>
		/// Remove trailing empty lines
		/// </summary>
		/********************************************************************/
		public static List<string> RemoveTrailingEmptyLines(this List<string> lines)
		{
			if (lines.Count > 0)
			{
				for (int line = lines.Count - 1; line >= 0; line--)
				{
					if (string.IsNullOrEmpty(lines[line]))
						lines.RemoveAt(line);
					else
						break;
				}
			}

			return lines;
		}
	}
}
