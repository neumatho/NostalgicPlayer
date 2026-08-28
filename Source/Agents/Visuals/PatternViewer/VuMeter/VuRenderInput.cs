//---------------------------------------------------------------------------------------
// <copyright file="VuRenderInput.cs" company="NostalgicPlayer">
// Copyright (c) NostalgicPlayer. All rights reserved.
// </copyright>
//---------------------------------------------------------------------------------------
namespace Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.VuMeter
{
	/// <summary>
	/// Input for VU meter rendering
	/// </summary>
	internal readonly struct VuRenderInput
	{
		/// <summary>
		/// X positions of each visible channel (left edge)
		/// </summary>
		public required int[] ChannelXPositions
		{
			get;
			init;
		}

		/// <summary>
		/// Width of each channel
		/// </summary>
		public required int ChannelWidth
		{
			get;
			init;
		}

		/// <summary>
		/// Y position where VU bars can grow to (top)
		/// </summary>
		public required int VuBarTop
		{
			get;
			init;
		}

		/// <summary>
		/// Y position where VU bars start (current row)
		/// </summary>
		public required int VuBarBottom
		{
			get;
			init;
		}
	}
}
