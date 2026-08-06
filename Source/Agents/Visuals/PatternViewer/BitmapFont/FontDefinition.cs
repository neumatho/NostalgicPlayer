//---------------------------------------------------------------------------------------
// <copyright file="FontDefinition.cs" company="NostalgicPlayer">
// Copyright (c) NostalgicPlayer. All rights reserved.
// </copyright>
//---------------------------------------------------------------------------------------
namespace Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.BitmapFont
{
	/// <summary>
	/// Defines a bitmap font (PNG resource and character properties).
	/// This is the base definition without scaling - use FontConfig to combine with scaling.
	/// </summary>
	internal class FontDefinition
	{
		/// <summary>
		/// Embedded resource path for the PNG file (e.g., "Tracker.ProTracker.ProTracker31.Resources.PTFont.png")
		/// </summary>
		public string ResourcePath
		{
			get;
			init;
		}

		/// <summary>
		/// Width of each character in pixels
		/// </summary>
		public int CharWidth
		{
			get;
			init;
		}

		/// <summary>
		/// Height of each character in pixels
		/// </summary>
		public int CharHeight
		{
			get;
			init;
		}

		/// <summary>
		/// String containing all available characters in order.
		/// Position in string = index in bitmap sprite sheet (horizontal layout).
		/// </summary>
		public string CharacterMap
		{
			get;
			init;
		}

		/// <summary>
		/// If true, all text will be converted to uppercase before rendering
		/// </summary>
		public bool ForceUppercase
		{
			get;
			init;
		}

		/// <summary>
		/// For variable-width fonts (CharWidth = 0), array of widths for each character.
		/// Length must match CharacterMap length. null for fixed-width fonts.
		/// </summary>
		public int[] VariableWidths
		{
			get;
			init;
		}

		/// <summary>
		/// Extra padding after each character in the font file.
		/// Position in font = index * (CharWidth + CharPadding), but only CharWidth pixels are rendered.
		/// </summary>
		public int CharPadding
		{
			get;
			init;
		}

		/// <summary>
		/// Padding added above each character during rendering.
		/// The padding is applied in source pixels and scales with font scaling.
		/// </summary>
		public int PaddingTop
		{
			get;
			init;
		}

		/// <summary>
		/// Padding added below each character during rendering.
		/// The padding is applied in source pixels and scales with font scaling.
		/// </summary>
		public int PaddingBottom
		{
			get;
			init;
		}

		/// <summary>
		/// Padding added left of each character during rendering.
		/// The padding is applied in source pixels and scales with font scaling.
		/// </summary>
		public int PaddingLeft
		{
			get;
			init;
		}

		/// <summary>
		/// Padding added right of each character during rendering.
		/// The padding is applied in source pixels and scales with font scaling.
		/// </summary>
		public int PaddingRight
		{
			get;
			init;
		}
	}
}
