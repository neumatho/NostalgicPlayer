//---------------------------------------------------------------------------------------
// <copyright file="CharHelper.cs" company="NostalgicPlayer">
// Copyright (c) NostalgicPlayer. All rights reserved.
// </copyright>
//---------------------------------------------------------------------------------------
namespace Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.Pattern
{
	/// <summary>
	/// Helper class for character and font dimension calculations
	/// </summary>
	internal static class RendererCharHelper
	{
		/********************************************************************/
		/// <summary>
		/// Convert character count to pixel width
		/// </summary>
		/// <param name="charCount">Number of characters</param>
		/// <param name="charWidth">Width of a single character in pixels</param>
		/********************************************************************/
		internal static int CalcCharsWidth(int charCount, float charWidth)
		{
			return (int)(charCount * charWidth);
		}

		/********************************************************************/
		/// <summary>
		/// Convert line count to pixel height
		/// </summary>
		/// <param name="lineCount">Number of lines</param>
		/// <param name="charHeight">Height of a single character/line in pixels</param>
		/********************************************************************/
		internal static int CalcLinesHeight(int lineCount, float charHeight)
		{
			return (int)(lineCount * charHeight);
		}
	}
}
