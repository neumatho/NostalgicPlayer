//---------------------------------------------------------------------------------------
// <copyright file="VolumeBarParameters.cs" company="NostalgicPlayer">
// Copyright (c) NostalgicPlayer. All rights reserved.
// </copyright>
//---------------------------------------------------------------------------------------
using Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.Containers;
using Polycode.NostalgicPlayer.Kit.Containers;

namespace Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.VuMeter
{
	/// <summary>
	/// Rendering context for volume bars
	/// </summary>
	internal sealed class VolumeBarRenderingContext
	{
		/// <summary>
		/// Top Y position for rendering
		/// </summary>
		public required int TopY
		{
			get;
			init;
		}

		/// <summary>
		/// Width of row number column
		/// </summary>
		public required int RowNumberWidth
		{
			get;
			init;
		}

		/// <summary>
		/// Height of each row
		/// </summary>
		public required int RowHeight
		{
			get;
			init;
		}

		/// <summary>
		/// Width of each channel column
		/// </summary>
		public required int ChannelWidth
		{
			get;
			init;
		}

		/// <summary>
		/// Left margin offset
		/// </summary>
		public required int LeftMargin
		{
			get;
			init;
		}

		/// <summary>
		/// Bottom position where VU bars start (they grow upward from here)
		/// </summary>
		public required int VuBarBottom
		{
			get;
			init;
		}

		/// <summary>
		/// Brush cache for gradient VU meters
		/// </summary>
		public required VuMeterBrushCache BrushCache
		{
			get;
			init;
		}
	}

	/// <summary>
	/// Channel configuration
	/// </summary>
	internal sealed class VolumeBarChannelConfig
	{
		/// <summary>
		/// Index of first visible channel
		/// </summary>
		public int FirstVisibleChannel
		{
			get;
			set;
		}

		/// <summary>
		/// Total number of channels
		/// </summary>
		public int ChannelCount
		{
			get;
			set;
		}

		/// <summary>
		/// Channel information array
		/// </summary>
		public ChannelChanged[] ChannelInfo
		{
			get;
			set;
		}

		/// <summary>
		/// Generate visible channel configuration for rendering
		/// </summary>
		public VolumeBarVisibleChannelConfig Generate(int visibleChannels)
		{
			return new VolumeBarVisibleChannelConfig {VisibleChannels = visibleChannels, FirstVisibleChannel = FirstVisibleChannel, ChannelCount = ChannelCount, ChannelInfo = ChannelInfo};
		}
	}

	/// <summary>
	/// Visible channel configuration for volume bar rendering
	/// </summary>
	internal sealed class VolumeBarVisibleChannelConfig
	{
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
		/// Channel information array
		/// </summary>
		public required ChannelChanged[] ChannelInfo
		{
			get;
			init;
		}
	}

	/// <summary>
	/// Display settings for volume bars
	/// </summary>
	internal sealed class VolumeBarDisplaySettings
	{
		/// <summary>
		/// Volume bar display mode
		/// </summary>
		public required VolumeBarMode Mode
		{
			get;
			set;
		}

		/// <summary>
		/// VU meter visual style (reference to VuMeterRegistration.Id, Family.Variant)
		/// </summary>
		public required string VuMeterId
		{
			get;
			set;
		}
	}

	/// <summary>
	/// Audio measurement data for volume bars
	/// </summary>
	internal sealed class VolumeBarAudioData
	{
		/// <summary>
		/// Volume bar decay values for note kick mode
		/// </summary>
		public required int[] VolumeBarDecay
		{
			get;
			init;
		}

		/// <summary>
		/// Smoothed real volume values
		/// </summary>
		public required float[] RealVolumeSmoothed
		{
			get;
			init;
		}

		/// <summary>
		/// Measured real volume values
		/// </summary>
		public required float[] RealVolumeMeasured
		{
			get;
			init;
		}
	}

	/// <summary>
	/// Channel state tracking for volume bars
	/// </summary>
	internal sealed class VolumeBarChannelState
	{
		/// <summary>
		/// Counter for enabled state changes per channel
		/// </summary>
		public required int[] EnabledChangeCount
		{
			get;
			init;
		}

		/// <summary>
		/// Counter for muted state changes per channel
		/// </summary>
		public required int[] MutedChangeCount
		{
			get;
			init;
		}

		/// <summary>
		/// Counter for how many times channelInfo was null per channel
		/// </summary>
		public required int[] ChannelInfoNullCount
		{
			get;
			init;
		}

		/// <summary>
		/// Track if channelInfo was null in the last frame per channel
		/// </summary>
		public required bool[] WasChannelInfoNull
		{
			get;
			init;
		}
	}

	/// <summary>
	/// AHX-specific animation state
	/// </summary>
	internal sealed class VolumeBarAhxAnimation
	{
		/// <summary>
		/// Hue counter for AHX style animation
		/// </summary>
		public required float HueCounter
		{
			get;
			set;
		}

		/// <summary>
		/// Saturation counter for AHX style animation
		/// </summary>
		public required float SaturationCounter
		{
			get;
			set;
		}

		/// <summary>
		/// Saturation direction for AHX style animation
		/// </summary>
		public required float SaturationDirection
		{
			get;
			set;
		}
	}

	/// <summary>
	/// Debug rendering options for volume bars
	/// </summary>
	internal sealed class VolumeBarDebugOptions
	{
		/// <summary>
		/// Current display mode
		/// </summary>
		public required DisplayMode CurrentDisplayMode
		{
			get;
			init;
		}

		/// <summary>
		/// Whether to show debug information
		/// </summary>
		public required bool ShowDebugInfo
		{
			get;
			init;
		}

		/// <summary>
		/// Current pattern data (optional)
		/// </summary>
		public SongPatternViewData CurrentPattern
		{
			get;
			init;
		}

		/// <summary>
		/// Current row number
		/// </summary>
		public int CurrentRow
		{
			get;
			init;
		}
	}
}
