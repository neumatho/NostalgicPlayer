//---------------------------------------------------------------------------------------
// <copyright file="VuMeterBrushCache.cs" company="NostalgicPlayer">
// Copyright (c) NostalgicPlayer. All rights reserved.
// </copyright>
//---------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.VuMeter
{
	/// <summary>
	/// Cache for VU meter gradient brushes. Brushes are created on demand and persist
	/// until the style changes.
	/// </summary>
	internal class VuMeterBrushCache : IDisposable
	{
		private readonly Dictionary<int, SolidBrush> brushCache = new();

		public void Dispose()
		{
			InvalidateCache();
		}

		public SolidBrush GetBrush(Color color)
		{
			int key = color.ToArgb();

			if (!brushCache.TryGetValue(key, out SolidBrush brush))
			{
				brush = new SolidBrush(color);
				brushCache[key] = brush;
			}

			return brush;
		}

		public void InvalidateCache()
		{
			foreach (SolidBrush brush in brushCache.Values)
			{
				brush?.Dispose();
			}

			brushCache.Clear();
		}
	}
}
