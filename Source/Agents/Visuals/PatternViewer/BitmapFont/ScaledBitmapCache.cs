//---------------------------------------------------------------------------------------
// <copyright file="ScaledBitmapCache.cs" company="NostalgicPlayer">
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
	/// Central cache for scaled bitmaps.
	/// Register a bitmap once, get all 7 scales (100%, 125%, 150%, 175%, 200%, 300%, 400%).
	/// Uses same scaling logic as fonts:
	/// - NearestNeighbor for integer scales (200%, 300%, 400%) to keep pixels sharp
	/// - HighQualityBicubic for non-integer scales (125%, 150%, 175%) for smoother results
	/// </summary>
	internal static class ScaledBitmapCache
	{
		// Cache: resource path -> (scale -> bitmap)
		private static readonly Dictionary<string, Dictionary<FontScale, Bitmap>> cache = new();

		/********************************************************************/
		/// <summary>
		/// Register a bitmap and create all 7 scaled versions
		/// </summary>
		/// <param name="resourcePath">Unique identifier for this bitmap</param>
		/// <param name="baseBitmap">The 100% bitmap</param>
		/********************************************************************/
		public static void Register(string resourcePath, Bitmap baseBitmap)
		{
			if (cache.ContainsKey(resourcePath))
			{
				return;
			}

			Dictionary<FontScale, Bitmap> scales = new();

			foreach (FontScale scale in Enum.GetValues(typeof(FontScale)))
			{
				float scaleFactor = (int)scale / 100f;
				scales[scale] = CreateScaledBitmap(baseBitmap, scaleFactor);
			}

			cache[resourcePath] = scales;
		}

		/********************************************************************/
		/// <summary>
		/// Get a bitmap at the specified scale
		/// </summary>
		/// <param name="resourcePath">The resource path used during Register</param>
		/// <param name="scale">The desired scale</param>
		/// <returns>Scaled bitmap, or null if not registered</returns>
		/********************************************************************/
		public static Bitmap GetBitmap(string resourcePath, FontScale scale)
		{
			if (!cache.TryGetValue(resourcePath, out Dictionary<FontScale, Bitmap> scales))
			{
				return null;
			}

			if (!scales.TryGetValue(scale, out Bitmap bitmap))
			{
				return null;
			}

			return bitmap;
		}

		/********************************************************************/
		/// <summary>
		/// Get the 100% base bitmap
		/// </summary>
		/********************************************************************/
		public static Bitmap GetBaseBitmap(string resourcePath)
		{
			return GetBitmap(resourcePath, FontScale.Scale100);
		}

		/********************************************************************/
		/// <summary>
		/// Check if a bitmap is registered
		/// </summary>
		/********************************************************************/
		public static bool IsRegistered(string resourcePath)
		{
			return cache.ContainsKey(resourcePath);
		}

		/********************************************************************/
		/// <summary>
		/// Clear all cached bitmaps
		/// </summary>
		/********************************************************************/
		public static void ClearCache()
		{
			foreach (Dictionary<FontScale, Bitmap> scales in cache.Values)
			{
				foreach (Bitmap bitmap in scales.Values)
				{
					bitmap?.Dispose();
				}
			}

			cache.Clear();
		}

		/********************************************************************/
		/// <summary>
		/// Create a scaled bitmap
		/// </summary>
		/********************************************************************/
		private static Bitmap CreateScaledBitmap(Bitmap baseBitmap, float scaleFactor)
		{
			// If no scaling needed, return a copy
			if (Math.Abs(scaleFactor - 1f) < 0.001f)
			{
				return new Bitmap(baseBitmap);
			}

			int scaledWidth = (int)(baseBitmap.Width * scaleFactor);
			int scaledHeight = (int)(baseBitmap.Height * scaleFactor);

			// Use NearestNeighbor for integer scales (200%, 300%, 400%) to keep pixels sharp
			// Use HighQualityBicubic for non-integer scales (125%, 150%, 175%) for smoother results
			bool isIntegerScale = Math.Abs(scaleFactor - 2f) < 0.001f ||
			                      Math.Abs(scaleFactor - 3f) < 0.001f ||
			                      Math.Abs(scaleFactor - 4f) < 0.001f;

			Bitmap scaledBitmap = new(scaledWidth, scaledHeight, PixelFormat.Format32bppArgb);
			using (Graphics g = Graphics.FromImage(scaledBitmap))
			{
				g.InterpolationMode = isIntegerScale ? InterpolationMode.NearestNeighbor : InterpolationMode.HighQualityBicubic;
				g.PixelOffsetMode = PixelOffsetMode.Half;
				g.DrawImage(baseBitmap, 0, 0, scaledWidth, scaledHeight);
			}

			return scaledBitmap;
		}
	}
}
