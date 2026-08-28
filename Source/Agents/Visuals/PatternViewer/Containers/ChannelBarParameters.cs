//---------------------------------------------------------------------------------------
// <copyright file="ChannelBarParameters.cs" company="NostalgicPlayer">
// Copyright (c) NostalgicPlayer. All rights reserved.
// </copyright>
//---------------------------------------------------------------------------------------
using System;
using System.Drawing;
using Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.Pattern;
using Polycode.NostalgicPlayer.Kit.Containers;

namespace Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.Containers
{
	/// <summary>
	/// Result of channel bar rendering
	/// </summary>
	internal readonly struct ChannelBarResult
	{
		/// <summary>
		/// Rectangle for the left scroll button
		/// </summary>
		public required Rectangle LeftScrollButtonRect
		{
			get;
			init;
		}

		/// <summary>
		/// Rectangle for the right scroll button
		/// </summary>
		public required Rectangle RightScrollButtonRect
		{
			get;
			init;
		}
	}

	/// <summary>
	/// Common input parameters for channel bar rendering
	/// </summary>
	internal readonly struct ChannelBarInput
	{
		/// <summary>
		/// Current pattern data being displayed
		/// </summary>
		public required SongPatternViewData DisplaySongPattern
		{
			get;
			init;
		}

		/// <summary>
		/// Number of visible channels
		/// </summary>
		public required int VisibleChannels
		{
			get;
			init;
		}

		/// <summary>
		/// Index of first visible channel
		/// </summary>
		public required int FirstVisibleChannel
		{
			get;
			init;
		}

		/// <summary>
		/// Total number of channels
		/// </summary>
		public required int ChannelCount
		{
			get;
			init;
		}

		/// <summary>
		/// Array indicating which mixer channels are enabled
		/// </summary>
		public required bool[] MixerChannelsEnabled
		{
			get;
			init;
		}

		/// <summary>
		/// Array to store channel bar rectangles for click detection
		/// </summary>
		public required Rectangle[] ChannelBarRects
		{
			get;
			init;
		}

		/// <summary>
		/// Function to get track name for a channel index
		/// </summary>
		public required Func<int, string> GetTrackName
		{
			get;
			init;
		}
	}

	/// <summary>
	/// Input parameters for 3D style channel bar (ProTracker, AHX, DigiBooster1, DigiBooster2)
	/// </summary>
	internal readonly struct ChannelBar3DStyleInput
	{
		/// <summary>
		/// Render metrics
		/// </summary>
		public required RenderMetrics Metrics
		{
			get;
			init;
		}

		/// <summary>
		/// Background brush for channel bar
		/// </summary>
		public required Brush HeaderBgBrush
		{
			get;
			init;
		}

		/// <summary>
		/// Pen for 3D light effect
		/// </summary>
		public required Pen Box3DLightPen
		{
			get;
			init;
		}

		/// <summary>
		/// Pen for 3D dark effect
		/// </summary>
		public required Pen Box3DDarkPen
		{
			get;
			init;
		}

		/// <summary>
		/// Pen for 3D background
		/// </summary>
		public required Pen Box3DBgPen
		{
			get;
			init;
		}

		/// <summary>
		/// Brush for embossed shadow effect (track names)
		/// </summary>
		public required Brush TrackNameEmbossedShadowBrush
		{
			get;
			init;
		}

		/// <summary>
		/// Brush for embossed light effect (track names)
		/// </summary>
		public required Brush TrackNameEmbossedLightBrush
		{
			get;
			init;
		}

		/// <summary>
		/// Brush for channel bar text
		/// </summary>
		public required Brush HeaderBrush
		{
			get;
			init;
		}

		/// <summary>
		/// Brush for muted channel bar text
		/// </summary>
		public required Brush HeaderMutedBrush
		{
			get;
			init;
		}

		/// <summary>
		/// Font for channel bar text (may be null when using bitmap fonts)
		/// </summary>
		public required Font HeaderFont
		{
			get;
			init;
		}

		/// <summary>
		/// Brush for disabled UI elements (scroll buttons when can't scroll)
		/// </summary>
		public required Brush DisabledBrush
		{
			get;
			init;
		}
	}
}
