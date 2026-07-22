//---------------------------------------------------------------------------------------
// <copyright file="BitmapFontManager.cs" company="NostalgicPlayer">
// Copyright (c) NostalgicPlayer. All rights reserved.
// </copyright>
//---------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;

namespace Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.BitmapFont
{
	/// <summary>
	/// Manages bitmap fonts loaded from PNG sprite sheets
	/// </summary>
	internal class BitmapFontManager : IDisposable
	{
		private readonly Dictionary<char, int> charMap;
		private readonly int charPadding;
		private readonly Bitmap fontBitmap;
		private readonly bool forceUppercase;
		private readonly int paddingBottom;
		private readonly int paddingLeft;
		private readonly int paddingRight;
		private readonly int paddingTop;
		private readonly List<BitmapFontRenderer> renderers;
		private readonly int sourceCharHeight;
		private readonly int sourceCharWidth;
		private readonly int[] variableWidths;

		/********************************************************************/
		/// <summary>
		/// Load a bitmap font from embedded resources using a FontDefinition
		/// </summary>
		/// <param name="definition">Font definition containing resource path and character mapping</param>
		/********************************************************************/
		public BitmapFontManager(FontDefinition definition)
		{
			Assembly assembly = Assembly.GetExecutingAssembly();
			string resourcePrefix = "Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.";

			// Load bitmap from embedded resource
			string fullResourcePath = resourcePrefix + definition.ResourcePath;
			using (Stream stream = assembly.GetManifestResourceStream(fullResourcePath))
			{
				if (stream == null)
				{
					string[] available = assembly.GetManifestResourceNames();
					throw new InvalidOperationException(
						$"Could not find embedded resource: {fullResourcePath}\n" +
						$"Available resources:\n{string.Join("\n", available)}");
				}

				fontBitmap = new Bitmap(stream);
			}

			sourceCharWidth = definition.CharWidth;
			sourceCharHeight = definition.CharHeight;
			paddingTop = definition.PaddingTop;
			paddingBottom = definition.PaddingBottom;
			paddingLeft = definition.PaddingLeft;
			paddingRight = definition.PaddingRight;
			variableWidths = definition.VariableWidths;
			forceUppercase = definition.ForceUppercase;
			charPadding = definition.CharPadding;

			// Build character map from string
			charMap = new Dictionary<char, int>();
			for (int i = 0; i < definition.CharacterMap.Length; i++)
			{
				charMap[definition.CharacterMap[i]] = i;
			}

			renderers = new List<BitmapFontRenderer>();
		}

		/********************************************************************/
		/// <summary>
		/// Get base character width (unscaled, including padding)
		/// </summary>
		/********************************************************************/
		public int BaseCharWidth => sourceCharWidth + paddingLeft + paddingRight;

		/********************************************************************/
		/// <summary>
		/// Get base character height (unscaled, including padding)
		/// </summary>
		/********************************************************************/
		public int BaseCharHeight => sourceCharHeight + paddingTop + paddingBottom;

		/********************************************************************/
		/// <summary>
		/// Get scaled character width
		/// </summary>
		/********************************************************************/
		public int CharWidth => (int)(BaseCharWidth * BitmapFontRenderer.ScaleFactor);

		/********************************************************************/
		/// <summary>
		/// Get scaled character height
		/// </summary>
		/********************************************************************/
		public int CharHeight => (int)(BaseCharHeight * BitmapFontRenderer.ScaleFactor);

		/********************************************************************/
		/// <summary>
		/// Dispose resources
		/// </summary>
		/********************************************************************/
		public void Dispose()
		{
			// Dispose all renderers
			foreach (BitmapFontRenderer renderer in renderers)
			{
				renderer?.Dispose();
			}

			renderers.Clear();

			fontBitmap?.Dispose();
		}

		/********************************************************************/
		/// <summary>
		/// Create a renderer for a specific color with optional font scaling
		/// </summary>
		/********************************************************************/
		public BitmapFontRenderer CreateRenderer(Color color, int fontScaleX = 1, int fontScaleY = 1)
		{
			BitmapFontRenderer renderer = new(fontBitmap, charMap, sourceCharWidth, sourceCharHeight, variableWidths, color,
				forceUppercase, charPadding, paddingTop, paddingBottom, paddingLeft, paddingRight, fontScaleX, fontScaleY);
			renderers.Add(renderer);
			return renderer;
		}
	}
}
