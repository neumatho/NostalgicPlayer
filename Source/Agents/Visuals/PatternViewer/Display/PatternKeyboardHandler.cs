//---------------------------------------------------------------------------------------
// <copyright file="PatternKeyboardHandler.cs" company="NostalgicPlayer">
// Copyright (c) NostalgicPlayer. All rights reserved.
// </copyright>
//---------------------------------------------------------------------------------------
using System;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.Pattern;
using Polycode.NostalgicPlayer.Kit.Containers;

namespace Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.Display
{
	/// <summary>
	/// Static handler for pattern viewer keyboard navigation
	/// </summary>
	internal static class PatternKeyboardHandler
	{
		/********************************************************************/
		/// <summary>
		/// Get safe row count (at least 1 for navigation purposes)
		/// </summary>
		/********************************************************************/
		private static int GetSafeRowCount(PatternRenderer renderer, int songPosition)
		{
			if (renderer.SongData == null || songPosition < 0 || songPosition >= renderer.SongData.Count)
			{
				return 1;
			}

			SongPatternViewData pattern = renderer.SongData[songPosition];
			return pattern?.RowCount > 0 ? pattern.RowCount : 1;
		}

		/********************************************************************/
		/// <summary>
		/// Clamp row to valid range for pattern
		/// </summary>
		/********************************************************************/
		private static void ClampRowToPattern(PatternRenderer renderer)
		{
			int maxRows = GetSafeRowCount(renderer, renderer.ManualSongPosition);
			renderer.ManualRow = Math.Max(0, Math.Min(renderer.ManualRow, maxRows - 1));
		}

		/********************************************************************/
		/// <summary>
		/// Handle keyboard input for pattern navigation
		/// </summary>
		/// <returns>True if redraw is needed</returns>
		/********************************************************************/
		public static bool HandleKeyDown(PatternRenderer renderer, Keys keyCode, bool ctrlPressed)
		{
			if (renderer.SongData == null || renderer.SongData.Count == 0)
			{
				return false;
			}

			if (ctrlPressed)
			{
				return HandleCtrlKey(renderer, keyCode);
			}

			return HandleNormalKey(renderer, keyCode);
		}

		/********************************************************************/
		/// <summary>
		/// Handle Ctrl+Key combinations
		/// </summary>
		/********************************************************************/
		private static bool HandleCtrlKey(PatternRenderer renderer, Keys keyCode)
		{
			switch (keyCode)
			{
				case Keys.Left:
					return MoveSongPositionLeft(renderer);

				case Keys.Right:
					return MoveSongPositionRight(renderer);

				case Keys.Home:
					return JumpToFirstPattern(renderer);

				case Keys.End:
					return JumpToLastPattern(renderer);

				default:
					return false;
			}
		}

		/********************************************************************/
		/// <summary>
		/// Handle normal key presses (without Ctrl)
		/// </summary>
		/********************************************************************/
		private static bool HandleNormalKey(PatternRenderer renderer, Keys keyCode)
		{
			switch (keyCode)
			{
				case Keys.Up:
					return MoveRowUp(renderer);

				case Keys.Down:
					return MoveRowDown(renderer);

				case Keys.Left:
					return ScrollChannelsLeft(renderer);

				case Keys.Right:
					return ScrollChannelsRight(renderer);

				case Keys.PageUp:
					return PageUp(renderer);

				case Keys.PageDown:
					return PageDown(renderer);

				case Keys.Home:
					return JumpToFirstRow(renderer);

				case Keys.End:
					return JumpToLastRow(renderer);

				default:
					return false;
			}
		}

		/********************************************************************/
		/// <summary>
		/// Move to previous pattern (Ctrl+Left)
		/// </summary>
		/********************************************************************/
		private static bool MoveSongPositionLeft(PatternRenderer renderer)
		{
			if (!renderer.AllowPatternScrolling)
			{
				return false;
			}

			if (renderer.ManualSongPosition <= 0)
			{
				return false;
			}

			renderer.ManualSongPosition--;
			ClampRowToPattern(renderer);

			return true;
		}

		/********************************************************************/
		/// <summary>
		/// Move to next pattern (Ctrl+Right)
		/// </summary>
		/********************************************************************/
		private static bool MoveSongPositionRight(PatternRenderer renderer)
		{
			if (!renderer.AllowPatternScrolling)
			{
				return false;
			}

			if (renderer.ManualSongPosition >= renderer.SongData.Count - 1)
			{
				return false;
			}

			renderer.ManualSongPosition++;
			ClampRowToPattern(renderer);

			return true;
		}

		/********************************************************************/
		/// <summary>
		/// Jump to first pattern, row 0 (Ctrl+Home)
		/// </summary>
		/********************************************************************/
		private static bool JumpToFirstPattern(PatternRenderer renderer)
		{
			if (!renderer.AllowPatternScrolling)
			{
				return false;
			}

			renderer.ManualSongPosition = 0;
			renderer.ManualRow = 0;
			return true;
		}

		/********************************************************************/
		/// <summary>
		/// Jump to last pattern, last row (Ctrl+End)
		/// </summary>
		/********************************************************************/
		private static bool JumpToLastPattern(PatternRenderer renderer)
		{
			if (!renderer.AllowPatternScrolling)
			{
				return false;
			}

			if (renderer.SongData == null || renderer.SongData.Count == 0)
			{
				return false;
			}

			renderer.ManualSongPosition = renderer.SongData.Count - 1;
			int maxRows = GetSafeRowCount(renderer, renderer.ManualSongPosition);
			renderer.ManualRow = maxRows - 1;
			return true;
		}

		/********************************************************************/
		/// <summary>
		/// Move one row up
		/// </summary>
		/********************************************************************/
		private static bool MoveRowUp(PatternRenderer renderer)
		{
			if (!renderer.AllowPatternScrolling)
			{
				return false;
			}

			renderer.ManualRow--;

			if (renderer.ManualRow < 0)
			{
				if (renderer.RollingPatterns && renderer.ManualSongPosition > 0)
				{
					// Rolling mode: move to previous pattern
					renderer.ManualSongPosition--;
					int prevPatternRows = GetSafeRowCount(renderer, renderer.ManualSongPosition);
					renderer.ManualRow = prevPatternRows - 1; // Last row of previous pattern
				}
				else
					// Normal mode: clamp at 0
				{
					renderer.ManualRow = 0;
				}
			}

			return true;
		}

		/********************************************************************/
		/// <summary>
		/// Move one row down
		/// </summary>
		/********************************************************************/
		private static bool MoveRowDown(PatternRenderer renderer)
		{
			if (!renderer.AllowPatternScrolling)
			{
				return false;
			}

			// Get max rows for current position
			int maxRows = GetSafeRowCount(renderer, renderer.ManualSongPosition);

			renderer.ManualRow++;

			if (renderer.ManualRow >= maxRows)
			{
				if (renderer.RollingPatterns && renderer.ManualSongPosition < renderer.SongData.Count - 1)
				{
					// Rolling mode: move to next pattern
					renderer.ManualSongPosition++;
					renderer.ManualRow = 0; // First row of next pattern
				}
				else
					// Normal mode: clamp at max
				{
					renderer.ManualRow = maxRows - 1;
				}
			}

			return true;
		}

		/********************************************************************/
		/// <summary>
		/// Scroll channels left (show earlier channels)
		/// </summary>
		/********************************************************************/
		private static bool ScrollChannelsLeft(PatternRenderer renderer)
		{
			if (renderer.FirstVisibleChannel <= 0)
			{
				return false;
			}

			renderer.FirstVisibleChannel--;
			return true;
		}

		/********************************************************************/
		/// <summary>
		/// Scroll channels right (show later channels)
		/// </summary>
		/********************************************************************/
		private static bool ScrollChannelsRight(PatternRenderer renderer)
		{
			// We want to allow scrolling until the last channel is the first visible one
			// This ensures the last channel is always fully visible
			if (renderer.FirstVisibleChannel >= renderer.ChannelCount - 1)
			{
				return false;
			}

			renderer.FirstVisibleChannel++;
			return true;
		}

		/********************************************************************/
		/// <summary>
		/// Page up (10 rows)
		/// </summary>
		/********************************************************************/
		private static bool PageUp(PatternRenderer renderer)
		{
			if (!renderer.AllowPatternScrolling)
			{
				return false;
			}

			renderer.ManualRow -= 10;

			if (renderer.RollingPatterns)
				// Rolling mode: can move across patterns
			{
				while (renderer.ManualRow < 0)
				{
					if (renderer.ManualSongPosition <= 0)
					{
						// First pattern - clamp at 0
						renderer.ManualRow = 0;
						break;
					}

					// Move to previous pattern
					renderer.ManualSongPosition--;
					int prevPatternRows = GetSafeRowCount(renderer, renderer.ManualSongPosition);
					renderer.ManualRow += prevPatternRows;
				}
			}
			else
			{
				// Normal mode: clamp at 0
				if (renderer.ManualRow < 0)
				{
					renderer.ManualRow = 0;
				}
			}

			return true;
		}

		/********************************************************************/
		/// <summary>
		/// Page down (10 rows)
		/// </summary>
		/********************************************************************/
		private static bool PageDown(PatternRenderer renderer)
		{
			if (!renderer.AllowPatternScrolling)
			{
				return false;
			}

			renderer.ManualRow += 10;

			if (renderer.RollingPatterns)
				// Rolling mode: can move across patterns
			{
				while (renderer.ManualSongPosition < renderer.SongData.Count)
				{
					int currentPatternRows = GetSafeRowCount(renderer, renderer.ManualSongPosition);

					if (renderer.ManualRow < currentPatternRows)
					{
						break; // Row is valid in current pattern
					}

					if (renderer.ManualSongPosition >= renderer.SongData.Count - 1)
					{
						// Last pattern - clamp at max
						renderer.ManualRow = currentPatternRows - 1;
						break;
					}

					// Move to next pattern
					renderer.ManualRow -= currentPatternRows;
					renderer.ManualSongPosition++;
				}
			}
			else
			{
				// Normal mode: clamp at max rows in current pattern
				int currentPatternRows = GetSafeRowCount(renderer, renderer.ManualSongPosition);
				if (renderer.ManualRow >= currentPatternRows)
				{
					renderer.ManualRow = currentPatternRows - 1;
				}
			}

			return true;
		}

		/********************************************************************/
		/// <summary>
		/// Jump to first row in current pattern (Home)
		/// </summary>
		/********************************************************************/
		private static bool JumpToFirstRow(PatternRenderer renderer)
		{
			if (!renderer.AllowPatternScrolling)
			{
				return false;
			}

			renderer.ManualRow = 0;
			return true;
		}

		/********************************************************************/
		/// <summary>
		/// Jump to last row in current pattern (End)
		/// </summary>
		/********************************************************************/
		private static bool JumpToLastRow(PatternRenderer renderer)
		{
			if (!renderer.AllowPatternScrolling)
			{
				return false;
			}

			// Get max rows for current position
			int maxRows = GetSafeRowCount(renderer, renderer.ManualSongPosition);

			renderer.ManualRow = maxRows - 1;
			return true;
		}
	}
}
