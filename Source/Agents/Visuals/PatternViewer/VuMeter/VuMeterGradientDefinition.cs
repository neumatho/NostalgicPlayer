//---------------------------------------------------------------------------------------
// <copyright file="VuMeterGradientDefinition.cs" company="NostalgicPlayer">
// Copyright (c) NostalgicPlayer. All rights reserved.
// </copyright>
//---------------------------------------------------------------------------------------
namespace Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.VuMeter
{
	/// <summary>
	/// Gradient definition for VU meters (based on BZRPlayer implementation)
	/// </summary>
	internal class VuMeterGradientDefinition
	{
		/// <summary>
		/// Main color gradient (center of the bar)
		/// </summary>
		public VuMeterColorStop[] MainGradient
		{
			get;
			set;
		}

		/// <summary>
		/// Highlight gradient (left edge for 3D effect) - optional
		/// </summary>
		public VuMeterColorStop[] HighlightGradient
		{
			get;
			set;
		}

		/// <summary>
		/// Shadow gradient (right edge for 3D effect) - optional
		/// </summary>
		public VuMeterColorStop[] ShadowGradient
		{
			get;
			set;
		}

		/// <summary>
		/// Whether to render highlight and shadow gradients for 3D effect
		/// </summary>
		public bool UseHighlightShadow
		{
			get;
			set;
		}
	}
}
