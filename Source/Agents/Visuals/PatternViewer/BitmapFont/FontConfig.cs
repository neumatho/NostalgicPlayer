//---------------------------------------------------------------------------------------
// <copyright file="FontConfig.cs" company="NostalgicPlayer">
// Copyright (c) NostalgicPlayer. All rights reserved.
// </copyright>
//---------------------------------------------------------------------------------------
namespace Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.BitmapFont
{
	/// <summary>
	/// Combines a FontDefinition with scaling factors.
	/// Use this to specify which font to use and how to scale it.
	/// </summary>
	/// <param name="Definition">The font definition (PNG, character map, etc.)</param>
	/// <param name="ScaleX">Horizontal scale factor (1 = normal, 2 = double width)</param>
	/// <param name="ScaleY">Vertical scale factor (1 = normal, 2 = double height)</param>
	internal record FontConfig(FontDefinition Definition, int ScaleX = 1, int ScaleY = 1);
}
