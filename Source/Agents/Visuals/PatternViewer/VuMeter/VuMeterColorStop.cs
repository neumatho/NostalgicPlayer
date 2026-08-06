//---------------------------------------------------------------------------------------
// <copyright file="VuMeterColorStop.cs" company="NostalgicPlayer">
// Copyright (c) NostalgicPlayer. All rights reserved.
// </copyright>
//---------------------------------------------------------------------------------------
using System.Drawing;

namespace Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.VuMeter
{
	/// <summary>
	/// Defines a color stop in a gradient for VU meters
	/// </summary>
	internal struct VuMeterColorStop
	{
		/// <summary>
		/// Position in gradient (0.0 = bottom/quiet, 1.0 = top/loud)
		/// </summary>
		public float Position;

		/// <summary>
		/// Color at this position
		/// </summary>
		public Color Color;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public VuMeterColorStop(float position, Color color)
		{
			Position = position;
			Color = color;
		}
	}
}
