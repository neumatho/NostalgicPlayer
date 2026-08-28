//---------------------------------------------------------------------------------------
// <copyright file="BitmapFontRenderer.cs" company="NostalgicPlayer">
// Copyright (c) NostalgicPlayer. All rights reserved.
// </copyright>
//---------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.Tracker;

namespace Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.BitmapFont
{
	/// <summary>
	/// Renders bitmap font characters in a specific color with caching
	/// Scaled bitmaps are created once at initialization for performance
	/// </summary>
	internal class BitmapFontRenderer : IDisposable
	{
		private static FontScale currentFontScale = FontScale.Scale100;

		// Static font scale shared by all bitmap font renderers
		private readonly Dictionary<char, Bitmap> charCache;
		private readonly Dictionary<char, int> charMap;
		private readonly int charPadding;
		private readonly int fontScaleX;
		private readonly int fontScaleY;
		private readonly bool forceUppercase;
		private readonly int paddingBottom;
		private readonly int paddingLeft;
		private readonly int paddingRight;
		private readonly int paddingTop;
		private readonly Color renderColor;
		private readonly int[] scaledWidths;
		private readonly float scaleFactor;
		private readonly Bitmap sourceBitmap;
		private readonly int sourceCharHeight;
		private readonly int sourceCharWidth;
		private readonly int[] variableWidths;

		/********************************************************************/
		/// <summary>
		/// Create a renderer for a specific color with pre-scaled bitmaps
		/// </summary>
		/********************************************************************/
		internal BitmapFontRenderer(Bitmap sourceBitmap, Dictionary<char, int> charMap, int charWidth, int charHeight,
			int[] variableWidths, Color color, bool forceUppercase, int charPadding,
			int paddingTop, int paddingBottom, int paddingLeft, int paddingRight, int fontScaleX = 1, int fontScaleY = 1)
		{
			this.sourceBitmap = sourceBitmap;
			this.charMap = charMap;
			sourceCharWidth = charWidth;
			sourceCharHeight = charHeight;
			this.fontScaleX = fontScaleX;
			this.fontScaleY = fontScaleY;
			this.variableWidths = variableWidths;
			renderColor = color;
			this.forceUppercase = forceUppercase;
			this.charPadding = charPadding;
			this.paddingTop = paddingTop;
			this.paddingBottom = paddingBottom;
			this.paddingLeft = paddingLeft;
			this.paddingRight = paddingRight;
			charCache = new Dictionary<char, Bitmap>();

			// Store scale factor at creation time
			scaleFactor = ScaleFactor;

			// Pre-calculate scaled widths for variable width fonts (including horizontal padding)
			if (variableWidths != null)
			{
				scaledWidths = new int[variableWidths.Length];
				for (int i = 0; i < variableWidths.Length; i++)
				{
					scaledWidths[i] = (int)((variableWidths[i] + paddingLeft + paddingRight) * fontScaleX * scaleFactor);
				}
			}

			// Pre-create all scaled character bitmaps
			foreach (KeyValuePair<char, int> kvp in charMap)
			{
				char c = kvp.Key;
				int index = kvp.Value;
				charCache[c] = CreateScaledTintedChar(index);
			}
		}

		/********************************************************************/
		/// <summary>
		/// Get/set global font scale for all bitmap font renderers
		/// </summary>
		/********************************************************************/
		public static FontScale CurrentFontScale
		{
			get => currentFontScale;
			set
			{
				if (currentFontScale == value)
				{
					return;
				}

				currentFontScale = value;
				FontScaleChanged?.Invoke(value);
			}
		}

		/********************************************************************/
		/// <summary>
		/// Get scale factor as float
		/// </summary>
		/********************************************************************/
		public static float ScaleFactor => (int)CurrentFontScale / 100f;

		/********************************************************************/
		/// <summary>
		/// Get base character width (after font scaling, before global scaling, including padding)
		/// </summary>
		/********************************************************************/
		public int BaseCharWidth => (sourceCharWidth + paddingLeft + paddingRight) * fontScaleX;

		/********************************************************************/
		/// <summary>
		/// Get base character height (after font scaling, before global scaling, including padding)
		/// </summary>
		/********************************************************************/
		public int BaseCharHeight => (sourceCharHeight + paddingTop + paddingBottom) * fontScaleY;

		/********************************************************************/
		/// <summary>
		/// Get scaled character width (after both font and global scaling, including padding)
		/// </summary>
		/********************************************************************/
		public int CharWidth => (int)(BaseCharWidth * scaleFactor);

		/********************************************************************/
		/// <summary>
		/// Get scaled character height (after both font and global scaling, including padding)
		/// </summary>
		/********************************************************************/
		public int CharHeight => (int)(BaseCharHeight * scaleFactor);

		/********************************************************************/
		/// <summary>
		/// Dispose cached character bitmaps
		/// </summary>
		/********************************************************************/
		public void Dispose()
		{
			foreach (Bitmap bitmap in charCache.Values)
			{
				bitmap?.Dispose();
			}

			charCache.Clear();
		}

		/// <summary>
		/// Fires when CurrentFontScale changes. Tracker-specific bitmap renderers (with their own
		/// scale state) subscribe here so they no longer need to be touched from the host.
		/// </summary>
		public static event Action<FontScale> FontScaleChanged;

		/********************************************************************/
		/// <summary>
		/// Draw a string using pre-scaled bitmap font
		/// </summary>
		/********************************************************************/
		public void DrawString(Graphics g, string text, int x, int y)
		{
			// Convert to uppercase if required
			if (forceUppercase)
			{
				text = text.ToUpper();
			}

			int currentX = x;

			foreach (char c in text)
			{
				if (charMap.TryGetValue(c, out int index))
				{
					// Get scaled character width
					int scaledWidth = GetScaledCharWidth(index);

					// Get pre-scaled cached character bitmap
					if (charCache.TryGetValue(c, out Bitmap charBitmap))
					{
						g.DrawImageUnscaled(charBitmap, currentX, y);
					}

					currentX += scaledWidth;
				}
				else
					// Unknown character, advance by fixed width
				{
					currentX += CharWidth;
				}
			}
		}

		/********************************************************************/
		/// <summary>
		/// Measure text width in pixels (scaled)
		/// </summary>
		/********************************************************************/
		public int MeasureTextWidth(string text)
		{
			// Convert to uppercase if required
			if (forceUppercase)
			{
				text = text.ToUpper();
			}

			int totalWidth = 0;
			foreach (char c in text)
			{
				if (charMap.TryGetValue(c, out int index))
				{
					totalWidth += GetScaledCharWidth(index);
				}
				else
					// Unknown character, use fixed width
				{
					totalWidth += CharWidth;
				}
			}

			return totalWidth;
		}

		/********************************************************************/
		/// <summary>
		/// Get the base (unscaled) width of a character at the given index (including padding)
		/// </summary>
		/********************************************************************/
		private int GetBaseCharWidth(int index)
		{
			// If variable widths are defined, use them (with font scaling and padding)
			if (variableWidths != null && index < variableWidths.Length)
			{
				return (variableWidths[index] + paddingLeft + paddingRight) * fontScaleX;
			}

			// Otherwise use fixed width (already includes padding)
			return BaseCharWidth;
		}

		/********************************************************************/
		/// <summary>
		/// Get the scaled width of a character at the given index
		/// </summary>
		/********************************************************************/
		private int GetScaledCharWidth(int index)
		{
			// If scaled widths are pre-calculated, use them
			if (scaledWidths != null && index < scaledWidths.Length)
			{
				return scaledWidths[index];
			}

			// Otherwise use fixed scaled width
			return CharWidth;
		}

		/********************************************************************/
		/// <summary>
		/// Calculate source X position for a character index in the sprite sheet
		/// For variable-width fonts, sum up all previous character widths
		/// </summary>
		/********************************************************************/
		private int GetSourceX(int index)
		{
			if (variableWidths != null)
			{
				int srcX = 0;
				for (int i = 0; i < index; i++)
				{
					srcX += variableWidths[i];
				}

				return srcX;
			}

			return index * (sourceCharWidth + charPadding);
		}

		/********************************************************************/
		/// <summary>
		/// Get the source (unscaled) width of a character at the given index
		/// </summary>
		/********************************************************************/
		private int GetSourceCharWidth(int index)
		{
			if (variableWidths != null && index < variableWidths.Length)
			{
				return variableWidths[index];
			}

			return sourceCharWidth;
		}

		/********************************************************************/
		/// <summary>
		/// Create a scaled and tinted character bitmap
		/// </summary>
		/********************************************************************/
		private Bitmap CreateScaledTintedChar(int index)
		{
			int srcCharWidth = GetSourceCharWidth(index);

			// Calculate dimensions including padding
			int paddedWidth = srcCharWidth + paddingLeft + paddingRight;
			int paddedHeight = sourceCharHeight + paddingTop + paddingBottom;

			int fontScaledWidth = paddedWidth * fontScaleX;
			int fontScaledHeight = paddedHeight * fontScaleY;
			int finalWidth = (int)(fontScaledWidth * scaleFactor);
			int finalHeight = (int)(fontScaledHeight * scaleFactor);

			// Calculate source position in sprite sheet
			int srcX = GetSourceX(index);

			// Validate source rectangle is within bitmap bounds
			if (srcX < 0 || srcX + srcCharWidth > sourceBitmap.Width || sourceCharHeight > sourceBitmap.Height)
			{
				// Return empty transparent bitmap if out of bounds
				return new Bitmap(Math.Max(1, finalWidth), Math.Max(1, finalHeight), PixelFormat.Format32bppArgb);
			}

			// First create the font-scaled tinted bitmap (apply fontScaleX/Y, including padding)
			Bitmap fontScaledBitmap = new(fontScaledWidth, fontScaledHeight, PixelFormat.Format32bppArgb);

			Rectangle srcRect = new(srcX, 0, srcCharWidth, sourceCharHeight);

			// Lock source bitmap for fast pixel access
			BitmapData sourceData = sourceBitmap.LockBits(srcRect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
			BitmapData fontScaledData = fontScaledBitmap.LockBits(new Rectangle(0, 0, fontScaledWidth, fontScaledHeight),
				ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

			// Calculate padding offsets in font-scaled coordinates
			int paddingOffsetX = paddingLeft * fontScaleX;
			int paddingOffsetY = paddingTop * fontScaleY;

			try
			{
				unsafe
				{
					byte* srcPtr = (byte*)sourceData.Scan0;
					byte* dstPtr = (byte*)fontScaledData.Scan0;

					int srcStride = sourceData.Stride;
					int dstStride = fontScaledData.Stride;

					for (int srcY = 0; srcY < sourceCharHeight; srcY++)
					{
						byte* srcRow = srcPtr + (srcY * srcStride);

						for (int srcXPos = 0; srcXPos < srcCharWidth; srcXPos++)
						{
							// ARGB format: B, G, R, A - use alpha channel to detect character pixels
							byte a = srcRow[(srcXPos * 4) + 3];
							bool isCharPixel = a > 0;

							// Write scaled pixels
							byte outB = isCharPixel ? renderColor.B : (byte)0;
							byte outG = isCharPixel ? renderColor.G : (byte)0;
							byte outR = isCharPixel ? renderColor.R : (byte)0;
							byte outA = isCharPixel ? (byte)255 : (byte)0;

							// Repeat pixel fontScaleX times horizontally and fontScaleY times vertically
							// Offset by padding to center the glyph
							for (int sy = 0; sy < fontScaleY; sy++)
							{
								int dstY = paddingOffsetY + (srcY * fontScaleY) + sy;
								byte* dstRow = dstPtr + (dstY * dstStride);

								for (int sx = 0; sx < fontScaleX; sx++)
								{
									int dstX = paddingOffsetX + (srcXPos * fontScaleX) + sx;
									dstRow[(dstX * 4) + 0] = outB;
									dstRow[(dstX * 4) + 1] = outG;
									dstRow[(dstX * 4) + 2] = outR;
									dstRow[(dstX * 4) + 3] = outA;
								}
							}
						}
					}
				}
			}
			finally
			{
				sourceBitmap.UnlockBits(sourceData);
				fontScaledBitmap.UnlockBits(fontScaledData);
			}

			// If no global scaling needed, return font-scaled bitmap
			if (scaleFactor == 1f)
			{
				return fontScaledBitmap;
			}

			// Apply global scale
			// Use NearestNeighbor for integer scales (200%, 300%, 400%) to keep pixels sharp
			// Use HighQualityBicubic for non-integer scales (125%, 150%, 175%) for smoother results
			bool isIntegerScale = scaleFactor == 2f || scaleFactor == 3f || scaleFactor == 4f;

			Bitmap finalBitmap = new(finalWidth, finalHeight, PixelFormat.Format32bppArgb);
			using (Graphics g = Graphics.FromImage(finalBitmap))
			{
				g.InterpolationMode = isIntegerScale ? InterpolationMode.NearestNeighbor : InterpolationMode.HighQualityBicubic;
				g.PixelOffsetMode = PixelOffsetMode.Half;
				g.DrawImage(fontScaledBitmap, 0, 0, finalWidth, finalHeight);
			}

			fontScaledBitmap.Dispose();
			return finalBitmap;
		}
	}
}
