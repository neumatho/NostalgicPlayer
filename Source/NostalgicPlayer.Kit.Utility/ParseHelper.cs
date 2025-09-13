/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Text.RegularExpressions;

namespace Polycode.NostalgicPlayer.Kit.Utility
{
	/// <summary>
	/// Parse helper methods
	/// </summary>
	public static class ParseHelper
	{
		/********************************************************************/
		/// <summary>
		/// Parse an integer from a string and ignoring any trailing garbage
		/// </summary>
		/********************************************************************/
		public static int ParseInt(string s)
		{
			Match match = Regex.Match(s, @"^\s*[\+\-0-9][0-9]*");
			if (match.Success)
				return int.Parse(match.Value);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Parse a float from a string and ignoring any trailing garbage
		/// </summary>
		/********************************************************************/
		public static float ParseFloat(string s)
		{
			Match match = Regex.Match(s, @"^\s*[\+\-0-9e]+");
			if (match.Success)
				return float.Parse(match.Value);

			return 0;
		}
	}
}
