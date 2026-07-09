//---------------------------------------------------------------------------------------
// <copyright file="PatternDrawingHelper.cs" company="NostalgicPlayer">
// Copyright (c) NostalgicPlayer. All rights reserved.
// </copyright>
//---------------------------------------------------------------------------------------
using System.Drawing;
using System.IO;
using Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.Pattern;

namespace Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.Display
{
	/// <summary>
	/// Helper class for pattern drawing operations
	/// </summary>
	internal static class PatternDrawingHelper
	{
		// Cached brushes for info page
		private static readonly SolidBrush TitleBrush = new(Color.White);
		private static readonly SolidBrush LabelBrush = new(Color.LightGray);
		private static readonly SolidBrush ValueBrush = new(Color.LightBlue);
		private static readonly SolidBrush ErrorBrush = new(Color.Orange);

		/********************************************************************/
		/// <summary>
		/// Draw info page when module is loaded but no patterns available
		/// </summary>
		/********************************************************************/
		public static void DrawInfoPage(Graphics g, RectangleF bounds, PatternRenderer renderer)
		{
			using (Font titleFont = new("Consolas", 12, FontStyle.Bold))
			using (Font labelFont = new("Consolas", 10, FontStyle.Bold))
			using (Font valueFont = new("Consolas", 10))
			using (Font errorFont = new("Consolas", 9))
			{
				float x = 50;
				float y = 50;

				// Title
				string title = string.IsNullOrEmpty(renderer.GetErrorMessage())
					? "Pattern Information Not Available"
					: "Pattern Load Error";
				g.DrawString(title, titleFont, TitleBrush, x, y);
				y += 40;

				// Player Name
				g.DrawString("Player:", labelFont, LabelBrush, x, y);
				g.DrawString(renderer.PlayerName, valueFont, ValueBrush, x + 150, y);
				y += 25;

				// Song Name (from title if available)
				if (!string.IsNullOrEmpty(renderer.SongTitle))
				{
					g.DrawString("Song:", labelFont, LabelBrush, x, y);
					g.DrawString(renderer.SongTitle, valueFont, ValueBrush, x + 150, y);
					y += 25;
				}

				// File Name (just the name, not full path)
				string fileName = Path.GetFileName(renderer.FileName);
				g.DrawString("File:", labelFont, LabelBrush, x, y);
				g.DrawString(fileName, valueFont, ValueBrush, x + 150, y);
				y += 25;

				// Format
				if (!string.IsNullOrEmpty(renderer.SongFormat))
				{
					g.DrawString("Format:", labelFont, LabelBrush, x, y);
					g.DrawString(renderer.SongFormat, valueFont, ValueBrush, x + 150, y);
					y += 25;
				}

				// If there's an error message, display it at the bottom
				string errorMessage = renderer.GetErrorMessage();
				if (!string.IsNullOrEmpty(errorMessage))
				{
					y += 20;
					g.DrawString("Error:", labelFont, LabelBrush, x, y);
					y += 25;

					// Draw error message with word wrapping
					RectangleF errorRect = new(x, y, bounds.Width - (x * 2), bounds.Height - y - 50);
					g.DrawString(errorMessage, errorFont, ErrorBrush, errorRect);
				}
			}
		}

		/********************************************************************/
		/// <summary>
		/// Draw waiting message when no module is loaded
		/// </summary>
		/********************************************************************/
		public static void DrawWaitingMessage(Graphics g, RectangleF bounds)
		{
			string message = "SongPattern Viewer - Waiting for MOD file...";

			using (Font valueFont = new("Consolas", 10))
			{
				SizeF size = g.MeasureString(message, valueFont);
				float x = (bounds.Width - size.Width) / 2;
				float y = (bounds.Height - size.Height) / 2;

				g.DrawString(message, valueFont, LabelBrush, x, y);
			}
		}

		/********************************************************************/
		/// <summary>
		/// Paint the pattern panel - main entry point for painting
		/// </summary>
		/// <returns>True if pattern was re-rendered (not from cache)</returns>
		/********************************************************************/
		public static bool PaintPatternPanel(Graphics g, RectangleF clipBounds, PatternRenderer renderer)
		{
			// Show a grid-like pattern display if we have pattern data
			if (renderer.SongData != null && string.IsNullOrEmpty(renderer.GetErrorMessage()))
			{
				return renderer.DrawWithCache(g, new Size((int)clipBounds.Width, (int)clipBounds.Height));
			}

			if (!string.IsNullOrEmpty(renderer.FileName))
				// Module loaded but no pattern support or error occurred - show info page
			{
				DrawInfoPage(g, clipBounds, renderer);
			}
			else
				// No module loaded - show waiting message
			{
				DrawWaitingMessage(g, clipBounds);
			}

			return false;
		}
	}
}
