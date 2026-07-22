//---------------------------------------------------------------------------------------
// <copyright file="ControlBarConfig.cs" company="NostalgicPlayer">
// Copyright (c) NostalgicPlayer. All rights reserved.
// </copyright>
//---------------------------------------------------------------------------------------
namespace Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.ControlBar
{
	/// <summary>
	/// Configuration for which elements to show in the control bar
	/// </summary>
	internal class ControlBarConfig
	{
		// Navigation buttons
		public bool ShowPrevModule
		{
			get;
			set;
		}

		public bool ShowPrevSubSong
		{
			get;
			set;
		} = true;

		public bool ShowPrevSnapshot
		{
			get;
			set;
		} = true;

		public bool ShowRestart
		{
			get;
			set;
		} = true;

		public bool ShowPlayPause
		{
			get;
			set;
		} = true;

		public bool ShowStop
		{
			get;
			set;
		} = false;

		public bool ShowNextSnapshot
		{
			get;
			set;
		} = true;

		public bool ShowNextSubSong
		{
			get;
			set;
		} = true;

		public bool ShowNextModule
		{
			get;
			set;
		}

		// Other elements
		public bool ShowSlider
		{
			get;
			set;
		} = true;

		public bool ShowTimeDisplay
		{
			get;
			set;
		} = true;

		public bool ShowPositionDisplay
		{
			get;
			set;
		}

		/// <summary>
		/// Default configuration with all elements visible
		/// </summary>
		public static ControlBarConfig Default => new();

		/// <summary>
		/// Minimal configuration with only essential playback controls
		/// </summary>
		public static ControlBarConfig Minimal => new()
		{
			ShowPrevModule = false,
			ShowPrevSubSong = false,
			ShowRestart = false,
			ShowNextSubSong = false,
			ShowNextModule = false,
			ShowTimeDisplay = false,
			ShowPositionDisplay = false
		};

		/// <summary>
		/// Playback only - just play/pause/stop and snapshot navigation
		/// </summary>
		public static ControlBarConfig PlaybackOnly => new()
		{
			ShowPrevModule = false,
			ShowPrevSubSong = false,
			ShowRestart = false,
			ShowNextSubSong = false,
			ShowNextModule = false,
			ShowSlider = false,
			ShowTimeDisplay = false,
			ShowPositionDisplay = false
		};
	}
}
