//---------------------------------------------------------------------------------------
// <copyright file="VolumeBarState.cs" company="NostalgicPlayer">
// Copyright (c) NostalgicPlayer. All rights reserved.
// </copyright>
//---------------------------------------------------------------------------------------
using System;
using Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.Tracker.ModernTracker;

namespace Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.VuMeter
{
	/// <summary>
	/// Complete state container for volume bar rendering
	/// </summary>
	internal sealed class VolumeBarState : IDisposable
	{
		private const int MaxChannels = 64;

		/// <summary>
		/// Constructor - initializes all sub-objects
		/// </summary>
		public VolumeBarState()
		{
			AudioData = new VolumeBarAudioData {VolumeBarDecay = new int[MaxChannels], RealVolumeSmoothed = new float[MaxChannels], RealVolumeMeasured = new float[MaxChannels]};

			ChannelState = new VolumeBarChannelState {EnabledChangeCount = new int[MaxChannels], MutedChangeCount = new int[MaxChannels], ChannelInfoNullCount = new int[MaxChannels], WasChannelInfoNull = new bool[MaxChannels]};

			AhxAnimation = new VolumeBarAhxAnimation {HueCounter = 0, SaturationCounter = 128, SaturationDirection = 1};

			ChannelConfig = new VolumeBarChannelConfig {FirstVisibleChannel = 0, ChannelCount = 0, ChannelInfo = null};

			DisplaySettings = new VolumeBarDisplaySettings {Mode = VolumeBarMode.Off, VuMeterId = ModernTrackerPainter.VuMeterIdLight};

			BrushCache = new VuMeterBrushCache();
		}

		/// <summary>
		/// Audio measurement data
		/// </summary>
		public VolumeBarAudioData AudioData
		{
			get;
		}

		/// <summary>
		/// Channel state tracking
		/// </summary>
		public VolumeBarChannelState ChannelState
		{
			get;
		}

		/// <summary>
		/// AHX animation state
		/// </summary>
		public VolumeBarAhxAnimation AhxAnimation
		{
			get;
		}

		/// <summary>
		/// Channel configuration
		/// </summary>
		public VolumeBarChannelConfig ChannelConfig
		{
			get;
		}

		/// <summary>
		/// Display settings
		/// </summary>
		public VolumeBarDisplaySettings DisplaySettings
		{
			get;
		}

		/// <summary>
		/// Brush cache for gradient VU meters
		/// </summary>
		public VuMeterBrushCache BrushCache
		{
			get;
		}

		/********************************************************************/
		/// <summary>
		/// Dispose resources
		/// </summary>
		/********************************************************************/
		public void Dispose()
		{
			BrushCache?.Dispose();
		}

		/********************************************************************/
		/// <summary>
		/// Reset all state counters (called when song changes)
		/// </summary>
		/********************************************************************/
		public void ResetCounters()
		{
			for (int i = 0; i < MaxChannels; i++)
			{
				ChannelState.EnabledChangeCount[i] = 0;
				ChannelState.MutedChangeCount[i] = 0;
				ChannelState.ChannelInfoNullCount[i] = 0;
				ChannelState.WasChannelInfoNull[i] = false;
			}
		}
	}
}
