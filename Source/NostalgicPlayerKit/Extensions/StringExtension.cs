/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
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
	}
}
