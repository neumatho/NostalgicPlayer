/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Polycode.NostalgicPlayer.GuiKit.Extensions
{
	/// <summary>
	/// Extension methods to the string class
	/// </summary>
	public static class StringExtension
	{
		/********************************************************************/
		/// <summary>
		/// Split the given string into multiple lines if needed if the
		/// exceed the given length
		/// </summary>
		/********************************************************************/
		public static IEnumerable<string> SplitIntoLines(this string str, IntPtr handle, int maxWidth, Font font)
		{
			using (Graphics g = Graphics.FromHwnd(handle))
			{
				while (!string.IsNullOrEmpty(str))
				{
					string tempStr;

					// See if there is any newlines
					int index = str.IndexOf('\n');
					if (index != -1)
					{
						// There is, get the line
						tempStr = str.Substring(0, index);
						str = str.Substring(index + 1);
					}
					else
					{
						tempStr = str;
						str = string.Empty;
					}

					// Adjust the description line
					tempStr = tempStr.Trim();

					// Just add empty lines
					if (string.IsNullOrEmpty(tempStr))
						yield return string.Empty;
					else
					{
						do
						{
							int lineWidth = (int)g.MeasureString(tempStr, font).Width;

							string tempStr1 = string.Empty;

							while (lineWidth >= maxWidth)
							{
								// We need to split the line
								index = tempStr.LastIndexOf(' ');
								if (index != -1)
								{
									// Found a space, check if the line can be showed now
									tempStr1 = tempStr.Substring(index) + tempStr1;
									tempStr = tempStr.Substring(0, index);

									lineWidth = (int)g.MeasureString(tempStr, font).Width;
								}
								else
								{
									// Well, the line can't be showed and we can't split it :-(
									break;
								}
							}

							// Adjust the description line
							tempStr = tempStr.Trim();

							// Add the line in the grid
							yield return tempStr;

							tempStr = tempStr1.Trim();
						}
						while (!string.IsNullOrEmpty(tempStr));
					}
				}
			}
		}
	}
}
