//---------------------------------------------------------------------------------------
// <copyright file="RenderViewerResult.cs" company="NostalgicPlayer">
// Copyright (c) NostalgicPlayer. All rights reserved.
// </copyright>
//---------------------------------------------------------------------------------------
namespace Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.Containers
{
	/// <summary>
	/// Result of RenderViewer - contains results from all sub-renderers
	/// </summary>
	internal readonly struct RenderViewerResult
	{
		/// <summary>
		/// Result from pattern rows rendering
		/// </summary>
		public PatternRowsResult? PatternRows
		{
			get;
			init;
		}
	}

	/// <summary>
	/// Result of DrawPatternRows
	/// </summary>
	internal readonly struct PatternRowsResult
	{
		/// <summary>
		/// X positions of each visible channel
		/// </summary>
		public int[] ChannelXPositions
		{
			get;
			init;
		}

		/// <summary>
		/// Width of each channel
		/// </summary>
		public int ChannelWidth
		{
			get;
			init;
		}

		/// <summary>
		/// Y position where VU bars start (top of pattern area)
		/// </summary>
		public int VuBarTop
		{
			get;
			init;
		}

		/// <summary>
		/// Y position of the current row (VU bars grow upward from here)
		/// </summary>
		public int VuBarBottom
		{
			get;
			init;
		}
	}
}
